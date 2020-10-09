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

        public static Vector3 Average(this System.Collections.Generic.IEnumerable<Vector3> source)
        {
            var x = source.Average(vec => vec.x);
            var y = source.Average(vec => vec.y);
            var z = source.Average(vec => vec.z);
            return new Vector3(x,y,z);
        }
        //public static Vector3 Average<TSource>(this System.Collections.Generic.IEnumerable<TSource> source, Func<TSource, Vector3> selector)
        //{
            
        //    var x = source.Average(vec => vec.x);
        //    var y = source.Average(vec => vec.y);
        //    var z = source.Average(vec => vec.z);
        //    return new Vector3(x, y, z);
        //}

    }
}