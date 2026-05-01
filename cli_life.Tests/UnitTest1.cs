using Microsoft.VisualStudio.TestTools.UnitTesting;
using cli_life;
using System;
using System.IO;
using System.Linq;

namespace cli_life.Tests
{
    [TestClass]
    public class CellTests
    {
        [TestMethod]
        public void TestCellInitialState()
        {
            Cell cell = new Cell();
            Assert.IsFalse(cell.IsAlive);
        }

        [TestMethod]
        public void TestCellToggle()
        {
            Cell cell = new Cell();
            cell.IsAlive = true;
            Assert.IsTrue(cell.IsAlive);
        }

        [TestMethod]
        public void TestUnderpopulationDeath()
        {
            Cell cell = new Cell();
            cell.IsAlive = true;
            cell.neighbors.Clear();
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.IsFalse(cell.IsAlive);
        }

        [TestMethod]
        public void TestSurvivalTwoNeighbors()
        {
            Cell cell = new Cell();
            cell.IsAlive = true;
            
            Cell neighbor1 = new Cell() { IsAlive = true };
            Cell neighbor2 = new Cell() { IsAlive = true };
            
            cell.neighbors.Add(neighbor1);
            cell.neighbors.Add(neighbor2);
            
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.IsTrue(cell.IsAlive);
        }

        [TestMethod]
        public void TestSurvivalThreeNeighbors()
        {
            Cell cell = new Cell();
            cell.IsAlive = true;
            
            for (int i = 0; i < 3; i++)
                cell.neighbors.Add(new Cell() { IsAlive = true });
            
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.IsTrue(cell.IsAlive);
        }

        [TestMethod]
        public void TestOverpopulationDeath()
        {
            Cell cell = new Cell();
            cell.IsAlive = true;
            
            for (int i = 0; i < 4; i++)
                cell.neighbors.Add(new Cell() { IsAlive = true });
            
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.IsFalse(cell.IsAlive);
        }

        [TestMethod]
        public void TestNoBirth()
        {
            Cell cell = new Cell();
            cell.IsAlive = false;
            
            for (int i = 0; i < 2; i++)
                cell.neighbors.Add(new Cell() { IsAlive = true });
            
            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.IsFalse(cell.IsAlive);
        }
    }

    [TestClass]
public class AdvancedBoardTests
{
    [TestMethod]
    public void TestGetClusters()
    {
        Board board = new Board(10, 10, 1, 0);
        // Создаём два отдельных кластера
        bool[,] block = { { true, true }, { true, true } };
        bool[,] blinker = { { true, true, true } };
        
        board.SetFigure(2, 2, block);
        board.SetFigure(6, 5, blinker);
        
        var clusters = board.GetClusters();
        
        // Должно быть 2 кластера: блок (4 клетки) и мигалка (3 клетки)
        Assert.AreEqual(2, clusters.Count);
        Assert.AreEqual(4, clusters[0].Count);
        Assert.AreEqual(3, clusters[1].Count);
    }
    
    [TestMethod]
    public void TestConfigurationStability()
    {
        Board board = new Board(10, 10, 1, 0);
        // Блок - стабильная фигура
        bool[,] block = { { true, true }, { true, true } };
        board.SetFigure(4, 4, block);
        
        string hash1 = board.GetConfigurationHash();
        board.Advance();
        string hash2 = board.GetConfigurationHash();
        
        // Блок не должен меняться
        Assert.AreEqual(hash1, hash2);
    }
    
    [TestMethod]
    public void TestLoadNonExistentFile()
    {
        Board board = null;
        Exception caughtException = null;
        
        try
        {
            board = Board.LoadFromFile("non_existent_file_12345.json");
        }
        catch (FileNotFoundException ex)
        {
            caughtException = ex;
        }
        
        Assert.IsNotNull(caughtException);
        Assert.IsTrue(caughtException.Message.Contains("не найден"));
    }
    
    [TestMethod]
    public void TestBoardClone()
    {
        Board original = new Board(20, 20, 1, 0.3);
        Board clone = original.Clone();
        
        // Проверяем, что клоны одинаковы
        Assert.AreEqual(original.GetConfigurationHash(), clone.GetConfigurationHash());
        Assert.AreEqual(original.CountLiveCells(), clone.CountLiveCells());
        
        // Изменяем оригинал
        original.Advance();
        
        // Клон не должен измениться
        Assert.AreNotEqual(original.GetConfigurationHash(), clone.GetConfigurationHash());
    }
    
    [TestMethod]
    public void TestGliderPattern()
    {
        Board board = new Board(20, 20, 1, 0);
        bool[,] glider = { 
            { false, false, true },
            { true, false, true },
            { false, true, true }
        };
        board.SetFigure(8, 8, glider);
        
        var clusters = board.GetClusters();
        Assert.AreEqual(1, clusters.Count);
        
        string pattern = board.GetPattern(clusters[0]);
        Assert.AreEqual("Глидер", pattern);
    }
    
    [TestMethod]
    public void TestIsConfigurationEqualTo()
    {
        Board board1 = new Board(10, 10, 1, 0);
        Board board2 = new Board(10, 10, 1, 0);
        
        bool[,] block = { { true, true }, { true, true } };
        board1.SetFigure(4, 4, block);
        board2.SetFigure(4, 4, block);
        
        Assert.IsTrue(board1.IsConfigurationEqualTo(board2));
        
        board2.Advance();
        Assert.IsFalse(board1.IsConfigurationEqualTo(board2));
    }
}

    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void TestBoardInitialization()
        {
            Board board = new Board(50, 50, 1, 0);
            Assert.AreEqual(50, board.Columns);
            Assert.AreEqual(50, board.Rows);
            Assert.AreEqual(0, board.CountLiveCells());
        }

        [TestMethod]
        public void TestBlinkerEvolution()
        {
            Board board = new Board(10, 10, 1, 0);
            bool[,] blinker = { { true, true, true } };
            board.SetFigure(4, 5, blinker);
            
            // Горизонтальная мигалка
            Assert.IsTrue(board.Cells[4, 5].IsAlive);
            Assert.IsTrue(board.Cells[5, 5].IsAlive);
            Assert.IsTrue(board.Cells[6, 5].IsAlive);
            
            board.Advance();
            
            // Стала вертикальной
            Assert.IsTrue(board.Cells[5, 4].IsAlive);
            Assert.IsTrue(board.Cells[5, 5].IsAlive);
            Assert.IsTrue(board.Cells[5, 6].IsAlive);
            
            board.Advance();
            
            // Снова горизонтальная
            Assert.IsTrue(board.Cells[4, 5].IsAlive);
            Assert.IsTrue(board.Cells[5, 5].IsAlive);
            Assert.IsTrue(board.Cells[6, 5].IsAlive);
        }

        [TestMethod]
        public void testSaveAndLoad()
        {
            Board original = new Board(10, 10, 1, 0);
            bool[,] block = { { true, true }, { true, true } };
            original.SetFigure(4, 4, block);
            original.Generation = 5;
            
            string filename = "test_board.json";
            original.SaveToFile(filename);
            
            Board loaded = Board.LoadFromFile(filename);
            
            Assert.AreEqual(original.Columns, loaded.Columns);
            Assert.AreEqual(original.Rows, loaded.Rows);
            Assert.AreEqual(original.Generation, loaded.Generation);
            Assert.AreEqual(original.CountLiveCells(), loaded.CountLiveCells());
            
            File.Delete(filename);
        }

        [TestMethod]
        public void TestCountLiveCells()
        {
            Board board = new Board(10, 10, 1, 0);
            Assert.AreEqual(0, board.CountLiveCells());
            
            board.Cells[0, 0].IsAlive = true;
            board.Cells[1, 1].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;
            
            Assert.AreEqual(3, board.CountLiveCells());
        }

        [TestMethod]
        public void TestBeehivePattern()
        {
            Board board = new Board(20, 20, 1, 0);
            bool[,] beehive = {
                { false, true, true, false },
                { true, false, false, true },
                { false, true, true, false }
            };
            board.SetFigure(8, 8, beehive);
            
            var clusters = board.GetClusters();
            string pattern = board.GetPattern(clusters[0]);
            Assert.AreEqual("Улей", pattern);
        }

        [TestMethod]
        public void TestBoatPattern()
        {
            Board board = new Board(20, 20, 1, 0);
            bool[,] boat = {
                { true, true, false },
                { true, false, true },
                { false, true, false }
            };
            board.SetFigure(8, 8, boat);
            
            var clusters = board.GetClusters();
            string pattern = board.GetPattern(clusters[0]);
            Assert.AreEqual("Лодка", pattern);
        }

        [TestMethod]
        public void TestSetFigureBoundaries()
        {
            Board board = new Board(10, 10, 1, 0);
            bool[,] figure = { { true, true }, { true, true } };
            
            // Попытка установить фигуру за границами
            board.SetFigure(9, 9, figure);
            
            // Должна установиться только часть фигуры
            Assert.IsTrue(board.Cells[9, 9].IsAlive);
            Assert.IsFalse(board.Cells[9, 10].IsAlive); // За границей
        }
    }

    [TestClass]
    public class ConfigTests
    {
        [TestMethod]
        public void TestConfigLoadSave()
        {
            Config config = new Config();
            config.BoardWidth = 100;
            config.BoardHeight = 50;
            config.LiveDensity = 0.3;
            
            string filename = "test_config.json";
            config.Save(filename);
            
            Config loaded = Config.Load(filename);
            
            Assert.AreEqual(100, loaded.BoardWidth);
            Assert.AreEqual(50, loaded.BoardHeight);
            Assert.AreEqual(0.3, loaded.LiveDensity);
            
            File.Delete(filename);
        }

        [TestMethod]
        public void TestConfigDefaultValues()
        {
            Config config = new Config();
            Assert.AreEqual(80, config.BoardWidth);
            Assert.AreEqual(40, config.BoardHeight);
            Assert.AreEqual(0.2, config.LiveDensity);
            Assert.AreEqual(100, config.DelayMs);
            Assert.AreEqual(1000, config.MaxGenerations);
        }
    }
}