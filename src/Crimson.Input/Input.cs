using System.Numerics;
using Crimson.Platform;

namespace Crimson.Input;

/// <summary>
/// Manage input from the keyboard, mouse, and controller, including creating and managing <see cref="ActionSet"/>s.
/// </summary>
public static class Input
{
    private static HashSet<Key> _keysDown = null!;
    private static HashSet<Key> _keysPressed = null!;

    private static Vector2 _mousePosition;
    private static Vector2 _mouseDelta;

    /// <summary>
    /// Gets the absolute mouse position in pixel coordinates, relative to the top-left corner of the window.
    /// </summary>
    public static Vector2 MousePosition => _mousePosition;

    /// <summary>
    /// Gets the change in mouse position since the last frame, in pixel coordinates.
    /// </summary>
    public static Vector2 MouseDelta => _mouseDelta;

    /// <summary>
    /// Check if the given key is held down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><see langword="true"/> if the key is held down, <see langword="false"/> otherwise.</returns>
    public static bool IsKeyDown(Key key) => _keysDown.Contains(key);

    /// <summary>
    /// Check if the given key was pressed this frame.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><see langword="true"/> if the key was pressed this frame, <see langword="false"/> otherwise.</returns>
    public static bool IsKeyPressed(Key key) => _keysPressed.Contains(key);

    /// <summary>
    /// Initialize the Input subsystem.
    /// </summary>
    public static void Init()
    {
        _keysDown = [];
        _keysPressed = [];
        
        Events.KeyDown += OnKeyDown;
        Events.KeyUp += OnKeyUp;

        Events.MouseMove += OnMouseMove;
    }

    /// <summary>
    /// Free the Input subsystem.
    /// </summary>
    public static void Free()
    {
        Events.KeyDown -= OnKeyDown;
        Events.KeyUp -= OnKeyUp;
        
        Events.MouseMove -= OnMouseMove;
    }

    /// <summary>
    /// Update the Input subsystem. This should be called once per frame, <b>before</b> event polling.
    /// </summary>
    public static void Update()
    {
        // clear the pressed keys and mouse buttons to ensure they only exist in the set for one frame.
        // as input updating is done before event polling, this means this is cleared in time for event polling.
        _keysPressed.Clear();
        
        _mouseDelta = Vector2.Zero;
    }

    private static void OnKeyDown(Key key, bool repeat)
    {
        if (repeat)
            return;
        
        _keysDown.Add(key);
        _keysPressed.Add(key);
    }
    
    private static void OnKeyUp(Key key)
    {
        _keysDown.Remove(key);
        _keysPressed.Remove(key);
    }
    
    private static void OnMouseMove(Vector2 position, Vector2 delta)
    {
        _mousePosition = position;
        _mouseDelta += delta;
    }
}