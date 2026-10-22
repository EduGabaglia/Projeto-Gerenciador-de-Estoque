/* ============================================================================
   001_criar_tabelas_promocao.sql

   Complementa o schema existente do banco EstoqueDB (somee.com) com as tabelas
   do módulo de Promoções, que ainda não existiam no banco.

   Convenções seguidas do schema original:
     - Id INT IDENTITY como chave primária;
     - nomes de constraints no padrão PK_/UQ_/FK_/CK_/DF_/IX_ das tabelas existentes;
     - Ativo/Ativa com DEFAULT (1) e CHECK de faixas onde aplicável.

   Executado por: nelson_machado_SQLLogin_1 (db_owner)
   ============================================================================ */

IF OBJECT_ID(N'dbo.Promocao', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Promocao
    (
        Id                 INT IDENTITY(1,1) NOT NULL,
        Nome               NVARCHAR(150)     NOT NULL,
        DescontoPercentual DECIMAL(5,2)      NOT NULL,
        Ativa              BIT               NOT NULL CONSTRAINT DF_Promocao_Ativa DEFAULT (1),
        DataCadastro       DATETIME2         NOT NULL CONSTRAINT DF_Promocao_DataCadastro DEFAULT (GETDATE()),
        CONSTRAINT PK_Promocao PRIMARY KEY (Id),
        CONSTRAINT UQ_Promocao_Nome UNIQUE (Nome),
        CONSTRAINT CK_Promocao_Desconto CHECK (DescontoPercentual > 0 AND DescontoPercentual < 100)
    );
END;
GO

IF OBJECT_ID(N'dbo.PromocaoItem', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PromocaoItem
    (
        Id          INT IDENTITY(1,1) NOT NULL,
        PromocaoId  INT               NOT NULL,
        ProdutoId   INT               NOT NULL,
        PrecoTabela DECIMAL(9,2)      NOT NULL,
        CONSTRAINT PK_PromocaoItem PRIMARY KEY (Id),
        CONSTRAINT UQ_PromocaoItem_Promocao_Produto UNIQUE (PromocaoId, ProdutoId),
        CONSTRAINT FK_PromocaoItem_Promocao FOREIGN KEY (PromocaoId) REFERENCES dbo.Promocao (Id),
        CONSTRAINT FK_PromocaoItem_Produto FOREIGN KEY (ProdutoId) REFERENCES dbo.Produto (Id),
        CONSTRAINT CK_PromocaoItem_PrecoTabela CHECK (PrecoTabela >= 0)
    );

    CREATE INDEX IX_PromocaoItem_ProdutoId ON dbo.PromocaoItem (ProdutoId);
END;
GO
