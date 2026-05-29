# Quick Start — Aquamarine

> Comece a usar em 5 minutos

## 1️⃣ Setup (primeira vez)

### Banco de dados

```powershell
# Abra SQL Server Management Studio
# Execute o script:
PBL\PBL\Scripts_BD.sql

# Pronto! Tabelas criadas
```

### Chave da IA (opcional)

```powershell
# Crie arquivo PBL\.env com:
GEMINI_API_KEY=sua_chave_do_google_aqui

# Obtenha a chave em: https://ai.google.dev/
# (free tier: 15 RPM, 2M tokens/dia)
```

### Rodar a app

```powershell
cd PBL\PBL
dotnet restore
dotnet build
dotnet run
```

Acesse: `http://localhost:5000`

---

## 2️⃣ Primeiro Acesso

```
URL: http://localhost:5000
    ↓
[Tela de Login]
    ↓
Clique em "Cadastrar"
    ↓
Preencha: Nome, Login, Senha
    ↓
Clique "Cadastrar"
    ↓
Faz login com suas credenciais
    ↓
[Menu Principal aparece]
```

---

## 3️⃣ Criar um Aquário

```
Menu → "Novo Aquário"
    ↓
Nome: "Meu Aquário"
Capacidade: 50 L
Tipo de Água: Doce
    ↓
[Salvar]
    ↓
✅ Aquário criado!
```

---

## 4️⃣ Cadastrar um Peixe (COM IA)

### Caminho rápido:

```
Menu → "Novo Peixe"
    ↓
Nome: "Nemo"
Espécie: "Amphiprion"
Tamanho: 8 cm
Aquário: "Meu Aquário"
    ↓
[Upload foto do peixe]
    ↓
Clique "Detectar parâmetros pela imagem (IA)"
    ↓
⏳ Aguarde 2-3 segundos...
    ↓
✅ Campos preenchidos automaticamente!
    ↓
[Salvar]
    ↓
✅ Peixe criado, Smart Lamp atualizada!
```

### Se não tiver foto:

```
Só digite a espécie e clique "Detectar pela espécie (IA)"
```

---

## 5️⃣ Ver Dashboard

```
Menu → "Dashboard IoT"
    ↓
Aquário: "Meu Aquário"
Período: "Últimas 24h"
    ↓
[Filtrar]
    ↓
📊 Gráficos aparecem
(vazio se não tiver dados ainda)
```

---

## 6️⃣ Enviar Dados de Sensores (IoT)

### Com cURL:

```bash
curl -X POST http://localhost:5000/api/leituras \
  -H "Content-Type: application/json" \
  -d '{
    "aquarioId": 1,
    "temperatura": 25.5,
    "nivelAgua": 85,
    "tdsPpm": 120
  }'
```

### Com Arduino/ESP32:

```cpp
#include <HTTPClient.h>

HTTPClient http;
http.begin("http://seu-ip:5000/api/leituras");
http.addHeader("Content-Type", "application/json");

String payload = "{\"aquarioId\":1,\"temperatura\":25.5,\"nivelAgua\":85}";
http.POST(payload);
http.end();
```

---

## 7️⃣ Consultar Dados

```
Menu → "Consultas"
    ↓
[Peixes] ou [Leituras]
    ↓
Filtros (Aquário, Período, etc)
    ↓
[Filtrar]
    ↓
📋 Tabela com resultados
```

---

## 🧠 IA Funcionando?

### Se NÃO tá funcionando:

```
❌ Erro ao detectar → Verificar:
   1. GEMINI_API_KEY no .env está correto?
   2. Chave tá ativa no Google AI?
   3. Quota não foi excedida?
   4. Foto é válida (JPG/PNG)?

✅ Debug: Veja logs em:
   bin/Debug/netcoreapp3.1/
```

### Se SIM:

```
✅ Botão "Detectar parâmetros" funciona
✅ Badge "🧠 Gerado pela IA" aparece no peixe
✅ Smart Lamp recebe brilho automaticamente
```

---

## 📡 Smart Lamp Funcionando?

### Se NÃO tá funcionando:

```
❌ Luz não acende quando clica "Salvar" → Verificar:
   1. ESP32 tá conectado ao WiFi?
   2. Broker MQTT tá rodando?
   3. Config MQTT em appsettings.json tá correta?
   4. Tópico é /TEF/lamp*/cmd ?

✅ Debug: Veja logs do SmartLampMqttService
```

### Se SIM:

```
✅ Brilho muda em tempo real
✅ Cor RGB muda quando configura
✅ Ao cadastrar peixe, brilho se auto-ajusta
```

---

## 📚 Próximos Passos

| Quer | Vá para |
|------|---------|
| Entender como funciona | `docs/ARCHITECTURE.md` |
| Usar todas as features | `docs/FEATURES.md` |
| Conectar ESP32/Arduino | `docs/api_demo.md` |
| Melhorar segurança | `docs/security.md` |
| Testar API no navegador | Acesse `/swagger` |

---

## 🚨 Problemas Comuns

### "Erro ao conectar no banco"

```
Verificar:
✓ SQL Server tá rodando?
✓ String de conexão em ConexaoBD.cs está certa?
✓ Usuário/senha corretos?
✓ Banco "PBL" existe?
```

### "Nada é salvo"

```
Verificar:
✓ Não há erros de validação na tela?
✓ Arquivo SQL foi executado?
✓ Permissões do usuário SQL tá ok?
```

### "IA não funciona"

```
Verificar:
✓ .env tem GEMINI_API_KEY?
✓ Chave é válida?
✓ Tem internet?
✓ Quota do Google não foi excedida?
```

### "Smart Lamp não responde"

```
Verificar:
✓ ESP32/Arduino tá alimentado?
✓ Tá conectado ao WiFi?
✓ Broker MQTT rodando?
✓ Pode fazer SSH pro chip?
```

---

## 📞 Contato / Debug

Se preso em algo:

1. Veja os **logs** (console ao rodar `dotnet run`)
2. Leia a documentação (comece com `FEATURES.md`)
3. Cheque a **API** em `/swagger`
4. Veja issues já abertas no GitHub (se houver)

---

*Quick Start — Aquamarine*  
*Mai/2026*
