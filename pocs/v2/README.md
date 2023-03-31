# poc v2

Essa PoC é a implementação realizada durante a "Etapa 2: Implementação do sistema proposto e definições sobre os testes" de TCC II. A ideia é que o código escrito nesse diretório seja refinado, com implementação de novas features e resolução dos "To Do" deixados nesse código.

## Rodar localmente

**Necessário:**
* [Docker](https://www.docker.com/products/docker-desktop/)
* [.NET SDK 7.0](https://dotnet.microsoft.com/en-us/download)

Utilizar `sh ./entrypoint.sh` para iniciar o sistema:

* O container do InfluxDB será iniciado em `http://localhost:8086`
  * Detalhes em [monitor/README.md](./monitor/README.md)
* A aplicação vai rodar em `http://localhost:5070`
  * Detalhes em [ApiBroker/README.md](./ApiBroker/README.md)


** Para testar só a aplicação, talvez dê para utilizar o GitHub Codespaces também, dependendo que precisar rodar

** Importar [broker.postman_collection.json](./broker.postman_collection.json) no Postman para testar enviando requisições