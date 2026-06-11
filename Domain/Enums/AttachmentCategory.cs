namespace Domain.Enums
{
    public enum AttachmentCategory
    {
        SupportingEvidence = 1, // Tài liệu sở cứ 
        AppraisalProof = 2,     // Minh chứng thẩm định 
        IssueEvidence = 3,      // Minh chứng lỗi thực tế
        MainDocumentDraft = 4,  // Các bản thảo (Draft) trong quá trình xây dựng
        FinalSignedDocument = 5, // Bản chính thức đã ký duyệt/đóng dấu 
        ManufacturerDatasheet = 6, // Datasheet của hãng sản xuất
        Other = 99
    }
}