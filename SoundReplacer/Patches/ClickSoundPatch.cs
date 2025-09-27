using System;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace SoundReplacer.Patches
{
    internal class ClickSoundPatch : IInitializable, IAffinity, IDisposable
    {
        private readonly SoundLoader _soundLoader;
        private readonly PluginConfig _config;
        private readonly BasicUIAudioManager _basicUIAudioManager;

        private readonly AudioClip[] _clickSounds = new AudioClip[1];
        private AudioClip[]? _originalClickSounds;

        public ClickSoundPatch(SoundLoader soundLoader, PluginConfig config, BasicUIAudioManager basicUIAudioManager)
        {
            _soundLoader = soundLoader;
            _config = config;
            _basicUIAudioManager = basicUIAudioManager;
        }

        public void Initialize()
        {
            _config.OnChanged += AdjustClickSoundVolume;
            AdjustClickSoundVolume();
        }

        public void Dispose()
        {
            _soundLoader.Unload(SoundType.Click);
            _config.OnChanged -= AdjustClickSoundVolume;
        }

        private void AdjustClickSoundVolume()
        {
            float oldVolume = _basicUIAudioManager._audioSource.volume;
            float newVolume = oldVolume - _config.MusicDecibelOffset;
            _basicUIAudioManager._audioSource.volume = Math.Clamp(newVolume, 0f, 1f);
        }

        [AffinityPatch(typeof(BasicUIAudioManager), nameof(BasicUIAudioManager.Start))]
        [AffinityPrefix]
        private void ReplaceClickSounds(BasicUIAudioManager __instance)
        {
            _originalClickSounds ??= __instance._clickSounds;

            if (_config.ClickSound == SoundLoader.NoSoundID)
            {
                _clickSounds[0] = SoundLoader.Empty;
                __instance._clickSounds = _clickSounds;
            }
            else if (_config.ClickSound == SoundLoader.DefaultSoundID)
            {
                __instance._clickSounds = _originalClickSounds;
            }
            else
            {
                _clickSounds[0] = _soundLoader.Load(_clickSounds[0], SoundType.Click);
                __instance._clickSounds = _clickSounds;
            }
        }
    }
}
