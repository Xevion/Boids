using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Boids are represented by a moving, rotating triangle.
// Boids should communicate with sibling Boids
public class Boid : MonoBehaviour {
    [NonSerialized] public Vector2 position = Vector2.zero;
    [NonSerialized] public Vector2 velocity;
    [NonSerialized] public bool IsWrappingX = false;
    [NonSerialized] public bool IsWrappingY = false;
    private BoidController parent;

    void Start() {
        parent = transform.parent.GetComponent<BoidController>();
        // Acquire a Velocity Vector with a magnitude
        velocity = GetRandomVelocity(parent.boidStartVelocity);
    }

    void Update() {
        // Updates the rotation of the object based on the Velocity
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * -Mathf.Atan2(velocity.x, velocity.y));

        // Skip Flock Calculations if wrapping in progress
        if (!IsWrappingX && !IsWrappingY) {
            // Acquires all Boids within the local flock
            // List<Boid> flock = GetFlock(parent.boids, parent.boidGroupRange);
            List<Boid> flock = parent.boids;

            if (flock.Count > 0) {
                // Calculate all offsets and multiple by magnitudes given
                Vector2 r1 = Rule1(flock) * parent.cohesionBias;
                Vector2 r2 = Rule2(flock) * parent.separationBias;
                Vector2 r3 = Rule3(flock) * parent.alignmentBias;
                velocity += r1 + r2 + r3;
            }

            // Limit the Velocity Vector to a certain Magnitude
            if (velocity.magnitude > parent.boidVelocityLimit) {
                velocity = (velocity / velocity.magnitude) * parent.boidVelocityLimit;
            }

        }

        // Update 2D and 3D transform positions based on current velocity
        position += velocity;
        transform.position = new Vector3(position.x, position.y, 0);

        // If either dimension of wrapping is still unlocked, check wrapping code.
        if(!IsWrappingX || !IsWrappingY)
            Wrapping();
    }

    void Wrapping() {
        if (!parent.space.Contains(position)) {
            // Activate Wrap, Move
            Vector2 newPosition = transform.position;
            Vector3 viewportPosition = parent._cam.WorldToViewportPoint(newPosition);

            if (!IsWrappingX && (viewportPosition.x > 1 || viewportPosition.x < 0)) {
                newPosition.x = -newPosition.x;
                IsWrappingX = true;
            }

            if (!IsWrappingY && (viewportPosition.y > 1 || viewportPosition.y < 0)) {
                newPosition.y = -newPosition.y;
                IsWrappingY = true;
            }

            transform.position = newPosition;
            position = newPosition;
        }
        else {
            // Within the rectangle again
            IsWrappingX = false;
            IsWrappingY = false;
        }
    }

    Vector2 GetRandomVelocity(float magnitude) {
        Vector2 vector = new Vector2(magnitude, magnitude);
        return Util.RotateBy(vector, Random.Range(0, 180));
    }

    // Cohesion: Steer towards center of mass of flock
    Vector2 Rule1(List<Boid> flock) {
        Vector2 center = Vector2.zero;
        foreach (Boid boid in flock)
            center += boid.position;
        center /= parent.boids.Count;
        return (center - this.position) / 100;
    }

    // Separation: Steer to avoid other Boids within flock
    Vector2 Rule2(List<Boid> flock) {
        Vector2 c = Vector2.zero;
        foreach (Boid boid in flock) {
            Vector2 diff = boid.position - this.position;
            if (diff.magnitude < parent.separationRange)
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