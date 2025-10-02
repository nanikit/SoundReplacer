using System;
using SiraUtil.Affinity;
using UnityEngine;

namespace SoundReplacer.Patches
{
    internal class BombCutSoundPatch : IAffinity, IDisposable
    {
        private readonly SoundLoader _soundLoader;
        private readonly PluginConfig _config;

        private readonly AudioClip[] _bombCutSounds = new AudioClip[1];
        private AudioClip[]? _originalBombCutSounds;
        private float _originalVolume = -1;

        private BombCutSoundPatch(SoundLoader soundLoader, PluginConfig config)
        {
            _soundLoader = soundLoader;
            _config = config;
        }

        public void Dispose()
        {
            _soundLoader.Unload(SoundType.BombCut);
        }

        [AffinityPatch(typeof(BombCutSoundEffectManager), nameof(BombCutSoundEffectManager.Start))]
        [AffinityPrefix]
        private void ReplaceBombCutSounds(BombCutSoundEffectManager __instance)
        {
            _originalBombCutSounds ??= __instance._bombExplosionAudioClips;
            if (_originalVolume == -1)
            {
                _originalVolume = __instance._volume;
            }

            switch (_config.BombCutSound)
            {
            case SoundLoader.NoSoundID:
                _bombCutSounds[0] = SoundLoader.Empty;
                __instance._volume = _originalVolume;
                break;
            case SoundLoader.DefaultSoundID:
                __instance._bombExplosionAudioClips = _originalBombCutSounds;
                AdjustVolume(__instance);
                break;
            default:
                _bombCutSounds[0] = _soundLoader.Load(_bombCutSounds[0], SoundType.BombCut);
                __instance._bombExplosionAudioClips = _bombCutSounds;
                AdjustVolume(__instance);
                break;
            }
        }

        private void AdjustVolume(BombCutSoundEffectManager __instance)
        {
            __instance._volume = ReplacerAudioHelpers.DBToNormalizedVolume(ReplacerAudioHelpers.NormalizedVolumeToDB(__instance._volume) + _config.SfxDecibelOffset);
        }
    }
}
