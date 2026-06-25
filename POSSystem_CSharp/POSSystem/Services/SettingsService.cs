using POSSystem.Database;
using POSSystem.Models;
using System.Data;

namespace POSSystem.Services
{
    public static class SettingsService
    {
        private static AppSettings? _cached;

        public static AppSettings Get()
        {
            if (_cached != null) return _cached;
            var dt = DatabaseHelper.ExecuteQuery("SELECT TOP 1 * FROM AppSettings");
            if (dt.Rows.Count == 0) return new AppSettings();
            var r = dt.Rows[0];
            _cached = new AppSettings
            {
                Id = (int)r["Id"],
                StoreName = (string)r["StoreName"],
                StoreNameAr = r["StoreNameAr"] == DBNull.Value ? null : (string)r["StoreNameAr"],
                Address = r["Address"] == DBNull.Value ? null : (string)r["Address"],
                Phone = r["Phone"] == DBNull.Value ? null : (string)r["Phone"],
                TaxRate = (decimal)r["TaxRate"],
                ServiceRate = (decimal)r["ServiceRate"],
                Currency = (string)r["Currency"],
                CurrencySymbol = (string)r["CurrencySymbol"],
                ReceiptFooter = r["ReceiptFooter"] == DBNull.Value ? null : (string)r["ReceiptFooter"],
                LicenseStatus = (string)r["LicenseStatus"]
            };
            return _cached;
        }

        public static void Save(AppSettings s)
        {
            _cached = null;
            DatabaseHelper.ExecuteNonQuery(@"UPDATE AppSettings SET StoreName=@SN,StoreNameAr=@SA,Address=@Ad,Phone=@Ph,
                TaxRate=@Tax,ServiceRate=@Svc,Currency=@Cur,CurrencySymbol=@Sym,ReceiptFooter=@RF WHERE Id=@Id",
                new() { ["@SN"]=s.StoreName, ["@SA"]=s.StoreNameAr, ["@Ad"]=s.Address, ["@Ph"]=s.Phone,
                    ["@Tax"]=s.TaxRate, ["@Svc"]=s.ServiceRate, ["@Cur"]=s.Currency, ["@Sym"]=s.CurrencySymbol,
                    ["@RF"]=s.ReceiptFooter, ["@Id"]=s.Id });
        }

        public static void ClearCache() => _cached = null;
    }
}
