﻿using Nethereum.BlockchainProcessing.Processing.Logs;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Nethereum.LogProcessing.Tests.Processors
{
    public class LogProcessorTests
    {
        [Fact]
        public async Task IsLogForEvent_WhenTheEventMatchesReturnsTrue()
        {
            EventLog<TestData.Contracts.StandardContract.TransferEvent>[] logsProcessed = null;
            var processor = new LogProcessor<TestData.Contracts.StandardContract.TransferEvent>((logs) => { logsProcessed = logs.ToArray(); return Task.CompletedTask; });

            var transferEvent = TestData.Contracts.StandardContract.SampleTransferLog();
            var nonTransferLog = new FilterLog();

            Assert.True(await processor.IsLogForMeAsync(transferEvent));
            Assert.False(await processor.IsLogForMeAsync(nonTransferLog));
        }

        [Fact]
        public async Task Process_InvokesCallback()
        {
            EventLog<TestData.Contracts.StandardContract.TransferEvent>[] logsProcessed = null;
            var processor = new LogProcessor<TestData.Contracts.StandardContract.TransferEvent>((logs) => { logsProcessed = logs.ToArray(); return Task.CompletedTask; });

            var logsToProcess = new[]{
                TestData.Contracts.StandardContract.SampleTransferLog(),
                TestData.Contracts.StandardContract.SampleTransferLog()};

            await processor.ProcessLogsAsync(logsToProcess);

            Assert.Equal(logsToProcess.Length, logsProcessed.Length);
            foreach (var processedLog in logsProcessed)
            {
                Assert.IsType<EventLog<TestData.Contracts.StandardContract.TransferEvent>>(processedLog);
            }
        }

    }
}
