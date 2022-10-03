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

DELETE FROM resources;
DELETE FROM providers;
DELETE FROM dictionary;

INSERT INTO resources 
VALUES (
   1, -- id
   'randomtest', -- name
   100, -- intended_response_time_ms
   200, -- timedout_response_time_ms
   10, -- threshold_count_exceeded_intended_response_time
   5, -- threshold_count_timedout_request
   5, -- thershold_count_error
   1 -- threshold_interval_hours
   );

INSERT INTO providers
VALUES (
   1, -- id
   'https://teste.com/1', -- url
   1, -- resource_id
   1, -- priority
   60 -- healthcheck_frequency_seconds
   );

INSERT INTO dictionary
VALUES (
   1, -- id
   1, -- provider_id
   'request', -- payload_type
   0, -- response_http_status_code
   'mapped_filed_1', -- provider_key_name
   'field_1' -- resource_key_name
   ),
   (2,1,'request',0,'mapped_request_field_2','field_2'),
   (3,1,'request',0,'mapped_request_field_3','field_3'),
   (4,1,'request',0,'mapped_request_field_4','field_4'),
   (5,1,'request',0,'mapped_request_field_5','field_5'),
   (6,1,'request',0,'mapped_request_field_6','field_6'),
   (7,1,'request',0,'mapped_request_field_7','field_7'),
   (8,1,'request',0,'mapped_request_field_8','field_8'),
   (9,1,'request',0,'mapped_request_field_9','field_9'),
   (10,1,'request',0,'mapped_request_field_10','field_10'),
   (11,1,'request',0,'mapped_request_field_11','field_11'),
   (12,1,'request',0,'mapped_request_field_12','field_12'),
   (13,1,'request',0,'mapped_request_field_13','field_13'),
   (14,1,'request',0,'mapped_request_field_14','field_14'),
   (15,1,'request',0,'mapped_request_field_15','field_15'),
   (16,1,'response',0,'mapped_response_field_1','response_filed_1'),
   (17,1,'response',0,'mapped_response_field_2','response_filed_2'),
   (18,1,'response',0,'mapped_response_field_3','response_filed_3');
COMMIT;