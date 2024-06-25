DROP PROCEDURE IF EXISTS AddProductToWarehouse;

CREATE PROCEDURE AddProductToWarehouse(
    IN IdProduct INT,
    IN IdWarehouse INT,
    IN Amount INT,
    IN CreatedAt DATETIME
)
BEGIN
    DECLARE IdOrder INT;
    DECLARE Price DECIMAL(25,2);

    -- Sprawdzenie czy produkt istnieje
    SELECT Price INTO Price
    FROM Product
    WHERE IdProduct = IdProduct;

    IF Price IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invalid parameter: Provided IdProduct does not exist';
    END IF;

    -- Sprawdzenie czy istnieje zamówienie do zrealizowania
    SELECT IdOrder INTO IdOrder
    FROM `Order`
    WHERE IdProduct = IdProduct AND Amount = Amount AND CreatedAt < CreatedAt AND FulfilledAt IS NULL
    LIMIT 1;

    IF IdOrder IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invalid parameter: There is no order to fulfill';
    END IF;

    -- Sprawdzenie czy magazyn istnieje
    IF NOT EXISTS (SELECT 1 FROM Warehouse WHERE IdWarehouse = IdWarehouse) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invalid parameter: Provided IdWarehouse does not exist';
    END IF;

    -- Rozpoczęcie transakcji
    START TRANSACTION;

    -- Aktualizacja zamówienia
    UPDATE `Order`
    SET FulfilledAt = CreatedAt
    WHERE IdOrder = IdOrder;

    -- Dodanie produktu do magazynu
    INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
    VALUES (IdWarehouse, IdProduct, IdOrder, Amount, Price * Amount, CreatedAt);

    -- Zakończenie transakcji
    COMMIT;

    SELECT LAST_INSERT_ID() AS NewId;
END;
