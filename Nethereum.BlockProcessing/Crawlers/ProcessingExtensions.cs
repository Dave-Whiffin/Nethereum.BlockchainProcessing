﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.BlockchainProcessing.Handlers;
using Nethereum.BlockchainProcessing.Nethereum.RPC.Eth.DTOs;
using Nethereum.BlockProcessing.Filters;
using Nethereum.BlockProcessing.ValueObjects;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json.Linq;
using Nethereum.Util;

namespace Nethereum.BlockchainProcessing.Processors
{
    public static class ProcessingExtensions
    {
        public static async Task<bool> IgnoreAsync<T>(
            this IEnumerable<IFilter<T>> filters, T item)
        {
            var match = await filters.IsMatchAsync(item).ConfigureAwait(false);
            return !match;
        }

        public static async Task<bool> IsMatchAsync<T>(
            this IEnumerable<IFilter<T>> filters, T item)
        {
            //match everything if we have no filters
            if (filters == null || !filters.Any()) return true;

            foreach (var filter in filters)
            {
                if(await filter.IsMatchAsync(item).ConfigureAwait(false)) 
                    return true;
            }

            return false;
        }
        //TODO: JB mOVE TO TXN EXTENSION
        public static IEnumerable<LogWithReceiptAndTransaction> GetTransactionLogs(this Transaction transaction, TransactionReceipt receipt)
        {
            for (var i = 0; i < receipt.Logs?.Count; i++)
            {
                if (receipt.Logs[i] is JObject log)
                {
                    var typedLog = log.ToObject<FilterLog>();

                    yield return
                        new LogWithReceiptAndTransaction(transaction, receipt, typedLog);
                }
            }
        }
        //TODO: JB mOVE TO TXN EXTENSION
        public static string[] GetAllRelatedAddresses(this Transaction tx, TransactionReceipt receipt)
        {
            if (tx == null)
                return Array.Empty<string>();

            var uniqueAddresses = new UniqueAddressList()
                {tx.From};

            if (tx.To.IsNotAnEmptyAddress()) 
                uniqueAddresses.Add(tx.To);

            if (receipt != null)
            {
                if (receipt.ContractAddress.IsNotAnEmptyAddress())
                    uniqueAddresses.Add(receipt.ContractAddress);

                foreach (var log in tx.GetTransactionLogs(receipt))
                {
                    if (log.Address.IsNotAnEmptyAddress())
                        uniqueAddresses.Add(log.Address);
                }
            }

            return uniqueAddresses.ToArray();

        }

    }
}
