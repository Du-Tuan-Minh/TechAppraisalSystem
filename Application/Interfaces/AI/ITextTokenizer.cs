namespace Application.Interfaces.AI
{
    public interface ITextTokenizer
    {
        long[] Encode(string text);
        string Decode(long[] tokens);
    }
}
