﻿using Nethereum.BlockchainProcessing.Handlers;
using System.Threading.Tasks;

namespace Nethereum.BlockchainProcessing.Processing.Logs.Handling.Handlers.Handlers
{
    public class RepositoryHandler: EventHandlerBase, IEventHandler
    {
        public RepositoryHandler(
            IEventSubscription subscription, 
            long id, 
            ILogHandler logHandler) :base(subscription, id)
        {
            LogHandler = logHandler;
        }

        public ILogHandler LogHandler { get; }

        public async Task<bool> HandleAsync(DecodedEvent decodedEvent)
        {
            await LogHandler.HandleAsync(decodedEvent.Log);
            return true;
        }

    }
}