﻿using Nethereum.ABI.Model;

using Nethereum.Contracts;
using Nethereum.LogProcessing.Dynamic.Configuration;
using Nethereum.RPC.Eth.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nethereum.LogProcessing.Dynamic.Handling
{
    public class EventHandlerManager : IEventHandlerManager
    {
        public EventHandlerManager(
            IEventHandlerHistoryRepository eventHandlerHistory = null)
        {
            History = eventHandlerHistory;
        }

        public IEventHandlerHistoryRepository History { get; }

        public async Task HandleAsync<TEventDto>(IEventSubscription subscription, EventABI abi, params FilterLog[] eventLogs) where TEventDto : new()
        {
            foreach (var log in eventLogs)
            {
                if (!TryDecode<TEventDto>(log, abi, out DecodedEvent decodedEvent))
                {
                    continue;
                }

                SetStateValues(subscription, decodedEvent);

                await InvokeHandlers(subscription, decodedEvent).ConfigureAwait(false);
            }
        }

        public async Task HandleAsync(IEventSubscription subscription, EventABI[] abis, params FilterLog[] eventLogs)
        {
            foreach(var log in eventLogs)
            {
                if (!TryDecode(abis, log, out DecodedEvent decodedEvent))
                {
                    continue;
                }

                SetStateValues(subscription, decodedEvent);

                await InvokeHandlers(subscription, decodedEvent).ConfigureAwait(false);
            }
        }

        private async Task<bool> IsDuplicate(IEventHandler handler, DecodedEvent decodedEvent)
        {
            if (History is null) return false;
            
            return await History.ContainsAsync(handler.Id, decodedEvent.Key).ConfigureAwait(false);
        }

        private async Task InvokeHandlers(IEventSubscription subscription, DecodedEvent decodedEvent)
        {
            foreach (var handler in subscription.EventHandlers)
            {
                if(await IsDuplicate(handler, decodedEvent).ConfigureAwait(false)) 
                {
                    continue;
                }


                decodedEvent.State["HandlerInvocations"] = 1 + (int)decodedEvent.State["HandlerInvocations"];

                var invokeNextHandler = await handler.HandleAsync(decodedEvent).ConfigureAwait(false);

                if(History != null)
                {
                    await WriteToHistoryAsync(decodedEvent, handler).ConfigureAwait(false);
                }

                if (!invokeNextHandler)
                {
                    break;
                }
            }
        }

        private async Task WriteToHistoryAsync(DecodedEvent decodedEvent, IEventHandler handler)
        {
            var history = new EventHandlerHistoryDto
            {
                EventHandlerId = handler.Id,
                EventSubscriptionId = handler.Subscription?.Id ?? 0,
                EventKey = decodedEvent.Key,
                SubscriberId = handler.Subscription?.SubscriberId ?? 0
            };

            await History.UpsertAsync(history);
        }

        private void SetStateValues(IEventSubscription subscription, DecodedEvent decodedEvent)
        {
            subscription.State.Increment("EventsHandled");
            decodedEvent.State["SubscriberId"] = subscription.SubscriberId;
            decodedEvent.State["EventSubscriptionId"] = subscription.Id;
        }

        private bool TryDecode(EventABI[] abis, FilterLog log, out DecodedEvent decodedEvent)
        {
            decodedEvent = null;

            if(abis == null || abis.Length == 0)
            {
                decodedEvent = log.ToDecodedEvent();
                return true;
            }


            var abi = abis.FirstOrDefault(a => a.IsLogForEvent(log));
            if (abi is null) return false;

            try
            {
                decodedEvent = log.ToDecodedEvent(abi);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryDecode<TEventDto>(FilterLog log, EventABI abi, out DecodedEvent decodedEvent) where TEventDto : new()
        {
            decodedEvent = null;

            if (abi is null) return false;

            try
            {
                decodedEvent = log.ToDecodedEvent<TEventDto>(abi);
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
