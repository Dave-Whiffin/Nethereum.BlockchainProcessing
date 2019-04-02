﻿using Nethereum.BlockchainProcessing.BlockchainProxy;
using System.Threading.Tasks;

namespace Nethereum.BlockchainProcessing.Processing.Logs.Handling
{
    public class GetTransactionEventHandler : IEventHandler
    {
        public GetTransactionEventHandler(IGetTransactionByHash proxy)
        {
            Proxy = proxy;
        }

        public IGetTransactionByHash Proxy { get; }

        public async Task<bool> HandleAsync(DecodedEvent decodedEvent)
        {
            decodedEvent.Transaction = await Proxy.GetTransactionByHash(decodedEvent.Log.TransactionHash);
            return true;
        }
    }
}