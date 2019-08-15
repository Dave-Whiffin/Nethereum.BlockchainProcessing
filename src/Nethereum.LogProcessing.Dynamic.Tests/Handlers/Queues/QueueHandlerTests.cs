﻿using Moq;
using Nethereum.BlockchainProcessing.Queue;
using Nethereum.LogProcessing.Dynamic.Handling.Handlers;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Nethereum.LogProcessing.Dynamic.Tests.Handlers.Queues
{
    public class QueueHandlerTests
    {
        [Fact]
        public async Task TransformsAndSendsToQueue()
        {
            var subscription = new Mock<IEventSubscription>();
            var queue = new Mock<IQueue>();
            var handler = new QueueHandler(subscription.Object, 99, queue.Object);
            var decodedLog = DecodedEvent.Empty();

            EventLogQueueMessage actualQueueMessage = null;
            queue
                .Setup(q => q.AddMessageAsync(It.IsAny<object>()))
                .Callback<object>((msg) => actualQueueMessage = msg as EventLogQueueMessage)
                .Returns(Task.CompletedTask);

            var result = await handler.HandleAsync(decodedLog);

            Assert.True(result);
            Assert.NotNull(actualQueueMessage);
        }

        [Fact]
        public async Task CanUseCustomMessageMapper()
        {
            var subscription = new Mock<IEventSubscription>();
            var queue = new Mock<IQueue>();
            var messageToQueue = new object();
            var customMapper = new QueueMessageMapper(decodedEvent => messageToQueue);
            var handler = new QueueHandler(subscription.Object, 99, queue.Object, customMapper);
            var decodedLog = DecodedEvent.Empty();

            object actualQueueMessage = null;
            queue
                .Setup(q => q.AddMessageAsync(It.IsAny<object>()))
                .Callback<object>((msg) => actualQueueMessage = msg)
                .Returns(Task.CompletedTask);

            var result = await handler.HandleAsync(decodedLog);

            Assert.True(result);
            Assert.Same(messageToQueue, actualQueueMessage);
        }

        [Fact]
        public void TransformDecodedEventToQueueMessage()
        {
            var log = TestData.Contracts.StandardContract.SampleTransferLog();
            var decodedLog = log.ToDecodedEvent(TestData.Contracts.StandardContract.TransferEventAbi);

            decodedLog.Transaction = new RPC.Eth.DTOs.Transaction();
            decodedLog.State["test"] = "test";

            var queueMessage = decodedLog.ToQueueMessage();

            Assert.NotNull(queueMessage);
            Assert.Equal(decodedLog.Key, queueMessage.Key);
            Assert.Equal(decodedLog.State["test"], queueMessage.State["test"]);

            Assert.Equal(decodedLog.Event.Count, queueMessage.ParameterValues.Count);

            foreach (var parameter in decodedLog.Event)
            {
                var copy = queueMessage.ParameterValues.FirstOrDefault(p => p.Order == parameter.Parameter.Order);
                Assert.NotNull(copy);
                Assert.Equal(parameter.Result, copy.Value);
                Assert.Equal(parameter.Parameter.Indexed, copy.Indexed);
                Assert.Equal(parameter.Parameter.Name, copy.Name);
                Assert.Equal(parameter.Parameter.ABIType.Name, copy.AbiType);
            }
        }


    }
}
