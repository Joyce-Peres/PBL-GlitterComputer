-- ==========================================
-- SCRIPT DE CRIA��O DAS STORED PROCEDURES
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
-- STORED PROCEDURES GEN�RICAS (PadraoDAO)
-- ==========================================
 
-- SP: spListagem - Listagem gen�rica
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
 
-- SP: spProximoId - Obt�m o pr�ximo ID
CREATE OR ALTER PROCEDURE spProximoId
    @tabela NVARCHAR(100)
AS
BEGIN
    IF @tabela = 'Usuarios'
    BEGIN
        SELECT ISNULL(MAX(id), 0) + 1 FROM Usuarios
    END
    ELSE IF @tabela = 'PBL'
    BEGIN
        SELECT ISNULL(MAX(id), 0) + 1 FROM PBL
    END
END
GO
 
-- SP: spConsulta - Consulta gen�rica por ID
CREATE OR ALTER PROCEDURE spConsulta
    @id INT,
    @tabela NVARCHAR(100)
AS
BEGIN
    IF @tabela = 'Usuarios'
    BEGIN
        SELECT id, nome, login, senha FROM Usuarios WHERE id = @id
    END
    ELSE IF @tabela = 'PBL'
    BEGIN
        SELECT id, localViagem, dataViagem, imagem1, imagem2, imagem3, usuarioId, dataCriacao, dataUltimaAlteracao
        FROM PBL WHERE id = @id
    END
END
GO
 
-- SP: spDelete - Exclus�o gen�rica
CREATE OR ALTER PROCEDURE spDelete
    @id INT,
    @tabela NVARCHAR(100)
AS
BEGIN
    IF @tabela = 'Usuarios'
    BEGIN
        DELETE FROM PBL WHERE usuarioId = @id
        DELETE FROM Usuarios WHERE id = @id
    END
    ELSE IF @tabela = 'PBL'
    BEGIN
        DELETE FROM PBL WHERE id = @id
    END
END
GO
 
-- ==========================================
-- STORED PROCEDURES ESPEC�FICAS - USUARIO
-- ==========================================
 
-- SP: spIncluiUsuario - Insere novo usu�rio
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
 
-- SP: spConsultaUsuario - Consulta usu�rio por ID
CREATE OR ALTER PROCEDURE spConsultaUsuario
    @id INT
AS
BEGIN
    SELECT id, nome, login, senha FROM Usuarios WHERE id = @id
END
GO
 
-- SP: spConsultaUsuarioPorLogin - Consulta usu�rio por Login
CREATE OR ALTER PROCEDURE spConsultaUsuarioPorLogin
    @login VARCHAR(50)
AS
BEGIN
    SELECT id, nome, login, senha FROM Usuarios WHERE login = @login
END
GO
 