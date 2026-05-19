# PBL-GlitterComputer

## Como rodar esse projeto

### Pré-requisitos
- .NET SDK instalado
- ASP.NET Core Runtime 3.1 instalado (o projeto usa `netcoreapp3.1`)
- SQL Server (LocalDB ou instância local)

### 1) Configurar banco de dados
1. Crie um banco chamado **PBL** no SQL Server.
2. Execute o script:
   - `PBL/PBL/Scripts_BD.sql`
3. Defina a conexão por variável de ambiente (recomendado):

```bash
export PBL_CONNECTION_STRING='Data Source=SEU_SERVIDOR;Initial Catalog=PBL;user id=SEU_USUARIO;password=SUA_SENHA'
```

> No Windows (PowerShell), use `setx PBL_CONNECTION_STRING "Data Source=...;Initial Catalog=PBL;user id=...;password=..."`.

### 2) Restaurar e compilar
No terminal:

```bash
cd PBL
dotnet restore PBL.sln
dotnet build PBL.sln
```

### 3) Executar a aplicação

```bash
cd PBL
dotnet run --project PBL/PBL.csproj
```

> Se a variável `PBL_CONNECTION_STRING` não estiver definida, o projeto usa a string padrão existente no código (`PBL/PBL/DAO/ConexaoBD.cs`).

### 4) Acessar
- Aplicação: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

> Observação: essas URLs consideram a porta definida no `launchSettings.json` (`http://localhost:5000`).
