namespace Sandbox.Collections.Generic;

public class Map<TKey, TValue> where TKey : IComparable<TKey>
{
    private List<TValue>[] _collection;
    private int _lenght = 0;
    private int _bucketSize = 4;

    public int Lenght { get => _lenght; }

    public Map()
    {
        _collection = new List<TValue>[_bucketSize];
    }

    public void Add(TKey key, TValue value)
    {
        if (_lenght > _bucketSize * 0.75)
        {
            Resize();
        }

        int bucketIndex = GetBucketIndex(key);

        if (_collection[bucketIndex] is null)
        {
            _collection[bucketIndex] = new();
        }

        _collection[bucketIndex].Add(value);

        _lenght++;
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        value = default;

        int bucketIndex = GetBucketIndex(key);

        List<TValue> values = _collection[bucketIndex];

        if (values is null)
        {
            return false;
        }

        if (values.Count > 1)
        {
            Console.WriteLine($"Warning! Collision detected for key = {key}");
        }

        value = values.First();

        return true;
    }

    private void Resize()
    {
        Console.WriteLine("Resizing ...");

        _bucketSize *= 2;

        List<TValue>[] newCollection = new List<TValue>[_bucketSize];

        for (int idx = 0; idx < _collection.Length; idx++)
        {
            newCollection[idx] = _collection[idx];
        }

        _collection = newCollection;
    }

    private int GetBucketIndex(TKey key)
        => Math.Abs(key.GetHashCode() % _bucketSize);
}

public static class Map
{
    public static void Run()
    {
        Map<string, int> map = new();

        foreach (string key in GetKeys())
        {
            map.Add(key, GetValue(key));
        }

        Console.WriteLine($"Length = {map.Lenght}");

        foreach (string key in GetKeys())
        {
            if (map.TryGetValue(key, out int value))
            {
                Console.WriteLine($"key = {key}, value = {value}");
            }
        }
    }

    private static IEnumerable<string> GetKeys()
        => new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };

    private static int GetValue(string key)
        => key switch
        {
            "one" => 1,
            "two" => 2,
            "three" => 3,
            "four" => 4,
            "five" => 5,
            "six" => 6,
            "seven" => 7,
            "eight" => 8,
            "nine" => 9,
            "ten" => 10,
            _ => -1,
        };
}
