using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using Random = UnityEngine.Random;

// Boids are represented by a moving, rotating triangle.
// Boids should communicate with sibling Boids
public class Boid : MonoBehaviour {
    [NonSerialized] public Vector2 position = Vector2.zero;
    public Vector2 velocity;
    [NonSerialized] public Renderer[] renderers;
    [NonSerialized] public bool IsWrappingX = false;
    [NonSerialized] public bool IsWrappingY = false;

    void Start() {
        velocity = new Vector2(0.03f * Random.value < 0.5 ? 1 : -1, 0.03f * Random.value < 0.5 ? 1 : -1);
        renderers = GetComponents<Renderer>();
    }

    void Update() {
        // Updates the rotation of the object based on the Velocity
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * -Mathf.Atan2(velocity.x, velocity.y));
    }
    
    public Vector2 NextPosition(List<Boid> boids, float[] magnitudes) {
        if (IsWrappingX || IsWrappingY)
            return position + velocity;
        
        // Acquires all
        List<Boid> flock = GetFlock(boids, 20);

        if (flock.Count > 0) {
            // Calculate all offsets and multiple by magnitudes given
            Vector2 r1 = Rule1(flock) * magnitudes[0];
            Vector2 r2 = Rule2(flock) * magnitudes[1];
            Vector2 r3 = Rule3(flock) * magnitudes[2];
            velocity += r1 + r2 + r3;
        }

        LimitVelocity();

        return position + velocity;
    }

    void LimitVelocity() {
        if (velocity.magnitude > 2f) {
            velocity = (velocity / velocity.magnitude) * 2f;
        }
    }

    // Cohesion: Steer towards center of mass of flock
    Vector2 Rule1(List<Boid> flock) {
        Vector2 center = Vector2.zero;
        foreach (Boid boid in flock)
            center += boid.position;
        center /= flock.Count;
        return (center - this.position) / 100;
    }

    // Separation: Steer to avoid other Boids within flock
    Vector2 Rule2(List<Boid> flock) {
        Vector2 c = Vector2.zero;
        foreach (Boid boid in flock) {
            Vector2 diff = boid.position - this.position;
            if (diff.magnitude < 5)
                c -= diff;
        }

        return c;
    }

    // Alignment: Steer to align with the average heading of the flock
    Vector3 Rule3(List<Boid> flock) {
        if (flock.Count == 0)
            return Vector2.zero;

        Vector2 perceived = Vector2.zero;
        foreach (Boid boid in flock)
            perceived += boid.velocity;
        perceived /= flock.Count;
        return (perceived - velocity) / 8;
    }

    // Returns a list of boids within a certain radius of the Boid, representing it's local 'flock'
    List<Boid> GetFlock(List<Boid> boids, float radius) {
        List<Boid> flock = new List<Boid>();
        foreach (Boid boid in boids)
            if (boid != this && Vector2.Distance(this.position, boid.position) <= radius)
                flock.Add(boid);
        return flock;
    }
}