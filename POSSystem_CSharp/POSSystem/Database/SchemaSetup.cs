namespace POSSystem.Database
{
    public static class SchemaSetup
    {
        public static void CreateTablesIfNotExist()
        {
            string sql = @"
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categories' AND xtype='U')
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Color NVARCHAR(50) DEFAULT '#4CAF50',
    SortOrder INT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Items' AND xtype='U')
CREATE TABLE Items (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(300) NOT NULL,
    NameAr NVARCHAR(300),
    Barcode NVARCHAR(100),
    ItemNumber NVARCHAR(100),
    Price DECIMAL(12,2) NOT NULL DEFAULT 0,
    TaxRate DECIMAL(5,2) DEFAULT 0,
    DiscountRate DECIMAL(5,2) DEFAULT 0,
    CategoryId INT REFERENCES Categories(Id),
    Stock INT,
    IsFavorite BIT DEFAULT 0,
    IsAvailable BIT DEFAULT 1,
    Notes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(300) NOT NULL,
    Phone NVARCHAR(50),
    Email NVARCHAR(200),
    Notes NVARCHAR(500),
    TotalPurchases DECIMAL(14,2) DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) DEFAULT 'cashier',
    Pin NVARCHAR(20) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE()
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Invoices' AND xtype='U')
CREATE TABLE Invoices (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'open',
    OrderType NVARCHAR(30) DEFAULT 'dine_in',
    TableNumber NVARCHAR(20),
    CustomerId INT REFERENCES Customers(Id),
    CashierId INT REFERENCES Users(Id),
    DiscountAmount DECIMAL(12,2) DEFAULT 0,
    ServiceAmount DECIMAL(12,2) DEFAULT 0,
    AmountPaid DECIMAL(12,2) DEFAULT 0,
    PaymentMethod NVARCHAR(30),
    Notes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    PaidAt DATETIME
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InvoiceItems' AND xtype='U')
CREATE TABLE InvoiceItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL REFERENCES Invoices(Id) ON DELETE CASCADE,
    ItemId INT NOT NULL,
    Name NVARCHAR(300) NOT NULL,
    Price DECIMAL(12,2) NOT NULL,
    Quantity DECIMAL(10,3) DEFAULT 1,
    TaxRate DECIMAL(5,2) DEFAULT 0,
    DiscountRate DECIMAL(5,2) DEFAULT 0,
    Notes NVARCHAR(500)
);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AppSettings' AND xtype='U')
CREATE TABLE AppSettings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StoreName NVARCHAR(300) DEFAULT N'نقطة المبيعات',
    StoreNameAr NVARCHAR(300),
    Address NVARCHAR(500),
    Phone NVARCHAR(50),
    TaxRate DECIMAL(5,2) DEFAULT 15,
    ServiceRate DECIMAL(5,2) DEFAULT 0,
    Currency NVARCHAR(10) DEFAULT 'SAR',
    CurrencySymbol NVARCHAR(10) DEFAULT N'ر.س',
    ReceiptFooter NVARCHAR(500),
    LicenseStatus NVARCHAR(20) DEFAULT 'active'
);
";
            DatabaseHelper.ExecuteNonQuery(sql);
            SeedInitialData();
        }

        private static void SeedInitialData()
        {
            // Seed default admin user
            var userCount = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Users");
            if (Convert.ToInt32(userCount) == 0)
            {
                DatabaseHelper.ExecuteNonQuery(@"
INSERT INTO Users (Name, Role, Pin, IsActive) VALUES (N'المدير', 'admin', '1234', 1);
INSERT INTO Users (Name, Role, Pin, IsActive) VALUES (N'الكاشير', 'cashier', '0000', 1);
");
            }

            // Seed default settings
            var settingsCount = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM AppSettings");
            if (Convert.ToInt32(settingsCount) == 0)
            {
                DatabaseHelper.ExecuteNonQuery(@"
INSERT INTO AppSettings (StoreName, StoreNameAr, TaxRate, Currency, CurrencySymbol) 
VALUES (N'مطعم النجمة', N'مطعم النجمة', 15, 'SAR', N'ر.س');
");
            }

            // Seed sample categories
            var catCount = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Categories");
            if (Convert.ToInt32(catCount) == 0)
            {
                DatabaseHelper.ExecuteNonQuery(@"
INSERT INTO Categories (Name, Color, SortOrder) VALUES (N'مشروبات', '#2196F3', 1);
INSERT INTO Categories (Name, Color, SortOrder) VALUES (N'وجبات رئيسية', '#4CAF50', 2);
INSERT INTO Categories (Name, Color, SortOrder) VALUES (N'بيتزا', '#FF9800', 3);
INSERT INTO Categories (Name, Color, SortOrder) VALUES (N'سلطات', '#8BC34A', 4);
INSERT INTO Categories (Name, Color, SortOrder) VALUES (N'حلويات', '#E91E63', 5);
INSERT INTO Categories (Name, Color, SortOrder) VALUES (N'مقبلات', '#9C27B0', 6);
");
            }

            // Seed sample items
            var itemCount = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Items");
            if (Convert.ToInt32(itemCount) == 0)
            {
                DatabaseHelper.ExecuteNonQuery(@"
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'بيتزا مرغريتا كبير', 9000, 15, 3, 1, 1);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'بيتزا مرغريتا وسط', 7000, 15, 3, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'بيتزا مرغريتا صغير', 5000, 15, 3, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'بيتزا خضروات وسط', 8000, 15, 3, 1, 1);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'بيتزا دجاج كبير', 9000, 15, 3, 1, 1);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'بيتزا خضروات كبير', 8000, 15, 3, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'كريب ريال', 3400, 15, 3, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'كريب دجاج صغير', 5800, 15, 3, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'كريب خضروات صغير', 3800, 15, 3, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'كريب ماكن صغير', 5800, 15, 3, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'كريب نوتيلا', 3600, 15, 3, 1, 1);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'كريب بالبطاطا', 4000, 15, 3, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'كريب باللحم', 4500, 15, 3, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'بروست دجاج', 8000, 15, 2, 1, 1);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'شاورما دجاج', 5000, 15, 2, 1, 1);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'شاورما لحم', 6000, 15, 2, 1, 1);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'ساندوتش دجاج', 4500, 15, 2, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'عصير برتقال', 1800, 15, 1, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'عصير مانجو', 2000, 15, 1, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'مياه معدنية', 1500, 15, 1, 1, 1);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'قهوة عربية', 2000, 15, 1, 1, 1);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'شاي', 1500, 15, 1, 1, 0);
INSERT INTO Items (Name, Price, TaxRate, CategoryId, IsAvailable, IsFavorite) VALUES (N'نسكافيه', 2500, 15, 1, 1, 0);
");
            }
        }
    }
}
