﻿using Microsoft.Extensions.Configuration;
using Nethereum.Microsoft.Configuration.Utils;
using System;

namespace Nethereum.BlockchainProcessing.Samples
{
    public static class TestConfiguration
    {
        public static class BlockchainUrls
        {
            public static class Infura
            {
                public const string Rinkeby = "https://rinkeby.infura.io/v3/7238211010344719ad14a89db874158c";
                public const string Mainnet = "https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c";
            }
        }

        public static string USER_SECRETS_ID = "Nethereum.BlockchainProcessing.Samples";

        public static IConfigurationRoot LoadConfig()
        {
            ConfigurationUtils.SetEnvironmentAsDevelopment();

            //use the command line to set your azure search api key
            //e.g. dotnet user-secrets set "AzureStorageConnectionString" "<put key here>"
            var appConfig = ConfigurationUtils
                .Build(Array.Empty<string>(), userSecretsId: TestConfiguration.USER_SECRETS_ID);

            return appConfig;
        }

    }
}
