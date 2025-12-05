using System;
using System.Data;

namespace ERPAdmin.PDF
{
    public static class BookingReportDataProvider
    {
        public static DataTable GetCompanyInfo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CompanyName");
            dt.Columns.Add("Address");
            dt.Columns.Add("LogoPath");

            dt.Rows.Add(
                "Ramayana Restro & Banquets",
                "1st Floor, Crossway Mall, Above Croma, Near Ramchowk, Ghoddod Road, Surat",
                "banquatlogo.png"
            );

            return dt;
        }

        public static DataTable GetBookingData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("BookingRef");
            dt.Columns.Add("ClientName");
            dt.Columns.Add("EventDate");
            dt.Columns.Add("Property");
            dt.Columns.Add("Hall");
            dt.Columns.Add("Slot");
            dt.Columns.Add("Timing");
            dt.Columns.Add("DealAmount");
            dt.Columns.Add("BookingDiscount");
            dt.Columns.Add("PayableAmount");
            dt.Columns.Add("TotalPaid");
            dt.Columns.Add("CashDiscount");
            dt.Columns.Add("OutstandingAmount");

            dt.Rows.Add("84517", "MR. BREHAT SEHGAL", "03-12-2025", "Ramayana Restro & Banquets", "RAMA", "Evening", "07:00 PM - 11:30 PM", "50,050", "0", "50,050", "5,100", "0", "44,950");

            return dt;
        }

        public static DataTable GetBookingItems()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Sr#");
            dt.Columns.Add("Date");
            dt.Columns.Add("Title");
            dt.Columns.Add("Food Package");
            dt.Columns.Add("Rate");
            dt.Columns.Add("Pax OR Per");
            dt.Columns.Add("Package Amount");
            dt.Columns.Add("Chargeable Addon Items");
            dt.Columns.Add("Total");

            dt.Rows.Add("1", "03-Dec-2025", "Food", "PACKAGE - 3", "770", "65", "50,050", "-", "50,050");

            return dt;
        }
    }
}
