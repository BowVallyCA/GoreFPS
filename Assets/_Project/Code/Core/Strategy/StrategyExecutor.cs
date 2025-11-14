using System;
using UnityEngine;

namespace _Project.Code.Core.Strategy
{
    public class StrategyExecutor<TStrategy, TContext> where TStrategy : IStrategy<TContext>
    {
        private TStrategy _currentStrategy;
        
        public TStrategy CurrentStrategy => _currentStrategy;

        public StrategyExecutor(TStrategy initialStrategy)
        {
            SetStrategy(initialStrategy);
        }

        public void SetStrategy(TStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));
                
            _currentStrategy = strategy;
        }

        public void Execute(TContext context)
        {
            if (_currentStrategy == null)
            {
                Debug.LogError("No strategy set!");
                return;
            }
            
            _currentStrategy.Execute(context);
        }
    }

    public class StrategyExecutor<TStrategy, TContext, TResult> where TStrategy : IStrategyWithResult<TContext, TResult>
    {
        private TStrategy _currentStrategy;
        
        public TStrategy CurrentStrategy => _currentStrategy;

        public StrategyExecutor(TStrategy initialStrategy)
        {
            SetStrategy(initialStrategy);
        }

        public void SetStrategy(TStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));
                
            _currentStrategy = strategy;
        }

        public TResult Execute(TContext context)
        {
            if (_currentStrategy == null)
            {
                Debug.LogError("No strategy set!");
                return default;
            }
            
            return _currentStrategy.Execute(context);
        }
    }
}