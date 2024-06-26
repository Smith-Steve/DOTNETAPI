using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Data
{
    class DataContextDapper
    {
        private readonly IConfiguration _configuration;
        public DataContextDapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<T> LoadData<T>(string sqlMultiLineQuery)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sqlMultiLineQuery);
        }

        public T LoadSingle<T>(string sqlSingleQuery)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            Console.WriteLine(sqlSingleQuery);
            return dbConnection.QuerySingle<T>(sqlSingleQuery);
        }

        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql) > 0;
        }

        public int ExecuteSqlWithRowCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql);
        }

        public bool ExecuteSqlWithParameters(string sql, List<SqlParameter> sqlParameters)
        {
            SqlCommand commandWithParameters = new SqlCommand(sql);
            foreach (SqlParameter parameter in sqlParameters)
            {
                commandWithParameters.Parameters.Add(parameter);
            }
            SqlConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            dbConnection.Open();
            commandWithParameters.Connection = dbConnection;

            int rowsAffected = commandWithParameters.ExecuteNonQuery();

            dbConnection.Close();

            return rowsAffected > 0;
        }
    }
}