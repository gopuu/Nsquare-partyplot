namespace ERPAdmin.Models
{
    /*
    #region Booking Notes
    //request
    public class GetBookingCalendar_NoteCategoryRequest
    {
        public long Id { get; set; }
    }
    //request
    public class GetBookingCalendar_NoteSubCategoryRequest
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
    }
    //request
    public class GetBookingCalendar_NoteOptionRequest
    {
        public long Id { get; set; }
        public long SubCategoryId { get; set; }
    }

    //response
    public class BookingCalendar_NoteCategoryModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
    //response
    public class BookingCalendar_NoteSubCategoryModel
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Name { get; set; }
        public string InputType { get; set; }
        public int SortOrder { get; set; }
    }
    //response
    public class BookingCalendar_NoteOptionModel
    {
        public long Id { get; set; }
        public long SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string OptionName { get; set; }
        public int SortOrder { get; set; }
    }
    #endregion
    */

    public class CustomerInvoiceReportRequest : BaseClass
    {
        public long BookingId { get; set; }          // CustomerInvoice.Id
        public long? InvoiceNo { get; set; }     
        public string? GSTInvoiceNo { get; set; }     
        public long? ProformaInvoiceNo { get; set; }     
        public bool? WithHeader { get; set; }
    }

    //DownloadBookingEventReport
    public class DownloadBookingEventReportRequest : BaseClass
    {
        public long BookingId { get; set; }
        public bool? WithHeader { get; set; } = true;
    }

    //Booking Receipt Model
    public class BookingReceiptModel
    {
        // ================= RECEIPT =================
        public long ReceiptNo { get; set; }
        public string? PaymentDateRaw { get; set; }   // optional if you want raw date
        public string PaymentDate { get; set; }
        public string PaymentTime { get; set; }

        // ================= BOOKING =================
        public long BookingId { get; set; }
        public string ClientName { get; set; }
        public string EventDate { get; set; }

        public long? BookingSubEventId { get; set; }
        public string BookingSubEventName { get; set; }

        public long? BanquetId { get; set; }
        public string BanquetName { get; set; }

        public string StartTime { get; set; }
        public string StartTimeFull { get; set; }

        public string ShiftFullName { get; set; }

        // ================= PAYMENT =================
        public int? PaymentModeId { get; set; }
        public string PaymentMode { get; set; }

        public decimal PaymentAmount { get; set; }
        public string PaymentAmountFormatted
            => PaymentAmount.ToString("N0");   // ₹ 5,000

        public string PaymentAmountInWords { get; set; }

        public string TransactionId { get; set; }
        public string ChequeNo { get; set; }
        public string ChequeBank { get; set; }
        public string ChequeDate { get; set; }

        // ================= CREATED =================
        public long? BookedByEmpId { get; set; }
        public string BookedByEmpName { get; set; }

        public long? PaymentCreatedById { get; set; }
        public string? PaymentCreatedBy { get; set; }

        public string? BookedOn { get; set; }

        // ================= EXTRA =================
        public string PaymentNote { get; set; }

        // ================= DERIVED (FOR PDF) =================
        public string EventDetailsLine =>
            $"Event Date : {EventDate}    Event : {BookingSubEventName}";
        public string BanquetDetailsLine =>
            $"{BanquetName} at {StartTimeFull} ({ShiftFullName})";
    }

    public class DownloadBookingReceiptRequest : BaseClass
    {
        public long? PaymentId { get; set; }
        public long? BookingId { get; set; }
        public bool? WithHeader { get; set; } = true;
    }


    //Reschedule
    public class SaveBookingCalendarReschedule : BaseClass
    {
        public long BookingId { get; set; }

        public long? BanquetId { get; set; }
        public long? SlotId { get; set; }

        public string? FromDate { get; set; }     // yyyy-MM-dd
        public string? TillDate { get; set; }     // yyyy-MM-dd
        public string? StartTime { get; set; }    // HH:mm
        public string? EndTime { get; set; }      // HH:mm

        public string? ShiftCode { get; set; }

    }

    //Get Calendar mock
    public class GetEnquiryCalendarMock_Request
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }
    public class EnquiryCalendarMock_Model
    {
        public string Date { get; set; }
        public string Day { get; set; }
        public int Enquiries { get; set; }
        public List<EnquiryCalendarMock_Customer> EnquiryList { get; set; }
    }

    public class EnquiryCalendarMock_Customer
    {
        public string Name { get; set; }
        public long Pax { get; set; }
        public decimal Amount { get; set; }
    }



    public class GetBookingCalendar
    {
        public long? BookingId { get; set; } = 0;
        public long? BanquetId { get; set; } = 0;
        public string? EventDate { get; set; } = null;
        public long? YearId { get; set; } = 0;
        public long? MonthId { get; set; } = 0;
    }

    public class DeleteBookingCalendarRequest : BaseClass
    {
        public long? BookingId { get; set; }
    }
    public class DeleteBookingCalendarUploadRequest : BaseClass
    {
        public long? BookingUploadId { get; set; }
    }

    //cancel booking
    public class CancelBookingCalendarRequest : BaseClass
    {
        public long? BookingId { get; set; }
        public string? CancelReason { get; set; }
    }

    //SAVE
    public class SaveBookingCalendar : BaseClass
    {
        public long? BookingId { get; set; }
        public string? EventDate { get; set; }
        public string? ShiftCode { get; set; }
        public bool? GSTAccepted { get; set; }

        public long? BanquetId { get; set; }
        public string? FromDate { get; set; }
        public string? TillDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }

        public long? Client_EnquiryId { get; set; } = 0;
        public long? ClientMaster_Id { get; set; } = 0;
        public string? Client_Name { get; set; }
        public string? Client_Address { get; set; }
        public string? Client_Email { get; set; }
        public string? Client_Contact { get; set; }
        public string? Client_GSTIN { get; set; }
        public long? Client_StateId { get; set; }
        public string? Client_DOB { get; set; } = null;
        public string? Client_AnniversaryDate { get; set; } = null;

        public long? Booking_SubEventId { get; set; }
        public long? Booking_PAX { get; set; }
        public decimal? Booking_HallRent { get; set; }
        public decimal? Booking_GST { get; set; }
        public long? Booking_LeadReferenceId { get; set; }

        public long? Booking_MenuType_Id { get; set; }
        public long? Booking_TypeId { get; set; }
        public bool? Booking_IsAgent { get; set; }

        public long? Payment_ModeId { get; set; }
        public long? PaymentDetails_AccountId { get; set; }
        public string? PaymentDetails_TransactionId { get; set; }
        public string? PaymentDetails_ChequeBank { get; set; }
        public string? PaymentDetails_ChequeNo { get; set; }
        public string? PaymentDetails_ChequeDate { get; set; }

        public decimal? Payment_Amount { get; set; }
        public string? Payment_PaidTime { get; set; }
        public string? Payment_DueDate { get; set; }
        public string? Payment_Note { get; set; }

        public decimal? OtherCharges_Decoration_Amount { get; set; }
        public decimal? OtherCharges_Decoration_GST { get; set; }
        public decimal? OtherCharges_RoomCharges_Amount { get; set; }
        public decimal? OtherCharges_RoomCharges_GST { get; set; }
        public decimal? OtherCharges_SoundSystem_Amount { get; set; }
        public decimal? OtherCharges_SoundSystem_GST { get; set; }
        public decimal? OtherCharges_ProjectorScreen_Amount { get; set; }
        public decimal? OtherCharges_ProjectorScreen_GST { get; set; }
        public decimal? OtherCharges_ElectricityBill_Amount { get; set; }
        public decimal? OtherCharges_ElectricityBill_GST { get; set; }

        public decimal? OtherCharges_GeneratorCharge_Amount { get; set; }
        public decimal? OtherCharges_GeneratorCharge_GST { get; set; }

        public decimal? OtherCharges_Amount { get; set; }
        public decimal? OtherCharges_GST { get; set; }

        public string? OtherCharges_BookingNoteForCustomer { get; set; }
        public string? OtherCharges_InternalOfficeNote { get; set; }

        public decimal? PaymentSummary_TaxableAmount { get; set; }
        public decimal? PaymentSummary_CGST { get; set; }
        public decimal? PaymentSummary_SGST { get; set; }
        public decimal? PaymentSummary_IGST { get; set; }
        public decimal? PaymentSummary_Total { get; set; }
        public long? PaymentSummary_BookingDiscount_Type_Id { get; set; }
        public decimal? PaymentSummary_BookingDiscount_Type_Amount { get; set; }
        public decimal? PaymentSummary_BookingDiscount_Total { get; set; }
        public decimal? PaymentSummary_PayableAmount { get; set; }
        public List<FoodPackage>? JSON_FoodPackage { get; set; }
        public List<BookingCalendarNoteItem>? JSON_Notes { get; set; }
        public List<BookingCalendarGuestModel>? JSON_Guests { get; set; }
        public List<BookingCalendarUploadModel>? JSON_Uploads { get; set; }
    }
    public class FoodPackage
    {
        public long? Id { get; set; }
        public long? PackageId { get; set; }
        public float PerPlate { get; set; }
        public float PackageCost { get; set; }
        public float AddonsCost { get; set; }
        public float GST { get; set; }
        public float TotalCharges { get; set; }
        public string? FoodPackageTitleName { get; set; }
        public long? Pax { get; set; }
        public string? FoodPackageDateTime { get; set; }

        public List<FoodPackage_Menu>? MenuItems { get; set; } = new();
        public List<FoodPackage_AddOn>? AddOns { get; set; } = new();
    }

    // =====================================================
    // MENU - 3 LAYERS
    // Layer 1 = Header
    // Layer 2 = Item
    // Layer 3 = Child
    // =====================================================
    public class FoodPackage_Menu
    {
        public long? MenuHeader_Id { get; set; }
        public string? MenuHeader_Name { get; set; }
        public long? Inclusion_Id { get; set; }

        public List<FoodPackage_MenuItem>? Items { get; set; } = new();
    }
    public class FoodPackage_MenuItem
    {
        public long? MenuItem_Id { get; set; }
        public string? MenuItem_Name { get; set; }
        public string? MenuItem_Name_NewAdded { get; set; }

        public bool? IsChecked { get; set; }
        public bool? IsParent { get; set; }

        public int? MenuLayer { get; set; }

        public long? MenuParentItem_Id { get; set; }

        public List<FoodPackage_MenuChild>? Children { get; set; } = new();
    }
    public class FoodPackage_MenuChild
    {
        public long? MenuItem_Id { get; set; }
        public string? MenuItem_Name { get; set; }
        public string? MenuItem_Name_NewAdded { get; set; }

        public bool? IsChecked { get; set; }
        public bool? IsParent { get; set; }

        public int? MenuLayer { get; set; }

        public long? MenuParentItem_Id { get; set; }
    }


    // =====================================================
    // ADD-ONS - 3 LAYERS
    // Layer 1 = Header
    // Layer 2 = Item
    // Layer 3 = Child
    // =====================================================
    public class FoodPackage_AddOn
    {
        public long? AddOn_Header_Id { get; set; }
        public string? AddOn_Header_Name { get; set; }

        public List<FoodPackage_AddOnItem>? Items { get; set; } = new();
    }
    public class FoodPackage_AddOnItem
    {
        public long? AddOn_Item_Id { get; set; }
        public string? AddOn_Item_Name { get; set; }

        public long? AddOn_Parent_Item_Id { get; set; }
        public string? AddOn_Parent_Item_Name { get; set; }

        public int? AddOn_Layer { get; set; }

        public float? AddOn_Rate { get; set; }
        public long? AddOn_Pax { get; set; }
        public float? AddOn_Total { get; set; }

        public bool? IsParent { get; set; }

        public List<FoodPackage_AddOnChild>? Children { get; set; } = new();
    }
    public class FoodPackage_AddOnChild
    {
        public long? AddOn_Item_Id { get; set; }
        public string? AddOn_Item_Name { get; set; }

        public long? AddOn_Parent_Item_Id { get; set; }
        public string? AddOn_Parent_Item_Name { get; set; }

        public int? AddOn_Layer { get; set; }

        public float? AddOn_Rate { get; set; }
        public long? AddOn_Pax { get; set; }
        public float? AddOn_Total { get; set; }

        public bool? IsParent { get; set; }
    }

    public class BookingCalendarNoteItem
    {
        public long? CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public string? Value { get; set; }
        public string? Remarks { get; set; }
        public List<BookingCalendarNoteOption>? Options { get; set; }
    }

    public class BookingCalendarNoteOption
    {
        public long? OptionId { get; set; }
    }

    public class BookingCalendarGuestModel
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? MobileNo { get; set; }
        public string? WhatsappContact { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public long CityId { get; set; }
        public string? Dob { get; set; }
        public string? AnniversaryDate { get; set; }
    }
    public class BookingCalendarUploadModel
    {
        public long? UploadFile_Id { get; set; } = 0;
        public string? UploadFile_Base64 { get; set; }
    }

    //GET MODEL MASTER -NOTES
    public class BookingNoteMasterResponse
    {
        public List<BookingNoteCategoryModel_Master> Categories { get; set; }
    }
    public class BookingNoteCategoryModel_Master
    {
        public long CategoryId { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }

        public List<BookingNoteSubCategoryModel_Master> SubCategories { get; set; }
    }

    public class BookingNoteSubCategoryModel_Master
    {
        public long SubCategoryId { get; set; }
        public long CategoryId { get; set; }
        public string Name { get; set; }
        public string InputType { get; set; }
        public int SortOrder { get; set; }

        public List<BookingNoteOptionModel_Master> Options { get; set; }
    }

    public class BookingNoteOptionModel_Master
    {
        public long SubCategoryOptionId { get; set; }
        public long SubCategoryId { get; set; }
        public string OptionName { get; set; }
        public int SortOrder { get; set; }
    }
    //GET ALL
    public class BookingModel
    {
        public bool? BookingCalendar_IsBlocked { get; set; }
        public long? BookingId { get; set; }
        public string? EventDate { get; set; }
        public string? EventDate_Format { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftFullName { get; set; }
        public long? SlotId { get; set; }
        public bool GSTAccepted { get; set; }
        public long? BanquetId { get; set; }
        public string? BanquetName { get; set; }
        public string? BanquetPropertyName { get; set; }
        public string? FromDate { get; set; }
        public string? FromDate_Full { get; set; }
        public string? FromDate_Short { get; set; }
        public string? TillDate { get; set; }
        public string? TillDate_Full { get; set; }
        public string? TillDate_Short { get; set; }
        public string? StartTime { get; set; }
        public string? StartTime_Full { get; set; }
        public string? EndTime { get; set; }
        public string? EndTime_Full { get; set; }
        public long? Client_EnquiryId { get; set; } = 0;
        public long? ClientMaster_Id { get; set; } = 0;
        public string? Client_Name { get; set; }
        public string? Client_Address { get; set; }
        public string? Client_Email { get; set; }
        public string? Client_Contact { get; set; }
        public string? Client_GSTIN { get; set; }
        public long? Client_StateId { get; set; }
        public string? Client_DOB { get; set; }
        public string? Client_DOB2 { get; set; }
        public string? Client_AnniversaryDate { get; set; }
        public string? Client_AnniversaryDate2 { get; set; }
        public long? Booking_SubEventId { get; set; }
        public string? Booking_SubEventName { get; set; }
        public int Booking_PAX { get; set; }
        public decimal Booking_HallRent { get; set; }
        public decimal Booking_GST { get; set; }
        public long? Booking_LeadReferenceId { get; set; }
        public long? Booking_MenuType_Id { get; set; }
        public long? Booking_TypeId { get; set; }
        public long? Booking_EventId { get; set; }
        public string? Booking_EventName { get; set; }
        public bool Booking_IsAgent { get; set; }

        public long? Payment_ModeId { get; set; }
        public long? PaymentDetails_AccountId { get; set; }
        public string? PaymentDetails_TransactionId { get; set; }
        public string? PaymentDetails_ChequeBank { get; set; }
        public string? PaymentDetails_ChequeNo { get; set; }
        public string? PaymentDetails_ChequeDate { get; set; }

        public decimal Payment_Amount { get; set; }
        public string? Payment_PaidTime { get; set; }
        public string? Payment_DueDate { get; set; }
        public string? Payment_Note { get; set; }

        public decimal OtherCharges_Decoration_Amount { get; set; }
        public decimal OtherCharges_Decoration_GST { get; set; }
        public decimal OtherCharges_RoomCharges_Amount { get; set; }
        public decimal OtherCharges_RoomCharges_GST { get; set; }
        public decimal OtherCharges_SoundSystem_Amount { get; set; }
        public decimal OtherCharges_SoundSystem_GST { get; set; }
        public decimal OtherCharges_ProjectorScreen_Amount { get; set; }
        public decimal OtherCharges_ProjectorScreen_GST { get; set; }
        public decimal OtherCharges_ElectricityBill_Amount { get; set; }
        public decimal OtherCharges_ElectricityBill_GST { get; set; }

        public decimal OtherCharges_GeneratorCharge_Amount { get; set; }
        public decimal OtherCharges_GeneratorCharge_GST { get; set; }

        public decimal OtherCharges_Amount { get; set; }
        public decimal OtherCharges_GST { get; set; }

        public string? ReachedUs { get; set; }
        public string? Reference { get; set; }
        public string? OtherCharges_BookingNoteForCustomer { get; set; }
        public string? OtherCharges_InternalOfficeNote { get; set; }

        public decimal PaymentSummary_TaxableAmount { get; set; }
        public decimal PaymentSummary_CGST { get; set; }
        public decimal PaymentSummary_SGST { get; set; }
        public decimal PaymentSummary_IGST { get; set; }
        public decimal PaymentSummary_Total { get; set; }
        public long? PaymentSummary_BookingDiscount_Type_Id { get; set; }
        public decimal PaymentSummary_BookingDiscount_Type_Amount { get; set; }
        public decimal PaymentSummary_BookingDiscount_Total { get; set; }
        public decimal PaymentSummary_PayableAmount { get; set; }
        public long? Booked_By_EmpId { get; set; }
        public string? Payment_Status { get; set; }
        public string? Booked_By_EmpName { get; set; }
        public string? Booked_On { get; set; }
        public List<GetFoodPackage>? JSON_FoodPackage { get; set; }
        public List<GetNotes>? JSON_Notes { get; set; }
        public long? VendorWorkOrder_Count { get; set; }
        public long? InvoiceNo { get; set; }
        public long? ProformaInvoiceNo { get; set; }
        public List<PaymentTrailModel>? PaymentTrailList { get; set; }
        public List<GuestModel>? GuestList { get; set; }
        public List<BookingUploadModel>? UploadsList { get; set; }
    }
    public class PaymentTrailModel
    {
        public long? PaymentId { get; set; }
        public int? ModeId { get; set; }
        public string? ModeName { get; set; }
        public string? DateOfPayment { get; set; }
        public string? DateOfPayment2 { get; set; }
        public string? PaymentTime { get; set; }
        public string? TxnId { get; set; }
        public string? ChequeBank { get; set; }
        public string? ChequeNo { get; set; }
        public string? ChequeDate { get; set; }
        public string? ChequeDate2 { get; set; }
        public decimal? Amount { get; set; }
        public decimal? DueBalance { get; set; }
        public string? NextDueDate { get; set; }
        public string? NextDueDate2 { get; set; }
        public string? PaymentNotes { get; set; }
    }
    public class GetFoodPackage
    {
        public long? Id { get; set; }
        public long? FoodPackageId { get; set; }
        public string? FoodPackageName { get; set; }
        public float PerPlate { get; set; }
        public float PackageCost { get; set; }
        public float Addons { get; set; }
        public float GST { get; set; }
        public float TotalCharges { get; set; }
        public string? FoodPackageTitleName { get; set; }
        public long? Pax { get; set; }
        public string? FoodPackageDateTime { get; set; }

        public List<GetFoodPackage_Menu> MenuItemsList { get; set; } = new();
        public List<GetFoodPackage_AddOn> AddOnsList { get; set; } = new();
    }
    // 3-LAYER MENU
    public class GetFoodPackage_Menu
    {
        public long? MenuHeader_Id { get; set; }
        public string? MenuHeader_Name { get; set; }
        public long? Inclusion_Id { get; set; }

        public List<GetFoodPackage_MenuItem> Items { get; set; } = new();
    }

    public class GetFoodPackage_MenuItem
    {
        public long? MenuItem_Id { get; set; }
        public string? MenuItem_Name { get; set; }
        public string? MenuItem_Name_NewAdded { get; set; }
        public bool? IsChecked { get; set; }
        public long? MenuLayer { get; set; }
        public bool? IsParent { get; set; }

        public List<GetFoodPackage_MenuChildItem> Children { get; set; } = new();
    }

    public class GetFoodPackage_MenuChildItem
    {
        public long? MenuItem_Id { get; set; }
        public string? MenuItem_Name { get; set; }
        public string? MenuItem_Name_NewAdded { get; set; }
        public bool? IsChecked { get; set; }
        public long? MenuParentItem_Id { get; set; }
        public long? MenuLayer { get; set; }
        public bool? IsParent { get; set; }
    }

    // 3-LAYER ADD-ON
    public class GetFoodPackage_AddOn
    {
        public long? AddOn_Header_Id { get; set; }
        public string? AddOn_Header_Name { get; set; }

        public List<GetFoodPackage_AddOnItem> Items { get; set; } = new();
    }

    public class GetFoodPackage_AddOnItem
    {
        public long? AddOn_Item_Id { get; set; }
        public string? AddOn_Item_Name { get; set; }
        public long? AddOn_Parent_Item_Id { get; set; }
        public long? AddOn_Layer { get; set; }

        public float? AddOn_Rate { get; set; }
        public float? AddOn_Pax { get; set; }
        public float? AddOn_Total { get; set; }

        public bool? IsParent { get; set; }

        public List<GetFoodPackage_AddOnChildItem> Children { get; set; } = new();
    }

    public class GetFoodPackage_AddOnChildItem
    {
        public long? AddOn_Item_Id { get; set; }
        public string? AddOn_Item_Name { get; set; }
        public long? AddOn_Parent_Item_Id { get; set; }
        public long? AddOn_Layer { get; set; }

        public float? AddOn_Rate { get; set; }
        public float? AddOn_Pax { get; set; }
        public float? AddOn_Total { get; set; }

        public bool? IsParent { get; set; }
    }

    public class GetNotes
    {
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public long? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }

        public string? Value { get; set; }
        public string? Remarks { get; set; }

        public List<GetNoteOption>? Options { get; set; }
    }
    public class GetNoteOption
    {
        public long? OptionId { get; set; }
        public string? OptionName { get; set; }
    }

    public class BookingUploadModel
    {
        public long? UploadId { get; set; }
        public string? FilePath { get; set; }
    }
    public class GuestModel
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? MobileNo { get; set; }
        public string? WhatsappContact { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public long? CityId { get; set; }
        public string? CityName { get; set; }
        public string? Dob { get; set; }
        public string? Dob2 { get; set; }
        public string? AnniversaryDate { get; set; }
        public string? AnniversaryDate2 { get; set; }
    }


    public class GetPackage_MenuAndFoodItemsListRequest
    {
        public long? Package_Id { get; set; } = null;
    }

    /// PACKAGE MENU--START
    public class FoodItemModel
    {
        public long? Item_Id { get; set; }
        public string? Item_Name { get; set; }
        public bool? IsParent { get; set; }
        public List<FoodChildItemModel> Children { get; set; } = new List<FoodChildItemModel>();
    }

    public class FoodChildItemModel
    {
        public long? Item_Id { get; set; }
        public string? Item_Name { get; set; }
        public long? ParentId { get; set; }
    }

    public class FoodMenuModel
    {
        public long? FoodMenu_Id { get; set; }
        public string? FoodMenu_Name { get; set; }
        public long? Inclusion_Id { get; set; }
        public long? Count_Size { get; set; }
        public List<FoodItemModel> FoodItemsList { get; set; } = new List<FoodItemModel>();
    }
    /// PACKAGE MENU--END


    public class PackageReportRequest : BaseClass
    {
        public long BookingId { get; set; }
        public bool IsWithPrice { get; set; }
        public bool? WithHeader { get; set; } = true;
    }

    public class BookingReportModel
    {
        public long BookingId { get; set; }
        public string? BookingRef { get; set; }
        public string? ClientName { get; set; }
        public string? ContactNumber { get; set; }
        public string? EventDate { get; set; }
        public string? Property { get; set; }
        public int Pax { get; set; }
        public string? Hall { get; set; }
        public string? Slot { get; set; }
        public string? Timing { get; set; }
        public string? BookedOn { get; set; }
        public string? EventType { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyState { get; set; }
        public string? CompanyLogo { get; set; }
        public string? Email { get; set; }
        public string? HowReached { get; set; }
        public string? ReachUs { get; set; }
        public string? Reference { get; set; }
        public string? UpdatedBy { get; set; }
        public string? UpdatedOn { get; set; }
        public string? Comment { get; set; }

        public string? OtherCharges_BookingNoteForCustomer { get; set; }
        public string? OtherCharges_InternalOfficeNote { get; set; }
        public string? Notes_FoodBeverage { get; set; }
        public string? Notes_EventTiming { get; set; }
        public string? Notes_DecorationSetup { get; set; }
        public string? Notes_TechnicalService { get; set; }
        public string? Notes_GuestCustom { get; set; }
        public string? Notes_FinancialInternal { get; set; }


        public string? PrimaryPackage { get; set; }
        public string? RatePerPlate { get; set; }

        public string? Mukhwas { get; set; }
        public string? PackageRate { get; set; }
        public string? PackageAmount { get; set; }
        public string? TotalPaid { get; set; }
        public string? OutstandingAmount { get; set; }
        public string? BookingDiscount { get; set; }
        public string? PayableAmount { get; set; }
        public List<PackageItem>? PackageItems { get; set; }
    }

    public class PackageItem
    {
        public string? Date { get; set; }
        public string? Title { get; set; }
        public string? PackageName { get; set; }
        public decimal Rate { get; set; }
        public decimal Pax { get; set; }
        public decimal Amount { get; set; }
        public string? Addon { get; set; }
        public decimal Total { get; set; }
    }

    public class PackageItem_FunctionProspect
    {
        public string? FoodPackageName { get; set; }
        public string? PerPlate { get; set; }
        public string? PackageCost { get; set; }
        public string? Addons { get; set; }
        public string? GST { get; set; }
        public string? TotalCharges { get; set; }
        public string? FoodPackageTitleName { get; set; }
        public string? Pax { get; set; }
        public string? FoodPackageDateTime { get; set; }
        public List<PackageItem_FunctionProspect_Items>? MenuItemsList { get; set; }
        public List<PackageItem_FunctionProspect_AddOns>? AddOnsList { get; set; }
    }
    public class PackageItem_FunctionProspect_Items
    {
        public string? MenuHeader_Name { get; set; }
        public string? MenuItem_Name { get; set; }
    }
    public class PackageItem_FunctionProspect_AddOns
    {
        public string? AddOn_Header_Name { get; set; }
        public string? AddOn_Item_Name { get; set; }
        public string? AddOn_Pax { get; set; }
    }

}
