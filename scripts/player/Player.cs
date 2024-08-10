using Godot;
using System;

public partial class Player : CharacterBodyEntity
{
    [Export]
    private const float SpriteScale = 0.24f;
    private const float RunSpeed = 18000f;
    private const float SuperRunSpeed = 50000f;
    private const float StartingSlideSpeed = 50000f;
    private const float FloorFriction = 0.2f;
    private const float AirAccelerationFriction = 0.2f;
    private const float AirDecelerationFriction = 0.01f;
    private const float SlideDeceleration = 0.02f;
    public const float JumpHeight = -1200f;
    private const int MaxGlideCharge = 40;
    private const int FramesForSuperRun = (int)(60 * 3.5);

    private int _jumpBuffer;
    private int _coyoteFrames;
    private int _superRunFrames;
    private int _glideCharge;
    private int _slideDirection;
    private float _slideSpeed;
    private bool _isMidJump;
    
    private PlayerAnimationController AnimationController { get; set; }
    private Node2D SpritesHolder { get; set; }
    private PlayerParticleController ParticleController { get; set; }

    public override void _Ready()
    {
        base._Ready();
        AnimationController = GetNode<PlayerAnimationController>("AnimationTree/AnimationController");
        ParticleController = GetNode<PlayerParticleController>("Particles");
        SpritesHolder = GetNode<Node2D>("Sprites");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        ProcessMovement(delta);
        ProcessJump(delta);
        ProcessGlide(delta);
        ProcessSlide(delta);

        var isOnFloor = _coyoteFrames > 0;
        var isRunning = Mathf.Abs(Speed.X) > RunSpeed*delta / 2;
        var isSuperRunning = _superRunFrames > FramesForSuperRun;
        AnimationController.IsOnFloor = isOnFloor;
        AnimationController.IsFalling = Speed.Y > 0;
        AnimationController.IsMoving = isRunning;
        AnimationController.IsSuperRunning = isSuperRunning;
        AnimationController.IsSliding = Input.IsActionPressed("slide") && Mathf.Abs(_slideSpeed) > 1;
        AnimationController.SlideAnimSpeed = (_slideSpeed / StartingSlideSpeed);
        ParticleController.SetRunSmokePlaying(isOnFloor && isRunning && !isSuperRunning, -Mathf.Clamp(Speed.X, -1, 1));

        if(Speed.X < 0) 
        {
            SpritesHolder.Scale = new Vector2(-SpriteScale, SpriteScale);
        }
        else 
        {
            SpritesHolder.Scale = new Vector2(SpriteScale, SpriteScale);
        }
    }

    private void ProcessMovement(double delta)
    {
        if (IsOnFloor()) _coyoteFrames = 10;
        else if (_coyoteFrames > 0) _coyoteFrames -= 1;
        
        var moveSpeed = _superRunFrames > FramesForSuperRun ? SuperRunSpeed : RunSpeed;
        var moveDirection = GetMovementDirection(moveSpeed);

        moveDirection.X *= (float)delta;
        moveDirection.Y *= (float)delta;

        if(_coyoteFrames > 0)
        {
            var isMoving = Mathf.Abs(moveDirection.X) > 0;
            var isMovingInSameDirection = (Speed.X > 0) == (moveDirection.X > 0); 
            if(isMoving && isMovingInSameDirection && !Input.IsActionPressed("slide")) _superRunFrames += 1;
            else _superRunFrames = 0;
        }
        
        var friction = _coyoteFrames > 0
            ? FloorFriction
            : ((moveDirection != Vector2.Zero) ? AirAccelerationFriction : AirDecelerationFriction);

        var newSpeed = Speed;
        newSpeed.X = Mathf.Lerp(Speed.X, moveDirection.X, friction);
        if (IsOnFloor()) newSpeed.Y = Mathf.Lerp(Speed.Y, moveDirection.Y, friction);

        Speed = newSpeed;
    }
    
    private Vector2 GetMovementDirection(float moveSpeed)
    {
        var inputDirection = 0;
        if (Input.IsActionPressed("move_left")) inputDirection = -1;
        else if (Input.IsActionPressed("move_right")) inputDirection = 1;
        var moveDirection = IsOnFloor()
            ? GetPerpendicularNormal(inputDirection) 
            : new Vector2(inputDirection, 0);
        return moveDirection * moveSpeed;
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

    private void ProcessJump(double delta)
    {
        if (_jumpBuffer > 0)
        {
            if (_coyoteFrames > 0) Jump();
            _jumpBuffer -= 1;
        }
        if (Input.IsActionJustPressed("jump")) _jumpBuffer = 6;

        if (_isMidJump)
        {
            if (Speed.Y > -400f) _isMidJump = false;
            else if (!Input.IsActionPressed("jump"))
            {
                _isMidJump = false;
                Speed /= new Vector2(1, 2);
            }
        }
        
    }

    public void Jump(float jumpHeight = JumpHeight)
    {
        Speed = new Vector2(Speed.X, jumpHeight);
        if(IsOnFloor()) ParticleController.PlayJumpParticles();
        AnimationController.IsJumping = true;
        _glideCharge = MaxGlideCharge;
        _isMidJump = true;
        _coyoteFrames = 0;
        _jumpBuffer = 0;
    }

    private void ProcessGlide(double delta)
    {
        var isGliding = false;
        if(IsOnFloor()) _glideCharge = MaxGlideCharge;
        else if (Input.IsActionPressed("glide") && _glideCharge > 0 && Speed.Y > 0)
        {
            Speed = new Vector2(Speed.X, 20);
            _glideCharge -= 1;
            isGliding = true;
        }

        AnimationController.IsGliding = isGliding;
    }

    private void ProcessSlide(double delta)
    {
        var moveDir = Input.GetAxis("move_left", "move_right");
        if (Input.IsActionJustPressed("slide") && IsOnFloor() && moveDir != 0f)
        {
            _slideSpeed = StartingSlideSpeed * moveDir;
        }
        else if (Input.IsActionJustReleased("slide"))
        {
            _slideSpeed = 0;
            Speed = new Vector2(10f, Speed.Y);
        }
        else if (Input.IsActionPressed("slide") && Mathf.Abs(_slideSpeed) > 1)
        {
            GD.Print(_slideSpeed);
            _slideSpeed = Mathf.Lerp(_slideSpeed, 0, SlideDeceleration);
            Speed = new Vector2((float)(_slideSpeed * delta), Speed.Y);
        }
    }
}
