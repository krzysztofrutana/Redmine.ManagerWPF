using Redmine.ManagerWPF.Helpers;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Data.Dapper
{
    public class Context : IContext
    {
        public SqlConnection GetConnection()
        {
            var connectionString = "";
            var databaseName = SettingsHelper.GetDatabaseName();
            var server = SettingsHelper.GetServerName();

            if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(server))
            {
                connectionString = $"Server={server};Database={databaseName};Trusted_Connection=True;";
            }

            SqlConnection sqlConnection = new SqlConnection(connectionString);

            sqlConnection.Open();

            return sqlConnection;
        }

        public async Task<SqlConnection> GetConnectionAsync()
        {
            var connectionString = "";
            var databaseName = SettingsHelper.GetDatabaseName();
            var server = SettingsHelper.GetServerName();

            if (!string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(server))
            {
                connectionString = $"Server={server};Database={databaseName};Trusted_Connection=True;";
            }

            SqlConnection sqlConnection = new SqlConnection(connectionString);

            await sqlConnection.OpenAsync();

            return sqlConnection;
        }

        public async Task<SqlConnection> GetConnectionAsync(string server, string dbName)
        {
            var connectionString = "";

            if (!string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(dbName))
            {
                connectionString = $"Server={server};Database={dbName};Trusted_Connection=True;";
            }

            SqlConnection sqlConnection = new SqlConnection(connectionString);

            await sqlConnection.OpenAsync();

            return sqlConnection;
        }

        public async Task<SqlConnection> GetMasterConnectionAsync()
        {
            var connectionString = "";
            var server = SettingsHelper.GetServerName();

            if (!string.IsNullOrWhiteSpace(server))
            {
                connectionString = $"Server={server};Database=master;Trusted_Connection=True;";
            }

            SqlConnection sqlConnection = new SqlConnection(connectionString);

            await sqlConnection.OpenAsync();

            return sqlConnection;
        }

    }
}