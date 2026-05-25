# API Demo — Aquamarine (IoT)

## Documentação interativa

Abra no navegador (com a aplicação rodando):

```
http://localhost:5000/swagger
```

A UI Swagger inclui descrição dos endpoints, parâmetros, schemas e códigos de resposta (200, 400, 500).

## Variáveis

- `BASE_URL` — URL base (ex.: `http://localhost:5000`).

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/leituras` | Lista leituras (filtros opcionais: `aquarioId`, `dataInicio`, `dataFim`) |
| GET | `/api/leituras/aquario/{aquarioId}` | Leituras de um aquário |
| POST | `/api/leituras` | Registra leitura do dispositivo IoT |

## Exemplos curl

Listar leituras:

```bash
curl -sS "%BASE_URL%/api/leituras" 
```

Listar por aquário:

```bash
curl -sS "%BASE_URL%/api/leituras/aquario/1"
```

Registrar leitura (POST):

```bash
curl -X POST "%BASE_URL%/api/leituras" ^
  -H "Content-Type: application/json" ^
  -d "{\"aquarioId\":1,\"temperatura\":25.5,\"nivelAgua\":85,\"tdsPpm\":120,\"salinidadePpt\":0.35,\"qualidadeTds\":\"Boa\"}"
```

Resposta esperada (200):

```json
{
  "mensagem": "Leitura registrada com sucesso.",
  "aquarioId": 1,
  "dataLeitura": "2026-05-24T20:00:00"
}
```

> Substitua `%BASE_URL%` pela porta exibida no `dotnet run`.
