# VPC module — private network for Cloud Run, Cloud SQL, and Redis

resource "google_compute_network" "main" {
  name                    = "${var.environment}-saastemplate-vpc"
  project                 = var.project_id
  auto_create_subnetworks = false
  routing_mode            = "REGIONAL"
}

resource "google_compute_subnetwork" "main" {
  name          = "${var.environment}-saastemplate-subnet"
  project       = var.project_id
  region        = var.region
  network       = google_compute_network.main.id
  ip_cidr_range = var.subnet_cidr

  private_ip_google_access = true
}

# Reserved IP range for Google-managed services (Cloud SQL, Memorystore)
resource "google_compute_global_address" "private_services" {
  name          = "${var.environment}-private-services-ip"
  project       = var.project_id
  purpose       = "VPC_PEERING"
  address_type  = "INTERNAL"
  prefix_length = 16
  network       = google_compute_network.main.id
}

# Private services access — enables Cloud SQL private IP
resource "google_service_networking_connection" "private_services" {
  network                 = google_compute_network.main.id
  service                 = "servicenetworking.googleapis.com"
  reserved_peering_ranges = [google_compute_global_address.private_services.name]

  deletion_policy = "ABANDON"
}

# Serverless VPC Access Connector — allows Cloud Run to reach VPC resources
resource "google_vpc_access_connector" "main" {
  name          = "${var.environment}-saastemplate-conn"
  project       = var.project_id
  region        = var.region
  ip_cidr_range = var.connector_cidr
  network       = google_compute_network.main.name

  min_instances = var.connector_min_instances
  max_instances = var.connector_max_instances
}
