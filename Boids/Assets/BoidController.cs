using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

public class BoidController : MonoBehaviour {
    // Controller Attributes
    [NonSerialized] public Rect Space;
    [NonSerialized] public Rect Boundary;

    // Swarm Attributes
    public int boidCount = 50;
    public float boidGroupRange = 1.0f;
    public float boidStartVelocity = 0.005f;
    public float boidVelocityLimit = 1.0f;
    public float boidSeparationRange = 2.3f;

    // Bias changes how different rules influence individual Boids more or less
    [SerializeField] public float separationBias = 0.05f;
    [SerializeField] public float alignmentBias = 0.05f;
    [SerializeField] public float cohesionBias = 0.05f;
    [SerializeField] public float boundaryBias = 1f;
    [SerializeField] public float boundaryForce = 10f;
    [SerializeField] public bool localFlocks = true;
    [SerializeField] public bool edgeWrapping = true;

    public Boid focusedBoid; // A focused Boid has special rendering
    public GameObject boidObject; // Boid Object Prefab
    [NonSerialized] public List<Boid> boids = new List<Boid>(); // Boid Objects for Updates
    [NonSerialized] public Camera Cam; // Used for wrapping detection

    private void OnDrawGizmos() {
        // Draw a Wire Cube for the Rectangle Area
        Gizmos.DrawWireCube(Space.center, Space.size);
        Gizmos.DrawWireCube(Boundary.center, Boundary.size);

        if (focusedBoid != null)
            Handles.DrawWireDisc(focusedBoid.transform.position, Vector3.forward, boidGroupRange);

        if (Cam == null)
            return;

        // Draw a Wire Cube for the Cam's Viewport Area
        Vector3 position = transform.position;
        Vector3 screenBottomLeft = Cam.ViewportToWorldPoint(new Vector3(0, 0, position.z));
        Vector3 screenTopRight = Cam.ViewportToWorldPoint(new Vector3(1, 1, position.z));
        var screenWidth = screenTopRight.x - screenBottomLeft.x;
        var screenHeight = screenTopRight.y - screenBottomLeft.y;
        Gizmos.DrawWireCube(Cam.transform.position, new Vector3(screenWidth, screenHeight, 1));
    }

    private void Start() {
        // Setup Camera
        Cam = Camera.main;

        // Size the Rectangle based on the Camera's Orthographic View
        float height = 2f * Cam.orthographicSize;
        var size = new Vector2(height * Cam.aspect, height);
        Space = new Rect((Vector2) transform.position - size / 2, size);
        Boundary = new Rect(Vector2.zero, Space.size * 0.95f);
        Boundary.center = Space.center;
        
        AddBoids(boidCount);
    }

    public void AddBoids(int n) {
        for (int i = 0; i < n; i++) {
            // Instantiate a Boid prefab within the boundaries randomly
            Vector2 position = RandomPosition() * 0.95f;
            GameObject boid = Instantiate(boidObject, position, Quaternion.identity);

            // Set parent, add Boid component to Boids list
            boid.transform.parent = transform;
            boids.Add(boid.GetComponent<Boid>());
        }
    }

    public void RemoveBoids(int n) {
        while (n-- > 0 && boids.Count >= 1) {
            int index = Random.Range(0, boids.Count - 1);

            // Only remove the focused Boid if it is the last one
            if (boids[index] == focusedBoid)
                if (boids.Count == 1)
                    RemoveBoid(1);
                else
                    n++;
            else
                RemoveBoid(index);
        }
    }

    private void RemoveBoid(int index) {
        Boid boid = boids[index];
        boids.RemoveAt(index);
        Destroy(boid.transform.gameObject);
    }

    private Vector2 RandomPosition() {
        return new Vector2(
            Random.Range(-Boundary.size.x, Boundary.size.x) / 2,
            Random.Range(-Boundary.size.y, Boundary.size.y) / 2);
    }
}