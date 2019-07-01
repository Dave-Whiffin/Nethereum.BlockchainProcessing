﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nethereum.BlockchainProcessing.Processing.Logs
{
    public static class ActionToFuncExtensions
    {
        public static Func<IEnumerable<T>, Task> ToFunc<T>(this Action<IEnumerable<T>> action)
        {
            return new Func<IEnumerable<T>, Task>((events) => { action(events); return Task.CompletedTask;});
        }
    }
}
