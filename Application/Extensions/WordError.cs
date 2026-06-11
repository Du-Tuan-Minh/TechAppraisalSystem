namespace Application.Extensions
{
    public class WordError
    {
        public string Original { get; set; } = "";
        public string Corrected { get; set; } = "";
        public float Confidence { get; set; }
        public int Position { get; set; }
    }
}
