using System.Collections.Generic;

namespace Topica.Helpers;

public static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> GetByBatch<T>(this IEnumerable<T>? source, int batchSize)
    {
        if (source == null)
        {
            yield break;
        }

        using var enumerator = source.GetEnumerator();
        while (true)
        {
            var batch = new List<T>(batchSize);
            var count = 0;

            while (count < batchSize && enumerator.MoveNext())
            {
                batch.Add(enumerator.Current);
                count++;
            }

            if (count > 0)
            {
                yield return batch;
            }
            else
            {
                yield break;
            }
        }
    }
}