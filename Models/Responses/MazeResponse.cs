namespace Maze_Solver_API.Models.Responses
{
    public record MazeResponse(List<List<int>> Maze, int Width, int Height, Guid MazeId);
}