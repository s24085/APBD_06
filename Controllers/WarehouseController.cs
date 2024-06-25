using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using APBD_06.Models;
using Microsoft.EntityFrameworkCore;
using APBD_06.Data;
using MySql.Data.MySqlClient;

namespace APBD_06.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly WarehouseContext _context;

        public WarehouseController(WarehouseContext context)
        {
            _context = context;
        }

        [HttpPost("add-product")]
        public async Task<IActionResult> AddProductToWarehouse([FromBody] AddProductRequest request)
        {
            // Implement your logic here...
            return Ok();
        }

        [HttpPost("add-product-stored-procedure")]
        public async Task<IActionResult> AddProductToWarehouseWithSP([FromBody] AddProductRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0.");
            }

            using (var connection = new MySqlConnection(_context.Database.GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("AddProductToWarehouse", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                    command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                    command.Parameters.AddWithValue("@Amount", request.Amount);
                    command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

                    try
                    {
                        var result = await command.ExecuteScalarAsync();
                        return Ok(result);
                    }
                    catch (MySqlException ex)
                    {
                        return StatusCode(500, ex.Message);
                    }
                }
            }
        }
    }
}
