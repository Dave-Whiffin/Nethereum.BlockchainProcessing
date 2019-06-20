﻿using Nethereum.BlockchainStore.EF.Hana;
using Nethereum.BlockchainStore.Entities;

namespace Nethereum.BlockchainStore.EF.Tests.Hana
{
    public class TestHanaContextFactory: HanaBlockchainDbContextFactory
    {
        public TestHanaContextFactory():base("BlockchainDbContext_hana", "DEMO")
        {            
        }
    }
}
