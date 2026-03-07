// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Commands.ContactActions;
using Propely.AiApi.Application.Actions.Helpers;
using Propely.AiApi.Domain.Actions;
using Propely.ContactsApi.Client;
using Propely.ContactsApi.Client.Models;

namespace Propely.AiApi.Application.Actions.Handlers.ContactActions;

/// <summary>
/// Handles the CreateContactActionCommand by extracting contact parameters
/// and creating a contact via the Contacts API SDK.
/// </summary>
public sealed class CreateContactActionHandler : IRequestHandler<CreateContactActionCommand, ActionResult>
{
    private readonly IContactsApiClient _contactsClient;
    private readonly ILogger<CreateContactActionHandler> _logger;

    public CreateContactActionHandler(
        IContactsApiClient contactsClient,
        ILogger<CreateContactActionHandler> logger)
    {
        _contactsClient = contactsClient;
        _logger = logger;
    }

    public async Task<ActionResult> Handle(CreateContactActionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CreateContact action for tenant {TenantId} by agent {AgentId}",
            request.TenantId, request.AgentId);

        var parameters = request.Parameters;

        var firstName = ParameterExtractor.GetString(parameters, "first_name");
        var lastName = ParameterExtractor.GetString(parameters, "last_name");
        var email = ParameterExtractor.GetString(parameters, "email");
        var phone = ParameterExtractor.GetString(parameters, "phone");
        var role = ParameterExtractor.GetString(parameters, "role");
        var company = ParameterExtractor.GetString(parameters, "company");
        var notes = ParameterExtractor.GetString(parameters, "notes");
        var source = ParameterExtractor.GetString(parameters, "source");

        if (string.IsNullOrWhiteSpace(firstName))
        {
            return ActionResult.Fail(
                ["Contact first name is required."],
                ActionType.CreateContact,
                "I need the contact's first name. Could you provide it?");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            return ActionResult.Fail(
                ["Contact last name is required."],
                ActionType.CreateContact,
                "I need the contact's last name. Could you provide it?");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return ActionResult.Fail(
                ["Contact email is required."],
                ActionType.CreateContact,
                "I need the contact's email address.");
        }

        var roles = new List<string>();
        if (!string.IsNullOrWhiteSpace(role))
            roles.Add(NormalizeRole(role));
        if (roles.Count == 0)
            roles.Add("Buyer"); // default role

        ContactResponse result;
        try
        {
            result = await _contactsClient.CreateContactAsync(new CreateContactClientRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                Roles = roles,
                Company = company,
                Notes = notes,
                Source = source ?? "NaturalLanguage",
                AssignedAgentId = request.AgentId
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create contact via Contacts API");

            return ActionResult.Fail(
                ["Failed to create contact. The Contacts service may be unavailable."],
                ActionType.CreateContact,
                "I encountered an error while creating the contact. Please try again later.");
        }

        var confirmationMessage = $"Created contact {firstName} {lastName} ({email}) as {string.Join(", ", roles)}.";

        _logger.LogInformation(
            "CreateContact action completed: contact {ContactId} for tenant {TenantId}",
            result.Id, request.TenantId);

        return ActionResult.Ok(
            data: new { result.Id, result.FirstName, result.LastName, result.Email, result.Roles },
            message: confirmationMessage,
            type: ActionType.CreateContact);
    }

    private static string NormalizeRole(string role)
    {
        return role.Trim().ToLowerInvariant() switch
        {
            "buyer" or "comprador" or "compradora" => "Buyer",
            "seller" or "vendedor" or "vendedora" => "Seller",
            "tenant" or "inquilino" or "inquilina" => "Tenant",
            "landlord" or "propietario" or "propietaria" or "casero" or "casera" => "Landlord",
            "professional" or "profesional" => "Professional",
            _ => char.ToUpperInvariant(role[0]) + role[1..].ToLowerInvariant()
        };
    }
}
