output "service_account_emails" {
  value       = { for k, sa in google_service_account.services : k => sa.email }
  description = "Map of service name to service account email"
}
