BEGIN;

DROP TABLE [resources];
DROP TABLE [providers];
DROP TABLE [dictionary];

CREATE TABLE IF NOT EXISTS [resources] (
   id INTEGER NOT NULL PRIMARY KEY,
   name TEXT NOT NULL,
   intended_response_time_ms INTEGER NOT NULL,
   timedout_response_time_ms INTEGER NOT NULL,
   threshold_count_exceeded_intended_response_time INTEGER NOT NULL,
   threshold_count_timedout_request INTEGER NOT NULL,
   thershold_count_error INTEGER NOT NULL,
   threshold_interval_hours INTEGER NOT NULL
   );

CREATE TABLE IF NOT EXISTS [providers] (
   id INTEGER NOT NULL PRIMARY KEY,
   url TEXT NOT NULL,
   resource_id INTEGER NOT NULL,
   priority INTEGER NOT NULL,
   healthcheck_frequency_seconds INTEGER NOT NULL,
   FOREIGN KEY (resource_id) REFERENCES resources(id)
   );

CREATE TABLE IF NOT EXISTS [dictionary] (
   id INTEGER PRIMARY KEY,
   provider_id INTEGER NOT NULL,
   payload_type TEXT NOT NULL,
   response_http_status_code INTEGER NOT NULL,
   provider_key_name TEXT NOT NULL,
   resource_key_name TEXT NOT NULL,
   FOREIGN KEY (provider_id) REFERENCES providers(id)
   );

COMMIT;