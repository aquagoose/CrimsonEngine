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

    private static HashSet<MouseButton> _buttonsDown = null!;
    private static HashSet<MouseButton> _buttonsPressed = null!;

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
    /// Check if the given mouse button is held down.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns><see langword="true"/> if the button is held down, <see langword="false"/> otherwise.</returns>
    public static bool IsMouseButtonDown(MouseButton button) => _buttonsDown.Contains(button);

    /// <summary>
    /// Check if the given mouse button was pressed this frame.
    /// </summary>
    /// <param name="button">The mouse button to check.</param>
    /// <returns><see langword="true"/> if the button was pressed this frame, <see langword="false"/> otherwise.</returns>
    public static bool IsMouseButtonPressed(MouseButton button) => _buttonsPressed.Contains(button);

    /// <summary>
    /// Initialize the Input subsystem.
    /// </summary>
    public static void Init()
    {
        _keysDown = [];
        _keysPressed = [];

        _buttonsDown = [];
        _buttonsPressed = [];
        
        Events.KeyDown += OnKeyDown;
        Events.KeyUp += OnKeyUp;

        Events.MouseButtonDown += OnMouseButtonDown;
        Events.MouseButtonUp += OnMouseButtonUp;
        Events.MouseMove += OnMouseMove;
    }

    /// <summary>
    /// Free the Input subsystem.
    /// </summary>
    public static void Free()
    {
        Events.KeyDown -= OnKeyDown;
        Events.KeyUp -= OnKeyUp;

        Events.MouseButtonDown -= OnMouseButtonDown;
        Events.MouseButtonUp -= OnMouseButtonUp;
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
        _buttonsPressed.Clear();
        
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
    
    private static void OnMouseButtonDown(MouseButton button)
    {
        _buttonsDown.Add(button);
        _buttonsPressed.Add(button);
    }
    
    private static void OnMouseButtonUp(MouseButton button)
    {
        _buttonsDown.Remove(button);
        _buttonsPressed.Remove(button);
    }
    
    private static void OnMouseMove(Vector2 position, Vector2 delta)
    {
        _mousePosition = position;
        _mouseDelta += delta;
    }
}