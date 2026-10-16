using System.ComponentModel.DataAnnotations;

namespace MyApi.Models;

public class CreateProductRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 99999999)]
    public decimal Price { get; set; }
}
