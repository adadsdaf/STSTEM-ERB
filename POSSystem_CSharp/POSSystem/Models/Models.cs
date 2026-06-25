namespace POSSystem.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Color { get; set; } = "#4CAF50";
        public int SortOrder { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? NameAr { get; set; }
        public string? Barcode { get; set; }
        public string? ItemNumber { get; set; }
        public decimal Price { get; set; }
        public decimal TaxRate { get; set; }
        public decimal DiscountRate { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryColor { get; set; }
        public int? Stock { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Notes { get; set; }
        public decimal TotalPurchases { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "cashier";
        public string Pin { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class InvoiceItem
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal TaxRate { get; set; }
        public decimal DiscountRate { get; set; }
        public string? Notes { get; set; }
        public decimal Subtotal => Price * Quantity;
        public decimal TaxAmount => Subtotal * (TaxRate / 100);
        public decimal DiscountAmount => Subtotal * (DiscountRate / 100);
        public decimal Total => Subtotal + TaxAmount - DiscountAmount;
    }

    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public string Status { get; set; } = "open";
        public string OrderType { get; set; } = "dine_in";
        public string? TableNumber { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? CashierId { get; set; }
        public string? CashierName { get; set; }
        public List<InvoiceItem> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public decimal TaxAmount => Items.Sum(i => i.TaxAmount);
        public decimal DiscountAmount { get; set; }
        public decimal ServiceAmount { get; set; }
        public decimal Total => Subtotal + TaxAmount - DiscountAmount + ServiceAmount;
        public decimal AmountPaid { get; set; }
        public decimal Change => AmountPaid - Total;
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class AppSettings
    {
        public int Id { get; set; }
        public string StoreName { get; set; } = "نقطة المبيعات";
        public string? StoreNameAr { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public decimal TaxRate { get; set; } = 15;
        public decimal ServiceRate { get; set; } = 0;
        public string Currency { get; set; } = "SAR";
        public string CurrencySymbol { get; set; } = "ر.س";
        public string? ReceiptFooter { get; set; }
        public string LicenseStatus { get; set; } = "active";
    }
}
