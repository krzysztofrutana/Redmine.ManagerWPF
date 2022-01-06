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

        SqlConnection GetConnection();
    }
}
