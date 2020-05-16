using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController : MonoBehaviour {
    // Controller Attributes
    public Rect space;
    
    // Swarm Attributes
    public int boidCount = 1000;
    public float boidGroupRange = 1.0f;
    
    // Bias changes how different rules influence individual Boids more or less
    public float separationBias = 1.0f;
    public float alignmentBias = 1.0f;
    public float cohesionBias = 1.0f;
    
    private List<Boid> _boids = new List<Boid>();
    
    void Start() {
        
    }

    void Update()
    {
        
    }
}
