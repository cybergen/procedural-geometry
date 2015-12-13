using UnityEngine;

public class FractalGenerator : MonoBehaviour
{
    public int FractalIterations;
    public float BaseWidth;

    public void CreateMesh()
    {
        Debug.Log("Generating mesh");

        var mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        var vertices = new Vector3[4];
        var uvs = new Vector2[4];
        var triangles = new int[12];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(1, 0, 0);
        vertices[2] = new Vector3(0.5f, 1, 0);
        vertices[3] = new Vector3(0.5f, 0.5f, 1);
        mesh.vertices = vertices;

        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0.5f, 0.5f);
        uvs[3] = new Vector2(1, 1);
        mesh.uv = uvs;

        triangles = new int[12] { 0, 2, 1, 0, 2, 3, 2, 1, 3, 0, 3, 1 };
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
