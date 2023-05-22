# Utilização do Kapacitor para monitorar e alertar falhas nos provedores

**Versões utilizadas nessa PoC:**
- `InfluxDB 1.8`
- `Kapacitor OSS 1.6.6 (git: HEAD 79897085a4802304bb2fb052035bac4d16913302)`

## Resultados obtidos

Com essa poc estamos conseguindo enviar altertas de:
* quando o healthcheck identifica que um provedor está disponível
* quando a quantidade de requisições com erro seguidas atinge um limite configurado e 
* quando a quantidade de requisições seguidas com latência maior que um trheshold atinge um limite configurado.

Os alertas estão sendo enviados para a aplicação `FakeBroker`, para testar o comportamento.

Esses são 2 exemplos de paylaods de alertas enviados pelo Kapacitor via HTTP:
``` json
{
    "id": "status_provedor_p1pa",
    "message": "status_provedor_p1pa is CRITICAL",
    "details": "The past 2 requests for \"nome_recurso\": p1 and \"nome_provedor\": pa were considered to change to this level alert.",
    "time": "2023-05-21T23:51:10.038Z",
    "duration": 0,
    "level": "CRITICAL",
    "data": {
        "series": [
            {
                "name": "performance_influx",
                "tags": {
                    "nome_provedor": "pa",
                    "nome_recurso": "p1"
                },
                "columns": [
                    "time",
                    "consecutive_error",
                    "latencia",
                    "sucesso"
                ],
                "values": [
                    [
                        "2023-05-21T23:51:10.038Z",
                        2,
                        251,
                        0
                    ]
                ]
            }
        ]
    },
    "previousLevel": "INFO",
    "recoverable": false
}
```

``` json
{
    "id": "status_provedor_p1pa",
    "message": "status_provedor_p1pa is INFO",
    "details": "The past -1 requests for \"nome_recurso\": p1 and \"nome_provedor\": pa were considered to change to this level alert.",
    "time": "2023-05-21T23:51:12.352Z",
    "duration": 2314000000,
    "level": "INFO",
    "data": {
        "series": [
            {
                "name": "performance_influx",
                "tags": {
                    "nome_provedor": "pa",
                    "nome_recurso": "p1"
                },
                "columns": [
                    "time",
                    "consecutive_error",
                    "latencia",
                    "sucesso"
                ],
                "values": [
                    [
                        "2023-05-21T23:51:12.352Z",
                        -1,
                        251,
                        1
                    ]
                ]
            }
        ]
    },
    "previousLevel": "CRITICAL",
    "recoverable": false
}
```

## Executar localmente

Basta rodar `docker compose up -d` na raíz do projeto. O `docker-compose.yml` vai criar os containers do InfluxDB, Kapacitor e Telegraf (embora não estejamos usando por enquanto).

## Arquivos e estrutura de pastas

A estrutura dessa poc foi baixada de um exemplo disponibilizado pela influxdata nesse [link](https://docs.influxdata.com/downloads/tik-docker-tutorial.tar.gz).

Em `/etc` temos os arquivos de configuração do Kapacitor e do Telegraf (utilizado para enviar dados para o Influx, porém não estamos utilizando).

Os arquivos `.conf` desses diretórios cotém **todas** as configurações disponíveis, com todos os plugins, por isso é tão grande, mas a maioria está comentado, apenas como referência.

Por curiosidade, esses arquivos `.conf` estão no formato [TOML](https://toml.io/en/)

A configuração mais importante do Kapacitor que vamos utilizar nesse diretório é o `[[influxdb]]`, para definir a url e as credenciais de acesso ao InfluxDB.

Em `/home` ficam os arquivos `.tick`, que é onde definimos as tasks do Kapacitor. Essas tasks serão utilizadas para analisar os dados do InfluxDB e gerar alertas no nosso Broker.

Existem dois tipos de tasks: `Stream` e `Batch`. Os detalhes sobre cada tipo estão nesse [artigo da InfluxData](https://www.influxdata.com/blog/batch-processing-vs-stream-processing/). Nessa poc utilizamos o tipo `Stream`, que cria uma subscription para as escritas de um banco de dados e processa a tarefa a cada novo registro.

Criamos uma task para:
* Monitorar os erros do provedor e notificar quando fica disponível/indisponível
* Monitorar a latência do provedor e notificar quando a performance fica abaixo do desejado

Em `/var` temos os arquivos de logs gerados pelos alertas.

## Comandos

Os detalhes sobre todos os comandos estão na documentação oficial da influxdata (link mais abaixo), mas os principais comandos utilizados no desenvolvimento da poc estão listados a seguir:

1. `docker exec -it tik_kapacitor_1 sh` para acessar o conatiner docker do Kapacitor
2. `cd /home/kapacitor && kapacitor define {NOME_DA_TASK} -type stream -tick ./{NOME_DO_SCRIPT}.tick -dbrp {NOME_DO_BANCO_DE_DADOS}.autogen` para definir a task no Kapacitor
3. `kapacitor enable {NOME_DA_TASK_DEFINIDA}` para habilitar a task definida

O arquivo [apply.sh](/home/kapacitor/apply.sh) tem os exemplos de comandos executados para as tasks criadas nessa poc.

Outros comandos comuns são:
* `kapacitor list tasks` para verificar quais tasks foram definidas e o estado de cada uma
* `kapacitor show {NOME_TASK}` para inspecionar uma task
* `kapacitor disable {NOME_TASK}` para desabilitar uma task
* `kapacitor delete tasks {NOME_DAS_TASKS_SEPARADO_POR_ESPACO}` para remover 1 ou mais tasks

## Principais pontos de aprendizado da poc

### StateCount
Estamos utilizando o `stateCount()` ([doc](https://docs.influxdata.com/kapacitor/v1.6/nodes/state_count_node/)) para contar quantas vezes um determinado evento acontece no InfluxDB (latência ou qtde de erros maior que algum limite). A contagem é interrompida a cada vez que o estado muda, ou seja, se o limite de erros é 10 e o provedor falha 9 vezes, mas na 10a retorna sucesso, o alerta não é disparado, pois a contagem reiniciou.

### Alerta com StateChangesOnly
É importante para nós não ficar enviando eventos duplicados, por exemplo a cada healthcheck ou requisição. Queremos utilizar os eventos para provocar uma alteração no estado do nosso Broker. 

Para o Kapacitor funcionar dessa forma, estamos configurando o `stateChangesOnly` no pipeline do alerta, que só vai disparar um novo alerta caso seja para um estado diferente do anterior.

Os tipos de alerta são OK, INFO, WARNING e CRITICAL.

Estamos trabalhando com INFO, para notificar que um provedor está disponível (1) no primeiro healthceck, feito na inicialização do broker e (2) quando um provedor o healthcheck identifica que um provedor foi recuperado.

No (1) exemplo, o `stateChangesOnly` não vai ficar enviado vários alertas de INFO a cada requisição bem sucedida no Broker. Vai enviar apenas na primeira vez.

Estamos trabalhando com o CRITICAL para notificar (1) que X requisições seguidas estavam acima do limite desejado de tempo de resposta e (2) notificar que X requisições seguidas em um provedor retornaram falha.

O `stateChangesOnly` está sendo usado apenas no 2o exemplo e força que apenas um alerta seja enviado quando o erro ocorrer. O próximo alerta enviado só será de recuperação do provedor.

Para a latência não estamos nos preocupando com a "recuperação" do provedor, por enquanto. Nesse cenário a latência vai disparar um alerta e o Broker vai colocar o provedor como última opção da lista. Um novo alerta para o mesmo provedor só será disparado caso os demais provedores também apresentem problemas de latência.

**Observação importante:** o `stateChangesOnly` considera alertas disparados para o mesmo id. Como estamos lidando com vários provedores por recurso, é importante que o estado do último alerta seja contado pelo grupo recurso/provedor. Para isso definimos o `id` do alerta como sendo `'status_provedor_{{ index .Tags "nome_recurso" }}{{ index .Tags "nome_provedor" }}'`. Assim o Kapacitor não vai enviar alertas duplicados apenas para o mesmo grupo, mas vai notificar mudanças em outros provedores do mesmo recurso.

## Documentação e Referências

- [Documentação oficial influxdata: Getting Started with TICK and Docker Compose](https://docs.influxdata.com/kapacitor/v1.6/introduction/install-docker/)
- [Artigo exemplo (Medium): A DevOps tutorial to setup intelligent machine learning driven alerts](https://medium.com/loud-ml/a-devops-tutorial-to-setup-intelligent-machine-learning-driven-alerts-c4de93cf6d66)

