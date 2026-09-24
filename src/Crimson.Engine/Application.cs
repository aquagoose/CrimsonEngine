namespace Crimson.Engine;

/// <summary>
/// A set of methods that run when assigned to the <see cref="App"/>. Derive this class to execute code globally, or to
/// have finer control over the engine's update loop. 
/// </summary>
public class Application : IDisposable
{
    /// <summary>
    /// Runs once, when the application is initialized.
    /// </summary>
    public virtual void Init() { }

    /// <summary>
    /// Runs every tick. The tick rate is fixed and is denoted by <see cref="App.TicksPerSecond"/>.
    /// Put physics and logic requiring a fixed tick speed in here.
    /// </summary>
    /// <param name="dt">The time in seconds since the last tick. This value is fixed.</param>
    public virtual void Tick(float dt) { }

    /// <summary>
    /// Runs every frame.
    /// Put input, per-frame logic, and rendering code in here.
    /// </summary>
    /// <param name="dt">The time in seconds since the last frame.</param>
    public virtual void Loop(float dt) { }

    /// <summary>
    /// Called when the application is disposed. Free resources in here.
    /// </summary>
    public virtual void Dispose() { }
}