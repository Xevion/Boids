using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoidController : MonoBehaviour {
    // Controller Attributes
    public Rect space = new Rect(new Vector2(0, 0), new Vector2(24000, 14000));

    // Swarm Attributes
    public int boidCount = 50;
    public float boidGroupRange = 1.0f;

    // Bias changes how different rules influence individual Boids more or less
    public float separationBias = 1.0f;
    public float alignmentBias = 1.0f;
    public float cohesionBias = 1.0f;

    public GameObject boidObject;

    public List<Boid> boids = new List<Boid>();

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(space.center, space.size);
    }

    void Start() {
        for (int i = 0; i < boidCount; i++) {
            var position = new Vector2(Random.Range(-15, 15), Random.Range(-15, 15));
            var boid = Instantiate(boidObject, position, Quaternion.identity);
            
            boid.transform.parent = transform;
            boids.Add(boid.GetComponent<Boid>());
            boids[boids.Count - 1].position = position;
        }
    }

    void Update() {
        // foreach (Boid boid in boids) {
        //     if (!space.Contains(boid.position))
        //         boid.position = transform.position;
        // }
        
        float[] magnitudes = {cohesionBias, separationBias, alignmentBias};
        // Update all Boid positions
        foreach (Boid boid in boids) {
            Vector2 next = boid.NextPosition(boids, magnitudes);
            boid.position = next;
            boid.transform.position = new Vector3(next.x, next.y, 0);
        }
    }
}