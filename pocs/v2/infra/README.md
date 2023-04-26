# Infra

## Criar a infraestrutura na AWS

1. Obter as credenciais da AWS pelo Console (access key e secret key) e exportar usando `export AWS_ACCESS_KEY_ID=<SECRET>` e `export AWS_SECRET_ACCESS_KEY=<SECRET>` ou com `aws configure` (necessário instalar a ferramenta CLI)
2. Executar `terraform init` nesse diretório, para baixar as dependências
3. Executar `terraform validate` para garantir que todas as configurações estão corretas
4. Executar `terraform plan -out demo.tfplan`, para mostar quais recursos serão criados
5. Executar `terraform apply "demo.tfplan"` para efetivamente criar os recursos na AWS