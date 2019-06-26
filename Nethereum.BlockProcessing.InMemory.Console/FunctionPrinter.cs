﻿using Nethereum.ABI.Model;
using Nethereum.Contracts;
using System;
using System.Threading.Tasks;
using Nethereum.BlockchainProcessing.Handlers;
using Nethereum.BlockProcessing.ValueObjects;

namespace Nethereum.BlockchainProcessing.InMemory.Console
{
    public class FunctionPrinter<TFunctionInput>: ITransactionHandler<TFunctionInput> where TFunctionInput : FunctionMessage, new()
    {
        private readonly FunctionABI _functionAbi = ABITypedRegistry.GetFunctionABI<TFunctionInput>();

        public Task HandleContractCreationTransactionAsync(ContractCreationTransaction contractCreationTransaction)
        {
            return Task.CompletedTask;
        }

        public Task HandleTransactionAsync(TransactionWithReceipt txnWithReceipt)
        {
            if(!txnWithReceipt.IsForFunction<TFunctionInput>())
                return Task.CompletedTask;

            var dto = txnWithReceipt.Decode<TFunctionInput>();

            System.Console.WriteLine($"[FUNCTION]");
            System.Console.WriteLine($"\t{_functionAbi.Name ?? "unknown"}");
   
            foreach (var prop in dto.GetType().GetProperties())
            {
                System.Console.WriteLine($"\t\t[{prop.Name}:{prop.GetValue(dto) ?? "null"}]");
            }

            return Task.CompletedTask;
        }
    }
}
