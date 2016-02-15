using System.IO;
using UnityEngine;

public class SpiralGenerator : Generatable
{
    public int TexSize = 256;
    public float CircleStep = 0.01f;
    public float MultiplierStep = 0.001f;

    public override void Generate()
    {
        Texture2D tex = new Texture2D(TexSize, TexSize, TextureFormat.RGB24, false);
        int x;
        int y;
        for (y = 0; y < TexSize; y++)
        {
            for (x = 0; x < TexSize; x++) tex.SetPixel(x, y, Color.red);
        }

        x = y = 0;
        float angle = 0;
        bool keepGoing = true;
        float multiplier = 0f;

        do
        {
            Debug.LogError("In loop with x: " + x + ", and y: " + y + ", and keepGoing: " + keepGoing);
            angle += CircleStep;
            multiplier += multiplier * MultiplierStep;

            var radians = Mathf.Deg2Rad * angle;
            x = (int)(Mathf.Cos(radians) * multiplier);
            y = (int)(Mathf.Sin(radians) * multiplier);

            keepGoing = Mathf.Abs(x) < TexSize / 2 && Mathf.Abs(y) < TexSize / 2;
            x += TexSize / 2;
            y += TexSize / 2;

            tex.SetPixel(x, y, Color.black);
        } while (keepGoing);

        tex.Apply();
        var bytes = tex.EncodeToPNG();
        DestroyImmediate(tex);
        File.WriteAllBytes(Application.dataPath + "/Textures/GenerateTexture.png", bytes);
    }
}
