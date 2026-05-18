namespace API.Models.DTOs.Auth
{
    public class FOVAuthDto
    {
        public bool LogInStatus { get; set; }
        public string EmpCode { get; set; } = null!;
        public string UnitID { get; set; } = null!;
        public string? EmailAdd { get; set; }
        public string MenuName { get; set; } = null!;
        public string RightLevel { get; set; } = null!;
        public string? IsApprover { get; set; }
        public string? Approver { get; set; }
        public string? ApproverEmail { get; set; }
    }
}
