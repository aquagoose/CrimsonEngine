namespace Crimson.Engine.Actors;

public class Actor : IDisposable
{
    /// <summary>
    /// The name of the Actor, identifiable in the scene.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The transform of this Actor.
    /// </summary>
    public Transform Transform;

    public Actor(string name, Transform transform)
    {
        Name = name;
        Transform = transform;
    }

    public Actor(string name) : this(name, new Transform()) { }

    public virtual void Init() { }

    public virtual void Tick(float dt) { }

    public virtual void Loop(float dt) { }

    public virtual void Dispose() { }
}