﻿using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.BlockchainProcessing.Processor;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

public class LogProcessing_AnyContractManyEventAsync
{
    [Event("Transfer")]
    public class TransferEvent: IEventDTO
    {
        [Parameter("address", "_from", 1, true)]
        public string From { get; set; }

        [Parameter("address", "_to", 2, true)]
        public string To { get; set; }

        [Parameter("uint256", "_value", 3, false)]
        public BigInteger Value { get; set; }
    }

    [Event("Transfer")]
    public class Erc721TransferEvent
    {
        [Parameter("address", "_from", 1, true)]
        public string From { get; set; }

        [Parameter("address", "_to", 2, true)]
        public string To { get; set; }

        [Parameter("uint256", "_value", 3, true)]
        public BigInteger Value { get; set; }
    }
        
    public static async Task Main(string[] args)
    {
        var erc20transferEventLogs = new List<EventLog<TransferEvent>>();
        var erc721TransferEventLogs = new List<EventLog<Erc721TransferEvent>>();

        var web3 = new Web3("https://rinkeby.infura.io/v3/7238211010344719ad14a89db874158c");

        var erc20TransferHandler = new EventLogProcessorHandler<TransferEvent>(
            eventLog => erc20transferEventLogs.Add(eventLog));

        var erc721TransferHandler = new EventLogProcessorHandler<Erc721TransferEvent>(
            eventLog => erc721TransferEventLogs.Add(eventLog)); 

        var processingHandlers = new ProcessorHandler<FilterLog>[] {
            erc20TransferHandler, erc721TransferHandler};

        //create our processor to retrieve transfers
        //restrict the processor to Transfers for a specific contract address
        var processor = web3.Processing.Logs.CreateProcessor(processingHandlers);

        //if we need to stop the processor mid execution - call cancel on the token
        var cancellationToken = new CancellationToken();

        //crawl the required block range
        await processor.ExecuteAsync(
            toBlockNumber: new BigInteger(3146690),
            cancellationToken: cancellationToken,
            startAtBlockNumberIfNotProcessed: new BigInteger(3146684));

        Console.WriteLine($"Expected 13 ERC20 transfers. Logs found: {erc20transferEventLogs.Count}.");
        Console.WriteLine($"Expected 3 ERC721 transfers. Logs found: {erc721TransferEventLogs.Count}.");
    }
}


