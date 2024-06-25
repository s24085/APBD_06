using MySql.Data.MySqlClient;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace APBD_06.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(string connectionString, ILogger logger)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                await ExecuteSqlScriptAsync(connection, "Data/create.sql", logger);
                await ExecuteSqlScriptAsync(connection, "Data/proc.sql", logger);
            }
        }

        private static async Task ExecuteSqlScriptAsync(MySqlConnection connection, string scriptPath, ILogger logger)
        {
            var script = await File.ReadAllTextAsync(scriptPath);
            using (var command = new MySqlCommand(script, connection))
            {
                try
                {
                    await command.ExecuteNonQueryAsync();
                    logger.LogInformation($"Executed script: {scriptPath}");
                }
                catch (MySqlException ex)
                {
                    logger.LogError($"Error executing script: {scriptPath}, Error: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
