using Maze_Solver_API.Models.Requests;

namespace MazeSolver.API.Models.Responses
{
    public record PathResult(List<List<int>> MinimalPath , int Cost);
}