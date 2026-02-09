namespace ERPAdmin.Models
{
    public class CustomerInvoiceReportRequest : BaseClass
    {
        public long BookingId { get; set; }          // CustomerInvoice.Id
        public long? InvoiceNo { get; set; }     
        public long? ProformaInvoiceNo { get; set; }     
        public bool? WithHeader { get; set; }
    }

    //DownloadBookingEventReport
    public class DownloadBookingEventReportRequest : BaseClass
    {
        public long BookingId { get; set; }
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
        public decimal Amount { get; set; }
    }



    public class GetBookingCalendar
    {
        public long? BookingId { get; set; } = 0;
        public long? BanquetId { get; set; } = 0;
        public string? EventDate { get; set; } = null;
    }

    public class DeleteBookingCalendarRequest : BaseClass
    {
        public long? BookingId { get; set; }
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

        public long? Client_EnquiryId { get; set; }
        public string? Client_Name { get; set; }
        public string? Client_Address { get; set; }
        public string? Client_Email { get; set; }
        public string? Client_Contact { get; set; }
        public string? Client_GSTIN { get; set; }
        public long? Client_StateId { get; set; }

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

        //public string? JSON_FoodPackage { get; set; }
        public List<FoodPackage>? JSON_FoodPackage { get; set; }
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
        public List<FoodPackage_MenuIems>? MenuItems { get; set; }
        public List<FoodPackage_AddOns>? AddOns { get; set; }
    }
    public class FoodPackage_MenuIems
    {
        public long? FoodMenu_Id { get; set; } = null;
        public long? FoodItem_Id { get; set; } = null;
        public bool? IsChecked { get; set; } = null;
    }
    public class FoodPackage_AddOns
    {
        public long? AddOn_Header_Id { get; set; } = null;
        public long? AddOn_Item_Id { get; set; } = null;
        public float? AddOn_Rate { get; set; } = null;
        public float? AddOn_Pax { get; set; } = null;
        public float? AddOn_Total { get; set; } = null;
    }


    //GET ALL
    public class BookingModel
    {
        public long? BookingId { get; set; }
        public string? EventDate { get; set; }
        public string? EventDate_Format { get; set; }
        public string? ShiftCode { get; set; }
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
        public long? Client_EnquiryId { get; set; }
        public string? Client_Name { get; set; }
        public string? Client_Address { get; set; }
        public string? Client_Email { get; set; }
        public string? Client_Contact { get; set; }
        public string? Client_GSTIN { get; set; }
        public long? Client_StateId { get; set; }
        public long? Booking_SubEventId { get; set; }
        public string? Booking_SubEventName { get; set; }
        public int Booking_PAX { get; set; }
        public decimal Booking_HallRent { get; set; }
        public decimal Booking_GST { get; set; }
        public long? Booking_LeadReferenceId { get; set; }
        public long? Booking_MenuType_Id { get; set; }
        public long? Booking_TypeId { get; set; }
        public string? Booking_TypeName { get; set; }
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
        public string? Booked_By_EmpName { get; set; }
        public string? Booked_On { get; set; }
        public List<GetFoodPackage>? JSON_FoodPackage { get; set; }
        public long? VendorWorkOrder_Count { get; set; }
        public long? InvoiceNo { get; set; }
        public long? ProformaInvoiceNo { get; set; }

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
        public List<GetFoodPackage_MenuIems>? MenuItemsList { get; set; }
        public List<GetFoodPackage_AddOns>? AddOnsList { get; set; }
    }
    public class GetFoodPackage_MenuIems
    {
        public long? MenuItem_Id { get; set; } = null;
        public string? MenuItem_Name { get; set; } = null;
        public long? MenuHeader_Id { get; set; } = null;
        public string? MenuHeader_Name { get; set; } = null;
        public bool? MenuItem_IsAdded { get; set; } = null;
    }
    public class GetFoodPackage_AddOns
    {
        public long? AddonCharges_HeaderId { get; set; } = null;
        public string? AddonCharges_HeaderName { get; set; } = null;
        public long? AddonCharges_ItemId { get; set; } = null;
        public string? AddonCharges_ItemName { get; set; } = null;
        public float? Rate { get; set; } = null;
        public float? Pax { get; set; } = null;
        public float? Total { get; set; } = null;
    }




    public class GetPackage_MenuAndFoodItemsListRequest
    {
        public long? Package_Id { get; set; } = null;
    }

    public class FoodItemModel
    {
        public long? Item_Id { get; set; }
        public string? Item_Name { get; set; }
    }

    public class FoodMenuModel
    {
        public long? FoodMenu_Id { get; set; }
        public string? FoodMenu_Name { get; set; }
        public List<FoodItemModel>? FoodItemsList { get; set; }
    }
}
