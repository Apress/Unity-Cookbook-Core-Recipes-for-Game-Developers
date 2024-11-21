public interface IState
{
    //public abstract bool IsActive { get; set; }

    public void OnStateEnter();
    public void OnStateUpdate(float deltaTime);
    public void OnStateExit();
}
