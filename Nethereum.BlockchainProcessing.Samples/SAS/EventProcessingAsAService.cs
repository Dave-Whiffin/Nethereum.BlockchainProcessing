﻿using Microsoft.Extensions.Configuration;
using Nethereum.BlockchainProcessing.BlockchainProxy;
using Nethereum.BlockchainProcessing.Processing;
using Nethereum.BlockchainProcessing.Processing.Logs;
using Nethereum.BlockchainProcessing.Processing.Logs.Handling;
using Nethereum.BlockchainProcessing.Processing.Logs.Matching;
using Nethereum.BlockchainProcessing.Queue.Azure.Processing.Logs;
using Nethereum.BlockchainStore.AzureTables.Bootstrap;
using Nethereum.BlockchainStore.AzureTables.Repositories;
using Nethereum.BlockchainStore.Search.Azure;
using Nethereum.Configuration;
using Nethereum.Hex.HexTypes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Nethereum.BlockchainProcessing.Samples.SAS
{
    public class EventProcessingAsAService
    {
        [Fact]
        public async Task WebJobExample()
        {
            var config = LoadConfig();
            string azureStorageConnectionString = GetAzureStorageConnectionString(config);
            string azureSearchKey = config["AzureSearchApiKey"];
            const string AZURE_SEARCH_SERVICE_NAME = "blockchainsearch";

            const long PartitionId = 1;
            const ulong MinimumBlockNumber = 4063361;
            const uint MaxBlocksPerBatch = 10;

            var configRepo = new MockEventProcessingRepository();
            IEventProcessingConfigurationDb configDb = MockEventProcessingDb.Create(configRepo);

            var web3 = new Web3.Web3(TestConfiguration.BlockchainUrls.Infura.Rinkeby);
            var blockchainProxy = new BlockchainProxyService(web3);

            IAzureSearchService searchService = new AzureSearchService(serviceName: AZURE_SEARCH_SERVICE_NAME, searchApiKey: azureSearchKey);
            ISubscriberSearchIndexFactory subscriberSearchIndexFactory = new AzureSubscriberSearchIndexFactory(configDb, searchService);

            var subscriberQueueFactory = new AzureSubscriberQueueFactory(azureStorageConnectionString, configDb);
            var storageCloudSetup = new CloudTableSetup(azureStorageConnectionString, prefix: $"Partition{PartitionId}");

            // load subscribers and event subscriptions
            var eventMatcherFactory = new EventMatcherFactory(configDb);
            var eventHandlerFactory = new EventHandlerFactory(blockchainProxy, configDb, subscriberQueueFactory, subscriberSearchIndexFactory);
            var eventSubscriptionFactory = new EventSubscriptionFactory(configDb, eventMatcherFactory, eventHandlerFactory);
            List<IEventSubscription> eventSubscriptions = await eventSubscriptionFactory.LoadAsync(PartitionId);

            // load service
            var blockProgressRepo = storageCloudSetup.CreateBlockProgressRepository();
            var logProcessor = new BlockchainLogProcessor(blockchainProxy, eventSubscriptions);
            var progressService = new BlockProgressService(blockchainProxy, MinimumBlockNumber, blockProgressRepo);
            var batchProcessorService = new BlockchainBatchProcessorService(logProcessor, progressService, MaxBlocksPerBatch);

            // execute
            BlockRange? rangeProcessed;
            try
            {
                var ctx = new System.Threading.CancellationTokenSource();
                rangeProcessed = await batchProcessorService.ProcessLatestBlocksAsync(ctx.Token);
            }
            finally
            {
                await ClearDown(storageCloudSetup, searchService, subscriberQueueFactory);
            }

            // save event subscription state
            await eventHandlerFactory.SaveStateAsync();

            // assertions
            Assert.NotNull(rangeProcessed);
            Assert.Equal((ulong)11, rangeProcessed.Value.BlockCount);

            var subscriptionState1 = configRepo.GetEventSubscriptionState(eventSubscriptionId: 1); // interested in transfers with contract queries and aggregations
            var subscriptionState2 = configRepo.GetEventSubscriptionState(eventSubscriptionId: 2); // interested in transfers with simple aggregation
            var subscriptionState3 = configRepo.GetEventSubscriptionState(eventSubscriptionId: 3); // interested in any event for a specific address

            Assert.Equal("4009000000002040652615", subscriptionState1.Values["RunningTotalForTransferValue"].ToString());
            Assert.Equal((uint)19, subscriptionState2.Values["CurrentTransferCount"]);

            var txForSpecificAddress = (List<string>)subscriptionState3.Values["AllTransactionHashes"];
            Assert.Equal("0x362bcbc78a5cc6156e8d24d95bee6b8f53d7821083940434d2191feba477ae0e", txForSpecificAddress[0]);
            Assert.Equal("0xe63e9422dedf84d0ce13f9f75ebfd86333ce917b2572925fbdd51b51caf89b77", txForSpecificAddress[1]);

            var blockNumbersForSpecificAddress = (List<HexBigInteger>)subscriptionState3.Values["AllBlockNumbers"];
            Assert.Equal((BigInteger)4063362, blockNumbersForSpecificAddress[0].Value);
            Assert.Equal((BigInteger)4063362, blockNumbersForSpecificAddress[1].Value);

        }

        private async Task ClearDown(CloudTableSetup cloudTableSetup, IAzureSearchService searchService, AzureSubscriberQueueFactory subscriberQueueFactory)
        {
            await searchService.DeleteIndexAsync("subscriber-transfer-indexer");

            foreach(var queue in new []{"subscriber-george", "subscriber-harry", "subscriber-nosey"})
            {
                var qRef = subscriberQueueFactory.CloudQueueClient.GetQueueReference(queue);
                await qRef.DeleteIfExistsAsync();
            }

            await cloudTableSetup.GetCountersTable().DeleteIfExistsAsync();
        }

        private static IConfigurationRoot LoadConfig()
        {
            ConfigurationUtils.SetEnvironment("development");

            //use the command line to set your azure search api key
            //e.g. dotnet user-secrets set "AzureStorageConnectionString" "<put key here>"
            var appConfig = ConfigurationUtils
                .Build(Array.Empty<string>(), userSecretsId: "Nethereum.BlockchainProcessing.Samples");

            return appConfig;
        }

        private static string GetAzureStorageConnectionString(IConfigurationRoot appConfig)
        {
            var azureStorageConnectionString = appConfig["AzureStorageConnectionString"];
            return azureStorageConnectionString;
        }
    }
}