namespace BetfairStreamClient.Stream
{
    public readonly struct LevelDelta
{
    public int Level { get; }
    public double Price { get; }
    public double Size { get; }

    public LevelDelta(int level, double price, double size)
    {
        Level = level;
        Price = price;
        Size = size;
    }
}
}