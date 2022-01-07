using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Data.Dapper
{
    public interface IContext
    {
        Task<SqlConnection> GetConnectionAsync();
        Task<SqlConnection> GetConnectionAsync(string server, string dbName);

        SqlConnection GetConnection();

        Task<SqlConnection> GetMasterConnectionAsync();
    }
}
