﻿using System.Threading.Tasks;
using Nethereum.BlockchainProcessing.Processors;
using Nethereum.BlockProcessing.Filters;
using Xunit;

namespace Nethereum.BlockProcessing.Tests
{
    public class FilterTests
    {
        [Fact]
        public async Task MatchesAsync_WithAsyncCondition()
        {
            var filter = new Filter<TestFilterItem>(i => Task.FromResult(i.Value == "target"));
            Assert.True(await filter.IsMatchAsync(new TestFilterItem{Value = "target"}));
            Assert.False(await filter.IsMatchAsync(new TestFilterItem{Value = ""}));
        }

        [Fact]
        public async Task MatchesAsync_WithSyncCondition()
        {
            var filter = new Filter<TestFilterItem>(i => i.Value == "target");
            Assert.True(await filter.IsMatchAsync(new TestFilterItem{Value = "target"}));
            Assert.False(await filter.IsMatchAsync(new TestFilterItem{Value = ""}));
        }
    }
}
