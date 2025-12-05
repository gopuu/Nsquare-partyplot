namespace ERPAdmin.Models
{
    public class StaffMaster
    {

        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Emp_Code { get; set; }
        public string? Address { get; set; }
        public string? PhoneNo { get; set; }
        public string? Emergency_PhoneNo { get; set; }
        public string? Email { get; set; }
        public long DesignationId { get; set; }

        public string? AadharCardNo { get; set; }
        public string? JoiningDate { get; set; }
        public decimal Salary { get; set; }

        public string? Photo { get; set; }
        public string? User_Name { get; set; }
        public string? Password { get; set; }
        public bool? IsSuperUser { get; set; }
        public bool? IsAutoLogin { get; set; }
       

        public string? Bank_Name { get; set; }
        public string? Bank_BranchName { get; set; }
        public string? Bank_IFSC { get; set; }
        public string? Bank_AccountNo { get; set; }
        public string? Bank_AccountHolderName { get; set; }
        public string? Bank_UPI_Id { get; set; }
    }


    // StaffMaster Models

    public class GetStaffMasterRequest
    {
        public long Id { get; set; }
    }

    public class SaveStaffMasterRequest
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Emp_Code { get; set; }
        public string? Address { get; set; }
        public string? PhoneNo { get; set; }
        public string? Emergency_PhoneNo { get; set; }
        public string? Email { get; set; }
        public long DesignationId { get; set; }

        public string? AadharCardNo { get; set; }
        public string? JoiningDate { get; set; }
        public decimal Salary { get; set; }

        public string? Photo { get; set; }
        public string? User_Name { get; set; }
        public string? Password { get; set; }
        public bool? IsSuperUser { get; set; }
        public bool? IsAutoLogin { get; set; }
      
        public string? Bank_Name { get; set; }
        public string? Bank_BranchName { get; set; }
        public string? Bank_IFSC { get; set; }
        public string? Bank_AccountNo { get; set; }
        public string? Bank_AccountHolderName { get; set; }
        public string? Bank_UPI_Id { get; set; }

        public long ActionBy { get; set; }
    }

    public class DeleteStaffMasterRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class StaffMasterModel
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Emp_Code { get; set; }
        public string? Address { get; set; }
        public string? PhoneNo { get; set; }
        public string? Emergency_PhoneNo { get; set; }
        public string? Email { get; set; }
        public long DesignationId { get; set; }

        public string? AadharCardNo { get; set; }
        public string? JoiningDate { get; set; }
        public decimal Salary { get; set; }

        public string? Photo { get; set; }
        public string? User_Name { get; set; }
        public string? Password { get; set; }
        public bool? IsSuperUser { get; set; }
        public bool? IsAutoLogin { get; set; }

        public string? Bank_Name { get; set; }
        public string? Bank_BranchName { get; set; }
        public string? Bank_IFSC { get; set; }
        public string? Bank_AccountNo { get; set; }
        public string? Bank_AccountHolderName { get; set; }
        public string? Bank_UPI_Id { get; set; }
    }

}
