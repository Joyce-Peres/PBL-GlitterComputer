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

Como testar integração IA (reconhecer_peixe.py):
- Configure `GEMINI_API_KEY` no ambiente e execute o script Python:
  python reconhecer_peixe.py --image "path/to/image.jpg"
- O script retorna JSON com `temperaturaMin`, `temperaturaMax`, `luminosidadeMin`, `luminosidadeMax`, `phMin`, `phMax`, `originFromAI` e `parametersUpdatedAt`.
