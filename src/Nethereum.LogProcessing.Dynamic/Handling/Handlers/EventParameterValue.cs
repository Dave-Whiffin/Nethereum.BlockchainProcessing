﻿namespace Nethereum.LogProcessing.Dynamic.Handling.Handlers
{
    public class EventParameterValue
    {
        public int Order { get;set;}
        public string Name { get;set;}
        public string AbiType { get; set;}
        public object Value { get;set;}

        public bool Indexed { get;set;}
    }

}
