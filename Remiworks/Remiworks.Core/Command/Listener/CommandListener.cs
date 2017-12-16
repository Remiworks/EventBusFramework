﻿using System;
using System.Threading.Tasks;
using EnsureThat;
using Remiworks.Core.Command.Listener.Callbacks;

namespace Remiworks.Core.Command.Listener
{
    public class CommandListener : ICommandListener
    {
        private const string KeyContainsWildcardMessage = "Key should not contain wildcards";
        private const string StarWildcard = "*";
        private const string HashtagWildcard = "#";
        
        private readonly IBusProvider _busProvider;
        private readonly ICommandCallbackRegistry _commandCallbackRegistry;
        
        public CommandListener(IBusProvider busProvider, ICommandCallbackRegistry commandCallbackRegistry)
        {
            _busProvider = busProvider;
            _commandCallbackRegistry = commandCallbackRegistry;
        }
        
        public Task SetupCommandListenerAsync<TParam>(string queueName, string key, CommandReceivedCallback<TParam> callback)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));           
            EnsureArg.IsNotNullOrWhiteSpace(key, nameof(key));
            EnsureArg.IsNotNull(callback, nameof(callback));

            if(!IsValidKey(key)) throw new ArgumentException(KeyContainsWildcardMessage, nameof(key));
            
            return SetupCommandListenerAsync(
                queueName, 
                key,
                parameter => callback((TParam) parameter),
                typeof(TParam));
        }

        public Task SetupCommandListenerAsync(string queueName, string key, CommandReceivedCallback callback, Type parameterType)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));           
            EnsureArg.IsNotNullOrWhiteSpace(key, nameof(key));
            EnsureArg.IsNotNull(callback, nameof(callback));
            EnsureArg.IsNotNull(parameterType, nameof(parameterType));
            
            if(!IsValidKey(key)) throw new ArgumentException(KeyContainsWildcardMessage, nameof(key));

            return null;
        }

        private bool IsValidKey(string key)
        {
            return
                !key.Contains(StarWildcard) &&
                !key.Contains(HashtagWildcard);
        }
    }
}