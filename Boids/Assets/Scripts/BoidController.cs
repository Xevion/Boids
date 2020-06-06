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
    [SerializeField] public int boidCount = 250;
    [SerializeField] public float boidGroupRange = 3.3f;
    [SerializeField] public float minSpeed;
    [SerializeField] public float maxSpeed;

    // Boid Rules are multiplied by this to allow rule 'tweaking'
    [SerializeField] public float globalBias = 1.0f;
    [SerializeField] public float separationBias = 2f;
    [SerializeField] public float alignmentBias = 0.288f;
    [SerializeField] public float cohesionBias = 0.3f;
    [SerializeField] public float boundaryBias = 1.5f;

    // Enable/Disable Boid Rules Altogether
    [SerializeField] public bool enableSeparation = true;
    [SerializeField] public bool enableAlignment = true;
    [SerializeField] public bool enableCohesion = true;
    [SerializeField] public bool enableBoundary = true;
    [SerializeField] public bool enableFovChecks = true;

    [SerializeField] public float boidSeparationRange = 1.4f; // Boid Separation rule's activation distance
    [SerializeField] public float boundaryForce = 50f; // The force applied when a Boid hits the boundary
    [SerializeField] public bool localFlocks = true; // Calculate Local 'Neighborhood' for flocks?
    [SerializeField] public bool edgeWrapping = true; // Enforce Edge Wrapping
    [SerializeField] public float maxSteerForce = 400f;
    [SerializeField] public float boidFov = 240;


    public Boid focusedBoid; // A focused Boid has special rendering
    public GameObject boidObject; // Boid Object Prefab
    [NonSerialized] public List<Boid> boids = new List<Boid>(); // Boid Objects for Updates
    [NonSerialized] public Camera Cam; // Used for wrapping detection

    private void OnDrawGizmos() {
        // Draw a Wire Cube for the Rectangle Area
        Gizmos.DrawWireCube(Space.center, Space.size);
        Gizmos.DrawWireCube(Boundary.center, Boundary.size);

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

    private void Update() {
        // Focus a different Boid
        if (Input.GetKeyDown("space")) {
            // Undo previous Boid's focus
            if (focusedBoid != null)
                focusedBoid.DisableFocusing();

            // Pick a Boid randomly and enable focusing
            focusedBoid = boids[Random.Range(0, boids.Count)];
            focusedBoid.EnableFocusing();
        }
        
        // Focus on the boid in scene view when one is focused
        #if UNITY_EDITOR
        if(focusedBoid != null)
            SceneView.lastActiveSceneView.LookAtDirect(focusedBoid.transform.position, Quaternion.identity);
        #endif
    }

    private void Start() {
        SetupCamera();
        AddBoids(boidCount);
    }

    /// <summary>
    /// Utility function for setting up Camera and Boundary related variables.
    /// </summary>
    private void SetupCamera() {
        // Setup Camera
        Cam = Camera.main;

        // Assert that there is an active camera
        Debug.Assert(Cam != null, nameof(Cam) + " != null");
        
        // Size the Rectangle based on the Camera's Orthographic View
        float height = 2f * Cam.orthographicSize;
        var size = new Vector2(height * Cam.aspect, height);
        Space = new Rect((Vector2) transform.position - size / 2, size);
        Boundary = new Rect(Vector2.zero, Space.size * 0.95f);
        Boundary.center = Space.center;
    }

    /// <summary>
    /// Adds a number of boids.
    /// </summary>
    /// <param name="n"></param>
    public void AddBoids(int n) {
        // Skip if negative or zero
        if (n <= 0)
            return;
        
        for (int i = 0; i < n; i++) {
            // Instantiate a Boid prefab within the boundaries randomly
            Vector2 position = RandomPosition() * 0.95f;
            GameObject boid = Instantiate(boidObject, position, Quaternion.identity);

            // Set parent, add Boid component to Boids list
            boid.transform.parent = transform;
            boids.Add(boid.GetComponent<Boid>());
        }
    }

    /// <summary>
    /// Removes a number of boids.
    /// </summary>
    /// <param name="n">Number of Boids to Remove</param>
    public void RemoveBoids(int n) {
        // If there are still Boids to remove and the number left to remove is more than 1 (post-decrementing)
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

    /// <summary>
    /// Remove a Boid at a specific index in the Boids array.
    /// </summary>
    /// <param name="index"></param>
    private void RemoveBoid(int index) {
        Boid boid = boids[index];
        boids.RemoveAt(index);
        Destroy(boid.transform.gameObject);
    }

    /// <summary>
    /// Returns a random valid Boid position
    /// </summary>
    /// <returns>A Vector2 position within the Boid boundary area</returns>
    private Vector2 RandomPosition() {
        return new Vector2(
            Random.Range(-Boundary.size.x, Boundary.size.x) / 2,
            Random.Range(-Boundary.size.y, Boundary.size.y) / 2);
    }
}