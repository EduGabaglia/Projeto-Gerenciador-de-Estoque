-- Recalcula os itens de pedidos gravados com o preço de tabela antes de a API
-- passar a aplicar o desconto da promoção ativa no momento da criação/edição.
-- Só considera pedidos com data igual ou posterior ao cadastro da promoção.
-- Execute uma vez no banco EstoqueDB.

SET XACT_ABORT ON;

BEGIN TRANSACTION;

CREATE TABLE #PedidosAfetados
(
    PedidoId INT NOT NULL CONSTRAINT PK_PedidosAfetados PRIMARY KEY
);

INSERT INTO #PedidosAfetados (PedidoId)
SELECT DISTINCT pi.PedidoId
FROM dbo.PedidoItem pi
        INNER JOIN dbo.Pedido ped ON ped.Id = pi.PedidoId
        INNER JOIN dbo.Produto produto ON produto.Id = pi.ProdutoId
        CROSS APPLY (
            SELECT TOP (1)
                promo.DescontoPercentual,
                promo.DataCadastro
            FROM dbo.PromocaoItem item
                    INNER JOIN dbo.Promocao promo ON promo.Id = item.PromocaoId
            WHERE item.ProdutoId = pi.ProdutoId
              AND promo.Ativa = 1
            ORDER BY promo.Id
        ) promocao
WHERE CONVERT(date, ped.Data) >= CONVERT(date, promocao.DataCadastro)
  AND pi.PrecoUnitario <> CAST(
        ROUND(produto.Preco * (1 - promocao.DescontoPercentual / 100.0), 2)
        AS decimal(10, 2));

UPDATE pi
SET pi.PrecoUnitario = CAST(
        ROUND(produto.Preco * (1 - promocao.DescontoPercentual / 100.0), 2)
        AS decimal(10, 2))
FROM dbo.PedidoItem pi
        INNER JOIN #PedidosAfetados afetado ON afetado.PedidoId = pi.PedidoId
        INNER JOIN dbo.Pedido ped ON ped.Id = pi.PedidoId
        INNER JOIN dbo.Produto produto ON produto.Id = pi.ProdutoId
        CROSS APPLY (
            SELECT TOP (1)
                promo.DescontoPercentual,
                promo.DataCadastro
            FROM dbo.PromocaoItem item
                    INNER JOIN dbo.Promocao promo ON promo.Id = item.PromocaoId
            WHERE item.ProdutoId = pi.ProdutoId
              AND promo.Ativa = 1
            ORDER BY promo.Id
        ) promocao
WHERE CONVERT(date, ped.Data) >= CONVERT(date, promocao.DataCadastro);

UPDATE ped
SET ped.ValorTotal = totais.ValorTotal
FROM dbo.Pedido ped
        INNER JOIN #PedidosAfetados afetado ON afetado.PedidoId = ped.Id
        CROSS APPLY (
            SELECT CAST(
                    SUM(CAST(pi.Quantidade AS decimal(12, 2)) * pi.PrecoUnitario)
                    AS decimal(12, 2)) AS ValorTotal
            FROM dbo.PedidoItem pi
            WHERE pi.PedidoId = ped.Id
        ) totais;

DROP TABLE #PedidosAfetados;

COMMIT TRANSACTION;
