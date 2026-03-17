-- =============================================
-- AgInventory Database Setup Script
-- Project: Ag Inventory & Parts Tracker
-- Author: Nikhil
-- Date: 2026-03-17
-- =============================================

-- Create and use the database
CREATE DATABASE AgInventory
GO

USE AgInventory
GO

-- =============================================
-- TABLES
-- =============================================

CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name       VARCHAR(100) NOT NULL,
    IsActive   BIT NOT NULL DEFAULT 1
)

CREATE TABLE Suppliers (
    SupplierId  INT IDENTITY(1,1) PRIMARY KEY,
    Name        VARCHAR(200) NOT NULL,
    ContactName VARCHAR(100),
    Phone       VARCHAR(20),
    Email       VARCHAR(100),
    IsActive    BIT NOT NULL DEFAULT 1
)

CREATE TABLE Locations (
    LocationId INT IDENTITY(1,1) PRIMARY KEY,
    Name       VARCHAR(100) NOT NULL,
    IsActive   BIT NOT NULL DEFAULT 1
)

CREATE TABLE Users (
    UserId   INT IDENTITY(1,1) PRIMARY KEY,
    FullName VARCHAR(150) NOT NULL,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Role     VARCHAR(20) NOT NULL DEFAULT 'Technician',
    IsActive BIT NOT NULL DEFAULT 1
)

CREATE TABLE Parts (
    PartId       INT IDENTITY(1,1) PRIMARY KEY,
    SKU          VARCHAR(50) NOT NULL UNIQUE,
    Name         VARCHAR(200) NOT NULL,
    CategoryId   INT NOT NULL FOREIGN KEY REFERENCES Categories(CategoryId),
    SupplierId   INT NOT NULL FOREIGN KEY REFERENCES Suppliers(SupplierId),
    UnitCost     DECIMAL(10,2) NOT NULL DEFAULT 0,
    ReorderPoint INT NOT NULL DEFAULT 5,
    Notes        VARCHAR(500),
    IsActive     BIT NOT NULL DEFAULT 1
)

CREATE TABLE Stock (
    StockId     INT IDENTITY(1,1) PRIMARY KEY,
    PartId      INT NOT NULL FOREIGN KEY REFERENCES Parts(PartId),
    LocationId  INT NOT NULL FOREIGN KEY REFERENCES Locations(LocationId),
    QtyOnHand   INT NOT NULL DEFAULT 0,
    LastUpdated DATETIME NOT NULL DEFAULT GETDATE()
)

CREATE TABLE AuditLog (
    LogId      INT IDENTITY(1,1) PRIMARY KEY,
    PartId     INT NOT NULL FOREIGN KEY REFERENCES Parts(PartId),
    UserId     INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    ChangeType VARCHAR(30) NOT NULL,
    QtyDelta   INT NOT NULL,
    Reason     VARCHAR(300),
    CreatedAt  DATETIME NOT NULL DEFAULT GETDATE()
)

CREATE TABLE Checkouts (
    CheckoutId    INT IDENTITY(1,1) PRIMARY KEY,
    PartId        INT NOT NULL FOREIGN KEY REFERENCES Parts(PartId),
    UserId        INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
    WorkOrderNum  VARCHAR(50),
    QtyCheckedOut INT NOT NULL,
    CheckedOutAt  DATETIME NOT NULL DEFAULT GETDATE(),
    ReturnedAt    DATETIME NULL
)

CREATE TABLE PurchaseOrders (
    POId       INT IDENTITY(1,1) PRIMARY KEY,
    SupplierId INT NOT NULL FOREIGN KEY REFERENCES Suppliers(SupplierId),
    Status     VARCHAR(20) NOT NULL DEFAULT 'Draft',
    CreatedAt  DATETIME NOT NULL DEFAULT GETDATE(),
    ReceivedAt DATETIME NULL
)

CREATE TABLE PurchaseOrderItems (
    POItemId   INT IDENTITY(1,1) PRIMARY KEY,
    POId       INT NOT NULL FOREIGN KEY REFERENCES PurchaseOrders(POId),
    PartId     INT NOT NULL FOREIGN KEY REFERENCES Parts(PartId),
    QtyOrdered INT NOT NULL,
    UnitCost   DECIMAL(10,2) NOT NULL
)
GO

-- =============================================
-- SAMPLE DATA
-- =============================================

INSERT INTO Categories (Name) VALUES
('Filters'),
('Belts & Drives'),
('Hydraulics'),
('Electrical'),
('Fasteners')

INSERT INTO Suppliers (Name, ContactName, Phone, Email) VALUES
('Ag Parts Direct',   'Tom Hanson',  '715-555-0101', 'tom@agpartsdirect.com'),
('Farm Supply Co',    'Sarah Mills', '715-555-0202', 'sarah@farmsupply.com'),
('Midwest Equipment', 'Dave Larson', '715-555-0303', 'dave@midwestequip.com')

INSERT INTO Locations (Name) VALUES
('Main Shop'),
('Field Shed'),
('Parts Room')

INSERT INTO Users (FullName, Username, Role) VALUES
('Nikhil Admin', 'nikhil', 'Manager'),
('John Smith',   'jsmith', 'Technician'),
('Mike Brown',   'mbrown', 'Technician')

INSERT INTO Parts (SKU, Name, CategoryId, SupplierId, UnitCost, ReorderPoint) VALUES
('HF-220', 'Hydraulic Filter',    3, 1, 12.99, 10),
('DB-44A', 'Drive Belt 44A',      2, 2,  8.50,  5),
('GN-M6',  'Grease Nipple M6',    5, 3,  0.75, 20),
('EF-110', 'Engine Air Filter',   1, 1, 18.00,  8),
('HH-330', 'Hydraulic Hose 3/8"', 3, 2, 24.50,  4),
('SP-12V', 'Spark Plug 12V',      4, 3,  3.25, 15),
('OB-500', 'Oil Filter OB500',    1, 1,  9.99, 10),
('CB-200', 'Conveyor Belt 200cm', 2, 2, 65.00,  2)

INSERT INTO Stock (PartId, LocationId, QtyOnHand) VALUES
(1, 1,  2),
(2, 1,  4),
(3, 1, 45),
(4, 2,  8),
(5, 1,  6),
(6, 3, 30),
(7, 1,  3),
(8, 2,  1)
GO

-- =============================================
-- STORED PROCEDURES
-- =============================================

CREATE PROCEDURE usp_GetLowStockParts
AS
BEGIN
    SELECT
        p.PartId,
        p.SKU,
        p.Name        AS PartName,
        c.Name        AS Category,
        st.QtyOnHand,
        p.ReorderPoint,
        l.Name        AS Location
    FROM Parts p
    JOIN Categories c ON p.CategoryId  = c.CategoryId
    JOIN Stock st     ON p.PartId      = st.PartId
    JOIN Locations l  ON st.LocationId = l.LocationId
    WHERE st.QtyOnHand <= p.ReorderPoint
    AND   p.IsActive = 1
    ORDER BY st.QtyOnHand ASC
END
GO

CREATE PROCEDURE usp_AdjustStock
    @PartId     INT,
    @LocationId INT,
    @QtyDelta   INT,
    @UserId     INT,
    @ChangeType VARCHAR(30),
    @Reason     VARCHAR(300)
AS
BEGIN
    UPDATE Stock
    SET
        QtyOnHand   = QtyOnHand + @QtyDelta,
        LastUpdated = GETDATE()
    WHERE PartId     = @PartId
    AND   LocationId = @LocationId

    INSERT INTO AuditLog
        (PartId, UserId, ChangeType, QtyDelta, Reason)
    VALUES
        (@PartId, @UserId, @ChangeType, @QtyDelta, @Reason)
END
GO

CREATE PROCEDURE usp_ReceivePO
    @POId   INT,
    @UserId INT
AS
BEGIN
    DECLARE @PartId   INT
    DECLARE @Qty      INT
    DECLARE @Location INT = 1

    DECLARE po_cursor CURSOR FOR
        SELECT PartId, QtyOrdered
        FROM PurchaseOrderItems
        WHERE POId = @POId

    OPEN po_cursor
    FETCH NEXT FROM po_cursor INTO @PartId, @Qty

    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC usp_AdjustStock
            @PartId     = @PartId,
            @LocationId = @Location,
            @QtyDelta   = @Qty,
            @UserId     = @UserId,
            @ChangeType = 'PO_RECEIVE',
            @Reason     = 'Purchase order received'

        FETCH NEXT FROM po_cursor INTO @PartId, @Qty
    END

    CLOSE po_cursor
    DEALLOCATE po_cursor

    UPDATE PurchaseOrders
    SET Status     = 'Received',
        ReceivedAt = GETDATE()
    WHERE POId = @POId
END
GO