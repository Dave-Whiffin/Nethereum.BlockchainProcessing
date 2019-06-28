﻿using Microsoft.Extensions.Logging;
using Nethereum.BlockchainProcessing.Processing;
using Nethereum.BlockchainStore.AzureTables.Bootstrap;
using Nethereum.BlockchainStore.Processing;
using Microsoft.Configuration.Utils;
using Microsoft.Logging.Utils;

namespace Nethereum.BlockchainStore.AzureTables.Core.Console
{
    class Program
    {
        private const string ConnectionStringKey = "AzureStorageConnectionString";

        public static int Main(string[] args)
        {
            var log = ApplicationLogging.CreateConsoleLogger<Program>().ToILog();

            var appConfig = ConfigurationUtils
                .Build(args, userSecretsId: "Nethereum.BlockchainStore.AzureTables");

            var configuration = BlockchainSourceConfigurationFactory.Get(appConfig);

            var connectionString = appConfig[ConnectionStringKey];

            if (string.IsNullOrEmpty(connectionString))
                throw ConfigurationUtils.CreateKeyNotFoundException(ConnectionStringKey);

            var repositoryFactory = new BlockProcessingCloudTableSetup(connectionString, configuration.Name);

            var blockProgressRepository = new BlockProgressCloudTableSetup(connectionString, configuration.Name)
                .CreateBlockProgressRepository();

            return StorageProcessorConsole.Execute(repositoryFactory, blockProgressRepository, configuration, log: log).Result;
        }
    }
}
