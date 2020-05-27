using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    [NonSerialized] private BoidController _parent;
    [NonSerialized] public bool isFocused = false;

    private void Start() {
        _parent = transform.parent
            .GetComponent<BoidController>(); // Parent used to perform physics math without caching
        _renderers = transform.GetComponents<Renderer>(); // Acquire Renderer(s) to check for Boid visibility
        _velocity = Util.GetRandomVelocity(_parent.boidStartVelocity); // Acquire a Velocity Vector with a magnitude
        _position = transform.position; // Track 2D position separately
        transform.name = $"Boid {transform.GetSiblingIndex()}"; // Name the Game Object so Boids can be tracked somewhat
    }

    private void Update() {
        // Updates the rotation of the object based on the Velocity
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * -Mathf.Atan2(_velocity.x, _velocity.y));

        // Skip Flock Calculations if wrapping in progress
        if (_isWrappingX || _isWrappingY) {
            UpdateCenteringVelocity();
            _position += _centeringVelocity * Time.deltaTime;
            transform.position = _position;
        }
        else {
            Vector2 acceleration = Vector2.zero;
            List<Boid> flock = _parent.localFlocks ? GetFlock(_parent.boids, _parent.boidGroupRange) : _parent.boids;
            _latestNeighborhoodCount = flock.Count;

            // Calculate all offsets and multiple by magnitudes given
            if (flock.Count > 0) {
                if (_parent.enableCohesion)
                    acceleration += SteerTowards(Rule1(flock)) * _parent.cohesionBias;
                if (_parent.enableSeparation)
                    acceleration += SteerTowards(Rule2(flock)) * _parent.separationBias;
                if (_parent.enableAlignment)
                    acceleration += SteerTowards(Rule3(flock)) * _parent.alignmentBias;
            }

            if (_parent.enableBoundary && _parent.Boundary.Contains(_position))
                acceleration += SteerTowards(RuleBound()) * _parent.boundaryBias;

            // Limit the Velocity Vector to a certain Magnitude
            _velocity += acceleration * Time.deltaTime;
            float speed = _velocity.magnitude;
            Vector2 dir = _velocity / speed;
            speed = Mathf.Clamp(speed, _parent.minSpeed, _parent.maxSpeed);
            _velocity = dir * speed;

            _position += _velocity * Time.deltaTime;
            transform.position = _position;
            // transform.forward = dir;
        }

        if (_parent.edgeWrapping)
            Wrapping();
    }

    private Vector2 SteerTowards(Vector2 vector) {
        Vector2 v = vector.normalized * _parent.maxSpeed - _velocity;
        return Vector2.ClampMagnitude(v, _parent.maxSteerForce);
    }

    private void Wrapping() {
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
    private void UpdateCenteringVelocity() {
        _centeringVelocity = Util.RotateBy(new Vector2(_parent.maxSpeed, _parent.maxSpeed),
            Vector2.Angle(_position, _parent.Space.center));
        _centeringVelocity = Util.MaxVelocity(_parent.Space.center - _position, _parent.maxSpeed / 2.0f);
    }

    // Cohesion: Steer towards center of mass of flock
    private Vector2 Rule1(List<Boid> flock) {
        Vector2 center = Vector2.zero;
        foreach (Boid boid in flock)
            center += boid._position;
        center /= _parent.boids.Count;
        return (center - this._position) / 100.0f;
    }

    // Separation: Steer to avoid other Boids within flock
    private Vector2 Rule2(List<Boid> flock) {
        Vector2 c = Vector2.zero;
        foreach (Boid boid in flock) {
            Vector2 diff = boid._position - this._position;
            if (diff.sqrMagnitude < _parent.boidSeparationRange * _parent.boidSeparationRange)
                c -= diff;
        }

        return c;
    }

    // Alignment: Steer to align with the average heading of the flock
    private Vector2 Rule3(List<Boid> flock) {
        if (flock.Count == 0)
            return Vector2.zero;

        Vector2 perceived = Vector2.zero;
        foreach (Boid boid in flock)
            perceived += boid._velocity;
        perceived /= flock.Count;
        return (perceived - _velocity) / 8.0f;
    }

    // Asks Boids to stay within the Boundaries set
    private Vector2 RuleBound() {
        Vector2 vector = Vector2.zero;

        // Boundary X Force
        if (_position.x < _parent.Boundary.xMin)
            vector.x = _parent.boundaryForce *
                       Mathf.InverseLerp(_parent.Boundary.xMin, _parent.Space.xMin, _position.x);
        else if (_position.x > _parent.Boundary.xMax)
            vector.x = -_parent.boundaryForce *
                       Mathf.InverseLerp(_parent.Boundary.xMax, _parent.Space.xMax, _position.x);

        // Boundary Y Force
        if (_position.y < _parent.Boundary.yMin)
            vector.y = _parent.boundaryForce *
                       Mathf.InverseLerp(_parent.Boundary.yMin, _parent.Space.yMin, _position.y);
        else if (_position.y > _parent.Boundary.yMax)
            vector.y = -_parent.boundaryForce *
                       Mathf.InverseLerp(_parent.Boundary.yMax, _parent.Space.yMax, _position.y);

        return vector;
    }

    // Returns a list of boids within a certain radius of the Boid, representing it's local 'flock'
    private List<Boid> GetFlock(List<Boid> boids, float radius) {
        return boids.Where(boid => boid != this && Vector2.Distance(this._position, boid._position) <= radius).ToList();
    }

    // Sets up a Boid to be 'Focused', adds Circles around object and changes color
    public void EnableFocusing() {
        if (isFocused) {
            Debug.LogWarning("enableFocusing called on previously focused Boid");
            return;
        }

        isFocused = true;

        // Update Mesh Material Color
        var triangle = transform.GetComponent<Triangle>();
        triangle.meshRenderer.material.color = Color.red;

        // Add a LineRenderer for Radius Drawing
        DrawCircle(_parent.boidSeparationRange, "Separation Range Circle");
        DrawCircle(_parent.boidGroupRange, "Group Range Circle");
    }

    // Disable focusing, removing LineRenderers and resetting color
    public void DisableFocusing() {
        isFocused = false;

        // Update Mesh Material Color
        var oldTriangle = transform.GetComponent<Triangle>();
        oldTriangle.meshRenderer.material.color = new Color32(49, 61, 178, 255);

        // Destroy Line Renderers (and child GameObjects)
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    private void DrawCircle(float radius, string childName) {
        // Create a new child GameObject to hold the LineRenderer Component
        var child = new GameObject(childName);
        child.transform.SetParent(transform);
        child.transform.position = transform.position;
        var line = child.AddComponent<LineRenderer>();

        _parent.circleVertexCount = 360;

        // Setup LineRenderer properties
        line.useWorldSpace = false;
        line.startWidth = _parent.circleWidth;
        line.endWidth = _parent.circleWidth;
        line.positionCount = _parent.circleVertexCount + 1;

        // Calculate points for circle
        var pointCount = _parent.circleVertexCount + 1;
        var points = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++) {
            var rad = Mathf.Deg2Rad * (i * 360f / _parent.circleVertexCount);
            points[i] = new Vector3(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius, 0);
        }

        // Add points to LineRenderer
        line.SetPositions(points);
    }
}