# Checklist de Entrega (Evidências)

Este arquivo consolida os itens solicitados pelo Moodle e indica onde cada requisito está implementado no projeto.

## Sumário
- Home e Sobre
- CRUDs com herança
- FKs e JOINs
- Upload de imagens
- Telas de consulta com filtros
- Uso de Ajax
- Dashboards
- API IoT (JSON)
- Swagger

## Mapeamento rápido
- **Home / Sobre**: Views em `Views/Home/Index.cshtml` e `Views/Sobre/Index.cshtml`
- **CRUDs com herança**: `PadraoController` / `PadraoDAO` como base; `PeixeController`, `AquarioController`.
- **FKs / JOINs**: Scripts em `Scripts_BD.sql` e implementações em `DAO/*DAO.cs` (ex.: `PeixeDAO.cs`).
- **Upload de imagens**: `PeixeController` (ação `SalvarFoto`), diretório `wwwroot/uploads/peixes`.
- **Consultas / Filtros**: `Views/Consulta/Peixes.cshtml`, `Views/Consulta/Leituras.cshtml`.
- **Ajax**: Chamadas em `Views/Peixe/form.cshtml` para endpoints de detecção e info de aquário.
- **Dashboards**: `Views/Dashboard/Index.cshtml` e `Views/SmartLamp/Dashboard.cshtml`.
- **API IoT**: Controller `Controllers/Api/LeiturasController.cs` — endpoints `GET /api/leituras`, `POST /api/leituras`.
- **Swagger**: configurado em `Startup.cs` e disponível em `/swagger` quando a aplicação está rodando.

## Observações para entrega
- Inclua a URL base (ex.: `http://localhost:5000`) ao demonstrar endpoints.
- Faça backup do banco antes de executar `Scripts_BD.sql`.

---
Arquivo gerado automaticamente a partir de `CHECKLIST_MOODLE.md` — mantido como evidência e reorganizado.
