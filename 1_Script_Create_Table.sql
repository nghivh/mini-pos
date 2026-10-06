-- 1. Phân loại sản phẩm (EF Core quản lý)
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500)
);

-- 2. Danh mục sản phẩm (Master Data)
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CategoryId INT NOT NULL,
    Barcode NVARCHAR(50) UNIQUE, -- Dùng cho chức năng Scanner
    ProductName NVARCHAR(200) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL DEFAULT 0, -- Giá bán hiện tại
    CostPrice DECIMAL(18, 2) NOT NULL DEFAULT 0, -- Giá vốn để tính lợi nhuận
    StockQuantity INT NOT NULL DEFAULT 0,
    ImageUrl NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- 3. Khách hàng
CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(200) NOT NULL,
    PhoneNumber NVARCHAR(20) UNIQUE,
    LoyaltyPoints INT DEFAULT 0, -- Điểm tích lũy
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 4. Hóa đơn (Header - EF Core quản lý)
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    CustomerId INT NULL, -- Có thể bán cho khách vãng lai (null)
    TotalAmount DECIMAL(18, 2) NOT NULL,
    DiscountAmount DECIMAL(18, 2) DEFAULT 0,
    FinalAmount DECIMAL(18, 2) NOT NULL, -- Số tiền thực thu
    PaymentMethod NVARCHAR(50), -- Cash, Momo, BankTransfer
	Status INT DEFAULT 1, -- 1: Hoàn thành, 0: Đã hủy
    Notes NVARCHAR(500),
    CONSTRAINT FK_Order_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

-- 5. Chi tiết hóa đơn (Detail - Dapper & SP xử lý)
CREATE TABLE OrderDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL, -- Lưu giá tại thời điểm bán
    SubTotal DECIMAL(18, 2) NOT NULL,
    CONSTRAINT FK_Detail_Order FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT FK_Detail_Product FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- 6. Users
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL, -- 'Admin' hoặc 'Cashier'
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);
