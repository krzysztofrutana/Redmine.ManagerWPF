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

    }
}