using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class GenerateMapRequest
{
    [Required(ErrorMessage = "The parameter Size is required.")]
    [Range(0, 64, ErrorMessage = "The field Size must be between 0 and 64.")]
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [Required(ErrorMessage = "The parameter ObstacleDensity is required.")]
    [Range(0, 0.9, ErrorMessage = "The field ObstacleDensity must be between 0 and 0.9")]
    [JsonPropertyName("obstacleDensity")]
    public double ObstacleDensity { get; set; }
}