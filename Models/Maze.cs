using Maze_Solver_API.Models.Requests;
using MazeSolver.API.Models.Responses;

namespace Maze_Solver_API.Models;

public class Maze
{
    public Guid Id { get; set; }
    public int[,] Matrix { get; set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public double ObstacleDensity { get; private set; }

    public Maze(int width, int height, double obstacleDensity)
    {
        Width = width;
        Height = height;
        Matrix = new int[height, width];
        ObstacleDensity = obstacleDensity;
        Id = Guid.NewGuid();
        GenerateMaze();
    }

    public void GenerateMaze()
    {
        Random random = new();
        for (int row = 0; row < Height; row++)
        {
            for (int col = 0; col < Width; col++)
            {
                double obstacleChance = random.NextDouble();
                Matrix[row, col] = (obstacleChance <= ObstacleDensity) ? 0 : random.Next(4);
            }
        }
    }

    public List<List<int>> GetPresentationMaze()
    {
        List<List<int>> matrixList = new();

        for (int row = 0; row < Height; row++)
        {
            List<int> rowList = new();
            for (int col = 0; col < Width; col++)
            {
                rowList.Add(Matrix[row, col]);
            }
            matrixList.Add(rowList);
        }
        return matrixList;
    }


    public PathResult FindShortestPath(Coordinate start, Coordinate end)
    {
        int rows = Matrix.GetLength(0);
        int cols = Matrix.GetLength(1);

        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        int[,] distance = new int[rows, cols];
        bool[,] visited = new bool[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                distance[i, j] = int.MaxValue;
                visited[i, j] = false;
            }
        }

        distance[start.X, start.Y] = 0;

        while (true)
        {
            int minDistance = int.MaxValue;
            Coordinate? minCoordinate = null;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (!visited[i, j] && distance[i, j] < minDistance)
                    {
                        minDistance = distance[i, j];
                        minCoordinate = new Coordinate(i, j);
                    }
                }
            }

            if (minCoordinate == null)
                break;

            visited[minCoordinate.X, minCoordinate.Y] = true;

            for (int i = 0; i < 4; i++)
            {
                int newX = minCoordinate.X + dx[i];
                int newY = minCoordinate.Y + dy[i];

                if (newX >= 0 && newX < rows && newY >= 0 && newY < cols &&
                    !visited[newX, newY] && Matrix[newX, newY] != 0)
                {
                    int newDistance = distance[minCoordinate.X, minCoordinate.Y] + Matrix[newX, newY];

                    if (newDistance < distance[newX, newY])
                    {
                        distance[newX, newY] = newDistance;
                    }
                }
            }
        }

        // Reconstruct the path from end to start
        List<Coordinate> path = new();
        Coordinate currentCoordinate = end;

        while (currentCoordinate.X != start.X || currentCoordinate.Y != start.Y)
        {
            path.Add(currentCoordinate);

            for (int i = 0; i < 4; i++)
            {
                int newX = currentCoordinate.X + dx[i];
                int newY = currentCoordinate.Y + dy[i];

                if (newX >= 0 && newX < rows && newY >= 0 && newY < cols &&
                    distance[newX, newY] + Matrix[currentCoordinate.X, currentCoordinate.Y] == distance[currentCoordinate.X, currentCoordinate.Y])
                {
                    currentCoordinate = new Coordinate(newX, newY);
                    break;
                }
            }
        }

        path.Add(start);
        path.Reverse();

        // Calculate the total cost
        int cost = distance[end.X, end.Y];

        return new PathResult(path, cost);
    }

}