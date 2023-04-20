# Diagramas

Para exportar um diagrama `.mmd`, utilizar o comando: `docker run --rm -v $(pwd)/diagramas:/data minlag/mermaid-cli -i <NOME_ARQUIVO.mmd -o <NOME_ARQUIVO>.png --scale 5`, ou utilizar o workflow no Github Actions.