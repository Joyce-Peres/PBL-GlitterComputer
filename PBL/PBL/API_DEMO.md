API Demo - comandos curl

1) Swagger UI (navegador):
- Abra: http://localhost:5000/swagger

2) Listar leituras (GET):
- curl:
  curl -sS "http://localhost:5000/api/leituras" | jq

3) Listar leituras por aquário (GET):
- curl:
  curl -sS "http://localhost:5000/api/leituras/aquario/1" | jq

4) Inserir leitura (POST):
- Exemplo:
  curl -X POST "http://localhost:5000/api/leituras" \
    -H "Content-Type: application/json" \
    -d "{\"AquarioId\":1,\"Temperatura\":25.5,\"Ph\":7.2,\"NivelAgua\":85.0}"

Observações:
- Ajuste `localhost:5000` para a porta exibida ao executar `dotnet run`.
- `jq` é útil para formatar JSON; não é obrigatório.

Como testar integração IA (Google Gemini via C#):
- Configure `GEMINI_API_KEY` (ou `GOOGLE_API_KEY`) no ambiente da aplicação.
- Use o endpoint de detecção por espécie:
  curl -X POST "http://localhost:5000/Peixe/DetectarParametrosPorEspecie" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "especie=Betta splendens"
- Ou use o endpoint de upload de imagem no formulário de Peixe (action `DetectarParametros`).
