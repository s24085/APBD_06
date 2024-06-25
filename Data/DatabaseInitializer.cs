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

                // Usuń istniejące tabele
                await DropTablesIfExists(connection, logger);

                // Wykonaj create.sql
                await ExecuteSqlScriptAsync(connection, "Data/create.sql", logger);

                // Wykonaj DROP PROCEDURE
                await ExecuteSqlCommandAsync(connection, "DROP PROCEDURE IF EXISTS AddProductToWarehouse;", logger);

                // Wykonaj proc.sql
                await ExecuteSqlScriptAsync(connection, "Data/proc.sql", logger);
            }
        }

        private static async Task DropTablesIfExists(MySqlConnection connection, ILogger logger)
        {
            string[] tables = { "Product_Warehouse", "`Order`", "Product", "Warehouse" };
            foreach (var table in tables)
            {
                string dropTableCmd = $"DROP TABLE IF EXISTS {table};";
                logger.LogInformation($"Executing: {dropTableCmd}");
                await ExecuteSqlCommandAsync(connection, dropTableCmd, logger);
            }
        }

        private static async Task ExecuteSqlScriptAsync(MySqlConnection connection, string scriptPath, ILogger logger)
        {
            string script = await File.ReadAllTextAsync(scriptPath);
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

        private static async Task ExecuteSqlCommandAsync(MySqlConnection connection, string commandText, ILogger logger)
        {
            using (var command = new MySqlCommand(commandText, connection))
            {
                try
                {
                    await command.ExecuteNonQueryAsync();
                    logger.LogInformation($"Executed command: {commandText}");
                }
                catch (MySqlException ex)
                {
                    logger.LogError($"Error executing command: {commandText}, Error: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
