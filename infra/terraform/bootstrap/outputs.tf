output "state_bucket_name" {
  value       = google_storage_bucket.terraform_state.name
  description = "GCS bucket name for Terraform remote state"
}

output "terraform_sa_email" {
  value       = google_service_account.terraform.email
  description = "Email of the Terraform automation service account"
}

output "workload_identity_provider" {
  value       = google_iam_workload_identity_pool_provider.github.name
  description = "Full resource name of the WIF provider (use as GCP_WORKLOAD_IDENTITY_PROVIDER in GitHub)"
}

output "deploy_sa_email" {
  value       = google_service_account.github_actions_deploy.email
  description = "Email of the GitHub Actions deploy service account (use as GCP_DEPLOY_SERVICE_ACCOUNT in GitHub)"
}
