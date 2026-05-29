# 📚 Índice de Documentação — Aquamarine

> Encontre o documento certo para sua necessidade

---

## 📋 Visão Geral de Documentos

```
┌─────────────────────────────────────────────────────┐
│              DOCUMENTAÇÃO AQUAMARINE                │
├─────────────────────────────────────────────────────┤
│                                                     │
│  👔 Gestores/Apresentações                          │
│  └─ EXECUTIVE_SUMMARY.md (1 página)                 │
│                                                     │
│  🚀 Iniciantes / Quick Start                        │
│  └─ QUICKSTART.md (5 minutos)                       │
│                                                     │
│  👤 Usuários Finais                                 │
│  └─ FEATURES.md (guia completo)                     │
│                                                     │
│  👨‍💻 Desenvolvedores                                │
│  ├─ ARCHITECTURE.md (fluxos + padrões)              │
│  └─ COMMUNICATION_MAP.md (diagramas)                │
│                                                     │
│  📱 IoT Developers                                  │
│  ├─ api_demo.md (exemplos de código)                │
│  └─ COMMUNICATION_MAP.md (seção API)                │
│                                                     │                          
│                                                     │
└─────────────────────────────────────────────────────┘
```

---

## 🎯 Qual documento eu devo ler?

### Cenário 1: Preciso apresentar o projeto em 2 minutos

**Leia:** [`EXECUTIVE_SUMMARY.md`](EXECUTIVE_SUMMARY.md)  
**Tempo:** 5 minutos  
**O que contém:**
- Elevator pitch (o que é Aquamarine)
- Stack técnico (visual)
- Features principais
- Fluxo visual
- Benefícios

---

### Cenário 2: Quero começar a usar AGORA

**Leia:** [`QUICKSTART.md`](QUICKSTART.md)  
**Tempo:** 5 minutos  
**O que contém:**
- Setup banco de dados
- Setup IA (Gemini)
- Rodar a app
- Primeiros passos (passo a passo)
- Troubleshooting

---

### Cenário 3: Quero saber TUDO que a app faz

**Leia:** [`FEATURES.md`](FEATURES.md)  
**Tempo:** 15 minutos  
**O que contém:**
- Descrição de cada módulo (com emojis)
- Como usar cada feature
- Fluxo típico do usuário
- FAQ (perguntas frequentes)
- Troubleshooting

---

### Cenário 4: Preciso entender como tudo funciona (arquitetura)

**Leia:** [`ARCHITECTURE.md`](ARCHITECTURE.md)  
**Tempo:** 20 minutos  
**O que contém:**
- Padrão de design (MVC + DAO)
- Camadas da aplicação (View → Controller → DAO → BD)
- Fluxos principais com sequência
  - Autenticação
  - Cadastro com IA
  - Smart Lamp MQTT
  - Dashboard IoT
- Comunicação entre componentes
- Integrações externas
- Estrutura de pastas explicada

---

### Cenário 5: Preciso ver os FLUXOS VISUAIS

**Leia:** [`COMMUNICATION_MAP.md`](COMMUNICATION_MAP.md)  
**Tempo:** 10 minutos  
**O que contém:**
- 7 fluxos principais com ASCII art:
  1. Autenticação
  2. Cadastro de Peixe com IA
  3. IoT — Sensor envia leitura
  4. Dashboard — Consulta histórico
  6. CRUD genérico
  7. Validação de compatibilidade
- Tabela de componentes
- Diagrama: pontos de integração
- Fluxo da sessão do usuário

**Leia:** [`api_demo.md`](api_demo.md)  
**O que contém:**
- Exemplos de cURL
- Código Arduino/ESP32 em C++
- Payload JSON esperado
- Respostas da API
- Tratamento de erros
**O que contém:**
- Inconsistências encontradas
### Cenário 6: Encontrei erro na documentação ou quer verificar coerência
---
```
│      └─→ QUICKSTART.md                 │
│  └─→ Se quer aprofundar:               │
│      ├─→ api_demo.md                   │
│  └─→ Referenciado por: FEATURES.md     │
│  └─→ Referenciado por: api_demo.md     │
└────────────────────────────────────────┘

┌─ api_demo.md ──────────────────────────┐
│  └─→ Referencia: COMMUNICATION_MAP.md  │
│  └─→ Pré-requisito: Conhecer REST API  │
│  └─→ Hardware: Arduino/ESP32           │
└────────────────────────────────────────┘

┌─ security.md ──────────────────────────┐
│  └─→ Referenciado por: ARCHITECTURE.md │
│  └─→ Aplicar após: QUICKSTART.md       │
│  └─→ Para produção: obrigatório        │
└────────────────────────────────────────┘
```

---

## 🔍 Tabela: Documento → Público

| Documento | Perfil | Tempo | Pré-requisitos |
| **EXECUTIVE_SUMMARY.md** | 👔 Gestores | 5 min | Nenhum |
| **QUICKSTART.md** | 🚀 Iniciantes | 5 min | SQL Server, .NET Core |
| **FEATURES.md** | 👤 Usuários | 15 min | Ter rodado QUICKSTART |
| **ARCHITECTURE.md** | 👨‍💻 Dev | 20 min | Conhecer MVC, SQL |
| **COMMUNICATION_MAP.md** | 🔍 Analista | 10 min | Nenhum |
| **COHERENCE_REPORT.md** | 🔎 QA/Verificação | 5 min | Ter lido outros docs |


## 📊 Documentação Técnica Adicional

### Project_Documentation.md
- **Conteúdo:** Documentação original do projeto (antiga)
- **Use se:** Precisa de contexto histórico

### REPORT_EC5.md

### Scripts_BD.sql
- **Conteúdo:** Schema SQL completo
### fiware-aquarios-mapeamento.md
- Conteudo: Mapeamento entre Aquamarine e FIWARE
- Use se: Vai integrar com FIWARE/STH-Comet

- **Use se:** Quer entender a estrutura do banco

---

## 🎓 Roteiros de Aprendizado

### Para Iniciantes (3 dias)

```
DIA 1 (2h):
├─ Ler: EXECUTIVE_SUMMARY.md (5 min)
├─ Executar: QUICKSTART.md (30 min)
└─ Explorar: FEATURES.md (1h 25 min)

DIA 2 (2h):
├─ Ler: COMMUNICATION_MAP.md (30 min)
├─ Ler: ARCHITECTURE.md (1h)
└─ Explorar: código do projeto (30 min)

DIA 3 (2h):
├─ Modificar: alguma feature pequena
├─ Testar: alterações
└─ Explorar: funcionalidades da app
```

### Para Desenvolvedores (1 semana)

```
SEGUNDA:
├─ QUICKSTART.md
├─ ARCHITECTURE.md
└─ Explorar: Controllers

TERÇA:
├─ Explorar: DAOs
└─ Entender: Stored Procedures

QUARTA:
├─ Explorar: Services (FishAiService, SmartLampMqttService)
└─ Testes: testar integração com API

QUINTA:

SEXTA:
├─ Testes completos
├─ Deploy: teste
└─ Documentação: adicionar comentários
```

### Para DevOps/Segurança (2-3 dias)

```
DIA 1:
├─ EXECUTIVE_SUMMARY.md
├─ QUICKSTART.md (setup)
└─ security.md (full read)

DIA 2:
├─ Análise: código contra OWASP Top 10
├─ Implementação: melhorias (BCrypt, HTTPS, etc)
└─ Teste: penetration testing

DIA 3:
├─ Deploy: produção
├─ Monitoramento: setup
└─ Documentação: runbook
```


## ❓ FAQ sobre Documentação

### P: Qual documento é oficial?

**R:** EXECUTIVE_SUMMARY.md + ARCHITECTURE.md + FEATURES.md = documentação oficial da v1.0.

---

### P: Posso imprimir algum?

**R:** Sim! Estes ficam bons em papel:
- **EXECUTIVE_SUMMARY.md** (1 página, ideal para apresentar)
- **QUICKSTART.md** (2-3 páginas, checklist rápido)
- **COMMUNICATION_MAP.md** (5-6 páginas, fluxos visuais)

---

### P: Está desatualizado?

**R:** Todos os docs foram gerados **Mai/2026**. Se encontrar erro:
1. Verifique com a versão atual da app
2. Reporte issue (se houver repositório)
3. Atualize você mesmo (é open-source!)

---

### P: Em qual ordem devo ler?

**R:** Depende do seu objetivo:

---

### P: Qual documento descreve o banco de dados?

**R:** Vários:
- **Schema visual**: ARCHITECTURE.md (seção "Camadas")
- **Queries SQL**: Scripts_BD.sql
- **ER Diagram**: (não temos, mas ARCHITECTURE.md descreve)

---

### P: Como funciona a IA?

**R:** Veja:
- **Rápido**: FEATURES.md (seção "🐠 Peixes")
- **Detalhado**: ARCHITECTURE.md (seção "Fluxo 2: Cadastro com IA")
- **Código**: FishAiService.cs (comentado)

---

### P: Como faço a integração IoT?

**R:** Veja:
- **Start**: api_demo.md (exemplos)
- **Fluxo**: COMMUNICATION_MAP.md (seção "IoT — Dispositivo envia leitura")
- **Validação**: QUICKSTART.md (seção "Enviar Dados de Sensores")

---

### P: Onde estão os comentários do código?

**R:** No código-fonte mesmo:
- Controllers: `/PBL/PBL/Controllers/`
- DAOs: `/PBL/PBL/DAO/`
- Services: `/PBL/PBL/Services/`
- Views: `/PBL/PBL/Views/`

Todos têm comentários em português explicando a lógica.

---

## 📞 Suporte

Se tiver dúvidas que não estão nesta documentação:

1. **Procure em:** README.md ou cada .md específico
2. **Pergunte:** Community (se houver)
3. **Contribua:** Melhorias na docs = Pull Request bem-vindo!

---

*Índice de Documentação — Aquamarine*  
*Uma página para reinar sobre elas*  
*Mai/2026*
