using System;
using System.IO;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage;
using IPA.Utilities;
using Zenject;

namespace SoundReplacer.UI
{
    [ViewDefinition("SoundReplacer.SettingsView.bsml")]
    [HotReload(RelativePathToLayout = "SettingsView.bsml")]
    internal class SettingsViewController : BSMLAutomaticViewController
    {
        private const string MomentaryLufsOption = "Momentary LUFS";
        private const string PeakOption = "Peak";

        private SongPreviewPlayer _songPreviewPlayer = null!;
        private PluginConfig _config = null!;
        private BasicUIAudioManager _basicUIAudioManager = null!;

        [Inject]
        private void Construct(SongPreviewPlayer songPreviewPlayer, PluginConfig config)
        {
            _songPreviewPlayer = songPreviewPlayer;
            _config = config;
        }

        private void Awake()
        {
            _basicUIAudioManager = BeatSaberUI.BasicUIAudioManager;
        }

        public void RefreshSoundList()
        {
            try
            {
                var directoryInfo = new DirectoryInfo(Path.Combine(UnityGame.UserDataPath, nameof(SoundReplacer)));
                directoryInfo.Create();
                SoundList = SoundLoader.DefaultSounds
                    .Concat(directoryInfo
                        .EnumerateFiles("*", SearchOption.AllDirectories)
                        .Where(f => f.Extension is ".ogg" or ".mp3" or ".wav")
                        .Select(f => f.Name))
                    .ToArray();

                NotifyPropertyChanged(nameof(SoundList));
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Could not load sounds. {ex}");
            }
        }

        [UIValue("sound-list")]
        protected string[] SoundList { get; private set; } = SoundLoader.DefaultSounds;

        [UIValue("cut-sound-volume-methods")]
        protected string[] CutSoundVolumeMethods { get; } = [MomentaryLufsOption, PeakOption];

        [UIValue("good-hitsound")]
        protected string SettingCurrentGoodHitSound
        {
            get => _config.CutSound;
            set => _config.CutSound = value;
        }

        [UIValue("bad-hitsound")]
        protected string SettingCurrentBadHitSound
        {
            get => _config.BadCutSound;
            set => _config.BadCutSound = value;
        }

        [UIValue("bomb-hitsound")]
        protected string SettingCurrentBombHitSound
        {
            get => _config.BombCutSound;
            set => _config.BombCutSound = value;
        }

        [UIValue("menu-music")]
        protected string SettingCurrentMenuMusic
        {
            get => _config.MenuMusic;
            set {
                _config.MenuMusic = value;
                _songPreviewPlayer.Start();
                _songPreviewPlayer.CrossfadeToDefault();
            }
        }

        [UIValue("click-sound")]
        protected string SettingCurrentClickSound
        {
            get => _config.ClickSound;
            set {
                _config.ClickSound = value;
                _basicUIAudioManager.Start();
            }
        }

        [UIValue("success-sound")]
        protected string SettingCurrentSuccessSound
        {
            get => _config.LevelClearedSound;
            set => _config.LevelClearedSound = value;
        }

        [UIValue("fail-sound")]
        protected string SettingCurrentFailSound
        {
            get => _config.LevelFailedSound;
            set => _config.LevelFailedSound = value;
        }

        [UIValue("lock-pitch")]
        protected bool IsPitchLocked
        {
            get => _config.PitchLock;
            set => _config.PitchLock = value;
        }

        [UIValue("cut-sound-volume-method")]
        protected string SelectedCutSoundVolumeMethod
        {
            get => _config.CutSoundVolumeMethod switch
            {
                CutSoundVolumeMethod.MomentaryLufs => MomentaryLufsOption,
                CutSoundVolumeMethod.Peak => PeakOption,
                _ => throw new ArgumentOutOfRangeException()
            };
            set => _config.CutSoundVolumeMethod = value switch
            {
                MomentaryLufsOption => CutSoundVolumeMethod.MomentaryLufs,
                PeakOption => CutSoundVolumeMethod.Peak,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        [UIValue("music-decibel-offset")]
        protected float MusicDecibelOffset
        {
            get => _config.MusicDecibelOffset;
            set => _config.MusicDecibelOffset = value;
        }

        [UIValue("sfx-decibel-multiplier")]
        protected float SfxDecibelMultiplier
        {
            get => _config.SfxDecibelMultiplier;
            set => _config.SfxDecibelMultiplier = value;
        }

        [UIValue("sfx-decibel-offset")]
        protected float SfxDecibelOffset
        {
            get => _config.SfxDecibelOffset;
            set => _config.SfxDecibelOffset = value;
        }
    }
}
