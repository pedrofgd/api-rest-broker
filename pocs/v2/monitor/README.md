##

Documentação:
* [Documentação base](https://docs.influxdata.com/influxdb/v2.6/install/?t=Docker)
* [Documentação client C#](https://github.com/influxdata/influxdb-client-csharp)

**Para executar o InfluxDB localmente:**
1. Executar o `docker-compose.yml` com `docker compose up -d` no diretório do arquivo
2. Acessar [http://localhost:8086](http://localhost:8086)
3. Clicar em "Get Started" e configurar o usuário inicial. O nome do usuário e a senha serão utilizados para acessar o portal do InfluxDB ("admin" e "01Senha!", por exemplo). O "Organization Name" e o "Bucket Name" serão utilizados para salvar os registros no Broker (**manter os nomes "broker" e "logs"**, pois é o que está configurado na aplicação .NET)


**Obter o token de acesso para aplicação do Broker:**
1. Abrir o portal do InfluxDB em [http://localhost:8086](http://localhost:8086)
2. Fazer login com usuário e senha cadastrados
3. Ir no ícone de "seta para cima", na barra lateral esquerda e "API Token" > "Generate API Token" > "All Access API Token"
4. Inserir uma descrição para o token ("broker", por exemplo) e copiar o token gerado
5. Colar o token gerado na aplicação do Broker (atualmente hard coded no [Monitorador.cs](./../ApiBroker/src/ApiBroker.API/Monitoramento/Monitorador.cs))

**Para visualizar os dados:**
1. Primeiro, gerar inserir alguns dados no TSDB, para isso chamar algumas vezes a URL `GET http://localhost:5070/api/cep-promise/01222-020` usando o Postman
2. Acessar o portal do InfluxDB
3. Ir no ícone de "seta para cima" e em "Buckets", depois em "logs", que é o nome do bucket criado para registrar os resultados do Broker
4. Selecionar `metricas_recursos` na aba de filtros (parte inferior) e ativar o toogle "View Raw Data'. Depois clicar em "Submit"

<img width="1440" alt="Captura de Tela 2023-03-23 às 21 53 40" src="https://user-images.githubusercontent.com/50634340/227397983-e79ce956-3771-4f75-9e2f-94d5a6fafacd.png">

## Modelagem

Possível modelagem dos dados no InfluxDB:

**Measurement: logs**
* Retention policy: 1 semana
* Tags: nome do recurso, nome do provedor
* Fields: tempo de resposta, sucesso (bool), "completude" (bool)

** *Measurement* é equivalente a uma tabela no banco de dados<br>
** *Tags* são metadados para descrever os fields<br>
** *Fields* são informações que mudam conforme o tempo<br>

## Ranqueamento

A ideia inicial é fazer consulta (script) para obter os dados pelo nome do recurso, ordenar por tempo de resposta e contar a quantidade de erros / vezes que excedeu o tempo desejado (passando as configurações da aplicação Broker). Depois de obter os dados, tentar aplicar um filtro no C# para obter o melhor provedor (pegar o provedor que atende aos critérios das configurações e, caso nenhum atenda, pegar o primeiro da lista retornada na consulta... que já estaria ordenada)

Scripts de teste:

* Filtrar por status da requisição (sucesso ou não)
``` flux
// Obtem a contagem de erro de cada provedor do recurso cep_promise
from(bucket: "logs")
    |> range(start: -1d)
    |> filter(fn: (r) => r._measurement == "metricas_recursos" and
                        r.nome_recurso == "cep-promise" and
                        r._field == "sucesso" and
                        r._value == false)
    |> count()
```
<img width="1252" alt="Captura de Tela 2023-03-24 às 13 09 27" src="https://user-images.githubusercontent.com/50634340/227583495-fbe7e729-5310-4bbb-8277-ee06a6d53029.png">

<br>

* Filtrar por média de latência
``` flux
// Obtem a média de latência de cada provedor do recurso cep_promise
from(bucket: "logs")
    |> range(start: -1d)
    |> filter(fn: (r) => r._measurement == "metricas_recursos" and
                        r.nome_recurso == "cep-promise" and
                        r._field == "latencia")
    |> group(columns: ["nome_provedor"])
    |> mean(column: "_value")
```
<img width="1252" alt="Captura de Tela 2023-03-24 às 13 23 59" src="https://user-images.githubusercontent.com/50634340/227583833-78590e99-6926-4497-98b6-d3a87c1d3e7b.png">
