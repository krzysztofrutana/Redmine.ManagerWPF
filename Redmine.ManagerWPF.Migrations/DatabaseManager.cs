using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Dapper;
using Microsoft.Extensions.Logging;
using Redmine.ManagerWPF.Data.Dapper;

namespace Redmine.ManagerWPF.Database
{
    public class DatabaseManager
    {
        private readonly IContext _context;
        private readonly ILogger<DatabaseManager> _logger;

        public DatabaseManager(IContext context)
        {
            _context = context;
            var loggerFactory = Ioc.Default.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<DatabaseManager>();
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
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(CreateDatabaseAsync), ex.Message);
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
            catch (Exception ex)
            {
                _logger.LogError("{0} {1}", nameof(CheckDatabaseExistAsync), ex.Message);
                return false;
            }
        }
    }
}
