# PBL-GlitterComputer (Aquamarine)

Aplicação web em ASP.NET Core 3.1 desenvolvida para o PBL da disciplina Linguagem de Programação I (Fundação Salvador Arena — EC5).

O sistema cobre autenticação, cadastro e consulta de aquários/peixes, dashboards de leituras IoT, controle de Smart Lamp via MQTT e uma integração opcional para preencher parâmetros do peixe a partir de imagem ou espécie.

## Funcionalidades

- Login/cadastro e áreas internas com sessão
- CRUD de aquários e peixes
- Upload de fotos de peixes
- Consultas com filtros (peixes e leituras)
- Dashboard de sensores e Smart Lamp
- API REST para leituras IoT em `/api/leituras` (Swagger em `/swagger`)
- Preenchimento automático de parâmetros do peixe (opcional)

## Requisitos

- .NET SDK compatível com `netcoreapp3.1`
- SQL Server (ou compatível com `System.Data.SqlClient`)
- Broker MQTT (opcional, para Smart Lamp)
- Chave de API para o serviço de análise (opcional, quando usar preenchimento automático)

## Executar

```powershell
cd PBL\PBL
dotnet restore
dotnet build
dotnet run --project PBL.csproj
```

A rota padrão abre a tela de login. Após autenticar, o menu inicial fica em `/Home`.

## Configuração

### Banco de dados

1. Crie o banco e execute `PBL/PBL/Scripts_BD.sql` no SQL Server.
2. Ajuste a string de conexão em `PBL/PBL/DAO/ConexaoBD.cs` conforme seu ambiente.

### Variáveis/segredos

O projeto lê um arquivo `.env` (não versionado) e/ou variáveis de ambiente.

Exemplo em `PBL/.env`:

```env
# opcional
GEMINI_API_KEY=sua_chave_aqui
```

Também são aceitas variáveis `GOOGLE_API_KEY` e `FishAi__ApiKey`.

### MQTT e FIWARE

Se for usar Smart Lamp e/ou FIWARE, revise `PBL/PBL/appsettings.json` (seções `SmartLampMqtt` e `FiwareSthComet`).

## Estrutura

```
PBL-GlitterComputer/
├── README.md
└── PBL/
    └── PBL/
        ├── Controllers/
        ├── DAO/
        ├── Models/
        ├── Services/
        ├── Views/
        ├── wwwroot/
        ├── Scripts_BD.sql
        └── appsettings.json
```

## Documentação

Os documentos do projeto ficam em `PBL/PBL/docs`.

- `INDEX.md`: índice da documentação
- `QUICKSTART.md`: passo a passo rápido
- `FEATURES.md`: telas e funcionalidades
- `ARCHITECTURE.md`: visão geral de arquitetura e fluxos

## CI

Build automatizado via GitHub Actions em `PBL/PBL/.github/workflows/dotnet-ci.yml`.

## Disciplina e equipe

Projeto PBL — Linguagem de Programação I (Fundação Salvador Arena — EC5). Informações adicionais na tela “Sobre” (`/Sobre`).
