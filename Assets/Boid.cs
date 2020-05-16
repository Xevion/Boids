using System.Collections.Generic;
using UnityEngine;

// Boids are represented by a moving, rotating triangle.
// Boids should communicate with sibling Boids
public class Boid : MonoBehaviour {
    public Vector2 velocity = Vector2.zero;

    void Start() {
        velocity = Vector2.one;
    }

    // Returns the next position the Boid will be moving to
    public Vector2 NextPosition(List<Boid> boids, float[] magnitudes) {
        // Find the local flock
        List<Boid> flock = GetFlock(boids, 5);
        
        // Calculate all offsets and multiple by magnitudes given
        Vector2 r1 = Rule1(flock) * magnitudes[0];
        Vector2 r2 = Rule2(flock) * magnitudes[1];
        Vector2 r3 = Rule3(flock) * magnitudes[2];

        return transform.position + (Vector3) (r1 + r2 + r3);
    }

    // Cohesion: Steer towards center of mass of flock
    Vector2 Rule1(List<Boid> flock) {
        Vector2 center = Vector2.zero;
        foreach (Boid boid in flock)
            center += (Vector2) boid.transform.position;
        center /= flock.Count;
        return (center - (Vector2) transform.position) / 100;
    }

    // Separation: Steer to avoid other Boids within flock
    Vector2 Rule2(List<Boid> flock) {
        Vector2 c = Vector2.zero;
        foreach (Boid boid in flock) {
            Vector2 diff = boid.transform.position - transform.position;
            if (diff.magnitude < 5)
                c -= diff;
        }
        return c;
    }

    // Alignment: Steer to align with the average heading of the flock
    Vector3 Rule3(List<Boid> flock) {
        Vector2 perceived = Vector2.zero;
        foreach (Boid boid in flock)
            perceived += boid.velocity;
        perceived /= flock.Count;
        return (perceived - velocity) / 8;
    }

    // Returns a list of boids within a certain radius of the Boid, representing it's local 'flock'
    List<Boid> GetFlock(List<Boid> boids, float radius) {
        List<Boid> flock = new List<Boid>();
        foreach(Boid boid in boids)
            if(boid != this && Vector2.Distance(transform.position, boid.transform.position) <= radius)
                flock.Add(boid);
        return flock;
    }
}
