﻿using Nethereum.LogProcessing.Dynamic.Configuration;
using System.Threading.Tasks;
using Xunit;

namespace Nethereum.LogProcessing.Dynamic.Tests.Handlers.Aggregators.Count
{
    public class CountToSubscriptionStateTests : CountTestsBase
    {
        private const string OUTPUT_NAME = "TotalCount";

        protected override IEventAggregatorDto CreateConfiguration()
        {
            return new EventAggregatorDto
            {
                Operation = AggregatorOperation.Count,
                Destination = AggregatorDestination.EventSubscriptionState,
                OutputKey = OUTPUT_NAME
            };
        }

        [Fact]
        public override async Task CreatesAndIncrementsCounter()
        {
            for (var i = 0; i < 3; i++)
            {
                await Aggregator.HandleAsync(DecodedEvent.Empty());
            }

            Assert.Equal((uint)3, EventSubscriptionState.Values[OUTPUT_NAME]);
        }

        [Fact]
        public override async Task IncrementsExistingCounter()
        {
            EventSubscriptionState.Values[OUTPUT_NAME] = 10;

            await Aggregator.HandleAsync(DecodedEvent.Empty());

            Assert.Equal(11, EventSubscriptionState.Values[OUTPUT_NAME]);
        }
    }
}

