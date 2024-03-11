using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SnakeGame
{
    public class SnakeGame
    {
        private const int _delay = 300;

        private readonly Snake _snake; // Змейка
        private readonly Food _food; // Еда
        private Controls _direction; // Куда змейка ползет
        private CancellationTokenSource _cts = null;
        private bool _addDelay;

        public List<List<Cell>> GameArea { get; } // игровое поле

        public Controls Controls
        {
            get => _direction;
            set
            {
                if (value != _direction && (int)value % 2 != (int)_direction % 2)
                {
                    _direction = value;
                    Update(); // немедленная реакция на нажатие
                }
            }
        }

        // создание новой игры
        public SnakeGame()
        {
            int width = 16;
            int height = 16;
            GameArea = new List<List<Cell>>();
            for (int i = 0; i < height; i++)
            {
                List<Cell> row = new List<Cell>();
                for (int j = 0; j < width; j++)
                {
                    row.Add(new Cell());
                }
                GameArea.Add(row);
            }
            _food = new Food(GameArea, 10, 2);
            _snake = new Snake(this, _food, new Position(GameArea[0].Count / 2, GameArea.Count / 2), 1, Controls.Right);
        }

        // запустить игру
        public void Start()
        {
            if (_cts == null)
                Run();
        }

        // остановить игру
        public void Stop()
        {
            _cts?.Cancel();
        }

        // а здесь змейка в цикле ползает, обратите внимание на то что метод "async" - асинхронный
        private async void Run()
        {
            using (_cts = new CancellationTokenSource())
            {
                try
                {
                    while (true) // повторять, пока не надоест
                    {
                        if (_snake.Died) // если змейка умерла
                        {
                            break;
                        }
                        else
                            Update(); // Обновить игровое состояние

                        await Task.Delay(_delay, _cts.Token); // а вот единственная асинхронная операция
                        if (_addDelay)
                        {
                            _addDelay = false;
                            await Task.Delay(_delay / 2, _cts.Token);
                        }
                    }
                }
                catch (OperationCanceledException) { } // была остановка?
                catch (Exception ex) // была другая ошибка?
                {
                    MessageBox.Show(ex.Message);
                }
            }
            _cts = null;

        }

        // начисляет 10 очков за каждую найденную еду
        public void GiveScore()
        {
            //_viewModel.Score += 10;
        }

        // обновляет игровое состояние
        public void Update()
        {
            _snake.Move(Controls);
            _food.Update();
        }
    }
}
