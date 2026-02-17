-- Create separate databases for each service
CREATE DATABASE saastemplate_aiapi;
CREATE DATABASE saastemplate_orgsapi;

-- Enable extensions in ai-api database
\c saastemplate_aiapi
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable extensions in orgs-api database
\c saastemplate_orgsapi
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
