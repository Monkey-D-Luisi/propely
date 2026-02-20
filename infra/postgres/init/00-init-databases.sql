-- Create separate databases for each service
CREATE DATABASE propely_aiapi;
CREATE DATABASE propely_orgsapi;
CREATE DATABASE propely_propertiesapi;
CREATE DATABASE propely_publishingapi;
CREATE DATABASE propely_contactsapi;
CREATE DATABASE propely_appointmentsapi;

-- Enable extensions in ai-api database
\c propely_aiapi
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable extensions in orgs-api database
\c propely_orgsapi
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable extensions in properties-api database
\c propely_propertiesapi
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable extensions in publishing-api database
\c propely_publishingapi
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable extensions in contacts-api database
\c propely_contactsapi
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable extensions in appointments-api database
\c propely_appointmentsapi
CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
