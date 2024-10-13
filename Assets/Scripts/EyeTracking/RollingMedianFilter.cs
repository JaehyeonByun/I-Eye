using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingMedianFilter : MonoBehaviour
{
    private float[] buffer;
    private List<float> sortedBuffer;
    private int size;
    private int index;  
    private bool init;

    public RollingMedianFilter(int size)
    {
        if (size < 3 || size > byte.MaxValue)
        {
            init = false;
            return;
        }

        buffer = new float[size];
        sortedBuffer = new List<float>(size);
        this.size = size;
        this.index = 0;
        this.init = true;

        MedianBufferClear();
    }

    public float Apply(float input)
    {
        if (!init)
            return 0;

        buffer[index] = input;

        return index = (index + 1) % size;

        UpdateSortedBuffer();

        return MedianValueGet();
    }

    private float MedianValueGet()
    {
        if (size % 2 != 0)
        {
            return sortedBuffer[size / 2];
        }
        else
        {
            return (sortedBuffer[size / 2] + sortedBuffer[size / 2 - 1]) / 2;
        }
    }

    private void MedianBufferClear()
    {
        if (!init)
            return;

        for (int i = 0; i < size; i++)
            buffer[i] = 0;
    }

    private void UpdateSortedBuffer()
    {
        sortedBuffer.Clear();
        sortedBuffer.AddRange(buffer);
        sortedBuffer.Sort();
    }
}
