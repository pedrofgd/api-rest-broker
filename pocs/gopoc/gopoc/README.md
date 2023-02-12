# PoC para Proxy das requisições com GoLang

Features testadas até o momento:
* Só o forward da requisição (sem obter nenhuma configuração, ou mapear os campos)

Logs de teste lcoal com o tempo gasto para processamento em **Milisegundos**:
```
T=/usr/local/go #gosetup
GOPATH=/Users/pedrodias/go #gosetup
/usr/local/go/bin/go build -o /private/var/folders/wv/ll8xtxln0t97zg7g9gxkl8n00000gn/T/GoLand/___1go_build_gopoc gopoc #gosetup
/private/var/folders/wv/ll8xtxln0t97zg7g9gxkl8n00000gn/T/GoLand/___1go_build_gopoc
[reverse proxy server] received request at: 2023-02-12 14:01:02.556845 -0300 -03 m=+98.255315834
2023/02/12 14:01:03 Time elapsed for request: 546.810875ms
2023/02/12 14:01:03 Total time elapsed for proxy: 547.396083ms
[reverse proxy server] received request at: 2023-02-12 14:01:04.844112 -0300 -03 m=+100.542620501
2023/02/12 14:01:05 Time elapsed for request: 195.757ms
2023/02/12 14:01:05 Total time elapsed for proxy: 195.936333ms
[reverse proxy server] received request at: 2023-02-12 14:01:05.88378 -0300 -03 m=+101.582306751
2023/02/12 14:01:06 Time elapsed for request: 181.887667ms
2023/02/12 14:01:06 Total time elapsed for proxy: 182.081834ms
[reverse proxy server] received request at: 2023-02-12 14:01:06.707535 -0300 -03 m=+102.406075001
2023/02/12 14:01:06 Time elapsed for request: 157.401709ms
2023/02/12 14:01:06 Total time elapsed for proxy: 157.546833ms
[reverse proxy server] received request at: 2023-02-12 14:01:07.448374 -0300 -03 m=+103.146926709
2023/02/12 14:01:07 Time elapsed for request: 171.860292ms
2023/02/12 14:01:07 Total time elapsed for proxy: 171.969ms
[reverse proxy server] received request at: 2023-02-12 14:01:08.100887 -0300 -03 m=+103.799450417
2023/02/12 14:01:08 Time elapsed for request: 156.198916ms
2023/02/12 14:01:08 Total time elapsed for proxy: 156.2915ms
[reverse proxy server] received request at: 2023-02-12 14:01:08.852756 -0300 -03 m=+104.551331751
2023/02/12 14:01:09 Time elapsed for request: 167.108125ms
2023/02/12 14:01:09 Total time elapsed for proxy: 167.219292ms
[reverse proxy server] received request at: 2023-02-12 14:01:09.505555 -0300 -03 m=+105.204141542
2023/02/12 14:01:09 Time elapsed for request: 156.32825ms
2023/02/12 14:01:09 Total time elapsed for proxy: 156.644292ms
[reverse proxy server] received request at: 2023-02-12 14:01:10.188867 -0300 -03 m=+105.887464584
2023/02/12 14:01:10 Time elapsed for request: 155.564166ms
2023/02/12 14:01:10 Total time elapsed for proxy: 155.710625ms
[reverse proxy server] received request at: 2023-02-12 14:01:10.819804 -0300 -03 m=+106.518412251
2023/02/12 14:01:11 Time elapsed for request: 265.066333ms
2023/02/12 14:01:11 Total time elapsed for proxy: 265.173416ms
[reverse proxy server] received request at: 2023-02-12 14:01:11.582378 -0300 -03 m=+107.280999459
2023/02/12 14:01:11 Time elapsed for request: 156.245667ms
2023/02/12 14:01:11 Total time elapsed for proxy: 156.323167ms
[reverse proxy server] received request at: 2023-02-12 14:01:12.243177 -0300 -03 m=+107.941809376
2023/02/12 14:01:12 Time elapsed for request: 167.906708ms
2023/02/12 14:01:12 Total time elapsed for proxy: 168.039625ms
```

**Obs 1:** as requisições foram sequenciais. Nenhum teste foi feito para estressar o sistema com várias requisições por segundo.

## Referências

- [Build reverse proxy server in Go](https://dev.to/b0r/implement-reverse-proxy-in-gogolang-2cp4)