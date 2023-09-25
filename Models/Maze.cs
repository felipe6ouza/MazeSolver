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

    public List<List<int>> GetMatrix()
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

        int[,] distance = new int[rows, cols];
        Coordinate?[,] prev = new Coordinate?[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                distance[i, j] = int.MaxValue;
                prev[i, j] = null;
            }
        }

        distance[start.X, start.Y] = 0;
        Queue<Coordinate> queue = new Queue<Coordinate>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                int newX = current.X + dx[i];
                int newY = current.Y + dy[i];

                if (newX >= 0 && newX < rows && newY >= 0 && newY < cols && Matrix[newX, newY] == 0)
                {
                    int newDist = distance[current.X, current.Y] + 1;
                    if (newDist < distance[newX, newY])
                    {
                        distance[newX, newY] = newDist;
                        prev[newX, newY] = current;
                        queue.Enqueue(new Coordinate(newX, newY));
                    }
                }
            }
        }

        if (distance[end.X, end.Y] == int.MaxValue)
        {
            return new PathResult(new List<Coordinate>(), 0);
        }

        List<Coordinate> path = new();
        var currentPos = end;

        while (currentPos != null)
        {
            path.Add(currentPos);
            currentPos = prev[currentPos.X, currentPos.Y];
        }
        path.Reverse();

        return new PathResult(path, distance[end.X, end.Y]);
    }

}