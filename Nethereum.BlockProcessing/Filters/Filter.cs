﻿using System;
using System.Threading.Tasks;

namespace Nethereum.BlockProcessing.Filters
{
    public class Filter<T>: IFilter<T>
    {
        public static readonly Func<T, bool> AlwaysMatch = (tx) => true;

        public Filter():this(AlwaysMatch){}

        public Filter(Func<T, Task<bool>> condition)
        {
            Condition = condition;
        }

        public Filter(Func<T, bool> condition)
        {
            Condition = item => Task.FromResult(condition(item));
        }

        private Func<T, Task<bool>> Condition { get; }

        public virtual Task<bool> IsMatchAsync(T item)
        {
            return Condition(item);
        }
    }
}