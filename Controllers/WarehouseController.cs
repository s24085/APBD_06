using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using APBD_06.Models;
using Microsoft.EntityFrameworkCore;
using APBD_06.Data;
using MySql.Data.MySqlClient;
using System;

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
            if (request.Amount <= 0)
            {
                return BadRequest("Amount must be greater than 0.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                   
                    var product = await _context.Products.FindAsync(request.IdProduct);
                    if (product == null)
                    {
                        Console.WriteLine("Product not found");
                        return NotFound("Product not found");
                    }
                    Console.WriteLine($"Product found: {product.Name}");

                   
                    var warehouse = await _context.Warehouses.FindAsync(request.IdWarehouse);
                    if (warehouse == null)
                    {
                        Console.WriteLine("Warehouse not found");
                        return NotFound("Warehouse not found");
                    }
                    Console.WriteLine($"Warehouse found: {warehouse.Name}");

                  
                    var orders = await _context.Orders
                        .Where(o => o.IdProduct == request.IdProduct && o.FulfilledAt == null)
                        .ToListAsync();

                    if (!orders.Any())
                    {
                        Console.WriteLine($"No orders found for IdProduct: {request.IdProduct}");
                        return BadRequest("No matching order found to fulfill");
                    }

                    foreach (var order in orders)
                    {
                        Console.WriteLine($"Potential order: IdOrder: {order.IdOrder}, Amount: {order.Amount}, CreatedAt: {order.CreatedAt}, FulfilledAt: {order.FulfilledAt}");
                    }

                    var matchingOrder = orders
                        .Where(o => o.Amount == request.Amount && o.CreatedAt < request.CreatedAt)
                        .FirstOrDefault();

                    if (matchingOrder == null)
                    {
                        Console.WriteLine($"No matching order found to fulfill for IdProduct: {request.IdProduct}, Amount: {request.Amount}, CreatedAt: {request.CreatedAt}");
                        return BadRequest("No matching order found to fulfill");
                    }

                    Console.WriteLine($"Matching order found: IdOrder: {matchingOrder.IdOrder}");
                    Console.WriteLine($"Order details: IdOrder: {matchingOrder.IdOrder}, IdProduct: {matchingOrder.IdProduct}, Amount: {matchingOrder.Amount}, CreatedAt: {matchingOrder.CreatedAt}, FulfilledAt: {matchingOrder.FulfilledAt}");

                  
                    matchingOrder.FulfilledAt = DateTime.Now;
                    _context.Orders.Update(matchingOrder);
                    await _context.SaveChangesAsync();

                   
                    var productWarehouse = new ProductWarehouse
                    {
                        IdWarehouse = request.IdWarehouse,
                        IdProduct = request.IdProduct,
                        IdOrder = matchingOrder.IdOrder,
                        Amount = request.Amount,
                        Price = product.Price * request.Amount,
                        CreatedAt = DateTime.Now
                    };

                    _context.ProductWarehouses.Add(productWarehouse);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(productWarehouse.IdProductWarehouse);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, ex.Message);
                }
            }
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
