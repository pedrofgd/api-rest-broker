## Tasks
- [x] configurar SQLite
- [x] criar as estruturas de 'recurso', 'provedor' e 'mapeamento'
- [x] implementar função de map
- [x] marcar timestamp das execuções da função
- [x] criar um EP genérico para testar
- [x] configurar Artillery para teste de carga
- [ ] analisar tempo da função map
   - [ ] tempo médio ms
   - [ ] % proxy real
   - [ ] teste com stress
   - [ ] testar com uma API externa real para coletar métricas

## Setup
### Installation
- [Go](https://go.dev/doc/install)
- SQLite3: `sudo apt install sqlite3`

### SQLite3 setup
- Execute seed: `sqlite3 proxy.db < seed.sql`
- Run: `sqlite3 proxy.db` for enter bank, then run sql commands for manipulat (aways finish with ; to execute) (ctrl+D for exit)

### Execute
- `go run .`

### Run load test
[Artillery.io - Cloud-scale performance testing](https://www.artillery.io/)
- While running de go app, run in a different terminal: `artillery run load.yml` (it will send the number of requests and time configured in the yml file and generate a report at the end)
