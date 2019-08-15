﻿using Nethereum.LogProcessing.Dynamic.Handling.Handlers;
using System.Threading.Tasks;

namespace Nethereum.LogProcessing.Dynamic.Configuration
{
    public static class ConfigurationExtensions
    {
        public static async Task<ContractQueryConfiguration> LoadContractQueryConfiguration(
            this IContractQueryRepository contractQueryRepository,
            long subscriberId,
            long eventHandlerId,
            ISubscriberOwnedRepository<ISubscriberContractDto> subscriberContractsRepository,
            IContractQueryParameterRepository contractQueryParameterRepository)
        {
            var queryConfig = await contractQueryRepository.GetAsync(eventHandlerId).ConfigureAwait(false);
            var contractConfig = await subscriberContractsRepository.GetAsync(subscriberId, queryConfig.ContractId).ConfigureAwait(false);
            var queryParameters = await contractQueryParameterRepository.GetManyAsync(queryConfig.Id).ConfigureAwait(false);

            return queryConfig.ToContractQueryConfiguration(contractConfig, queryParameters);
        }

        public static ContractQueryConfiguration ToContractQueryConfiguration(
            this IContractQueryDto queryConfig, 
            ISubscriberContractDto contractConfig, 
            IContractQueryParameterDto[] queryParameters)
        {
            return new ContractQueryConfiguration
            {
                Contract = contractConfig,
                Query = queryConfig,
                Parameters = queryParameters
            };
        }
    }
}
