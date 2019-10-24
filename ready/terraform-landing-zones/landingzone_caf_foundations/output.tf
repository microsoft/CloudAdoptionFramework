output "blueprint_foundations" {
  sensitive   = true                      # to hide content from logs
  value       = module.blueprint_foundations
}

output "prefix" {
  value = local.prefix
}

output "tags" {
  value = var.tags_hub
}
