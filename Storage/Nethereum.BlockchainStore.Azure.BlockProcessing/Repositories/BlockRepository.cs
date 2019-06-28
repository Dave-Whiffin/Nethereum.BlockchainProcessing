﻿using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Nethereum.BlockchainStore.AzureTables.Entities;
using Nethereum.BlockchainStore.Entities;
using Nethereum.BlockchainStore.Repositories;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Block = Nethereum.BlockchainStore.AzureTables.Entities.Block;


namespace Nethereum.BlockchainStore.AzureTables.Repositories
{
    public class BlockRepository : AzureTableRepository<Block>, IBlockRepository
    {
        private bool _maxBlockInitialised = false;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
        private readonly CloudTable _countersTable;

        public BlockRepository(CloudTable table) : base(table)
        {
        }

        public async Task UpsertBlockAsync(RPC.Eth.DTOs.Block source)
        {
            await _lock.WaitAsync();
            try
            {
                var blockEntity = MapBlock(source, new Block(source.Number.Value.ToString()));
                await UpsertAsync(blockEntity);
            }
            finally
            {
                _lock.Release();
            }
        }


        public Block MapBlock(RPC.Eth.DTOs.Block blockSource, Block blockOutput)
        {
            blockOutput.SetBlockNumber(blockSource.Number);
            blockOutput.SetDifficulty(blockSource.Difficulty);
            blockOutput.SetGasLimit(blockSource.GasLimit);
            blockOutput.SetGasUsed(blockSource.GasUsed);
            blockOutput.SetSize(blockSource.Size);
            blockOutput.SetTimeStamp(blockSource.Timestamp);
            blockOutput.SetTotalDifficulty(blockSource.TotalDifficulty);
            blockOutput.ExtraData = blockSource.ExtraData ?? string.Empty;
            blockOutput.Hash = blockSource.BlockHash ?? string.Empty;
            blockOutput.ParentHash = blockSource.ParentHash ?? string.Empty;
            blockOutput.Miner = blockSource.Miner ?? string.Empty;
            blockOutput.Nonce = string.IsNullOrEmpty(blockSource.Nonce) ? 0 : (long)new HexBigInteger(blockSource.Nonce).Value;

            blockOutput.TransactionCount = blockSource.TransactionCount();

            return blockOutput;
        }


        public async Task<IBlockView> FindByBlockNumberAsync(HexBigInteger blockNumber)
        {
            var operation = TableOperation.Retrieve<Block>(blockNumber.Value.ToString(), "");
            var results = await Table.ExecuteAsync(operation);
            return results.Result as Block;
        }


    }
}