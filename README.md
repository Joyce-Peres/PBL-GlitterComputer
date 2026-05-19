# PBL-GlitterComputer

## Como rodar esse projeto

### Pré-requisitos
- SQL Server (Express ou Developer)
- .NET SDK (compatível com `netcoreapp3.1`)

### 1) Configurar o banco de dados
1. Crie um banco chamado `PBL` no SQL Server.
2. Abra e execute o script:
   - `PBL/PBL/Scripts_BD.sql`
3. Verifique a string de conexão em:
   - `PBL/PBL/DAO/ConexaoBD.cs`
   - Ajuste para o seu ambiente (servidor, usuário e senha), por exemplo:
     - `Data Source=LOCALHOST;Initial Catalog=PBL;user id=sa; password=SUA_SENHA_AQUI`

### 2) Restaurar e executar a aplicação
No terminal:

```bash
cd PBL
dotnet restore PBL.sln
dotnet run --project PBL/PBL.csproj
```

### 3) Acessar no navegador
- Aplicação: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
