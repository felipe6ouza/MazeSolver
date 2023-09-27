## Instalando em Ambiente local com Docker

Clone o projeto e navegue até o diretório do projeto que contem o arquivo ```DockerFile```

Build da imagem Docker:

```
docker build -t api-game-map .
```

Isso irá criar uma imagem Docker com base no Dockerfile no diretório.

Execute o contêiner:

```
docker run -p 8080:80 api-game-map

```

Agora, sua API está em execução em um contêiner Docker, mapeando a porta 8080 do host para a porta 80 do contêiner.

Acesse a API em seu navegador ou em ferramentas como o Postman/Insomnia:
URL da API: http://localhost:8080
