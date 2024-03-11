using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
{
    public class Snake
    {
        private readonly Queue<Position> _tail; // хвост змейки
        private readonly Food _food; // змейка взаимодействует с едой
        private readonly SnakeGame _game; // там есть игровое поле

        private Position _head; // голова змейки
        public int _length; // длина змейки без учета головы
        public bool Died { get; private set; } // показывает, жива ли змейка

        public Position Head // голова
        {
            get => _head;
            private set
            {
                _head = value;
                // отобразить голову на арене
                _game.GameArea[value.Y][value.X].State = GridSetting.Snake;
            }
        }

        // создать новую змейку
        public Snake(SnakeGame game, Food food, Position head, int length, Controls direction)
        {
            _game = game;
            _food = food;
            _tail = new Queue<Position>();
            Head = head;
            _length = length;
            while (_tail.Count < _length)
                Move(direction); // проползти немного, чтобы змейка была во всю длину на поле
        }

        // ползти на одну клетку в направлении direction
        public void Move(Controls direction)
        {
            Position coords = Head;
            switch (direction)
            {
                case Controls.Right:
                    coords = new Position(coords.X + 1, coords.Y);
                    break;
                case Controls.Down:
                    coords = new Position(coords.X, coords.Y + 1);
                    break;
                case Controls.Left:
                    coords = new Position(coords.X - 1, coords.Y);
                    break;
                case Controls.Up:
                    coords = new Position(coords.X, coords.Y - 1);
                    break;
            }
            if (!CheckMove(coords)) // а можно ли туда ползти?
                return;
            _tail.Enqueue(Head); // старая голова стала началом хвоста
            Head = coords; // новая голова

            while (_tail.Count > _length) // если хвост длиннее, чем нужно
            {
                Position tail = _tail.Dequeue(); // отрезать клетку от хваоста
                _game.GameArea[tail.Y][tail.X].State = GridSetting.Empty; // и отобразить это на игровом поле
            }
        }

        // проверка следующего хода
        private bool CheckMove(Position coords)
        {
            // если выползем за пределы или врежемся в себя
            if (coords.X >= _game.GameArea[0].Count || coords.X < 0 || coords.Y >= _game.GameArea.Count || coords.Y < 0 || _game.GameArea[coords.Y][coords.X].State == GridSetting.Snake)
                Died = true; // то умрём
            else
            if (_game.GameArea[coords.Y][coords.X].State == GridSetting.Food) // иначе может попасться еда
            {
                _food.FoodCount--; // сказать еде, что ее стало меньше
                _length++; // вырастить хвост
                _game.GiveScore(); // дать игроку очки
            }
            return !Died;
        }
    }
}
