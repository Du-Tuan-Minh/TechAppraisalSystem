using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Departments")]
    public class Department : BaseEntity
    {
        [Required, MaxLength(255)]
        public string NameDepartment { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string CodeDepartment { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid? ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        public virtual Department? Parent { get; set; }

        public virtual ICollection<Department> SubDepartments { get; set; } = new List<Department>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<TechnicalDocument> Documents { get; set; } = new List<TechnicalDocument>();
        public virtual ICollection<FeedbackIssue> HandledIssues { get; set; } = new List<FeedbackIssue>();
        public virtual ICollection<DepartmentInvitation> Invitations { get; set; } = new List<DepartmentInvitation>();
        public virtual ICollection<AppraisalAssignment> AppraisalAssignments { get; set; } = new List<AppraisalAssignment>();
    }
}
