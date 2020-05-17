using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoidController : MonoBehaviour {
    // Controller Attributes
    [NonSerialized] public Rect Space;

    // Swarm Attributes
    public int boidCount = 50;
    public float boidGroupRange = 1.0f;
    public float boidStartVelocity = 0.005f;
    public float boidVelocityLimit = 1.0f;
    public float separationRange = 2.3f;

    // Bias changes how different rules influence individual Boids more or less
    public float separationBias = 0.05f;
    public float alignmentBias = 0.05f;
    public float cohesionBias = 0.05f;
    public bool localFlocks = true;

    // Boid Object Prefab
    public GameObject boidObject;

    // Boid Objects for Updates
    [NonSerialized] [HideInInspector] public List<Boid> boids = new List<Boid>();

    // Used for wrapping
    public Camera cam;

    private void OnDrawGizmos() {
        // Draw a Wire Cube for the Rectangle Area
        Gizmos.DrawWireCube(Space.center, Space.size);
        
        // Draw a Wire Cube for the Cam's Viewport Area
        Vector3 screenBottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, transform.position.z));
        Vector3 screenTopRight = cam.ViewportToWorldPoint(new Vector3(1, 1, transform.position.z));
        var screenWidth = screenTopRight.x - screenBottomLeft.x;
        var screenHeight = screenTopRight.y - screenBottomLeft.y;
        Gizmos.DrawWireCube(cam.transform.position, new Vector3(screenWidth, screenHeight, 1));
    }

    private void Start() {
        // Setup Camera
        cam = Camera.main;

        // Size the Rectangle based on the Camera's Orthographic View
        float height = 2f * cam.orthographicSize;
        Vector2 size = new Vector2(height * cam.aspect, height);
        Space = new Rect((Vector2) transform.position - size / 2, size);

        // Add in Boid Objects / Spawn Boid Prefabs
        for (int i = 0; i < boidCount; i++) {
            // Generate a new position within the Rect boundaries (minus a little)
            var position = new Vector2(
                Random.Range(-Space.size.x, Space.size.x) / 2 * 0.95f,
                Random.Range(-Space.size.y, Space.size.y) / 2 * 0.95f);
            // Spawn a new Boid prefab
            GameObject boid = Instantiate(boidObject, position, Quaternion.identity);

            // Set parent, add Boid component to Boids list
            boid.transform.parent = transform;
            boids.Add(boid.GetComponent<Boid>());
        }
    }
}