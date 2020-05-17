using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoidController : MonoBehaviour {
    // Controller Attributes
    [NonSerialized] public Rect space;

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

    // Boid Object Prefab
    public GameObject boidObject;

    // Boid Objects for Updates
    public List<Boid> boids = new List<Boid>();

    // Used for wrapping
    Camera _cam;

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(space.center, space.size);
    }

    private void Start() {
        // Setup Camera
        _cam = Camera.main;

        // Size the Rectangle based on the Camera's Orthographic View
        float height = 2f * _cam.orthographicSize;
        float width = height * _cam.aspect;
        space = new Rect(transform.position, new Vector2(width, height));

        // Add in Boid Objects / Spawn Boid Prefabs
        for (int i = 0; i < boidCount; i++) {
            var position = new Vector2(
                Random.Range(-space.size.x, space.size.x) / 2 * 0.95f,
                Random.Range(-space.size.y, space.size.y) / 2 * 0.95f);
            GameObject boid = Instantiate(boidObject, position, Quaternion.identity);

            boid.transform.parent = transform;
            boids.Add(boid.GetComponent<Boid>());
            boids[boids.Count - 1].position = position;
        }
    }

    private void Update() {
        // Wrapping Functionality
        foreach (Boid boid in boids) {
            if (!space.Contains(boid.position)) {
                // Activate Wrap, Move
                Vector2 newPosition = boid.transform.position;
                Vector3 viewportPosition = _cam.WorldToViewportPoint(newPosition);

                if (!boid.IsWrappingX && (viewportPosition.x > 1 || viewportPosition.x < 0)) {
                    newPosition.x = -newPosition.x;
                    boid.IsWrappingX = true;
                }

                if (!boid.IsWrappingY && (viewportPosition.y > 1 || viewportPosition.y < 0)) {
                    newPosition.y = -newPosition.y;
                    boid.IsWrappingY = true;
                }

                boid.transform.position = newPosition;
                boid.position = newPosition;
            }
            else {
                // Within the rectangle again
                boid.IsWrappingX = false;
                boid.IsWrappingY = false;
            }
        }

        float[] magnitudes = {cohesionBias, separationBias, alignmentBias};
        // Update all Boid positions
        foreach (Boid boid in boids) {
            Vector2 next = boid.NextPosition(boids, magnitudes);

            boid.position = next;
            boid.transform.position = new Vector3(next.x, next.y, 0);
        }
    }
}