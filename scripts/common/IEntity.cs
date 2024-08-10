using Godot;
using System;

public interface IEntity
{
    public Vector2 Speed { get; }
    static readonly float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public void Move(double delta);
    public void ApplyForce(Vector2 force);
    public void ApplyForceX(float force);
    public void ApplyForceY(float force);
}
