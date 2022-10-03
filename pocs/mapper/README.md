## Tasks
- configurar SQLite
- criar as estruturas de 'recurso', 'provedor' e 'mapeamento'
- implementar função de map
- marcar timestamp das execuções da função
- criar um EP genérico para testar
- configurar Artillery para teste de carga
- analisar tempo da função map
   - tempo médio ms
   - % proxy real
   - teste com stress
   - testar com uma API externa real para coletar métricas

## Setup
### Installation
- [Go](https://go.dev/doc/install)
- SQLite3: `sudo apt install sqlite3`

### SQLite3 setup
- Execute seed: `sqlite3 proxy.db < seed.sql`
- Run: `sqlite3 proxy.db` for enter bank, then run sql commands for manipulat (aways finish with ; to execute) (ctrl+D for exit)

### Execute
- `go run .`

