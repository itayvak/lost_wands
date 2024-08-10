using Godot;
using System;

public partial class PlayerAnimationController : Node
{
    private AnimationTree AnimationTree { get; set; }
    public bool IsOnFloor { get; set; }
    public bool IsFalling { get; set; }
    public bool IsMoving { get; set; }
    public bool IsGliding { get; set; }
    public bool IsJumping { get; set; }
    public bool IsSuperRunning { get; set; }
    public bool IsSliding { get; set; }
    public float SlideAnimSpeed { get; set; }

    public override void _Ready()
    {
        base._Ready();
        AnimationTree = GetNode<AnimationTree>("..");
        AnimationTree.Active = true;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        AnimationTree.Set("parameters/air_state/transition_request", IsFalling ? "fall" : "jump");

        var isCrouching = Input.IsActionPressed("move_down");
        var crouchValue = isCrouching ? 1 : 0;
        var crouchAnimSpeed = isCrouching ? 1.7 : 1;
        var currentCrouchValue = (float) AnimationTree.Get("parameters/crouch_value/add_amount");
        var currentCrouchAnimSpeed = (float) AnimationTree.Get("parameters/crouch_anim_speed/scale");
        
        AnimationTree.Set("parameters/crouch_value/add_amount", Mathf.Lerp(currentCrouchValue, crouchValue, 0.6f));
        AnimationTree.Set("parameters/crouch_anim_speed/scale", Mathf.Lerp(currentCrouchAnimSpeed, crouchAnimSpeed, 0.6f));

        AnimationTree.Set("parameters/is_gliding/transition_request", IsGliding ? "true" : "false");

        AnimationTree.Set("parameters/is_on_floor/transition_request", IsOnFloor ? "true" : "false");

        AnimationTree.Set("parameters/is_super_running/transition_request", IsSuperRunning ? "true" : "false");

        if(!IsOnFloor) AnimationTree.Set("parameters/on_land/request", 1);

        if(IsSuperRunning) AnimationTree.Set("parameters/reset_super_run/request", 1);

        AnimationTree.Set("parameters/is_running/transition_request", IsMoving ? "true" : "false");
        
        AnimationTree.Set("parameters/slide_anim_speed/scale", SlideAnimSpeed);
        
        AnimationTree.Set("parameters/is_sliding/transition_request", IsSliding ? "true" : "false");
    }
}
