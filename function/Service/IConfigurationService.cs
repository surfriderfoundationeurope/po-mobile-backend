﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public interface IConfigurationService
    {
        
        string GetValue(string key);
    }

    public class ConfigurationServiceWellKnownKeys
    {
        public const string JwtTokenSignatureKey = "JwtTokenKey";
    }

    public class EnvironmentConfigurationService : IConfigurationService
    {
        public string GetValue(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
    }

    public class InMemoryConfigurationService : IConfigurationService
    {
        public Dictionary<string, string> Values = new Dictionary<string, string>();

        public void SetValue(string key, string value)
        {
            Values.TryAdd(key, value);
        }

        public string GetValue(string key)
        {
            Values.TryGetValue(key, out string value);
            return value;
        }
    }
}
