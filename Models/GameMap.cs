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
                double obstacleChance = random.NextDouble(); //Obtem um valor aleatório entre 0 e 1
               
                int weight = obstacleChance <= ObstacleDensity ? -1 : random.Next(1, 4);  //Joga os dados da probabilidade para saber se vai criar um obstáculo ou dar um peso entre 1 e 4.  
                
                //Balanceia os pesos para garantir que o grafo seja unidirecional
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

        // Inicializa um conjunto de distâncias mínimas estimadas
        int[,] distances = InitializeDistances(rows, cols, start);

        // Conjunto de vértices a serem explorados
        //A fila é usada para garantir que o próximo vértice a ser explorado seja um vizinho do vértice explorado anteriormente.                }
        Queue<int[]> queue = new();

        // Dicionário para rastrear os pais dos vértices para reconstruir o caminho
        Dictionary<string, int[]> parents = new();

        queue.Enqueue(new int[] { start.X, start.Y }); //Marca o ponto inicial como aberto

        while (queue.Count > 0) //Enquanto estiver vertices não visitados na fila
        {
            var current = queue.Dequeue(); //retira o primeiro vértice da fila para explorar.
            int currentRow = current[0];
            int currentCol = current[1];

            foreach (var neighbor in GetValidNeighbors(currentRow, currentCol, rows, cols))
            {
                int neighborRow = neighbor[0];
                int neighborCol = neighbor[1];

                int currentShortestDistance = distances[currentRow, currentCol]; //valor corrente da menor distância conhecida do ponto de partida ao vértice atual
                int neighborEdgeWeight = Graph[neighborRow, neighborCol]; //peso da aresta entre o vértice atual (u) até seu vizinho (v). u -> v

                int newDistance = currentShortestDistance + neighborEdgeWeight; 

                if (newDistance < distances[neighborRow, neighborCol])
                {
                    distances[neighborRow, neighborCol] = newDistance;
                   
                    //O dicionário parents é usado para rastrear os pais dos vértices para reconstruir o caminho mais curto posteriormente
                    //Em outros termos, marcamos o vertice atual como pai de seu vizinho, cuja distância mínima foi atualizada em uma estrutura key-value
                    parents[$"{neighborRow},{neighborCol}"] = new int[] { currentRow, currentCol }; 
                    
                    queue.Enqueue(new int[] { neighborRow, neighborCol }); // O vizinho é adicionado na fila para que ele seja explorado posteriormente
                }
            }
        }

        /// No algoritmo de Dijkstra, você calcula as menores distâncias a partir do ponto de partida para todos os outros vértices no grafo.
        /// No entanto, apenas calcular as menores distâncias não é suficiente se você também quiser saber o caminho exato que leva do ponto de partida até qualquer outro ponto no grafo.
        /// Portanto, é preciso filtrar o Caminho Mínimo entre o ponto de partida e o ponto de chegada.
        Dictionary<string, int> shortestPath = ReconstructShortestPath(parents, start, end); 
        
        int totalCost = CalculateTotalCost(shortestPath); //Com o caminho obtido, vamos calcular entre o ponto de partida e o ponto de chegada.

        return new PathResult(ConvertToPathList(shortestPath), totalCost); 
    }


    ///<summary>
    ///Inicializa a matriz de distâncias com valores infinitos, 
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
        distances[start.X, start.Y] = 0; ///exceto a posição inicial com distância 0. 

        return distances;
    }
    ///<summary>
    /// Obter os vizinhos possíveis
    ///</summary>
    private IEnumerable<int[]> GetValidNeighbors(int row, int col, int rows, int cols)
    {
        //Identifica os vizinhos potenciais do ponto atual
        int[][] neighbors = {
        new int[] { row - 1, col },
        new int[] { row + 1, col },
        new int[] { row, col - 1 },
        new int[] { row, col + 1 }
    };
        
        //Retorna somente os vizinhos válidos
        return neighbors.Where(neighbor =>
            neighbor[0] >= 0 && neighbor[0] < rows && // Garante que o vizinho esteja dentro dos limites das linhas da matriz.
            neighbor[1] >= 0 && neighbor[1] < cols && // Garante que o vizinho esteja dentro dos limites das colunas da matriz.
            Graph[neighbor[0], neighbor[1]] != -1 // Garante que o vizinho não seja um obstáculo
        ); ;
    }
    ///<summary>
    /// Reconstruir o caminho a partir dos pais
    ///</summary>
    private Dictionary<string, int> ReconstructShortestPath(Dictionary<string, int[]> parents, Coordinate start, Coordinate end)
    {
        var shortestPath = new Dictionary<string, int>(); // Novo dicionário para armanezar o menor caminho entre coordenada inicial e final

        int row = end.X;
        int col = end.Y;

        while (row != start.X || col != start.Y) // Partindo do final o loop continuará até que o ponto inicial seja alcançado e o caminho mais curto reconstruído.
        {
            string key = $"{row},{col}";
            
            shortestPath[key] = Graph[row, col]; //armazenando o custo associado ao mover-se para a posição (row, col) no caminho mínimo que está sendo construído

            int[] parent = parents[key]; //Obtendo as coordenadas do pai do vertice atual

            //atualiza a row e col com as coordenadados do pai do vertice atual 
            row = parent[0];
            col = parent[1];
        }
        return shortestPath; //retorna as coordenadas do mapa associados como chaves, e os valores associados a essas chaves são os pesos das arestas correspondentes a esse caminho.
    }

    private static List<List<int>> ConvertToPathList(Dictionary<string, int> shortestPath)
    {
        return shortestPath.Keys
            .Reverse() // Inverter o dicionário de caminho para ter a ordem correta  das coordenadas do caminho
            .Select(key => key.Split(',').Select(int.Parse).ToList()) //Converter as chaves do dicionário em formato de coordenadas
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