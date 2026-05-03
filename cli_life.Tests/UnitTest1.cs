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
        bool[,] block = { { true, true }, { true, true } };
        bool[,] blinker = { { true, true, true } };
        
        board.SetFigure(2, 2, block);
        board.SetFigure(6, 5, blinker);
        
        var clusters = board.GetClusters();
        
        Assert.AreEqual(2, clusters.Count);
        Assert.AreEqual(4, clusters[0].Count);
        Assert.AreEqual(3, clusters[1].Count);
    }
    
    [TestMethod]
    public void TestConfigurationStability()
    {
        Board board = new Board(10, 10, 1, 0);
        bool[,] block = { { true, true }, { true, true } };
        board.SetFigure(4, 4, block);
        
        string hash1 = GetBoardHash(board);
        board.Advance();
        string hash2 = GetBoardHash(board);

        Assert.AreEqual(hash1, hash2);
    }
    
    [TestMethod]
    public void TestBoardClone()
    {
        Board original = new Board(20, 20, 1, 0.3);
        Board clone = CloneBoard(original);
        
        Assert.AreEqual(GetBoardHash(original), GetBoardHash(clone));
        Assert.AreEqual(original.CountLiveCells(), clone.CountLiveCells());

        original.Advance();

        Assert.AreNotEqual(GetBoardHash(original), GetBoardHash(clone));
    }

    private string GetBoardHash(Board board)
        {
            var hash = new System.Text.StringBuilder();
            for (int y = 0; y < board.Rows; y++)
            {
                for (int x = 0; x < board.Columns; x++)
                {
                    hash.Append(board.Cells[x, y].IsAlive ? "1" : "0");
                }
            }
            return hash.ToString();
        }
        
        private Board CloneBoard(Board original)
        {
            Board clone = new Board(original.Columns, original.Rows, 1, 0);
            for (int x = 0; x < original.Columns; x++)
            {
                for (int y = 0; y < original.Rows; y++)
                {
                    clone.Cells[x, y].IsAlive = original.Cells[x, y].IsAlive;
                }
            }
            clone.Generation = original.Generation;
            return clone;
        }
        
        private bool AreBoardsEqual(Board board1, Board board2)
        {
            if (board1.Columns != board2.Columns || board1.Rows != board2.Rows)
                return false;
                
            for (int x = 0; x < board1.Columns; x++)
            {
                for (int y = 0; y < board1.Rows; y++)
                {
                    if (board1.Cells[x, y].IsAlive != board2.Cells[x, y].IsAlive)
                        return false;
                }
            }
            return true;
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
            Assert.AreEqual(1, clusters.Count);
            string pattern = board.GetPattern(clusters[0]);
            Assert.AreEqual("Лодка", pattern);
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