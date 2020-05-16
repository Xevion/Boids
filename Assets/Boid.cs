using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Boid {
    private Vector2 _position;
    private Vector2 _velocity;

    // Renders the Boid. If focused, the 
    void Render(bool focused = false) {
        
    }

    // Returns the next position the Boid will be moving to
    Vector2 NextPosition(List<Boid> boids, float[] magnitudes) {
        
    }

    // Cohesion: Steer towards center of mass of flock
    Vector2 Rule1(List<Boid> flock) {
        
    }

    List<Boid> GetFlock(List<Boid> boids, float radius) {
        List<Boid> flock = new List<Boid>();
        foreach(Boid boid in boids)
            if(Vector2.Distance(this._position, boid._position) <= radius)
                flock.Add(boid);
        return flock;
    }
}
