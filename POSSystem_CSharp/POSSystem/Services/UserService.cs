using POSSystem.Database;
using POSSystem.Models;
using System.Data;

namespace POSSystem.Services
{
    public static class UserService
    {
        public static User? Login(string pin)
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Users WHERE Pin=@Pin AND IsActive=1", new() { ["@Pin"] = pin });
            return dt.Rows.Count > 0 ? MapUser(dt.Rows[0]) : null;
        }

        public static List<User> GetAll()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Users ORDER BY Name");
            return dt.Rows.Cast<DataRow>().Select(MapUser).ToList();
        }

        public static int Create(User u)
        {
            var id = DatabaseHelper.ExecuteScalar("INSERT INTO Users(Name,Role,Pin,IsActive) VALUES(@N,@R,@P,@A);SELECT SCOPE_IDENTITY();",
                new() { ["@N"] = u.Name, ["@R"] = u.Role, ["@P"] = u.Pin, ["@A"] = u.IsActive });
            return Convert.ToInt32(id);
        }

        public static void Update(User u)
        {
            DatabaseHelper.ExecuteNonQuery("UPDATE Users SET Name=@N,Role=@R,Pin=@P,IsActive=@A WHERE Id=@Id",
                new() { ["@N"] = u.Name, ["@R"] = u.Role, ["@P"] = u.Pin, ["@A"] = u.IsActive, ["@Id"] = u.Id });
        }

        public static void Delete(int id) => DatabaseHelper.ExecuteNonQuery("DELETE FROM Users WHERE Id=@Id", new() { ["@Id"] = id });

        private static User MapUser(DataRow r) => new User
        {
            Id = (int)r["Id"], Name = (string)r["Name"], Role = (string)r["Role"],
            Pin = (string)r["Pin"], IsActive = (bool)r["IsActive"], CreatedAt = (DateTime)r["CreatedAt"]
        };
    }
}
