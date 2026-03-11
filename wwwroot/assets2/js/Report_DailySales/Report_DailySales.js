const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No", visible: true },
    { key: "Date", label: "Date", visible: true },
    { key: "InvoiceNos", label: "Invoice Nos", visible: true },
    { key: "TotalInvoices", label: "Total No of Invoice", visible: true },
    { key: "GrossSales", label: "Gross Sales", visible: true },
    { key: "GST", label: "GST", visible: true },
    { key: "NetSales", label: "Net Sales", visible: true },
    { key: "Cash", label: "Cash", visible: true },
    { key: "Online", label: "Online", visible: true },
    { key: "Due", label: "Due", visible: true },
    { key: "Total", label: "Total", visible: true }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "dailySalesPrintWithHeader";
let selectedInvoiceDate = null;

/* ─────────────────────────────────────────────
   INIT
───────────────────────────────────────────── */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadDailySalesGrid();
    loadColumnCheckboxes();

    /* ── Quick range ── */
    $("#quickRange").on("change", function () {

        const newFilter = $(this).val();
        if (selectedMonthDuration === newFilter) return;
        selectedMonthDuration = newFilter;

        if (selectedMonthDuration === "customrange") return;

        const today = new Date();
        let fromDate, toDate;

        if (selectedMonthDuration === "thismonth") {
            fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
            toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
        }
        else if (selectedMonthDuration === "lastmonth") {
            fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
            toDate = new Date(today.getFullYear(), today.getMonth(), 0);
        }
        else if (selectedMonthDuration === "days") {
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 6);
            toDate = today;
        }
        else if (selectedMonthDuration === "lastdays") {
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 29);
            toDate = today;
        }
        else if (selectedMonthDuration === "yesterday") {
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 1);
            toDate = fromDate;
        }
        else if (selectedMonthDuration === "12months") {
            fromDate = new Date(today);
            fromDate.setMonth(today.getMonth() - 12);
            toDate = today;
        }

        $("#fromDate").val(formatDate2(fromDate));
        $("#toDate").val(formatDate2(toDate));

        loadDailySalesGrid();
    });

    /* ── Filter panel ── */
    $("#filterPanel").on("click", function (e) { e.stopPropagation(); });

    $(document).on("click", function (e) {
        if ($(e.target).closest(".dropdown, .dropdown-menu, #filterPanel, #btnToggleFilter").length > 0) return;
        $("#filterPanel").removeClass("active");
    });

    //$(document).on("click", function (e) {
    //    if ($(e.target).closest(".dropdown, .dropdown-menu").length > 0) return;
    //    if ($(e.target).closest("#filterPanel").length > 0) return;
    //});

    $("#btnToggleFilter").on("click", function () {
        $("#filterPanel").toggleClass("active");
    });

    $("#btnCloseFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
    });

    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
        loadDailySalesGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        setCurrentMonthDates();
        loadDailySalesGrid();
    });

    /* ── With Header toggle ── */
    const saved = sessionStorage.getItem(HEADER_KEY);
    if (saved !== null) {
        document.getElementById("toggleWithHeader").checked = (saved === "true");
    }

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
        loadDailySalesGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });

    $(document).on("click", ".daily-sales-date", function () {
        selectedInvoiceDate = $(this).data("date");
        console.log("invoiceDate:", selectedInvoiceDate);
        loadDailySalesDateDetails(selectedInvoiceDate);
        $("#dailySalesDetailsModal").modal("show");
    });

    //Recalculate Columns When Modal Opens
    $('#dailySalesDetailsModal').on('shown.bs.modal', function () {
        $('#tblDailySalesDetails').DataTable().columns.adjust();
    });

});

/* ─────────────────────────────────────────────
   LOAD GRID
───────────────────────────────────────────── */
function loadDailySalesGrid() {

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
    };

    $.ajax({
        url: "/api/Report_DailySales/GetDailySalesGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),

        success: function (res) {
            console.log("GetDailySalesGrid res:", res);

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            const rows = res.data?.rows || [];
            const totals = res.data?.totals || null;

            renderDailySalesTable(rows, totals);
        },

        error: function (xhr, status, error) {
            console.error("Fetch Daily Sales Failed", xhr, status, error);
            Swal.fire("Error", "Failed to load daily sales data", "error");
        }
    });
}

/* ─────────────────────────────────────────────
   RENDER TABLE
───────────────────────────────────────────── */
function renderDailySalesTable(data, totals) {

    if ($.fn.DataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().clear().destroy();
    }

    $("#tableMain table").DataTable({
        data: data,
        responsive: true,
        pageLength: 25,
        scrollX: true,
        order: [[1, "asc"]],

        columns: [
            { data: "SrNo", title: "#" },

            {
                data: "Date",
                title: "Date",
                render: function (data, type, row) {
                    return `<a href="javascript:void(0)" 
                            class="daily-sales-date"
                            data-date="${row.InvoiceDate}">
                            ${data}
                        </a>`;
                }
            },

            {
                data: "TotalInvoices",
                title: "Invoices",
                className: "text-center"
            },

            {
                data: "GrossSales",
                title: "Gross Sales",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "Taxes",
                title: "Taxes",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "NetSales",
                title: "Net Sales",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "PaymentAmount",
                title: "Payment Amount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "PaymentMode",
                title: "Payment Mode",
                className: "text-center"
            },

            {
                data: "DuePayments",
                title: "Due Payments",
                className: "text-end",
                render: d => formatINR2(d)
            }
        ],

        /* ── Footer totals ── */

        footerCallback: function () {

            if (!totals) return;

            const fmt = v => formatINR2(v);
            const api = this.api();

            $(api.column(0).footer()).html("<strong>TOTAL</strong>");
            $(api.column(2).footer()).html(`<strong>${totals.TotalInvoices ?? ""}</strong>`);
            $(api.column(3).footer()).html(`<strong>${fmt(totals.GrossSales)}</strong>`);
            $(api.column(4).footer()).html(`<strong>${fmt(totals.Taxes)}</strong>`);
            $(api.column(5).footer()).html(`<strong>${fmt(totals.NetSales)}</strong>`);
            $(api.column(6).footer()).html(`<strong>${fmt(totals.TotalPayments)}</strong>`);
            $(api.column(8).footer()).html(`<strong>${fmt(totals.DuePayments)}</strong>`);
        }        
    });
}

/* ─────────────────────────────────────────────
   DOWNLOAD PDF / EXCEL
───────────────────────────────────────────── */
function ViewDailySalesReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration,
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = (actionName === "downloadExcel")
        ? "/api/Report_DailySales/DownloadDailySalesReport_Excel"
        : "/api/Report_DailySales/DownloadDailySalesReport_Pdf";

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

            const fileUrl = res.data.FileUrl;
            console.log("fileUrl:", fileUrl);

            const ext = actionName === "downloadExcel" ? "xlsx" : "pdf";
            const ts = new Date().toISOString().replace(/[:.-]/g, "");

            const link = document.createElement("a");
            link.href = fileUrl;
            link.download = `DailySales_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            Swal.close();
            console.error("Download Daily Sales Failed", xhr, status, error);
            Swal.fire("Error", "Failed to generate report.", "error");
        }
    });
}

/* ─────────────────────────────────────────────
   HELPERS
───────────────────────────────────────────── */
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

//Details modal - START
function loadDailySalesDateDetails(invoiceDate) {
    var payload = {
        InvoiceDate: invoiceDate
    };

    $.ajax({
        url: "/api/Report_DailySales/GetDailySalesDateDetails",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }
            renderDailySalesDetailsTable(res.data.rows, res.data.totals);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load invoice details");
        }
    });

}
function renderDailySalesDetailsTable(rows, totals) {

    const table = $("#tblDailySalesDetails");

    if ($.fn.DataTable.isDataTable(table)) {
        table.DataTable().clear().destroy();
    }

    table.DataTable({
        data: rows,
        paging: false,
        searching: false,
        info: false,
        scrollX: true,
        scrollY: "350px",
        scrollCollapse: true,
        autoWidth: false,
        destroy: true,

        columns: [
            { data: "SrNo", width: "50px" },

            { data: "InvoiceNo", width: "120px" },

            { data: "InvoiceDate", width: "120px" },

            { data: "ClientName", width: "200px" },

            {
                data: "GrossAmount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "Taxes",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "NetAmount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "PaidAmount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "DueAmount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "PaymentModes",
                width: "160px"
            }
        ],    

        footerCallback: function () {

            if (!totals) return;

            const api = this.api();

            $(api.column(4).footer()).html(formatINR2(totals.GrossAmount));
            $(api.column(5).footer()).html(formatINR2(totals.Taxes));
            $(api.column(6).footer()).html(formatINR2(totals.NetAmount));
            $(api.column(7).footer()).html(formatINR2(totals.PaidAmount));
            $(api.column(8).footer()).html(formatINR2(totals.DueAmount));

            $(api.column(0).footer()).html("TOTAL");
        }
    });
}

function ViewDailySalesDetailsReport(actionName) {

    const payload = {
        InvoiceDate: selectedInvoiceDate,
        Columns: null,
        WithHeader: true
    };

    const url = actionName === "downloadPDF"
        ? "/api/Report_DailySales/DownloadDailySalesDateDetailsReport_Pdf"
        : "/api/Report_DailySales/DownloadDailySalesDateDetailsReport_Excel";

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

            const fileUrl = res.data.FileUrl;
            console.log("details fileUrl:", fileUrl);
            const ext = actionName === "downloadExcel" ? "xlsx" : "pdf";
            const ts = new Date().toISOString().replace(/[:.-]/g, "");

            const link = document.createElement("a");
            link.href = fileUrl;
            link.download = `DailySales_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

        },
        error: function (xhr, status, error) {
            Swal.close();
            Swal.fire("Error", "Failed to generate details report.", "error");
            handleApiError(xhr, status, error, "Failed to generate details report");
        }
    });
}
//Details modal - END