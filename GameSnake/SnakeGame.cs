using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace GameSnake
{ 
    public class SnakeGame : INotifyPropertyChanged
    {
        private Random random = new Random();

        public byte Rows { get; }
        public byte Cols { get; }
        public GridStates[,] Grid { get; private set; }
        public Direction PrevDir { get; private set; } //Прошлое направление необходимо для невозможности нажать направление "назад" во время игры
        public Direction Dir { get; private set; }
        private int score;
        public int Score { get { return score; } private set { score = value; OnPropertyChanged("Score"); } }
        public int Buffer { get; private set; }
        private readonly int specialChance = 4; //3 будет означать шанс 1/3, 4 - 1/4 и т.д., что появится золотая еда на 3 очка
        private bool gameStarted;
        public bool GameStarted { get { return gameStarted; } set { gameStarted = value; OnPropertyChanged("GameStarted"); } }
        private bool gameOver;
        public bool GameOver { get { return gameOver; } set { gameOver = value; OnPropertyChanged("GameOver"); } }

        private readonly LinkedList<GridPosition> nodesPositions = new LinkedList<GridPosition>();

        public SnakeGame(byte rows, byte cols, int level)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridStates[Rows, Cols];
            GameOver = false;
            GameStarted = false;

            if (level!=0)
                NewGame(level);
        }

        public void NewGame(int level)
        {
            Buffer = 0;
            Score = 0;
            switch (level)
            {
                case 1: //Змейка
                    PrevDir = Direction.Right;
                    Dir = Direction.Right;

                    CreateSnake(Rows/2);
                    CreateFood();
                    break;
                case 2: //Обжорство
                    PrevDir = Direction.Right;
                    Dir = Direction.Right;

                    CreateSnake(Rows/2);
                    for (int i = 0; i < Cols; i++)
                    {
                        CreateWall(0, i);
                        CreateWall(Rows-1, i);
                    }
                    for (int i = 0; i < Rows; i++)
                    {
                        CreateWall(i, 0);
                        CreateWall(i, Cols-1);
                    }
                    CreateFood();
                    CreateFood();
                    CreateFood();
                    break;
                case 3: //Бегущий по грани
                    PrevDir = Direction.Right;
                    Dir = Direction.Right;

                    CreateSnake(0);
                    for (int i = 2; i < Cols - 2; i++)
                    {
                        for (int j = 2; j < Rows - 2; j++)
                        {
                            CreateWall(j, i);
                        }
                    }
                    for (int i = 0; i < Cols; i++)
                    {
                        for (int j = Rows / 2; j <= Rows / 2+1; j++)
                        {
                            CreateWall(j, i);
                        }
                    }
                    CreateFood();
                    break;
                case 4: //Шоссе, поменять скорость игры
                    PrevDir = Direction.Right;
                    Dir = Direction.Right;

                    CreateSnake(Rows/2);
                    for (int i = 0; i < Cols / 2 - 1; i++)
                    {
                        for (int j = 0; j < Rows / 2 - 1; j++)
                        {
                            CreateWall(j, i);
                        }
                    }
                    for (int i = Cols / 2 + 1; i < Cols; i++)
                    {
                        for (int j = 0; j < Rows / 2 - 1; j++)
                        {
                            CreateWall(j, i);
                        }
                    }
                    for (int i = 0; i < Cols / 2 - 1; i++)
                    {
                        for (int j = Rows / 2 + 1; j < Rows; j++)
                        {
                            CreateWall(j, i);
                        }
                    }
                    for (int i = Cols / 2 + 1; i < Cols; i++)
                    {
                        for (int j = Rows / 2 + 1; j < Rows; j++)
                        {
                            CreateWall(j, i);
                        }
                    }
                    CreateFood();
                    break;
                case 5: //Ветрянка
                    PrevDir = Direction.Right;
                    Dir = Direction.Right;

                    CreateSnake(0);
                    for (int i = 2; i < Cols; i+=3)
                    {
                        for (int j = 2; j < Rows; j+=3)
                        {
                            CreateWall(j-1, i-1);
                            CreateWall(j-1, i);
                            CreateWall(j, i-1);
                            CreateWall(j, i);
                        }
                    }
                    
                    CreateFood();
                    break;
            }

            
            GameStarted = true;
        }

        private IEnumerable<GridPosition> EmptyCells()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (Grid[i, j] == GridStates.Empty)
                    {
                        yield return new GridPosition(i, j); //yield return нужен так как IEnumerable содержит итерации по коллекции
                    }
                }
            }
        }

        #region SnakeNodes
        private void CreateSnake(int startingRow)
        {
            for (int i = 1; i < 4; i++)
            {
                Grid[startingRow, i] = GridStates.Snake;
                nodesPositions.AddFirst(new GridPosition(startingRow, i));
            }
        }

        public IEnumerable<GridPosition> SnakePositions()
        {
            return nodesPositions;
        }

        public GridPosition SnakeHead()
        {
            return nodesPositions.First.Value;
        }

        public GridPosition SnakeTail()
        {
            return nodesPositions.Last.Value;
        }

        public void CreateHead(GridPosition gridPosition)
        {
            if (nodesPositions.Count > 1)
            {
                GridPosition prevPosition = SnakeHead();
                Grid[prevPosition.Row, prevPosition.Col] = GridStates.Snake;
            }

            nodesPositions.AddFirst(gridPosition);
            Grid[gridPosition.Row, gridPosition.Col] = GridStates.SnakeHead;

            
        }

        public void SnakeBody(GridPosition gridPosition)
        { 
            if (nodesPositions.Count>1)
                Grid[gridPosition.Row, gridPosition.Col] = GridStates.SnakeHead;
        }

        public void RemoveTail()
        {
            GridPosition tail = SnakeTail();
            Grid[tail.Row, tail.Col] = GridStates.Empty;
            nodesPositions.RemoveLast();
        }
        #endregion
        #region SnakeMove

        public void ChangeDir(Direction direction)
        {
            if (!(PrevDir.HorMove==-direction.HorMove && PrevDir.VerMove == -direction.VerMove))
                Dir = direction;
        }

        /*private bool OutsideGrid(GridPosition position)
        {
            return position.Row >= Rows || position.Row < 0 || position.Col >= Cols || position.Col < 0;
        }*/

        private GridPosition NewPositionBorder(GridPosition dir)
        {
            if (dir.Row >= Rows)
            {
                Direction ZeroDown = new Direction(-dir.Row+1, 0);
                return SnakeHead().NewPosition(ZeroDown);
            }
            else if (dir.Row < 0)
            {
                Direction ZeroUp = new Direction(Rows - 1, 0);
                return SnakeHead().NewPosition(ZeroUp);
            }
            else if (dir.Col >= Cols)
            {
                Direction ZeroRight = new Direction(0, -dir.Col+1);
                return SnakeHead().NewPosition(ZeroRight);
            }
            else if (dir.Col < 0)
            {
                Direction ZeroLeft = new Direction(0, Cols - 1);
                return SnakeHead().NewPosition(ZeroLeft);
            }
            else
            {
                return SnakeHead().NewPosition(Dir);
            }
        }

        private GridStates CheckHit(GridPosition nextPosition)
        {
            //System.Windows.MessageBox.Show($"{nextPosition.Row}, {nextPosition.Col}, {nextPosition.ToString()}");

            if (nextPosition == SnakeTail())
            {
                return GridStates.Empty;
            }

            return Grid[nextPosition.Row, nextPosition.Col];
        }

        public void Move()
        {
            GridPosition nextPosition = NewPositionBorder(SnakeHead().NewPosition(Dir));
            GridStates hit = CheckHit(nextPosition);
            PrevDir = Dir;

            if (hit == GridStates.Snake || hit == GridStates.Wall)
            {
                GameOver = true;
                GameStarted = false;
            }
            if (hit == GridStates.Empty)
            {
                if (Buffer > 0)
                    Buffer--;
                else
                    RemoveTail();
                CreateHead(nextPosition);
            }
            if (hit == GridStates.Food)
            {
                CreateHead(nextPosition);
                CreateFood();
                Score++;
            }
            if (hit == GridStates.SpecialFood)
            {
                CreateHead(nextPosition);
                CreateFood();
                Score += 3;
                Buffer += 2;
            }
        }
        #endregion
        

        private void CreateWall(int rows, int cols)
        {
            List<GridPosition> empty = new List<GridPosition>(EmptyCells());

            if (empty.Count != 0)
            {
                Grid[rows, cols] = GridStates.Wall;
            }
        }

        private void CreateFood() //Для случайной расстановки на пустых клетках
        {
            List<GridPosition> empty = new List<GridPosition>(EmptyCells());
            int chances = random.Next(specialChance);

            if (empty.Count != 0)
            {
                GridPosition gridPosition = empty[random.Next(empty.Count)];
                if (chances!=specialChance-1)
                    Grid[gridPosition.Row, gridPosition.Col] = GridStates.Food;
                else
                    Grid[gridPosition.Row, gridPosition.Col] = GridStates.SpecialFood;
            }
        }

        private void CreateFood(int row, int col) //Для ручной расстановки в начале уровня
        {
            Grid[row, col] = GridStates.Food;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
