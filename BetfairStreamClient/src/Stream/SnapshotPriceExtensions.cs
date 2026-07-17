namespace BetfairStreamClient.Stream
{
    public static class SnapshotPriceExtensions
    {
        public static PriceSize? FindBestPrice(PriceSize[] rentedLadder, int activeCount)
        {
            //If the cache reported no active levels, it's completly empty.
            if(activeCount==0 || rentedLadder == null)return null;

            //Fast sequential loop over the exact active window boundary
            for(int i = 0; i < activeCount; i++)
            {
                //The first index with size > 0 is mathematically best.
                if (rentedLadder[i].Size > 0)
                {
                    return rentedLadder[i];
                }
            }   
            return null;
        }
    }
}