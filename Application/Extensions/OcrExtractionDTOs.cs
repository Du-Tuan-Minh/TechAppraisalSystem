namespace Application.Extensions
    {
        public class OcrExtractionResponseDto
        {
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public float Confidence { get; set; }
            public List<int[]> KeyCoordinates { get; set; } = new(); 
            public List<int[]> ValueCoordinates { get; set; } = new();
        }
    }
