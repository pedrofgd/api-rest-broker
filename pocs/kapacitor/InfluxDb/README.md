# Aplicação para gerenciar eventos enviados pelo Kapacitor

**Obs:** O InfluxDB 1.8 dessa PoC não está configurado para exigir autorização, então o Token utilizado aqui ficou só para facilitar trocar para a outra versão que estamos testando (2.6), que já está configurada para autenticação

Essa aplicação tem um EP `GET /{nomeRecurso}/{nomeProvedor}/{success}` para simular uma chamada a um provedor realizada pelo Broker, um EP `POST /webhook`, que recebe os alertas do Kapacitor e um EP `GET /provedores/{nomeRecurso}` para printar a lista de provedores salva em memória.

Os alertas do Kapacitor tratados no EP `/webhook` alteram a lista chave com o `nome_recurso` criada em memória com `IMemoryCache` do .NET Core.

O [InfluxDb.Kapacitor/KapacitorAlertDto.cs](InfluxDb.Kapacitor/KapacitorAlertDto.cs) tem o DTO do payload de alerta enviado pelo Kapacitor.