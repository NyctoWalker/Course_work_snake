using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame
{
    public class Food
    {
        private readonly int _foodDelay; // задержка между появлением еды в игровых ходах
        private readonly int _maxFood; // максимальное количество еды на поле
        private readonly Random _rnd; // генератор случайных чисел
        public readonly List<List<Cell>> _area; // ссылка на игровое поле

        private int tick; // сколько ходов прошло с момента последнего появления еды

        public int FoodCount { get; set; } // сколько сейчас еды на поле

        public Food(List<List<Cell>> area, int foodDelay, int maxFood)
        {
            _rnd = new Random();
            _area = area;
            _foodDelay = foodDelay;
            _maxFood = maxFood;
        }

        // добавить еду
        public void Update()
        {
            if (tick >= _foodDelay && FoodCount < _maxFood)
            {
                tick = 0;
                while (true)
                {
                    Position coords = new Position(_rnd.Next(_area[0].Count), _rnd.Next(_area.Count)); // выбрать случайную клетку
                    if (_area[coords.Y][coords.X].State == GridSetting.Empty) // если там пусто
                    {
                        _area[coords.Y][coords.X].State = GridSetting.Food; // нарисовать еду
                        FoodCount++; // учесть, еды стало больше
                        break;
                    }
                }
            }
            else
                tick++; // +1 ход не было еды
        }
    }
}
