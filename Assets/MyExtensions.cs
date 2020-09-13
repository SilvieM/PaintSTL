using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Assets
{
    public static class MyExtensions
    {
        public static IEnumerable<(T item, int index)> LoopIndex<T>(this IEnumerable<T> self) =>
            self.Select((item, index) => (item, index));
    }
}