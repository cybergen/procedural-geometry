using UnityEngine;

public interface IMeshDataGenerator
{
    void Draw(Vector3 center, Vector3 up, Vector3 forward, MeshData md, float width, float depth, bool bottom = true);
}
