using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetApi.Data
{
    class DataContextDapper
    {
        private readonly IConfiguration _configuration;

        public DataContextDapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<T> LoadData<T>(string sqlQuery)
        {
            IDbConnection dbConnection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
            return dbConnection.Query<T>(sqlQuery);
        }

        public T LoadSingleData<T>(string sqlQuery)
        {
            IDbConnection dbConnection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
            return dbConnection.QuerySingle<T>(sqlQuery);
        }

        public bool ExecuteSql(string sqlQuery)
        {
            IDbConnection dbConnection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
            // Returns the number of rows affected, therefore we check if we affected any rows at all by checking if the number of rows affected is greater than 0
            return dbConnection.Execute(sqlQuery) > 0;
        }

        public int ExecuteSqlWithRowCount(string sqlQuery)
        {
            IDbConnection dbConnection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
            // Returns the number of rows affected
            return dbConnection.Execute(sqlQuery);
        }
    }
}
