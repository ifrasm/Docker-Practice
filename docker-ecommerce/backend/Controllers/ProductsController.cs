using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MyApi.Data;
using MyApi.Models;
using StackExchange.Redis;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private const string CacheKey = "products:all";
    private readonly ProductRepository _repository;
    private readonly IDatabase _cache;

    public ProductsController(ProductRepository repository, IConnectionMultiplexer redis)
    {
        _repository = repository;
        _cache = redis.GetDatabase();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var cached = await _cache.StringGetAsync(CacheKey);
        if (cached.HasValue)
        {
            var products = JsonSerializer.Deserialize<IEnumerable<Product>>(cached.ToString());
            return Ok(products);
        }

        var result = (await _repository.GetAllAsync()).ToList();
        await _cache.StringSetAsync(CacheKey, JsonSerializer.Serialize(result), TimeSpan.FromMinutes(2));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(CreateProductRequest request)
    {
        var product = await _repository.CreateAsync(request);
        await _cache.KeyDeleteAsync(CacheKey);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateProductRequest request)
    {
        if (!await _repository.UpdateAsync(id, request)) return NotFound();
        await _cache.KeyDeleteAsync(CacheKey);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _repository.DeleteAsync(id)) return NotFound();
        await _cache.KeyDeleteAsync(CacheKey);
        return NoContent();
    }
}
