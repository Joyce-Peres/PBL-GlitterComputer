# Checklist para entrega (EC-B10503)

Este documento lista cada requisito e mostra onde ele está implementado no projeto.

- **Home e Sobre**: implementados.
  - Home: [PBL/PBL/Views/Home/Index.cshtml](PBL/PBL/Views/Home/Index.cshtml)
  - Sobre: [PBL/PBL/Views/Sobre/Index.cshtml](PBL/PBL/Views/Sobre/Index.cshtml)

- **Pelo menos 2 CRUDs com herança**: implementados via `PadraoController` / `PadraoDAO`.
  - `PeixeController` (CRUD): [PBL/PBL/Controllers/PeixeController.cs](PBL/PBL/Controllers/PeixeController.cs)
  - `AquarioController` (CRUD): [PBL/PBL/Controllers/AquarioController.cs](PBL/PBL/Controllers/AquarioController.cs)
  - Base: [PBL/PBL/Controllers/PadraoController.cs](PBL/PBL/Controllers/PadraoController.cs), [PBL/PBL/DAO/PadraoDAO.cs](PBL/PBL/DAO/PadraoDAO.cs)

- **FKs e JOINs nas listagens**:
  - Script e procedures com JOINs: [PBL/PBL/Scripts_BD.sql](PBL/PBL/Scripts_BD.sql)
  - DAO: [PBL/PBL/DAO/PeixeDAO.cs](PBL/PBL/DAO/PeixeDAO.cs)

- **Manipulação de imagem (upload/exibir)**:
  - Upload e salvamento: [PBL/PBL/Controllers/PeixeController.cs#SalvarFoto](PBL/PBL/Controllers/PeixeController.cs)
  - Diretório de uploads: [PBL/PBL/wwwroot/uploads/peixes](PBL/PBL/wwwroot/uploads/peixes)

- **Área Sobre**:
  - [PBL/PBL/Views/Sobre/Index.cshtml](PBL/PBL/Views/Sobre/Index.cshtml) (contém informações do sistema e alunos)

- **Telas de consulta com filtros (mínimo 2 filtros cada)**:
  - Consulta Peixes: [PBL/PBL/Views/Consulta/Peixes.cshtml](PBL/PBL/Views/Consulta/Peixes.cshtml)
  - Consulta Leituras (filtros por aquário/data/temperatura): [PBL/PBL/Views/Consulta/Leituras.cshtml](PBL/PBL/Views/Consulta/Leituras.cshtml)

- **Uso de Ajax em ≥2 locais**:
  - Form Peixe: chamadas a `/Peixe/DetectarParametros`, `/Peixe/DetectarParametrosPorEspecie`, `/Peixe/InfoAquario` — [PBL/PBL/Views/Peixe/form.cshtml](PBL/PBL/Views/Peixe/form.cshtml)

- **Dashboards (≥2)**:
  - [PBL/PBL/Views/Dashboard/Index.cshtml](PBL/PBL/Views/Dashboard/Index.cshtml)
  - [PBL/PBL/Views/SmartLamp/Dashboard.cshtml](PBL/PBL/Views/SmartLamp/Dashboard.cshtml)

- **API IoT (JSON)**:
  - Controller: [PBL/PBL/Controllers/Api/LeiturasController.cs](PBL/PBL/Controllers/Api/LeiturasController.cs)
  - Endpoints: `GET /api/leituras`, `GET /api/leituras/aquario/{id}`, `POST /api/leituras`

- **Swagger**:
  - Configuração em `Startup.cs` com `Swashbuckle` — Swagger UI disponível em `/swagger`.

- **Observações**:
  - Build atual: `dotnet build` concluído com sucesso (3 warnings sobre `netcoreapp3.1` EOL).
  - Recomenda-se gerar migrations e acrescentar testes automatizados.

---

Gerado automaticamente para anexar ao Moodle como evidência dos itens implementados.
