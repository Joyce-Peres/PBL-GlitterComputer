# Guia de Funcionalidades — Aquamarine

> Como usar cada recurso do sistema, explicado de forma prática

## 📚 Índice

1. [Autenticação (Login/Cadastro)](#-autenticaçãologincadastro)
2. [Menu Principal](#-menu-principal)
3. [Gerenciar Aquários](#-gerenciar-aquários)
4. [Gerenciar Peixes](#-gerenciar-peixes--ia)
5. [Dashboard de Leituras](#-dashboard-de-leituras)
6. [Smart Lamp](#-smart-lamp)
7. [Consultas](#-consultas)
8. [API IoT](#-api-iot)

---

## 🔐 Autenticação (Login/Cadastro)

### O que faz?

Controla quem acessa o sistema. Cada usuário tem sua própria conta e seus aquários/peixes são isolados.

### Como usar?

#### Primeira vez: Cadastro

1. Acesse `http://localhost:5000` (ou a porta configurada)
2. Clique na aba **"Cadastrar"**
3. Preencha:
   - **Nome**: seu nome completo
   - **Login**: username único (ex: `joao_silva`)
   - **Senha**: escolha uma (⚠️ guardada em texto plano no BD por enquanto)
4. Clique "Cadastrar"
5. Você é redirecionado pro login

#### Login normal

1. Preencha **Login** e **Senha**
2. Clique "Entrar"
3. Se correto, você vê o **Menu Principal**
4. Se errado, mensagem de erro aparece

#### Sair

- Qualquer página tem botão "Sair" no topo
- Clique e sessão é encerrada
- Você volta pro login

### Detalhes técnicos

- Sessão é armazenada em memória (não é seguro pra produção)
- Senhas em texto plano (é fraco de segurança, veja em `docs/security.md`)
- Tabela: `Usuarios` (id, nome, login, senha)

---

## 🏠 Menu Principal

### O que faz?

Ponto de entrada após login. Mostra atalhos pra tudo que você pode fazer.

### Funcionalidades do menu

| Item | O que faz |
|------|-----------|
| 🐟 **Catálogo de Peixes** | Lista TODOS os peixes (seus + dos outros users) |
| 🏊 **Novo Aquário** | Cria um aquário novo |
| 📊 **Dashboard IoT** | Vê gráficos de leituras dos sensores |
| 💡 **Smart Lamp** | Controla brilho/cor da lâmpada |
| 🔍 **Consultas** | Filtra peixes e leituras |
| ℹ️ **Sobre** | Info do projeto e equipe |
| 📡 **API Swagger** | Abre documentação interativa da API |

### Banner especial

Topo da página tem um banner informando que peixes podem ter **recomendação automática por IA**.

---

## 🏊 Gerenciar Aquários

### O que faz?

Cadastra aquários (tanques) e associa peixes a eles. Cada aquário:
- Tem uma **capacidade em litros**
- Tem um **tipo de água** (Doce, Salgada, Mista)
- Pertence a **um usuário**

### Criar aquário

1. Clique em "Novo Aquário" no menu
2. Preencha:
   - **Nome**: ex "Aquário da Sala"
   - **Capacidade (L)**: ex 50
   - **Tipo de Água**: Doce / Salgada / Mista
3. Clique "Salvar"
4. Pronto! Aquário criado

### Editar aquário

1. Vá em "Consulta de Peixes"
2. Na listagem, clique no ícone **editar** (lápis) da linha do aquário
3. Mude os dados
4. Clique "Salvar"

### Deletar aquário

⚠️ **Atenção**: deleta também TODOS os peixes associados!

1. Vá em "Consulta de Peixes"
2. Clique no ícone **delete** (X) da linha do aquário
3. Confirme na caixa de diálogo
4. Aquário deletado com todos seus peixes

### Detalhes técnicos

- Tabela: `Aquarios` (id, nome, capacidadeLitros, tipoAgua, usuarioId)
- User logado é automaticamente o proprietário
- User não consegue criar aquário pra outro user

---

## 🐠 Gerenciar Peixes + IA

### O que faz?

Cadastra peixes com foto e parâmetros de saúde. A IA pode **sugerir automaticamente** os parâmetros ideais a partir da foto ou nome da espécie.

### Criar peixe

1. Clique em "Novo Peixe" no menu (ou via "Consulta de Peixes")
2. Preencha os **dados básicos**:
   - **Nome**: ex "Nemo"
   - **Espécie**: ex "Amphiprion ocellaris"
   - **Tamanho (cm)**: ex 8
   - **Aquário**: selecione um da lista

3. _(Opcional)_ Upload de **foto**
   - Clique em "Escolher arquivo"
   - Selecione JPG/PNG

4. _(Opcional)_ Use **IA pra detectar automaticamente**

   **Opção A: Foto**
   - Upload foto do peixe
   - Clique "Detectar parâmetros pela imagem (IA)"
   - Aguarde (2-3 seg)
   - Campos de temperatura, luminosidade, TDS, salinidade são preenchidos
   - Um badge "🧠 Gerado pela IA" aparece

   **Opção B: Só o nome da espécie**
   - Digite o nome da espécie
   - Clique "Detectar pela espécie (IA)"
   - Aguarde
   - Campos preenchidos

5. _(Opcional)_ Revise e ajuste os **parâmetros ambientais** na seção "Avançado":
   - **Temperatura (°C)**: min, ideal, max
   - **Luminosidade (0-100)**: min, ideal, max
   - **TDS (ppm)**: min, max
   - **Salinidade (ppt)**: min, max
   - **Volume mínimo (L)**: espaço que o peixe precisa

6. Clique "Salvar"

### Validações ao salvar

Se algo está errado, o sistema **avisa**:

- ❌ Peixe precisa de 100L mas aquário tem 50L → Pergunta se quer continuar
- ❌ Peixe é marinho (sal > 0.5 ppt) mas aquário é doce → Pergunta se quer continuar
- ❌ Temperatura mínima > ideal → Erro, precisa corrigir

### Editar peixe

1. Vá em "Consulta de Peixes"
2. Clique no ícone **editar** (lápis)
3. Mude o que precisar (foto, parâmetros, aquário, etc)
4. Pode rodar IA de novo pra atualizar parâmetros
5. Clique "Salvar"

### Deletar peixe

1. Vá em "Consulta de Peixes"
2. Clique no ícone **delete** (X)
3. Confirme
4. Peixe deletado

### Detalhes técnicos

- Tabela: `Peixes` (id, nome, especie, foto, e 13 colunas de parâmetros)
- Colunas de parâmetros: temperaturaIdeal/Min/Max, luminosidadeIdeal/Min/Max, tdsPpmMin/Max, salinidadePptMin/Max, volumeMinLitros
- Campo `OriginFromAI`: marca se veio da IA ou foi manual
- Campo `ParametersUpdatedAt`: timestamp da última atualização
- Foto salva em: `wwwroot/uploads/peixes/`

### Como a IA funciona?

```
1. Photo é salva temporariamente
2. Enviada ao Google Gemini via API
3. Gemini analisa e retorna JSON com parâmetros
4. Sistema converte JSON em objetos C#
5. Foto temporária é deletada
6. Dados mostrados no form pra review do user
```

**Por que precisa de chave Gemini?**
- IA é um serviço externo (Google)
- Você controla a chave via `.env` ou variável de ambiente
- Sem chave, botões de IA ficam desabilitados

---

## 📊 Dashboard de Leituras

### O que faz?

Mostra gráficos em tempo real de sensores do aquário:
- **Temperatura** da água
- **Nível de água**
- **TDS** (qualidade elétrica)
- **Luminosidade (LDR)**
- **Salinidade** (se aplicável)

### Como usar?

1. Clique "Dashboard IoT" no menu
2. Selecione um **aquário** no combo
3. Selecione um **período** (últimas 24h, 7 dias, etc)
4. Clique "Filtrar"
5. Gráficos aparecem com dados do período

### Fonte dos dados

```
Dados podem vir de 2 lugares (fallback automático):

1. STH-Comet (FIWARE) — se tá configurado
   → Dados vêm de um servidor MongoDB em nuvem
   → Mais histórico, mais integrado com IoT

2. SQL Server local — fallback
   → Dados que você mesmo capturou/simulou
   → Local, rápido, sem dependência externa
```

### Adicionar dados manualmente

Se quer simular sensores ou testar, use a **API**:

```bash
POST http://localhost:5000/api/leituras
Content-Type: application/json

{
  "aquarioId": 1,
  "temperatura": 25.5,
  "nivelAgua": 85,
  "tdsPpm": 120,
  "salinidadePpt": 1.023,
  "qualidadeTds": "Boa"
}
```

Sucesso: Leitura aparece em 1-2 segundos no dashboard.

### Detalhes técnicos

- Tabela: `LeiturasSensor` (id, aquarioId, temperatura, nivelAgua, ...)
- Integração: `FiwareSthCometService` (se configurado)
- Gráficos: Chart.js com AJAX refresh a cada 5 seg

---

## 💡 Smart Lamp

### O que faz?

Controla uma lâmpada LED RGB conectada via MQTT (protocolo IoT).

- Muda **brilho** (0-100%)
- Muda **modo** (Desligada, Fraca, Média, Forte, Personalizado)
- Muda **cor RGB** (personalizado)
- Mostra **histórico de luminosidade** do aquário

### Como usar?

1. Clique "Smart Lamp" no menu
2. Selecione um **aquário** (geralmente o padrão é carregado)
3. Escolha um **modo**:
   - 0 - Desligada
   - 1 - Fraca (20% brilho)
   - 2 - Média (50% brilho)
   - 3 - Forte (100% brilho)
   - 4 - Personalizado (você controla brilho e RGB)

4. Se escolher **Personalizado**:
   - Deslize o **brilho** (0-100%)
   - Selecione a **cor** (RGB)
   - Defina **alvo de brilho automático** (opcional)
   - Defina **alvo de temperatura automático** (opcional)

5. Clique "Salvar"
6. Se o chip ESP32 tá conectado, LED muda **imediatamente**

### Integração com Peixes

Quando você **cadastra um peixe com parâmetros ideais**:

```
Peixe tem Luminosidade Ideal = 40%
    ↓
Ao salvar peixe, sistema chama:
    SmartLampMqttService.AplicarBrilhoAsync(40)
    ↓
Mensagem MQTT é enviada pro ESP32:
    { "brilho": 40 }
    ↓
LED da lâmpada MUDA PRA 40% automaticamente
```

Prático! User não precisa mexer manualmente pra cada peixe.

### Histórico de luminosidade

- Gráfico mostra últimas leituras do **LDR (sensor de luz)** do aquário
- Atualiza a cada 5 segundos se houver dados novos
- Fora do período selecionado, mostra "sem leituras"

### Detalhes técnicos

- Tabela: `LampConfigs` (aquarioId, modo, brilho, r, g, b, luzAlvo, tempAlvo)
- Protocolo: **MQTT** (tópicos em `/TEF/lamp*/cmd` e `/TEF/lamp*/status`)
- Service: `SmartLampMqttService`
- Hardware esperado: ESP32 ou Arduino com WiFi + LED RGB

---

## 🔍 Consultas

### Consulta de Peixes

Filtra e lista peixes com opções de busca:

1. Clique "Consultas" → "Peixes" no menu
2. Filtros disponíveis:
   - **Aquário**: selecione um
   - **Nome**: busca por nome do peixe
   - **Espécie**: busca por espécie
3. Clique "Filtrar"
4. Tabela mostra:
   - ID, Nome, Espécie, Tamanho
   - Temperatura/Luminosidade/TDS/Salinidade (ideais)
   - Volume mínimo
   - Aquário
   - Foto
   - Ícones: Editar, Deletar

#### Cards informativos

No topo da página:

| Card | Mostra |
|------|--------|
| Com Foto | Quantos peixes tem foto |
| Com Parâmetros Completos | Quantos peixes têm todos os 13 parâmetros |
| Detectados pela IA | Quantos vieram de análise de imagem/espécie |
| Com Parâmetros Manuais | Quantos foram digitados à mão |

### Consulta de Leituras

Filtra leituras de sensores com período e filtros:

1. Clique "Consultas" → "Leituras" no menu
2. Selecione:
   - **Aquário**: qual aquário quer ver
   - **Período**: data início e fim
   - **Parâmetro**: temperatura, TDS, etc (opcional)
3. Clique "Filtrar"
4. Tabela mostra todas as leituras do período

**Fonte dos dados**: Se STH-Comet tá up, busca lá. Senão, usa BD local.

### Detalhes técnicos

- Controller: `ConsultaController`
- Views: `Consulta/Peixes.cshtml`, `Consulta/Leituras.cshtml`
- DAOs: `PeixeDAO.ConsultaPeixesFiltro()`, `LeituraSensorDAO.ConsultaLeiturasFiltro()`

---

## 📡 API IoT

### O que faz?

Endpoints REST pra dispositivos IoT (ESP32, Arduino, etc) enviarem e consultarem leituras.

### Endpoints principais

#### 1️⃣ POST `/api/leituras` — Registrar leitura

Dispositivo IoT envia dados de sensores:

```http
POST /api/leituras
Content-Type: application/json

{
  "aquarioId": 1,
  "temperatura": 25.5,
  "nivelAgua": 85.0,
  "tdsPpm": 120.0,
  "salinidadePpt": 0.350,
  "qualidadeTds": "Boa"
}
```

**Resposta (sucesso)**:
```json
{
  "sucesso": true,
  "mensagem": "Leitura registrada"
}
```

**Resposta (erro)**:
```json
{
  "sucesso": false,
  "mensagem": "aquarioId é obrigatório"
}
```

#### 2️⃣ GET `/api/leituras` — Listar leituras

Busca histórico com filtros:

```http
GET /api/leituras?aquarioId=1&dataInicio=2026-05-01&dataFim=2026-05-29
```

**Resposta**:
```json
[
  {
    "id": 1,
    "aquarioId": 1,
    "temperatura": 25.5,
    "nivelAgua": 85.0,
    "tdsPpm": 120.0,
    "dataLeitura": "2026-05-29T14:30:00"
  },
  ...
]
```

#### 3️⃣ GET `/api/leituras/aquario/{aquarioId}` — Leituras de um aquário

Busca só de um aquário específico:

```http
GET /api/leituras/aquario/1
```

### Exemplo com cURL

```bash
# Enviar leitura
curl -X POST http://localhost:5000/api/leituras \
  -H "Content-Type: application/json" \
  -d '{
    "aquarioId": 1,
    "temperatura": 25.5,
    "nivelAgua": 85,
    "tdsPpm": 120
  }'

# Buscar leituras
curl "http://localhost:5000/api/leituras?aquarioId=1"
```

### Documentação interativa

Acesse `/swagger` na web para testar os endpoints de forma visual.

### Detalhes técnicos

- Controller: `LeiturasController` (em `Controllers/Api/`)
- Endpoints: GET `/api/leituras`, GET `/api/leituras/{id}`, POST `/api/leituras`
- Sem autenticação (é público por enquanto — veja `docs/security.md`)
- Resposta padrão: `LeituraRegistroResponse` em JSON

### Como integrar seu IoT

1. Configure o IP/porta da aplicação no chip (via WiFi)
2. A cada leitura, faça `POST /api/leituras` com JSON
3. Pronto! Dados aparecem no dashboard

---

## 🎯 Fluxo Típico de Uso (Do zero)

```
1. Cadastra-se (primeira vez)
   ↓
2. Faz login
   ↓
3. Cria um aquário (ex: 50L de água doce)
   ↓
4. Cadastra um peixe:
   a. Upload foto
   b. Clica "Detectar pela imagem"
   c. IA preenche parâmetros
   d. Smart Lamp recebe brilho automático
   ↓
5. Configura ESP32 pra enviar leituras pra `/api/leituras`
   ↓
6. Acessa Dashboard, vê gráficos em tempo real
   ↓
7. Se temperatura sai do ideal, vê no gráfico e ajusta

Repetir: Cadastra mais peixes → Monitora → Ajusta
```

---

## ❓ FAQ

**P: Posso ter múltiplos aquários?**
R: Sim! Crie quantos quiser. Cada um é independente.

**P: Posso compartilhar um aquário com outro usuário?**
R: Não (por enquanto). Cada aquário pertence a um user.

**P: A IA funciona sem internet?**
R: Não. Precisa conectar ao Google Gemini (precisa chave de API).

**P: Quanto custa a IA?**
R: Google Gemini oferece free tier com limite de requisições/mês. Veja https://ai.google.dev/pricing

**P: O MQTT é obrigatório?**
R: Não. Sem ESP32 conectado, você só vê os dados salvos no BD.

**P: Posso usar com outro banco de dados?**
R: No momento apenas SQL Server. Seria preciso adaptar as Stored Procedures.

**P: Senhas em texto plano é realmente perigoso?**
R: SIM. Em produção, sempre use BCrypt/Argon2. Veja `docs/security.md`.

---

*Documentação de funcionalidades — Aquamarine*  
*Última atualização: Mai/2026*
