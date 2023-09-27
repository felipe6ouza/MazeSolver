using Maze_Solver_API.Models;
using Maze_Solver_API.Models.Requests;
using Maze_Solver_API.Models.Responses;
using MazeSolver.API.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;


namespace Maze_Solver_API.Controllers
{
    [ApiController]
    [Route("map-solver")]
    public class GameMapController : ControllerBase
    {
        public readonly IMemoryCache _memoryCache;
        public GameMapController(IMemoryCache cache)
        {
            _memoryCache = cache;
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(MapResponse), 200)]
        public IActionResult CreateMaze([FromBody] GenerateMapRequest mazerRequest)
        {
            var createdMap = new GameMap(mazerRequest.Size, mazerRequest.ObstacleDensity);

            _memoryCache.Set(createdMap.Id, createdMap, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            });

            MapResponse response = new(
                Map: createdMap.GetPresentationMap(),
                Size: createdMap.Size,
                MazeId:createdMap.Id);

            return Ok(response);
        }


        [HttpPost("find-minimum-path")]
        [ProducesResponseType(typeof(PathResult), 200)]
        public IActionResult FindMinimumPath([FromBody] FindMinimalPathRequest request)
        {
            if (_memoryCache.Get(request.Id) is not GameMap gameMap)
                return BadRequest("gameMap not found.");

            if (!IsValidCoordinate(request.StartingPosition, gameMap.Graph) || !IsValidCoordinate(request.EndPosition, gameMap.Graph))
                return BadRequest("Invalid Cordinates in the gameMap");

            var pathResult = gameMap.FindShortestPath(request.StartingPosition, request.EndPosition);

            return Ok(pathResult);
        }


        private static bool IsValidCoordinate(Coordinate coordinate, int[,] gameMap)
        {

            return (coordinate.X >= 0 && coordinate.X < gameMap.GetLength(0)) 
                && (coordinate.Y >= 0 && coordinate.Y < gameMap.GetLength(1)) 
                && (gameMap[coordinate.X, coordinate.Y] != -1);

        }
    }
}