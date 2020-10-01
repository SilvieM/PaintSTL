using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Classes;

namespace Assets
{
    public static class MyExtensions
    {
        public static IEnumerable<(T item, int index)> LoopIndex<T>(this IEnumerable<T> self) =>
            self.Select((item, index) => (item, index));

        public static void AddIfNotExists(this Dictionary<Vector3, Vertex> dictionary, Vector3 key, Vertex value)
        {
            if(!dictionary.ContainsKey(key))
                dictionary[key] = value;
        }
        public static void AddIfNotExists(this Dictionary<(Vector3, Vector3), Edge> dictionary, (Vector3, Vector3) key, Edge value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary[key] = value;
        }

    }
}