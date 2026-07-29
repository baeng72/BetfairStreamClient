namespace BetfairStreamClient.ExchangeStream
{
    public readonly struct LevelPriceSize
    {
        public int Level { get; }
        public double Price { get; }
        public double Size { get; }

        public LevelPriceSize(int level, double price, double size)
        {
            Level = level;
            Price = price;
            Size = size;
        }
       
    }
}