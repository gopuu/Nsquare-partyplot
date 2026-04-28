//let selectedMonthDuration = "thismonth";
const HEADER_KEY = "profitLossPrintWithHeader";

let selectedYearId = null;
let selectedYear = new Date().getFullYear();
let selectedMonthFrom = null;
let selectedMonthTo = null;

let selectedMonthId = null;

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    populateYearDropdown();

    /* ── Quick Year Range ── */
    $("#quickRange").on("change", function () {
        selectedYearId = $(this).val();
        selectedYear = $("#quickRange option:selected").data("year");
        fetchMonthData(selectedYearId);
    });
    $("#quickRange_MonthFrom, #quickRange_MonthTo").on("change", function () {
        loadProfitLossGrid();
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
        loadProfitLossGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        $("#quickRange_MonthFrom").val("0");
        $("#quickRange_MonthTo").val("0");

        selectedMonthFrom = null;
        selectedMonthTo = null;
        loadProfitLossGrid();
    });

    /* ── With Header ── */
    const saved = sessionStorage.getItem(HEADER_KEY);
    if (saved !== null)
        document.getElementById("toggleWithHeader").checked = (saved === "true");

    document.getElementById("toggleWithHeader").addEventListener("change", function () {
        sessionStorage.setItem(HEADER_KEY, this.checked);
    });
});

/* =========================
   LOAD YEAR DROPDOWN
========================= */
function populateYearDropdown() {
    $.ajax({
        url: "/api/YearMaster/GetYearList",
        type: "GET",
        contentType: "application/json",
        success: function (res) {
            if (res.status === "Success") {
                const years = res.data || [];
                let dropdown = $("#quickRange");

                dropdown.empty();
                console.log("years:", years);

                years.forEach(y => {
                    let option = $("<option>");

                    option.val(y.Id);
                    option.attr("data-year", y.YearName);

                    if (parseInt(y.YearName) === selectedYear) {
                        option.prop("selected", true);
                        selectedYearId = y.Id;
                        option.text(y.YearName + " (This Year)");
                    }
                    else {
                        option.text(y.YearName);
                    }
                    dropdown.append(option);
                });
                // Fetch months for current year
                fetchMonthData(selectedYearId);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching years");
        }
    });
}

/* =========================
   LOAD MONTHS
========================= */
function fetchMonthData(yearId) {
    var payload = { YearId: parseInt(yearId), MonthId: 0 };
    console.log("month list payload:", payload);

    $.ajax({
        url: "/api/MonthMaster/GetMonthList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                const months = res.data || [];
                console.log("months:", months);
                let fromDropdown = $("#quickRange_MonthFrom");
                let toDropdown = $("#quickRange_MonthTo");

                fromDropdown.empty();
                toDropdown.empty();

                fromDropdown.append(`<option value="0">Select</option>`);
                toDropdown.append(`<option value="0">Select</option>`);

                months.forEach(m => {

                    let option1 = $("<option>");
                    option1.val(m.MonthId);
                    option1.text(m.MonthName);

                    let option2 = $("<option>");
                    option2.val(m.MonthId);
                    option2.text(m.MonthName);

                    fromDropdown.append(option1);
                    toDropdown.append(option2);

                });

                // Load grid with current year
                loadProfitLossGrid();
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching months");
        }
    });
}

/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadProfitLossGrid() {
    selectedYearId = Number($("#quickRange").val() || 0);
    selectedMonthFrom = Number($("#quickRange_MonthFrom").val() || 0);
    selectedMonthTo = Number($("#quickRange_MonthTo").val() || 0);

    const payload = {
        YearId: selectedYearId,
        Year: selectedYear,
        MonthId_From: selectedMonthFrom,
        MonthId_To: selectedMonthTo
    };

    console.log("payload:", payload);

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading P&amp;L data...
        </div>
    `);
    $("#plSummarySection").hide();

    $.ajax({
        url: "/api/Report_ProfitLoss/GetProfitLossGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            $("#tableMain").html(`
                <table id="tblProfitLoss" class="table table-bordered w-100">
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

            renderProfitLossTable(res.data.LineItems);
            renderSummaryCards(res.data.Summary);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch P&L Data Failed");
            $("#tableMain").html(`<div class="alert alert-danger">Failed to load data. Please try again.</div>`);
        }
    });
}

/* ═══════════════════════════════════════════════════
   RENDER LINE ITEMS TABLE
═══════════════════════════════════════════════════ */
function renderProfitLossTable(data) {

    if ($.fn.DataTable.isDataTable("#tblProfitLoss")) {
        $("#tblProfitLoss").DataTable().clear().destroy();
    }

    $("#tblProfitLoss").DataTable({
        data: data,
        responsive: true,
        paging: false,         // P&L always has ≤ 4 rows — no paging needed
        searching: false,
        info: false,
        scrollX: true,
        order: [],

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
                data: "Type",
                title: "Type",
                width: "120px",
                render: function (data) {
                    const badges = {
                        "Sales": "success",
                        "Income": "info",
                        "Purchase": "danger",
                        "Expense": "warning"
                    };
                    const cls = badges[data] || "secondary";
                    return `<span class="badge bg-${cls}">${data}</span>`;
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
                    const cls = row.nature === "Credit" ? "text-success" : "text-danger";
                    return `<span class="${cls}">${formatINR2(data)}</span>`;
                }
            },
            {
                data: "Nature",
                title: "Nature",
                className: "text-center",
                render: function (data) {
                    const cls = data === "Credit" ? "success" : "danger";
                    return `<span class="badge bg-${cls}-subtle text-${cls}-emphasis border border-${cls}-subtle">${data}</span>`;
                }
            }
        ],

        /* ── Footer totals ── */
        footerCallback: function (tfoot, data, start, end, display) {

            const api = this.api();

            const totalCredit = api.column(3, { page: "all" }).data()
                .reduce((acc, val, idx) => {
                    const nature = api.row(idx).data()?.Nature;
                    return nature === "Credit" ? acc + (parseFloat(val) || 0) : acc;
                }, 0);

            const totalDebit = api.column(3, { page: "all" }).data()
                .reduce((acc, val, idx) => {
                    const nature = api.row(idx).data()?.Nature;
                    return nature === "Debit" ? acc + (parseFloat(val) || 0) : acc;
                }, 0);

            const $cells = $(tfoot).find("th");
            $cells.eq(0).html("<strong>TOTAL</strong>");
            $cells.eq(1).html("");
            $cells.eq(2).html("");
            $cells.eq(3).html(`
                <div class="text-success">${formatINR2(totalCredit)} <small class="text-muted">Credit</small></div>
                <div class="text-danger">${formatINR2(totalDebit)} <small class="text-muted">Debit</small></div>
            `);
            $cells.eq(4).html("");
        },

        /* ── Row colouring by nature ── */
        createdRow: function (row, rowData) {
            if (rowData.nature === "Credit") {
                $(row).addClass("table-success").css("opacity", "0.9");
            } else {
                $(row).addClass("table-danger").css("opacity", "0.9");
            }
        },

        language: {
            emptyTable: "No P&L data found for the selected period."
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
function renderSummaryCards(summary) {
    if (!summary) return;

    const netProfit = parseFloat(summary.NetProfit) || 0;
    const grossProfit = parseFloat(summary.GrossProfit) || 0;

    const cards = [
        {
            label: "Total Sales",
            value: summary.TotalSales,
            icon: "fa-file-invoice-dollar",
            color: "success",
            light: "success",
            formula: "Customer Invoices (GrandTotal)"
        },
        {
            label: "Other Income",
            value: summary.TotalOtherIncome,
            icon: "fa-circle-plus",
            color: "info",
            light: "info",
            formula: "Income Transactions (Amount)"
        },
        {
            label: "Total Purchase",
            value: summary.TotalPurchase,
            icon: "fa-cart-shopping",
            color: "danger",
            light: "danger",
            formula: "Purchase (Grand Total Amount)"
        },
        {
            label: "Total Expense",
            value: summary.TotalExpense,
            icon: "fa-money-bill-wave",
            color: "warning",
            light: "warning",
            formula: "Expense Transactions (Amount)"
        },
        {
            label: "Gross Profit",
            value: summary.GrossProfit,
            icon: "fa-chart-line",
            color: grossProfit >= 0 ? "success" : "danger",
            light: grossProfit >= 0 ? "success" : "danger",
            formula: "Total Sales\n− Total Purchase"
        }
    ];

    let cardsHtml = "";
    cards.forEach(c => {
        const formatted = formatINR2(parseFloat(c.value) || 0);

        // Tooltip content — newlines become <br> inside data-bs-html tooltip
        const tooltipText = c.formula.replace(/\n/g, "&#10;");

        cardsHtml += `
        <div class="col-md-4 col-lg">
            <div class="card border-${c.light} h-100 pl-summary-card"
                 data-bs-toggle="tooltip"
                 data-bs-placement="top"
                 data-bs-custom-class="pl-formula-tooltip"
                 data-bs-html="true"
                 title="<div class='text-start'><span class='text-warning fw-semibold'>${c.label}</span><hr class='my-1'/>${c.formula.replace(/\n/g, "<br/>")}</div>"
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
                        <div class="fw-bold fs-7 text-${c.color}">${formatted}</div>
                    </div>
                </div>
            </div>
        </div>`;
    });

    $("#plSummaryCards").html(cardsHtml);

    // ── Net Profit banner ──
    const bannerCls = netProfit >= 0 ? "bg-success" : "bg-danger";
    const bannerIcon = netProfit >= 0 ? "fa-trophy" : "fa-triangle-exclamation";
    const bannerText = netProfit >= 0 ? "Net Profit" : "Net Loss";
    const netFormula = "(Total Sales + Other Income) − (Total Purchase + Total Expense)";

    $("#netProfitBanner").html(`
        <span class="badge ${bannerCls} fs-5 px-4 py-3 rounded-3 pl-summary-card"
              data-bs-toggle="tooltip"
              data-bs-placement="top"
              data-bs-custom-class="pl-formula-tooltip"
              data-bs-html="true"
              title="<div class='text-start'><span class='text-warning fw-semibold'>${bannerText}</span><hr class='my-1'/>${netFormula}</div>"
              style="cursor:help;">
            <i class="fa ${bannerIcon} me-2"></i>
            ${bannerText}: ${formatINR2(Math.abs(netProfit))}
            <i class="fa fa-circle-info ms-2" style="font-size:11px; opacity:0.7;"></i>
        </span>
    `).show();

    $("#plSummarySection").show();

    // ── Init Bootstrap tooltips on newly rendered cards ──
    document.querySelectorAll(".pl-summary-card[data-bs-toggle='tooltip']").forEach(el => {
        // Dispose any existing instance first (safe re-init on filter change)
        const existing = bootstrap.Tooltip.getInstance(el);
        if (existing) existing.dispose();

        new bootstrap.Tooltip(el, {
            trigger: "hover focus",
            html: true
        });
    });
}

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewProfitLossReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        YearId: selectedYearId,
        Year: selectedYear,
        MonthId_From: selectedMonthFrom,
        MonthId_To: selectedMonthTo,
        WithHeader: withHeader,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_ProfitLoss/DownloadProfitLossReport_Excel"
        : "/api/Report_ProfitLoss/DownloadProfitLossReport_Pdf";

    showTempLoader("Generating P&L Report...");

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
            link.download = `ProfitLoss_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            Swal.close();
            handleApiError(xhr, status, error, "Download P&L Report Failed");
            Swal.fire("Error", "Failed to generate report. Please try again.", "error");
        }
    });
}


