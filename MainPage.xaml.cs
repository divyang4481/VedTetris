using Microsoft.Maui.Storage;

namespace VedTetris
{
    public partial class MainPage : ContentPage
    {
        private readonly Game _game;
        private readonly TetrisDrawable _drawable;
        private readonly NextPieceDrawable _nextPieceDrawable;
        private int _highScore = 0;
        private IDispatcherTimer _gameLoop;
        private const string HighScoreKey = "VedTetris_HighScore";

        public MainPage()
        {
            InitializeComponent();

            // Set up main game canvas
            _drawable = new TetrisDrawable();
            GameCanvas.Drawable = _drawable;
            
            // Set up next piece preview
            _nextPieceDrawable = new NextPieceDrawable();
            NextPieceView.Drawable = _nextPieceDrawable;
            
            // Create the game instance
            _game = new Game();
            _drawable.Game = _game;
            _nextPieceDrawable.Game = _game;
            
            // Load high score from preferences
            LoadHighScore();

            // Set up swipe gesture recognition
            var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
            swipeLeft.Swiped += (s, e) => MoveLeft_Clicked(s, e);
            
            var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
            swipeRight.Swiped += (s, e) => MoveRight_Clicked(s, e);
            
            var swipeUp = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
            swipeUp.Swiped += (s, e) => Rotate_Clicked(s, e);
            
            var swipeDown = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
            swipeDown.Swiped += (s, e) => Drop_Clicked(s, e);
            
            GameCanvas.GestureRecognizers.Add(swipeLeft);
            GameCanvas.GestureRecognizers.Add(swipeRight);
            GameCanvas.GestureRecognizers.Add(swipeUp);
            GameCanvas.GestureRecognizers.Add(swipeDown);
            
            // Game loop timer
            _gameLoop = Dispatcher.CreateTimer();
            _gameLoop.Tick += OnGameTick;
            
            // Start the game
            RestartGame();
        }
        
        private void LoadHighScore()
        {
            if (Preferences.Default.ContainsKey(HighScoreKey))
            {
                _highScore = Preferences.Default.Get(HighScoreKey, 0);
                HighScoreLabel.Text = _highScore.ToString();
            }
        }
        
        private void SaveHighScore(int score)
        {
            if (score > _highScore)
            {
                _highScore = score;
                Preferences.Default.Set(HighScoreKey, _highScore);
                HighScoreLabel.Text = _highScore.ToString();
            }
        }

        private void OnGameTick(object sender, EventArgs e)
        {
            if (!_game.IsGameOver)
            {
                _game.GameTick();
                
                // Update UI
                GameCanvas.Invalidate();
                NextPieceView.Invalidate();
                UpdateGameStats();
            }
            else
            {
                _gameLoop.Stop();
                
                // Save high score
                SaveHighScore(_game.Score);
                
                DisplayAlert("Game Over", $"Your score: {_game.Score}", "Play Again")
                    .ContinueWith((task) => 
                    {
                        if (task.IsCompleted)
                        {
                            RestartGame();
                        }
                    });
            }
        }
        
        private void RestartGame()
        {
            _game.Reset();
            UpdateGameStats();
            
            // Adjust game speed based on level
            _gameLoop.Interval = TimeSpan.FromMilliseconds(_game.GetGameSpeed());
            _gameLoop.Start();
            
            // Update UI state
            PauseButton.Text = "⏸️ Pause";
            
            // Make sure UI is updated
            GameCanvas.Invalidate();
            NextPieceView.Invalidate();
        }
        
        private void UpdateGameStats()
        {
            ScoreLabel.Text = _game.Score.ToString();
            LevelLabel.Text = _game.Level.ToString();
            LinesLabel.Text = _game.LinesCleared.ToString();
            
            // Update timer speed based on current level
            _gameLoop.Interval = TimeSpan.FromMilliseconds(_game.GetGameSpeed());
        }

        private void MoveLeft_Clicked(object sender, EventArgs e)
        {
            if (!_game.IsGameOver && !_game.IsPaused) 
            {
                _game.MoveLeft();
                GameCanvas.Invalidate();
            }
        }

        private void MoveRight_Clicked(object sender, EventArgs e)
        {
            if (!_game.IsGameOver && !_game.IsPaused)
            {
                _game.MoveRight();
                GameCanvas.Invalidate();
            }
        }

        private void Rotate_Clicked(object sender, EventArgs e)
        {
            if (!_game.IsGameOver && !_game.IsPaused)
            {
                _game.Rotate();
                GameCanvas.Invalidate();
            }
        }

        private void Drop_Clicked(object sender, EventArgs e)
        {
            if (!_game.IsGameOver && !_game.IsPaused)
            {
                _game.Drop();
                GameCanvas.Invalidate();
            }
        }
        
        private void PauseButton_Clicked(object sender, EventArgs e)
        {
            _game.TogglePause();
            
            if (_game.IsPaused)
            {
                PauseButton.Text = "▶️ Resume";
                _gameLoop.Stop();
            }
            else
            {
                PauseButton.Text = "⏸️ Pause";
                _gameLoop.Start();
            }
            
            GameCanvas.Invalidate();
        }
    }
}
