using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

// Boids are represented by a moving, rotating triangle.
// Boids should communicate with sibling Boids
public class Boid : MonoBehaviour {
    [NonSerialized] private Vector2 _position = Vector2.zero;
    [NonSerialized] private Vector2 _velocity;
    [NonSerialized] private bool _isWrappingX = false;
    [NonSerialized] private bool _isWrappingY = false;
    [NonSerialized] private Renderer[] _renderers;
    [NonSerialized] private Vector2 _centeringVelocity;
    [NonSerialized] private int _latestNeighborhoodCount = 0;
    private BoidController _parent;

    void Start() {
        _parent = transform.parent
            .GetComponent<BoidController>(); // Parent used to perform physics math without caching
        _renderers = transform.GetComponents<Renderer>(); // Acquire Renderer(s) to check for Boid visibility
        _velocity = GetRandomVelocity(_parent.boidStartVelocity); // Acquire a Velocity Vector with a magnitude
        _position = transform.position; // Track 2D position separately
        transform.name = $"Boid {transform.GetSiblingIndex()}"; // Name the Game Object so Boids can be tracked somewhat
    }

void OnDrawGizmos() {
        var transform_ = transform;
        Handles.Label(transform_.position, $"{transform_.name} {_latestNeighborhoodCount}");
}

    void Update() {
        // Updates the rotation of the object based on the Velocity
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * -Mathf.Atan2(_velocity.x, _velocity.y));

        // Skip Flock Calculations if wrapping in progress
        if (_isWrappingX || _isWrappingY) {
            UpdateCenteringVelocity();
            _position += _centeringVelocity;
            transform.position = _position;
        }
        else {
            List<Boid> flock = _parent.localFlocks ? GetFlock(_parent.boids, _parent.boidGroupRange) : _parent.boids;
            _latestNeighborhoodCount = flock.Count;

            // Calculate all offsets and multiple by magnitudes given
            if (flock.Count > 0) {
                Vector2 r1 = Rule1(flock) * _parent.cohesionBias;
                Vector2 r2 = Rule2(flock) * _parent.separationBias;
                Vector2 r3 = Rule3(flock) * _parent.alignmentBias;
                _velocity += r1 + r2 + r3;
            }

            // Limit the Velocity Vector to a certain Magnitude
            _velocity = Util.LimitVelocity(_velocity, _parent.boidVelocityLimit);

            _position += _velocity;
            transform.position = new Vector3(_position.x, _position.y, 0);
        }

        Wrapping();
    }

    void Wrapping() {
        if (!_parent.Space.Contains(_position)) {
            // Activate Wrap, Move
            Vector2 newPosition = transform.position;
            Vector3 viewportPosition = _parent.Cam.WorldToViewportPoint(newPosition);

            if (!_isWrappingX && (viewportPosition.x > 1 || viewportPosition.x < 0)) {
                newPosition.x = -newPosition.x;
                _isWrappingX = true;
                UpdateCenteringVelocity();
            }

            if (!_isWrappingY && (viewportPosition.y > 1 || viewportPosition.y < 0)) {
                newPosition.y = -newPosition.y;
                _isWrappingY = true;
                UpdateCenteringVelocity();
            }

            transform.position = newPosition;
            _position = newPosition;
        }
        else {
            // Within the rectangle again
            _isWrappingX = false;
            _isWrappingY = false;
        }
    }

    // When Wrapping, this Velocity directs the Boid to the center of the Rectangle
    void UpdateCenteringVelocity() {
        _centeringVelocity = Util.RotateBy(new Vector2(_parent.boidVelocityLimit, _parent.boidVelocityLimit),
            Vector2.Angle(_position, _parent.Space.center));
        _centeringVelocity = Util.LimitVelocity(_parent.Space.center - _position, _parent.boidVelocityLimit / 2.0f);
    }
    
    // Returns a velocity (Vector2) at a random angle with a specific overall magnitude
    Vector2 GetRandomVelocity(float magnitude) {
        Vector2 vector = new Vector2(magnitude, magnitude);
        return Util.RotateBy(vector, Random.Range(0, 180));
    }

    // Cohesion: Steer towards center of mass of flock
    Vector2 Rule1(List<Boid> flock) {
        Vector2 center = Vector2.zero;
        foreach (Boid boid in flock)
            center += boid._position;
        center /= _parent.boids.Count;
        return (center - this._position) / 100.0f;
    }

    // Separation: Steer to avoid other Boids within flock
    Vector2 Rule2(List<Boid> flock) {
        Vector2 c = Vector2.zero;
        foreach (Boid boid in flock) {
            Vector2 diff = boid._position - this._position;
            if (diff.sqrMagnitude < _parent.boidSeparationRange)
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
            perceived += boid._velocity;
        perceived /= flock.Count;
        return (perceived - _velocity) / 8.0f;
    }

    // Returns a list of boids within a certain radius of the Boid, representing it's local 'flock'
    List<Boid> GetFlock(List<Boid> boids, float radius) {
        return boids.Where(boid => boid != this && Vector2.Distance(this._position, boid._position) <= radius).ToList();
    }
}