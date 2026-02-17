# Backend configuration — partial config, supply bucket at init time:
#   terraform init -backend-config="bucket=YOUR_STATE_BUCKET"
terraform {
  backend "gcs" {
    prefix = "production"
  }
}
