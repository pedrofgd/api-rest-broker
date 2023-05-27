# Fake Provider

JAVA 17

## Subir
`./gradlew bootRun --args=--server.port=8080 -PjvmArgs="-Davailability=10"`

## Rotas
http://localhost:8080/correios-alt/01512020 POST

http://localhost:8080/via-cep/01512020 GET

http://localhost:8080/widenet/01512020 GET
