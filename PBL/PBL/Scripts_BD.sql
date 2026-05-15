-- ==========================================
-- SCRIPT DE CRIAÇÃO DAS STORED PROCEDURES
-- Projeto: CadFotosViagem
-- ==========================================

-- ==========================================
-- TABELA USUARIOS
-- ==========================================
IF OBJECT_ID('Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE Usuarios (
        id INT PRIMARY KEY IDENTITY(1,1),
        nome VARCHAR(150) NOT NULL,
        login VARCHAR(50) NOT NULL UNIQUE,
        senha VARCHAR(50) NOT NULL
    )
END
GO

-- ==========================================
-- TABELA FOTOSVIAGEM
-- ==========================================
IF OBJECT_ID('FotosViagem', 'U') IS NULL
BEGIN
    CREATE TABLE FotosViagem (
        id INT PRIMARY KEY IDENTITY(1,1),
        localViagem VARCHAR(255) NOT NULL,
        dataViagem DATETIME NOT NULL,
        imagem1 VARCHAR(MAX) NOT NULL,
        imagem2 VARCHAR(MAX),
        imagem3 VARCHAR(MAX),
        usuarioId INT NOT NULL,
        dataCriacao DATETIME NOT NULL,
        dataUltimaAlteracao DATETIME,
        FOREIGN KEY (usuarioId) REFERENCES Usuarios(id)
    )
END
GO

-- ==========================================
-- STORED PROCEDURES GENÉRICAS (PadraoDAO)
-- ==========================================

-- SP: spListagem - Listagem genérica
CREATE OR ALTER PROCEDURE spListagem
    @tabela NVARCHAR(100),
    @Ordem INT
AS
BEGIN
    IF @tabela = 'Usuarios'
    BEGIN
        SELECT id, nome, login, senha FROM Usuarios ORDER BY id
    END
    ELSE IF @tabela = 'FotosViagem'
    BEGIN
        SELECT id, localViagem, dataViagem, imagem1, imagem2, imagem3, usuarioId, dataCriacao, dataUltimaAlteracao 
        FROM FotosViagem ORDER BY id
    END
END
GO

-- SP: spProximoId - Obtém o próximo ID
CREATE OR ALTER PROCEDURE spProximoId
    @tabela NVARCHAR(100)
AS
BEGIN
    IF @tabela = 'Usuarios'
    BEGIN
        SELECT ISNULL(MAX(id), 0) + 1 FROM Usuarios
    END
    ELSE IF @tabela = 'FotosViagem'
    BEGIN
        SELECT ISNULL(MAX(id), 0) + 1 FROM FotosViagem
    END
END
GO

-- SP: spConsulta - Consulta genérica por ID
CREATE OR ALTER PROCEDURE spConsulta
    @id INT,
    @tabela NVARCHAR(100)
AS
BEGIN
    IF @tabela = 'Usuarios'
    BEGIN
        SELECT id, nome, login, senha FROM Usuarios WHERE id = @id
    END
    ELSE IF @tabela = 'FotosViagem'
    BEGIN
        SELECT id, localViagem, dataViagem, imagem1, imagem2, imagem3, usuarioId, dataCriacao, dataUltimaAlteracao 
        FROM FotosViagem WHERE id = @id
    END
END
GO

-- SP: spDelete - Exclusão genérica
CREATE OR ALTER PROCEDURE spDelete
    @id INT,
    @tabela NVARCHAR(100)
AS
BEGIN
    IF @tabela = 'Usuarios'
    BEGIN
        DELETE FROM FotosViagem WHERE usuarioId = @id
        DELETE FROM Usuarios WHERE id = @id
    END
    ELSE IF @tabela = 'FotosViagem'
    BEGIN
        DELETE FROM FotosViagem WHERE id = @id
    END
END
GO

-- ==========================================
-- STORED PROCEDURES ESPECÍFICAS - USUARIO
-- ==========================================

-- SP: spIncluiUsuario - Insere novo usuário
CREATE OR ALTER PROCEDURE spIncluiUsuario
    @id INT,
    @nome VARCHAR(150),
    @login VARCHAR(50),
    @senha VARCHAR(50)
AS
BEGIN
    INSERT INTO Usuarios (nome, login, senha)
    VALUES (@nome, @login, @senha)
END
GO

-- SP: spConsultaUsuario - Consulta usuário por ID
CREATE OR ALTER PROCEDURE spConsultaUsuario
    @id INT
AS
BEGIN
    SELECT id, nome, login, senha FROM Usuarios WHERE id = @id
END
GO

-- SP: spConsultaUsuarioPorLogin - Consulta usuário por Login
CREATE OR ALTER PROCEDURE spConsultaUsuarioPorLogin
    @login VARCHAR(50)
AS
BEGIN
    SELECT id, nome, login, senha FROM Usuarios WHERE login = @login
END
GO

-- ==========================================
-- STORED PROCEDURES ESPECÍFICAS - FOTOVIAGEM
-- ==========================================

-- SP: spIncluiFotoViagem - Insere nova foto de viagem
CREATE OR ALTER PROCEDURE spIncluiFotoViagem
    @id INT,
    @localViagem VARCHAR(255),
    @dataViagem DATETIME,
    @imagem1 VARCHAR(MAX),
    @imagem2 VARCHAR(MAX),
    @imagem3 VARCHAR(MAX),
    @usuarioId INT,
    @dataCriacao DATETIME,
    @dataUltimaAlteracao DATETIME
AS
BEGIN
    INSERT INTO FotosViagem (localViagem, dataViagem, imagem1, imagem2, imagem3, usuarioId, dataCriacao, dataUltimaAlteracao)
    VALUES (@localViagem, @dataViagem, @imagem1, @imagem2, @imagem3, @usuarioId, @dataCriacao, @dataUltimaAlteracao)
END
GO

-- SP: spAlteraFotoViagem - Altera foto de viagem existente
CREATE OR ALTER PROCEDURE spAlteraFotoViagem
    @id INT,
    @localViagem VARCHAR(255),
    @dataViagem DATETIME,
    @imagem1 VARCHAR(MAX),
    @imagem2 VARCHAR(MAX),
    @imagem3 VARCHAR(MAX),
    @usuarioId INT,
    @dataCriacao DATETIME,
    @dataUltimaAlteracao DATETIME
AS
BEGIN
    UPDATE FotosViagem
    SET localViagem = @localViagem,
        dataViagem = @dataViagem,
        imagem1 = @imagem1,
        imagem2 = @imagem2,
        imagem3 = @imagem3,
        usuarioId = @usuarioId,
        dataUltimaAlteracao = @dataUltimaAlteracao
    WHERE id = @id
END
GO

-- SP: spConsultaFotoViagem - Consulta foto de viagem por ID
CREATE OR ALTER PROCEDURE spConsultaFotoViagem
    @id INT
AS
BEGIN
    SELECT id, localViagem, dataViagem, imagem1, imagem2, imagem3, usuarioId, dataCriacao, dataUltimaAlteracao 
    FROM FotosViagem 
    WHERE id = @id
END
GO

-- SP: spExcluiFotoViagem - Exclui foto de viagem por ID
CREATE OR ALTER PROCEDURE spExcluiFotoViagem
    @id INT
AS
BEGIN
    DELETE FROM FotosViagem WHERE id = @id
END
GO

-- SP: spConsultaFotoViagemPorUsuario - Consulta fotos de viagem por usuário
CREATE OR ALTER PROCEDURE spConsultaFotoViagemPorUsuario
    @usuarioId INT
AS
BEGIN
    SELECT id, localViagem, dataViagem, imagem1, imagem2, imagem3, usuarioId, dataCriacao, dataUltimaAlteracao 
    FROM FotosViagem 
    WHERE usuarioId = @usuarioId
    ORDER BY dataCriacao DESC
END
GO

-- ==========================================
-- FIM DO SCRIPT
-- ==========================================
