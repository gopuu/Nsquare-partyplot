/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No" },
    { key: "ModDate", label: "Date" },
    { key: "Module", label: "Module" },
    { key: "RefNo", label: "Reference" },
    { key: "Amount", label: "Amount Impact" }
];

const MODULE_CONFIG = {
    Sales: { color: "success", icon: "fa-file-invoice-dollar", bg: "#e8f5e9" },
    Purchase: { color: "danger", icon: "fa-cart-shopping", bg: "#ffebee" },
    Expense: { color: "warning", icon: "fa-money-bill-wave", bg: "#fff8e1" },
    Income: { color: "info", icon: "fa-circle-plus", bg: "#e3f2fd" }
};

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "modificationPrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadModificationGrid();
    loadColumnCheckboxes();

    /* ── Module filter live reload ── */
    $("#ddlModule").on("change", function () {
        loadModificationGrid();
    });

    /* ── Quick range ── */
    $("#quickRange").on("change", function () {

        const newFilter = $(this).val();
        if (selectedMonthDuration === newFilter) return;
        selectedMonthDuration = newFilter;

        if (selectedMonthDuration === "customrange") return;

        const today = new Date();
        let fromDate, toDate;

        switch (selectedMonthDuration) {
            case "thismonth":
                fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
                toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
                break;
            case "lastmonth":
                fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
                toDate = new Date(today.getFullYear(), today.getMonth(), 0);
                break;
            case "thisyear":
                fromDate = new Date(today.getFullYear(), 0, 1);
                toDate = new Date(today.getFullYear(), 11, 31);
                break;
            case "lastyear":
                fromDate = new Date(today.getFullYear() - 1, 0, 1);
                toDate = new Date(today.getFullYear() - 1, 11, 31);
                break;
            case "last7days":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 6);
                toDate = today;
                break;
            case "last30days":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 29);
                toDate = today;
                break;
            case "yesterday":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 1);
                toDate = new Date(fromDate);
                break;
            case "last12months":
                fromDate = new Date(today);
                fromDate.setMonth(today.getMonth() - 12);
                toDate = today;
                break;
        }

        $("#fromDate").val(formatDate2(fromDate));
        $("#toDate").val(formatDate2(toDate));
        loadModificationGrid();
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

    $("#btnToggleFilter").on("click", function () { $("#filterPanel").toggleClass("active"); });
    $("#btnCloseFilter").on("click", function () { $("#filterPanel").removeClass("active"); });

    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
        loadModificationGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        $("#ddlModule").val("");
        setCurrentMonthDates();
        loadModificationGrid();
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
        loadModificationGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadModificationGrid() {

    const payload = {
        Module: $("#ddlModule").val() || null,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
    };

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading Modification data...
        </div>
    `);
    $("#modSummarySection").hide();

    $.ajax({
        url: "/api/Report_Modification/GetModificationGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            $("#tableMain").html(`
                <table id="tblModification" class="table table-bordered w-100">
                    <tfoot>
                        <tr>
                            <th></th><th></th><th></th><th></th><th></th>
                        </tr>
                    </tfoot>
                </table>
            `);

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            renderModificationTable(res.data.LineItems);
            renderModificationSummary(res.data.Summary);
        },
        error: function (xhr, status, error) {
            console.error("Fetch Modification Data Failed", xhr, status, error);
            $("#tableMain").html(`<div class="alert alert-danger">Failed to load data. Please try again.</div>`);
        }
    });
}

/* ═══════════════════════════════════════════════════
   RENDER TABLE
═══════════════════════════════════════════════════ */
function renderModificationTable(data) {

    if ($.fn.DataTable.isDataTable("#tblModification")) {
        $("#tblModification").DataTable().clear().destroy();
    }

    $("#tblModification").DataTable({
        data: data,
        responsive: true,
        pageLength: 25,
        scrollX: true,
        order: [[1, "asc"]],

        paging: false,
        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows
        scrollX: true,            // horizontal scroll if needed

        columns: [
            {
                data: "SrNo",
                title: "#",
                width: "50px",
                className: "text-center"
            },
            {
                data: "ModDate",
                title: "Date",
                width: "110px"
            },
            {
                data: "Module",
                title: "Module",
                width: "110px",
                className: "text-center",
                render: function (data) {
                    const cfg = MODULE_CONFIG[data] || { color: "secondary", icon: "fa-circle" };
                    return `<span class="badge bg-${cfg.color}">
                                <i class="fa ${cfg.icon} me-1"></i>${data}
                            </span>`;
                }
            },
            {
                data: "RefNo",
                title: "Reference"
            },
            {
                data: "Amount",
                title: "Amount Impact",
                className: "text-end fw-semibold",
                render: d => formatINR2(d)
            }
        ],

        /* ── Footer totals ── */
        footerCallback: function (tfoot, data, start, end, display) {

            const api = this.api();
            const sumCol = idx =>
                api.column(idx, { page: "all" })
                    .data()
                    .reduce((acc, val) => acc + (parseFloat(val) || 0), 0);

            const $cells = $(tfoot).find("th");
            $cells.eq(0).html("<strong>TOTAL</strong>");
            $cells.eq(1).html("");
            $cells.eq(2).html(`<strong>${api.rows({ page: "all" }).count()} records</strong>`);
            $cells.eq(3).html("");
            $cells.eq(4).html(`<strong>${formatINR2(sumCol(4))}</strong>`);
        },

        /* ── Row colour by module ── */
        createdRow: function (row, rowData) {
            const cfg = MODULE_CONFIG[rowData.Module];
            if (cfg) {
                $(row).css("background-color", cfg.bg);
            }
        },

        language: {
            emptyTable: "No modification records found for the selected period."
        },

        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },

    });
}

/* ═══════════════════════════════════════════════════
   RENDER SUMMARY CARDS
═══════════════════════════════════════════════════ */
function renderModificationSummary(summary) {

    if (!summary) return;

    const modules = [
        {
            key: "Sales",
            countKey: "SalesCount",
            amtKey: "SalesAmount",
            formula: "Customer Invoice"
        },
        {
            key: "Purchase",
            countKey: "PurchaseCount",
            amtKey: "PurchaseAmount",
            formula: "Purchase"
        },
        {
            key: "Expense",
            countKey: "ExpenseCount",
            amtKey: "ExpenseAmount",
            formula: "Expense Transactions"
        },
        {
            key: "Income",
            countKey: "IncomeCount",
            amtKey: "IncomeAmount",
            formula: "Income Transactions"
        }
    ];

    let cardsHtml = "";
    modules.forEach(m => {
        const cfg = MODULE_CONFIG[m.key];
        const count = parseInt(summary[m.countKey]) || 0;
        const amount = parseFloat(summary[m.amtKey]) || 0;
        const tooltipHtml = `<div class='text-start'><span class='text-warning fw-semibold'>${m.key} Modifications</span><hr class='my-1'/>${m.formula.replace(/\n/g, "<br/>")}</div>`;

        cardsHtml += `
        <div class="col-md-6 col-lg-3">
            <div class="card border-${cfg.color} h-100 mod-summary-card"
                 data-bs-toggle="tooltip"
                 data-bs-placement="top"
                 data-bs-custom-class="pl-formula-tooltip"
                 data-bs-html="true"
                 title="${tooltipHtml.replace(/"/g, '&quot;')}"
                 style="cursor:help;">
                <div class="card-body">
                    <div class="d-flex align-items-center gap-3 mb-2">
                        <div class="rounded-circle bg-${cfg.color} bg-opacity-10 p-3 flex-shrink-0">
                            <i class="fa ${cfg.icon} text-${cfg.color} fa-lg"></i>
                        </div>
                        <div>
                            <div class="text-muted small">
                                ${m.key}
                                <i class="fa fa-circle-info ms-1 text-secondary" style="font-size:10px;"></i>
                            </div>
                            <div class="fw-bold fs-7 text-${cfg.color}">${m.key} Modifications</div>
                        </div>
                    </div>
                    <div class="d-flex justify-content-between align-items-center mt-2 pt-2 border-top">
                        <span class="badge bg-${cfg.color} bg-opacity-75">
                            <i class="fa fa-pen me-1"></i>${count} records
                        </span>
                        <span class="fw-semibold text-${cfg.color}">${formatINR2(amount)}</span>
                    </div>
                </div>
            </div>
        </div>`;
    });

    $("#modSummaryCards").html(cardsHtml);

    // Total banner
    const total = parseInt(summary.TotalModifications) || 0;
    const totalAmt = parseFloat(summary.TotalAmount) || 0;

    $("#modTotalBanner").html(`
        <span class="badge bg-secondary fs-6 px-4 py-3 rounded-3 me-3">
            <i class="fa fa-pen-to-square me-2"></i>
            Total Modifications: <strong>${total}</strong>
        </span>
        <span class="badge bg-dark fs-6 px-4 py-3 rounded-3">
            <i class="fa fa-indian-rupee-sign me-2"></i>
            Total Amount Impact: <strong>${formatINR2(totalAmt)}</strong>
        </span>
    `).show();

    $("#modSummarySection").show();

    // ── Init tooltips ──
    document.querySelectorAll(".mod-summary-card[data-bs-toggle='tooltip']").forEach(el => {
        const existing = bootstrap.Tooltip.getInstance(el);
        if (existing) existing.dispose();
        new bootstrap.Tooltip(el, { trigger: "hover focus", html: true });
    });
}

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewModificationReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        Module: $("#ddlModule").val() || null,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration,
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_Modification/DownloadModificationReport_Excel"
        : "/api/Report_Modification/DownloadModificationReport";

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
            link.download = `Modification_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            Swal.close();
            console.error("Download Modification Report Failed", xhr, status, error);
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