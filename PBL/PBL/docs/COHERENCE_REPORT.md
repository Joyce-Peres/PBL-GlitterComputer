# Relatório de Coerência — Documentação vs. Código

> Verificação de 29/05/2026 — Alinhamento entre documentação e implementação

## ✅ O que está CORRETO e coerente

| Elemento | Documentação | Código | Status |
|----------|-------------|--------|--------|
| **Stack Técnico** | ASP.NET Core 3.1 MVC | ✓ Program.cs, Startup.cs | ✅ Correto |
| **Banco de Dados** | SQL Server com Stored Procedures | ✓ Scripts_BD.sql | ✅ Correto |
| **DAO Pattern** | Herança genérica PadraoDAO<T> | ✓ PadraoDAO.cs | ✅ Correto |
| **Services** | FishAiService, SmartLampMqttService, FiwareSthCometService | ✓ Em Services/ | ✅ Correto |
| **Controllers** | Login, Home, Aquario, Peixe, SmartLamp, Consulta, Dashboard, Sobre | ✓ Em Controllers/ | ✅ Correto |
| **API REST** | GET/POST /api/leituras | ✓ LeiturasController.cs | ✅ Correto |
| **Swagger** | /swagger endpoint | ✓ Startup.cs UseSwagger | ✅ Correto |
| **IA - Imagem** | DetectarParametros (foto) | ✓ PeixeController | ✅ Correto |
| **IA - Espécie** | DetectarParametrosPorEspecie | ✓ PeixeController | ✅ Correto |
| **MQTT** | SmartLampMqttService com TCP puro | ✓ SmartLampMqttService.cs | ✅ Correto |
| **Autenticação** | Session-based | ✓ Program.cs AddSession | ✅ Correto |
| **Upload Foto** | wwwroot/uploads/peixes/ | ✓ PeixeController.SalvarFoto | ✅ Correto |
| **Modelos** | ViewModel pattern | ✓ Models/*.cs | ✅ Correto |

---

## ❌ INCONSISTÊNCIAS ENCONTRADAS

### 1. **Número de Tabelas do Banco** ⚠️ CRÍTICO

**Problema:**
- 📝 Documentação diz: **"11 tabelas + Stored Procedures"** (EXECUTIVE_SUMMARY.md:52)
- 🗄️ Código real: **Apenas 5 tabelas**

**Tabelas reais:**
1. `Usuarios`
2. `Aquarios`
3. `Peixes`
4. `LampConfigs`
5. `LeiturasSensor`

**Raiz do problema:** A documentação provavelmente contava Stored Procedures como "tabelas" ou foi uma estimativa incorreta inicial.

**Impacto:** ⚠️ MÉDIO — Confunde quem ler a EXECUTIVE_SUMMARY

**Arquivos afetados:**
- `docs/EXECUTIVE_SUMMARY.md` (linha 52)

**Ação recomendada:** Corrigir para "5 tabelas + 13 Stored Procedures"

---

### 2. **Tabela "FishParameters"** ⚠️ ALTO

**Problema:**
- 📝 Documentação diz: Existe uma **subtabela `FishParameters`** separada
  - FEATURES.md (linha 194): "SubTabela: `FishParameters` (guarda os 13 parâmetros ambientais)"
  - COMMUNICATION_MAP.md (linha 136): "INSERT INTO FishParameters ..."
- 🗄️ Código real: **Não existe tabela FishParameters**
  - Todos os 13 parâmetros estão como **colunas em `Peixes`**
  - Existe `FishParametersViewModel` (apenas um model C#, não tabela)

**Parâmetros na tabela Peixes:**
```
temperaturaIdeal, temperaturaMin, temperaturaMax
luminosidadeIdeal, luminosidadeMin, luminosidadeMax
tdsPpmMin, tdsPpmMax
salinidadePptMin, salinidadePptMax
volumeMinLitros
originFromAI
parametersUpdatedAt
```

**Raiz do problema:** A documentação foi escrita com a intenção de ter uma tabela separada, mas a implementação optou por desnormalização (todas as colunas em Peixes).

**Impacto:** ⚠️ ALTO — Desenvolvedores esperando tabela separada encontrarão colunas em Peixes

**Arquivos afetados:**
- `docs/FEATURES.md` (linha 194)
- `docs/COMMUNICATION_MAP.md` (linha 136)
- `docs/ARCHITECTURE.md` (possível menção)

**Ação recomendada:**
- Remover menção a "SubTabela FishParameters"
- Explicar que parâmetros estão como colunas desnormalizadas em Peixes
- Atualizar COMMUNICATION_MAP.md para mostrar "INSERT INTO Peixes" em vez de dois INSERT

---

### 3. **Menção a "Catálogo de Peixes" no Menu** ⚠️ BAIXO

**Problema:**
- 📝 FEATURES.md (linha 48): Menu tem item "🐟 **Catálogo de Peixes**"
- ❓ Não está claro se isso é uma rota separada ou se é "Consulta → Peixes"

**Verificação necessária:** Confirmar se existe rota `/Peixe` (listar) ou se é apenas via Consulta

**Arquivos afetados:**
- `docs/FEATURES.md` (linha 48)

---

## 🔍 Verificações Complementares (OK)

| Item | Verificação | Resultado |
|------|-------------|-----------|
| Controllers existem | Aquario, Peixe, Login, SmartLamp, Consulta, Dashboard, Home, Sobre, Api/Leituras | ✅ Todos presentes |
| DAOs existem | AquarioDAO, PeixeDAO, LeituraSensorDAO, UsuarioDAO, SmartLampConfigDAO | ✅ Todos presentes |
| Pastas Views | Aquario/, Peixe/, SmartLamp/, Dashboard/, Login/, Consulta/, Shared/, Home/, Sobre/ | ✅ Todas presentes |
| Services funcionam | FishAiService.AnalisarImagemAsync, AnalisarEspecieAsync | ✅ Implementados |
| Tabela Usuarios | id, nome, login, senha | ✅ Conforme descrito |
| Tabela Aquarios | id, nome, capacidadeLitros, tipoAgua, usuarioId | ✅ Conforme descrito |
| Stored Procedures genéricas | spListagem, spConsulta, spDelete, spProximoId | ✅ Implementadas |
| MQTT config | BrokerHost, BrokerPort, TopicCmd | ✅ Em appsettings.json |
| .env loading | Program.cs carrega .env | ✅ Implementado |

---

## 📊 Resumo de Ações Necessárias

| Prioridade | Arquivo | Linha | Correção |
|-----------|---------|-------|----------|
| 🔴 ALTO | COMMUNICATION_MAP.md | 136 | Remover "INSERT INTO FishParameters" |
| 🔴 ALTO | FEATURES.md | 194 | Remover menção a "SubTabela FishParameters" |
| 🟡 MÉDIO | EXECUTIVE_SUMMARY.md | 52 | Mudar "11 tabelas" para "5 tabelas + 13 Stored Procedures" |
| 🟡 MÉDIO | ARCHITECTURE.md | ??? | Verificar menções a FishParameters |
| 🟢 BAIXO | FEATURES.md | 48 | Clarificar se "Catálogo de Peixes" é rota separada |

---

## 💡 Recomendações de Melhoria Estrutural

### Sobre a desnormalização (parâmetros em Peixes)

**Discussão:** Ter todos os parâmetros como colunas em Peixes é válido, mas criar uma tabela separada teria benefícios:

✅ **Vantagens da desnormalização atual (mantida):**
- Simples, sem JOIN
- Performance melhor para leitura
- Menos código DAO

⚠️ **Desvantagens:**
- Tabela `Peixes` fica muito larga (20+ colunas)
- Se adicionar mais parâmetros (pH, CO2, etc), cresce mais

**Sugestão:** Se for adicionar mais sensores no futuro, considere:
```sql
CREATE TABLE FishParameters (
    id INT PRIMARY KEY IDENTITY,
    peixeId INT UNIQUE,
    temperaturaIdeal DECIMAL(5,2),
    -- ... outros 12 parâmetros
    FOREIGN KEY (peixeId) REFERENCES Peixes(id)
);
```

Mas **não é urgente** pra versão atual.

---

## ✅ Próximas Etapas

1. **Imediato** (hoje): Corrigir COMMUNICATION_MAP.md e FEATURES.md
2. **Hoje**: Corrigir EXECUTIVE_SUMMARY.md (contagem de tabelas)
3. **Verificar**: ARCHITECTURE.md por menções a FishParameters
4. **Confirmar**: Se há rota separada para "Catálogo de Peixes"

---

**Gerado em:** 29/05/2026  
**Verificação:** Comparação documentação ↔ Scripts_BD.sql + Controllers + Services + Models

