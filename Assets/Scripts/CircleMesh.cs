using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CircleMesh : MonoBehaviour
{
    [SerializeField] public float radius = 1.0f;
    [SerializeField] public int segments = 24;
    [SerializeField] public float startAngle = 0.0f;
    [SerializeField] public float endAngle = 360.0f;

    private Mesh mesh;
    private MeshFilter meshFilter;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.name = "Circle Mesh";
        meshFilter.mesh = mesh;
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        if (mesh == null) return;

        Vector3[] vertices = new Vector3[segments + 2];
        vertices[0] = Vector3.zero; // Center point

        float angleStep = (endAngle - startAngle) / segments;

        // Draw circle in XZ plane
        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + i * angleStep;
            float rad = angle * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(
                radius * Mathf.Cos(rad),    // X
                0,                          // Y (constant)
                radius * Mathf.Sin(rad)     // Z
            );
        }

        // Generate triangles with correct winding order for upward normal
        int[] triangles = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;           // Center
            triangles[i * 3 + 2] = i + 1;   // Current vertex
            triangles[i * 3 + 1] = i + 2;   // Next vertex
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void OnValidate()
    {
        if (mesh != null)
        {
            UpdateMesh();
        }
    }
}