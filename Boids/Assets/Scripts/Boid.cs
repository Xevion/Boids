using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {
    // Basic Boid Physics Attributes
    private Vector2 _position = Vector2.zero;
    public Vector2 velocity;

    // Wrapping related attributes
    private bool _isWrappingX;
    private bool _isWrappingY;
    private Vector2 _centeringVelocity;

    // Used for tracking Gizmo drawing
    public int latestNeighborhoodCount = 0;
    public List<Boid> latestNeighborhood;

    // Parent Boid Controller
    private BoidController _parent;

    public bool isFocused; // Whether or not the current Boid is focused
    private LineRenderer[] _lineRenderers; // Store LineRenderers used by Focused Boids

    private void Start() {
        _parent = transform.parent
            .GetComponent<BoidController>(); // Parent used to perform physics math without caching
        velocity = Util.GetRandomVelocity(Random.Range(_parent.minSpeed,
            _parent.maxSpeed)); // Acquire a Velocity Vector with a magnitude
        _position = transform.position; // Track 2D position separately
        transform.name = $"Boid {transform.GetSiblingIndex()}"; // Name the Game Object so Boids can be tracked somewhat
    }

    private void Update() {
        // Updates the rotation of the object based on the Velocity
        transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * -Mathf.Atan2(velocity.x, velocity.y));

        // Skip Flock Calculations if wrapping in progress
        if (_isWrappingX || _isWrappingY) {
            UpdateCenteringVelocity();
            _position += _centeringVelocity * Time.deltaTime;
            transform.position = _position;
        }
        else {
            // Find local neighborhood flock
            Vector2 acceleration = Vector2.zero;
            List<Boid> flock = _parent.localFlocks ? GetFlock(_parent.boids, _parent.boidGroupRange) : _parent.boids;
            latestNeighborhoodCount = flock.Count;

            // Only update latest neighborhood when we need it for focused boid Gizmo draws
            if (isFocused)
                latestNeighborhood = flock;

            // Calculate all offsets and multiple by magnitudes given
            if (flock.Count > 0) {
                if (_parent.enableCohesion)
                    acceleration += SteerTowards(Rule1(flock)) * _parent.cohesionBias;
                if (_parent.enableSeparation)
                    acceleration += SteerTowards(Rule2(flock)) * _parent.separationBias;
                if (_parent.enableAlignment)
                    acceleration += SteerTowards(Rule3(flock)) * _parent.alignmentBias;
            }

            if (_parent.enableBoundary && !_parent.Boundary.Contains(_position)) {
                acceleration += SteerTowards(RuleBound()) * _parent.boundaryBias;
            }

            // Limit the Velocity Vector to a certain Magnitude
            velocity += acceleration * Time.deltaTime;
            float speed = velocity.magnitude;
            Vector2 dir = velocity / speed;
            speed = Mathf.Clamp(speed, _parent.minSpeed, _parent.maxSpeed);
            velocity = dir * speed;

            _position += velocity * Time.deltaTime;
            transform.position = _position;
            // transform.forward = dir;
        }

        if (_parent.edgeWrapping)
            Wrapping();
    }

    /// <summary>
    /// Assists with clamping and normalizing a Vector2 force
    /// </summary>
    /// <param name="vector">Force Vector being applied by a rule</param>
    /// <returns>Vector2 force to be applied</returns>
    private Vector2 SteerTowards(Vector2 vector) {
        Vector2 v = vector.normalized * _parent.maxSpeed - velocity;
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
            perceived += boid.velocity;
        perceived /= flock.Count;
        return (perceived - velocity) / 8.0f;
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
        List<Boid> flock = new List<Boid>();

        foreach (Boid boid in boids) {
            // Distance Check
            if (boid == this || Vector2.Distance(this._position, boid._position) > radius)
                continue;

            // FOV Check
            if (_parent.enableFovChecks) {
                float angle1 = Util.Vector2ToAngle(velocity); // Current Heading
                float angle2 =
                    Util.AngleBetween(transform.position, boid.transform.position); // Angle between Boid and other Boid

                // Outside of FOV range, skip
                if (Mathf.Abs(Mathf.DeltaAngle(angle1, angle2)) > _parent.boidFov / 2)
                    continue;
            }

            // Boid passed all checks, add to local Flock list
            flock.Add(boid);
        }

        return flock;
    }

    // Sets up a Boid to be 'Focused', adds Circles around object and changes color
    public void EnableFocusing() {
        if (isFocused) {
            Debug.LogWarning($"enableFocusing called on previously focused Boid ({transform.name})");
            return;
        }

        isFocused = true;

        // Create all LineRenderers
        _lineRenderers = new LineRenderer[3];
        _lineRenderers[0] = GetLineRenderer("Group Range");
        _lineRenderers[1] = GetLineRenderer("Separation Range");
        _lineRenderers[2] = GetLineRenderer("FOV Arc");

        // Update Mesh Material Color
        var triangle = transform.GetComponent<Triangle>();
        triangle.meshRenderer.material.color = Color.red;

        // Draw all focus related elements
        Draw(false);
    }

    // Disable focusing, removing LineRenderers and resetting color
    public void DisableFocusing() {
        isFocused = false;

        // Update Mesh Material Color
        var oldTriangle = transform.GetComponent<Triangle>();
        oldTriangle.meshRenderer.material.color = new Color32(49, 61, 178, 255);


        // Destroy Line Renderers (and child GameObjects)
        for (int i = 0; i < _lineRenderers.Length; i++) {
            _lineRenderers[i].positionCount = 0;
            DestroyImmediate(_lineRenderers[i].gameObject);
            _lineRenderers[i] = null;
        }
    }

    /// <summary>
    /// returns a new LineRenderer component stored on a child GameObject.
    /// </summary>
    /// <param name="childName">The name of the associated child GameObject</param>
    /// <returns>A LineRenderer</returns>
    public LineRenderer GetLineRenderer(string childName) {
        var child = new GameObject(childName);
        // Make object a child of Boid, set position as such
        child.transform.SetParent(transform);
        child.transform.position = transform.position;
        // add and return LineRenderer component
        return child.AddComponent<LineRenderer>();
    }

    /// <summary>
    /// Draw (or re-draw) all lines corresponding to separation and group circles and FOV Arc
    /// </summary>
    /// <param name="redraw"><c>true</c> if draw operation should be treated as a re-draw</param>
    public void Draw(bool redraw) {
        // Clear positions when redrawing
        if (redraw)
            foreach (LineRenderer lineRenderer in _lineRenderers)
                lineRenderer.positionCount = 0;

        // Add a LineRenderer for Radius Drawing
        if (_parent.enableFovChecks)
            ShapeDraw.DrawArc(_lineRenderers[2], _parent.boidFov, _parent.boidGroupRange); // FOV Arc
        else
            ShapeDraw.DrawCircle(_lineRenderers[0], _parent.boidGroupRange); // Group Circle
        ShapeDraw.DrawCircle(_lineRenderers[1], _parent.boidSeparationRange); // Separation Circle

        if (_parent.enableFovChecks) {
            // Set FOV Arc rotation to mimic Boid transform rotation
            _lineRenderers[2].transform.rotation = transform.rotation;
            _lineRenderers[2].transform.Rotate(0, 0, _parent.boidFov / 2f);
        }
    }

    /// <summary>
    /// Return a random position nearby a certain distance away
    /// </summary>
    /// <param name="distance">Vector2 position</param>
    /// <returns></returns>
    public Vector2 GetNearby(float distance) {
        return _position + Util.RotateBy(new Vector2(distance, distance),
            Random.Range(0f, 360f));
    }
}