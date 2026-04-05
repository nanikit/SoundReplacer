using IPA.Utilities;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using UnityEngine;
using Zenject;

namespace SoundReplacer.Patches
{
    internal class CutSoundPatch : IInitializable, IDisposable, IAffinity
    {
        private const float IntrinsicOffset = 10f;
        private const float MinCutLoudnessDb = -50f;

        private readonly NoteCutSoundEffectManager _noteCutSoundEffectManager;
        private readonly SoundLoader _soundLoader;
        private readonly PluginConfig _config;
        private readonly SiraLog _logger;

        private readonly AudioClip[] _cutSounds = new AudioClip[1];
        private readonly AudioClip[] _originalLongCutSounds;
        private readonly AudioClip[] _originalShortCutSounds;

        private volatile float _responsiveCutVolume;

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

        [AffinityPatch(typeof(AdaptiveSfxVolume), nameof(AdaptiveSfxVolume.Start))]
        [AffinityPrefix]
        private void GiveMeUntouchedLufs(AdaptiveSfxVolume __instance)
        {
            FieldAccessor<AdaptiveSfxVolume, float>.Set(__instance, nameof(AdaptiveSfxVolume._minThreshold), float.MinValue);
        }

        [AffinityPatch(typeof(MomentaryLoudnessHistory), nameof(MomentaryLoudnessHistory.Add))]
        [AffinityPrefix]
        private void CaptureResponsiveCutVolume(float momentaryLoudness)
        {
            _responsiveCutVolume = ToCutVolume(momentaryLoudness);
        }

        [AffinityPatch(typeof(NoteCutSoundEffect), nameof(NoteCutSoundEffect.NoteWasCut))]
        [AffinityPostfix]
        private void AdjustCutSoundVolume(NoteCutSoundEffect __instance, NoteController noteController)
        {
            if (__instance._noteController == noteController)
            {
                __instance._audioSource.volume = GetCurrentCutVolume();
            }
        }

        [AffinityPatch(typeof(NoteCutSoundEffect), nameof(NoteCutSoundEffect.OnLateUpdate))]
        [AffinityPrefix]
        private void AdjustCutSoundVolumeOnUpdate(NoteCutSoundEffect __instance)
        {
            float cutVolume = GetCurrentCutVolume();
            __instance._badCutVolume = cutVolume;
            __instance._goodCutVolume = cutVolume;
        }

        [AffinityPatch(typeof(NoteCutSoundEffect), nameof(NoteCutSoundEffect.ComputeDSPTimes))]
        [AffinityPrefix]
        private void TryFixingPitch(NoteCutSoundEffect __instance)
        {
            __instance._audioSource.outputAudioMixerGroup = null;
            __instance._audioSource.volume = GetCurrentCutVolume();
            if (_config.PitchLock)
            {
                __instance._pitch = 1f;
            }
        }

        private float GetAdjustedLoudnessDb(float loudnessDb)
        {
            return Mathf.Max(
                MinCutLoudnessDb,
                loudnessDb * _config.SfxDecibelMultiplier + _config.SfxDecibelOffset + _config.MusicDecibelOffset - IntrinsicOffset);
        }

        private float ToCutVolume(float loudnessDb)
            => ReplacerAudioHelpers.DBToNormalizedVolume(GetAdjustedLoudnessDb(loudnessDb));

        private float GetCurrentCutVolume() => _responsiveCutVolume;
    }
}
