README - Desenvolvimento

Requisitos locais:
- .NET SDK (versão compatível com o projeto, alvo: netcoreapp3.1)
- SQL Server (ou instância compatível)
- Python 3.8+ para o script `cadastro-peixe/reconhecer_peixe.py`

Comandos úteis:

1) Restaurar e construir
```powershell
cd PBL
dotnet restore
dotnet build PBL.csproj
```

2) Executar a aplicação
```powershell
cd PBL
dotnet run --project PBL.csproj
```

3) Aplicar script SQL (faça backup antes)
- Use o `sqlcmd` ou o SSMS para executar `PBL/Scripts_BD.sql` no banco alvo.

4) Ambiente Python para IA
```bash
cd cadastro-peixe
python -m venv .venv
.venv\Scripts\activate   # Windows
pip install -r requirements.txt
# Exportar variável GEMINI_API_KEY no ambiente (ou usar dotenv)
set GEMINI_API_KEY=seu_token_aqui
python reconhecer_peixe.py --image path/to/image.jpg
```

5) Testar API (ex.: com curl/Postman)
- Veja `postman_collection_min.json` e `API_DEMO.md` para exemplos.

6) CI
- Há um workflow exemplo em `.github/workflows/dotnet-ci.yml` que roda restore/build.

Observações:
- Não armazene chaves de API no código; use variáveis de ambiente.
- Faça backup do banco antes de aplicar `Scripts_BD.sql`.
