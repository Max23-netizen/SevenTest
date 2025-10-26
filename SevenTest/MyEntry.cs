namespace SevenTest;

public struct MyEntry<TKey, TValue>
{
    public TKey Key;

    public TValue Value;
        
    public long HashCode;

    public long Next;
}