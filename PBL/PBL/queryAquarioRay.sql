-- 1. CRIAR O BANCO DE DADOS
CREATE DATABASE AquarioIA;
GO
USE AquarioIA;
GO

-- 2. TABELA DE USUÁRIOS
CREATE TABLE usuarios (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    nome NVARCHAR(100) NOT NULL,
    email NVARCHAR(100) UNIQUE NOT NULL,
    senha_hash NVARCHAR(MAX) NOT NULL,
    criado_em DATETIME DEFAULT GETDATE()
);

-- 3. TABELA DE ESPÉCIES (A ser preenchida automaticamente pelo Gemini)
CREATE TABLE especies (
    id INT PRIMARY KEY IDENTITY(1,1),
    nome_popular NVARCHAR(100) NOT NULL,
    nome_cientifico NVARCHAR(100),
    temp_min DECIMAL(4,2),
    temp_max DECIMAL(4,2),
    ph_min DECIMAL(3,1),
    ph_max DECIMAL(3,1),
    nivel_dificuldade NVARCHAR(20),
    dicas_ia NVARCHAR(MAX), -- Dicas de cuidados geradas pela IA
    ultima_atualizacao_ia DATETIME DEFAULT GETDATE()
);

-- 4. TABELA DE AQUÁRIOS
CREATE TABLE aquarios (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    usuario_id UNIQUEIDENTIFIER NOT NULL,
    nome NVARCHAR(100) NOT NULL,
    volume_litros INT,
    tipo_agua NVARCHAR(20) DEFAULT 'doce',
    criado_em DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Aquario_Usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- 5. RELACIONAMENTO AQUÁRIO X FAUNA (O que tem dentro do aquário)
CREATE TABLE aquario_fauna (
    id INT PRIMARY KEY IDENTITY(1,1),
    aquario_id UNIQUEIDENTIFIER NOT NULL,
    especie_id INT NOT NULL,
    quantidade INT DEFAULT 1,
    data_adicao DATE DEFAULT GETDATE(),
    CONSTRAINT FK_Fauna_Aquario FOREIGN KEY (aquario_id) REFERENCES aquarios(id) ON DELETE CASCADE,
    CONSTRAINT FK_Fauna_Especie FOREIGN KEY (especie_id) REFERENCES especies(id)
);

-- 6. TELEMETRIA (Dados brutos dos sensores IoT)
CREATE TABLE telemetria (
    id BIGINT PRIMARY KEY IDENTITY(1,1),
    aquario_id UNIQUEIDENTIFIER NOT NULL,
    temperatura DECIMAL(4,2),
    ph DECIMAL(3,1),
    amonia DECIMAL(4,2),
    timestamp DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Telemetria_Aquario FOREIGN KEY (aquario_id) REFERENCES aquarios(id) ON DELETE CASCADE
);

-- 7. ANÁLISES DA IA (Insights gerados pelo Gemini)
CREATE TABLE ia_analises (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    aquario_id UNIQUEIDENTIFIER NOT NULL,
    data_analise DATETIME DEFAULT GETDATE(),
    status_saude_geral NVARCHAR(50), -- Ex: 'Crítico', 'Estável', 'Ótimo'
    insight_texto NVARCHAR(MAX),     -- O diagnóstico da IA
    recomendacao_acao NVARCHAR(MAX), -- O que o dono deve fazer
    dados_brutos_json NVARCHAR(MAX), -- Resposta bruta da API Gemini (JSON)
    log_referencia_id BIGINT,        -- Link com o registro da telemetria que causou o insight
    CONSTRAINT FK_Analise_Aquario FOREIGN KEY (aquario_id) REFERENCES aquarios(id) ON DELETE CASCADE,
    CONSTRAINT FK_Analise_Log FOREIGN KEY (log_referencia_id) REFERENCES telemetria(id)
);

-- 8. NOTIFICAÇÕES (Alertas preditivos para o App)
CREATE TABLE notificacoes_ia (
    id INT PRIMARY KEY IDENTITY(1,1),
    usuario_id UNIQUEIDENTIFIER NOT NULL,
    analise_id UNIQUEIDENTIFIER,
    titulo NVARCHAR(100),
    mensagem NVARCHAR(MAX),
    prioridade INT DEFAULT 1, -- 1: Info, 2: Alerta, 3: Crítico
    lida BIT DEFAULT 0,       -- 0 para Não Lida, 1 para Lida
    enviado_em DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Notificacao_Usuario FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE,
    CONSTRAINT FK_Notificacao_Analise FOREIGN KEY (analise_id) REFERENCES ia_analises(id)
);
GO