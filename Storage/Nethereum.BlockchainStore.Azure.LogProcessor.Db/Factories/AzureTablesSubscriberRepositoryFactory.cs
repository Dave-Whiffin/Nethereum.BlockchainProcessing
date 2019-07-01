﻿using Nethereum.BlockchainProcessing.Handlers;
using Nethereum.BlockchainProcessing.Processing.Logs.Configuration;
using Nethereum.BlockchainProcessing.Processing.Logs.Handling;
using Nethereum.BlockchainStore.AzureTables.Bootstrap;
using Nethereum.BlockchainStore.Repositories.Handlers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nethereum.BlockchainStore.AzureTables.Factories
{
    public class AzureTablesSubscriberRepositoryFactory : ISubscriberStorageFactory
    {
        Dictionary<string, BlockProcessingCloudTableSetup> _cloudTableSetups = new Dictionary<string, BlockProcessingCloudTableSetup>();
        public AzureTablesSubscriberRepositoryFactory( 
            string azureStorageConnectionString)
        {
            AzureStorageConnectionString = azureStorageConnectionString;
        }

        public string AzureStorageConnectionString { get; }

        public Task<ILogHandler> GetLogRepositoryHandlerAsync(string tablePrefix)
        {
            BlockProcessingCloudTableSetup cloudTableSetup = GetCloudTableSetup(tablePrefix);
            var repo = cloudTableSetup.CreateTransactionLogRepository();
            var handler = new TransactionLogRepositoryHandler(repo);
            return Task.FromResult(handler as ILogHandler);
        }

        public Task<ILogHandler> GetLogRepositoryHandlerAsync(ISubscriberStorageDto config) => GetLogRepositoryHandlerAsync(config.Name);

        private BlockProcessingCloudTableSetup GetCloudTableSetup(string tablePrefix)
        {
            if(!_cloudTableSetups.TryGetValue(tablePrefix, out BlockProcessingCloudTableSetup setup))
            {
                setup = new BlockProcessingCloudTableSetup(AzureStorageConnectionString, tablePrefix);
                _cloudTableSetups.Add(tablePrefix, setup);
            }
            return setup;
        }

        public async Task DeleteTablesAsync()
        {
            foreach(var prefix in _cloudTableSetups.Keys)
            {
                await _cloudTableSetups[prefix].GetTransactionsLogTable().DeleteIfExistsAsync();
            }
        }
    }
}
