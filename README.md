# PBL-GlitterComputer

Projeto PBL — Aquário Inteligente (solução .NET Core 3.1)

**Visão geral**
- Aplicação web para gerenciamento de aquários, leituras de sensores e controle de uma Smart Lamp (via MQTT). Inclui integração opcional de IA para reconhecimento de peixes.

**Requisitos**
- .NET SDK compatível com `netcoreapp3.1`
- SQL Server (ou compatível)
- Chave de IA em variável de ambiente: `GEMINI_API_KEY` (ou `GOOGLE_API_KEY` conforme configuração)

**Instalação / Execução (desenvolvimento)**
1. Restaurar e compilar:
```powershell
cd PBL
dotnet restore
dotnet build PBL.csproj
```
2. Executar a aplicação:
```powershell
cd PBL
dotnet run --project PBL.csproj
```

**Banco de dados**
- Execute `PBL/PBL/Scripts_BD.sql` no seu SQL Server para criar/atualizar tabelas e procedures.
- A string de conexão e lógica está em `PBL/PBL/DAO/ConexaoBD.cs`.

**Smart Lamp (MQTT + PWA)**
- Configurações MQTT em `PBL/PBL/appsettings.json` (seção `SmartLampMqtt`).
- A PWA está em `PBL/PBL/wwwroot/smartlamp-app/` (arquivo principal: `index.html`).
- Acesse a PWA quando o projeto estiver rodando em `/smartlamp-app/index.html`.

**Cadastro de peixe com IA**
- No formulário de cadastro de peixe é possível enviar uma foto e usar o recurso "Detectar parâmetros pela imagem (IA)".
- Configure a chave de API via variável de ambiente `GEMINI_API_KEY`.
- Configurações do serviço de IA podem ser ajustadas em `appsettings.json` (seção `FishAi`).

**Desenvolvimento e testes rápidos**
- Coleção Postman: `PBL/PBL/postman_collection_min.json`.
- Exemplos de uso da API: `PBL/PBL/API_DEMO.md`.
- CI exemplo: `.github/workflows/dotnet-ci.yml` (restore/build).

**Boas práticas / Observações**
- Nunca incluir chaves de API em código-fonte; use variáveis de ambiente.
- Faça backup do banco antes de aplicar `Scripts_BD.sql`.

**Notas sobre documentação consolidada**
- Este `README.md` consolida informações que estavam em `PBL/PBL/README_DEV.md` e `PBL/PBL/wwwroot/smartlamp-app/README.md`.
- Foram removidos os READMEs secundários para manter a documentação centralizada.

**Documentação adicional**
- Documentos otimizados disponíveis em `PBL/PBL/docs/` (checklist, security, api_demo).

---
Se quiser, posso rodar os testes ou commitar as mudanças para você.
