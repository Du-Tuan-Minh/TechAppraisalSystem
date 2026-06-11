namespace Application.DTOs
{
    public class DocumentVersionDto
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public int VersionNumber { get; set; }
        public Dictionary<string, object> TechnicalSpecsJson { get; set; } = new();
    }

    public class DocumentVersionDetailDto : DocumentVersionDto
    {
        public string? ChangeReason { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? SourceIssueId { get; set; }
    }

    //public class DocumentVersionCreateDto
    //{
    //    [Required(ErrorMessage = "Không được để trống ô này.")]
    //    public Dictionary<string, object> TechnicalSpecsJson { get; set; } = new();

    //    [MaxLength(1000, ErrorMessage = "Ghi chú thay đổi không được vượt quá 1000 ký tự.")]
    //    public string? ChangeReason { get; set; }
    //}
}