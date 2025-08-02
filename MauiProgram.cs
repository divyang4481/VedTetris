using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;

namespace VedTetris
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register audio service
            builder.Services.AddSingleton(AudioManager.Current);
            builder.Services.AddSingleton<SoundManager>();
            builder.Services.AddSingleton<GameStatistics>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

    /// <summary>
    /// Manages all game sound effects and audio functionality
    /// </summary>
    public class SoundManager
    {
        private readonly IAudioManager _audioManager;
        private bool _soundEnabled = true;
        
        public SoundManager(IAudioManager audioManager)
        {
            _audioManager = audioManager;
            LoadSettings();
        }
        
        private void LoadSettings()
        {
            _soundEnabled = Preferences.Get("SoundEnabled", true);
        }
        
        public void SetSoundEnabled(bool enabled)
        {
            _soundEnabled = enabled;
            Preferences.Set("SoundEnabled", enabled);
        }
        
        public bool IsSoundEnabled => _soundEnabled;
        
        public async Task PlayMoveSound()
        {
            if (!_soundEnabled) return;
            await PlaySound("move.wav");
        }
        
        public async Task PlayRotateSound()
        {
            if (!_soundEnabled) return;
            await PlaySound("rotate.wav");
        }
        
        public async Task PlayDropSound()
        {
            if (!_soundEnabled) return;
            await PlaySound("drop.wav");
        }
        
        public async Task PlayLineClearSound()
        {
            if (!_soundEnabled) return;
            await PlaySound("line_clear.wav");
        }
        
        public async Task PlayTetrisSound()
        {
            if (!_soundEnabled) return;
            await PlaySound("tetris.wav");
        }
        
        public async Task PlayGameOverSound()
        {
            if (!_soundEnabled) return;
            await PlaySound("game_over.wav");
        }
        
        public async Task PlayLevelUpSound()
        {
            if (!_soundEnabled) return;
            await PlaySound("level_up.wav");
        }
        
        private async Task PlaySound(string soundFile)
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(soundFile);
                var player = _audioManager.CreatePlayer(stream);
                player.Play();
            }
            catch (Exception ex)
            {
                // Log error but don't crash the game
                System.Diagnostics.Debug.WriteLine($"Error playing sound {soundFile}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Tracks detailed game statistics and achievements
    /// </summary>
    public class GameStatistics
    {
        public int TotalGamesPlayed { get; set; }
        public int TotalLinesCleared { get; set; }
        public int HighestLevel { get; set; } = 1;
        public TimeSpan TotalPlayTime { get; set; }
        public int TetrisesMade { get; set; } // 4-line clears
        public int PerfectClears { get; set; } // Board cleared completely
        
        public void SaveToPreferences()
        {
            Preferences.Set("TotalGames", TotalGamesPlayed);
            Preferences.Set("TotalLines", TotalLinesCleared);
            Preferences.Set("HighestLevel", HighestLevel);
            Preferences.Set("TotalPlayTime", TotalPlayTime.TotalSeconds);
            Preferences.Set("TetrisesMade", TetrisesMade);
            Preferences.Set("PerfectClears", PerfectClears);
        }
        
        public static GameStatistics LoadFromPreferences()
        {
            return new GameStatistics
            {
                TotalGamesPlayed = Preferences.Get("TotalGames", 0),
                TotalLinesCleared = Preferences.Get("TotalLines", 0),
                HighestLevel = Preferences.Get("HighestLevel", 1),
                TotalPlayTime = TimeSpan.FromSeconds(Preferences.Get("TotalPlayTime", 0.0)),
                TetrisesMade = Preferences.Get("TetrisesMade", 0),
                PerfectClears = Preferences.Get("PerfectClears", 0)
            };
        }
        
        public void RecordGamePlayed() => TotalGamesPlayed++;
        public void RecordLinesCleared(int lines) => TotalLinesCleared += lines;
        public void RecordTetris() => TetrisesMade++;
        public void RecordPerfectClear() => PerfectClears++;
        public void UpdateHighestLevel(int level) => HighestLevel = Math.Max(HighestLevel, level);
        public void AddPlayTime(TimeSpan time) => TotalPlayTime = TotalPlayTime.Add(time);
    }
}
