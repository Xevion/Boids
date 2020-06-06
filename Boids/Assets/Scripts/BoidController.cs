using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoidController : MonoBehaviour {

    // Manually managed/set
    public GameObject boidObject; // Boid Object Prefab
    
    // Swarm Attributes
    public int boidCount = 250;
    public float boidGroupRange = 3.3f;
    public float minSpeed;
    public float maxSpeed;

    // Boid Rules are multiplied by this to allow rule 'tweaking'
    public float globalBias = 1.0f;
    public float separationBias = 2f;
    public float alignmentBias = 0.288f;
    public float cohesionBias = 0.3f;
    public float boundaryBias = 1.5f;

    // Enable/Disable Boid Rules Altogether
    public bool enableSeparation = true;
    public bool enableAlignment = true;
    public bool enableCohesion = true;
    public bool enableBoundary = true;
    public bool enableFovChecks = true;

    public float boidSeparationRange = 1.4f; // Boid Separation rule's activation distance
    public float boundaryForce = 50f; // The force applied when a Boid hits the boundary
    public bool localFlocks = true; // Calculate Local 'Neighborhood' for flocks?
    public bool edgeWrapping = true; // Enforce Edge Wrapping
    public float maxSteerForce = 400f;
    public float boidFov = 240;

    // Runtime Controller Attributes
    [NonSerialized] public List<Boid> boids = new List<Boid>(); // Boid Objects for Updates
    [NonSerialized] public Rect Space;
    [NonSerialized] public Rect Boundary;
    [NonSerialized] public Camera Cam; // Used for wrapping detection
    public Boid focusedBoid; // A focused Boid has special rendering

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
        Boundary = new Rect(Vector2.zero, Space.size * 0.90f);
        Boundary.center = Space.center;
    }

    /// <summary>
    /// Adds a number of boids.
    /// </summary>
    /// <param name="n">Number of Boids to add</param>
    /// <param name="useNearby">Spawn Boids nearby each other</param>
    public void AddBoids(int n, bool useNearby = false) {
        // Skip if negative or zero
        if (n <= 0)
            return;
        
        for (int i = 0; i < n; i++) {
            // Instantiate a Boid prefab within the boundaries randomly
            Vector2 position = useNearby ? RandomNearbyPosition() : RandomPosition() * 0.90f;
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

    /// <summary>
    /// Returns a random valid position near a Boid
    /// </summary>
    /// <returns>A Vector2 position within the set boundaries</returns>
    private Vector2 RandomNearbyPosition(int maxRetries = 5) { 
        for (int i = 0; i < maxRetries; i++) {
            Vector2 possible = RandomBoid().GetNearby(boidSeparationRange);
            if (Boundary.Contains(possible))
                return possible;
        }

        // if MaxRetries exceeded, fall back to RandomPosition()
        Debug.Log($"{maxRetries} retries failed, falling back to RandomPosition");
        return RandomPosition();
    }

    /// <summary>
    /// Returns a random Boid
    /// </summary>
    /// <returns></returns>
    public Boid RandomBoid() {
        return boids[Random.Range(0, boids.Count - 1)];
    }
}