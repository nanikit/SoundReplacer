using System;

namespace SoundReplacer.Logics;

public static class ReplacerAudioHelpers
{
    public static float NormalizedVolumeToDB(float normalizedVolume)
    {
        return (float)Math.Max(-100f, Math.Log(normalizedVolume, 1.1f));
    }

    public static float DBToNormalizedVolume(float db)
    {
        return (float)Math.Pow(1.1f, db);
    }
}
