﻿using CsvHelper.Configuration;
using Nethereum.BlockchainStore.Entities;
using Nethereum.BlockchainStore.Entities.Mapping;
using Nethereum.BlockchainStore.Repositories;
using Nethereum.Hex.HexTypes;
using System.Threading.Tasks;
using Block = Nethereum.BlockchainStore.Entities.Block;

namespace Nethereum.BlockchainStore.Csv.Repositories
{
    public class BlockRepository : CsvRepositoryBase<Block>, IBlockRepository
    {
        public BlockRepository(string csvFolderpath) : base(csvFolderpath, "Blocks")
        {
        }

        public async Task<IBlockView> FindByBlockNumberAsync(HexBigInteger blockNumber)
        {
            return await FindAsync(b => b.BlockNumber == blockNumber.Value.ToString());
        }

        public async Task UpsertBlockAsync(Nethereum.RPC.Eth.DTOs.Block source)
        {
            var block = new Block();
            block.Map(source);
            block.UpdateRowDates();
            await Write(block).ConfigureAwait(false);
        }

        protected override ClassMap<Block> CreateClassMap()
        {
            return BlockMap.Instance;
        }
    }

    public class BlockMap : ClassMap<Block>
    {
        public static BlockMap Instance = new BlockMap();

        public BlockMap()
        {
            AutoMap();
            Map(b => b.Timestamp).Name("_Timestamp");
        }
    }
}
