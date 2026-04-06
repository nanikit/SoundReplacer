using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(IPA.Config.Stores.GeneratedStore.AssemblyVisibilityTarget)]

namespace SoundReplacer
{
    internal class PluginConfig
    {
        public event Action OnChanged = delegate { };

        public string CutSound { get; set; } = SoundLoader.DefaultSoundID;
        public string BadCutSound { get; set; } = SoundLoader.DefaultSoundID;
        public string BombCutSound { get; set; } = SoundLoader.DefaultSoundID;
        public string MenuMusic { get; set; } = SoundLoader.DefaultSoundID;
        public string ClickSound { get; set; } = SoundLoader.DefaultSoundID;
        public string LevelClearedSound { get; set; } = SoundLoader.DefaultSoundID;
        public string LevelFailedSound { get; set; } = SoundLoader.DefaultSoundID;
        public bool PitchLock { get; set; } = false;
        public CutSoundVolumeMethod CutSoundVolumeMethod { get; set; } = CutSoundVolumeMethod.MomentaryLufs;
        public float MusicDecibelOffset { get; set; } = 0f;
        public float SfxDecibelOffset { get; set; } = 0f;
        public float SfxDecibelMultiplier { get; set; } = 1f;


        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            OnChanged();
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            OnChanged();
        }
    }
}
