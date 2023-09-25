using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class GenerateMazeRequest
{
    [Required(ErrorMessage = "The parameter Width is required.")]
    [Range(0, 64, ErrorMessage = "The field Width must be between 0 and 64.")]
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [Required(ErrorMessage = "The parameter Height is required.")]
    [Range(0, 64, ErrorMessage = "The field Height must be between 0 and 64.")]
    [Compare("Width", ErrorMessage = "The parameters Height and Width must have the same values.")]
    [JsonPropertyName("height")]
    public int Height { get; set; }

    [Required(ErrorMessage = "The parameter ObstacleDensity is required.")]
    [Range(0, 1, ErrorMessage = "The field ObstacleDensity must be between 0 and 1.")]
    [JsonPropertyName("obstacleDensity")]
    public double ObstacleDensity { get; set; }
}