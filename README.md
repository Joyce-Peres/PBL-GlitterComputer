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
     - `Data Source=localhost;Initial Catalog=PBL;User ID=sa;Password=SUA_SENHA_AQUI` *(substitua `SUA_SENHA_AQUI` pela sua senha real)*
   - Recomendação: não versionar credenciais reais. Use variável de ambiente ou mecanismo seguro de configuração no seu ambiente local/produção.
   - Recomendação de segurança: evite usar `sa` em produção; prefira autenticação integrada (quando disponível) ou um usuário dedicado com permissões mínimas.

### 2) Restaurar e executar a aplicação
No terminal:

```bash
cd PBL
dotnet restore PBL.sln
dotnet run --project PBL/PBL.csproj
```

### 3) Acessar no navegador
- URL padrão (launchSettings): `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Se a porta mudar no seu ambiente, use a URL exibida no console ao executar o `dotnet run`.
