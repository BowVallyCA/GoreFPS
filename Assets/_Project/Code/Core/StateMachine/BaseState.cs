namespace _Project.Code.Core.StateMachine
{
    public abstract class BaseState : IState
    {
        public virtual void Enter()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void FixedUpdate()
        {
        }

        public virtual void Exit()
        {
        }
    }
}