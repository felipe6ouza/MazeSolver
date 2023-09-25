using Maze_Solver_API.Models.Requests;

namespace MazeSolver.API.Models.Responses
{
    public record PathResult(List<Coordinate> MinimalPath, int Cost);
}