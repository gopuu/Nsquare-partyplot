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

    //SAVE

    public class SaveStaffMasterRequest : BaseClass
    {
        // ===== Staff Master =====
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? Emp_Code { get; set; }
        public string? Address { get; set; }
        public string? PhoneNo { get; set; }
        public string? Emergency_PhoneNo { get; set; }
        public string? Email { get; set; }
        public long? DesignationId { get; set; }

        public string? AadharCardNo { get; set; }
        public string? JoiningDate { get; set; }
        public string? BirthDate { get; set; }
        public string? AnniversaryDate { get; set; }
        public decimal? Salary { get; set; }

        public string? User_Name { get; set; }
        public string? Password { get; set; }
        public bool IsSuperUser { get; set; }
        public bool IsAutoLogin { get; set; }
        public bool IsActive { get; set; }

        // ===== Photo =====
        public string? Photo { get; set; } // base64

        // ===== Bank =====
        public string? Bank_Name { get; set; }
        public string? Bank_BranchName { get; set; }
        public string? Bank_IFSC { get; set; }
        public string? Bank_AccountNo { get; set; }
        public string? Bank_AccountHolderName { get; set; }
        public string? Bank_UPI_Id { get; set; }

        // ===== Documents =====
        public List<StaffDocumentRequest>? Documents { get; set; }

    }
    public class StaffDocumentRequest
    {
        public long? Id { get; set; }              // 0 for new
        public string? DocumentName { get; set; }  // Aadhaar, PAN, Resume, etc.
        public string? FileBase64 { get; set; }    // base64
    }


    //DELETE
    public class DeleteStaffMasterRequest : BaseClass
    {
        public long Id { get; set; }
    }

    //GET
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
        public string? BirthDate { get; set; }
        public string? AnniversaryDate { get; set; }
        public string? JoiningDate_Format { get; set; }
        public string? BirthDate_Format { get; set; }
        public string? AnniversaryDate_Format { get; set; }
        public decimal Salary { get; set; }
        public string? Photo { get; set; }

        public string? User_Name { get; set; }
        public string? Password { get; set; }
        public bool? IsSuperUser { get; set; }
        public bool? IsAutoLogin { get; set; }
        public bool? IsActive { get; set; }

        public string? Bank_Name { get; set; }
        public string? Bank_BranchName { get; set; }
        public string? Bank_IFSC { get; set; }
        public string? Bank_AccountNo { get; set; }
        public string? Bank_AccountHolderName { get; set; }
        public string? Bank_UPI_Id { get; set; }

        public List<StaffDocumentsModel2>? Documents { get; set; }
    }
    public class StaffDocumentsModel2
    {
        public long Id { get; set; }
        public string? DocumentName { get; set; }
        public string? FilePath { get; set; }
    }


}
