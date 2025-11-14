using UnityEngine;

namespace _Project.Code.Core.Strategy
{
    public abstract class ScriptableStrategy<TContext> : ScriptableObject, IStrategy<TContext>
    {
        public abstract void Execute(TContext context);
    }

    public abstract class ScriptableStrategyWithResult<TContext, TResult> : ScriptableObject, IStrategyWithResult<TContext, TResult>
    {
        public abstract TResult Execute(TContext context);
    }
}