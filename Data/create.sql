-- Drop tables if they exist
DROP TABLE IF EXISTS Product_Warehouse;
DROP TABLE IF EXISTS `Order`;
DROP TABLE IF EXISTS Product;
DROP TABLE IF EXISTS Warehouse;

-- Create tables
CREATE TABLE `Order` (
                         IdOrder int AUTO_INCREMENT PRIMARY KEY,
                         IdProduct int NOT NULL,
                         Amount int NOT NULL,
                         CreatedAt datetime NOT NULL,
                         FulfilledAt datetime NULL
);

CREATE TABLE Product (
                         IdProduct int AUTO_INCREMENT PRIMARY KEY,
                         Name varchar(200) NOT NULL,
                         Description varchar(200) NOT NULL,
                         Price decimal(25,2) NOT NULL
);

CREATE TABLE Product_Warehouse (
                                   IdProductWarehouse int AUTO_INCREMENT PRIMARY KEY,
                                   IdWarehouse int NOT NULL,
                                   IdProduct int NOT NULL,
                                   IdOrder int NOT NULL,
                                   Amount int NOT NULL,
                                   Price decimal(25,2) NOT NULL,
                                   CreatedAt datetime NOT NULL
);

CREATE TABLE Warehouse (
                           IdWarehouse int AUTO_INCREMENT PRIMARY KEY,
                           Name varchar(200) NOT NULL,
                           Address varchar(200) NOT NULL
);

-- Foreign keys
ALTER TABLE Product_Warehouse ADD CONSTRAINT FK_Product_Warehouse_Order
    FOREIGN KEY (IdOrder) REFERENCES `Order` (IdOrder);

ALTER TABLE `Order` ADD CONSTRAINT FK_Order_Product
    FOREIGN KEY (IdProduct) REFERENCES Product (IdProduct);

ALTER TABLE Product_Warehouse ADD CONSTRAINT FK_Product_Warehouse_Product
    FOREIGN KEY (IdProduct) REFERENCES Product (IdProduct);

ALTER TABLE Product_Warehouse ADD CONSTRAINT FK_Product_Warehouse_Warehouse
    FOREIGN KEY (IdWarehouse) REFERENCES Warehouse (IdWarehouse);

-- Insert initial data
INSERT INTO Warehouse (Name, Address) VALUES ('Warsaw', 'Kwiatowa 12');

INSERT INTO Product (Name, Description, Price) VALUES
                                                   ('Abacavir', '', 25.5),
                                                   ('Acyclovir', '', 45.0),
                                                   ('Allopurinol', '', 30.8);

-- Poprawienie zapytania, aby unikać podzapytań zwracających więcej niż jeden wiersz
INSERT INTO `Order` (IdProduct, Amount, CreatedAt)
SELECT IdProduct, 125, NOW()
FROM Product
WHERE Name='Abacavir'
LIMIT 1;
