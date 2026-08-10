//using Microsoft.AspNetCore;
//using Microsoft.Extensions.Configuration;
//using System.Data.SqlClient;

//namespace YourNamespace.Data // Change this to your actual namespace
//{
//    public class DatabaseContext
//    {
//        private readonly string _connectionString;

//        public DatabaseContext(IConfiguration configuration)
//        {
//            _connectionString = configuration.GetConnectionString("DefaultConnection");
//        }

//        public SqlConnection CreateConnection()
//        {
//            return new SqlConnection(_connectionString);
//        }
//    }
//}
