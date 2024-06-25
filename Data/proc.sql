CREATE PROCEDURE AddProductToWarehouse(
    IN IdProduct INT,
    IN IdWarehouse INT,
    IN Amount INT,
    IN CreatedAt DATETIME
)
BEGIN
    DECLARE IdProductFromDb INT;
    DECLARE IdOrder INT;
    DECLARE Price DECIMAL(25,2);

    -- Check for the existence of the product and the order
    SELECT o.IdOrder INTO IdOrder
    FROM `Order` o
             LEFT JOIN Product_Warehouse pw ON o.IdOrder = pw.IdOrder
    WHERE o.IdProduct = IdProduct AND o.Amount = Amount AND pw.IdProductWarehouse IS NULL AND o.CreatedAt < CreatedAt
    LIMIT 1;

    IF IdOrder IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invalid parameter: There is no order to fulfill';
    END IF;

    SELECT IdProduct, Price INTO IdProductFromDb, Price
    FROM Product
    WHERE IdProduct = IdProduct;

    IF IdProductFromDb IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invalid parameter: Provided IdProduct does not exist';
    END IF;

    -- Check for the existence of the warehouse
    IF NOT EXISTS (SELECT 1 FROM Warehouse WHERE IdWarehouse = IdWarehouse) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invalid parameter: Provided IdWarehouse does not exist';
    END IF;

    -- Start transaction
    START TRANSACTION;

    -- Update the order's FulfilledAt date
    UPDATE `Order` SET FulfilledAt = CreatedAt WHERE IdOrder = IdOrder;

    -- Insert into Product_Warehouse
    INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
    VALUES (IdWarehouse, IdProduct, IdOrder, Amount, Amount * Price, CreatedAt);

    -- Return the new Product_Warehouse ID
    SELECT LAST_INSERT_ID() AS NewId;

    -- Commit the transaction
    COMMIT;
END;
