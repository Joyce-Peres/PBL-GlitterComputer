# PBL-GlitterComputer

## Projeto PBL (Aquário Inteligente)

### Banco de dados
- Execute o script [PBL/PBL/Scripts_BD.sql](PBL/PBL/Scripts_BD.sql) no SQL Server para criar/atualizar as tabelas e procedures.
- A conexão está em `PBL/PBL/DAO/ConexaoBD.cs` (ajuste conforme seu ambiente).

### Smart Lamp (personalização + dashboard)
- No menu (após login): **Smart Lamp → Personalização** e **Smart Lamp → Dashboard (Luz)**.
- Configuração MQTT no arquivo [PBL/PBL/appsettings.json](PBL/PBL/appsettings.json) (seção `SmartLampMqtt`).
- PWA (opcional): [PBL/PBL/wwwroot/smartlamp-app/index.html](PBL/PBL/wwwroot/smartlamp-app/index.html)

### Cadastro de peixe com IA (por imagem)
- No formulário de peixe: envie uma foto e clique em **Detectar parâmetros pela imagem (IA)**.
- A chave da IA deve estar em variável de ambiente: `GEMINI_API_KEY`.
- O backend chama o script Python em `PBL/PBL/cadastro-peixe/reconhecer_peixe.py` (configurável em `FishAi` no `appsettings.json`).
- Ao salvar um peixe com `LuminosidadeIdeal`, o sistema tenta aplicar automaticamente o brilho da Smart Lamp via MQTT.
