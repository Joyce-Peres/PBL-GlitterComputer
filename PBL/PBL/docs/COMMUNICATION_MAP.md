# Mapa de Comunicação — Aquamarine

> Visualização de como todos os componentes se conectam

## 🔄 Fluxo Global

```
┌─────────────────────────────────────────────────────────────────┐
│                         AQUAMARINE                              │
├──────────────┬──────────────┬──────────────┬────────────────────┤
│              │              │              │                    │
│    USER      │    BROWSER   │   SERVER     │   EXTERNAL         │
│              │              │              │                    │
└──────────────┴──────────────┴──────────────┴────────────────────┘
```

---

## 1️⃣ Autenticação

```
┌─────────────────────────────────────────────────────┐
│                   USER NA TELA LOGIN                │
└───────────────┬─────────────────────────────────────┘
                │
                ├─→ POST /Login/FazLogin(usuario, senha)
                │
        ┌───────▼────────────────────┐
        │   LoginController          │
        │ (valida credenciais)       │
        └───────┬────────────────────┘
                │
        ┌───────▼────────────────────┐
        │   UsuarioDAO.Consulta()    │
        │ (busca no BD)              │
        └───────┬────────────────────┘
                │
        ┌───────▼────────────────────┐
        │   SQL Server               │
        │ SELECT * FROM Usuarios     │
        └───────┬────────────────────┘
                │
        ┌───────▼────────────────────┐
        │ Se correto:                │
        │ • Session["Logado"] = true │
        │ • Session["UsuarioId"] = 5 │
        └───────┬────────────────────┘
                │
        ┌───────▼────────────────────┐
        │ Redirect /Home (Menu)      │
        │ ✅ User logado!            │
        └────────────────────────────┘
```

---

## 2️⃣ Cadastro de Peixe (com IA)

```
┌────────────────────────────────────────────┐
│  USER no form: /Peixe/Create               │
│  • Upload foto                             │
│  • Preenche Nome, Espécie, etc             │
└────────────────┬───────────────────────────┘
                 │
                 ├─→ JavaScript detecta IA
                 │
         ┌───────▼──────────────────────────┐
         │ POST /Peixe/DetectarParametros   │
         │ (envia foto)                     │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ PeixeController                  │
         │ • Salva foto em temp/            │
         │ • Chama FishAiService            │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ FishAiService                    │
         │ • Calcula hash da imagem         │
         │ • Lê bytes                       │
         │ • Cria prompt de especialista    │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ GOOGLE GEMINI API                │
         │ 🌐 Análise de imagem             │
         │ ⏳ 2-3 segundos                  │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ Google retorna JSON:             │
         │ {                                │
         │   "especie": "...",              │
         │   "dht22_temp_alvo": 25.0,       │
         │   "ldr_luz_alvo": 40,            │
         │   ...                            │
         │ }                                │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ Controller converte JSON         │
         │ → FishAiResult (C# object)       │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ Response JSON ao Browser         │
         │ JavaScript preenche form         │
         │ Badge "🧠 Gerado pela IA" mostra │
         └──────────────────────────────────┘
                 │
                 ├─→ User clica "Salvar"
                 │
         ┌───────▼──────────────────────────┐
         │ POST /Peixe/Salvar               │
         │ (form completo)                  │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ PeixeController.Save()           │
         │ • Valida dados                   │
         │ • Chama DAO.Insert()             │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ PeixeDAO.Insert()                │
         │ • Cria SqlParameter[]            │
         │ • Executa spInsert_Peixes        │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ SQL Server                       │
         │ EXEC spInsert_Peixes             │
         │ INSERT INTO Peixes ...           │
         │ (inclui 13 parâmetros de coluna) │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ AUTOMAÇÃO: Smart Lamp            │
         │ SmartLampDao.AplicarAlvos()      │
         │ • Salva brilho no BD             │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ MQTT publish:                    │
         │ SmartLampMqttService             │
         │ Topic: /TEF/lamp001/cmd          │
         │ Payload: {brilho: 40}            │
         └───────┬──────────────────────────┘
                 │
         ┌───────▼──────────────────────────┐
         │ ESP32 recebe MQTT                │
         │ 🔴 LED muda pra 40% brilho       │
         │ ✅ Automação completa!           │
         └──────────────────────────────────┘
```

---

## 3️⃣ IoT — Dispositivo envia leitura

```
┌────────────────────────────────────┐
│  DISPOSITIVO IOT                   │
│  (ESP32/Arduino com sensores)      │
│  • DHT22 (temperatura)             │
│  • LDR (luminosidade)              │
│  • TDS (qualidade)                 │
└────────────────┬───────────────────┘
                 │
        ┌────────▼──────────────────────┐
        │ POST /api/leituras             │
        │ JSON:                          │
        │ {                              │
        │   "aquarioId": 1,              │
        │   "temperatura": 25.5,         │
        │   "nivelAgua": 85,             │
        │   "tdsPpm": 120                │
        │ }                              │
        └────────┬──────────────────────┘
                 │
        ┌────────▼──────────────────────┐
        │ LeiturasController             │
        │ .Post(LeituraIoTRequest)       │
        └────────┬──────────────────────┘
                 │
        ┌────────▼──────────────────────┐
        │ Valida dados                   │
        │ (aquarioId > 0, etc)           │
        └────────┬──────────────────────┘
                 │
        ┌────────▼──────────────────────┐
        │ LeituraSensorDAO.Inserir()     │
        │ (salva no BD)                  │
        └────────┬──────────────────────┘
                 │
        ┌────────▼──────────────────────┐
        │ (Opcional)                     │
        │ FiwareSthCometService          │
        │ (envia pra nuvem FIWARE)       │
        └────────┬──────────────────────┘
                 │
        ┌────────▼──────────────────────┐
        │ Response: 200 OK               │
        │ {sucesso: true}                │
        └────────┬──────────────────────┘
                 │
        ┌────────▼──────────────────────┐
        │ Dashboard atualiza gráficos    │
        │ (AJAX refresh 5 seg)           │
        │ 📊 Usuário vê dados em tempo   │
        │    real no gráfico             │
        └────────────────────────────────┘
```

---

## 4️⃣ Dashboard — Consulta histórico

```
┌──────────────────────────────────┐
│  USER no Dashboard               │
│  • Seleciona aquário             │
│  • Seleciona período             │
│  Clica "Filtrar"                 │
└──────────────────┬───────────────┘
                   │
        ┌──────────▼──────────────┐
        │ GET /api/leituras       │
        │ ?aquarioId=1            │
        │ &dataInicio=2026-05-01  │
        │ &dataFim=2026-05-29     │
        └──────────────┬──────────┘
                       │
        ┌──────────────▼──────────────┐
        │ LeiturasController           │
        │ .Get(aquarioId, datas)      │
        └──────────────┬──────────────┘
                       │
        ┌──────────────▼──────────────┐
        │ Verifica fonte de dados:     │
        │ • STH-Comet (preferido)?     │
        │ • BD local (fallback)?       │
        └──────────────┬──────────────┘
                       │
        ┌──────────────▼──────────────┐
        │ Query (SQL ou REST)          │
        │ SELECT leituras WHERE ...    │
        └──────────────┬──────────────┘
                       │
        ┌──────────────▼──────────────┐
        │ Resultado: List<Leitura>    │
        │ [                            │
        │   {id: 1, temp: 25.5, ...},  │
        │   {id: 2, temp: 25.7, ...},  │
        │   ...                        │
        │ ]                            │
        └──────────────┬──────────────┘
                       │
        ┌──────────────▼──────────────┐
        │ Response JSON ao Browser     │
        │ JavaScript renderiza:        │
        │ • Tabela com dados           │
        │ • Gráficos com Chart.js      │
        │ 📊 Dashboard visualiza!      │
        └──────────────────────────────┘
```

---

## 5️⃣ Smart Lamp — Controle em tempo real

```
┌──────────────────────────────────┐
│  USER no controle Smart Lamp     │
│  • Seleciona brilho (slider)     │
│  • Seleciona cor RGB (picker)    │
│  Clica "Salvar"                  │
└──────────────┬───────────────────┘
               │
       ┌───────▼────────────────────┐
       │ POST /SmartLamp/Salvar      │
       │ {modo: 4, brilho: 70}      │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ SmartLampController         │
       │ • Salva no BD (LampConfigs) │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ SmartLampMqttService        │
       │ .AplicarBrilhoAsync(70)     │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ MQTT Publish:               │
       │ Broker: (ex. broker.mqtt)   │
       │ Topic: /TEF/lamp001/cmd     │
       │ Payload:                    │
       │ {                           │
       │   "brilho": 70,             │
       │   "modo": 4,                │
       │   "r": 255,                 │
       │   "g": 128,                 │
       │   "b": 0                    │
       │ }                           │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ ESP32/Arduino               │
       │ ESCUTA tópico MQTT          │
       │ Recebe mensagem             │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ ESP32 executa:              │
       │ • digitalWrite(LED_PIN, 70%)│
       │ • setColor(255, 128, 0)     │
       │ ⚡ LED muda IMEDIATAMENTE   │
       │ (latência < 100ms)          │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ (Opcional)                  │
       │ ESP32 publica status:        │
       │ Topic: /TEF/lamp001/status  │
       │ {brilho: 70, temp: 28.5}    │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ Dashboard mostra:            │
       │ "🟢 Lâmpada: 70% brilho"     │
       │ (atualiza em tempo real)     │
       └────────────────────────────┘
```

---

## 6️⃣ CRUD de Peixes (genérico)

```
┌──────────────────────────────────┐
│  USER clica "Editar" ou "Deletar" │
└──────────────┬───────────────────┘
               │
       ┌───────▼────────────────────┐
       │ PeixeController             │
       │ (herda de PadraoController) │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ PeixeDAO                    │
       │ (herda de PadraoDAO<T>)     │
       │ • Update()                  │
       │ • Delete()                  │
       │ • Insert()                  │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ HelperDAO                   │
       │ .ExecutaProc()              │
       │ (executa Stored Procedure)  │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ SQL Server                  │
       │ EXEC spUpdate_Peixes        │
       │ EXEC spDelete_Peixes        │
       │ etc                         │
       └───────┬────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ ✅ Dados atualizados        │
       │ Redirect pra listagem       │
       └────────────────────────────┘

PADRÃO GENÉRICO:
- 1 Controller base → todos herdam
- 1 DAO base → todos herdam
- 13 Stored Procedures genéricas
- Reduz duplicação de código 70%!
```

---

## 7️⃣ Validação de Compatibilidade

```
┌─────────────────────────────────────┐
│ User tenta salvar peixe             │
│ • Peixe: 100L mínimo                │
│ • Aquário: 50L capacidade           │
│ • Tipo: Salgada                     │
│ • Salinidade: > 0.5 ppt (marinho)   │
└─────────────┬───────────────────────┘
              │
      ┌───────▼─────────────────────┐
      │ PeixeController.ValidaDados │
      └───────┬─────────────────────┘
              │
      ┌───────▼─────────────────────┐
      │ if (volumeMin >             │
      │     capacidadeAquario)       │
      │   confirm("Aviso: ...")      │
      │   return false              │
      └───────┬─────────────────────┘
              │
      ┌───────▼─────────────────────┐
      │ if (aquario.TipoAgua ==      │
      │     "Doce" &&               │
      │     peixe.Salinidade >       │
      │     0.5)                     │
      │   confirm("Aviso: ...")      │
      │   return false              │
      └───────┬─────────────────────┘
              │
      ┌───────▼─────────────────────┐
      │ Se todas validações OK:      │
      │ ✅ Salva no BD              │
      │                             │
      │ Se falhou:                  │
      │ ❌ Mostra erros no form     │
      └─────────────────────────────┘
```

---

## 📊 Tabela: Componentes e Responsabilidades

| Componente | Responsabilidade | Comunica com |
|------------|------------------|--------------|
| **Views** | Renderizar HTML, validações frontend | JavaScript |
| **Controllers** | Lógica de negócio, routing | Views, Services, DAOs |
| **Services** | Integrações externas (IA, MQTT) | APIs externas, DAOs |
| **DAOs** | Acesso a dados, Stored Procedures | SQL Server |
| **SQL Server** | Persistência, Stored Procedures | DAO via SqlClient |
| **Google Gemini** | Análise de imagens/IA | FishAiService |
| **MQTT Broker** | Pub/Sub de mensagens IoT | SmartLampService, ESP32 |
| **ESP32/Arduino** | Hardware, sensores, LED | MQTT, API REST |

---

## 🔌 Pontos de Integração

```
┌────────────────────────────────────────────────────┐
│              AQUAMARINE                            │
│                                                    │
│  ┌─────────────────────────────────────────────┐  │
│  │ Integrações Externas                        │  │
│  ├─────────────────────────────────────────────┤  │
│  │ 🔵 Google Gemini AI ← FishAiService         │  │
│  │ 📊 FIWARE STH-Comet ← FiwareSthCometService│  │
│  │ 📡 MQTT Broker ← SmartLampMqttService       │  │
│  │ 🛠️ SQL Server ← DAOs                        │  │
│  └─────────────────────────────────────────────┘  │
│                                                    │
│  ┌─────────────────────────────────────────────┐  │
│  │ IoT Devices (entrada de dados)              │  │
│  ├─────────────────────────────────────────────┤  │
│  │ 🟢 ESP32/Arduino sensores → API /leituras   │  │
│  │ 💡 Smart Lamp (ESP32) ← MQTT                │  │
│  └─────────────────────────────────────────────┘  │
│                                                    │
└────────────────────────────────────────────────────┘
```

---

## Fluxo da Sessão do Usuário (início ao fim)

```
1. User acessa localhost:5000
   ↓
2. Sem sessão → redireciona pro /Login
   ↓
3. Cadastra ou faz login
   ↓
4. Session criada no servidor
   ↓
5. Redirect /Home (Menu)
   ↓
6. Todo controller checa:
   if (!VerificaUserLogado(HttpContext.Session))
      redirect /Login
   ↓
7. Se tá logado → acessa feature
   ↓
8. User clica "Sair"
   ↓
9. Session.Clear()
   ↓
10. Redirect /Login
    ↓
11. Session destruída, user logado fora
    ↓
12. Tela de login novamente
```

---

*Mapa de Comunicação — Aquamarine*  
*Visualização dos fluxos e integrações*  
*Mai/2026*
