import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";
import { getAuth, clearTokenCache } from "./auth.js";

const DEFAULT_MODEL = process.env.DEFAULT_MODEL || "gpt-5.4";
const FALLBACK_MODEL = process.env.FALLBACK_MODEL || "gpt-5.2";
const CHATGPT_RESPONSES_URL = "https://chatgpt.com/backend-api/codex/responses";
const DEFAULT_INSTRUCTIONS = "You are a helpful AI assistant participating in product and architecture discussions. Be thorough but concise.";

function log(msg: string): void {
  process.stderr.write(`[mcp-openai-oauth] ${msg}\n`);
}

/**
 * Parse SSE stream from ChatGPT backend and extract the final response text.
 */
async function parseSSEResponse(response: Response): Promise<string> {
  const text = await response.text();
  const textParts: string[] = [];

  for (const line of text.split("\n")) {
    if (!line.startsWith("data: ")) continue;
    const data = line.slice(6);
    if (data === "[DONE]") break;

    try {
      const event = JSON.parse(data);
      if (event.type === "response.output_text.delta" && event.delta) {
        textParts.push(event.delta);
      }
    } catch {
      // skip unparseable lines
    }
  }

  return textParts.join("") || "(empty response)";
}

async function callChatGPT(
  prompt: string,
  instructions: string,
  model: string
): Promise<Response> {
  const auth = await getAuth();

  return fetch(CHATGPT_RESPONSES_URL, {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${auth.accessToken}`,
      "ChatGPT-Account-ID": auth.accountId,
      "Content-Type": "application/json",
      "Accept": "text/event-stream",
    },
    body: JSON.stringify({
      model,
      instructions,
      input: [{ role: "user", content: prompt }],
      store: false,
      stream: true,
    }),
  });
}

async function callGpt(
  prompt: string,
  system?: string,
  model?: string
): Promise<string> {
  const targetModel = model || DEFAULT_MODEL;
  const instructions = system || DEFAULT_INSTRUCTIONS;

  try {
    log(`Calling ${targetModel}...`);
    const resp = await callChatGPT(prompt, instructions, targetModel);

    if (resp.status === 401) {
      log("Auth error, refreshing token and retrying...");
      clearTokenCache();
      const retryResp = await callChatGPT(prompt, instructions, targetModel);
      if (!retryResp.ok) {
        const body = await retryResp.text();
        throw new Error(`${retryResp.status}: ${body}`);
      }
      return parseSSEResponse(retryResp);
    }

    if (!resp.ok) {
      const body = await resp.text();
      const errorMsg = `${resp.status}: ${body}`;

      // If model not found, try fallback
      if (
        targetModel !== FALLBACK_MODEL &&
        (resp.status === 404 || body.includes("does not exist"))
      ) {
        log(`Model ${targetModel} unavailable, falling back to ${FALLBACK_MODEL}`);
        const fallbackResp = await callChatGPT(prompt, instructions, FALLBACK_MODEL);
        if (!fallbackResp.ok) {
          const fbBody = await fallbackResp.text();
          throw new Error(`Fallback also failed: ${fallbackResp.status}: ${fbBody}`);
        }
        const result = await parseSSEResponse(fallbackResp);
        return `[Used ${FALLBACK_MODEL} — ${targetModel} unavailable]\n\n${result}`;
      }

      throw new Error(errorMsg);
    }

    return parseSSEResponse(resp);
  } catch (err) {
    log(`Error: ${err}`);
    throw err;
  }
}

const server = new McpServer({
  name: "openai-oauth",
  version: "1.0.0",
});

server.tool(
  "ask_gpt",
  "Send a question to GPT and get a response. Use for product discussions, architectural debates, second opinions on design decisions, or any other consultation with a different AI perspective.",
  {
    prompt: z.string().describe("The message or question to send to GPT"),
    system: z
      .string()
      .optional()
      .describe("Optional system/instructions prompt to set context for GPT"),
    model: z
      .string()
      .optional()
      .describe(`Model override (default: ${DEFAULT_MODEL}, fallback: ${FALLBACK_MODEL})`),
  },
  async ({ prompt, system, model }) => {
    try {
      const result = await callGpt(prompt, system, model);
      return {
        content: [{ type: "text", text: result }],
      };
    } catch (err: unknown) {
      const error = err as Error;
      return {
        content: [
          {
            type: "text",
            text: `Error communicating with GPT: ${error.message}`,
          },
        ],
        isError: true,
      };
    }
  }
);

async function main(): Promise<void> {
  log(`Starting MCP server (model: ${DEFAULT_MODEL}, fallback: ${FALLBACK_MODEL})`);
  const transport = new StdioServerTransport();
  await server.connect(transport);
  log("MCP server connected via stdio");
}

main().catch((err) => {
  log(`Fatal error: ${err}`);
  process.exit(1);
});
