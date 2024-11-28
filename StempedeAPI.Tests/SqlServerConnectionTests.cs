using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Xunit;

namespace StempedeAPI.Tests
{
    public class SqlServerConnectionTests
    {
        private const string ConnectionString = "Server=DESKTOP-USDRF6H\\SQLEXPRESS;Database=StempedeAPIshop;User ID=sa;Password=p@ssw0rd12345;TrustServerCertificate=True;";

        [Fact]
        public async Task Test_SqlServerConnection_ShouldConnectToDatabase()
        {
            // Arrange
            await using var connection = new SqlConnection(ConnectionString);

            try
            {
                // Act
                await connection.OpenAsync();

                // Assert - Check if the connection is open
                Assert.Equal(ConnectionState.Open, connection.State);

                // Optionally run a query to ensure the connection is to the correct database
                using (var command = new SqlCommand("SELECT DB_NAME()", connection))
                {
                    var databaseName = await command.ExecuteScalarAsync();
                    Assert.Equal("StempedeAPIshop", databaseName.ToString());
                }
            }
            catch (Exception ex)
            {
                // Fail the test if any exception occurs
                Assert.True(false, $"Exception occurred: {ex.Message}");
            }
            finally
            {
                // Ensure the connection is closed
                await connection.CloseAsync();
            }
        }
    }
}
