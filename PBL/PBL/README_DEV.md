README - Desenvolvimento

Requisitos locais:
- .NET SDK (versão compatível com o projeto, alvo: netcoreapp3.1)
- SQL Server (ou instância compatível)
- Chave da API Gemini configurada no ambiente (`GEMINI_API_KEY` ou `GOOGLE_API_KEY`)

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

4) Configurar IA (Google Gemini via C#)
```powershell
# Exemplo no Windows (sessão atual)
$env:GEMINI_API_KEY="seu_token_aqui"
```

Opcionalmente, defina `FishAi:Model` e `FishAi:ApiKey` no `appsettings.json`.

5) Testar API (ex.: com curl/Postman)
- Veja `postman_collection_min.json` e `API_DEMO.md` para exemplos.

6) CI
- Há um workflow exemplo em `.github/workflows/dotnet-ci.yml` que roda restore/build.

Observações:
- Não armazene chaves de API no código; use variáveis de ambiente.
- Faça backup do banco antes de aplicar `Scripts_BD.sql`.
