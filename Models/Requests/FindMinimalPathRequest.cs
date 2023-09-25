using System.Text.Json.Serialization;

namespace Maze_Solver_API.Models.Requests
{
    public record StartingPosition([property: JsonPropertyName("x")] int X, [property: JsonPropertyName("y")] int Y);
    public record EndPosition([property: JsonPropertyName("x")] int X, [property: JsonPropertyName("y")] int Y);

    public record FindMinimalPathRequest(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("startingPosition")] Coordinate StartingPosition,
        [property: JsonPropertyName("endPosition")] Coordinate EndPosition
    );

    public record Coordinate(int X, int Y);
}
