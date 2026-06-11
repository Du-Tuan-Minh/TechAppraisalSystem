namespace Domain.Enums
{
    public enum DocumentStatus
    {
        // GIAI ĐOẠN 1: XÂY DỰNG
        Draft = 0,
        InternalPending = 1,     // Chờ Leader CNL duyệt
        InternalApproved = 2,    // Leader/BGD Trung tâm mình đã OK

        // GIAI ĐOẠN 2: THẨM ĐỊNH ĐA BÊN
        AppraisalPending = 3,    // Đã gửi các bên, chờ BGD các bên assign người
        Appraising = 4,          // Các bên đang thực hiện thẩm định đa cấp
        AdjustmentRequired = 5,  // Có bên báo Not OK, chờ người tạo sửa

        // GIAI ĐOẠN 3: PHÊ DUYỆT & BAN HÀNH
        Signing = 6,             // Đã thống nhất, đang trình ký BGD Viện 
        Issued = 7,              // Đã ban hành chính thức (Kết thúc luồng thành công)

        // GIAI ĐOẠN 4: CẢI TIẾN
        FeedbackReceived = 8,    // Có lỗi thực tế (Chờ đánh giá)
        UnderImprovement = 9,    // Đang tạo version mới để khắc phục

        // TRẠNG THÁI CUỐI
        Rejected = 10,           // Kết thúc luồng thất bại (Bước 8 luồng tổng hợp)
        Archived = 11            // Tài liệu cũ đã hết hiệu lực, được thay thế
    }
}