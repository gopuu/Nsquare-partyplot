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

function toTitleCase(str) {
    return (str || "").toLowerCase().replace(/\b\w/g, c => c.toUpperCase());
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

                months.forEach((m,index) => {

                    let option1 = $("<option>");
                    option1.val(m.MonthId);
                    option1.text(m.MonthName);

                    let option2 = $("<option>");
                    option2.val(m.MonthId);
                    option2.text(m.MonthName);

                    // ✅ Select first month in FROM dropdown
                    if (index === 0) {
                        option1.prop("selected", true);
                    }

                    // ✅ Select last month in TO dropdown
                    if (index === months.length - 1) {
                        option2.prop("selected", true);
                    }

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


