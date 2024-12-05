﻿using System.Collections.Generic;

namespace OpenRestClient
{
    internal static class Extencions
    {
        public static void AddOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) { 
            if(dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }
    }
}
