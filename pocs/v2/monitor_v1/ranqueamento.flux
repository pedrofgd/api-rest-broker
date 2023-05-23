ranking = () => {
    meanLatency = from(bucket: "logs")
        |> range(start: -1h)
        |> filter(fn: (r) => r["_measurement"] == "metricas_recursos")
        |> filter(fn: (r) => r["nome_recurso"] == "cep-java")
        |> filter(fn: (r) => r["_field"] == "latencia")
        |> group(columns: ["nome_provedor"])
        |> mean()
        
    errorCount = from(bucket: "logs")
        |> range(start: -1h)
        |> filter(fn: (r) => r["_measurement"] == "metricas_recursos")
        |> filter(fn: (r) => r["_field"] == "sucesso")
        |> filter(fn: (r) => r["nome_recurso"] == "cep-java")
        |> filter(fn: (r) => r["_value"] == 0, onEmpty: "keep")
        |> group(columns: ["nome_provedor"])
        |> count()

    return join(tables: {meanLatency: meanLatency, errorCount: errorCount}, on: ["nome_provedor"])
        |> map(fn: (r) => ({provider: r.nome_provedor, mean_latency: r._value_meanLatency,
               error_count: r._value_errorCount}))
        |> sort(columns: ["error_count"])
        |> yield(name: "ranking")
}
ranking()