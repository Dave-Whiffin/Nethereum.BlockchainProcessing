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

public class LogProcessing_AnyContractAnyLogWithCriteria
{
    public async Task AnyContractAnyLogWithCriteria()
    {
        var logs = new List<FilterLog>();

        var web3 = new Web3("https://rinkeby.infura.io/v3/7238211010344719ad14a89db874158c");

        var processor = web3.Processing.Logs.CreateProcessor(
            action: log => logs.Add(log), 
            criteria: log => log.Removed == false);

        //if we need to stop the processor mid execution - call cancel on the token
        var cancellationToken = new CancellationToken();

        //crawl the required block range
        await processor.ExecuteAsync(
            toBlockNumber: new BigInteger(3146690),
            cancellationToken: cancellationToken,
            startAtBlockNumberIfNotProcessed: new BigInteger(3146684));

        Console.WriteLine($"Expected 65 logs. Logs found: {logs.Count}.");
    }

}


