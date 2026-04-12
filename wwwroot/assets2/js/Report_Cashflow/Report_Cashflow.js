let selectedMonthDuration = "thismonth";
const HEADER_KEY = "cashflowPrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadCashflowGrid();

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
        loadCashflowGrid();
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
        loadCashflowGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        setCurrentMonthDates();
        loadCashflowGrid();
    });

    /* ── With Header ── */
    const saved = sessionStorage.getItem(HEADER_KEY);
    if (saved !== null)
        document.getElementById("toggleWithHeader").checked = (saved === "true");

    document.getElementById("toggleWithHeader").addEventListener("change", function () {
        sessionStorage.setItem(HEADER_KEY, this.checked);
    });
});

/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadCashflowGrid() {

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
    };

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading Cashflow data...
        </div>
    `);
    $("#cashflowSummarySection").hide();

    $.ajax({
        url: "/api/Report_Cashflow/GetCashflowGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            $("#tableMain").html(`
                <table id="tblCashflow" class="table table-bordered w-100">
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

            renderCashflowTable(res.data.LineItems);
            renderCashflowSummary(res.data.Summary);
        },
        error: function (xhr, status, error) {
            console.error("Fetch Cashflow Data Failed", xhr, status, error);
            $("#tableMain").html(`<div class="alert alert-danger">Failed to load data. Please try again.</div>`);
        }
    });
}

/* ═══════════════════════════════════════════════════
   RENDER LINE ITEMS TABLE
═══════════════════════════════════════════════════ */
function renderCashflowTable(data) {

    if ($.fn.DataTable.isDataTable("#tblCashflow")) {
        $("#tblCashflow").DataTable().clear().destroy();
    }

    $("#tblCashflow").DataTable({
        data: data,
        responsive: true,
        pageLength: 25,
        scrollX: true,
        order: [[1, "asc"]],

        columns: [
            {
                data: "SrNo",
                title: "#",
                width: "50px",
                className: "text-center"
            },
            {
                data: "TxnDate",
                title: "Date",
                width: "110px"
            },
            {
                data: "FlowType",
                title: "Type",
                width: "100px",
                className: "text-center",
                render: function (data) {
                    const cls = data === "Inflow" ? "success" : "danger";
                    const icon = data === "Inflow"
                        ? "fa-arrow-down"
                        : "fa-arrow-up";
                    return `<span class="badge bg-${cls}">
                                <i class="fa ${icon} me-1"></i>${data}
                            </span>`;
                }
            },
            {
                data: "Description",
                title: "Description"
            },
            {
                data: "Amount",
                title: "Amount",
                className: "text-end fw-semibold",
                render: function (data, type, row) {
                    const cls = row.flowType === "Inflow" ? "text-success" : "text-danger";
                    const sign = row.flowType === "Inflow" ? "+" : "−";
                    return `<span class="${cls}">${sign} ${formatINR2(data)}</span>`;
                }
            }
        ],

        /* ── Footer: separate inflow / outflow totals ── */
        footerCallback: function (tfoot, data, start, end, display) {

            const api = this.api();
            let totalInflow = 0;
            let totalOutflow = 0;

            api.rows({ page: "all" }).every(function () {
                const d = this.data();
                if (d.FlowType === "Inflow")
                    totalInflow += parseFloat(d.Amount) || 0;
                else
                    totalOutflow += parseFloat(d.Amount) || 0;
            });

            const $cells = $(tfoot).find("th");
            $cells.eq(0).html("<strong>TOTAL</strong>");
            $cells.eq(1).html("");
            $cells.eq(2).html("");
            $cells.eq(3).html("");
            $cells.eq(4).html(`
                <div class="text-success fw-semibold">▲ ${formatINR2(totalInflow)}</div>
                <div class="text-danger fw-semibold">▼ ${formatINR2(totalOutflow)}</div>
            `);
        },

        /* ── Row colour by flow type ── */
        createdRow: function (row, rowData) {
            if (rowData.flowType === "Inflow")
                $(row).addClass("table-success").css("opacity", "0.85");
            else
                $(row).addClass("table-danger").css("opacity", "0.85");
        },

        language: {
            emptyTable: "No cashflow records found for the selected period."
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
function renderCashflowSummary(summary) {

    if (!summary) return;

    const netCashflow = parseFloat(summary.NetCashflow) || 0;

    const cards = [
        {
            label: "Total Inflow",
            value: summary.TotalInflow,
            icon: "fa-arrow-trend-up",
            color: "success",
            light: "success",
            formula: "Sales Collection (Customer Invoices)\n+ Other Income (Income Transactions)"
        },
        {
            label: "Total Outflow",
            value: summary.TotalOutflow,
            icon: "fa-arrow-trend-down",
            color: "danger",
            light: "danger",
            formula: "Expense Payments (Expense Transactions)\n+ Purchase Payments (Purchase_Payment)"
        }
    ];

    let cardsHtml = "";
    cards.forEach(c => {
        const formatted = formatINR2(parseFloat(c.value) || 0);
        const tooltipHtml = `<div class='text-start'><span class='text-warning fw-semibold'>${c.label}</span><hr class='my-1'/>${c.formula.replace(/\n/g, "<br/>")}</div>`;

        cardsHtml += `
        <div class="col-md-6 col-lg-4">
            <div class="card border-${c.light} h-100 cf-summary-card"
                 data-bs-toggle="tooltip"
                 data-bs-placement="top"
                 data-bs-custom-class="pl-formula-tooltip"
                 data-bs-html="true"
                 title="${tooltipHtml.replace(/"/g, '&quot;')}"
                 style="cursor:help;">
                <div class="card-body d-flex align-items-center gap-3">
                    <div class="rounded-circle bg-${c.light} bg-opacity-10 p-3 flex-shrink-0">
                        <i class="fa ${c.icon} text-${c.color} fa-lg"></i>
                    </div>
                    <div>
                        <div class="text-muted small">
                            ${c.label}
                            <i class="fa fa-circle-info ms-1 text-secondary" style="font-size:10px;"></i>
                        </div>
                        <div class="fw-bold fs-6 text-${c.color}">${formatted}</div>
                    </div>
                </div>
            </div>
        </div>`;
    });

    $("#cashflowSummaryCards").html(cardsHtml);

    // Net Cashflow banner
    const isPositive = netCashflow >= 0;
    const bannerCls = isPositive ? "bg-success" : "bg-danger";
    const bannerIcon = isPositive ? "fa-droplet" : "fa-triangle-exclamation";
    const bannerText = isPositive ? "Positive Net Cashflow" : "Negative Net Cashflow";
    const netFormula = "Total Inflow − Total Outflow";

    const tooltipHtml = `<div class='text-start'><span class='text-warning fw-semibold'>Net Cashflow</span><hr class='my-1'/>${netFormula}</div>`;

    $("#netCashflowBanner").html(`
        <span class="badge ${bannerCls} fs-5 px-4 py-3 rounded-3 cf-summary-card"
              data-bs-toggle="tooltip"
              data-bs-placement="top"
              data-bs-custom-class="pl-formula-tooltip"
              data-bs-html="true"
              title="${tooltipHtml.replace(/"/g, '&quot;')}"
              style="cursor:help;">
            <i class="fa ${bannerIcon} me-2"></i>
            ${bannerText}: ${formatINR2(Math.abs(netCashflow))}
            <i class="fa fa-circle-info ms-2" style="font-size:11px;opacity:0.7;"></i>
        </span>
    `).show();

    $("#cashflowSummarySection").show();

    // ── Init tooltips ──
    document.querySelectorAll(".cf-summary-card[data-bs-toggle='tooltip']").forEach(el => {
        const existing = bootstrap.Tooltip.getInstance(el);
        if (existing) existing.dispose();
        new bootstrap.Tooltip(el, { trigger: "hover focus", html: true });
    });
}

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewCashflowReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration,
        WithHeader: withHeader,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_Cashflow/DownloadCashflowReport_Excel"
        : "/api/Report_Cashflow/DownloadCashflowReport";

    showTempLoader("Generating Cashflow Report...");

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
            link.download = `Cashflow_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            Swal.close();
            console.error("Download Cashflow Report Failed", xhr, status, error);
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