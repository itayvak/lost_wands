using Godot;
using System;

public partial class PinkSheep : CharacterBodyEntity
{
	private const float MoveSpeed = 7000f;
    private const float BounceEntityHeight = Player.JumpHeight;
    private const float FloorFriction = 0.2f;
    private const float AirAccelerationFriction = 0.2f;
    private const float AirDecelerationFriction = 0.01f;
    
    private int MoveDirection { get; set; } = 1;
    
    private AnimationPlayer AnimationPlayer { get; set; }
    private Area2D TopArea { get; set; }

    public override void _Ready()
    {
        base._Ready();
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        TopArea = GetNode<Area2D>("TopArea");
    }

    public override void _PhysicsProcess(double delta)
    {
        // TODO: DELETE THIS
        if (Input.IsActionJustPressed("jump")) MoveDirection *= -1;
        base._PhysicsProcess(delta);
		var speed = Speed;
        
        ProcessMovement(delta);
        HandleCollisions();
        
        if(IsOnFloor()) AnimationPlayer.Play("walk");

        Speed = speed;
    }

    private void ProcessMovement(double delta)
    {
        var moveDirection = GetPerpendicularNormal(MoveDirection);
        moveDirection.X *= (float)delta;
        moveDirection.Y *= (float)delta;

        var friction = IsOnFloor() ? FloorFriction : AirAccelerationFriction;
        var newSpeed = Speed;
        
        newSpeed.X = Mathf.Lerp(Speed.X, moveDirection.X, friction);
        if (IsOnFloor()) newSpeed.Y = Mathf.Lerp(Speed.Y, moveDirection.Y, friction);
        Speed = newSpeed * MoveSpeed;
    }
    
    private Vector2 GetPerpendicularNormal(int direction)
    {
        if (direction == 0) return new Vector2(0, 0);
        var perp = IsOnFloor() ? GetFloorNormal() : Vector2.Up;
        perp = (direction > 0) 
            ? new Vector2(-perp.Y, perp.X) 
            : new Vector2(perp.Y, -perp.X);
        return perp;
    }

    private void HandleCollisions()
    {
        foreach (var body in TopArea.GetOverlappingBodies())
        {
            if (body is IEntity entity)
            {
                if (body is PinkSheep) continue;
                if (body is Player player) player.Jump(BounceEntityHeight);
                else entity.ApplyForceY(BounceEntityHeight);
                QueueFree();
            }
        }
    }
}
