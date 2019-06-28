﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nethereum.BlockchainStore.Entities;

namespace Nethereum.BlockchainStore.EFCore.EntityBuilders
{
    public class BlockProgressEntityBuilder : BaseEntityBuilder, IEntityTypeConfiguration<BlockProgress>
    {
        public void Configure(EntityTypeBuilder<BlockProgress> entityBuilder)
        {
            entityBuilder.ToTable("BlockProgress");
            entityBuilder.HasKey(b => b.RowIndex);

            entityBuilder.Property(b => b.LastBlockProcessed).IsAddress().IsRequired();
            entityBuilder.HasIndex(b => b.LastBlockProcessed);
        }
    }
}