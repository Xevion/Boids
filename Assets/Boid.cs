using System.Collections.Generic;
using UnityEngine;

public class Boid {
    private Vector2 _position;
    private Vector2 _velocity;

    Boid(Vector2 position, Vector2 velocity) {
        _position = position;
        _velocity = velocity;
    }

    public void SetPosition(Vector2 position) {
        _position = position;
    }
    
    // Renders the Boid. If focused, the 
    void Render(bool focused = false) {
        
    }

    // Returns the next position the Boid will be moving to
    Vector2 NextPosition(List<Boid> boids, float[] magnitudes) {
        // Find the local flock
        List<Boid> flock = GetFlock(boids, 5);
        
        // Calculate all offsets and multiple by magnitudes given
        Vector2 r1 = Rule1(flock) * magnitudes[0];
        Vector2 r2 = Rule2(flock) * magnitudes[1];
        Vector2 r3 = Rule3(flock) * magnitudes[2];

        return _position + r1 + r2 + r3;
    }

    // Cohesion: Steer towards center of mass of flock
    Vector2 Rule1(List<Boid> flock) {
        Vector2 center = Vector2.zero;
        foreach (Boid boid in flock)
            center += boid._position;
        center /= flock.Count;
        return (center - _position) / 100;
    }

    // Separation: Steer to avoid other Boids within flock
    Vector2 Rule2(List<Boid> flock) {
        Vector2 c = Vector2.zero;
        foreach (Boid boid in flock) {
            Vector2 diff = boid._position - _position;
            if (diff.magnitude < 5)
                c -= diff;
        }
        return c;
    }

    // Alignment: Steer to align with the average heading of the flock
    Vector3 Rule3(List<Boid> flock) {
        Vector2 perceived = Vector2.zero;
        foreach (Boid boid in flock)
            perceived += boid._velocity;
        perceived /= flock.Count;
        return (perceived - _velocity) / 8;
    }

    List<Boid> GetFlock(List<Boid> boids, float radius) {
        List<Boid> flock = new List<Boid>();
        foreach(Boid boid in boids)
            if(boid != this && Vector2.Distance(_position, boid._position) <= radius)
                flock.Add(boid);
        return flock;
    }
}
