﻿using Nethereum.BlockchainProcessing.Processing.Logs;
using Nethereum.RPC.Eth.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nethereum.LogProcessing.Tests.Processors
{
    public class CatchAllLogProcessorTests
    {

        [Fact]
        public async Task IsLogForEvent_AlwaysReturnsTrue()
        {
            var processor = new CatchAllFilterLogProcessor((logs) => Task.CompletedTask);
            Assert.True(await processor.IsLogForMeAsync(new FilterLog()));
        }

        [Fact]
        public async Task ProcessLogsAsync_InvokesCallback()
        {
            FilterLog[] logsProcessed = null;
            var processor = new CatchAllFilterLogProcessor((logs) => { logsProcessed = logs.ToArray(); return Task.CompletedTask; });

            var logsToProcess = new[] { new FilterLog(), new FilterLog() };

            await processor.ProcessLogsAsync(logsToProcess);

            Assert.Equal(logsToProcess, logsProcessed);
        }
    }
}
