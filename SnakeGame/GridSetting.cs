using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SnakeGame
{
    public enum GridSetting
    {
        Empty,
        Snake,
        Food
    }
    ///Возможно стены будут аналогичны snake, outofborder нужен для каки-нибудь проверок

    public enum Controls
    { 
        Right,
        Up,
        Left,
        Down
    }

    public struct Position
    { 
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Cell : INotifyPropertyChanged
    {
        private GridSetting _state;

        public GridSetting State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
