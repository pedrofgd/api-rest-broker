resource "aws_s3_bucket" "bucket" {
  bucket        = local.s3_bucket_name
  force_destroy = true
}

resource "aws_s3_object" "broker" {
  for_each = fileset("${path.cwd}/../ApiBroker/", "**")
  bucket = aws_s3_bucket.bucket.id
  key    =  "broker/${each.value}"
  source = "${path.cwd}/../ApiBroker/${each.value}"
}

output "bucket" {
  value = aws_s3_bucket.bucket.id
}