BEGIN;

DELETE FROM resources;
DELETE FROM providers;
DELETE FROM dictionary;

INSERT INTO resources 
VALUES (
   1, -- id
   'numbers', -- name
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
VALUES
   (1, 1, 'request', 0, 'zero', 'zero'),
   (2, 1, 'request', 0, 'one', 'um'),
   (3, 1, 'request', 0, 'two', 'dois'),
   (4, 1, 'request', 0, 'three', 'tres'),
   (5, 1, 'request', 0, 'four', 'quatro'),
   (6, 1, 'request', 0, 'five', 'cinco'),
   (7, 1, 'request', 0, 'six', 'seis'),
   (8, 1, 'request', 0, 'seven', 'sete'),
   (9, 1, 'request', 0, 'eight', 'oito'),
   (10, 1, 'request', 0, 'nine', 'nove'),
   (11, 1, 'request', 0, 'ten', 'dez'),
   (12, 1, 'request', 0, 'eleven', 'onze'),
   (13, 1, 'request', 0, 'twelve', 'doze'),
   (14, 1, 'request', 0, 'thirteen', 'treze'),
   (15, 1, 'request', 0, 'fourteen', 'quatorze'),
   (16, 1, 'request', 0, 'fifteen', 'quinze'),
   (17, 1, 'request', 0, 'sixteen', 'dezesseis'),
   (18, 1, 'request', 0, 'seventeen', 'dezessete'),
   (19, 1, 'request', 0, 'eighteen', 'dezoito'),
   (20, 1, 'request', 0, 'nineteen', 'dezenove'),
   (21, 1, 'request', 0, 'twenty', 'vinte'),
   (22, 1, 'request', 0, 'twenty_one', 'vinte_um'),
   (23, 1, 'request', 0, 'twenty_two', 'vinte_dois'),
   (24, 1, 'request', 0, 'twenty_three', 'vinte_três'),
   (25, 1, 'request', 0, 'twenty_four', 'vinte_quatro'),
   (26, 1, 'request', 0, 'twenty_five', 'vinte_cinco'),
   (27, 1, 'request', 0, 'twenty_six', 'vinte_seis'),
   (28, 1, 'request', 0, 'twenty_seven', 'vinte_sete'),
   (29, 1, 'request', 0, 'twenty_eight', 'vinte_oito'),
   (30, 1, 'request', 0, 'twenty_nine', 'vinte_nove'),
   (31, 1, 'request', 0, 'thirty', 'trinta'),
   (32, 1, 'request', 0, 'thirty_one', 'trinta_um'),
   (33, 1, 'request', 0, 'thirty_two', 'trinta_dois'),
   (34, 1, 'request', 0, 'thirty_three', 'trinta_tres'),
   (35, 1, 'request', 0, 'thirty_four', 'trinta_quatro'),
   (36, 1, 'request', 0, 'thirty_five', 'trinta_cinco'),
   (37, 1, 'request', 0, 'thirty_six', 'trinta_seis'),
   (38, 1, 'request', 0, 'thirty_seven', 'trinta_sete'),
   (39, 1, 'request', 0, 'thirty_eight', 'trinta_oito');
COMMIT;