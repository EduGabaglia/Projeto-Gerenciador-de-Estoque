-- Migra o fluxo de pedidos de entrega para retirada na loja.
-- Execute uma vez no banco EstoqueDB.

SET XACT_ABORT ON;

BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Pedido_Status'
      AND parent_object_id = OBJECT_ID('dbo.Pedido')
)
BEGIN
    ALTER TABLE dbo.Pedido DROP CONSTRAINT CK_Pedido_Status;
END;

UPDATE dbo.Pedido
SET Status = 'PRONTO_PARA_RETIRADA'
WHERE Status = 'ENVIADO';

UPDATE dbo.Pedido
SET Status = 'RETIRADO'
WHERE Status = 'ENTREGUE';

ALTER TABLE dbo.Pedido WITH CHECK
ADD CONSTRAINT CK_Pedido_Status CHECK (
    Status IN (
        'PENDENTE',
        'PAGO',
        'PROCESSANDO',
        'PRONTO_PARA_RETIRADA',
        'RETIRADO',
        'CANCELADO'
    )
);

ALTER TABLE dbo.Pedido CHECK CONSTRAINT CK_Pedido_Status;

COMMIT TRANSACTION;
