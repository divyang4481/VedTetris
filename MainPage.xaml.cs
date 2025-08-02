namespace VedTetris
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly Game _game;
        private readonly TetrisDrawable _drawable;

        public MainPage()
        {
            InitializeComponent();
            _drawable = new TetrisDrawable();
            GameCanvas.Drawable = _drawable;
            _game = new Game();

            // Set the drawable's game instance
            _drawable.Game = _game;

            // Game loop timer
            IDispatcherTimer gameLoop = Dispatcher.CreateTimer();
            gameLoop.Interval = TimeSpan.FromMilliseconds(500);
            gameLoop.Tick += (s, e) =>
            {
                if (!_game.IsGameOver)
                {
                    _game.GameTick();
                    GameCanvas.Invalidate(); // Redraw the canvas
                }
                else
                {
                    gameLoop.Stop();
                    DisplayAlert("Game Over", $"Your score: {_game.Score}", "Play Again").ContinueWith((task) =>
                    {
                        _game.Reset();
                        gameLoop.Start();
                    });
                }
            };
            gameLoop.Start();
        }


        private void MoveLeft_Clicked(object sender, EventArgs e)
        {
            if (!_game.IsGameOver) _game.MoveLeft();
            GameCanvas.Invalidate();
        }

        private void MoveRight_Clicked(object sender, EventArgs e)
        {
            if (!_game.IsGameOver) _game.MoveRight();
            GameCanvas.Invalidate();
        }

        private void Rotate_Clicked(object sender, EventArgs e)
        {
            if (!_game.IsGameOver) _game.Rotate();
            GameCanvas.Invalidate();
        }

        private void Drop_Clicked(object sender, EventArgs e)
        {
            if (!_game.IsGameOver) _game.Drop();
            GameCanvas.Invalidate();
        }
    }

}
