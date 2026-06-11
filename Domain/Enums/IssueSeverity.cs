namespace Domain.Enums
{
    public enum IssueSeverity
    {
        Minor = 1,    // Chính tả, trình bày, hyperlink (Information)
        Moderate = 2, // Thiếu tài liệu sở cứ, thuật ngữ chưa sát (Medium/Warning)
        Serious = 3,  // Sai giá trị chỉ tiêu, sai phương pháp thử (High)
        Critical = 4  // Vi phạm quy chuẩn, mất an toàn, sai part-number hàng mua sẵn
    }
}