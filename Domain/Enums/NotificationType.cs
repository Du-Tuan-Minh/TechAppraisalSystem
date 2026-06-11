namespace Domain.Enums
{
    public enum NotificationType
    {
        System = 1,          // Thông báo bảo trì, cập nhật hệ thống từ Admin

        Assignment = 2,      // Có nhiệm vụ mới (Phân công thẩm định, phân công sửa tài liệu)

        AppraisalComment = 3, // Có ý kiến phản hồi mới trong quá trình thẩm định (UC-2.4)

        WorkflowResult = 4,   // Kết quả phê duyệt (Đã ban hành, Bị từ chối, Yêu cầu điều chỉnh)

        DeadlineReminder = 5, // Nhắc nhở quá hạn/sắp hết hạn thẩm định (Module 2.1)

        IssueReported = 6,    // Có lỗi thực tế được báo cáo từ Module 4

        DocumentMention = 7   // Thông báo khi được tag hoặc nhắc tên trong thảo luận
    }
}