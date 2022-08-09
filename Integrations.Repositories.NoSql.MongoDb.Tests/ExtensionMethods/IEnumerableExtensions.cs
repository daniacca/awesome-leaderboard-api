using System;
using System.Collections.Generic;
using System.Linq;

namespace NoSql.MongoDb.Tests.ExtensionMethods;

public static class IEnumerableExtensions
{
    /// <summary>
    /// Iterate over a generic sequence of data by chunks of given size at a time
    /// </summary>
    /// <typeparam name="T">the type of element of the sequence</typeparam>
    /// <param name="sequence">the sequence to iterate</param>
    /// <param name="size">the size of each chunk of every iteration</param>
    /// <returns>a chunk of size element on every iteration</returns>
    public static IEnumerable<IList<T>> ChunksOf<T>(this IEnumerable<T> sequence, int size)
    {
        List<T> chunk = new(size);

        foreach (T element in sequence)
        {
            chunk.Add(element);
            if (chunk.Count == size)
            {
                yield return chunk;
                chunk = new List<T>(size);
            }
        }

        if (chunk.Any())
            yield return chunk;
    }

    private static bool IsIterable(Type type) => type.IsArray || typeof(IEnumerable<>).IsAssignableTo(type);

    private static void Flatten<T>(IEnumerable<object> toBeFlatten, List<T> accumulator)
    {
        foreach(var obj in toBeFlatten)
        {
            if (obj is T[] objArray) accumulator.AddRange(objArray);
            else if (obj is T item) accumulator.Add(item);
            else if (IsIterable(obj.GetType())) 
                Flatten((object[])obj, accumulator);
        }
    }
    
    /// <summary>
    /// Flatten a generic IEnumerable of object to a single array of element
    /// </summary>
    /// <typeparam name="TOut">the type of element to be flatted</typeparam>
    /// <param name="sequence">the nested array to be flatted</param>
    /// <returns>a flatted array of type <typeparamref name="TOut"/></returns>
    public static TOut[] Flatten<TOut>(this IEnumerable<object> sequence)
    {
        var flatted = new List<TOut>();
        Flatten(sequence, flatted);
        return flatted.ToArray();
    }

    public static bool IsLast<T>(this IEnumerable<T> items, T item)
    {
        var last = items.LastOrDefault();
        if (last == null)
            return false;
        return item.Equals(last);
    }

    public static bool IsFirst<T>(this IEnumerable<T> items, T item)
    {
        var first = items.FirstOrDefault();
        if (first == null)
            return false;
        return item.Equals(first);
    }

    public static bool IsFirstOrLast<T>(this IEnumerable<T> items, T item)
    {
        return items.IsFirst(item) || items.IsLast(item);
    }
}
