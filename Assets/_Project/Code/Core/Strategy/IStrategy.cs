namespace _Project.Code.Core.Strategy
{
    public interface IStrategy<TContext>
    {
        void Execute(TContext context);
    }
}