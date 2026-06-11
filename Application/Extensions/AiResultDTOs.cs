namespace Application.Extensions
{
    public class AiResult
    {
        public string RawText { get; set; } = "";
        public string CleanText { get; set; } = "";
        public string CorrectedText { get; set; } = "";
        public List<WordError> Errors { get; set; } = new();
    }
}
