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

### Troubleshoot
#### `package mapper is not in GOROOT (/usr/local/go/src/mapper)`
It may be necessary to add this code at `.bashrc` or `.zshrc`.
```
export GO111MODULE=on
#GOPATH MUST BE OUTSIDE OF GOROOT directory!!!
export GOPATH=/mnt/sda1/programming/gopath
export PATH=$PATH:$GOPATH/bin

export GOROOT=/usr/local/go
export PATH=$PATH:$GOROOT/bin
```

Details on [StackOverflow](https://stackoverflow.com/questions/64448560/golang-package-is-not-in-goroot-usr-local-go-src-packagename)

## Initial test results

### With mapper
#### requests: 300/sec
```
http.codes.200: ......................................... 18000
http.request_rate: ...................................... 300/sec
http.requests: .......................................... 18000
http.response_time:
  min: .................................................. 0
  max: .................................................. 140
  median: ............................................... 2
  p95: .................................................. 7.9
  p99: .................................................. 22
http.responses: ......................................... 18000
vusers.completed: ....................................... 18000
vusers.created: ......................................... 18000
vusers.created_by_name.0: ............................... 18000
vusers.failed: .......................................... 0
vusers.session_length:
  min: .................................................. 2
  max: .................................................. 143.6
  median: ............................................... 4.2
  p95: .................................................. 13.6
  p99: .................................................. 37.7
```

### Without mapping
```
http.codes.200: ........................................... 18000
http.request_rate: ........................................ 300/sec
http.requests: ............................................ 18000
http.response_time:
  min: .................................................... 0
  max: .................................................... 83
  median: ................................................. 0
  p95: .................................................... 3
  p99: .................................................... 13.1
http.responses: ........................................... 18000
vusers.completed: ......................................... 18000
vusers.created: ........................................... 18000
vusers.created_by_name.0: ................................. 18000
vusers.failed: ............................................ 0
vusers.session_length:
  min: .................................................... 1.2
  max: .................................................... 122
  median: ................................................. 2.6
  p95: .................................................... 8.4
  p99: .................................................... 30.9
```