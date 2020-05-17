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
    [HideInInspector] public List<Boid> boids = new List<Boid>();

    // Used for wrapping
    internal Camera _cam;

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(space.center, space.size);
    }

    private void Start() {
        // Setup Camera
        _cam = Camera.main;

        // Size the Rectangle based on the Camera's Orthographic View
        float height = 2f * _cam.orthographicSize;
        Vector2 size = new Vector2(height * _cam.aspect, height);
        space = new Rect((Vector2) transform.position - size / 2, size);

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
}