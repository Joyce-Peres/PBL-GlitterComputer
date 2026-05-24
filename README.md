# PBL-GlitterComputer — Aquamarine

Sistema web **Aquamarine** (Aquário Inteligente) desenvolvido em **ASP.NET Core 3.1** para o PBL da disciplina Linguagem de Programação I (Fundação Salvador Arena — EC5).

A aplicação permite cadastrar usuários, gerenciar aquários e peixes, consultar dados com filtros, acompanhar leituras de sensores IoT, controlar a Smart Lamp via MQTT e usar IA (Google Gemini) para sugerir parâmetros a partir de fotos de peixes.

## Funcionalidades

- **Autenticação**: tela inicial de login e cadastro na mesma página; áreas internas exigem sessão ativa.
- **Menu**: após o login, o usuário acessa o menu principal com atalhos (catálogo de peixes, novo aquário, dashboard IoT).
- **CRUDs**: aquários e peixes (com herança em `PadraoController` / `PadraoDAO`).
- **Upload de imagens**: fotos de peixes em `wwwroot/uploads/peixes`.
- **Consultas**: telas de consulta com filtros (peixes e leituras IoT).
- **Dashboards**: leituras dos sensores e dashboard da Smart Lamp.
- **API IoT**: endpoints REST para leituras (`/api/leituras`) documentados no **Swagger** (`/swagger`).
- **IA**: detecção de espécie e parâmetros ideais por imagem no cadastro de peixe.

## Requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) compatível com `netcoreapp3.1`
- **SQL Server** (ou compatível com `System.Data.SqlClient`)
- Chave da API Google Gemini (opcional, para o recurso de IA)

## Estrutura do projeto

```
PBL-GlitterComputer/
├── README.md
└── PBL/
    ├── .env                 # chave GEMINI_API_KEY (não versionado)
    └── PBL/
        ├── PBL.csproj
        ├── Program.cs
        ├── Startup.cs
        ├── appsettings.json
        ├── Scripts_BD.sql
        ├── Controllers/
        ├── Views/
        ├── DAO/
        ├── Services/
        ├── wwwroot/
        └── docs/
```

## Configuração

### Banco de dados

1. Crie o banco e execute o script `PBL/PBL/Scripts_BD.sql` no SQL Server.
2. Ajuste a string de conexão em `PBL/PBL/DAO/ConexaoBD.cs` conforme seu ambiente (servidor, catálogo, usuário e senha).

Exemplo padrão no código:

```text
Data Source=LOCALHOST;Initial Catalog=PBL;user id=sa;password=123456
```

### Chave da IA (Gemini)

**Opção recomendada — arquivo `.env`** na pasta `PBL/`:

```env
GEMINI_API_KEY=sua_chave_aqui
```

O `Program.cs` carrega automaticamente `PBL/.env` (ou `.env` na pasta do projeto) ao iniciar a aplicação.

**Outras formas aceitas:**

- Variável de ambiente do sistema: `GEMINI_API_KEY` ou `GOOGLE_API_KEY`
- Variável no formato .NET: `FishAi__ApiKey`

O modelo usado pela IA está em `appsettings.json` (seção `FishAi`, padrão `gemini-2.5-flash`).

### Demais configurações

Edite `PBL/PBL/appsettings.json` para MQTT (`SmartLampMqtt`) e FIWARE/STH Comet (`FiwareSthComet`), se necessário.

## Execução

Na pasta do projeto web:

```powershell
cd PBL\PBL
dotnet restore
dotnet build
dotnet run --project PBL.csproj
```

URLs comuns (conforme `launchSettings.json`):

| Perfil        | URL                      |
|---------------|--------------------------|
| CadAlunoMVC   | http://localhost:5000    |
| IIS Express   | http://localhost:52891   |

A rota padrão abre a **tela de login**. Após autenticar, o usuário é redirecionado ao **Menu** (`/Home`).

## Fluxo de navegação

1. **Login** (`/Login`) — entrar ou cadastrar; sem acesso ao menu, aquários, peixes ou sobre sem sessão.
2. **Menu** (`/Home`) — página inicial após login, com catálogo de peixes e atalhos.
3. **Módulos autenticados** — Aquários, Peixes, Dashboard, Smart Lamp, Consultas, Sobre, API Swagger.
4. **Sair** — encerra a sessão e retorna ao login.

## Smart Lamp (MQTT + PWA)

- Configuração MQTT: `appsettings.json` → `SmartLampMqtt`
- PWA em `PBL/PBL/wwwroot/smartlamp-app/` — acesse com a app rodando: `/smartlamp-app/index.html`

## Cadastro de peixe com IA

No formulário de peixe, use **Detectar parâmetros pela imagem (IA)** após configurar `GEMINI_API_KEY`.

Requisitos: chave válida no ambiente e imagem enviada no cadastro/edição.

## API e testes

- **Swagger UI**: `/swagger`
- **Coleção Postman**: `PBL/PBL/postman_collection_min.json`
- **Exemplos**: `PBL/PBL/docs/api_demo.md`

## Documentação adicional

| Arquivo | Conteúdo |
|---------|----------|
| `PBL/PBL/docs/checklist.md` | Checklist de entrega PBL / evidências |
| `PBL/PBL/docs/security.md` | Boas práticas de segurança |
| `PBL/PBL/docs/api_demo.md` | Demonstração da API IoT |

## CI

Workflow de build em `PBL/PBL/.github/workflows/dotnet-ci.yml` (restore + build).

## Boas práticas

- Nunca commitar `.env`, senhas ou chaves de API.
- O arquivo `.env` está listado em `PBL/.gitignore`.
- Faça backup do banco antes de executar `Scripts_BD.sql`.

## Disciplina e equipe

Projeto PBL — **Linguagem de Programação I**, Fundação Salvador Arena (EC5). Integrantes e detalhes na tela **Sobre** da aplicação (`/Sobre`).
