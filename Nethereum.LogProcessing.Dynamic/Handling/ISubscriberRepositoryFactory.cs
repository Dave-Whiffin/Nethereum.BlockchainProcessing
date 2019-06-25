﻿using Nethereum.BlockchainProcessing.Handlers;
using Nethereum.BlockchainProcessing.Processing.Logs.Configuration;
using System.Threading.Tasks;

namespace Nethereum.BlockchainProcessing.Processing.Logs.Handling
{
    public interface ISubscriberStorageFactory
    {
        Task<ILogHandler> GetLogRepositoryHandlerAsync(ISubscriberStorageDto dto);
    }
}
