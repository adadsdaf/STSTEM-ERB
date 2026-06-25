using POSSystem.Database;
using POSSystem.Models;
using System.Data;

namespace POSSystem.Services
{
    public static class InvoiceService
    {
        public static Invoice CreateNew(int? cashierId = null)
        {
            var num = GenerateInvoiceNumber();
            var id = DatabaseHelper.ExecuteScalar(
                "INSERT INTO Invoices(InvoiceNumber,Status,CashierId) VALUES(@Num,'open',@CId);SELECT SCOPE_IDENTITY();",
                new() { ["@Num"] = num, ["@CId"] = cashierId });
            return new Invoice { Id = Convert.ToInt32(id), InvoiceNumber = num, Status = "open", CreatedAt = DateTime.Now };
        }

        public static Invoice? GetById(int id)
        {
            var dt = DatabaseHelper.ExecuteQuery(@"
                SELECT i.*, u.Name AS CashierName, c.Name AS CustomerName
                FROM Invoices i
                LEFT JOIN Users u ON i.CashierId = u.Id
                LEFT JOIN Customers c ON i.CustomerId = c.Id
                WHERE i.Id = @Id", new() { ["@Id"] = id });
            if (dt.Rows.Count == 0) return null;
            var inv = MapInvoice(dt.Rows[0]);
            inv.Items = GetInvoiceItems(id);
            return inv;
        }

        public static List<Invoice> GetAll(string? status = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var sql = @"SELECT i.*, u.Name AS CashierName, c.Name AS CustomerName
                        FROM Invoices i LEFT JOIN Users u ON i.CashierId=u.Id LEFT JOIN Customers c ON i.CustomerId=c.Id WHERE 1=1";
            var p = new Dictionary<string, object?>();
            if (!string.IsNullOrEmpty(status)) { sql += " AND i.Status=@St"; p["@St"] = status; }
            if (dateFrom.HasValue) { sql += " AND i.CreatedAt>=@From"; p["@From"] = dateFrom.Value.Date; }
            if (dateTo.HasValue) { sql += " AND i.CreatedAt<@To"; p["@To"] = dateTo.Value.Date.AddDays(1); }
            sql += " ORDER BY i.CreatedAt DESC";
            var dt = DatabaseHelper.ExecuteQuery(sql, p);
            var list = dt.Rows.Cast<DataRow>().Select(MapInvoice).ToList();
            foreach (var inv in list) inv.Items = GetInvoiceItems(inv.Id);
            return list;
        }

        public static void SaveItems(int invoiceId, List<InvoiceItem> items)
        {
            DatabaseHelper.ExecuteNonQuery("DELETE FROM InvoiceItems WHERE InvoiceId=@Id", new() { ["@Id"] = invoiceId });
            foreach (var item in items)
            {
                DatabaseHelper.ExecuteNonQuery(@"INSERT INTO InvoiceItems(InvoiceId,ItemId,Name,Price,Quantity,TaxRate,DiscountRate,Notes)
                    VALUES(@InvId,@ItemId,@Name,@Price,@Qty,@Tax,@Disc,@Notes)",
                    new() { ["@InvId"]=invoiceId, ["@ItemId"]=item.ItemId, ["@Name"]=item.Name,
                        ["@Price"]=item.Price, ["@Qty"]=item.Quantity, ["@Tax"]=item.TaxRate,
                        ["@Disc"]=item.DiscountRate, ["@Notes"]=item.Notes });
            }
        }

        public static void UpdateHeader(Invoice inv)
        {
            DatabaseHelper.ExecuteNonQuery(@"UPDATE Invoices SET CustomerId=@CustId,TableNumber=@Table,OrderType=@OType,
                DiscountAmount=@Disc,ServiceAmount=@Svc,Notes=@Notes WHERE Id=@Id",
                new() { ["@CustId"]=inv.CustomerId, ["@Table"]=inv.TableNumber, ["@OType"]=inv.OrderType,
                    ["@Disc"]=inv.DiscountAmount, ["@Svc"]=inv.ServiceAmount, ["@Notes"]=inv.Notes, ["@Id"]=inv.Id });
        }

        public static void ProcessPayment(int invoiceId, string paymentMethod, decimal amountPaid)
        {
            DatabaseHelper.ExecuteNonQuery(@"UPDATE Invoices SET Status='paid',PaymentMethod=@PM,AmountPaid=@Paid,PaidAt=GETDATE() WHERE Id=@Id",
                new() { ["@PM"] = paymentMethod, ["@Paid"] = amountPaid, ["@Id"] = invoiceId });
        }

        public static void Cancel(int invoiceId) =>
            DatabaseHelper.ExecuteNonQuery("UPDATE Invoices SET Status='cancelled' WHERE Id=@Id", new() { ["@Id"] = invoiceId });

        public static (decimal TodaySales, int TodayCount, int OpenCount) GetTodaySummary()
        {
            var dt = DatabaseHelper.ExecuteQuery(@"
                SELECT 
                    ISNULL(SUM(CASE WHEN Status='paid' AND CAST(CreatedAt AS DATE)=CAST(GETDATE() AS DATE) THEN AmountPaid ELSE 0 END),0) AS TodaySales,
                    COUNT(CASE WHEN Status='paid' AND CAST(CreatedAt AS DATE)=CAST(GETDATE() AS DATE) THEN 1 END) AS TodayCount,
                    COUNT(CASE WHEN Status='open' THEN 1 END) AS OpenCount
                FROM Invoices");
            var row = dt.Rows[0];
            return ((decimal)row["TodaySales"], Convert.ToInt32(row["TodayCount"]), Convert.ToInt32(row["OpenCount"]));
        }

        private static List<InvoiceItem> GetInvoiceItems(int invoiceId)
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM InvoiceItems WHERE InvoiceId=@Id", new() { ["@Id"] = invoiceId });
            return dt.Rows.Cast<DataRow>().Select(r => new InvoiceItem
            {
                ItemId = (int)r["ItemId"], Name = (string)r["Name"],
                Price = (decimal)r["Price"], Quantity = (decimal)r["Quantity"],
                TaxRate = (decimal)r["TaxRate"], DiscountRate = (decimal)r["DiscountRate"],
                Notes = r["Notes"] == DBNull.Value ? null : (string)r["Notes"]
            }).ToList();
        }

        private static Invoice MapInvoice(DataRow r) => new Invoice
        {
            Id = (int)r["Id"], InvoiceNumber = (string)r["InvoiceNumber"],
            Status = (string)r["Status"], OrderType = (string)r["OrderType"],
            TableNumber = r["TableNumber"] == DBNull.Value ? null : (string)r["TableNumber"],
            CustomerId = r["CustomerId"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["CustomerId"]),
            CustomerName = r["CustomerName"] == DBNull.Value ? null : (string)r["CustomerName"],
            CashierId = r["CashierId"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["CashierId"]),
            CashierName = r["CashierName"] == DBNull.Value ? null : (string)r["CashierName"],
            DiscountAmount = (decimal)r["DiscountAmount"], ServiceAmount = (decimal)r["ServiceAmount"],
            AmountPaid = (decimal)r["AmountPaid"],
            PaymentMethod = r["PaymentMethod"] == DBNull.Value ? null : (string)r["PaymentMethod"],
            Notes = r["Notes"] == DBNull.Value ? null : (string)r["Notes"],
            CreatedAt = (DateTime)r["CreatedAt"],
            PaidAt = r["PaidAt"] == DBNull.Value ? null : (DateTime?)r["PaidAt"]
        };

        private static string GenerateInvoiceNumber()
        {
            var count = DatabaseHelper.ExecuteScalar("SELECT COUNT(*)+1 FROM Invoices");
            return $"INV-{DateTime.Now:yyyyMMdd}-{Convert.ToInt32(count):D4}";
        }
    }
}
