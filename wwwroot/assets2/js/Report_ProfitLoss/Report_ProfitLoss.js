//let selectedMonthDuration = "thismonth";
const HEADER_KEY = "profitLossPrintWithHeader";
let selectedMonthDuration = "thismonth";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadProfitLossGrid();

    /* ── Quick range ── */
    $("#quickRange").on("change", function () {
        const newFilter = $(this).val();

        if (selectedMonthDuration === newFilter) return;

        selectedMonthDuration = newFilter;

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

            case "thiscalendaryear":
                fromDate = new Date(today.getFullYear(), 0, 1);
                toDate = new Date(today.getFullYear(), 11, 31);
                break;

            case "lastcalendaryear":
                fromDate = new Date(today.getFullYear() - 1, 0, 1);
                toDate = new Date(today.getFullYear() - 1, 11, 31);
                break;

            case "thisfinancialyear":
                if (today.getMonth() >= 3) {
                    // Apr current year → Mar next year
                    fromDate = new Date(today.getFullYear(), 3, 1);
                    toDate = new Date(today.getFullYear() + 1, 2, 31);
                }
                else {
                    // Apr previous year → Mar current year
                    fromDate = new Date(today.getFullYear() - 1, 3, 1);
                    toDate = new Date(today.getFullYear(), 2, 31);
                }
                break;

            case "lastfinancialyear":
                if (today.getMonth() >= 3) {
                    // Apr previous year → Mar current year
                    fromDate = new Date(today.getFullYear() - 1, 3, 1);
                    toDate = new Date(today.getFullYear(), 2, 31);
                }
                else {
                    // Apr 2 years back → Mar previous year
                    fromDate = new Date(today.getFullYear() - 2, 3, 1);
                    toDate = new Date(today.getFullYear() - 1, 2, 31);
                }
                break;
        }

        $("#fromDate").val(formatDate2(fromDate));

        $("#toDate").val(formatDate2(toDate));

        loadProfitLossGrid();
    });

    $("#fromDate, #toDate").on("change", function () {
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
        selectedMonthDuration = "thismonth";
        setCurrentMonthDates();
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

function setCurrentMonthDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    $("#fromDate").val(formatDate2(firstDay));
    $("#toDate").val(formatDate2(lastDay));
}

function toTitleCase(str) {
    return (str || "").toLowerCase().replace(/\b\w/g, c => c.toUpperCase());
}

/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadProfitLossGrid() {

    const filterDuration = $("#quickRange").val() || null;
    const fromDate = $("#fromDate").val() || null;
    const toDate = $("#toDate").val() || null;

    const payload = {
        FilterDuration: filterDuration,
        FromDate: fromDate,
        ToDate: toDate
    };

    console.log("payload:", payload);

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading P&amp;L data...
        </div>
    `);

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

            // Update report title
            if (res.data && res.data.ReportTitle) {
                $("#reportTitle").text(res.data.ReportTitle);
            }

            renderProfitLossStatement(res.data);
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
function renderProfitLossStatement(data) {

    if (!data) {

        $("#tableMain").html(`
            <div class="alert alert-warning mb-0">
                No data found
            </div>
        `);

        return;
    }

    const revenue = data.Revenue || [];
    const cogs = data.COGS || [];
    const expenses = data.Expenses || [];

    const totalRevenue = data.TotalRevenue || 0;
    const totalCOGS = data.TotalCOGS || 0;
    const totalExpense = data.TotalExpense || 0;

    const grossProfit = totalRevenue - totalCOGS;

    const netPL = data.NetProfitLoss || 0;

    const isProfit = netPL >= 0;

    let html = `
    <div class="pl-report-wrapper">

        <!-- SUMMARY CARDS -->
        <div class="row g-2 mb-3">

            <!-- Revenue -->
            <div class="col-6 col-md-3">
                <div class="pl-summary-card revenue">

                    <div class="pl-summary-top">
                        <div class="pl-summary-icon">
                            <i class="fa fa-arrow-trend-up"></i>
                        </div>

                        <div class="pl-summary-label">
                            Revenue
                        </div>
                    </div>

                    <div class="pl-summary-value">
                        ${formatINR2(totalRevenue)}
                    </div>

                    <div class="pl-summary-footer text-success">
                        Income Generated
                    </div>

                </div>
            </div>

            <!-- COGS -->
            <div class="col-6 col-md-3">
                <div class="pl-summary-card cogs">

                    <div class="pl-summary-top">
                        <div class="pl-summary-icon">
                            <i class="fa fa-box"></i>
                        </div>

                        <div class="pl-summary-label">
                            COGS
                        </div>
                    </div>

                    <div class="pl-summary-value">
                        ${formatINR2(totalCOGS)}
                    </div>

                    <div class="pl-summary-footer text-warning">
                        Direct Costs
                    </div>

                </div>
            </div>

            <!-- Expenses -->
            <div class="col-6 col-md-3">
                <div class="pl-summary-card expense">

                    <div class="pl-summary-top">
                        <div class="pl-summary-icon">
                            <i class="fa fa-wallet"></i>
                        </div>

                        <div class="pl-summary-label">
                            Expenses
                        </div>
                    </div>

                    <div class="pl-summary-value">
                        ${formatINR2(totalExpense)}
                    </div>

                    <div class="pl-summary-footer text-danger">
                        Operating Costs
                    </div>

                </div>
            </div>

            <!-- Net -->
            <div class="col-6 col-md-3">
                <div class="pl-summary-card ${isProfit ? 'profit' : 'loss'}">

                    <div class="pl-summary-top">
                        <div class="pl-summary-icon">
                            <i class="fa fa-chart-line"></i>
                        </div>

                        <div class="pl-summary-label">
                            Net P&L
                        </div>
                    </div>

                    <div class="pl-summary-value">
                        ${formatINR2(Math.abs(netPL))}
                    </div>

                    <div class=" pl-summary-footer ${isProfit ? 'text-success' : 'text-danger'}">
                        ${isProfit ? 'Net Profit' : 'Net Loss'}
                    </div>

                </div>
            </div>

        </div>

        <!-- REVENUE -->
        <div class="card shadow-sm border-0 mb-4">

            <div class="card-header bg-success-subtle">
                <div class="fw-bold">
                    Revenue
                </div>
            </div>

            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                    <thead class="table-light">
                        <tr>
                            <th style="width:70px;">#</th>
                            <th>Description</th>
                            <th class="text-end pe-2">Amount</th>
                        </tr>
                    </thead>

                    <tbody>`;

                    revenue.forEach((r, i) => {
                        html += `
                            <tr>
                                <td class="text-muted text-center ps-2">
                                    ${i + 1}
                                </td>

                                <td>
                                    ${toTitleCase(r.Description)}
                                </td>

                                <td class="text-end fw-semibold pe-2">
                                    ${formatINR2(r.Amount)}
                                </td>
                            </tr>`;
                    });

                    html += `
                    </tbody>

                    <tfoot class="table-success">
                        <tr>
                            <th colspan="2">
                                Total Revenue
                            </th>

                            <th class="text-end">
                                ${formatINR2(totalRevenue)}
                            </th>
                        </tr>
                    </tfoot>

                </table>
            </div>
        </div>


        <!-- COGS -->
        <div class="card shadow-sm border-0 mb-4">

            <div class="card-header bg-warning-subtle">
                <div class="fw-bold">
                    Cost of Goods Sold (COGS)
                </div>
            </div>

            <div class="table-responsive">

                <table class="table table-hover align-middle mb-0">

                    <thead class="table-light">
                        <tr>
                            <th style="width:70px;">#</th>
                            <th>Description</th>
                            <th class="text-end pe-2">Amount</th>
                        </tr>
                    </thead>

                    <tbody>`;

                    cogs.forEach((c, i) => {
                        html += `
                            <tr>
                                <td class="text-muted text-center ps-2">
                                    ${i + 1}
                                </td>

                                <td>
                                    ${toTitleCase(c.Description)}
                                </td>

                                <td class="text-end fw-semibold pe-2">
                                    ${formatINR2(c.Amount)}
                                </td>
                            </tr>`;
                    });

                    html += `
                    </tbody>

                    <tfoot class="table-warning">
                        <tr>
                            <th colspan="2">
                                Total COGS
                            </th>

                            <th class="text-end">
                                ${formatINR2(totalCOGS)}
                            </th>
                        </tr>
                    </tfoot>

                </table>
            </div>
        </div>


        <!-- GROSS PROFIT -->
        <div class="alert alert-info border-0 shadow-sm mb-4">

            <div class="d-flex justify-content-between align-items-center">

                <div class="fw-bold">
                    Gross Profit
                </div>

                <div class="fw-bold fs-5">
                    ${formatINR2(grossProfit)}
                </div>
            </div>
        </div>


        <!-- EXPENSES -->
        <div class="card shadow-sm border-0 mb-4">

            <div class="card-header bg-danger-subtle">
                <div class="fw-bold">
                    Operating Expenses
                </div>
            </div>

            <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">

                    <thead class="table-light">
                        <tr>
                            <th style="width:70px;">#</th>
                            <th>Description</th>
                            <th class="text-end pe-2">Amount</th>
                        </tr>
                    </thead>

                    <tbody>`;

                    expenses.forEach((e, i) => {
                        html += `
                            <tr>
                                <td class="text-muted text-center ps-2">
                                    ${i + 1}
                                </td>

                                <td>
                                    ${toTitleCase(e.Description)}
                                </td>

                                <td class="text-end fw-semibold pe-2">
                                    ${formatINR2(e.Amount)}
                                </td>
                            </tr>`;
                    });

                    html += `
                    </tbody>
                    <tfoot class="table-danger">
                        <tr>
                            <th colspan="2">
                                Total Expenses
                            </th>

                            <th class="text-end">
                                ${formatINR2(totalExpense)}
                            </th>
                        </tr>
                    </tfoot>

                </table>
            </div>
        </div>


        <!-- FINAL NET P&L -->
        <div class="card border-0 shadow-lg ${isProfit ? 'bg-success-subtle' : 'bg-danger-subtle'}">
            <div class="card-body py-4">
                <div class="row align-items-center">
                    <div class="col-md-6">
                        <div class="small text-muted mb-1">
                            Final Result
                        </div>

                        <div class="fw-bold fs-3">
                            Net ${isProfit ? 'Profit' : 'Loss'}
                        </div>
                    </div>

                    <div class="col-md-6 text-md-end">
                        <div class="fw-bold display-6 ${isProfit ? 'text-success' : 'text-danger'}">
                            ${formatINR2(Math.abs(netPL))}
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>`;

    $("#tableMain").html(html);
}

function renderProfitLossStatement2(data) {

    if (!data) {
        $("#tableMain").html(`<div class="alert alert-warning">No data found</div>`);
        return;
    }

    const revenue = data.Revenue || [];
    const cogs = data.COGS || [];
    const expenses = data.Expenses || [];

    const totalRevenue = formatINR2(data.TotalRevenue || 0);
    const netPL = data.NetProfitLoss || 0;

    let html = `
    <div class="pl-container">

        <!-- (1) REVENUE -->
        <table class="table table-bordered mb-0">
            <tr class="pl-section-header">
                <th colspan="3">(1) Revenue</th>
            </tr>`;

    revenue.forEach((r, i) => {
        html += `
        <tr>
            <td style="width:50px;" class="text-center">${i + 1}</td>
            <td>${toTitleCase(r.Description)}</td>
            <td class="text-end">${formatINR2(r.Amount)}</td>
        </tr>`;
    });

    html += `
        <tr class="pl-total-row">
            <td colspan="2" class="fw-bold text-success text-center">Total Revenue</td>
            <td class="text-end fw-bold text-success">${totalRevenue}</td>
        </tr>
        </table>


        <!-- (2) COGS -->
        <table class="table table-bordered mb-0 mt-2">
            <tr class="pl-section-header">
                <th colspan="3">(2) COGS</th>
            </tr>`;

    cogs.forEach((c, i) => {
        html += `
        <tr>
            <td style="width:50px;" class="text-center">${i + 1}</td>
            <td>${toTitleCase(c.Description)}</td>
            <td class="text-end">${formatINR2(c.Amount)}</td>
        </tr>`;
    });

    html += `</table>`;


    /* ========= EXPENSE TABLE ========= */
    html += `
        <table class="table table-bordered mt-2">
            <tr class="pl-section-header">
                <th colspan="3">(3) Operating Expenses</th>
            </tr>
            <tr class="table-light fw-semibold">
                <th class="fs-9" style="width:80px;font-weight:700;">Sr. No.</th>
                <th class="fs-9" style="font-weight:700">Category Name</th>
                <th class="fs-9 text-end" style="font-weight:700">Amount</th>
            </tr>`;

    /* Expense Rows */
    expenses.forEach((e, i) => {
        html += `
        <tr>
            <td class="text-center">${i + 1}</td>
            <td>${toTitleCase(e.Description)}</td>
            <td class="text-end">${formatINR2(e.Amount)}</td>
        </tr>`;
    });


    /* ========= NET P&L (INSIDE SAME TABLE) ========= */

    const isProfit = netPL > 0;
    const isLoss = netPL < 0;

    let rowClass = "table-secondary";
    let textClass = "text-dark";

    if (isProfit) {
        rowClass = "table-success";
        textClass = "text-success";
    }
    else if (isLoss) {
        rowClass = "table-danger";
        textClass = "text-danger";
    }

    /* Net Row */
    html += `
        <tr class="${rowClass}">
            <td colspan="2" class="fw-bold text-center">
                Net P&amp;L
            </td>
            <td class="fw-bold text-end ${textClass}">
                ${formatINR2(Math.abs(netPL))}
            </td>
        </tr>
    </table>
    </div>`;

    $("#tableMain").html(html);
}

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewProfitLossReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const filterDuration = $("#quickRange").val() || null;
    const fromDate = $("#fromDate").val() || null;
    const toDate = $("#toDate").val() || null;

    const payload = {
        FilterDuration: filterDuration,
        FromDate: fromDate,
        ToDate: toDate,
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

            //Swal.close();

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
            //Swal.close();
            handleApiError(xhr, status, error, "Download P&L Report Failed");
            Swal.fire("Error", "Failed to generate report. Please try again.", "error");
        },
        complete: function(){
            hideTempLoader();
            //Swal.close();
        }
    });
}


