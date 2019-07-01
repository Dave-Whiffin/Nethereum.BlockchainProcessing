﻿using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using System.Threading.Tasks;

namespace Nethereum.BlockchainProcessing.Processing.Logs
{
    public abstract class LogProcessorBase<TEventDto> : ILogProcessor where TEventDto : class, new()
    {

        public virtual Task<bool> IsLogForMeAsync(FilterLog log) => Task.FromResult(log.IsLogForEvent<TEventDto>());

        public abstract Task ProcessLogsAsync(params FilterLog[] eventLogs);
    }
}
