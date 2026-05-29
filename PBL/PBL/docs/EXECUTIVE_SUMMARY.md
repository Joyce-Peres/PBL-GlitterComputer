# Executive Summary — Aquamarine

> Uma página (impressível) sobre o que é e como funciona

---

## 🎯 O que é?

**Aquamarine** é um sistema web para **gerenciar aquários com IA**.

- **Cadastra aquários** e os peixes dentro deles
- **Usa IA** (Google Gemini) para detectar espécie e parâmetros ideais por foto
- **Controla lâmpadas inteligentes** em tempo real via MQTT/IoT
- **Monitora sensores** (temperatura, nível de água, TDS)
- **Exibe dashboards** com gráficos de histórico
- **API REST** para integração com dispositivos

---

## 👥 Para quem?

| Persona | Caso de uso |
|---------|-----------|
| 🏠 **Aquarista doméstico** | Gerenciar múltiplos aquários e peixes da casa |
| 🏢 **Loja/Produção de peixes** | Monitorar aquários em escala, otimizar iluminação |
| 🎓 **Projeto educacional** | Aprender IoT + IA + MVC + BD + MQTT |
| 🔬 **Pesquisa** | Coletar dados de comportamento e parâmetros aquáticos |

---

## 📊 Stack Técnico

```
┌─────────────────────────────────────────┐
│ Frontend                                │
│ • Razor Views (.cshtml)                 │
│ • HTML5 + CSS3 + Bootstrap              │
│ • JavaScript vanilla + Chart.js         │
│ • PWA para Smart Lamp                   │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ Backend                                 │
│ • ASP.NET Core 3.1 (MVC)                │
│ • Padrão: MVC + DAO + Services          │
│ • C# + .NET                             │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ Banco de Dados                          │
│ • SQL Server                            │
│ • 5 tabelas + 13 Stored Procedures      │
│ • Relacionamentos: Usuários → Aquários  │
│                     Aquários → Peixes   │
└─────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────┐
│ Integrações Externas                    │
│ • 🧠 Google Gemini API (IA)             │
│ • 📡 MQTT Broker (IoT control)          │
│ • 📊 FIWARE STH-Comet (histórico)       │
│ • 🔒 Session-based auth                 │
└─────────────────────────────────────────┘
```

---

## ⚡ Principais Features

### 1. 🔐 Autenticação
- Login/Cadastro simples
- Dados isolados por usuário
- Session-based (seguro para intranet/LAN)

### 2. 🏊 Gestão de Aquários
- CRUD (Create, Read, Update, Delete)
- Controle de capacidade e tipo de água
- Isolamento por usuário

### 3. 🐠 Gestão de Peixes
- Cadastro com foto (opcional)
- **IA detecta**: espécie, brilho ideal, temperatura, salinidade
- Validação de compatibilidade com aquário
- Ligação automática com Smart Lamp

### 4. 💡 Smart Lamp (IoT)
- Controle de brilho (0-100%)
- Cores RGB customizáveis
- Automático baseado em espécie do peixe
- Comunicação via MQTT (tempo real < 100ms)

### 5. 📊 Dashboard
- Gráficos com Chart.js
- Filtro por aquário e período
- Múltiplas métricas simultâneas
- Suporta até 1 ano de histórico

### 6. 📡 API IoT
- Endpoint: `POST /api/leituras`
- Aceita: temperatura, nível de água, TDS, etc
- Responde em JSON
- Para Arduino/ESP32 enviar dados

### 7. 🔍 Consultas
- Filtrar peixes por aquário
- Filtrar leituras por período
- Busca full-text

### 8. ℹ️ Menu
- Dashboard dos peixes
- Links rápidos às features
- Informações do projeto

---

## 🔄 Fluxo Principal

```
user@home → 🌐 localhost:5000
         ↓
    [Login]
         ↓
    [Menu]
         ↓
    [Novo Aquário] ─→ [BD]
         ↓
    [Novo Peixe]
         ├─→ [Upload Foto]
         ├─→ [IA Gemini] ──→ parâmetros
         ├─→ [BD]
         └─→ [MQTT] ──→ 💡 Smart Lamp
         ↓
    [Dashboard] ← ESP32 envia dados → [BD] ← [Gráficos]
         ↓
    [Consultas] ← filtra dados
         ↓
    [Sair] ─→ [Login]
```

---

## 📈 Benefícios

| Aspecto | Benefício |
|--------|-----------|
| **Tempo** | Setup em 5 minutos, operação intuitiva |
| **Custo** | Código aberto, APIs free tier disponíveis |
| **Escalabilidade** | Suporta múltiplos aquários/peixes/usuários |
| **Inteligência** | IA automática para parâmetros ideais |
| **Real-time** | Controle Smart Lamp com latência < 100ms |
| **Educação** | Aprender full-stack (frontend, backend, BD, IoT, IA) |

---

## 🛠️ Arquitetura (Resumido)

**Design Pattern**: MVC + DAO (Data Access Object)

```
┌─────────────────────────────┐
│ View (Razor)                │
│ ↓ user submits form         │
├─────────────────────────────┤
│ Controller                  │
│ • valida input              │
│ • chama service se preciso  │
│ ↓ passa dados               │
├─────────────────────────────┤
│ Service (opcional)          │
│ • IA (Gemini)               │
│ • MQTT (Smart Lamp)         │
│ ↓ retorna resultado         │
├─────────────────────────────┤
│ DAO (Data Access Object)    │
│ • parâmetros SQL            │
│ • executa Stored Proc       │
│ ↓ retorna dados             │
├─────────────────────────────┤
│ SQL Server                  │
│ • persiste dados            │
└─────────────────────────────┘
```

**Vantagem**: Separação clara de responsabilidades, reutilização de código, segurança (SQL injection prevention).

---

## 🚀 Como Começar (30 segundos)

1. Clone o repo / abra a pasta
2. Execute `PBL\PBL\Scripts_BD.sql` no SQL Server
3. Crie `.env` com `GEMINI_API_KEY` (opcional)
4. `cd PBL\PBL && dotnet run`
5. Acesse `http://localhost:5000`

**Pronto!** Faça login → Crie um aquário → Adicione um peixe → Veja o dashboard.

---

## 📊 Dados do Projeto

| Métrica | Valor |
|---------|-------|
| **Linguagem** | C# (.NET Core 3.1) |
| **Arquivos de código** | ~25 (Controllers, DAOs, Views, Services) |
| **Linhas de código** | ~3000 (sem comentários) |
| **Tabelas BD** | 5 principais + 13 Stored Procedures |
| **APIs externas** | 3 (Gemini, MQTT, STH-Comet) |
| **Features principales** | 8 módulos |
| **Usuários suportados** | ∞ (isolados por sessão) |
| **Tempo de desenvolvimento** | PBL - 4 meses (equipe) |

---

## 🔐 Segurança

- ✅ SQL injection prevention (SqlParameter)
- ✅ XSS prevention (HtmlEncode)
- ✅ User isolation (SessionId)
- ⚠️ HTTPS (recomendado em produção)
- ⚠️ Passwords em plaintext (usar BCrypt!)
- ⚠️ Rate limiting (implementar)
- ⚠️ CORS (configurar conforme necessário)

---

## 📞 Contato / Próximos Passos

### Para aprender mais:

| Quer | Leia |
|------|------|
| **Começar em 5 min** | `QUICKSTART.md` |
| **Ver os fluxos de comunicação** | `COMMUNICATION_MAP.md` |
| **Entender a arquitetura** | `ARCHITECTURE.md` |
| **Usar todas as features** | `FEATURES.md` |
| **Integrar IoT** | `api_demo.md` |
| **Melhorar segurança** | `security.md` |

### Melhorias futuras:

1. [ ] Usar BCrypt para senhas
2. [ ] Implementar JWT API auth
3. [ ] HTTPS obrigatório
4. [ ] Histórico com Entity Framework
5. [ ] Alertas via email/SMS
6. [ ] Multi-idioma (i18n)
7. [ ] Testes unitários + E2E
8. [ ] Deploy em Docker/K8s

---

## 📄 Licença

Este projeto é um **PBL (Projeto Baseado em Aprendizagem)** para fins educacionais.

---

**Aquamarine — Smart Aquarium Management System**  
*Versão 1.0 — Mai/2026*  
*Para aprender, inovar, compartilhar*
