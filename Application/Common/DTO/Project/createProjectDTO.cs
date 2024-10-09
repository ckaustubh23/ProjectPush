namespace VendorBilling.Application.Common.DTO.Project
{
    public class createProjectDTO
    {
        public string? ProjectName { get; set; }
        public string? ProjectCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate {  get; set; }
        public string? Status { get; set; }
        public string? ClientType { get; set; }
        public string? Approval { get; set; }
        public IFormFile? excelFile { get; set; }
    }
}
