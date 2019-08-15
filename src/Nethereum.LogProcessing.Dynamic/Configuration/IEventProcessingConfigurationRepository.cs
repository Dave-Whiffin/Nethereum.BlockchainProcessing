﻿namespace Nethereum.LogProcessing.Dynamic.Configuration
{
    public interface IEventProcessingConfigurationRepository
    {
        ISubscriberRepository Subscribers { get; }
        ISubscriberStorageRepository SubscriberStorage { get; }
        ISubscriberContractRepository SubscriberContracts { get; }
        ISubscriberQueueRepository SubscriberQueues { get; }
        ISubscriberSearchIndexRepository SubscriberSearchIndexes { get; }
        IEventSubscriptionStateRepository EventSubscriptionStates { get;}
        IEventContractQueryConfigurationRepository EventContractQueries { get;}
        IEventRuleRepository EventRules { get;}
        IEventAggregatorRepository EventAggregators { get;}
        IEventSubscriptionAddressRepository EventSubscriptionAddresses { get;}
        IEventSubscriptionRepository EventSubscriptions { get;}
        IEventHandlerRepository EventHandlers { get;}
        IParameterConditionRepository ParameterConditions { get;}
        IContractQueryRepository ContractQueries { get;}
        IContractQueryParameterRepository ContractQueryParameters { get;}
        IEventHandlerHistoryRepository EventHandlerHistoryRepo { get;}
    }
}
