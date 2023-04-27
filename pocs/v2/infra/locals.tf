resource "random_integer" "rand" {
  min = 10000
  max = 99999
}

locals {
  common_tags = {
    organization = var.organization
    project      = "${var.project}-${var.organization}"
  }

  s3_bucket_name = "${var.project}-${var.organization}-${random_integer.rand.result}"
}