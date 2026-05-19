# PBL-GlitterComputer

## Como rodar esse projeto

### Pré-requisitos
- .NET SDK instalado (o projeto usa `netcoreapp3.1`)
- SQL Server (LocalDB ou instância local)

### 1) Configurar banco de dados
1. Crie um banco chamado **PBL** no SQL Server.
2. Execute o script:
   - `PBL/PBL/Scripts_BD.sql`
3. Confira/ajuste a string de conexão em:
   - `PBL/PBL/DAO/ConexaoBD.cs`
   - Valor atual: `Data Source=LOCALHOST;Initial Catalog=PBL;user id=sa; password=123456`

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

### 4) Acessar
- Aplicação: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
