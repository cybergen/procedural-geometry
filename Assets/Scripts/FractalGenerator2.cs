using UnityEngine;

public class FractalGenerator2 : MonoBehaviour
{
    public ComputeShader Shader;
    public int FractalIterations;
    public Material AttachedMaterial;

    public void Generate()
    {
        var kernel = Shader.FindKernel("CSMain");

        var displace = new RenderTexture(256, 256, 24);
        displace.enableRandomWrite = true;
        displace.Create();

        var normal = new RenderTexture(256, 256, 24);
        normal.enableRandomWrite = true;
        normal.Create();

        Shader.SetTexture(kernel, "Displace", displace);
        Shader.SetTexture(kernel, "Normal", normal);

        Shader.Dispatch(kernel, 256 / 8, 256 / 8, 1);

        AttachedMaterial.SetTexture("Displacement", displace);
        AttachedMaterial.SetTexture("Normal", normal);
    }
}
