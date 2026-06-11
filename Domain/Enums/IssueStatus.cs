namespace Domain.Enums
{
    public enum IssueStatus
    {
        New = 1,            // Mới ghi nhận (Chờ phân công xử lý)
        InProcessing = 2,   // Đang xử lý (Đã phân công cho kỹ sư xây dựng)
        Adjusted = 3,       // Đã điều chỉnh (Kỹ sư đã sửa tài liệu xong - Thay cho Resolved)
        Closed = 4,         // Đã đóng triệt để (Đã theo dõi 30-60 ngày không tái phát - Bước 4.4)
        Rejected = 5       // Không phải lỗi (Sau khi đối soát thấy tài liệu vẫn đúng)
    }
}