public abstract class PlayerState 
{
    private Player _player;

    public PlayerState(Player player)
    {
        _player = player;
    }

    public abstract void Enter();
    public abstract void Process(double delta);
    public abstract void Exit();
}