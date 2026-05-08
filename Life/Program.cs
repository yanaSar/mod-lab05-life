using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ScottPlot;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        private bool IsAliveNext;

        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }

    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;
        public int Generation { get; set; }

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;
            Generation = 0;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        public Board(int columns, int rows) : this(columns, rows, 1, 0) { }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            foreach (var cell in Cells)
                cell.DetermineNextLiveState();
            foreach (var cell in Cells)
                cell.Advance();
            Generation++;    
        }

        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }

        public int CountLiveCells()
        {
            int count = 0;
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    if (Cells[x, y].IsAlive) count++;
            return count;
        }
        
        public List<List<(int, int)>> GetClusters()
        {
            bool[,] visited = new bool[Columns, Rows];
            List<List<(int, int)>> clusters = new List<List<(int, int)>>();
                
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    if (Cells[x, y].IsAlive && !visited[x, y])
                    {
                        List<(int, int)> cluster = new List<(int, int)>();
                        DFS(x, y, visited, cluster);
                        if (cluster.Count > 0)
                            clusters.Add(cluster);
                    }
                }
            }
            return clusters;
        }
        
        private void DFS(int x, int y, bool[,] visited, List<(int, int)> cluster)
        {
            if (x < 0 || x >= Columns || y < 0 || y >= Rows || visited[x, y] || !Cells[x, y].IsAlive)
                return;
                
            visited[x, y] = true;
            cluster.Add((x, y));
            
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    DFS(x + i, y + j, visited, cluster);
                }
            }
        }
        
        public string GetPattern(List<(int, int)> cluster)
        {
            if (cluster.Count < 3) return "Другое";
            
            int minX = cluster.Min(c => c.Item1);
            int maxX = cluster.Max(c => c.Item1);
            int minY = cluster.Min(c => c.Item2);
            int maxY = cluster.Max(c => c.Item2);
            
            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            
            bool[,] pattern = new bool[width, height];
            foreach (var (x, y) in cluster)
            {
                pattern[x - minX, y - minY] = true;
            }

            if (width == 2 && height == 2 && 
                pattern[0,0] && pattern[0,1] && pattern[1,0] && pattern[1,1])
                return "Блок";

            if (width == 4 && height == 3)
            {
                if (!pattern[0,0] && pattern[0,1] && pattern[0,2] && !pattern[0,3] &&
                    pattern[1,0] && !pattern[1,1] && !pattern[1,2] && pattern[1,3] &&
                    !pattern[2,0] && pattern[2,1] && pattern[2,2] && !pattern[2,3])
                    return "Улей";
            }

            if (width == 3 && height == 3)
            {
                int aliveCount = 0;
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        if (pattern[i, j]) aliveCount++;
                
                if (aliveCount == 5) return "Лодка";
            }

            if ((width == 3 && height == 1 && pattern[0,0] && pattern[1,0] && pattern[2,0]) ||
                (width == 1 && height == 3 && pattern[0,0] && pattern[0,1] && pattern[0,2]))
                return "Мигалка";

            if (width == 4 && height == 4)
            {
                int aliveCount = 0;
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        if (pattern[i, j]) aliveCount++;
                
                if (aliveCount == 8) return "Ладья";
            }

            if (width == 3 && height == 3)
            {
                if (!pattern[0,0] && !pattern[0,1] && pattern[0,2] &&
                    pattern[1,0] && !pattern[1,1] && pattern[1,2] &&
                    !pattern[2,0] && pattern[2,1] && pattern[2,2])
                    return "Глидер";
            }
            
            return "Другое";
        }
    
        public void SaveToFile(string filename)
        {
            var data = new BoardData
            {
                Columns = Columns,
                Rows = Rows,
                Generation = Generation,
                Cells = new List<List<bool>>()
            };
            
            for (int y = 0; y < Rows; y++)
            {
                List<bool> row = new List<bool>();
                for (int x = 0; x < Columns; x++)
                {
                    row.Add(Cells[x, y].IsAlive);
                }
                data.Cells.Add(row);
            }
            
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filename, json);
        }
        
        public static Board LoadFromFile(string filename)
        {
            string json = File.ReadAllText(filename);
            var data = JsonSerializer.Deserialize<BoardData>(json);
            
            Board board = new Board(data.Columns, data.Rows, 1, 0);
            for (int y = 0; y < data.Rows; y++)
            {
                for (int x = 0; x < data.Columns; x++)
                {
                    board.Cells[x, y].IsAlive = data.Cells[y][x];
                }
            }
            board.Generation = data.Generation;
            return board;
        }
    
        public void SetFigure(int startX, int startY, bool[,] figure)
        {
            for (int x = 0; x < figure.GetLength(0); x++)
            {
                for (int y = 0; y < figure.GetLength(1); y++)
                {
                    int posX = startX + x;
                    int posY = startY + y;
                    if (posX >= 0 && posX < Columns && posY >= 0 && posY < Rows)
                        Cells[posX, posY].IsAlive = figure[x, y];
                    
                }
            }
        }
        
        public Dictionary<string, int> CountElements()
        {
            Dictionary<string, int> elements = new Dictionary<string, int>();
            elements["Клетки"] = CountLiveCells();
            
            var clusters = GetClusters();
            elements["Комбинации"] = clusters.Count;
            
            foreach (var cluster in clusters)
            {
                string pattern = GetPattern(cluster);
                if (!elements.ContainsKey(pattern))
                    elements[pattern] = 0;
                elements[pattern]++;
            }
            
            return elements;
        }
    }

    public class BoardData
    {
        public int Columns { get; set; }
        public int Rows { get; set; }
        public int Generation { get; set; }
        public List<List<bool>> Cells { get; set; }
    }

    public class Config
    {
        public int BoardWidth { get; set; } = 80;
        public int BoardHeight { get; set; } = 40;
        public int CellSize { get; set; } = 1;
        public double LiveDensity { get; set; } = 0.2;
        public int DelayMs { get; set; } = 100;
        public int MaxGenerations { get; set; } = 1000;
        
        public static Config Load(string filename)
        {
            if (File.Exists(filename))
            {
                string json = File.ReadAllText(filename);
                return JsonSerializer.Deserialize<Config>(json);
            }
            return new Config();
        }
        
        public void Save(string filename)
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filename, json);
        }
    }

    class Program
    {
        static Board board;
        static Config config;

        static void Main(string[] args)
        {
            config = Config.Load("config.json");
            config.Save("config.json");
            
            Console.WriteLine("1. Новая игра со случайным заполнением");
            Console.WriteLine("2. Загрузить фигуру из файла");
            Console.WriteLine("3. Исследование случайных распределений");
            Console.WriteLine("4. Анализ текущего поля");
            Console.WriteLine("5. Демонстрация известных фигур");
            Console.Write("\nВыберите действие (1-5): ");
            
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    ResetRandom();
                    RunSimulation();
                    break;
                case "2":
                    LoadAndSimulate();
                    break;
                case "3":
                    ResearchRandomDistributions();
                    break;
                case "4":
                    AnalyzeBoard();
                    break;
                case "5":
                    DemoFigures();
                    break;
                default:
                    ResetRandom();
                    RunSimulation();
                    break;
            }
        }

        static private void ResetRandom()
        {
            board = new Board(
                width: config.BoardWidth,
                height: config.BoardHeight,
                cellSize: config.CellSize,
                liveDensity: config.LiveDensity);
        }

        static void ResetEmpty()
        {
            board = new Board(config.BoardWidth, config.BoardHeight, config.CellSize, 0);
        }

        static void Render()
        {
            Console.SetCursorPosition(0, 0);
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine($"Поколение: {board.Generation} | Живых клеток: {board.CountLiveCells()}");
            sb.AppendLine(new string('=', board.Columns + 2));
            
            for (int y = 0; y < board.Rows; y++)
            {
                sb.Append('|');
                for (int x = 0; x < board.Columns; x++)
                {
                    sb.Append(board.Cells[x, y].IsAlive ? '*' : ' ');
                }
                sb.AppendLine("|");
            }
            sb.AppendLine(new string('=', board.Columns + 2));
            sb.AppendLine("Пробел - пауза, S - сохранить, Q - выход, A - анализ");
            
            Console.Write(sb.ToString());
        }
        static void RunSimulation()
        {
            bool running = true;
            bool paused = false;
            int stableCounter = 0;
            int previousLiveCount = board.CountLiveCells();
            
            Console.Clear();
            
            while (running && board.Generation < config.MaxGenerations)
            {
                if (!paused)
                {
                    Render();
                    
                    int currentLiveCount = board.CountLiveCells();
                    if (currentLiveCount == previousLiveCount)
                    {
                        stableCounter++;
                        if (stableCounter >= 10)
                        {
                            Console.WriteLine("\n Достигнуто стабильное состояние");
                            Console.WriteLine($"Стабильно на протяжении {stableCounter} поколений");
                            if (stableCounter >= 20) break;
                        }
                    }
                    else
                    {
                        stableCounter = 0;
                    }
                    previousLiveCount = currentLiveCount;
                    
                    board.Advance();
                }
                
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.Spacebar:
                            paused = !paused;
                            if (paused)
                                Console.WriteLine("\n Пауза. Нажмите Пробел для продолжения");
                            break;
                        case ConsoleKey.S:
                            SaveCurrentBoard();
                            break;
                        case ConsoleKey.A:
                            ShowAnalysis();
                            break;
                        case ConsoleKey.Q:
                            running = false;
                            break;
                    }
                }
                
                Thread.Sleep(config.DelayMs);
            }
            
            if (board.Generation >= config.MaxGenerations)
            {
                Console.WriteLine($"\nДостигнуто максимальное количество поколений ({config.MaxGenerations})");
            }
        }

        static void SaveCurrentBoard()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"board_gen_{board.Generation}_{timestamp}.json";
            board.SaveToFile(filename);
            Console.WriteLine($"\n Сохранено в {filename}");
            Thread.Sleep(1000);
        }

        static void LoadAndSimulate()
        {
            Console.Write("\nВведите имя файла для загрузки (например, glider.json): ");
            string filename = Console.ReadLine();
            
            string fullPath = filename;
            if (!File.Exists(fullPath))
                fullPath = Path.Combine("..","Data", filename);
                
            if (File.Exists(fullPath))
            {
                board = Board.LoadFromFile(fullPath);
                Console.WriteLine($"\n Загружено из {fullPath}");
                Console.WriteLine($"Поколение: {board.Generation}, Живых клеток: {board.CountLiveCells()}");
                Thread.Sleep(1500);
                RunSimulation();
            }
            else
            {
                Console.WriteLine($" Файл {filename} не найден");
                Console.WriteLine("Создаю пример файла с глидером...");
                
                if (Directory.Exists("Data"))
                {
                    var files = Directory.GetFiles("Data", "*.json");
                    foreach (var file in files)
                    {
                        Console.WriteLine($"  - {Path.GetFileName(file)}");
                    }
                }
                else
                {
                    Console.WriteLine("  Папка Data не найдена");
                }
                
                Console.WriteLine("\nНажмите любую клавишу для возврата в меню");
                Console.ReadKey();
                Main(new string[0]);
            }
        }
        
        static void ShowAnalysis()
        {
            var elements = board.CountElements();
            Console.WriteLine($"Размер поля: {board.Columns}x{board.Rows}");
            Console.WriteLine($"Поколение: {board.Generation}");
            Console.WriteLine("\nЭлементы:");
            foreach (var elem in elements)
            {
                Console.WriteLine($"  {elem.Key}: {elem.Value}");
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения");
            Console.ReadKey();
        }
        
        static void AnalyzeBoard()
        {
            Console.Write("\nВведите имя файла для анализа: ");
            string filename = Console.ReadLine();
            
            // Ищем сначала в папке Data на уровень выше, потом в текущей
            string dataPath = Path.Combine("..", "Data", filename);
            string fullPath = filename;
            
            if (File.Exists(dataPath))
                fullPath = dataPath;
            else if (!File.Exists(fullPath))
                fullPath = Path.Combine("Data", filename);
                
            if (File.Exists(fullPath))
            {
                board = Board.LoadFromFile(fullPath);
                ShowAnalysis();
            }
            else
            {
                Console.WriteLine($" Файл {filename} не найден");
            }
        }
        
        static void DemoFigures()
        {
            Console.Clear();
            Console.WriteLine("1. Блок (устойчивая)");
            Console.WriteLine("2. Мигалка (периодическая)");
            Console.WriteLine("3. Глидер (движущаяся)");
            Console.WriteLine("4. Ладья");
            Console.WriteLine("5. Улей");
            Console.Write("\nВыберите фигуру (1-5): ");
            
            string choice = Console.ReadLine();
            ResetEmpty();
            
            switch (choice)
            {
                case "1":
                    // Блок 2x2
                    bool[,] block = { { true, true }, { true, true } };
                    board.SetFigure(board.Columns/2 - 1, board.Rows/2 - 1, block);
                    Console.WriteLine("Блок - устойчивая фигура");
                    break;
                case "2":
                    // Мигалка (горизонтальная)
                    bool[,] blinker = { { true, true, true } };
                    board.SetFigure(board.Columns/2 - 1, board.Rows/2, blinker);
                    Console.WriteLine("Мигалка - периодическая фигура (период 2)");
                    break;
                case "3":
                    // Глидер
                    bool[,] glider = { 
                        { false, false, true },
                        { true, false, true },
                        { false, true, true }
                    };
                    board.SetFigure(board.Columns/2 - 1, board.Rows/2 - 1, glider);
                    Console.WriteLine("Глидер - движущаяся фигура");
                    break;
                case "4":
                    // Ладья
                    bool[,] ship = {
                        { true, true, false, false },
                        { true, false, true, false },
                        { false, true, false, true },
                        { false, false, true, true }
                    };
                    board.SetFigure(board.Columns/2 - 2, board.Rows/2 - 2, ship);
                    Console.WriteLine("Ладья - движущаяся фигура");
                    break;
                case "5":
                    // Улей
                    bool[,] beehive = {
                        { false, true, true, false },
                        { true, false, false, true },
                        { false, true, true, false }
                    };
                    board.SetFigure(board.Columns/2 - 2, board.Rows/2 - 1, beehive);
                    Console.WriteLine("Улей - устойчивая фигура");
                    break;
            }
            
            Thread.Sleep(2000);
            RunSimulation();
        }   
        
        static void ResearchRandomDistributions()
        {
            Console.Clear();
            Console.WriteLine("Исследование случайных распределений\n");
            
            List<double> densities = new List<double> { 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5 };
            List<int> stableTimes = new List<int>();
            
            string dataPath = Path.Combine("..", "Data");
            Directory.CreateDirectory(dataPath);
            string dataFile = Path.Combine(dataPath, "data.txt");
            
            Console.WriteLine($"Сохранение в: {Path.GetFullPath(dataFile)}");
            Console.WriteLine("Плотность   Поколений до стабилизации");
            
            using (StreamWriter writer = new StreamWriter(dataFile))
            {
                writer.WriteLine("Плотность\tПоколений до стабилизации");
                
                foreach (double density in densities)
                {
                    int totalGenerations = 0;
                    int simulations = 5;
                    
                    Console.Write($"{density:F2}       | ");
                    
                    for (int sim = 0; sim < simulations; sim++)
                    {
                        board = new Board(50, 50, 1, density);
                        int generations = RunToStability();
                        totalGenerations += generations;
                        Console.Write($"{generations} ");
                    }
                    
                    int avgGenerations = totalGenerations / simulations;
                    stableTimes.Add(avgGenerations);
                    Console.WriteLine($"→ среднее: {avgGenerations}");
                    writer.WriteLine($"{density}\t{avgGenerations}");
                }
            }
            
            Console.WriteLine($"\nДанные сохранены в {Path.GetFullPath(dataFile)}");
            SavePlotToPNG(densities, stableTimes);
        }

        static void SavePlotToPNG(List<double> densities, List<int> stableTimes)
        {
            try
            {
                string dataPath = Path.Combine("..", "Data");
                Directory.CreateDirectory(dataPath);
                string plotFile = Path.Combine(dataPath, "plot.png");
                
                Console.WriteLine($"Сохранение графика в: {Path.GetFullPath(plotFile)}");
                
                var plt = new ScottPlot.Plot(800, 600);
                double[] x = densities.ToArray();
                double[] y = stableTimes.Select(v => (double)v).ToArray();
                
                plt.AddScatter(x, y);
                plt.Title("Переход в стабильное состояние от плотности заполнения");
                plt.XLabel("Плотность заполнения");
                plt.YLabel("Среднее число поколений до стабилизации");
                plt.SaveFig(plotFile);
                
                Console.WriteLine($"\nГрафик сохранён в {Path.GetFullPath(plotFile)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nОшибка при сохранении графика: {ex.Message}");
                Console.WriteLine("График не сохранён, но данные доступны в Data/data.txt");
            }
        }
        
        static int RunToStability()
        {
            int maxGen = 500;
            int stableCounter = 0;
            int prevLiveCount = board.CountLiveCells();
            int lastChangeGen = 0;
            
            for (int gen = 0; gen < maxGen; gen++)
            {
                board.Advance();
                int currentLiveCount = board.CountLiveCells();
                
                if (currentLiveCount == prevLiveCount)
                {
                    stableCounter++;
                    if (stableCounter >= 15)
                    {
                        return gen;
                    }
                }
                else
                {
                    stableCounter = 0;
                    lastChangeGen = gen;
                }
                
                prevLiveCount = currentLiveCount;
            }
            
            return maxGen;
        }
    }
}
