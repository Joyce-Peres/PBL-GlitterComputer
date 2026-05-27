-- ==========================================
-- SCRIPT DE CRIAÇÃO - Aquário Inteligente
-- Projeto: PBL GlitterComputer
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
-- TABELA AQUARIOS (relaciona com Usuarios)
-- ==========================================
IF OBJECT_ID('Aquarios', 'U') IS NULL
BEGIN
    CREATE TABLE Aquarios (
        id INT PRIMARY KEY IDENTITY(1,1),
        nome VARCHAR(100) NOT NULL,
        capacidadeLitros DECIMAL(10,2) NOT NULL,
        tipoAgua VARCHAR(20) NOT NULL,
        usuarioId INT NOT NULL,
        CONSTRAINT FK_Aquarios_Usuarios FOREIGN KEY (usuarioId) REFERENCES Usuarios(id)
    )
END
GO

-- ==========================================
-- TABELA PEIXES (relaciona com Aquarios + foto)
-- ==========================================
IF OBJECT_ID('Peixes', 'U') IS NULL
BEGIN
    CREATE TABLE Peixes (
        id INT PRIMARY KEY IDENTITY(1,1),
        nome VARCHAR(100) NOT NULL,
        especie VARCHAR(100) NOT NULL,
        nomeCientifico VARCHAR(150) NULL,
        temperaturaIdeal DECIMAL(5,2) NULL,
        luminosidadeIdeal INT NULL,
        tamanhoCm DECIMAL(6,2) NOT NULL,
        aquarioId INT NOT NULL,
        foto VARCHAR(255) NULL,
        temperaturaMin DECIMAL(5,2) NULL,
        temperaturaMax DECIMAL(5,2) NULL,
        luminosidadeMin INT NULL,
        luminosidadeMax INT NULL,
        tdsPpmMin DECIMAL(10,2) NULL,
        tdsPpmMax DECIMAL(10,2) NULL,
        salinidadePptMin DECIMAL(10,3) NULL,
        salinidadePptMax DECIMAL(10,3) NULL,
        volumeMinLitros DECIMAL(10,2) NULL,
        originFromAI BIT NULL DEFAULT 0,
        parametersUpdatedAt DATETIME NULL,
        CONSTRAINT FK_Peixes_Aquarios FOREIGN KEY (aquarioId) REFERENCES Aquarios(id)
    )
END
GO

-- Compatibilidade: adiciona colunas se a tabela já existir
IF COL_LENGTH('Peixes', 'nomeCientifico') IS NULL
    ALTER TABLE Peixes ADD nomeCientifico VARCHAR(150) NULL
GO
IF COL_LENGTH('Peixes', 'temperaturaIdeal') IS NULL
    ALTER TABLE Peixes ADD temperaturaIdeal DECIMAL(5,2) NULL
GO
IF COL_LENGTH('Peixes', 'luminosidadeIdeal') IS NULL
    ALTER TABLE Peixes ADD luminosidadeIdeal INT NULL
GO
IF COL_LENGTH('Peixes', 'temperaturaMin') IS NULL
    ALTER TABLE Peixes ADD temperaturaMin DECIMAL(5,2) NULL
GO
IF COL_LENGTH('Peixes', 'temperaturaMax') IS NULL
    ALTER TABLE Peixes ADD temperaturaMax DECIMAL(5,2) NULL
GO
IF COL_LENGTH('Peixes', 'luminosidadeMin') IS NULL
    ALTER TABLE Peixes ADD luminosidadeMin INT NULL
GO
IF COL_LENGTH('Peixes', 'luminosidadeMax') IS NULL
    ALTER TABLE Peixes ADD luminosidadeMax INT NULL
GO
IF COL_LENGTH('Peixes', 'tdsPpmMin') IS NULL
    ALTER TABLE Peixes ADD tdsPpmMin DECIMAL(10,2) NULL
GO
IF COL_LENGTH('Peixes', 'tdsPpmMax') IS NULL
    ALTER TABLE Peixes ADD tdsPpmMax DECIMAL(10,2) NULL
GO
IF COL_LENGTH('Peixes', 'salinidadePptMin') IS NULL
    ALTER TABLE Peixes ADD salinidadePptMin DECIMAL(10,3) NULL
GO
IF COL_LENGTH('Peixes', 'salinidadePptMax') IS NULL
    ALTER TABLE Peixes ADD salinidadePptMax DECIMAL(10,3) NULL
GO
IF COL_LENGTH('Peixes', 'volumeMinLitros') IS NULL
    ALTER TABLE Peixes ADD volumeMinLitros DECIMAL(10,2) NULL
GO
IF COL_LENGTH('Peixes', 'originFromAI') IS NULL
    ALTER TABLE Peixes ADD originFromAI BIT NULL DEFAULT 0
GO
IF COL_LENGTH('Peixes', 'parametersUpdatedAt') IS NULL
    ALTER TABLE Peixes ADD parametersUpdatedAt DATETIME NULL
GO

-- ==========================================
-- TABELA LAMP CONFIG (relaciona com Aquarios)
-- ==========================================
IF OBJECT_ID('LampConfigs', 'U') IS NULL
BEGIN
    CREATE TABLE LampConfigs (
        aquarioId INT NOT NULL PRIMARY KEY,
        modo INT NOT NULL DEFAULT 4,
        brilho INT NOT NULL DEFAULT 80,
        r INT NOT NULL DEFAULT 255,
        g INT NOT NULL DEFAULT 255,
        b INT NOT NULL DEFAULT 255,
        luzAlvo INT NULL,
        tempAlvo DECIMAL(5,2) NULL,
        atualizadoEm DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_LampConfigs_Aquarios FOREIGN KEY (aquarioId) REFERENCES Aquarios(id)
    )
END
GO

-- ==========================================
-- TABELA LEITURAS SENSOR IoT (relaciona com Aquarios)
-- ==========================================
IF OBJECT_ID('LeiturasSensor', 'U') IS NULL
BEGIN
    CREATE TABLE LeiturasSensor (
        id INT PRIMARY KEY IDENTITY(1,1),
        aquarioId INT NOT NULL,
        temperatura DECIMAL(5,2) NOT NULL,
        nivelAgua DECIMAL(5,2) NOT NULL,
        tdsPpm DECIMAL(10,2) NULL,
        salinidadePpt DECIMAL(10,3) NULL,
        qualidadeTds VARCHAR(100) NULL,
        dataLeitura DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Leituras_Aquarios FOREIGN KEY (aquarioId) REFERENCES Aquarios(id)
    )
END
GO

-- Dados de exemplo para demonstração IoT
IF NOT EXISTS (SELECT 1 FROM LeiturasSensor)
BEGIN
    IF EXISTS (SELECT 1 FROM Aquarios)
    BEGIN
        INSERT INTO LeiturasSensor (aquarioId, temperatura, nivelAgua, tdsPpm, salinidadePpt, qualidadeTds, dataLeitura)
        SELECT TOP 1 id, 25.5, 85.0, 120.0, 0.350, 'Boa', GETDATE() FROM Aquarios
    END
END
GO

-- ==========================================
-- STORED PROCEDURES GENÉRICAS (PadraoDAO)
-- ==========================================

CREATE OR ALTER PROCEDURE spListagem
    @tabela NVARCHAR(100),
    @Ordem INT
AS
BEGIN
    IF @tabela = 'Usuarios'
    BEGIN
        SELECT id, nome, login, senha FROM Usuarios ORDER BY id
    END
    ELSE IF @tabela = 'Aquarios'
    BEGIN
        SELECT a.id, a.nome, a.capacidadeLitros, a.tipoAgua, a.usuarioId,
               u.nome AS nomeUsuario
        FROM Aquarios a
        INNER JOIN Usuarios u ON a.usuarioId = u.id
        ORDER BY a.id
    END
    ELSE IF @tabela = 'Peixes'
    BEGIN
        SELECT p.id, p.nome, p.especie, p.nomeCientifico, p.temperaturaIdeal, p.temperaturaMin, p.temperaturaMax,
               p.luminosidadeIdeal, p.luminosidadeMin, p.luminosidadeMax,
               p.tdsPpmMin, p.tdsPpmMax, p.salinidadePptMin, p.salinidadePptMax, p.volumeMinLitros,
               p.tamanhoCm, p.aquarioId, p.foto,
               aq.nome AS nomeAquario
        FROM Peixes p
        INNER JOIN Aquarios aq ON p.aquarioId = aq.id
        ORDER BY p.id
    END
    ELSE IF @tabela = 'LeiturasSensor'
    BEGIN
        SELECT l.id, l.aquarioId, l.temperatura, l.nivelAgua, l.dataLeitura,
               aq.nome AS nomeAquario
        FROM LeiturasSensor l
        INNER JOIN Aquarios aq ON l.aquarioId = aq.id
        ORDER BY l.dataLeitura DESC
    END
END
GO

CREATE OR ALTER PROCEDURE spProximoId
    @tabela NVARCHAR(100)
AS
BEGIN
    IF @tabela = 'Usuarios'
        SELECT ISNULL(MAX(id), 0) + 1 FROM Usuarios
    ELSE IF @tabela = 'Aquarios'
        SELECT ISNULL(MAX(id), 0) + 1 FROM Aquarios
    ELSE IF @tabela = 'Peixes'
        SELECT ISNULL(MAX(id), 0) + 1 FROM Peixes
    ELSE IF @tabela = 'LeiturasSensor'
        SELECT ISNULL(MAX(id), 0) + 1 FROM LeiturasSensor
END
GO

CREATE OR ALTER PROCEDURE spConsulta
    @id INT,
    @tabela NVARCHAR(100)
AS
BEGIN
    IF @tabela = 'Usuarios'
        SELECT id, nome, login, senha FROM Usuarios WHERE id = @id
    ELSE IF @tabela = 'Aquarios'
        SELECT a.id, a.nome, a.capacidadeLitros, a.tipoAgua, a.usuarioId,
               u.nome AS nomeUsuario
        FROM Aquarios a
        INNER JOIN Usuarios u ON a.usuarioId = u.id
        WHERE a.id = @id
    ELSE IF @tabela = 'Peixes'
           SELECT p.id, p.nome, p.especie, p.nomeCientifico, p.temperaturaIdeal, p.temperaturaMin, p.temperaturaMax,
                p.luminosidadeIdeal, p.luminosidadeMin, p.luminosidadeMax,
                p.tdsPpmMin, p.tdsPpmMax, p.salinidadePptMin, p.salinidadePptMax, p.volumeMinLitros,
                p.tamanhoCm, p.aquarioId, p.foto,
                aq.nome AS nomeAquario
        FROM Peixes p
        INNER JOIN Aquarios aq ON p.aquarioId = aq.id
        WHERE p.id = @id
    ELSE IF @tabela = 'LeiturasSensor'
        SELECT l.id, l.aquarioId, l.temperatura, l.nivelAgua, l.dataLeitura,
               aq.nome AS nomeAquario
        FROM LeiturasSensor l
        INNER JOIN Aquarios aq ON l.aquarioId = aq.id
        WHERE l.id = @id
END
GO

CREATE OR ALTER PROCEDURE spDelete
    @id INT,
    @tabela NVARCHAR(100)
AS
BEGIN
    IF @tabela = 'Usuarios'
    BEGIN
        DELETE FROM LeiturasSensor WHERE aquarioId IN (SELECT id FROM Aquarios WHERE usuarioId = @id)
        DELETE FROM Peixes WHERE aquarioId IN (SELECT id FROM Aquarios WHERE usuarioId = @id)
        DELETE FROM Aquarios WHERE usuarioId = @id
        DELETE FROM Usuarios WHERE id = @id
    END
    ELSE IF @tabela = 'Aquarios'
    BEGIN
        DELETE FROM LeiturasSensor WHERE aquarioId = @id
        DELETE FROM Peixes WHERE aquarioId = @id
        DELETE FROM Aquarios WHERE id = @id
    END
    ELSE IF @tabela = 'Peixes'
        DELETE FROM Peixes WHERE id = @id
    ELSE IF @tabela = 'LeiturasSensor'
        DELETE FROM LeiturasSensor WHERE id = @id
END
GO

-- ==========================================
-- STORED PROCEDURES - AQUARIOS
-- ==========================================
CREATE OR ALTER PROCEDURE spInsert_Aquarios
    @id INT,
    @nome VARCHAR(100),
    @capacidadeLitros DECIMAL(10,2),
    @tipoAgua VARCHAR(20),
    @usuarioId INT
AS
BEGIN
    SET IDENTITY_INSERT Aquarios ON
    INSERT INTO Aquarios (id, nome, capacidadeLitros, tipoAgua, usuarioId)
    VALUES (@id, @nome, @capacidadeLitros, @tipoAgua, @usuarioId)
    SET IDENTITY_INSERT Aquarios OFF
END
GO

CREATE OR ALTER PROCEDURE spUpdate_Aquarios
    @id INT,
    @nome VARCHAR(100),
    @capacidadeLitros DECIMAL(10,2),
    @tipoAgua VARCHAR(20),
    @usuarioId INT
AS
BEGIN
    UPDATE Aquarios
    SET nome = @nome, capacidadeLitros = @capacidadeLitros,
        tipoAgua = @tipoAgua, usuarioId = @usuarioId
    WHERE id = @id
END
GO

-- ==========================================
-- STORED PROCEDURES - PEIXES
-- ==========================================
CREATE OR ALTER PROCEDURE spInsert_Peixes
    @id INT,
    @nome VARCHAR(100),
    @especie VARCHAR(100),
    @nomeCientifico VARCHAR(150) = NULL,
    @temperaturaIdeal DECIMAL(5,2) = NULL,
    @temperaturaMin DECIMAL(5,2) = NULL,
    @temperaturaMax DECIMAL(5,2) = NULL,
    @luminosidadeIdeal INT = NULL,
    @luminosidadeMin INT = NULL,
    @luminosidadeMax INT = NULL,
    @tdsPpmMin DECIMAL(10,2) = NULL,
    @tdsPpmMax DECIMAL(10,2) = NULL,
    @salinidadePptMin DECIMAL(10,3) = NULL,
    @salinidadePptMax DECIMAL(10,3) = NULL,
    @volumeMinLitros DECIMAL(10,2) = NULL,
    @originFromAI BIT = NULL,
    @parametersUpdatedAt DATETIME = NULL,
    @tamanhoCm DECIMAL(6,2),
    @aquarioId INT,
    @foto VARCHAR(255)
AS
BEGIN
    SET IDENTITY_INSERT Peixes ON
    INSERT INTO Peixes (id, nome, especie, nomeCientifico, temperaturaIdeal, temperaturaMin, temperaturaMax,
                        luminosidadeIdeal, luminosidadeMin, luminosidadeMax,
                        tdsPpmMin, tdsPpmMax, salinidadePptMin, salinidadePptMax, volumeMinLitros,
                        originFromAI, parametersUpdatedAt, tamanhoCm, aquarioId, foto)
    VALUES (@id, @nome, @especie, @nomeCientifico, @temperaturaIdeal, @temperaturaMin, @temperaturaMax,
            @luminosidadeIdeal, @luminosidadeMin, @luminosidadeMax,
            @tdsPpmMin, @tdsPpmMax, @salinidadePptMin, @salinidadePptMax, @volumeMinLitros,
            @originFromAI, @parametersUpdatedAt, @tamanhoCm, @aquarioId, @foto)
    SET IDENTITY_INSERT Peixes OFF
END
GO

CREATE OR ALTER PROCEDURE spUpdate_Peixes
    @id INT,
    @nome VARCHAR(100),
    @especie VARCHAR(100),
    @nomeCientifico VARCHAR(150) = NULL,
    @temperaturaIdeal DECIMAL(5,2) = NULL,
    @temperaturaMin DECIMAL(5,2) = NULL,
    @temperaturaMax DECIMAL(5,2) = NULL,
    @luminosidadeIdeal INT = NULL,
    @luminosidadeMin INT = NULL,
    @luminosidadeMax INT = NULL,
    @tdsPpmMin DECIMAL(10,2) = NULL,
    @tdsPpmMax DECIMAL(10,2) = NULL,
    @salinidadePptMin DECIMAL(10,3) = NULL,
    @salinidadePptMax DECIMAL(10,3) = NULL,
    @volumeMinLitros DECIMAL(10,2) = NULL,
    @originFromAI BIT = NULL,
    @parametersUpdatedAt DATETIME = NULL,
    @tamanhoCm DECIMAL(6,2),
    @aquarioId INT,
    @foto VARCHAR(255)
AS
BEGIN
    UPDATE Peixes
    SET nome = @nome,
        especie = @especie,
        nomeCientifico = @nomeCientifico,
        temperaturaIdeal = @temperaturaIdeal,
        temperaturaMin = @temperaturaMin,
        temperaturaMax = @temperaturaMax,
        luminosidadeIdeal = @luminosidadeIdeal,
        luminosidadeMin = @luminosidadeMin,
        luminosidadeMax = @luminosidadeMax,
        tdsPpmMin = @tdsPpmMin,
        tdsPpmMax = @tdsPpmMax,
        salinidadePptMin = @salinidadePptMin,
        salinidadePptMax = @salinidadePptMax,
        volumeMinLitros = @volumeMinLitros,
        originFromAI = @originFromAI,
        parametersUpdatedAt = @parametersUpdatedAt,
        tamanhoCm = @tamanhoCm,
        aquarioId = @aquarioId,
        foto = @foto
    WHERE id = @id
END
GO

-- ==========================================
-- STORED PROCEDURES - USUARIO
-- ==========================================
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

CREATE OR ALTER PROCEDURE spConsultaUsuario
    @id INT
AS
BEGIN
    SELECT id, nome, login, senha FROM Usuarios WHERE id = @id
END
GO

CREATE OR ALTER PROCEDURE spConsultaUsuarioPorLogin
    @login VARCHAR(50)
AS
BEGIN
    SELECT id, nome, login, senha FROM Usuarios WHERE login = @login
END
GO

-- ==========================================
-- CONSULTAS COM FILTROS (Dados Gerenciais)
-- ==========================================
CREATE OR ALTER PROCEDURE spConsultaPeixesFiltro
    @nome VARCHAR(100) = NULL,
    @especie VARCHAR(100) = NULL,
    @aquarioId INT = NULL
AS
BEGIN
    SELECT p.id, p.nome, p.especie, p.nomeCientifico, p.temperaturaIdeal, p.temperaturaMin, p.temperaturaMax, p.luminosidadeIdeal, p.luminosidadeMin, p.luminosidadeMax, p.tdsPpmMin, p.tdsPpmMax, p.salinidadePptMin, p.salinidadePptMax, p.volumeMinLitros,
            p.tamanhoCm, p.aquarioId, p.foto,
            aq.nome AS nomeAquario
    FROM Peixes p
    INNER JOIN Aquarios aq ON p.aquarioId = aq.id
    WHERE (@nome IS NULL OR @nome = '' OR p.nome LIKE '%' + @nome + '%')
      AND (@especie IS NULL OR @especie = '' OR p.especie LIKE '%' + @especie + '%')
      AND (@aquarioId IS NULL OR @aquarioId = 0 OR p.aquarioId = @aquarioId)
    ORDER BY p.nome
END
GO

CREATE OR ALTER PROCEDURE spConsultaLeiturasFiltro
    @aquarioId INT = NULL,
    @dataInicio DATETIME = NULL,
    @dataFim DATETIME = NULL,
    @temperaturaMin DECIMAL(5,2) = NULL,
    @temperaturaMax DECIMAL(5,2) = NULL
AS
BEGIN
        SELECT l.id, l.aquarioId, l.temperatura, l.nivelAgua, l.tdsPpm, l.salinidadePpt, l.qualidadeTds, l.dataLeitura,
           aq.nome AS nomeAquario
    FROM LeiturasSensor l
    INNER JOIN Aquarios aq ON l.aquarioId = aq.id
    WHERE (@aquarioId IS NULL OR @aquarioId = 0 OR l.aquarioId = @aquarioId)
      AND (@dataInicio IS NULL OR l.dataLeitura >= @dataInicio)
      AND (@dataFim IS NULL OR l.dataLeitura <= @dataFim)
      AND (@temperaturaMin IS NULL OR l.temperatura >= @temperaturaMin)
      AND (@temperaturaMax IS NULL OR l.temperatura <= @temperaturaMax)
    ORDER BY l.dataLeitura DESC
END
GO

CREATE OR ALTER PROCEDURE spDashboardLeituras
    @aquarioId INT = NULL,
    @dataInicio DATETIME = NULL,
    @dataFim DATETIME = NULL
AS
BEGIN
        SELECT l.id, l.aquarioId, l.temperatura, l.nivelAgua, l.tdsPpm, l.salinidadePpt, l.qualidadeTds, l.dataLeitura,
           aq.nome AS nomeAquario
    FROM LeiturasSensor l
    INNER JOIN Aquarios aq ON l.aquarioId = aq.id
    WHERE (@aquarioId IS NULL OR @aquarioId = 0 OR l.aquarioId = @aquarioId)
      AND (@dataInicio IS NULL OR l.dataLeitura >= @dataInicio)
      AND (@dataFim IS NULL OR l.dataLeitura <= @dataFim)
    ORDER BY l.dataLeitura DESC
END
GO

CREATE OR ALTER PROCEDURE spInserirLeituraSensor
    @aquarioId INT,
    @temperatura DECIMAL(5,2),
    @nivelAgua DECIMAL(5,2),
    @tdsPpm DECIMAL(10,2) = NULL,
    @salinidadePpt DECIMAL(10,3) = NULL,
    @qualidadeTds VARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO LeiturasSensor (aquarioId, temperatura, nivelAgua, tdsPpm, salinidadePpt, qualidadeTds, dataLeitura)
    VALUES (@aquarioId, @temperatura, @nivelAgua, @tdsPpm, @salinidadePpt, @qualidadeTds, GETDATE())
END
GO

-- ==========================================
-- STORED PROCEDURES - SmartLamp Config
-- ==========================================

CREATE OR ALTER PROCEDURE spConsultaLampConfig
    @aquarioId INT
AS
BEGIN
    SELECT lc.aquarioId, aq.nome AS nomeAquario,
           lc.modo, lc.brilho, lc.r, lc.g, lc.b,
           lc.luzAlvo, lc.tempAlvo, lc.atualizadoEm
    FROM LampConfigs lc
    INNER JOIN Aquarios aq ON aq.id = lc.aquarioId
    WHERE lc.aquarioId = @aquarioId
END
GO

CREATE OR ALTER PROCEDURE spSalvarLampConfig
    @aquarioId INT,
    @modo INT,
    @brilho INT,
    @r INT,
    @g INT,
    @b INT,
    @luzAlvo INT = NULL,
    @tempAlvo DECIMAL(5,2) = NULL
AS
BEGIN
    IF EXISTS (SELECT 1 FROM LampConfigs WHERE aquarioId = @aquarioId)
    BEGIN
        UPDATE LampConfigs
        SET modo = @modo, brilho = @brilho, r = @r, g = @g, b = @b,
            luzAlvo = @luzAlvo, tempAlvo = @tempAlvo, atualizadoEm = GETDATE()
        WHERE aquarioId = @aquarioId
    END
    ELSE
    BEGIN
        INSERT INTO LampConfigs (aquarioId, modo, brilho, r, g, b, luzAlvo, tempAlvo)
        VALUES (@aquarioId, @modo, @brilho, @r, @g, @b, @luzAlvo, @tempAlvo)
    END
END
GO

CREATE OR ALTER PROCEDURE spAplicarAlvosLamp
    @aquarioId INT,
    @luzAlvo INT = NULL,
    @tempAlvo DECIMAL(5,2) = NULL
AS
BEGIN
    IF EXISTS (SELECT 1 FROM LampConfigs WHERE aquarioId = @aquarioId)
    BEGIN
        UPDATE LampConfigs 
        SET luzAlvo = @luzAlvo, tempAlvo = @tempAlvo, atualizadoEm = GETDATE() 
        WHERE aquarioId = @aquarioId
    END
    ELSE
    BEGIN
        INSERT INTO LampConfigs (aquarioId, luzAlvo, tempAlvo) 
        VALUES (@aquarioId, @luzAlvo, @tempAlvo)
    END
END
GO
