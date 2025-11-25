using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using UnityEngine;
using Zenject;

namespace SoundReplacer.Patches
{
    internal class CutSoundPatch : IInitializable, IDisposable, IAffinity
    {
        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly SoundLoader _soundLoader;
        private readonly PluginConfig _config;
        private readonly SiraLog _logger;

        private readonly AudioClip[] _cutSounds = new AudioClip[1];
        private readonly AudioClip[] _originalLongCutSounds;
        private readonly AudioClip[] _originalShortCutSounds;

        private float _momentaryVolume = 1f;

        private CutSoundPatch(NoteCutSoundEffectManager noteCutSoundEffectManager, SoundLoader soundLoader, PluginConfig config, SiraLog logger)
        {
            _noteCutSoundEffectManager = noteCutSoundEffectManager;
            _soundLoader = soundLoader;
            _config = config;
            _logger = logger;
            _originalShortCutSounds = noteCutSoundEffectManager._shortCutEffectsAudioClips;
            _originalLongCutSounds = noteCutSoundEffectManager._longCutEffectsAudioClips;
        }

        public void Initialize()
        {
            if (_config.CutSound == SoundLoader.NoSoundID)
            {
                _cutSounds[0] = SoundLoader.Empty;
                _noteCutSoundEffectManager._shortCutEffectsAudioClips = _cutSounds;
                _noteCutSoundEffectManager._longCutEffectsAudioClips = _cutSounds;
            }
            else if (_config.CutSound == SoundLoader.DefaultSoundID)
            {
                _noteCutSoundEffectManager._shortCutEffectsAudioClips = _originalShortCutSounds;
                _noteCutSoundEffectManager._longCutEffectsAudioClips = _originalLongCutSounds;
            }
            else
            {
                _cutSounds[0] = _soundLoader.Load(_cutSounds[0], SoundType.Cut);
                _noteCutSoundEffectManager._shortCutEffectsAudioClips = _cutSounds;
                _noteCutSoundEffectManager._longCutEffectsAudioClips = _cutSounds;
            }
        }

        public void Dispose()
        {
            _soundLoader.Unload(SoundType.Cut);
        }

        [AffinityPatch(typeof(AdaptiveSfxVolume), nameof(AdaptiveSfxVolume.ApplyLoudness))]
        [AffinityPrefix]
        private void UpdateAdaptiveSfxVolume(ref float songLoudness)
        {
            // songLoudness maximum: 0dBFS
            float intrinsicOffset = 10f;
            songLoudness += _config.SfxDecibelOffset - intrinsicOffset;
            _momentaryVolume = ReplacerAudioHelpers.DBToNormalizedVolume(songLoudness);
        }

        [AffinityPatch(typeof(NoteCutSoundEffect), nameof(NoteCutSoundEffect.NoteWasCut))]
        [AffinityPrefix]
        private void AdjustCutSoundVolume(NoteCutSoundEffect __instance, NoteController noteController)
        {
            if (__instance._noteController == noteController) {
                __instance._audioSource.volume = _momentaryVolume;
            }
        }

        [AffinityPatch(typeof(NoteCutSoundEffect), nameof(NoteCutSoundEffect.OnLateUpdate))]
        [AffinityPrefix]
        private void AdjustCutSoundVolumeOnUpdate(NoteCutSoundEffect __instance)
        {
            __instance._audioSource.volume = _momentaryVolume;
        }

        [AffinityPatch(typeof(NoteCutSoundEffect), nameof(NoteCutSoundEffect.ComputeDSPTimes))]
        [AffinityPrefix]
        private void TryFixingPitch(NoteCutSoundEffect __instance)
        {
            __instance._audioSource.outputAudioMixerGroup = null;
            __instance._audioSource.volume = _momentaryVolume;
            if (_config.PitchLock)
            {
                __instance._pitch = 1f;
            }
        }
    }
}
