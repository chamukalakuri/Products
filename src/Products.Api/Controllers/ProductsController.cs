using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Api.Models;
using Products.Api.Services;
using System.Net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Products.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }


        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return StatusCode(500, "Internal server error");
            }
        }

        [Route("getProductByColor")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByColor(string color)
        {
            try
            {
                var products = await _productService.GetProductsByColorAsync(color);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by color: {Color}", color);
                return StatusCode(500, "Internal server error");
            }
        }

        [Route("getProductById")]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductDto>> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [Route("createProduct")]
        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
               
                var createdProduct = await _productService.CreateProductAsync(productDto);

                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
            {
                // Handle duplicate key violation
                _logger.LogError("Duplicate key error: {Message}", sqlEx.Message);
                return Problem(
                            title: "Duplicate SKU",
                            detail: ex.Message,
                            statusCode: StatusCodes.Status409Conflict
                        );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "Internal server error");
            }
        }

        [Route("updateProduct")]
        [HttpPut]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                try
                {
                    await _productService.UpdateProductAsync(id, productDto);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpDelete("deleteProductbyId")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}