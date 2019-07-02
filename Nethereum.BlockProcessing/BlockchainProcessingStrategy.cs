﻿using Nethereum.BlockchainProcessing.Processors;
using System.Numerics;
using System.Threading.Tasks;

namespace Nethereum.BlockchainProcessing.Processing
{
    public class BlockchainProcessingStrategy : IBlockchainProcessingStrategy
    {
        protected readonly IBlockCrawler BlockCrawler;

        public BlockchainProcessingStrategy(IBlockCrawler blockCrawler, IBlockProgressRepository blockProgressRepository = null)
        {
            BlockCrawler = blockCrawler;
            BlockProgressRepository = blockProgressRepository ?? new InMemoryBlockchainProgressRepository(null);
        }

        public virtual IWaitStrategy WaitStrategy { get; set; } = new WaitStrategy();

        public virtual uint MaxRetries { get; set; } = 3;
        public virtual ulong MinimumBlockNumber { get; set; } = 0;
        public virtual uint MinimumBlockConfirmations { get; set; } = 0;
        public IBlockProgressRepository BlockProgressRepository { get; }

        public virtual Task FillContractCacheAsync() { return Task.CompletedTask; }
        public virtual Task<BigInteger?> GetLastBlockProcessedAsync() => BlockProgressRepository.GetLastBlockNumberProcessedAsync();
        
        public virtual Task PauseFollowingAnError(uint retryNumber) => WaitStrategy.Apply(retryNumber);
        public virtual Task WaitForNextBlock(uint retryNumber) => WaitStrategy.Apply(retryNumber);
        public virtual async Task ProcessBlockAsync(BigInteger blockNumber) 
        {
            await BlockCrawler.ProcessBlockAsync(blockNumber).ConfigureAwait(false); 
            await BlockProgressRepository.UpsertProgressAsync(blockNumber).ConfigureAwait(false);
        }
        public virtual Task<BigInteger> GetMaxBlockNumberAsync() => BlockCrawler.GetMaxBlockNumberAsync();
    }
}