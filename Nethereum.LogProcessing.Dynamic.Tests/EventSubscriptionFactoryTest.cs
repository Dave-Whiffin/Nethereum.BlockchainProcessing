﻿using Moq;
using Nethereum.BlockchainProcessing.Processing.Logs;
using Nethereum.BlockchainProcessing.Processing.Logs.Configuration;
using Nethereum.BlockchainProcessing.Processing.Logs.Handling;
using Nethereum.BlockchainProcessing.Processing.Logs.Matching;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Nethereum.LogProcessing.Tests
{
    public class EventSubscriptionFactoryTest
    {
        const int PARTITION_ID = 99;
        EventSubscriptionFactory _factory;
        Mock<IEventProcessingConfigurationRepository> _mockDb;
        Mock<ISubscriberRepository> _mockSubscriberRepo;
        Mock<IEventSubscriptionRepository> _mockEventSubscriptionRepo;
        Mock<IEventHandlerRepository> _mockEventHandlerRepo;
        Mock<IEventSubscriptionStateRepository> _mockEventSubscriptionStateRepository;

        Mock<IEventHandlerFactory> _mockEventHandlerFactory;
        Mock<IEventMatcherFactory> _mockEventMatcherFactory;
        Mock<IEventHandler> _mockEventHandler;
        Mock<IEventMatcher> _mockEventMatcher;

        SubscriberDto _subscriberOneConfig;
        EventSubscriptionDto _eventSubscriptionConfig;
        EventSubscriptionStateDto _eventSubscriptionStateConfig;
        EventHandlerDto _eventHandlerConfig;


        public EventSubscriptionFactoryTest()
        {
            _mockSubscriberRepo = new Mock<ISubscriberRepository>();
            _mockEventSubscriptionRepo = new Mock<IEventSubscriptionRepository>();
            _mockEventHandlerRepo = new Mock<IEventHandlerRepository>();
            _mockEventSubscriptionStateRepository = new Mock<IEventSubscriptionStateRepository>();

            _mockDb = new Mock<IEventProcessingConfigurationRepository>();
            _mockDb.Setup(db => db.Subscribers).Returns(_mockSubscriberRepo.Object);
            _mockDb.Setup(db => db.EventSubscriptions).Returns(_mockEventSubscriptionRepo.Object);
            _mockDb.Setup(db => db.EventHandlers).Returns(_mockEventHandlerRepo.Object);
            _mockDb.Setup(db => db.EventSubscriptionStates).Returns(_mockEventSubscriptionStateRepository.Object);

            _mockEventHandlerFactory = new Mock<IEventHandlerFactory>();
            _mockEventMatcherFactory = new Mock<IEventMatcherFactory>();
            _factory = new EventSubscriptionFactory(_mockDb.Object, _mockEventMatcherFactory.Object, _mockEventHandlerFactory.Object);
            _mockEventHandler = new Mock<IEventHandler>();
            _mockEventMatcher = new Mock<IEventMatcher>();

            _subscriberOneConfig = new SubscriberDto { Id = 1 };
            _eventSubscriptionConfig = new EventSubscriptionDto
            {
                Id = 1,
                SubscriberId = _subscriberOneConfig.Id
            };
            _eventHandlerConfig = new EventHandlerDto
            {
                Id = 1,
                EventSubscriptionId = _eventSubscriptionConfig.Id,
                HandlerType = EventHandlerType.Queue
            };
            _eventSubscriptionStateConfig = new EventSubscriptionStateDto
            {
                Id = 1,
                EventSubscriptionId = _eventSubscriptionConfig.Id
            };

            _mockSubscriberRepo.Setup(d => d.GetManyAsync(PARTITION_ID)).ReturnsAsync(new[] { _subscriberOneConfig });
            _mockEventSubscriptionRepo.Setup(d => d.GetManyAsync(_subscriberOneConfig.Id)).ReturnsAsync(new[] { _eventSubscriptionConfig });
            _mockEventHandlerRepo.Setup(d => d.GetManyAsync(_eventSubscriptionConfig.Id)).ReturnsAsync(new[] { _eventHandlerConfig });
            _mockEventSubscriptionStateRepository.Setup(d => d.GetAsync(_eventSubscriptionConfig.Id)).ReturnsAsync(_eventSubscriptionStateConfig);

            _mockEventHandlerFactory.Setup(f => f.LoadAsync(It.IsAny<IEventSubscription>(), _eventHandlerConfig)).ReturnsAsync(_mockEventHandler.Object);
            _mockEventMatcherFactory.Setup(f => f.LoadAsync(_eventSubscriptionConfig)).ReturnsAsync(_mockEventMatcher.Object);
        }

        [Fact]
        public async Task LoadsEventSubscriptionsFromConfig()
        {
            var eventSubscriptions = await _factory.LoadAsync(PARTITION_ID);

            Assert.Single(eventSubscriptions);
            Assert.Equal(_subscriberOneConfig.Id, eventSubscriptions[0].SubscriberId);
            Assert.Equal(_eventSubscriptionConfig.Id, eventSubscriptions[0].Id);

            var eventSubscription = eventSubscriptions[0] as EventSubscription;
            Assert.NotNull(eventSubscription);

            Assert.Same(eventSubscription.Matcher, _mockEventMatcher.Object);
            Assert.Same(_mockEventHandler.Object, eventSubscription.EventHandlers.FirstOrDefault());

            var eventHandlerCoordinator = eventSubscription.HandlerManager as EventHandlerManager;
            Assert.NotNull(eventHandlerCoordinator);
        }

        [Fact]
        public async Task ExcludesDisabledSubscribers()
        {
            _subscriberOneConfig.Disabled = true;
            var eventSubscriptions = await _factory.LoadAsync(PARTITION_ID);
            Assert.Empty(eventSubscriptions);
        }

        [Fact]
        public async Task ExcludesDisabledEventSubscriptions()
        {
            _eventSubscriptionConfig.Disabled = true;
            var eventSubscriptions = await _factory.LoadAsync(PARTITION_ID);
            Assert.Empty(eventSubscriptions);
        }

        [Fact]
        public async Task ExcludesDisabledEventHandlers()
        {
            _eventHandlerConfig.Disabled = true;
            var eventSubscriptions = await _factory.LoadAsync(PARTITION_ID);

            var eventSubscription = eventSubscriptions[0] as EventSubscription;
            Assert.NotNull(eventSubscription);

            var eventHandlerCoordinator = eventSubscription.HandlerManager as EventHandlerManager;
            Assert.NotNull(eventHandlerCoordinator);
            Assert.Empty(eventSubscription.EventHandlers);
        }

    }
}
