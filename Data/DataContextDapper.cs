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

        public bool ExecuteSqlWithParameter(string sqlQuery, DynamicParameters sqlParameters)
        {
            IDbConnection dbConnection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            return dbConnection.Execute(sqlQuery, sqlParameters) > 0;

            // SqlCommand commandWithParams = new(sqlQuery);

            // foreach (SqlParameter parameter in sqlParameters)
            // {
            //     commandWithParams.Parameters.Add(parameter);
            // }

            // SqlConnection dbConnection =
            //     new(_configuration.GetConnectionString("DefaultConnection"));
            // dbConnection.Open();
            // commandWithParams.Connection = dbConnection;
            // // Returns the number of rows affected
            // int rowsAffected = commandWithParams.ExecuteNonQuery();

            // dbConnection.Close();

            // return rowsAffected > 0;
        }

        public IEnumerable<T> LoadDataWithParameters<T>(
            string sqlQuery,
            DynamicParameters sqlParameters
        )
        {
            IDbConnection dbConnection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
            return dbConnection.Query<T>(sqlQuery, sqlParameters);
        }

        public T LoadSingleDataWithParameters<T>(string sqlQuery, DynamicParameters sqlParameters)
        {
            IDbConnection dbConnection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
            return dbConnection.QuerySingle<T>(sqlQuery, sqlParameters);
        }
    }
}
