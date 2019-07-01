﻿using Nethereum.Contracts;
using System.Threading.Tasks;
using Nethereum.BlockchainProcessing.Handlers;
using Nethereum.BlockProcessing.ValueObjects;

namespace Nethereum.BlockchainProcessing.InMemory.Console
{
    public class EventPrinter<TEvent>: ITransactionLogHandler<TEvent> where TEvent: new()
    {
        private readonly string _eventName;

        public EventPrinter()
        {
            _eventName = ABITypedRegistry.GetEvent<TEvent>().Name;
        }

        public Task HandleAsync(FilterLogWithReceiptAndTransaction filterLogWithReceiptAndTransactionLog)
        {
            if (!filterLogWithReceiptAndTransactionLog.IsForEvent<TEvent>())
                return Task.CompletedTask;

            var eventValues = filterLogWithReceiptAndTransactionLog.Decode<TEvent>();
            if (eventValues == null) return Task.CompletedTask;

            System.Console.WriteLine($"[EVENT]");
            System.Console.WriteLine($"\t[{_eventName}]");
            foreach (var prop in eventValues?.Event.GetType().GetProperties())
            {
                System.Console.WriteLine($"\t\t[{prop.Name}:{prop.GetValue(eventValues.Event) ?? "null"}]");
            }

            return Task.CompletedTask;
        }
    }
}
