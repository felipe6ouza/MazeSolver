using Maze_Solver_API.Models;
using Maze_Solver_API.Models.Requests;
using Maze_Solver_API.Models.Responses;
using MazeSolver.API.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;


namespace Maze_Solver_API.Controllers
{
    [ApiController]
    [Route("maze-solver")]
    public class MazeSolverController : ControllerBase
    {
        public readonly IMemoryCache _memoryCache;
        public MazeSolverController(IMemoryCache cache)
        {
            _memoryCache = cache;
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(MazeResponse), 200)]
        [ProducesResponseType(400)]
        public IActionResult CreateMaze([FromBody] GenerateMazeRequest mazerRequest)
        {
            var createdMaze = new Maze(mazerRequest.Width, mazerRequest.Height, mazerRequest.ObstacleDensity);

            _memoryCache.Set(createdMaze.Id, createdMaze, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });

            MazeResponse response = new(
                Maze: createdMaze.GetPresentationMaze(),
                Width: createdMaze.Width,
                Height:createdMaze.Height,
                MazeId:createdMaze.Id);

            return Ok(response);
        }


        [HttpPost("find-minimum-path")]
        [ProducesResponseType(typeof(PathResult), 200)]
        public IActionResult FindMinimumPath([FromBody] FindMinimalPathRequest request)
        {
            if (_memoryCache.Get(request.Id) is not Maze maze)
                return BadRequest("Maze not found.");

            if (!IsValidCoordinate(request.StartingPosition, maze.Matrix) || !IsValidCoordinate(request.EndPosition, maze.Matrix))
                return BadRequest("Invalid Cordinates in the maze");

            var pathResult = maze.FindShortestPath(request.StartingPosition, request.EndPosition);

            if (pathResult.MinimalPath.Count == 0)
            {
                return BadRequest("No path found for the coordinates in the maze.");
            }
            return Ok(pathResult);
        }


        private static bool IsValidCoordinate(Coordinate coordinate, int[,] maze)
        {

            return (coordinate.X >= 0 && coordinate.X < maze.GetLength(0)) 
                && (coordinate.Y >= 0 && coordinate.Y < maze.GetLength(1)) 
                && (maze[coordinate.X, coordinate.Y] != 0);

        }
    }
}