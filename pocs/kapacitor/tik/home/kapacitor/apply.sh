# Definir tasks
kapacitor define latency_alert_stream -type stream -tick ./latency_alert_stream.tick -dbrp logs.autogen
kapacitor define error_alert_stream -type stream -tick ./error_alert_stream.tick -dbrp logs.autogen

# Habilitar tasks
kapacitor enable latency_alert_stream
kapacitor enable error_alert_stream