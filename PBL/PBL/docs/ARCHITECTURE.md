# Arquitetura e Comunicação — Aquamarine

> Guia conversacional de como as peças do Aquamarine funcionam juntas

## Visão Geral

O Aquamarine segue o padrão **MVC (Model-View-Controller)** com uma camada adicional de **DAO (Data Access Object)** e **Services** pra manter tudo organizado.

```
┌─────────────┐
│    VIEW     │ (HTML/Razor — form de peixe, dashboard, etc)
└──────┬──────┘
       │
┌──────▼──────┐
│ CONTROLLER  │ (PeixeController, AquarioController, etc)
└──────┬──────┘
       │
┌──────▼──────────────┐
│  SERVICES           │ (FishAiService, SmartLampMqttService)
└──────┬──────────────┘
       │
┌──────▼──────┐
│    DAO      │ (PeixeDAO, AquarioDAO — falam com BD)
└──────┬──────┘
       │
┌──────▼──────┐
│ SQL SERVER  │ (Stored Procedures, tabelas)
└─────────────┘
```

---

## 🔄 Fluxo de Comunicação Principal

### Caso de Uso: Usuário cadastra um peixe

#### 1️⃣ **View → Controller**

Usuário preenche o formulário de peixe em `/Peixe/Create` e clica em "Salvar":

```
Form (form.cshtml)
    ↓
POST /Peixe/Salvar
    ↓
PeixeController.Save(PeixeViewModel model, string Operacao)
```

#### 2️⃣ **Controller valida dados**

```csharp
// PeixeController.cs
ValidaDados(model, Operacao);

// Aqui checamos:
// - Nome não é vazio
// - Espécie foi preenchida
// - Tamanho > 0
// - Aquário foi selecionado
// - Parâmetros fazem sentido (min ≤ ideal ≤ max)

if (ModelState.IsValid == false) {
    // Se tá ruim, volta pro form com erros
    return View(NomeViewForm, model);
}
```

#### 3️⃣ **Controller salva foto (se enviada)**

```csharp
var arquivoFoto = Request.Form.Files["arquivoFoto"];
if (arquivoFoto != null && arquivoFoto.Length > 0) {
    // Salva em wwwroot/uploads/peixes/
    model.Foto = SalvarFoto(arquivoFoto, model.Id);
}
```

#### 4️⃣ **Controller chama DAO pra persistir**

```csharp
if (Operacao == "I")  // Inserir
    DAO.Insert(model);
else                   // Alterar
    DAO.Update(model);
```

#### 5️⃣ **DAO executa Stored Procedure**

```csharp
// PadraoDAO.cs (classe genérica)
public virtual void Insert(T model) {
    // Chama: spInsert_Peixes com os parâmetros do peixe
    HelperDAO.ExecutaProc("spInsert_" + Tabela, CriaParametros(model));
}
```

A SP insere na tabela `Peixes`:

```sql
INSERT INTO Peixes (nome, especie, nomeCientifico, ...)
VALUES (@nome, @especie, @nomeCientifico, ...)
```

#### 6️⃣ **Controller dispara automação (Smart Lamp)**

```csharp
// Se o peixe tem parâmetros ideais, aplica à lâmpada
if (model.LuminosidadeIdeal.HasValue || model.TemperaturaIdeal.HasValue) {
    _lampDao.AplicarAlvos(model.AquarioId, model.LuminosidadeIdeal, ...);
    
    // Se conseguir conectar ao chip MQTT, envia os alvos
    _ = _mqtt.AplicarBrilhoAsync(model.LuminosidadeIdeal.Value);
}
```

#### 7️⃣ **Resultado**

Peixe salvo no BD, foto no servidor, e a lâmpada (se conectada) já tá no brilho certo.

---

## 🧠 Fluxo da IA (Google Gemini)

### Caso de Uso: Detectar parâmetros pela imagem

```
1. User clica "Detectar parâmetros pela imagem (IA)"
   ↓
2. JavaScript faz POST /Peixe/DetectarParametros com a foto
   ↓
3. PeixeController.DetectarParametros() salva foto num temp dir
   ↓
4. Chama FishAiService.AnalisarImagemAsync(caminhoImagem)
   ↓
5. FishAiService:
   - Calcula hash da imagem (pra rastreabilidade)
   - Lê bytes da imagem
   - Envia pra Google Gemini com prompt especialista em peixes
   ↓
6. Google Gemini retorna JSON:
   {
     "especie": "Poecilia reticulata",
     "nome_cientifico": "Guppy",
     "dht22_temp_alvo": 25.0,
     "ldr_luz_alvo": 40,
     ...
   }
   ↓
7. FishAiService converte JSON → FishAiResult (C# object)
   ↓
8. Controller retorna JSON pro frontend
   ↓
9. JavaScript preenche automaticamente os campos no form
   ↓
10. User revisa e clica "Salvar" (volta ao fluxo anterior)
```

**Por que funciona assim?**

- Evita fazer chamada síncrona na view (ficaria lenta)
- User consegue revisar os dados antes de salvar
- Se a IA errar, user corrige manualmente
- Rastreabilidade: BD marca `OriginFromAI = true` pra esses dados

---

## 📊 Fluxo do Dashboard de Leituras IoT

### Chegam dados dos sensores

```
Dispositivo IoT (ESP32/Arduino)
    ↓
POST /api/leituras com JSON:
{
  "aquarioId": 1,
  "temperatura": 25.5,
  "nivelAgua": 85,
  "tdsPpm": 120
}
    ↓
LeiturasController.Post(LeituraIoTRequest request)
    ↓
Salva na tabela LeiturasSensor (BD local)
    ↓
(Opcional) Envia pra FIWARE STH-Comet se configurado
    ↓
Dashboard consulta:
GET /api/leituras?aquarioId=1
    ↓
Retorna histórico (últimas 24h ou período)
    ↓
Frontend renderiza gráficos com Chart.js
```

**Por que existem dois bancos?**

- **BD Local (SQL Server)**: rápido, confiável, controle total
- **STH-Comet (FIWARE MongoDB)**: histórico em nuvem, integração com IoT real

O sistema faz fallback: se STH-Comet tá down, usa BD local. Se STH-Comet tá up, prioriza lá.

---

## 💡 Fluxo da Smart Lamp (MQTT)

### Controle em tempo real

```
User seleciona brilho/modo na view SmartLamp/Personalizar
    ↓
JavaScript faz POST /SmartLamp/Salvar
    ↓
SmartLampController atualiza BD (LampConfigs)
    ↓
SmartLampMqttService.AplicarBrilhoAsync() publica no MQTT:
    
    Tópico: /TEF/lamp001/cmd
    Payload: { "brilho": 70, "modo": "Personalizado" }
    ↓
Chip ESP32 da Smart Lamp ESCUTA nesse tópico
    ↓
ESP32 recebe mensagem e IMEDIATAMENTE:
- Muda o brilho do LED pro valor recebido
- Atualiza cor RGB se configurado
    ↓
Opcional: ESP32 publica status de volta:
    Tópico: /TEF/lamp001/status
    Payload: { "brilho": 70, "temp": 28.5 }
    ↓
Dashboard pode mostrar status em tempo real
```

**Por que MQTT?**

- Protocolo leve, ideal pra IoT
- Pub/Sub naturalmente desacoplado
- Se a lâmpada desconectar, manda mensagem de volta qdo reconecta
- Latência baixa (ms)

---

## 🚪 Fluxo de Autenticação e Sessão

```
1. User acessa /Login (tela inicial)
   ↓
2. Preenche login/senha e clica "Entrar"
   ↓
3. POST /Login/FazLogin(usuario, senha)
   ↓
4. LoginController:
   - Consulta BD: SELECT * FROM Usuarios WHERE login = @login
   - Compara senha (⚠️ TEXTO PLANO AQUI - é segurança fraca!)
   ↓
5. Se credenciais corretas:
   HttpContext.Session.SetString("Logado", "true");
   HttpContext.Session.SetInt32("UsuarioId", 5);
   HttpContext.Session.SetString("UsuarioNome", "João");
   ↓
6. Redireciona pra /Home (Menu)
   ↓
7. Controllers monitoram sessão:
   if (!VerificaUserLogado(HttpContext.Session)) {
       return RedirectToAction("Index", "Login");
   }
   ↓
8. Se user tá logado, pode acessar tudo
   Se não tá, volta pro login
   ↓
9. Clica "Sair" → /Login/LogOff
   ↓
10. HttpContext.Session.Clear();
    Redireciona pro login
```

**Por que sessão?**

- Simples, funciona bem pra projetos pequenos
- Sem necessidade de JWT complexo
- Em produção, considerar Redis em vez de memória

---

## 🔀 Como as Camadas Se Conversam

### Exemplo: Filtrar peixes por aquário

```
1. View (Consulta/Peixes.cshtml)
   - User seleciona aquário e clica "Filtrar"

2. Controller (ConsultaController)
   public IActionResult Peixes(int aquarioId = 0) {
       var dao = new PeixeDAO();
       List<PeixeViewModel> peixes = dao.ConsultaPeixesFiltro(aquarioId);
       return View(peixes);
   }

3. DAO (PeixeDAO)
   public List<PeixeViewModel> ConsultaPeixesFiltro(int aquarioId) {
       var p = new SqlParameter[] {
           new SqlParameter("aquarioId", aquarioId)
       };
       var tabela = HelperDAO.ExecutaProcSelect(
           "spConsultaPeixesFiltro", p
       );
       // Converte DataTable → List<PeixeViewModel>
       return MontaLista(tabela);
   }

4. Helper (HelperDAO)
   - Abre conexão com BD
   - Executa Stored Procedure
   - Retorna DataTable

5. BD (SQL Server)
   EXEC spConsultaPeixesFiltro @aquarioId = 1
   
   A SP:
   SELECT p.id, p.nome, p.especie, p.aquarioId
   FROM Peixes p
   WHERE p.aquarioId = @aquarioId
   ORDER BY p.nome

6. Resultado volta:
   DataTable → PeixeViewModel[] → View
   
7. View renderiza tabela HTML
```

---

## 🔗 Dependências Entre Módulos

```
                        ┌─────────────┐
                        │   LOGIN     │
                        │  (Sessão)   │
                        └──────┬──────┘
                               │
                ┌──────────────┼──────────────┐
                │              │              │
           ┌────▼────┐  ┌──────▼─────┐  ┌────▼────┐
           │ AQUARIOS │  │  PEIXES    │  │ CONSULTA│
           └────┬────┘  └──────┬─────┘  └────┬────┘
                │              │            │
         ┌──────▼──────────────▼────────────▼──┐
         │     SMART LAMP                       │
         │  (quando peixe é salvo)              │
         └──────────────┬───────────────────────┘
                        │ MQTT
                   ┌────▼────┐
                   │  ESP32   │
                   │ (LED RGB)│
                   └──────────┘

         ┌──────────────────┐
         │   API LEITURAS   │
         │   (IoT devices)  │
         └────────┬─────────┘
                  │
          ┌───────▼──────────┐
          │    DASHBOARD     │
          │  (gráficos)      │
          └──────────────────┘

         ┌──────────────────┐
         │  AI (Google)     │
         │  (Gemini)        │
         └────────┬─────────┘
                  │
          ┌───────▼──────────┐
          │  PEIXE FORM      │
          │  (auto-fill)     │
          └──────────────────┘
```

---

## 💾 Padrões de Dados

### Fluxo de dados: Usuário → Form → Controller → DAO → BD

1. **View** manda `PeixeViewModel` com dados preenchidos
2. **Controller** valida e transforma em parâmetros SQL
3. **DAO** cria `SqlParameter[]` e executa SP
4. **BD** persiste

Exemplo:

```
PeixeViewModel {
    Nome = "Meu Peixe",
    Especie = "Poecilia reticulata",
    TemperaturaIdeal = 25.0
}
    ↓
SqlParameter[] {
    new SqlParameter("@nome", "Meu Peixe"),
    new SqlParameter("@especie", "Poecilia reticulata"),
    new SqlParameter("@tempIdeal", 25.0)
}
    ↓
EXEC spInsert_Peixes @nome, @especie, @tempIdeal
    ↓
INSERT INTO Peixes (nome, especie, ...)
VALUES ('Meu Peixe', 'Poecilia reticulata', ...)
```

---

## 🛡️ Segurança: Como Funciona

### Proteção contra SQL Injection

```
❌ ERRADO (concatenação):
string sql = "SELECT * FROM Peixes WHERE id = " + id;

✅ CERTO (parâmetros):
var p = new SqlParameter("id", id);
ExecutaProc("spConsultaPeixe", p);
```

Aqui usamos sempre `SqlParameter`, então injection não funciona.

### Proteção contra XSS

```
❌ ERRADO:
model.Nome = Request.Form["nome"]; // Pega direto

✅ CERTO:
model.Nome = WebUtility.HtmlEncode(Request.Form["nome"]?.Trim());
```

Isso converte `<script>alert('xss')</script>` em `&lt;script&gt;...`, neutro.

### Autenticação

```
❌ FRACO (texto plano):
if (usuarioLogado.Senha == senha) { ... }

⚠️ MELHOR (pra produção):
if (BCrypt.Net.BCrypt.Verify(senha, usuarioLogado.Senha)) { ... }
```

---

## 🔧 Configurações Importantes

### Onde tudo é configurado

1. **Banco de dados**: `DAO/ConexaoBD.cs` (hardcoded, não ideal)
2. **IA**: `appsettings.json` → seção `FishAi`
3. **MQTT**: `appsettings.json` → seção `SmartLampMqtt`
4. **FIWARE**: `appsettings.json` → seção `FiwareSthComet`
5. **Chave Gemini**: `.env` ou variável de ambiente

### Como carregar config

```csharp
public FishAiService(IConfiguration config) {
    var apiKey = config["FishAi:ApiKey"];
    var model = config["FishAi:Model"];
    // ...
}
```

Isso injeta a config do `appsettings.json` automaticamente.

---

## 📝 Resumo da Comunicação

| Evento | Flow | Resultado |
|--------|------|-----------|
| User cadastra peixe | Form → Controller → DAO → BD | Peixe salvo, lâmpada atualizada |
| User detecta com IA | Form → Controller → Google API | Parâmetros preenchidos no form |
| ESP32 envia leitura | POST /api/leituras | Leitura no BD, pode vir em gráfico |
| User muda brilho | SmartLamp form → MQTT → ESP32 | LED muda em tempo real |
| User faz login | LoginController → Sessão | Access liberado ou bloqueado |
| User filtra peixes | ConsultaController → DAO → Query | Tabela renderizada |

---

## 🚀 Próximas Melhorias

- [ ] Migrar senhas de texto plano pra BCrypt/Argon2
- [ ] Mover string de conexão de hardcoded pra `appsettings`
- [ ] Usar Entity Framework Core em vez de Stored Procedures
- [ ] Adicionar autenticação JWT pra API IoT
- [ ] Redis pra sessões (escalabilidade)
- [ ] Testes unitários com Moq/xUnit

---

*Documentação criada seguindo o padrão conversacional do projeto.*  
*Última atualização: Mai/2026*
