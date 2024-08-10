using Godot;
using System;

public partial class PlayerParticleController : Node2D
{
	private Player Player { get; set; }
	private GpuParticles2D JumpLine { get; set; }
	private GpuParticles2D JumpSmokeLeft { get; set; }
	private GpuParticles2D JumpSmokeRight { get; set; }
	private GpuParticles2D RunSmoke { get; set; }

	private Vector2 JumpPosition { get; set; }

	public override void _Ready()
	{
		Player = GetParent<Player>();
        JumpLine = GetNode<GpuParticles2D>("JumpLine");
        JumpSmokeLeft = GetNode<GpuParticles2D>("JumpSmokeLeft");
        JumpSmokeRight = GetNode<GpuParticles2D>("JumpSmokeRight");
        RunSmoke = GetNode<GpuParticles2D>("RunSmoke");
	}

	public void PlayJumpParticles()
	{
		JumpSmokeLeft.Emitting = true;
		JumpSmokeRight.Emitting = true;

		JumpPosition = Player.Position;
        var timer = new Timer
        {
            OneShot = true,
			Autostart = true,
			WaitTime = 0.02,
        };
		AddChild(timer);
		timer.Timeout += () => {
			JumpLine.GlobalPosition = JumpPosition;
			var angle = Mathf.RadToDeg(JumpPosition.AngleToPoint(Player.Position)) + 90;
			angle *= -1;
			JumpLine.ProcessMaterial.Set("angle_min", angle);
			JumpLine.ProcessMaterial.Set("angle_max", angle);
			JumpLine.Emitting = true;
		};
	}

	public void SetRunSmokePlaying(bool playing, float direction)
	{
		RunSmoke.Emitting = playing;
		RunSmoke.ProcessMaterial.Set("direction", new Vector2(direction, -1));
	}
}
