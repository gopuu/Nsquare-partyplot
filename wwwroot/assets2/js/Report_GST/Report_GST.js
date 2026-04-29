/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "InvoiceNo", label: "Invoice No" },
    { key: "InvoiceDate", label: "Invoice Date" },
    { key: "Name", label: "Customer Name" },
    { key: "GSTIN", label: "GSTIN" },
    { key: "EventType", label: "Event Type" },
    { key: "SubTotal", label: "Sub Total" },
    { key: "CGST", label: "CGST" },
    { key: "SGST", label: "SGST" },
    { key: "IGST", label: "IGST" },
    { key: "TotalTax", label: "Total GST" },
    { key: "TotalInvoiceAmount", label: "Invoice Amount" },
    { key: "PaymentMode", label: "Payment Mode" },
    { key: "PaidAmount", label: "Paid Amount" },
    { key: "DueAmount", label: "Due Amount" }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "gstSalesPrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadGSTSalesGrid();
    loadColumnCheckboxes();

    /* ── Quick range ── */
    $("#quickRange").on("change", function () {

        let newFilter = $(this).val();

        if (selectedMonthDuration === newFilter) return;

        selectedMonthDuration = newFilter;

        let today = new Date();
        let fromDate, toDate;

        if (selectedMonthDuration === "thismonth") {
            fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
            toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
        }
        else if (selectedMonthDuration === "lastmonth") {
            fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
            toDate = new Date(today.getFullYear(), today.getMonth(), 0);
        }
        else if (selectedMonthDuration === "last7days") { // Last 7 days
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 6);
            toDate = today;
        }
        else if (selectedMonthDuration === "last30days") { // Last 30 days
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 29);
            toDate = today;
        }
        else if (selectedMonthDuration === "yesterday") {
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 1);
            toDate = fromDate;
        }
        else if (selectedMonthDuration === "last12months") {
            fromDate = new Date(today);
            fromDate.setMonth(today.getMonth() - 12);
            toDate = today;
        }
        else if (selectedMonthDuration === "customrange") {
            return;
        }

        $("#fromDate").val(formatDate2(fromDate));
        $("#toDate").val(formatDate2(toDate));

        loadGSTSalesGrid();
    });    

    $("#fromDate, #toDate").on("change", function () {
        // If user manually edits date → switch dropdown to custom
        if ($("#quickRange").val() !== "customrange") {
            $("#quickRange").val("customrange");
            // Optional: update variable also
            selectedMonthDuration = "customrange";
        }
    });

    /* ── Filter panel ── */
    $("#filterPanel").on("click", function (e) { e.stopPropagation(); });

    $(document).on("click", function (e) {
        if ($(e.target).closest(".dropdown, .dropdown-menu, #filterPanel, #btnToggleFilter").length > 0) return;
        $("#filterPanel").removeClass("active");
    });

    /* Date change */
    $("#fromDate, #toDate").on("change", function () {
        loadGSTSalesGrid();
    });

    /* Search fields (live typing with delay) */
    let searchTimer;
    $("#txtSearch, #txtSearchInvoiceNo").on("input", function () {
        clearTimeout(searchTimer);

        searchTimer = setTimeout(function () {
            loadGSTSalesGrid();
        }, 200); // wait 500ms after typing
    });

    $("#btnToggleFilter").on("click", function () {
        $("#filterPanel").toggleClass("active");
    });

    $("#btnCloseFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
    });

    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
        loadGSTSalesGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thisyear");
        selectedMonthDuration = "thisyear";
        setCurrentMonthDates();
        loadGSTSalesGrid();
    });

    /* ── With Header toggle ── */
    const saved = sessionStorage.getItem(HEADER_KEY);
    if (saved !== null)
        document.getElementById("toggleWithHeader").checked = (saved === "true");

    document.getElementById("toggleWithHeader").addEventListener("change", function () {
        sessionStorage.setItem(HEADER_KEY, this.checked);
    });

    /* ── Column selector ── */
    $("#btn-select-columns").on("click", function () {
        loadColumnCheckboxes();
        $("#modalColumns").modal("show");
    });

    $("#btn-reset-columns").on("click", function () {
        $(".column-checkbox").prop("checked", true);
        $("#chk-select-all").prop("checked", true);
    });

    $("#btn-save-columns").on("click", function () {
        selectedColumns = [];
        $(".column-checkbox:checked").each(function () {
            selectedColumns.push($(this).val());
        });
        $("#modalColumns").modal("hide");
        loadGSTSalesGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });

    $(document).on("click", ".btn-view-invoice", function () {
        const bookingId = $(this).data('bookingid');
        const invoiceNo = $(this).data("invoiceno");

        //NEW
        const withHeader = $("#toggleWithHeader").is(":checked");

        const request = {
            BookingId: bookingId,
            InvoiceNo: invoiceNo,
            GSTInvoiceNo: null,
            ProformaInvoiceNo: null,
            WithHeader: withHeader,
            ActionBy: Number(sessionStorage.getItem('userId') || 0)
        };
        generateInvoicePdf(request);
    });
});

/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadGSTSalesGrid() {

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration || null,
        InvoiceNo: $("#txtSearchInvoiceNo").val().trim() || null,
        NameEmailContactNo: $("#txtSearch").val()?.trim() || null
    };

    $.ajax({
        url: "/api/Report_GST/GetGSTSalesGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("GST Grid:", res);

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            const rows = res.data?.rows || [];
            const totals = res.data?.totals || null;
            renderGSTSalesTable(rows, totals);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load GST data")
        }
    });
}


/* ═══════════════════════════════════════════════════
   RENDER TABLE
═══════════════════════════════════════════════════ */
function renderGSTSalesTable(data, totals) {

    if ($.fn.DataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().clear().destroy();
    }

    $("#tableMain table").DataTable({

        data: data,
        responsive: false,
        pageLength: 25,
        scrollX: true,
        order: [[0, "desc"]],

        autoWidth: false,
        paging: false,
        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows
        scrollX: true,            // horizontal scroll if needed

        columns: [
            {
                data: "InvoiceNo",
                title: "Inv.No.",
                className: "text-center"
            },
            {
                data: 'InvoiceDate', // actual date (ISO format from backend: yyyy-MM-dd)
                title: 'Invoice Date',
                name: 'InvoiceDate',
                render: function (data, type, row) {
                    if (type === 'display') {
                        return row.InvoiceDate_Format; // show formatted
                    }
                    return data; // use raw date for sorting
                }
            },

            {
                data: "Name",
                title: "Customer Name"
            },
            {
                data: "GSTIN",
                title: "GSTIN"
            },
            {
                data: "EventType",
                title: "Event Type"
            },
            {
                data: "SubTotal",
                title: "Sub Total",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "CGST",
                title: "CGST",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "SGST",
                title: "SGST",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "IGST",
                title: "IGST",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "TotalTax",
                title: "Total GST",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "TotalInvoiceAmount",
                title: "Invoice Amount",
                className: "text-end text-primary",
                render: d => formatINR2(d)
            },

            {
                data: null,
                title: "Invoice",
                className: "text-center",
                render: function (data, type, row) {
                    if (!data) return "";
                    return `
                        <button class="btn btn-xs btn-view-invoice btn-outline-danger"
                                data-invoiceno="${data.InvoiceNo}" 
                                data-bookingid="${data.BookingId}">
                            <i class="fa fa-download me-1"></i>
                        </button>
                    `;
                }
            },

            {
                data: "PaymentMode",
                title: "Payment Mode"
            },
            {
                data: "PaidAmount",
                title: "Paid Amount",
                className: "text-end text-success",
                render: d => formatINR2(d)
            },
            {
                data: "DueAmount",
                title: "Due Amount",
                className: "text-end text-danger",
                render: d => formatINR2(d)
            }
        ],

        /* ───── Footer Totals ───── */

        footerCallback: function () {

            if (!totals) return;

            const fmt = v => formatINR2(v || 0);
            const api = this.api();

            $(api.column(0).footer()).html("<strong class='fs-9'>TOTAL</strong>");

            $(api.column(5).footer()).html(`<strong class='fs-9'>${fmt(totals.TotalSubTotal)}</strong>`);
            $(api.column(6).footer()).html(`<strong class='fs-9'>${fmt(totals.TotalCGST)}</strong>`);
            $(api.column(7).footer()).html(`<strong class='fs-9'>${fmt(totals.TotalSGST)}</strong>`);
            $(api.column(8).footer()).html(`<strong class='fs-9'>${fmt(totals.TotalIGST)}</strong>`);

            $(api.column(9).footer()).html(`<strong class='fs-9'>${fmt(totals.TotalTax)}</strong>`);
            $(api.column(10).footer()).html(`<strong class='fs-9'>${fmt(totals.TotalInvoiceAmount)}</strong>`);

            $(api.column(13).footer()).html(`<strong class='fs-9'>${fmt(totals.TotalPaidAmount)}</strong>`);
            $(api.column(14).footer()).html(`<strong class='fs-9'>${fmt(totals.TotalDueAmount)}</strong>`);
        },

        language: {
            emptyTable: "No GST data found for the selected period."
        },
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },

    });
}


/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewGSTSalesReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);
    const invoiceNo = $("#txtSearchInvoiceNo").val().trim();

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration || null,
        InvoiceNo: invoiceNo !== "" ? Number(invoiceNo) : null,
        NameEmailContactNo: $("#txtSearch").val().trim() || null,

        WithHeader: withHeader,
        Columns: selectedColumns,

        ActionBy: userId
    };
    console.log("payload:", payload);
    const url = actionName === "downloadExcel"
        ? "/api/Report_GST/DownloadGSTSalesReport_Excel"
        : "/api/Report_GST/DownloadGSTSalesReport_Pdf";

    // Show loading on button
    const $btn = $(".dropdown-item:contains('PDF'), .dropdown-item:contains('Excel')").filter(function () {
        return $(this).attr("onclick")?.includes(actionName);
    });

    showTempLoader("Generating Report...");

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            Swal.close();

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            const ext = actionName === "downloadExcel" ? "xlsx" : "pdf";
            const ts = new Date().toISOString().replace(/[:.-]/g, "");
            const link = document.createElement("a");
            link.href = res.data.FileUrl;
            link.download = `GSTSales_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            Swal.close();
            console.error("Download GST Sales Failed", xhr, status, error);
            Swal.fire("Error", "Failed to generate report. Please try again.", "error");
        }
    });
}

/* ═══════════════════════════════════════════════════
   HELPERS
═══════════════════════════════════════════════════ */
function setCurrentMonthDates() {
    const today = new Date();

    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#fromDate").val(formatDate2(firstDay));
    $("#toDate").val(formatDate2(lastDay));
}
function loadColumnCheckboxes() {
    let html = "";

    REPORT_COLUMNS.forEach(col => {
        const isChecked = selectedColumns.includes(col.key);
        html += `
        <div class="col-md-4 mb-2">
            <div class="form-check">
                <input class="form-check-input column-checkbox"
                       type="checkbox"
                       value="${col.key}"
                       ${isChecked ? "checked" : ""}>
                <label class="form-check-label">${col.label}</label>
            </div>
        </div>`;
    });

    $("#column-list").html(html);
    updateSelectAllState();
}

function updateSelectAllState() {
    const total = $(".column-checkbox").length;
    const checked = $(".column-checkbox:checked").length;
    $("#chk-select-all").prop("checked", total === checked);
}
