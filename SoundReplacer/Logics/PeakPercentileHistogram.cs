using System;
using UnityEngine;

namespace SoundReplacer.Logics;

internal sealed class PeakPercentileHistogram
{
    private readonly int[] _histogram;
    private readonly int _binCount;
    private readonly float _percentile;

    public PeakPercentileHistogram(int binCount, float percentile)
    {
        _binCount = binCount;
        _percentile = percentile;
        _histogram = new int[binCount];
    }

    public float EstimateAmplitude(float[] data)
    {
        if (data.Length == 0)
        {
            return 0f;
        }

        Array.Clear(_histogram, 0, _histogram.Length);
        for (int i = 0; i < data.Length; i++)
        {
            float amplitude = Mathf.Clamp01(Mathf.Abs(data[i]));
            int binIndex = Mathf.Min((int)(amplitude * _binCount), _binCount - 1);
            _histogram[binIndex]++;
        }

        int targetSampleCount = Mathf.Clamp(Mathf.CeilToInt(data.Length * _percentile), 1, data.Length);
        int cumulativeCount = 0;
        int percentileBinIndex = 0;
        for (int i = 0; i < _histogram.Length; i++)
        {
            cumulativeCount += _histogram[i];
            if (cumulativeCount >= targetSampleCount)
            {
                percentileBinIndex = i;
                break;
            }
        }

        return (percentileBinIndex + 0.5f) / _binCount;
    }
}
