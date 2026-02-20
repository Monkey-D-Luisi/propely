-- Create separate databases for each service
CREATE DATABASE propely_aiapi;
CREATE DATABASE propely_orgsapi;

-- Enable extensions in ai-api database
\c propely_aiapi
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable extensions in orgs-api database
\c propely_orgsapi
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
