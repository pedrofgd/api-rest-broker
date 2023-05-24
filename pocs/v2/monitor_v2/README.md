# Configurações do sistema de monitoramento v2
Nessa versão utilizamos o InfluxDB 1.8 e o Kapacitor para monitorar as métricas em tempo real.

## Deploy

**:warning: Importante para deploy:**
* Criar o database se o influx for 1.8 (não cria sozinho)
* Mudar as portas e URLs nos alertas do Kapacitor

Criar uma rede Docker:
``` bash
docker network create tik_network
```

Para criar e executar o container do InfluxDB:
``` bash
docker run -d \
   --name=influxdb \
   --network=tik_network \
   -p 8086:8086 \
   influxdb:1.8
```

Executar os seguintes comandos no container do InfluxDB:
``` bash
# Criar o banco de dados de logs, utilizado pelo Broker
docker exec influxdb-teste influx -execute 'CREATE DATABASE logs'
```

Para criar e executar o container do Kapacitor:
``` bash
docker run -d \
   --name=kapacitor \
   --network=tik_network \
   pedrofgd/tcc-monitor-kapacitor:latest
```

## Rodar localmente

Utilizar o `docker-compose.yml` para executar localmente.
Por enquanto, é necessário trocar a url do influx no [kapacitor.conf](/etc/kapacitor/kapacitor.conf) para `http://host.docker.internal:8086`, porque, caso o container seja criado dentro de uma rede Docker, o broker local não vai conseguir acessar o banco de dados. Então criar o container localmente sem a rede e fazer esse ajute.

## Build e push das imagens para o Docker Hub

Foi criado um Dockerfile para criar uma imagem com base no `kapacitor:latest` e copiar os arquivos de configuração.

Utilizar `docker build -t pedrofgd/tcc-monitor-kapacitor:<TAG> .` no diretório raíz desse projeto para criar a imagem Docker.

Exemplo:
``` bash
docker build --no-cache --platform linux/arm64 -t pedrofgd/tcc-monitor-kapacitor:v0.1.0 .
docker build --no-cache --platform linux/arm64 -t pedrofgd/tcc-monitor-kapacitor:latest .
```

Criar uma imagem com a tag com versionamento semantico e a tag `latest` junto, para facilitar o controle de versão.

Utilizar `docker push pedrofgd/tcc-monitor-kapacitor:<TAG>` para fazer o upload no Docker Hub. Utilizar `docker login`, caso já não esteja logado (alterar o nome de usuário, caso for fazer o login para uma conta diferente).

Exemplo:
``` bash
docker push pedrofgd/tcc-monitor-kapacitor:v0.1.0
docker push pedrofgd/tcc-monitor-kapacitor:latest
```

Utilizar `docker run -d --name=tcc-monitor-kapacitor pedrofgd/tcc-monitor-kapacitor:<TAG>` para fazer o pull da imagem no Docker Hub e criar o container.