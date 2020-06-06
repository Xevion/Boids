# bugs

A list of frustrating or curious bugs.

- DrawCircle Function Draw on Incorrect Axis
    - For a long period of time, I couldn't figure out why my DrawCircle function was only drawing a straight line.
    I reworked the code, looked online and did everything I could to get the function working before I gave up.
    A day later, I switched from 2D mode to Freecam mode, and I finally saw it.
    The circle was drew aligned on the incorrect axis, causing it to appear as a single line.
    The fix was was just swapping one argument with the next. `(x, 0, y) -> (x, y, 0)`
- OnCenteringVelocity applied at > 200x correct rate
    - When I implemented `Time.deltaTime` based velocity calculations, I didn't correct the OnCenteringVelocity implementation.
    This caused a strange behaviour that looked very similar to a incorrect boundary calculation.
    I spent some time checking Boundary/Wrapping related code before I forgot it, as I hadn't modified it.
    It didn't take that long, but I'm still surprised I caught it since the mistake is so small.
    The error was that since the `Time.deltaTime` value is not multiplied (scaling the 1x multiplier down to 0.02x),
    the velocity would speed it to an incredible speed for a single frame.
    This would cause all edge wrapped boids to skip a certain distance to the center of the rectangle which looked like
    an edge wrapping/boundary error.