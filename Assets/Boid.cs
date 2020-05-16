using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

// Boids are represented by a moving, rotating triangle.
// Boids should communicate with sibling Boids
public class Boid : MonoBehaviour {
    public Vector2 position = Vector2.zero;
    public Vector2 velocity;
    private Renderer[] _renderers;
    bool _isWrappingX = false;
    bool _isWrappingY = false;
    Camera _cam;
    Vector3 _viewportPosition;

    void Start() {
        velocity = new Vector2(0.03f * Random.value < 0.5 ? 1 : -1, 0.03f * Random.value < 0.5 ? 1 : -1);
        _renderers = GetComponents<Renderer>();
        _cam = Camera.main;
        _viewportPosition = _cam.WorldToViewportPoint(transform.position);
    }

    void Update() {
        // Updates the rotation of the object based on the Velocity
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * -Mathf.Atan2(velocity.x, velocity.y));
        ScreenWrap();
    }

    bool CheckRenderers() {
        foreach (var renderer in _renderers) {
            // If at least one render is visible, return true
            if (renderer.isVisible) {
                return true;
            }
        }

        // Otherwise, the object is invisible
        return false;
    }

    void ScreenWrap() {
        var isVisible = CheckRenderers();

        if (isVisible) {
            _isWrappingX = false;
            _isWrappingY = false;
            return;
        }

        if (_isWrappingX && _isWrappingY) {
            return;
        }
        var newPosition = transform.position;

        if (!_isWrappingX && (_viewportPosition.x > 1 || _viewportPosition.x < 0)) {
            newPosition.x = -newPosition.x;
            _isWrappingX = true;
        }

        if (!_isWrappingY && (_viewportPosition.y > 1 || _viewportPosition.y < 0)) {
            newPosition.y = -newPosition.y;
            _isWrappingY = true;
        }

        transform.position = newPosition;
    }

    public Vector2 NextPosition(List<Boid> boids, float[] magnitudes) {
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