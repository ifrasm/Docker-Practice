using Dapper;
using MyApi.Models;
using Npgsql;

namespace MyApi.Data;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("PostgreSQL connection string is missing.");
    }

    private NpgsqlConnection CreateConnection() => new(_connectionString);

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        const string sql = "SELECT id, name, price FROM products ORDER BY id";
        await using var connection = CreateConnection();
        return await connection.QueryAsync<Product>(sql);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        const string sql = "SELECT id, name, price FROM products WHERE id = @id";
        await using var connection = CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { id });
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        const string sql = """
            INSERT INTO products (name, price)
            VALUES (@Name, @Price)
            RETURNING id, name, price;
            """;
        await using var connection = CreateConnection();
        return await connection.QuerySingleAsync<Product>(sql, request);
    }

    public async Task<bool> UpdateAsync(int id, CreateProductRequest request)
    {
        const string sql = """
            UPDATE products SET name = @Name, price = @Price WHERE id = @Id;
            """;
        await using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, new { Id = id, request.Name, request.Price }) > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM products WHERE id = @id";
        await using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, new { id }) > 0;
    }
}
