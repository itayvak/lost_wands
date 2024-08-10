using Godot;
using System;

public partial class CharacterBodyEntity : CharacterBody2D, IEntity
{
    public Vector2 Speed { get; protected set; }
    
    public void ApplyForce(Vector2 force) => Speed += force;
    public void ApplyForceX(float force) => ApplyForce(new Vector2(force, 0));
    public void ApplyForceY(float force) => ApplyForce(new Vector2(0, force));
    
    public void Move(double delta)
    {
        Velocity = Speed;
        MoveAndSlide();
        Speed = Velocity;
    }

    public override void _Ready()
    {
        base._Ready();
        AddToGroup("Entity");
    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if(!IsOnFloor()) 
            ApplyForceY(IEntity.Gravity * (float)delta);
        Move(delta);
    }
}
