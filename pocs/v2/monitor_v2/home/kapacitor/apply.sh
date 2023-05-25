#!/bin/bash

# Read the value of KAPACITOR_WEBHOOK_URL environment variable
webhook_url=$KAPACITOR_WEBHOOK_URL

# Iterate over all .tick files in the directory
for file in *.tick; do
    # Replace the placeholder with the webhook URL in the file
    sed -i "s#{{KAPACITOR_WEBHOOK_URL}}#'$webhook_url'#g" "$file"
done

# Define tasks
kapacitor define latency_alert_stream -type stream -tick ./latency_alert_stream.tick -dbrp logs.autogen
kapacitor define error_alert_stream -type stream -tick ./error_alert_stream.tick -dbrp logs.autogen

# Enable tasks
kapacitor enable latency_alert_stream
kapacitor enable error_alert_stream
