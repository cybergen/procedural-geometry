using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;

public class SpiralGenerator : Generatable
{
    public int TexSize = 256;
    public float CircleStep = 0.01f;
    public float MultiplierStep = 0.001f;
    public float GrowthFactor = 0.001f;
    public float PowerFactor = 1.3f;
    public int CalculationChunk = 5;

    public override void Generate()
    {
        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGB24, false);
        int x;
        int y;
        for (y = 0; y < TexSize; y++)
        {
            for (x = 0; x < TexSize; x++) tex.SetPixel(x, y, Color.red);
        }

        x = y = 1;

        StartCoroutine(DrawTexture(tex, x, y));
    }

    private IEnumerator DrawTexture(Texture2D tex, int x, int y)
    {
        float angle = 0;
        bool keepGoing = true;
        float multiplier = 0f;
        int count = 0;

        do
        {
            count++;
            angle += CircleStep;
            multiplier += MultiplierStep + Mathf.Pow((float)count, PowerFactor) * GrowthFactor;

            var radians = Mathf.Deg2Rad * angle;
            x = (int)(Mathf.Cos(radians) * multiplier);
            y = (int)(Mathf.Sin(radians) * multiplier);

            keepGoing = Mathf.Abs(x) < (TexSize / 2) && Mathf.Abs(y) < (TexSize / 2);

            x += TexSize / 2;
            y += TexSize / 2;

            tex.SetPixel(x, y, Color.black);

            if (IsMultiple(count, CalculationChunk)) yield return null;

            Debug.Log("Spiral calculation on step: " + count + ", with x: " + x + ", and y: " + y);
        } while (keepGoing);

        tex.Apply();
        var bytes = tex.EncodeToPNG();
        DestroyImmediate(tex);
        Debug.Log("Resolved spiral creation. Saving file.");
        using (var fs = new FileStream(Application.dataPath + "/Textures/GenerateTexture.png", FileMode.OpenOrCreate))
        {
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
        }
    }

    private Dictionary<int, int> _fibMap = new Dictionary<int, int>();

    private int Fibonacci(int count)
    {
        if (_fibMap.ContainsKey(count)) return _fibMap[count];
        if (count == 1 || count == 0) return 1;
        var value = Fibonacci(count - 1) + Fibonacci(count - 2);
        _fibMap.Add(count, value);
        return value;
    }

    private bool IsMultiple(int value, int divisor)
    {
        return (float)value % divisor == 0f; 
    }
}
