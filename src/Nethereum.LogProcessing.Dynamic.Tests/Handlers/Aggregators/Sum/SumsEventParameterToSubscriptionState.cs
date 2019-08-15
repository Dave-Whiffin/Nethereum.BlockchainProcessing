﻿using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Nethereum.LogProcessing.Dynamic.Configuration;
using System.Numerics;
using System.Threading.Tasks;
using Xunit;

namespace Nethereum.LogProcessing.Dynamic.Tests.Handlers.Aggregators.Sum
{
    public class SumsEventParameterToSubscriptionState : SumTestsBase
    {
        private const string OUTPUT_NAME = "RunningTotal";
        private const int EVENT_PARAMETER_NUMBER = 3;

        protected override IEventAggregatorDto CreateConfiguration()
        {
            return new EventAggregatorDto
            {
                Operation = AggregatorOperation.Sum,
                Source = AggregatorSource.EventParameter,
                EventParameterNumber = EVENT_PARAMETER_NUMBER,
                Destination = AggregatorDestination.EventSubscriptionState,
                OutputKey = OUTPUT_NAME
            };
        }

        [Fact]
        public override async Task CreatesAndIncrementsSum()
        {
            for (var i = 0; i < 3; i++)
            {
                var decodedEvent = DecodedEvent.Empty();
                decodedEvent.Event.Add(
                new ParameterOutput
                {
                    Result = (BigInteger)101,
                    Parameter = new Parameter("uint256", EVENT_PARAMETER_NUMBER)
                });
                await Aggregator.HandleAsync(decodedEvent);
            }

            Assert.Equal((BigInteger)303, EventSubscriptionState.Values[OUTPUT_NAME]);
        }

        [Fact]
        public override async Task IncrementsExistingSum()
        {
            EventSubscriptionState.Values[OUTPUT_NAME] = (BigInteger)202;

            var decodedEvent = DecodedEvent.Empty();
            decodedEvent.Event.Add(
                new ParameterOutput
                {
                    Result = (BigInteger)101,
                    Parameter = new Parameter("uint256", EVENT_PARAMETER_NUMBER)
                });

            await Aggregator.HandleAsync(decodedEvent);

            Assert.Equal((BigInteger)303, EventSubscriptionState.Values[OUTPUT_NAME]);
        }
    }
}

