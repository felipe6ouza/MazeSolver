namespace Maze_Solver_API.Models.Responses
{
    public record MapResponse(List<List<int>> Map, int Size, Guid MazeId);
}