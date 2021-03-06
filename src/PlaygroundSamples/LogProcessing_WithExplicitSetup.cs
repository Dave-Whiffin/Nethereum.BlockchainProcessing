﻿using Common.Logging;
using Nethereum.BlockchainProcessing;
using Nethereum.BlockchainProcessing.LogProcessing;
using Nethereum.BlockchainProcessing.Orchestrator;
using Nethereum.BlockchainProcessing.Processor;
using Nethereum.BlockchainProcessing.ProgressRepositories;
using Nethereum.RPC.Eth.Blocks;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Utils;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

public class LogProcessing_WithInDepthSetup
{
    public static async Task Main(string[] args)
    {
        // the number of blocks in a range to process in one batch
        const int DefaultBlocksPerBatch = 10; 

        const int RequestRetryWeight = 0; // see below for retry algorithm

        // ensures the processor does not process blocks considered unconfirmed
        const int MinimumBlockConfirmations = 6;

        // somewhere to put the logs
        var logs = new List<FilterLog>();

        // the web3 object dictates the target network
        // it provides the basis for processing
        var web3 = new Web3("https://rinkeby.infura.io/v3/7238211010344719ad14a89db874158c");

        // only logs matching this filter will be processed
        // in this example we are targetting logs from a specific contract
        var filter = new NewFilterInput(){ Address = new[]{ "0x9edcb9a9c4d34b5d6a082c86cb4f117a1394f831" } };

        // for logs matching the filter apply our handler
        // this handler has an action which be invoked if the criteria matches
        // async overloads for the action and criteria are also available
        // this handler is for any log  
        // for event specific handlers - there is EventLogProcessorHandler<TEventDto>
        var logProcessorHandler = new ProcessorHandler<FilterLog>(
            action: (log) => logs.Add(log),
            criteria: (log) => log.Removed == false);

        // the processor accepts multiple handlers
        // add our single handler to a list
        IEnumerable<ProcessorHandler<FilterLog>> logProcessorHandlers = new ProcessorHandler<FilterLog>[] { logProcessorHandler };

        // the processor accepts an optional ILog
        // replace this with your own Common.Logging.Log implementation
        ILog logger = null;

        /*
         * === Internal Log Request Retry Algorithm ===
         * If requests to retrieve logs from the client fails, subsequent requests will be retried based on this algorithm
         * It's aim is to throttle the number of blocks in the request range and avoid errors
         * The retry weight proportionately restricts the reduction in block range per retry
         * (pseudo code)
         * nextBlockRangeSize = numberOfBlocksPerRequest / (retryRequestNumber + 1) + (_retryWeight * retryRequestNumber);
         */

        // load the components into a LogOrchestrator
        IBlockchainProcessingOrchestrator orchestrator = new LogOrchestrator(
            ethApi: web3.Eth,
            logProcessors: logProcessorHandlers,
            filterInput: filter,
            defaultNumberOfBlocksPerRequest: DefaultBlocksPerBatch,
            retryWeight: RequestRetryWeight);

        // create a progress repository
        // can dictate the starting block (depending on the execution arguments)
        // stores the last block progresssed
        // you can write your own or Nethereum provides multiple implementations
        // https://github.com/Nethereum/Nethereum.BlockchainStorage/
        IBlockProgressRepository progressRepository = new InMemoryBlockchainProgressRepository();

        // this strategy is applied while waiting for block confirmations
        // it will apply a wait to allow the chain to add new blocks
        // the wait duration is dependant on the number of retries
        // feel free to implement your own
        IWaitStrategy waitForBlockConfirmationsStrategy = new WaitStrategy();

        // this retrieves the current block on the chain (the most recent block)
        // it determines the next block to process ensuring it is within the min block confirmations
        // in the scenario where processing is up to date with the chain (i.e. processing very recent blocks)
        // it will apply a wait until the minimum block confirmations is met
        ILastConfirmedBlockNumberService lastConfirmedBlockNumberService =
            new LastConfirmedBlockNumberService(
                web3.Eth.Blocks.GetBlockNumber, waitForBlockConfirmationsStrategy, MinimumBlockConfirmations);

        // instantiate the main processor
        var processor = new BlockchainProcessor(
            orchestrator, progressRepository, lastConfirmedBlockNumberService, logger);

        // if we need to stop the processor mid execution - call cancel on the token source
        var cancellationToken = new CancellationTokenSource();

        //crawl the required block range
        await processor.ExecuteAsync(
            toBlockNumber: new BigInteger(3146690),
            cancellationToken: cancellationToken.Token,
            startAtBlockNumberIfNotProcessed: new BigInteger(3146684));

        Console.WriteLine($"Expected 4 logs. Logs found: {logs.Count}.");
    }
}