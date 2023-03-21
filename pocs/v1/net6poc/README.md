# PoC para fazer o Proxy com .NET Core 6

Features atualmente:
* Obter configurações de recursos e provedores de um arquivo de configuração (`appsettings.Development.json`)
* Proxy da requisição apenas para parâmetros via rota
* Map do response body para o que foi configurado para o recurso

Obs: obtenção do provedor mais disponível é aleatório

Logs de teste lcoal com o tempo gasto para processamento em **Milisegundos**:
```
/usr/local/share/dotnet/dotnet /Users/pedrodias/dev/brocker/net6poc/src/ReverseProxy/bin/Debug/net6.0/ReverseProxy.dll
[13:38:30 INF] Now listening on: http://localhost:5069
[13:38:30 INF] Application started. Press Ctrl+C to shut down.
[13:38:30 INF] Hosting environment: Development
[13:38:30 INF] Content root path: /Users/pedrodias/dev/brocker/net6poc/src/ReverseProxy/
[13:38:32 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:32 INF] Time elapsed for request: 141
[13:38:32 INF] Total time elapsed for proxy to provider brasilapi: 301
[13:38:32 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 329.6352ms
[13:38:33 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:34 INF] Time elapsed for request: 620
[13:38:34 INF] Total time elapsed for proxy to provider viacep: 625
[13:38:34 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 626.4612ms
[13:38:35 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:35 INF] Time elapsed for request: 144
[13:38:35 INF] Total time elapsed for proxy to provider viacep: 146
[13:38:35 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 146.7499ms
[13:38:35 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:36 INF] Time elapsed for request: 140
[13:38:36 INF] Total time elapsed for proxy to provider viacep: 142
[13:38:36 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 142.8284ms
[13:38:36 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:36 INF] Time elapsed for request: 140
[13:38:36 INF] Total time elapsed for proxy to provider viacep: 142
[13:38:36 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 142.8371ms
[13:38:37 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:37 INF] Time elapsed for request: 141
[13:38:37 INF] Total time elapsed for proxy to provider viacep: 142
[13:38:37 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 143.5768ms
[13:38:38 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:38 INF] Time elapsed for request: 140
[13:38:38 INF] Total time elapsed for proxy to provider viacep: 141
[13:38:38 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 142.5882ms
[13:38:39 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:39 INF] Time elapsed for request: 145
[13:38:39 INF] Total time elapsed for proxy to provider viacep: 146
[13:38:39 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 147.5509ms
[13:38:39 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:39 INF] Time elapsed for request: 24
[13:38:39 INF] Total time elapsed for proxy to provider brasilapi: 25
[13:38:39 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 26.0464ms
[13:38:40 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:40 INF] Time elapsed for request: 21
[13:38:40 INF] Total time elapsed for proxy to provider brasilapi: 22
[13:38:40 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 23.4138ms
[13:38:41 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:41 INF] Time elapsed for request: 162
[13:38:41 INF] Total time elapsed for proxy to provider viacep: 163
[13:38:41 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 164.3078ms
[13:38:41 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:41 INF] Time elapsed for request: 20
[13:38:41 INF] Total time elapsed for proxy to provider brasilapi: 22
[13:38:41 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 22.6586ms
[13:38:42 INF] Request starting HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - -
[13:38:42 INF] Time elapsed for request: 20
[13:38:42 INF] Total time elapsed for proxy to provider brasilapi: 21
[13:38:42 INF] Request finished HTTP/1.1 GET http://localhost:5069/api/cep/01222020 - - - 200 - application/json;+charset=utf-8 22.5509ms
```

:warning: **Obs 1:** o tempo total da requisição no Postman é maior do que está no Log. Geralmente uns de 3 a 5 milisegundos a mais. Pode ser o tempo que o .NET demora para responser depois de processar, mas precisa ser melhor avaliado. Os tempos mostrados nos logs são apenas do processamento que é feito na aplicação.

**Obs 2:** as requisições foram sequenciais. Nenhum teste foi feito para estressar o sistema com várias requisições por segundo.

## Descrição para entender o código

Formato da url esperada: `/api/<RECURSO>/<PARAMETROS>`.

Exemplo: http://localhost:5069/api/cep/01222020

Passo a passo:
* `Program.cs` é o ponto inicial
  * Map para rotas que começam com `/api` são tratadas pelo `ReverseProxyHandler`
* No `ReverseProxyHandler.cs`
  * Obtem os dados do provedor selecionado para responser a requisição (`TargetResolver`):
    * Identifica o recurso recebido na requisição
    * Obtem o provedor que vai responder aleatoriamente com base no que foi configurado em `appsettings.Development.json`
    * Monta a URL do provedor (como é só parametros de rota, faz só o replace dos parametros na ordem em que foram recebidos com base no que foi configurado para o provedor selecionado)
  * Faz o Forward da requisição para o provedor (`HttpForwarder`):
    * Envia a requisição para o provedor via HTTP
    * Faz o replace dos campos para os nomes configurados
    * Faz o map do response para pegar apenas os campos que configurados

## Para executar

* Instalar o .NET SDK para versão 6
* `cd net6poc/src/ReverseProxy && dotnet run`

## Referências

- [Building a Reverse Proxy in .NET Core](https://auth0.com/blog/building-a-reverse-proxy-in-dot-net-core/)