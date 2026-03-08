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
            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }
            renderDailySalesTable(res.data);
        },
        error: function (xhr, status, error) {
            console.error("Fetch Daily Sales Failed", xhr, status, error);
        }
    });
}

/* ─────────────────────────────────────────────
   RENDER TABLE
───────────────────────────────────────────── */
function renderDailySalesTable(data) {

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
            {
                data: "SrNo",
                title: "#"
            },
            {
                data: "Date",
                title: "Date"
            },
            {
                data: "InvoiceNos",
                title: "Invoice Nos"
            },
            {
                data: "TotalInvoices",
                title: "Total Invoices",
                className: "text-center"
            },
            {
                data: "GrossSales",
                title: "Gross Sales",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "GST",
                title: "GST",
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
                data: "Cash",
                title: "Cash",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "Online",
                title: "Online",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "Due",
                title: "Due",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "Total",
                title: "Total",
                className: "text-end fw-bold",
                render: d => formatINR2(d)
            }
        ],

        /* ── Footer totals row ── */
        footerCallback: function (row, data, start, end, display) {

            const api = this.api();

            const sumCol = (colIndex) =>
                api.column(colIndex, { page: "all" })
                    .data()
                    .reduce((a, b) => a + (parseFloat(b) || 0), 0);

            const fmt = v => formatINR2(v);

            $(api.column(4).footer()).html(`<strong>${fmt(sumCol(4))}</strong>`);
            $(api.column(5).footer()).html(`<strong>${fmt(sumCol(5))}</strong>`);
            $(api.column(6).footer()).html(`<strong>${fmt(sumCol(6))}</strong>`);
            $(api.column(7).footer()).html(`<strong>${fmt(sumCol(7))}</strong>`);
            $(api.column(8).footer()).html(`<strong>${fmt(sumCol(8))}</strong>`);
            $(api.column(9).footer()).html(`<strong>${fmt(sumCol(9))}</strong>`);
            $(api.column(10).footer()).html(`<strong>${fmt(sumCol(10))}</strong>`);
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
        : "/api/Report_DailySales/DownloadDailySalesReport";

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