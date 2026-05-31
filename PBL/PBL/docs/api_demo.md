# API Demo — Aquamarine (IoT)

Este documento demonstra o requisito de EC5:
- **API que recebe e devolve dados do dispositivo IoT em JSON**
- **Documentação via Swagger** (e demonstração opcional via Postman)

## Endpoints (JSON)

Base URL (local): `http://localhost:5000`

- `GET /api/leituras`
  - Retorna lista JSON com as leituras mais recentes (pode ser vazia).
  - Filtros opcionais (query): `aquarioId`, `dataInicio`, `dataFim`.

- `GET /api/leituras/aquario/{aquarioId}`
  - Retorna lista JSON com leituras do aquário informado.
  - Filtros opcionais (query): `dataInicio`, `dataFim`.

- `POST /api/leituras`
  - Recebe JSON do dispositivo IoT e registra a leitura.
  - `Content-Type: application/json`

## 1) Executar o projeto

```powershell
cd PBL\PBL
dotnet restore
dotnet run
```

Abra o navegador em:
- App: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

## 2) Demonstração pelo Swagger

1. Acesse `http://localhost:5000/swagger`
2. Abra **Leituras IoT**
3. Execute um `POST /api/leituras` com o corpo abaixo
4. Em seguida execute `GET /api/leituras` para ver o retorno em JSON

### Exemplo de payload (POST)

```json
{
  "aquarioId": 1,
  "temperatura": 25.5,
  "nivelAgua": 85,
  "tdsPpm": 120,
  "salinidadePpt": 1.85,
  "qualidadeTds": "Boa"
}
```

## 3) Demonstração pelo Postman

Há uma collection pronta no projeto:
- `PBL/PBL/postman_collection_min.json`

Passos:
1. Postman → **Import** → selecione `postman_collection_min.json`
2. Rode as requisições na ordem:
   - `POST /api/leituras`
   - `GET /api/leituras`
   - `GET /api/leituras/aquario/1`

### Observação sobre a porta

Se o `dotnet run` subir em outra porta, ajuste a URL no Postman e/ou use a base URL mostrada no console.
