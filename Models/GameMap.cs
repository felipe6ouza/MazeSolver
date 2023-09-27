using Maze_Solver_API.Models.Requests;
using MazeSolver.API.Models.Responses;

namespace Maze_Solver_API.Models;

public class GameMap
{
    public Guid Id { get; set; }
    public int[,] Graph { get; set; }
    public int Size { get; private set; }
    public double ObstacleDensity { get; private set; }

    public GameMap(int size, double obstacleDensity)
    {
        Size = size;
        Graph = new int[Size, Size];
        ObstacleDensity = obstacleDensity;
        Id = Guid.NewGuid();
        GenerateMap();
    }

    ///<summary>
    /// Gera um grafo ponderado unidirecional
    ///<summary>
    public void GenerateMap()
    {
        Random random = new();
        for (int i = 0; i < Size; i++)
        {
            for (int j = i + 1; j < Size; j++)
            {
                double obstacleChance = random.NextDouble();
               
                int weight = obstacleChance <= ObstacleDensity ? -1 : random.Next(1, 4); 
                Graph[i, j] = weight;
                Graph[j, i] = weight;
            }
        }
    }
    ///<summary>
    /// Converte o Graph para um tipo suportado no JSON
    ///<summary>
    public List<List<int>> GetPresentationMap()
    {
        int rows = Graph.GetLength(0);
        int cols = Graph.GetLength(1);

        var presentationList = new List<List<int>>();

        for (int i = 0; i < rows; i++)
        {
            var rowList = new List<int>();
            for (int j = 0; j < cols; j++)
            {
                rowList.Add(Graph[i, j]);
            }
            presentationList.Add(rowList);
        }

        return presentationList;
    }
    ///<summary>
    /// Encontrar o menor caminho entre duas coordenadas utilizando o algoritmo de Dijkstra
    ///</summary>
    public PathResult FindShortestPath(Coordinate start, Coordinate end)
    {
        int rows = Graph.GetLength(0);
        int cols = Graph.GetLength(1);

        // Um conjunto de distâncias mínimas estimadas
        int[,] distances = InitializeDistances(rows, cols, start);

        // Conjunto de vértices a serem explorados
        Queue<int[]> queue = new();

        // Dicionário para rastrear os pais dos vértices para reconstruir o caminho
        Dictionary<string, int[]> parents = new();

        queue.Enqueue(new int[] { start.X, start.Y });

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int currentRow = current[0];
            int currentCol = current[1];

            foreach (var neighbor in GetValidNeighbors(currentRow, currentCol, rows, cols))
            {
                int neighborRow = neighbor[0];
                int neighborCol = neighbor[1];
                int newDistance = distances[currentRow, currentCol] + Graph[neighborRow, neighborCol];

                if (newDistance < distances[neighborRow, neighborCol])
                {
                    distances[neighborRow, neighborCol] = newDistance;
                    parents[$"{neighborRow},{neighborCol}"] = new int[] { currentRow, currentCol };
                    queue.Enqueue(new int[] { neighborRow, neighborCol });
                }
            }
        }

        Dictionary<string, int> shortestPath = ReconstructShortestPath(parents, start, end);
        int totalCost = CalculateTotalCost(shortestPath);

        return new PathResult(ConvertToPathList(shortestPath), totalCost);
    }


    ///<summary>
    ///Inicializa a matriz de distâncias com valores infinitos, exceto a posição inicial com distância 0. 
    ///</summary>
    private static int[,] InitializeDistances(int rows, int cols, Coordinate start)
    {
        int[,] distances = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                distances[i, j] = int.MaxValue;
            }
        }
        distances[start.X, start.Y] = 0;
        return distances;
    }
    ///<summary>
    /// Obter os vizinhos possíveis
    ///</summary>
    private IEnumerable<int[]> GetValidNeighbors(int row, int col, int rows, int cols)
    {
        int[][] neighbors = {
        new int[] { row - 1, col },
        new int[] { row + 1, col },
        new int[] { row, col - 1 },
        new int[] { row, col + 1 }
    };

        return neighbors.Where(neighbor =>
            neighbor[0] >= 0 && neighbor[0] < rows &&
            neighbor[1] >= 0 && neighbor[1] < cols &&
            Graph[neighbor[0], neighbor[1]] != -1
        );
    }
    ///<summary>
    /// Reconstruir o caminho a partir dos pais
    ///</summary>
    private Dictionary<string, int> ReconstructShortestPath(Dictionary<string, int[]> parents, Coordinate start, Coordinate end)
    {
        var shortestPath = new Dictionary<string, int>();
        int row = end.X;
        int col = end.Y;

        while (row != start.X || col != start.Y)
        {
            string key = $"{row},{col}";
            shortestPath[key] = Graph[row, col];
            int[] parent = parents[key];
            row = parent[0];
            col = parent[1];
        }

        return shortestPath;
    }

    private static List<List<int>> ConvertToPathList(Dictionary<string, int> shortestPath)
    {
        return shortestPath.Keys
            .Reverse() // Inverter o dicionário de caminho para ter a ordem correta
            .Select(key => key.Split(',').Select(int.Parse).ToList()) //Converter as chaves do dicionário em formtado de coordenadas
            .ToList();
    }
    ///<summary>
    /// Obter a soma total do custo para o caminho minimo
    ///</summary>
    private static int CalculateTotalCost(Dictionary<string, int> shortestPath)
    {
        return shortestPath.Values.Sum(); 
    }

}