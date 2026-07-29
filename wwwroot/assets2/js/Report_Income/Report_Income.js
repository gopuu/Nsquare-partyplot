/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "MonthName", label: "Month" },
    { key: "CategoryName", label: "Income Category" },
    { key: "Amount", label: "Amount" }
];
let selectedColumns = REPORT_COLUMNS.map(c => c.key);

/* ═══════════════════════════════════════════════════
   DETAIL REPORT COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const DETAIL_REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr. No." },
    { key: "VoucherNo", label: "Voucher No" },
    { key: "TransactionDate", label: "Date" },
    { key: "AccountName", label: "Account" },
    { key: "CategoryName", label: "Category" },
    { key: "SubCategoryName", label: "Sub-Category" },
    { key: "Notes", label: "Notes" },
    { key: "PaymentModeName", label: "Payment Mode" },
    { key: "Amount", label: "Amount" }
];

let selectedDetailColumns = DETAIL_REPORT_COLUMNS.map(c => c.key);

/* ═══════════════════════════════════════════════════
   OTHER SETTINGS
═══════════════════════════════════════════════════ */
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "incomesPrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {
    loadAccountDropdown();
    loadPaymentModeDropdown();

    loadCategoryDropdown();
    resetGrid(); //loadIncomeGrid();
    loadColumnCheckboxes();

    /* ── Category change → reload sub category ── */
    $("#ddlCategory").on("change", function () {
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
    });

    // ─────────────────────────────────────────────
    // MONTH CHANGE
    // ─────────────────────────────────────────────
    $("#incomeMonth").on("change", function () {
        updateIncomeReportTitle();
        loadIncomeGrid();
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
        loadIncomeGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

function resetGrid(){
    const now = new Date();
    const currentMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}`;
    // Reset month to current month
    $("#incomeMonth").val(currentMonth);

    // Reset filters
    $("#ddlCategory").val("0").trigger("change");

    // Update title
    updateIncomeReportTitle();

    // Reload grid
    loadIncomeGrid();
}

// ─────────────────────────────────────────────
// UPDATE REPORT TITLE
// ─────────────────────────────────────────────
function updateIncomeReportTitle() {
    const selectedMonth = $("#incomeMonth").val(); // yyyy-MM
    if (!selectedMonth) {
        $("#incomeReportTitle").text("Income Report");
        return;
    }

    const [year, month] = selectedMonth.split("-");
    const date = new Date(
        parseInt(year),
        parseInt(month) - 1,
        1
    );

    const monthYear = date.toLocaleString("en-US", {
        month: "long",
        year: "numeric"
    });

    $("#incomeReportTitle").text(
        `Income Report - ${monthYear}`
    );
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

function loadSubCategoryDropdown(categoryId = 0, selectedId = 0) {
    const dd = $("#ddlDetailSubCategory");
    dd.prop("disabled", true).empty().append(`<option value="0">-- All --</option>`);

    $.ajax({
        url: "/api/IncomeSubCategory/GetIncomeSubCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, CategoryId: categoryId }),
        success: function (res) {
            if (res.status === "Success" && res.data.length > 0) {
                res.data.forEach(item => {
                    dd.append(`<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>${item.SubCategoryName}</option>`);
                });
                dd.prop("disabled", false);
            } else {
                dd.prop("disabled", false);
            }
        },
        error: (xhr, s, e) => console.error("Sub category dropdown failed", e)
    });
}

function loadAccountDropdown(selectedId = 0) {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
        success: function (res) {
            const dd = $("#ddlDetailAccount");
            dd.empty().append(`<option value="0">-- All --</option>`);
            if (res.status === "Success" && res.data.length > 0) {
                res.data.forEach(item => {
                    dd.append(`<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>${item.AccountName}</option>`);
                });
            }
        },
        error: (xhr, s, e) => handleApiError(xhr, s, e, "Account dropdown failed")
    });
}

function loadPaymentModeDropdown(selectedId = 0) {
    const payload = {
        Id: 0
    };
    $.ajax({
        url: "/api/PaymentMode/GetPaymentMode",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            const dd = $("#ddlDetailPaymentMode");
            dd.empty().append(`<option value="0">-- All --</option>`);
            if (res.status === "Success" && res.data.length > 0) {
                res.data.forEach(item => {
                    dd.append(`<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>${item.PaymentModeName}</option>`);
                });
            }
        },
        error: (xhr, s, e) => console.error("Payment mode dropdown failed", e)
    });
}


/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadIncomeGrid() {

    const payload = {
        Month: $("#incomeMonth").val() || null, // yyyy-MM
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
        url: "/api/Report_Income/GetIncomeSummaryReportGrid",
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
                className: "text-end fw-semibold text-danger",
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
            $cells.eq(1).html(`<strong>TOTAL EXPENSE</strong>`);
            // Amount column
            $cells.eq(2).html(`<strong class="text-danger">${formatINR2(totals?.TotalAmount || 0)}</strong>`);
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

    const payload = {
        Month: $("#incomeMonth").val() || null, // yyyy-MM
        CategoryId: parseInt($("#ddlCategory").val()) || 0,

        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_Income/DownloadIncomeSummaryReport_Excel"
        : "/api/Report_Income/DownloadIncomeSummaryReport_Pdf";

    showTempLoader("Generating Report...");

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
            link.download = `Income_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            //Swal.close();
            handleApiError(xhr, status, error, "Download Income Report Failed");
            Swal.fire("Error", "Failed to generate report. Please try again.", "error");
        },
        complete: function(){
            hideTempLoader();
            //Swal.close();
        }
    });
}

/* ═══════════════════════════════════════════════════
   HELPERS
═══════════════════════════════════════════════════ */
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

// =============================================
// EXPENSE DETAIL STATE
// =============================================
let incomeDetailCategoryId = 0;
let incomeDetailMonth = "";
let incomeDetailCategoryName = "";

let incomeDetailRows = [];
let incomeDetailTotals = {};

let incomeDetailDataLoaded = false;
let incomeDetailAjaxRunning = false;

// true only after Bootstrap modal is fully opened
let incomeDetailModalReady = false;


$(document).on("click", "#tblIncomeSummary .income-amount-link", function () {
    incomeDetailCategoryId = parseInt($(this).data("category-id")) || 0;
    incomeDetailMonth = $(this).data("month") || "";
    incomeDetailCategoryName = $(this).data("category-name") || "-";

    console.log("CategoryId:", incomeDetailCategoryId);
    console.log("Month:", incomeDetailMonth);

    // Heading
    $("#detailMonthText").text(formatIncomeMonth(incomeDetailMonth));
    $("#detailCategoryText").text(incomeDetailCategoryName);

    // Reset optional detail filters
    $("#txtDetailVoucherNo").val("");

    $("#ddlDetailSubCategory").val("0").trigger("change");
    $("#ddlDetailAccount").val("0").trigger("change");
    $("#ddlDetailPaymentMode").val("0").trigger("change");

    //Load SubCategory
    loadSubCategoryDropdown(incomeDetailCategoryId);

    // -----------------------------------------
    // Reset grid state
    // -----------------------------------------

    incomeDetailRows = [];
    incomeDetailTotals = {};

    incomeDetailModalReady = false;
    incomeDetailDataLoaded = false;

    showIncomeDetailLoader();

    const modal = bootstrap.Modal.getOrCreateInstance( document.getElementById("incomeDetailModal") );

    modal.show();
    loadIncomeDetailGrid();
});

function loadIncomeDetailGrid() {
    const payload = {
        CategoryId: incomeDetailCategoryId,
        Month: incomeDetailMonth || null,
        SubCategoryId: parseInt($("#ddlDetailSubCategory").val()) || 0,
        AccountId: parseInt($("#ddlDetailAccount").val()) || 0,
        PaymentModeId: parseInt($("#ddlDetailPaymentMode").val()) || 0,
        VoucherNo: $("#txtDetailVoucherNo").val()?.trim() || null
    };
    console.log("Income Detail Payload:", payload);

    // Reset
    incomeDetailDataLoaded = false;
    incomeDetailAjaxRunning = true;

    // Destroy previous DataTable
    if ($.fn.DataTable.isDataTable("#tblIncomeDetail")) {
        $("#tblIncomeDetail").DataTable().clear().destroy();
    }
    // Show fetching loader
    showIncomeDetailLoader();

    $.ajax({

        url: "/api/Report_Income/GetIncomeDetailReportGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            incomeDetailAjaxRunning = false;
            if (res.status !== "Success") {
                incomeDetailDataLoaded = false;
                showIncomeDetailError();
                Swal.fire("Error", res.message, "error");
                return;
            }

            incomeDetailRows = res.data?.Rows || [];
            incomeDetailTotals = res.data?.Totals || {};

            incomeDetailDataLoaded = true;

            // Render only when modal has COMPLETELY opened
            if (incomeDetailModalReady) {
                renderIncomeDetailTable(incomeDetailRows, incomeDetailTotals);
            }
        },
        error: function (xhr, status, error) {
            incomeDetailAjaxRunning = false;
            incomeDetailDataLoaded = false;

            showIncomeDetailError();

            handleApiError(xhr, status, error, "Fetch Income Detail Failed");
        }
    });
}

function createIncomeDetailTable() {
    $("#incomeDetailTableMain").html(`
        <table id="tblIncomeDetail" class="table table-striped table-bordered w-100">
            <tfoot>
                <tr>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th></th>

                    <th></th>
                    <th></th>
                    <th></th>
                    <th class="text-end"></th>
                    <th class="text-end"></th>
                </tr>
            </tfoot>
        </table>
    `);
}

function renderIncomeDetailTable(rows, totals) {
    const $table = $("#tblIncomeDetail");

    if (!$table.length) {
        return;
    }

    if ($.fn.DataTable.isDataTable("#tblIncomeDetail")) {
        $table.DataTable().clear().destroy();
    }

    const table = $table.DataTable({
        data: rows,
        responsive: false,
        paging: false,
        autoWidth: true,
        scrollX: false,
        order: [[1, "asc"]],

        columns: [
            {
                data: "SrNo",
                title: "#",
                className: "text-center"
            },
            {
                data: "TransactionDate",
                title: "Date",
                className: "text-center",
                render: function (data, type, row) {
                    if (type === "display") {
                        return row.TransactionDate_Format || data || "-";
                    }

                    return data;
                }
            },
            {
                data: "VoucherNo",
                title: "Voucher No",
                className: "text-center",
                render: function (data, type, row) {
                    if (type === "display") {
                        return `<span class="badge bg-danger">${data || "-"}</span>`;
                    }

                    if (type === "sort" || type === "type") {
                        return row.VoucherNo_Num || 0;
                    }

                    return data;
                }
            },
            {
                data: "AccountName",
                title: "Account"
            },
            {
                data: "CategoryName",
                title: "Category"
            },
            {
                data: "SubCategoryName",
                title: "Sub-Category"
            },
            {
                data: "Notes",
                title: "Notes",
                render: function (data) {
                    if (!data || data === "-") {
                        return `<span class="text-muted">-</span>`;
                    }

                    const text = data.length > 40
                        ? data.substring(0, 40) + "…"
                        : data;

                    return `<span title="${data}">${text}</span>`;
                }
            },
            {
                data: "PaymentModeName",
                title: "Payment Mode",
                className: "text-center"
            },
            {
                data: "Amount",
                title: "Amount",
                className: "text-end fw-semibold text-danger",
                render: function (data, type) {
                    const amount = parseFloat(data) || 0;

                    if (type === "sort" || type === "type") {
                        return amount;
                    }

                    return formatINR2(amount);
                }
            }
        ],

        footerCallback: function (tfoot) {
            const $cells = $(tfoot).find("th");
            $cells.html("");
            $cells.eq(7)
                .html("<strong>TOTAL EXPENSE</strong>")
                .addClass("text-end");

            $cells.eq(8)
                .html(`<strong class="text-danger">${formatINR2(totals?.TotalAmount || 0)}</strong>`)
                .addClass("text-end");
        },

        language: {
            emptyTable: "No income records found."
        },

        initComplete: function () {
            const api = this.api();

            $(api.table().header())
                .find("th")
                .addClass("bg-warning-subtle text-dark");

            api.columns.adjust();
            showIncomeDetailGrid();
        }
    });

    table.columns.adjust();
}

$("#incomeDetailModal").on("shown.bs.modal", function () {
    incomeDetailModalReady = true;

    // API hasn't completed yet
    if (!incomeDetailDataLoaded) {
        return;
    }

    // Initialize DataTable after modal is fully visible
    if (!$.fn.DataTable.isDataTable("#tblIncomeDetail")) {
        renderIncomeDetailTable(incomeDetailRows, incomeDetailTotals);
        return;
    }

    // Already initialized
    $("#tblIncomeDetail").DataTable().columns.adjust();
});

$("#incomeDetailModal").on("hidden.bs.modal", function () {
    incomeDetailModalReady = false;

    if ($.fn.DataTable.isDataTable("#tblIncomeDetail")) {
        $("#tblIncomeDetail").DataTable().clear().destroy();
    }

    incomeDetailRows = [];
    incomeDetailTotals = {};

    incomeDetailDataLoaded = false;
    incomeDetailAjaxRunning = false;

    // Reset UI
    $("#incomeDetailLoader").addClass("d-none");
    $("#incomeDetailError").addClass("d-none");
    $("#incomeDetailGrid").addClass("d-none");
});

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

function showIncomeDetailLoader() {
    $("#incomeDetailError").addClass("d-none");
    $("#incomeDetailGrid").addClass("d-none");

    $("#incomeDetailLoader").removeClass("d-none");
}

function showIncomeDetailGrid() {
    $("#incomeDetailLoader").addClass("d-none");
    $("#incomeDetailError").addClass("d-none");

    $("#incomeDetailGrid").removeClass("d-none");
}

function showIncomeDetailError() {
    $("#incomeDetailLoader").addClass("d-none");
    $("#incomeDetailGrid").addClass("d-none");

    $("#incomeDetailError").removeClass("d-none");
}

$(document).on("click", "#btnApplyIncomeDetail", function () {
    loadIncomeDetailGrid();
});

$(document).on("keydown", "#txtDetailVoucherNo", function (e) {
    if (e.key === "Enter") {
        e.preventDefault();
        loadIncomeDetailGrid();
    }
});

$("#ddlDetailAccount, #ddlDetailSubCategory, #ddlDetailPaymentMode").on("input change", function () {
    loadIncomeDetailGrid();
});

/* ═══════════════════════════════════════════════════
   DOWNLOAD EXPENSE DETAIL PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewIncomeDetailReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId")) || 0;

    const payload = {
        Month: incomeDetailMonth || null,
        CategoryId: incomeDetailCategoryId || 0,

        SubCategoryId: parseInt($("#ddlDetailSubCategory").val()) || 0,
        AccountId: parseInt($("#ddlDetailAccount").val()) || 0,
        PaymentModeId: parseInt($("#ddlDetailPaymentMode").val()) || 0,
        VoucherNo: $("#txtDetailVoucherNo").val()?.trim() || null,

        WithHeader: withHeader,
        Columns: selectedDetailColumns,
        ActionBy: userId
    };

    const isExcel = actionName === "downloadExcel";

    const url = isExcel
        ? "/api/Report_Income/DownloadIncomeDetailReport_Excel"
        : "/api/Report_Income/DownloadIncomeDetailReport_Pdf";

    console.log("Income Detail Report Payload:", payload);

    showTempLoader("Generating Report...");

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("Income Detail Report Response:", res);

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            const ext = isExcel ? "xlsx" : "pdf";
            const ts = new Date().toISOString().replace(/[:.-]/g, "");

            const link = document.createElement("a");

            link.href = res.data.FileUrl;
            link.download = `Income_Detail_Report_${ts}.${ext}`;

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },

        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Download Income Detail Report Failed");
        },

        complete: function () {
            hideTempLoader();
        }
    });
}
// EXPENSE DETAILS -- END



