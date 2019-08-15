﻿using Moq;
using Nethereum.LogProcessing.Dynamic.Configuration;
using Nethereum.LogProcessing.Dynamic.Matching;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Nethereum.LogProcessing.Dynamic.Tests
{
    public class EventMatcherFactoryTest
    {
        EventMatcherFactory _factory;
        Mock<IParameterConditionRepository> _mockParameterConditionRepository;
        Mock<IEventSubscriptionAddressRepository> _mockEventSubscriptionAddressRepository;
        Mock<ISubscriberContractRepository> _mockSubscriberContractRepository;

        SubscriberDto _subscriberOneConfig;
        SubscriberContractDto _contractDto;
        EventSubscriptionDto _eventSubscriptionConfig;
        ParameterConditionDto _parameterConditionConfig;
        EventSubscriptionAddressDto _addressesConfig;

        public EventMatcherFactoryTest()
        {
            _mockParameterConditionRepository = new Mock<IParameterConditionRepository>();
            _mockEventSubscriptionAddressRepository = new Mock<IEventSubscriptionAddressRepository>();
            _mockSubscriberContractRepository = new Mock<ISubscriberContractRepository>();

            _factory = new EventMatcherFactory(
                _mockParameterConditionRepository.Object,
                _mockEventSubscriptionAddressRepository.Object,
                _mockSubscriberContractRepository.Object);

            _subscriberOneConfig = new SubscriberDto { Id = 1 };
            _contractDto = new SubscriberContractDto
            {
                Id = 1,
                SubscriberId = _subscriberOneConfig.Id,
                Abi = TestData.Contracts.StandardContract.Abi,
                Name = "Transfer"
            };
            _eventSubscriptionConfig = new EventSubscriptionDto
            {
                Id = 1,
                SubscriberId = _subscriberOneConfig.Id,
                ContractId = _contractDto.Id,
                EventSignatures = new[] { TestData.Contracts.StandardContract.TransferEventSignature }.ToList()
            };
            _addressesConfig = new EventSubscriptionAddressDto
            {
                Id = 1,
                Address = "",
                EventSubscriptionId = _eventSubscriptionConfig.Id
            };
            _parameterConditionConfig = new ParameterConditionDto
            {
                Id = 1,
                EventSubscriptionId = _eventSubscriptionConfig.Id,
                ParameterOrder = 1,
                Operator = ParameterConditionOperator.Equals,
                Value = "xyz"
            };

            _mockSubscriberContractRepository.Setup(d => d.GetAsync(_subscriberOneConfig.Id, _contractDto.Id)).ReturnsAsync(_contractDto);
            _mockEventSubscriptionAddressRepository.Setup(d => d.GetManyAsync(_eventSubscriptionConfig.Id)).ReturnsAsync(new[] { _addressesConfig });
            _mockParameterConditionRepository.Setup(d => d.GetManyAsync(_eventSubscriptionConfig.Id)).ReturnsAsync(new[] { _parameterConditionConfig });
        }

        [Fact]
        public async Task LoadsMatcherFromConfig()
        {
            var eventMatcher = await _factory.LoadAsync(_eventSubscriptionConfig) as EventMatcher;

            Assert.NotNull(eventMatcher);
            Assert.Equal(TestData.Contracts.StandardContract.TransferEventSignature, eventMatcher.Abis.First().Sha3Signature);

            var eventAddressMatcher = eventMatcher.AddressMatcher as EventAddressMatcher;
            Assert.Single(eventAddressMatcher.AddressesToMatch);
            Assert.Contains(_addressesConfig.Address, eventAddressMatcher.AddressesToMatch);

            var parameterMatcher = eventMatcher.ParameterMatcher as EventParameterMatcher;
            Assert.Single(parameterMatcher.ParameterConditions);
            var parameterEquals = parameterMatcher.ParameterConditions.First() as ParameterEquals;
            Assert.Equal(_parameterConditionConfig.ParameterOrder, parameterEquals.ParameterOrder);
            Assert.Equal(_parameterConditionConfig.Value, parameterEquals.ExpectedValue);
        }

        [Fact]
        public async Task CatchAllEventsForContract_LoadsAllContractEventAbis()
        {
            _eventSubscriptionConfig.CatchAllContractEvents = true;
            _eventSubscriptionConfig.EventSignatures = null;

            var eventMatcher = await _factory.LoadAsync(_eventSubscriptionConfig) as EventMatcher;

            Assert.Equal(
                TestData.Contracts.StandardContract.ContractAbi.Events.Select(e => e.Sha3Signature),
                eventMatcher.Abis.Select(e => e.Sha3Signature));
        }

        [Fact]
        public async Task SupportsANullContract()
        {
            _eventSubscriptionConfig.ContractId = null;
            _eventSubscriptionConfig.EventSignatures = null;

            var eventMatcher = await _factory.LoadAsync(_eventSubscriptionConfig) as EventMatcher;

            Assert.Null(eventMatcher.Abis);
        }

    }
}
