namespace VedTetris
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SoundManager? _soundManager;
        private readonly GameStatistics? _gameStatistics;

        public SettingsPage(SoundManager? soundManager, GameStatistics? gameStatistics)
        {
            InitializeComponent();
            _soundManager = soundManager;
            _gameStatistics = gameStatistics;
            
            LoadSettings();
            LoadStatistics();
        }
        
        private void LoadSettings()
        {
            SoundToggle.IsToggled = _soundManager?.IsSoundEnabled ?? true;
            HapticToggle.IsToggled = Preferences.Get("HapticEnabled", true);
        }
        
        private void LoadStatistics()
        {
            if (_gameStatistics != null)
            {
                TotalGamesLabel.Text = _gameStatistics.TotalGamesPlayed.ToString();
                TotalLinesLabel.Text = _gameStatistics.TotalLinesCleared.ToString();
                HighestLevelLabel.Text = _gameStatistics.HighestLevel.ToString();
                TotalTetrisLabel.Text = _gameStatistics.TetrisesMade.ToString();
                PlayTimeLabel.Text = _gameStatistics.TotalPlayTime.ToString(@"hh\:mm\:ss");
            }
        }
        
        private void OnSoundToggled(object sender, ToggledEventArgs e)
        {
            _soundManager?.SetSoundEnabled(e.Value);
        }
        
        private void OnHapticToggled(object sender, ToggledEventArgs e)
        {
            Preferences.Set("HapticEnabled", e.Value);
        }
        
        private async void OnResetStatsClicked(object sender, EventArgs e)
        {
            bool result = await DisplayAlert("Reset Statistics", 
                "Are you sure you want to reset all game statistics? This cannot be undone.", 
                "Yes", "No");
            
            if (result && _gameStatistics != null)
            {
                // Reset all statistics
                _gameStatistics.TotalGamesPlayed = 0;
                _gameStatistics.TotalLinesCleared = 0;
                _gameStatistics.HighestLevel = 1;
                _gameStatistics.TetrisesMade = 0;
                _gameStatistics.PerfectClears = 0;
                _gameStatistics.TotalPlayTime = TimeSpan.Zero;
                
                // Save and reload display
                _gameStatistics.SaveToPreferences();
                LoadStatistics();
                
                await DisplayAlert("Reset Complete", "All statistics have been reset.", "OK");
            }
        }
    }
}