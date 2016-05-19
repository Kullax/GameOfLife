using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

/// <remarks>
/// A simple Implementation of the GameOfLife, which will create a Game of default size 20x20 and populate it randomly with alive and dead cells.
/// New iterations of the GameBoard are calculated upon mouse click within the window of the game.
/// </remarks>
namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Will Dynamicly create a simple grid of a determined size, overwriting any content which may be present in the .xaml file.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Instead of working with labels, which are always drawn, 
        // I've chosen now to implement it as rectangles being drawn, allowing me to not draw dead cells.
        Canvas canvas;
        int index = 0;
        // size of the game of life, can be edited here.
        private int rows = 100;
        private int columns = 100;
        GameBoard gameBoard;

        // A list which will contain the Cells which needs to change status in between iterations.
        List<Cell> nextGen = new List<Cell>();

        public MainWindow()
        {
            canvas = new Canvas();
            canvas.Background = Brushes.GhostWhite;

            canvas.Width = 570;
            canvas.Height = 550;

            InitializeComponent();

            // Only used for initializing the game, assigning random states of life
            Random rand = new Random();

            gameBoard = new GameBoard(columns, rows);

            // Simple mouse capture, if the window is clicked, the next iteration is called.
            this.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.mouseClick);

            // populate the game with cells
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    // Increase the value for Next(2) to make life less common.
                    
                    Cell cell = new Cell(rand.Next(2) == 0, i, j);
                    gameBoard.AddCell(cell, i, j);
                    // Draws initial board.
                    if(cell.isAlive)
                        DrawRectangle(i,j, cell.isAlive);
                }
            }
            // overwrites any content which may have been written in the .xaml file, which is none.
            this.Content = canvas;
        }

        /// <summary>
        /// Will draw a rectangle, of the proper size, on a given location
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="alive">Determines the colour of the rectangle, alive is green, dead is white.</param>
        private void DrawRectangle(int i, int j, bool alive)
        {
            Rectangle rect = new Rectangle();
            rect.Width = canvas.Width / columns;
            rect.Height = canvas.Height / rows;
            rect.Stroke = Brushes.Black;
            if (columns < 100 || rows < 100)
                rect.StrokeThickness = 1;
            else
                rect.StrokeThickness = 0;
            // Drawing every cell, even the dead ones, is expensive.
            // But it should be an avaliable option
            if (alive)
                rect.Fill = Brushes.Green;
            else
                rect.Fill = Brushes.GhostWhite;
            Canvas.SetLeft(rect, i * canvas.Width / columns);
            Canvas.SetTop(rect, j * canvas.Height / rows);
            canvas.Children.Insert(index, rect);
            index++;
        }

        /// <summary>
        /// For a given cell, checks if the neighbor cells allows it to either become/stay alive, or die.
        /// </summary>
        /// <param name="cell">A cell within a constructed GameBoard</param>
        /// <returns>True if the conditions for life a ideal</returns>
        bool Check_Neighbours(Cell cell)
        {
            // alive is the number of alive neighbor cells.
            int alive = 0;

            // Checks all eight neighbors if they are alive.
            if (gameBoard.CellDown(cell).isAlive)
                alive += 1;
            if (gameBoard.CellUp(cell).isAlive)
                alive += 1;
            if (gameBoard.CellLeft(cell).isAlive)
                alive += 1;
            if (gameBoard.CellRight(cell).isAlive)
                alive += 1;
            // If alive is already 4 or above, no need to compute neighbor
            if (alive < 4 && gameBoard.CellUpLeft(cell).isAlive)
                alive += 1;
            if (alive < 4 && gameBoard.CellUpRight(cell).isAlive)
                alive += 1;
            if (alive < 4 && gameBoard.CellDownLeft(cell).isAlive)
                alive += 1;
            if (alive < 4 && gameBoard.CellDownRight(cell).isAlive)
                alive += 1;

            if (cell.isAlive && alive < 2)
                return true;
            if (cell.isAlive && alive > 3)
                return true;
            if (!cell.isAlive && alive == 3)
                return true;
            return false;
        }

        /// <summary>
        /// Will handle the computations needed for the next iteration to take place, and then update the state of the game.
        /// </summary>
        void NextIteration()
        {
            // Instead of recreating a list, reuse the same List.
            nextGen.Clear();
            // Constructs a temporary list which will hold the Cells which needs to Toggle their alive status.
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (Check_Neighbours(gameBoard[i, j]))
                        nextGen.Add(gameBoard[i, j]);
                }
            }

            // After computing the next game board, refresh the actual game
            foreach (Cell cell in nextGen)
            {
                cell.ToggleAlive();
            }

            // Now refresh the gameBoard.
            DrawBoard();
        }

        /// <summary>
        /// Draws the current gameBoard, drawing alive cells only.
        /// </summary>
        private void DrawBoard()
        {
            index = 0;
            canvas.Children.Clear();
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (gameBoard[i, j].isAlive)
                    {
                        DrawRectangle(i, j, true);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the mouse click. Will simply call the NextIteration function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mouseClick(object sender, System.EventArgs e)
        {
                NextIteration();
        }
    }

    /// <summary>
    /// Previously an extended label, but not anymore, as it would always be drawn, slowing down the game when going bigger.
    /// Includes information about cell location and state, being dead or alive.
    /// </summary>
    public class Cell
    {
        private bool alive;

        // Since I'm working in multidimensional arrays, I cannot easily find the index location of a cell
        // Hence I'm breaking a bit of Object Oriented Design allowing the cells to know their location within
        // the game board.
        public Tuple<int, int> index;

        /// <summary>
        /// True if the cell is currently alive
        /// </summary>
        public bool isAlive
        {
            get { return alive; }
        }

        /// <summary>
        /// Constructs a new Cell. A cell is smart, and knows its position within the GameBoard.
        /// </summary>
        /// <param name="alive">State of the Cell</param>
        /// <param name="column">The Column position of the Cell</param>
        /// <param name="row">The Row position of the Cell</param>
        public Cell(bool alive, int column, int row)
        {
            index = new Tuple<int, int>(column, row);
            if (alive)
                ToggleAlive();
        }

        /// <summary>
        /// Will kill any alive Cells or revive any dead Cells.
        /// </summary>
        public void ToggleAlive()
        {
            alive = !alive;
        }
    }

    /// <summary>
    /// A simple multidimensional array, which holds Cells.
    /// </summary>
    public class GameBoard
    {
        Cell[,] gameBoard;
        private int rows, columns;

        /// <summary>
        /// A GameBoard is a square or size [columns X rows].
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="rows"></param>
        public GameBoard(int columns, int rows)
        {
            this.rows = rows;
            this.columns = columns;
            gameBoard = new Cell[columns, rows];
        }

        /// <summary>
        /// Allows for indexing on the GameBoard, like a multidimensional array.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public Cell this[int columns, int rows]
        {
            get
            {
                return gameBoard[columns, rows];
            }
        }

        /// <summary>
        /// Adds a Cell to the GameBoard
        /// </summary>
        /// <param name="cell">Cell to be added</param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        public void AddCell(Cell cell, int column, int row)
        {
            gameBoard[column, row] = cell;
        }

        /// <summary>
        /// Returns the Upwards neighbor of a given Cell. If direction exceeds a border, any direction will be wrapped around the GameBoard.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Cell CellUp(Cell cell)
        {
            if (cell.index.Item2 == 0)
                return gameBoard[cell.index.Item1, rows - 1];
            return gameBoard[cell.index.Item1, cell.index.Item2 - 1];
        }
        /// <summary>
        /// Returns the Downwards neighbor of a given Cell. If direction exceeds a border, any direction will be wrapped around the GameBoard.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Cell CellDown(Cell cell)
        {
            if (cell.index.Item2 == rows - 1)
                return gameBoard[cell.index.Item1, 0];
            return gameBoard[cell.index.Item1, cell.index.Item2 + 1];
        }

        /// <summary>
        /// Returns the Rightside neighbor of a given Cell. If direction exceeds a border, any direction will be wrapped around the GameBoard.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Cell CellRight(Cell cell)
        {
            if (cell.index.Item1 == columns - 1)
                return gameBoard[0, cell.index.Item2];
            return gameBoard[cell.index.Item1 + 1, cell.index.Item2];
        }

        /// <summary>
        /// Returns the Leftside neighbor of a given Cell. If direction exceeds a border, any direction will be wrapped around the GameBoard.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Cell CellLeft(Cell cell)
        {
            if (cell.index.Item1 == 0)
                return gameBoard[columns - 1, cell.index.Item2];
            return gameBoard[cell.index.Item1 - 1, cell.index.Item2];
        }

        /// <summary>
        /// Returns the TopLeft neighbor of a given Cell. If direction exceeds a border, any direction will be wrapped around the GameBoard.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Cell CellUpLeft(Cell cell)
        {
            return CellUp(CellLeft(cell));
        }

        /// <summary>
        /// Returns the TopRight neighbor of a given Cell. If direction exceeds a border, any direction will be wrapped around the GameBoard.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Cell CellUpRight(Cell cell)
        {
            return CellUp(CellRight(cell));
        }

        /// <summary>
        /// Returns the DownLeft neighbor of a given Cell. If direction exceeds a border, any direction will be wrapped around the GameBoard.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Cell CellDownLeft(Cell cell)
        {
            return CellDown(CellLeft(cell));
        }

        /// <summary>
        /// Returns the DownRight neighbor of a given Cell. If direction exceeds a border, any direction will be wrapped around the GameBoard.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Cell CellDownRight(Cell cell)
        {
            return CellDown(CellRight(cell));
        }
    }
}