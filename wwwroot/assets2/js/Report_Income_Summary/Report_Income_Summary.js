/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "MonthName", label: "Month" },
    { key: "CategoryName", label: "Expense Category" },
    { key: "Amount", label: "Amount" }
];
let selectedColumns = REPORT_COLUMNS.map(c => c.key);

/* ═══════════════════════════════════════════════════
   OTHER SETTINGS
═══════════════════════════════════════════════════ */
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "incomesPrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {
    loadCategoryDropdown();

    resetGrid(); //loadIncomeGrid();
    loadColumnCheckboxes();

    /* ── Category change → reload sub category ── */
    $("#ddlCategory").on("change", function () {
        loadIncomeGrid();
    });
    $("#fromDate, #toDate").on("change", function () {

        // If user manually edits date → switch dropdown to custom
        if ($("#quickRange").val() !== "customrange") {
            $("#quickRange").val("customrange");
            // Optional: update variable also
            selectedMonthDuration = "customrange";
        }

        let dateValue = $(this).val();
        let errorId = this.id === "fromDate" ? "#fromDateError" : "#toDateError";

        if (!dateValue) {
            $(errorId).text("Please select a valid date.");
            return;
        }

        $(errorId).text("");

        loadIncomeGrid();
    });

    /* ── Quick range ── */
    $("#quickRange").on("change", function () {

        const newFilter = $(this).val();
        if (selectedMonthDuration === newFilter) return;
        selectedMonthDuration = newFilter;

        if (selectedMonthDuration === "customrange") {
            return;
        }

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
        loadIncomeGrid();
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
        loadIncomeGrid();
    });

    // ─────────────────────────────────────────────
    // RESET FILTER
    // ─────────────────────────────────────────────
    $("#btnResetFilter").on("click", function () {
        resetGrid();
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        setCurrentMonthDates();
        loadIncomeGrid();
    });

    // ─────────────────────────────────────────────
    // MONTH CHANGE
    // ─────────────────────────────────────────────
    /*$("#incomeMonth").on("change", function () {
        loadIncomeGrid();
    });*/

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
        loadIncomeGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

function resetGrid(){
    $("#quickRange").val("thismonth");
    selectedMonthDuration = "thismonth";
    setCurrentMonthDates();

    //const now = new Date();
    //const currentMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}`;
    // Reset month to current month
    //$("#incomeMonth").val(currentMonth);

    // Reset filters
    $("#ddlCategory").val("0").trigger("change");

    // Reload grid
    loadIncomeGrid();
}

/* ═══════════════════════════════════════════════════
   DROPDOWNS
═══════════════════════════════════════════════════ */
function loadCategoryDropdown(selectedId = 0) {
    $.ajax({
        url: "/api/IncomeCategory/GetIncomeCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            const dd = $("#ddlCategory");
            dd.empty().append(`<option value="0">-- All --</option>`);
            if (res.status === "Success" && res.data.length > 0) {
                res.data.forEach(item => {
                    dd.append(`<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>${item.CategoryName}</option>`);
                });
            }
        },
        error: (xhr, s, e) => console.error("Category dropdown failed", e)
    });
}

/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadIncomeGrid() {

    // 🔹 Duration
    const filterDuration = $("#quickRange").val() || null;
    // 🔹 Dates
    const fromDate = $("#fromDate").val() || null;
    const toDate = $("#toDate").val() || null;

    const payload = {
        FromDate: fromDate,
        ToDate: toDate,
        FilterDuration: filterDuration,
        CategoryId: parseInt($("#ddlCategory").val()) || 0
    };

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading Income Summary...
        </div>
    `);

    console.log("Income Summary Payload:", payload);

    $.ajax({
        url: "/api/Report_Income_Summary/GetIncomeSummaryReport_Grid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),

        success: function (res) {

            if (res.status !== "Success") {
                $("#tableMain").html("");
                Swal.fire("Error", res.message, "error");
                return;
            }

            // Create table only after success
            $("#tableMain").html(`
                <table id="tblIncomeSummary"
                       class="table table-striped table-bordered w-100">

                    <tfoot>
                        <tr>
                            <th></th>
                            <th></th>
                            <th></th>
                        </tr>
                    </tfoot>

                </table>
            `);

            renderIncomeSummaryTable(
                res.data?.Rows || [],
                res.data?.Totals || {}
            );
        },

        error: function (xhr, status, error) {

            handleApiError(
                xhr,
                status,
                error,
                "Fetch Income Summary Failed"
            );

            $("#tableMain").html(`
                <div class="alert alert-danger">
                    Failed to load income summary. Please try again.
                </div>
            `);
        }
    });
}


/* ═══════════════════════════════════════════════════
   RENDER EXPENSE SUMMARY TABLE
═══════════════════════════════════════════════════ */
function renderIncomeSummaryTable(rows, totals) {

    if ($.fn.DataTable.isDataTable("#tblIncomeSummary")) {
        $("#tblIncomeSummary")
            .DataTable()
            .clear()
            .destroy();
    }

    console.log("Income Summary Rows:", rows);
    console.log("Income Summary Totals:", totals);


    $("#tblIncomeSummary").DataTable({

        data: rows,

        responsive: true,

        paging: false,

        scrollY: "60vh",
        scrollX: true,
        scrollCollapse: true,

        // Month first, Category second
        order: [
            [0, "asc"],
            [1, "asc"]
        ],


        /* ─────────────────────────────
           COLUMNS
        ───────────────────────────── */
        columns: [

            // Month
            {
                data: "MonthName", //MonthName
                title: "Month",
                width: "150px",
                className: "text-center"
            },


            // Income Category
            {
                data: "IncomeCategoryName", //IncomeCategoryName
                title: "Income Category",
                className: "text-start",
                render: function (data) {

                    return data || "-";
                }
            },

            // Amount
            {
                data: "Amount",
                title: "Amount",
                width: "180px",
                className: "text-end fw-semibold text-success",
                render: function (data, type, row) {
                    const amount = parseFloat(data) || 0;

                    // Keep numeric for sorting
                    if (type === "sort" || type === "type") {
                        return amount;
                    }

                    return `
                        <span class="income-amount-link"
                              data-category-id="${row.IncomeCategoryId || 0}"
                              data-category-name="${row.IncomeCategoryName || ''}"
                              data-month="${row.MonthName2 || ''}">
                            ${formatINR2(amount)}
                        </span>
                    `;
                }
            }
        ],


        /* ─────────────────────────────
           FOOTER TOTAL
        ───────────────────────────── */
        footerCallback: function (tfoot) {
            const $cells = $(tfoot).find("th");
            // Month column
            $cells.eq(0).html("");
            // Category column
            $cells.eq(1).html(`<strong>TOTAL INCOME</strong>`);
            // Amount column
            $cells.eq(2).html(`<strong class="text-success">${formatINR2(totals?.TotalAmount || 0)}</strong>`);
            $cells.eq(1).addClass("text-end");
            $cells.eq(2).addClass("text-end fw-semibold");
        },

        /* ─────────────────────────────
           EMPTY MESSAGE
        ───────────────────────────── */
        language: {
            emptyTable: "No income summary found for the selected month."
        },

        /* ─────────────────────────────
           HEADER STYLE
        ───────────────────────────── */
        initComplete: function () {
            $(this.api().table().header())
                .find("th")
                .addClass("bg-warning-subtle text-dark");
        }
    });
}

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewIncomeReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    // 🔹 Duration
    const filterDuration = $("#quickRange").val() || null;
    // 🔹 Dates
    const fromDate = $("#fromDate").val() || null;
    const toDate = $("#toDate").val() || null;

    const payload = {
        FromDate: fromDate,
        ToDate: toDate,
        FilterDuration: filterDuration,
        CategoryId: parseInt($("#ddlCategory").val()) || 0,

        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_Income_Summary/DownloadIncomeSummaryReport_Excel"
        : "/api/Report_Income_Summary/DownloadIncomeSummaryReport_Pdf";

    downloadReport(url, payload, 
        loaderMsg = actionName === "downloadExcel" ? "Generating Income Excel Report..." : "Generating Income PDF Report...", 
        failedMsg = "Failed to generate Income Report");
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

function formatIncomeMonth(month) {
    if (!month) return "-";
    const [year, monthNo] = month.split("-");
    const months = [
        "Jan", "Feb", "Mar", "Apr",
        "May", "Jun", "Jul", "Aug",
        "Sep", "Oct", "Nov", "Dec"
    ];

    return `${months[parseInt(monthNo) - 1]}-${year}`;
}



