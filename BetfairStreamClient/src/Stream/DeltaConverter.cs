using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace BetfairStreamClient.Stream
{
public class DeltaConverter : JsonConverter<LevelDelta[]>
{
    public override LevelDelta[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("Expected array start.");

        // For absolute peak performance, borrow a temporary buffer from the ArrayPool
        // Betfair streams rarely exceed 20 ladder updates per runner change
        var tempBuffer = ArrayPool<LevelDelta>.Shared.Rent(32);
        int count = 0;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                int level = 0;
                double price = 0;
                double size = 0;
                int elementIndex = 0;

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (elementIndex == 0) level = (int)reader.GetDouble();
                    else if (elementIndex == 1) price = reader.GetDouble();
                    else if (elementIndex == 2) size = reader.GetDouble();
                    elementIndex++;
                }

                if (count < tempBuffer.Length)
                {
                    tempBuffer[count++] = new LevelDelta(level, price, size);
                }
            }
        }

        // Copy out only the elements we actually found into a precisely sized array
        if (count == 0)
        {
            ArrayPool<LevelDelta>.Shared.Return(tempBuffer);
            return Array.Empty<LevelDelta>();
        }

        var result = new LevelDelta[count];
        Array.Copy(tempBuffer, result, count);
        
        // Return the rented array to the pool immediately
        ArrayPool<LevelDelta>.Shared.Return(tempBuffer);
        return result;
    }

    public override void Write(Utf8JsonWriter writer, LevelDelta[] value, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Streaming is read-only.");
    }
}
}