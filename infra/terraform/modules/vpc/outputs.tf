output "network_self_link" {
  value       = google_compute_network.main.self_link
  description = "Self link of the VPC network"
}

output "network_id" {
  value       = google_compute_network.main.id
  description = "ID of the VPC network"
}

output "subnet_self_link" {
  value       = google_compute_subnetwork.main.self_link
  description = "Self link of the primary subnet"
}

output "vpc_connector_id" {
  value       = google_vpc_access_connector.main.id
  description = "ID of the Serverless VPC Access Connector"
}

output "private_services_connection_id" {
  value       = google_service_networking_connection.private_services.id
  description = "ID of the private services connection (dependency for Cloud SQL)"
}
