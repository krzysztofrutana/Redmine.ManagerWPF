using Dapper;
using Redmine.ManagerWPF.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redmine.ManagerWPF.Database
{
    public class DatabaseManager
    {
        private readonly IContext _context;
        public DatabaseManager(IContext context)
        {
            _context = context;
        }
        public async Task CreateDatabaseAsync(string dbName)
        {
            try
            {
                var query = "SELECT * FROM sys.databases WHERE name = @name";
                var parameters = new DynamicParameters();
                parameters.Add("name", dbName);
                using (var connection = await _context.GetMasterConnectionAsync())
                {
                    var records = await connection.QueryAsync(query, parameters);
                    if (!records.Any())
                        connection.Execute($"CREATE DATABASE {dbName}");
                }
            }
            catch
            {
            }
        }

        public async Task<bool> CheckDatabaseExistAsync(string dbName)
        {
            try
            {
                var query = "SELECT * FROM sys.databases WHERE name = @name";
                var parameters = new DynamicParameters();
                parameters.Add("name", dbName);
                using (var connection = await _context.GetMasterConnectionAsync())
                {
                    var records = await connection.QueryAsync(query, parameters);
                    return records.Any();
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
