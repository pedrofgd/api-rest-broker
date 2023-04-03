namespace ApiBroker.API.TestHelpers;

public static class RandomDataUtils
{
    public static string SomeString() => Guid.NewGuid().ToString();
    public static Random Random => new();
    public static int SomeInt(int max) => Random.Next(max);

}