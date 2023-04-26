locals {
  common_tags = {
    organization = var.organization
    project      = "${var.project}-${var.organization}"
  }
}