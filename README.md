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

## 🚀 Como Começar?

> ⚠️ **Se está perdido**, leia [`INDEX.md`](PBL/PBL/docs/INDEX.md) primeiro — índice completo com recomendações personalizadas!

Escolha seu caminho conforme seu perfil:

| Perfil | Comece por |
|--------|-----------|| 📋 **Só quer entender rápido** | [`EXECUTIVE_SUMMARY.md`](PBL/PBL/docs/EXECUTIVE_SUMMARY.md) (1 página) || 🎯 **Quero usar agora** | [`QUICKSTART.md`](PBL/PBL/docs/QUICKSTART.md) (5 min) |
| 👤 **Quero conhecer as funcionalidades** | [`FEATURES.md`](PBL/PBL/docs/FEATURES.md) (guia prático) |
| 👨‍💻 **Quero entender como funciona** | [`ARCHITECTURE.md`](PBL/PBL/docs/ARCHITECTURE.md) (fluxos técnicos) |
| � **Quero ver os fluxos de comunicação** | [`COMMUNICATION_MAP.md`](PBL/PBL/docs/COMMUNICATION_MAP.md) (visual) |

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

## 🏗️ Arquitetura do Projeto

O Aquamarine segue o padrão **MVC + DAO + Services**:

```
👤 Usuário
    ↓
🌐 View (Razor HTML)
    ↓
🎮 Controller (validações, lógica)
    ↓
⚙️ Services (IA, MQTT, etc)
    ↓
💾 DAO (Stored Procedures)
    ↓
🗄️ SQL Server
```

### Principais fluxos

- **Cadastro com IA**: Form → Controller → Google Gemini → Preenche campos automaticamente
- **Smart Lamp**: User controla brilho → MQTT → ESP32 → LED muda em tempo real
- **Leituras IoT**: Sensores enviam → API REST → BD → Dashboard mostra gráficos
- **CRUDs**: Padrão genérico com `PadraoDAO` + Stored Procedures (reduz duplicação)

**Quer entender a comunicação completa?** Leia [`ARCHITECTURE.md`](PBL/PBL/docs/ARCHITECTURE.md).

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


## Documentação adicional

Explore a documentação completa sobre como o projeto funciona:

| Arquivo | Para quem | Conteúdo |
|---------|-----------|---------|
| `PBL/PBL/docs/INDEX.md` | 🗂️ Começando | **Comece aqui!** Índice completo com recomendações |
| `PBL/PBL/docs/EXECUTIVE_SUMMARY.md` | 👔 Gestores/Apresentações | Uma página impressível com o resumo executivo |
| `PBL/PBL/docs/QUICKSTART.md` | 🚀 Iniciantes | Começar em 5 minutos |
| `PBL/PBL/docs/FEATURES.md` | 👤 Usuários | Como usar cada funcionalidade |
| `PBL/PBL/docs/ARCHITECTURE.md` | 👨‍💻 Desenvolvedores | Fluxos de comunicação e arquitetura |
| `PBL/PBL/docs/COMMUNICATION_MAP.md` | 🔍 Analistas | Mapa visual de todos os fluxos |
| `PBL/PBL/docs/COHERENCE_REPORT.md` | 🔎 QA/Verificação | Validação de coerência doc ↔ código |
| `PBL/PBL/docs/fiware-aquarios-mapeamento.md` | 🌐 FIWARE | Integração com plataforma FIWARE |

## CI

Workflow de build em `PBL/PBL/.github/workflows/dotnet-ci.yml` (restore + build).

## Boas práticas

- Nunca commitar `.env`, senhas ou chaves de API.
- O arquivo `.env` está listado em `PBL/.gitignore`.
- Faça backup do banco antes de executar `Scripts_BD.sql`.

## Disciplina e equipe

Projeto PBL — **Linguagem de Programação I**, Fundação Salvador Arena (EC5). Integrantes e detalhes na tela **Sobre** da aplicação (`/Sobre`).
