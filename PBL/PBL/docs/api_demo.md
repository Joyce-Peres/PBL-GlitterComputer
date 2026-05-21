# API Demo — Exemplos para testes

## Variáveis
- `BASE_URL` — URL base da aplicação em desenvolvimento (ex.: `http://localhost:5000`).

## Swagger
- Abra: `BASE_URL/swagger` para explorar endpoints via UI.

## Exemplos curl

Listar leituras (GET):

```bash
curl -sS "${BASE_URL}/api/leituras" | jq
```

Listar leituras por aquário (GET):

```bash
curl -sS "${BASE_URL}/api/leituras/aquario/1" | jq
```

Inserir leitura (POST):

```bash
curl -X POST "${BASE_URL}/api/leituras" \
  -H "Content-Type: application/json" \
  -d '{"AquarioId":1,"Temperatura":25.5,"Ph":7.2,"NivelAgua":85.0}'
```

> Nota: ajuste `BASE_URL` conforme a porta mostrada ao executar `dotnet run`.

## Testes IA (Google Gemini)

Configurar `GEMINI_API_KEY` no ambiente da aplicação. Exemplo de endpoint para detecção por espécie:

```bash
curl -X POST "${BASE_URL}/Peixe/DetectarParametrosPorEspecie" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "especie=Betta splendens"
```

Ou utilizar o formulário de upload em `Views/Peixe/form.cshtml` (action `DetectarParametros`).

---
Arquivo otimizado de `API_DEMO.md` com placeholders e instruções diretas.
