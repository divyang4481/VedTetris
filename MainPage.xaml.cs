using Microsoft.Maui.Storage;

namespace VedTetris
{
    public partial class MainPage : ContentPage
    {
        private readonly Game _game;
        private readonly TetrisDrawable _drawable;
        private readonly NextPieceDrawable _nextPieceDrawable;
        private readonly HoldPieceDrawable _holdPieceDrawable;
        private readonly SoundManager? _soundManager;
        private readonly GameStatistics _gameStatistics;
        
        private int _highScore = 0;
        private int _tetrisCount = 0;
        private DateTime _gameStartTime;
        private IDispatcherTimer _gameLoop = null!;
        private IDispatcherTimer _gameTimeTimer = null!;
        
        private const string HighScoreKey = "VedTetris_HighScore";

        public MainPage()
        {
            InitializeComponent();

            // Get services from dependency injection
            _soundManager = Handler?.MauiContext?.Services.GetService<SoundManager>();
            _gameStatistics = Handler?.MauiContext?.Services.GetService<GameStatistics>() ?? GameStatistics.LoadFromPreferences();

            // Set up drawables
            _drawable = new TetrisDrawable();
            _nextPieceDrawable = new NextPieceDrawable();
            _holdPieceDrawable = new HoldPieceDrawable();
            
            GameCanvas.Drawable = _drawable;
            NextPieceView.Drawable = _nextPieceDrawable;
            HoldPieceView.Drawable = _holdPieceDrawable;
            
            // Create the game instance
            _game = new Game();
            _drawable.Game = _game;
            _nextPieceDrawable.Game = _game;
            _holdPieceDrawable.Game = _game;
            
            // Subscribe to game events for sound effects
            SubscribeToGameEvents();
            
            // Load high score from preferences
            LoadHighScore();

            // Set up gesture recognition
            SetupGestureRecognizers();
            
            // Set up timers
            SetupGameLoop();
            SetupGameTimeTimer();
            
            // Start the game
            RestartGame();
        }
        
        private void SubscribeToGameEvents()
        {
            if (_soundManager != null)
            {
                _game.OnMove += async () => await _soundManager.PlayMoveSound();
                _game.OnRotate += async () => await _soundManager.PlayRotateSound();
                _game.OnDrop += async () => await _soundManager.PlayDropSound();
                _game.OnLinesCleared += async (lines) => await _soundManager.PlayLineClearSound();
                _game.OnTetris += async () => {
                    await _soundManager.PlayTetrisSound();
                    _tetrisCount++;
                    _gameStatistics?.RecordTetris();
                };
                _game.OnLevelUp += async () => await _soundManager.PlayLevelUpSound();
                _game.OnGameOver += async () => await _soundManager.PlayGameOverSound();
            }
        }
        
        private void SetupGestureRecognizers()
        {
            var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
            swipeLeft.Swiped += MoveLeft_Clicked;
            
            var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
            swipeRight.Swiped += MoveRight_Clicked;
            
            var swipeUp = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
            swipeUp.Swiped += Rotate_Clicked;
            
            var swipeDown = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
            swipeDown.Swiped += Drop_Clicked;
            
            GameCanvas.GestureRecognizers.Add(swipeLeft);
            GameCanvas.GestureRecognizers.Add(swipeRight);
            GameCanvas.GestureRecognizers.Add(swipeUp);
            GameCanvas.GestureRecognizers.Add(swipeDown);
        }
        
        private void SetupGameLoop()
        {
            _gameLoop = Dispatcher.CreateTimer();
            _gameLoop.Tick += OnGameTick;
        }
        
        private void SetupGameTimeTimer()
        {
            _gameTimeTimer = Dispatcher.CreateTimer();
            _gameTimeTimer.Interval = TimeSpan.FromSeconds(1);
            _gameTimeTimer.Tick += (sender, e) => UpdateGameTime();
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

        private void OnGameTick(object? sender, EventArgs e)
        {
            if (!_game.IsGameOver)
            {
                _game.GameTick();
                
                // Update UI
                GameCanvas.Invalidate();
                NextPieceView.Invalidate();
                HoldPieceView.Invalidate();
                UpdateGameStats();
            }
            else
            {
                _gameLoop.Stop();
                _gameTimeTimer.Stop();
                
                // Save statistics
                SaveHighScore(_game.Score);
                _gameStatistics?.RecordGamePlayed();
                _gameStatistics?.RecordLinesCleared(_game.LinesCleared);
                _gameStatistics?.UpdateHighestLevel(_game.Level);
                _gameStatistics?.AddPlayTime(DateTime.Now - _gameStartTime);
                _gameStatistics?.SaveToPreferences();
                
                DisplayAlert("Game Over", $"Your score: {_game.Score}\nLevel reached: {_game.Level}\nLines cleared: {_game.LinesCleared}", "Play Again")
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
            _tetrisCount = 0;
            _gameStartTime = DateTime.Now;
            
            UpdateGameStats();
            
            // Adjust game speed based on level
            _gameLoop.Interval = TimeSpan.FromMilliseconds(_game.GetGameSpeed());
            _gameLoop.Start();
            _gameTimeTimer.Start();
            
            // Update UI state
            PauseButton.Text = "⏸️ Pause";
            
            // Make sure UI is updated
            GameCanvas.Invalidate();
            NextPieceView.Invalidate();
            HoldPieceView.Invalidate();
        }
        
        private void UpdateGameStats()
        {
            ScoreLabel.Text = _game.Score.ToString();
            LevelLabel.Text = _game.Level.ToString();
            LinesLabel.Text = _game.LinesCleared.ToString();
            TetrisCountLabel.Text = $"Tetris: {_tetrisCount}";
            
            // Update timer speed based on current level
            _gameLoop.Interval = TimeSpan.FromMilliseconds(_game.GetGameSpeed());
        }
        
        private void UpdateGameTime()
        {
            if (!_game.IsGameOver && !_game.IsPaused)
            {
                var elapsed = DateTime.Now - _gameStartTime;
                GameTimeLabel.Text = $"Time: {elapsed:mm\\:ss}";
            }
        }

        private void MoveLeft_Clicked(object? sender, EventArgs e)
        {
            if (!_game.IsGameOver && !_game.IsPaused) 
            {
                _game.MoveLeft();
                GameCanvas.Invalidate();
                
                // Add haptic feedback for mobile
#if ANDROID || IOS
                try
                {
                    HapticFeedback.Perform(HapticFeedbackType.Click);
                }
                catch { /* Ignore haptic errors */ }
#endif
            }
        }

        private void MoveRight_Clicked(object? sender, EventArgs e)
        {
            if (!_game.IsGameOver && !_game.IsPaused)
            {
                _game.MoveRight();
                GameCanvas.Invalidate();
                
#if ANDROID || IOS
                try
                {
                    HapticFeedback.Perform(HapticFeedbackType.Click);
                }
                catch { /* Ignore haptic errors */ }
#endif
            }
        }

        private void Rotate_Clicked(object? sender, EventArgs e)
        {
            if (!_game.IsGameOver && !_game.IsPaused)
            {
                _game.Rotate();
                GameCanvas.Invalidate();
                
#if ANDROID || IOS
                try
                {
                    HapticFeedback.Perform(HapticFeedbackType.Click);
                }
                catch { /* Ignore haptic errors */ }
#endif
            }
        }

        private void Drop_Clicked(object? sender, EventArgs e)
        {
            if (!_game.IsGameOver && !_game.IsPaused)
            {
                _game.Drop();
                GameCanvas.Invalidate();
                
#if ANDROID || IOS
                try
                {
                    HapticFeedback.Perform(HapticFeedbackType.LongPress);
                }
                catch { /* Ignore haptic errors */ }
#endif
            }
        }
        
        private void Hold_Clicked(object? sender, EventArgs e)
        {
            if (!_game.IsGameOver && !_game.IsPaused)
            {
                if (_game.HoldCurrentPiece())
                {
                    GameCanvas.Invalidate();
                    HoldPieceView.Invalidate();
                    
#if ANDROID || IOS
                    try
                    {
                        HapticFeedback.Perform(HapticFeedbackType.Click);
                    }
                    catch { /* Ignore haptic errors */ }
#endif
                }
            }
        }
        
        private void PauseButton_Clicked(object? sender, EventArgs e)
        {
            _game.TogglePause();
            
            if (_game.IsPaused)
            {
                PauseButton.Text = "▶️ Resume";
                _gameLoop.Stop();
                _gameTimeTimer.Stop();
            }
            else
            {
                PauseButton.Text = "⏸️ Pause";
                _gameLoop.Start();
                _gameTimeTimer.Start();
            }
            
            GameCanvas.Invalidate();
        }
        
        private async void SettingsButton_Clicked(object? sender, EventArgs e)
        {
            var settingsPage = new SettingsPage(_soundManager, _gameStatistics);
            await Navigation.PushAsync(settingsPage);
        }
        
        protected override bool OnBackButtonPressed()
        {
            if (!_game.IsPaused)
            {
                PauseButton_Clicked(null, EventArgs.Empty);
            }
            return true; // Prevent default back button behavior
        }
    }
}
