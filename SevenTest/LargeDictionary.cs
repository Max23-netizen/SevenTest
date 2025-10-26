namespace SevenTest;

public class LargeDictionary<TKey, TValue>
{
    private static readonly long[] primes =
    [
        3, 7, 17, 37, 67, 131, 257, 521, 1031, 2053,
        4099, 8209, 16411, 32771, 65537, 131101, 262147,
        524309, 1048583, 2097169, 4194319, 8388617, 16777259,
        33554467, 67108879, 134217757, 268435459, 536870923, 1073741827
    ];

    private const long CHUNK_SIZE = 1_000_000;
    private const double LOAD_FACTOR = 0.8;
    private long[] _buckets;
    private MyEntry<TKey, TValue>[][] _entriesChunks;
    private long _count;


    public LargeDictionary(long capacity = 4)
    {
        var primeCapacity = GetPrime(capacity);
        _buckets = new long[primeCapacity];
        _entriesChunks = new MyEntry<TKey, TValue>[1][];
    }

    public void Add(TKey key, TValue value)
    {
        if (_count >= _buckets.LongLength * LOAD_FACTOR)
            Resize();

        var hashCode = (long)key!.GetHashCode() & 0x7FFFFFFFFFFFFFFF;
        var bucketIndex = hashCode % _buckets.LongLength;
        var existingIndex = _buckets[bucketIndex] - 1;

        while (existingIndex >= 0)
        {
            ref var existingEntry = ref GetEntry(existingIndex);
            if (existingEntry.HashCode == hashCode && Equals(existingEntry.Key, key))
                throw new InvalidOperationException($"Key '{key}' уже существует");
            existingIndex = existingEntry.Next;
        }

        var entryIndex = _count++;
        ref var entry = ref GetEntry(entryIndex);

        entry = new MyEntry<TKey, TValue>
        {
            Key = key,
            Value = value,
            HashCode = hashCode,
            Next = _buckets[bucketIndex] - 1
        };

        _buckets[bucketIndex] = entryIndex + 1;
    }

    public TValue? Find(TKey key)
    {
        var hashCode = (long)key!.GetHashCode() & 0x7FFFFFFFFFFFFFFF;
        var bucketIndex = hashCode % _buckets.LongLength;
        var i = _buckets[bucketIndex] - 1;

        while (i >= 0)
        {
            ref var entry = ref GetEntry(i);
            if (entry.HashCode == hashCode && Equals(entry.Key, key))
                return entry.Value;
            i = entry.Next;
        }

        return default;
    }

    private void Resize()
    {
        var newSize = GetPrime(_buckets.LongLength * 2);
        var newBuckets = new long[newSize];
        var newChunksCount = (_count + CHUNK_SIZE - 1) / CHUNK_SIZE;
        var newEntriesChunks = new MyEntry<TKey, TValue>[newChunksCount][];

        for (long chunk = 0; chunk < _entriesChunks.LongLength; chunk++)
        {
            if (_entriesChunks[chunk] != null)
            {
                newEntriesChunks[chunk] = new MyEntry<TKey, TValue>[_entriesChunks[chunk].LongLength];
                Array.Copy(_entriesChunks[chunk], newEntriesChunks[chunk], _entriesChunks[chunk].LongLength);
            }
        }
        
        for (long i = 0; i < _count; i++)
        {
            ref var entry = ref GetEntry(i, newEntriesChunks);
            var bucketIndex = entry.HashCode % newSize;
            entry.Next = newBuckets[bucketIndex] - 1;
            newBuckets[bucketIndex] = i + 1;
        }

        _buckets = newBuckets;
        _entriesChunks = newEntriesChunks;
    }

    private ref MyEntry<TKey, TValue> GetEntry(long index)
    {
        var chunkIndex = index / CHUNK_SIZE;
        var innerIndex = (int)(index % CHUNK_SIZE);

        if (_entriesChunks.Length <= chunkIndex)
        {
            Array.Resize(ref _entriesChunks, (int)(chunkIndex + 1));
        }

        if (_entriesChunks[chunkIndex] == null)
        {
            _entriesChunks[chunkIndex] = new MyEntry<TKey, TValue>[CHUNK_SIZE];
        }

        return ref _entriesChunks[chunkIndex][innerIndex];
    }

    private ref MyEntry<TKey, TValue> GetEntry(long index, MyEntry<TKey, TValue>[][] chunks)
    {
        var chunkIndex = index / CHUNK_SIZE;
        var innerIndex = (int)(index % CHUNK_SIZE);
        return ref chunks[chunkIndex][innerIndex];
    }

    public TValue this[TKey key]
    {
        get => Find(key);
        set => Add(key, value);
    }
    
    private static long GetPrime(long min)
    {
        foreach (var prime in primes)
        {
            if (prime >= min) return prime;
        }

        for (var i = (min | 1); i < long.MaxValue; i += 2)
        {
            if (IsPrime(i)) return i;
        }

        return min;
    }

    private static bool IsPrime(long value)
    {
        if ((value & 1) == 0) return value == 2;

        var limit = (long)Math.Sqrt(value);

        for (long i = 3; i <= limit; i += 2)
        {
            if (value % i == 0) return false;
        }

        return true;
    }
}