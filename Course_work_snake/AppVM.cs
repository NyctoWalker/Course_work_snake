using GameSnake;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace Course_work_snake
{
    public class AppVM : INotifyPropertyChanged
    {
        public byte gridRows { get; private set; }
        public byte gridCols { get; private set; }
        public SnakeGame game;
        public int Score { get { return game.Score; } }
        private int scoreTarget;
        public int ScoreTarget
        {
            get { return scoreTarget; }
            set { scoreTarget = value; OnPropertyChanged("ScoreTarget"); }
        }
        private int gameSpeed { get; set; }
        public bool GameOver { get { return game.GameOver; } }
        public bool GameStarted { get { return game.GameStarted; } }
        public string IsWin
        {
            get
            {
                if (Score >= ScoreTarget)
                {
                    return "Вы победили!";
                }
                else
                {
                    return "Вы проиграли :с";
                }
            }
        }
        public bool profilesVisibility
        {
            get
            {
                return !GameStarted;
            }
        }
        private GridStates[][] grid;
        public GridStates[][] Grid { get => grid; set { grid = value; OnPropertyChanged(); } }

        private RelayCommand _moveCommand;
        private RelayCommand _startCommand;
        private CancellationTokenSource cts = null; //Для остановки асинхронной функции

        //private readonly List<string> LevelsList = new List<string>{"Змейка", "Обжорство", "Бегущий по грани", "Шоссе", "Ветрянка"};
        public ObservableCollection<string> GameLevels { get; private set; }
        private string selectedLevel;
        public string SelectedLevel { get => selectedLevel; set { selectedLevel = value; OnPropertyChanged("SelectedLevel"); } }
        private byte startedLevel;
        public byte StartedLevel { get => startedLevel; set { startedLevel = value; OnPropertyChanged("StartedLevel"); } }
        public ObservableCollection<string> GameProfiles { get; private set; }
        private string selectedProfile;
        public string SelectedProfile
        {
            get
            {
                return selectedProfile;
            }
            set {
                selectedProfile = value;
                OnPropertyChanged("SelectedProfile");

                
                using (StreamWriter writer = new StreamWriter(@".\Profiles.txt", false))
                {
                    writer.Write(SelectedProfile);
                }
                

                currentRead();
                OnPropertyChanged("LevelRecords");
                OnPropertyChanged("WinCoefficient");
            }
        }

        #region GameStats
        private List<int> levelRecords;
        public List<int> LevelRecords
        {
            get
            {
                return levelRecords;
            }
            set
            {
                levelRecords = value;
                OnPropertyChanged("LevelRecords");
            }
        }
        private byte selectedTheme;
        //Темы оформлений в итоговой версии реализованы не были, однако всё ещё существуют в проекте и их значение записывается в файл
        public byte SelectedTheme
        {
            get
            {
                return selectedTheme;
            }
            set
            {
                selectedTheme = value;
                OnPropertyChanged("SelectedTheme");
            }
        }
        private int wins;
        public int Wins
        {
            get
            {
                return wins;
            }
            set
            {
                wins = value;
                OnPropertyChanged("Wins");
            }
        }
        private int loses;
        public int Loses
        {
            get
            {
                return loses;
            }
            set
            {
                loses = value;
                OnPropertyChanged("Loses");
            }
        }
        public int WinCoefficient
        {
            get
            {
                if (Wins + Loses != 0)
                    return ((Wins*100) / (Wins + Loses));
                else
                    return 0;
            }
        }
        #endregion

        public AppVM()
        {
            gridRows = 16;
            gridCols = 16;
            LevelRecords = new List<int> { 0, 0, 0, 0, 0};
            SelectedTheme = 0;
            Wins = 0;
            Loses = 0;

            if (!File.Exists(@".\Профиль 1.txt"))
            {
                using (StreamWriter writer = new StreamWriter(@".\Профиль 1.txt", false))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        writer.WriteLine(LevelRecords[i]);
                    }
                    writer.WriteLine(SelectedTheme);
                    writer.WriteLine(Wins);
                    writer.WriteLine(Loses);
                    //writer.WriteLine("Профиль 1");
                }
            }
            if (!File.Exists(@".\Профиль 2.txt"))
            {
                using (StreamWriter writer = new StreamWriter(@".\Профиль 2.txt", false))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        writer.WriteLine(LevelRecords[i]);
                    }
                    writer.WriteLine(SelectedTheme);
                    writer.WriteLine(Wins);
                    writer.WriteLine(Loses);
                }
            }
            if (!File.Exists(@".\Профиль 3.txt"))
            {
                using (StreamWriter writer = new StreamWriter(@".\Профиль 3.txt", false))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        writer.WriteLine(LevelRecords[i]);
                    }
                    writer.WriteLine(SelectedTheme);
                    writer.WriteLine(Wins);
                    writer.WriteLine(Loses);
                }
            }

            GameProfiles = new ObservableCollection<string> { "Профиль 1", "Профиль 2", "Профиль 3" };

            if (!File.Exists(@".\Profiles.txt"))
            {
                using (StreamWriter writer = new StreamWriter(@".\Profiles.txt", false))
                {
                    writer.Write("Профиль 1");
                }
            }
            //string[] fileContent = File.ReadAllLines(dictPath);
            SelectedProfile = File.ReadAllText(@".\Profiles.txt");
            currentRead();

            game = new SnakeGame(gridRows, gridCols, 0);
            game.GameStarted = false;
            Grid = new GridStates[gridRows][];

            GameLevels = new ObservableCollection<string> { "Змейка", "Обжорство", "Бегущий по грани", "Шоссе", "Ветрянка" };
            SelectedLevel = GameLevels[0];

            /*for (int i = 0, r=0; i < gridRows; i++)
            {
                Grid[i] = new GridStates[gridCols];
                for (int j = 0; j < gridCols; j++, r++)
                {
                    Grid[i][j]=game.Grid[i, j];
                }
            }*/

            game.PropertyChanged += MPropertyChanged;
            OnPropertyChanged("WinCoefficient");

            //OnPropertyChanged();
            //Run();
            //System.Windows.MessageBox.Show($"{Grid[8][4]}");
        }

        #region gameLoop
        private void Run()
        {
            if (cts == null)
                GameLoop();
        }

        private void Stop()
        {
            cts?.Cancel();
        }

        private async void GameLoop()
        {
            using (cts = new CancellationTokenSource()) //Это останавливает старый Game Loop при cts.Cancel
                while (!game.GameOver) //(game.GameStarted)
                {
                    game.Move();
                    Update();
                    //OnPropertyChanged("Score");
                    //System.Windows.MessageBox.Show($"{game.}");
                    await System.Threading.Tasks.Task.Delay(gameSpeed);
                }
            cts = null; //Если не использовать null вне цикла, то при GameStarted = false выдаст ошибку, токен не был утилизирован
        }

        private void Update()
        {
            Grid = new GridStates[gridRows][];
            for (int i = 0, r = 0; i < gridRows; i++)
            {
                Grid[i] = new GridStates[gridCols];
                for (int j = 0; j < gridCols; j++, r++)
                {
                    Grid[i][j] = game.Grid[i, j];
                    OnPropertyChanged();
                }
            }

            //OnPropertyChanged("game");
            //System.Windows.MessageBox.Show($"{Grid[8][4]}");
        }

        #endregion
        #region gameControls
        /*public ICommand MoveCommand => _moveCommand ??= new RelayCommand(parameter =>
        {
            if (true && Enum.TryParse(parameter.ToString(), out Direction direction))
            {
                game.ChangeDir(Direction.parameter);
            }
        });*/
        //Система очистки профиля(И функция для сохранения)
        //Темы для биндинга если пройдены уровни(светлая(базовая), тёмная(базовая), небо, трава, лава)

        public void currentRead()
        {
            string path = ".\\" + SelectedProfile + ".txt";
            if (!File.Exists(path))
            {
                LevelRecords = new List<int> { 0, 0, 0, 0, 0 };
                SelectedTheme = 0;
                Wins = 0;
                Loses = 0;
            }
            else
            {
                string[] fileContent = File.ReadAllLines(path);
                for (int i = 0; i < 5; i++)
                {
                    LevelRecords[i] = Int32.Parse(fileContent[i]);
                }
                SelectedTheme = Byte.Parse(fileContent[5]);
                Wins = Int32.Parse(fileContent[6]);
                Loses = Int32.Parse(fileContent[7]);
            } 
        }

        public void currentSave()
        {
            string path = ".\\" + SelectedProfile + ".txt";
            if (!File.Exists(path))
            {
                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    for (int i = 0; i < 8; i++)
                        writer.WriteLine("0");
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    for (int i = 0; i < 5; i++)
                        writer.WriteLine(LevelRecords[i].ToString());
                    writer.WriteLine(SelectedTheme.ToString());
                    writer.WriteLine(Wins.ToString());
                    writer.Write(Loses.ToString());
                }
            }
        }

        public RelayCommand MoveCommand
        {
            get
            {
                return _moveCommand ??
                    (_moveCommand = new RelayCommand(obj =>
                    {
                        switch (obj.ToString())
                        {
                            case "Right":
                                game.ChangeDir(Direction.Right);
                                break;
                            case "Up":
                                game.ChangeDir(Direction.Up);
                                break;
                            case "Left":
                                game.ChangeDir(Direction.Left);
                                break;
                            case "Down":
                                game.ChangeDir(Direction.Down);
                                break;
                        }
                    }));
            } 
        }

        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand ??
                    (_startCommand = new RelayCommand(obj =>
                    {
                        Stop();

                        switch (SelectedLevel)
                        {
                            default: //Во всех непрописанных случаях будет запускаться первый уровень "Змейка"
                                game = new SnakeGame(gridRows, gridCols, 1);
                                gameSpeed = 200;
                                ScoreTarget = 40;
                                StartedLevel = 0;
                                break;
                            case "Обжорство":
                                game = new SnakeGame(gridRows, gridCols, 2);
                                gameSpeed = 150;
                                ScoreTarget = 50;
                                StartedLevel = 1;
                                break;
                            case "Бегущий по грани":
                                game = new SnakeGame(gridRows, gridCols, 3);
                                gameSpeed = 200;
                                ScoreTarget = 30;
                                StartedLevel = 2;
                                break;
                            case "Шоссе":
                                game = new SnakeGame(gridRows, gridCols, 4);
                                gameSpeed = 100;
                                ScoreTarget = 35;
                                StartedLevel = 3;
                                break;
                            case "Ветрянка":
                                game = new SnakeGame(gridRows, gridCols, 5);
                                gameSpeed = 200;
                                ScoreTarget = 30;
                                StartedLevel = 4;
                                break;
                        }

                        for (int i = 0, r = 0; i < gridRows; i++)
                        {
                            Grid[i] = new GridStates[gridCols];
                            for (int j = 0; j < gridCols; j++, r++)
                            {
                                Grid[i][j] = game.Grid[i, j];
                            }
                        }
                        Run();
                        game.PropertyChanged += MPropertyChanged;
                        //Score = game.Score;
                        OnPropertyChanged("Score");
                        OnPropertyChanged("GameOver");
                        OnPropertyChanged("GameStarted");
                        OnPropertyChanged("profilesVisibility");
                    }));
            }
        }

        /*private void PreviewKeyDown_EventHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                    System.Windows.MessageBox.Show("RightPressed");
                    break;
                case Key.Up:
                    game.ChangeDir(Direction.Up);
                    break;
                case Key.Left:
                    game.ChangeDir(Direction.Left);
                    break;
                case Key.Down:
                    game.ChangeDir(Direction.Down);
                    break;
            }
        }*/

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        private void MPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Score")
                OnPropertyChanged("Score");
            if (e.PropertyName == "GameOver")
            {
                OnPropertyChanged("GameOver");
                OnPropertyChanged("profilesVisibility");
                if (game.GameOver == true)
                {
                    OnPropertyChanged("IsWin");
                    if (Score > LevelRecords[StartedLevel])
                    {
                        LevelRecords[StartedLevel] = Score;
                        OnPropertyChanged("LevelRecords");
                    }
                    if (Score >= ScoreTarget)
                    {
                        Wins += 1;
                    }
                    else
                    {
                        Loses += 1;
                    }
                    OnPropertyChanged("WinCoefficient");
                    currentSave();
                }
            }
            if (e.PropertyName == "GameStarted")
            {
                OnPropertyChanged("profilesVisibility");
                OnPropertyChanged("GameStarted");
            }
        }



    }
}
