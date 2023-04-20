// Contar o número de erros (sucesso igual a 0)
//  de cada provedor de um recurso
from(bucket: "logs-alt")
  |> range(start: v.timeRangeStart, stop: v.timeRangeStop)
  |> filter(fn: (r) => r["_measurement"] == "metricas_recursos")
  |> filter(fn: (r) => r["_field"] == "sucesso")
  |> filter(fn: (r) => r["nome_recurso"] == "cep-promise")
  |> filter(fn: (r) => r["_value"] == 0)
  |> count()
  |> yield(name: "count")

// Calcular a média de latência
//  para cada provedor de um recurso
from(bucket: "logs-alt")
  |> range(start: v.timeRangeStart, stop: v.timeRangeStop)
  |> filter(fn: (r) => r["_measurement"] == "metricas_recursos")
  |> filter(fn: (r) => r["nome_recurso"] == "cep-promise")
  |> filter(fn: (r) => r["_field"] == "latencia")
  |> mean()
  |> yield(name: "latencia")


// Join de media de latencia e contagem de erros
//  para cada provedor de um recurso
ranking = () => {
    meanLatency = from(bucket: "logs-alt")
      |> range(start: v.timeRangeStart, stop: v.timeRangeStop)
      |> filter(fn: (r) => r["_measurement"] == "metricas_recursos")
      |> filter(fn: (r) => r["nome_recurso"] == "cep-promise")
      |> filter(fn: (r) => r["_field"] == "latencia")
      |> mean()

    errorCount = from(bucket: "logs-alt")
      |> range(start: v.timeRangeStart, stop: v.timeRangeStop)
      |> filter(fn: (r) => r["_measurement"] == "metricas_recursos")
      |> filter(fn: (r) => r["_field"] == "sucesso")
      |> filter(fn: (r) => r["nome_recurso"] == "cep-promise")
      |> filter(fn: (r) => r["_value"] == 0)
      |> count()

    return join(tables: {meanLatency: meanLatency, errorCount: errorCount}, on: ["nome_provedor"])
        |> map(fn: (r) => ({provider: r.nome_provedor, mean_latency: r._value_meanLatency, error_count: r._value_errorCount}))
        |> sort(columns: ["mean_lantency", "error_count"])
        |> yield(name: "ranking")
}

ranking()