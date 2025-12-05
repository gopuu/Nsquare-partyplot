namespace ERPAdmin.Models
{
    public class StaffSalary
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public string? SalaryMonth { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public long? StatusId { get; set; }
        public string? PaidDate { get; set; }
    }

    // StaffSalary Models

    public class GetStaffSalaryRequest
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
    }

    public class SaveStaffSalaryRequest
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public string? SalaryMonth { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Deductions { get; set; }
        public long? StatusId { get; set; }
        public string? PaidDate { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteStaffSalaryRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class StaffSalaryModel
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public string? SalaryMonth { get; set; }
        public string? StatusName { get; set; }
        public string? StaffName { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public long? StatusId { get; set; }
        public string? PaidDate { get; set; }
    }

}
