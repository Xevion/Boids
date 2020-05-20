using System;
using System.Linq;
using UnityEngine;

public class Triangle : MonoBehaviour {
    public Material material;
    [NonSerialized] public Color FillColor = Color.red;
    [HideInInspector] public MeshRenderer meshRenderer;

    private void Start () {
        // Create Vector2 vertices
        var vertices2D = new Vector2[] {
            new Vector2(0,1),
            new Vector2(0.4f,0),
            new Vector2(-0.4f,0),
        };

        var vertices3D = System.Array.ConvertAll<Vector2, Vector3>(vertices2D, v => v);
 
        // Use the triangulator to get indices for creating triangles
        var triangulator = new Triangulator(vertices2D);
        var indices =  triangulator.Triangulate();
		
        // Generate a color for each vertex
        var colors = Enumerable.Repeat(FillColor, vertices3D.Length).ToArray();

        // Create the mesh
        var mesh = new Mesh {
            vertices = vertices3D,
            triangles = indices,
            colors = colors
        };
		
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
 
        // Set up game object with mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
		
        var filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = mesh;
    }
}