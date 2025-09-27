using System;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SoundReplacer.Patches
{
    internal class BadCutSoundAndMusicPatch : IAffinity, IDisposable
    {
        private readonly SoundLoader _soundLoader;
        private readonly PluginConfig _config;
        private readonly SiraLog _logger;

        private readonly AudioClip[] _badCutSounds = new AudioClip[1];
        private AudioClip[]? _originalBadCutSounds;

        private BadCutSoundAndMusicPatch(SoundLoader soundLoader, PluginConfig config, SiraLog logger)
        {
            _soundLoader = soundLoader;
            _config = config;
            _logger = logger;
        }

        public void Dispose()
        {
            _soundLoader.Unload(SoundType.BadCut);
        }

        [AffinityPatch(typeof(AudioManagerSO), nameof(AudioManagerSO.musicVolume), AffinityMethodType.Setter)]
        [AffinityPrefix]
        private void SetMusicVolume(ref float value)
        {
            // value is -5dBFS on custom level, and final value is -7dBFS
            value += _config.MusicDecibelOffset;
            // _logger.Trace($"musicVolume: {value - 2f}");
        }

        [AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
        [AffinityPrefix]
        private void ReplaceBadCutSounds(EffectPoolsManualInstaller __instance)
        {
            var original = __instance._noteCutSoundEffectPrefab;
            var noteCutSoundEffect = Object.Instantiate(original);

            _originalBadCutSounds ??= original._badCutSoundEffectAudioClips;

            if (_config.BadCutSound == SoundLoader.NoSoundID)
            {
                _badCutSounds[0] = SoundLoader.Empty;
                noteCutSoundEffect._badCutSoundEffectAudioClips = _badCutSounds;
            }
            else if (_config.BadCutSound == SoundLoader.DefaultSoundID)
            {
                noteCutSoundEffect._badCutSoundEffectAudioClips = _originalBadCutSounds;
            }
            else
            {
                _badCutSounds[0] = _soundLoader.Load(_badCutSounds[0], SoundType.BadCut);
                noteCutSoundEffect._badCutSoundEffectAudioClips = _badCutSounds;
            }

            __instance._noteCutSoundEffectPrefab = noteCutSoundEffect;
        }
    }
}
