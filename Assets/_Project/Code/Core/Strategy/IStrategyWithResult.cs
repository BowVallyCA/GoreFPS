namespace _Project.Code.Core.Strategy
{
    public interface IStrategyWithResult<TContext, TResult>
    {
        TResult Execute(TContext context);
    }
}