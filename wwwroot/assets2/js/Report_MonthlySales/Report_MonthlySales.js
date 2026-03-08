const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No", visible: true },
    { key: "Month", label: "Month", visible: true },
    { key: "InvoiceNos", label: "Invoice Nos", visible: true },
    { key: "TotalInvoices", label: "Total Invoice", visible: true },
    { key: "GrossSales", label: "Gross Sales", visible: true },
    { key: "CGST", label: "GST", visible: true },
    { key: "NetSales", label: "Net Sales", visible: true },
    { key: "Cash", label: "Cash", visible: true },
    { key: "Online", label: "Online", visible: true },
    { key: "Due", label: "Due", visible: true },
    { key: "Total", label: "Total", visible: true }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thisyear";
const HEADER_KEY = "monthlySalesPrintWithHeader";

/* ─────────────────────────────────────────────
   INIT
───────────────────────────────────────────── */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentYearDates();
    loadMonthlySalesGrid();
    loadColumnCheckboxes();

    /* ── Quick range ── */
    $("#quickRange").on("change", function () {

        const newFilter = $(this).val();
        if (selectedMonthDuration === newFilter) return;
        selectedMonthDuration = newFilter;

        if (selectedMonthDuration === "customrange") return;

        const today = new Date();
        let fromDate, toDate;

        if (selectedMonthDuration === "thisyear") {
            fromDate = new Date(today.getFullYear(), 0, 1);
            toDate = new Date(today.getFullYear(), 11, 31);
        }
        else if (selectedMonthDuration === "lastyear") {
            fromDate = new Date(today.getFullYear() - 1, 0, 1);
            toDate = new Date(today.getFullYear() - 1, 11, 31);
        }
        else if (selectedMonthDuration === "thismonth") {
            fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
            toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
        }
        else if (selectedMonthDuration === "lastmonth") {
            fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
            toDate = new Date(today.getFullYear(), today.getMonth(), 0);
        }
        else if (selectedMonthDuration === "12months") {
            fromDate = new Date(today);
            fromDate.setMonth(today.getMonth() - 12);
            toDate = today;
        }

        $("#fromDate").val(formatDate2(fromDate));
        $("#toDate").val(formatDate2(toDate));

        loadMonthlySalesGrid();
    });

    /* ── Filter panel ── */
    $("#filterPanel").on("click", function (e) { e.stopPropagation(); });

    $(document).on("click", function (e) {
        if ($(e.target).closest(".dropdown, .dropdown-menu, #filterPanel, #btnToggleFilter").length > 0) return;
        $("#filterPanel").removeClass("active");
    });

    $("#btnToggleFilter").on("click", function () {
        $("#filterPanel").toggleClass("active");
    });

    $("#btnCloseFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
    });

    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
        loadMonthlySalesGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thisyear");
        selectedMonthDuration = "thisyear";
        setCurrentYearDates();
        loadMonthlySalesGrid();
    });

    /* ── With Header ── */
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
        loadMonthlySalesGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

/* ─────────────────────────────────────────────
   LOAD GRID
───────────────────────────────────────────── */
function loadMonthlySalesGrid() {

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
    };

    $.ajax({
        url: "/api/Report_MonthlySales/GetMonthlySalesGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }
            renderMonthlySalesTable(res.data);
        },
        error: function (xhr, status, error) {
            console.error("Fetch Monthly Sales Failed", xhr, status, error);
        }
    });
}

/* ─────────────────────────────────────────────
   RENDER TABLE
───────────────────────────────────────────── */
function renderMonthlySalesTable(data) {

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
            { data: "Month", title: "Month" },
            { data: "InvoiceNos", title: "Invoice Nos" },
            { data: "TotalInvoices", title: "Total Invoice", className: "text-center" },
            { data: "GrossSales", title: "Gross Sales", className: "text-end", render: d => formatINR2(d) },
            { data: "CGST", title: "GST", className: "text-end", render: d => formatINR2(d) },
            { data: "NetSales", title: "Net Sales", className: "text-end", render: d => formatINR2(d) },
            { data: "Cash", title: "Cash", className: "text-end", render: d => formatINR2(d) },
            { data: "Online", title: "Online", className: "text-end", render: d => formatINR2(d) },
            { data: "Due", title: "Due", className: "text-end", render: d => formatINR2(d) },
            { data: "Total", title: "Total", className: "text-end fw-bold", render: d => formatINR2(d) },
            // hidden
            { data: "Yr", visible: false },
            { data: "Mn", visible: false }
        ],

        footerCallback: function (row, data, start, end, display) {

            const api = this.api();
            const sumCol = idx =>
                api.column(idx, { page: "all" })
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
   DOWNLOAD
───────────────────────────────────────────── */
function ViewMonthlySalesReport(actionName) {

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

    const url = actionName === "downloadExcel"
        ? "/api/Report_MonthlySales/DownloadMonthlySalesReport_Excel"
        : "/api/Report_MonthlySales/DownloadMonthlySalesReport";

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
            link.download = `MonthlySales_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            console.error("Download Monthly Sales Failed", xhr, status, error);
            Swal.fire("Error", "Failed to generate report.", "error");
        }
    });
}

/* ─────────────────────────────────────────────
   HELPERS
───────────────────────────────────────────── */
function setCurrentYearDates() {
    const today = new Date();
    $("#fromDate").val(formatDate2(new Date(today.getFullYear(), 0, 1)));
    $("#toDate").val(formatDate2(new Date(today.getFullYear(), 11, 31)));
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