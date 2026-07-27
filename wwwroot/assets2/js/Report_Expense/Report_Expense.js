/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "MonthName", label: "Month" },
    { key: "CategoryName", label: "Expense Category" },
    { key: "Amount", label: "Amount" }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "expensePrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {
    loadAccountDropdown();
    loadCategoryDropdown();
    loadPaymentModeDropdown();
    resetGrid(); //loadExpenseGrid();
    loadColumnCheckboxes();

    /* ── Category change → reload sub category ── */
    $("#ddlCategory").on("change", function () {
        loadExpenseGrid();
    });

    /* ── Other filter dropdowns ── */
    $("#ddlAccount, #ddlPaymentMode").on("input change", function () {
        loadExpenseGrid();
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
        loadExpenseGrid();
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
    $("#expenseMonth").on("change", function () {
        updateExpenseReportTitle();
        loadExpenseGrid();
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
        loadExpenseGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

function resetGrid(){
    const now = new Date();
    const currentMonth = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}`;
    // Reset month to current month
    $("#expenseMonth").val(currentMonth);

    // Reset filters
    $("#ddlAccount").val("0").trigger("change");
    $("#ddlCategory").val("0").trigger("change");
    $("#ddlPaymentMode").val("0").trigger("change");

    // Update title
    updateExpenseReportTitle();

    // Reload grid
    loadExpenseGrid();

}

// ─────────────────────────────────────────────
// UPDATE REPORT TITLE
// ─────────────────────────────────────────────
function updateExpenseReportTitle() {
    const selectedMonth = $("#expenseMonth").val(); // yyyy-MM
    if (!selectedMonth) {
        $("#expenseReportTitle").text("Expense Report");
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

    $("#expenseReportTitle").text(
        `Expense Report - ${monthYear}`
    );
}


/* ═══════════════════════════════════════════════════
   DROPDOWNS
═══════════════════════════════════════════════════ */
function loadAccountDropdown(selectedId = 0) {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
        success: function (res) {
            const dd = $("#ddlAccount");
            dd.empty().append(`<option value="0">-- All Accounts --</option>`);
            if (res.status === "Success" && res.data.length > 0) {
                res.data.forEach(item => {
                    dd.append(`<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>${item.AccountName}</option>`);
                });
            }
        },
        error: (xhr, s, e) => handleApiError(xhr, s, e, "Account dropdown failed")
    });
}

function loadCategoryDropdown(selectedId = 0) {
    $.ajax({
        url: "/api/ExpenseCategory/GetExpenseCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            const dd = $("#ddlCategory");
            dd.empty().append(`<option value="0">-- All Categories --</option>`);
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
    dd.prop("disabled", true).empty().append(`<option value="0">-- All Sub Categories --</option>`);

    $.ajax({
        url: "/api/ExpenseSubCategory/GetExpenseSubCategoryList",
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
            const dd = $("#ddlPaymentMode");
            dd.empty().append(`<option value="0">-- All Payment Modes --</option>`);
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
function loadExpenseGrid() {

    const payload = {
        Month: $("#expenseMonth").val() || null, // yyyy-MM
        CategoryId: parseInt($("#ddlCategory").val()) || 0,
        AccountId: parseInt($("#ddlAccount").val()) || 0,
        PaymentModeId: parseInt($("#ddlPaymentMode").val()) || 0
    };

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading Expense Summary...
        </div>
    `);

    console.log("Expense Summary Payload:", payload);

    $.ajax({
        url: "/api/Report_Expense/GetExpenseSummaryReportGrid",
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
                <table id="tblExpenseSummary"
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

            renderExpenseSummaryTable(
                res.data?.Rows || [],
                res.data?.Totals || {}
            );
        },

        error: function (xhr, status, error) {

            handleApiError(
                xhr,
                status,
                error,
                "Fetch Expense Summary Failed"
            );

            $("#tableMain").html(`
                <div class="alert alert-danger">
                    Failed to load expense summary. Please try again.
                </div>
            `);
        }
    });
}


/* ═══════════════════════════════════════════════════
   RENDER EXPENSE SUMMARY TABLE
═══════════════════════════════════════════════════ */
function renderExpenseSummaryTable(rows, totals) {

    if ($.fn.DataTable.isDataTable("#tblExpenseSummary")) {
        $("#tblExpenseSummary")
            .DataTable()
            .clear()
            .destroy();
    }

    console.log("Expense Summary Rows:", rows);
    console.log("Expense Summary Totals:", totals);


    $("#tblExpenseSummary").DataTable({

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


            // Expense Category
            {
                data: "ExpenseCategoryName", //ExpenseCategoryName
                title: "Expense Category",
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
                        <span class="expense-amount-link"
                              data-category-id="${row.ExpenseCategoryId || 0}"
                              data-category-name="${row.ExpenseCategoryName || ''}"
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
            emptyTable: "No expense summary found for the selected month."
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
function ViewExpenseReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        Month: $("#expenseMonth").val() || null, // yyyy-MM
        CategoryId: parseInt($("#ddlCategory").val()) || 0,
        AccountId: parseInt($("#ddlAccount").val()) || 0,
        PaymentModeId: parseInt($("#ddlPaymentMode").val()) || 0,

        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_Expense/DownloadExpenseSummaryReport_Excel"
        : "/api/Report_Expense/DownloadExpenseSummaryReport_Pdf";

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
            link.download = `Expense_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            //Swal.close();
            handleApiError(xhr, status, error, "Download Expense Report Failed");
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

// EXPENSE DETAILS -- START 
let expenseDetailCategoryId = 0;
let expenseDetailMonth = "";
let expenseDetailCategoryName = "";

$(document).on("click", "#tblExpenseSummary .expense-amount-link", function () {
    expenseDetailCategoryId = parseInt($(this).data("category-id")) || 0;
    expenseDetailMonth = $(this).data("month") || "";
    expenseDetailCategoryName = $(this).data("category-name") || "-";

    console.log("CategoryId:", expenseDetailCategoryId);
    console.log("Month:", expenseDetailMonth);

    // Heading
    $("#detailMonthText").text(formatExpenseMonth(expenseDetailMonth));
    $("#detailCategoryText").text(expenseDetailCategoryName);

    // Reset optional detail filters
    $("#txtDetailVoucherNo").val("");

    $("#ddlDetailSubCategory").val("0").trigger("change");
    $("#ddlDetailAccount").val("0").trigger("change");
    $("#ddlDetailPaymentMode").val("0").trigger("change");

    // Open modal
    const modal = bootstrap.Modal.getOrCreateInstance(
        document.getElementById("expenseDetailModal")
    );

    modal.show();
    
    //Load SubCategory
    loadSubCategoryDropdown(expenseDetailCategoryId);

    // Load detail
    loadExpenseDetailGrid();
});

function formatExpenseMonth(month) {
    if (!month) return "-";
    const [year, monthNo] = month.split("-");
    const months = [
        "Jan", "Feb", "Mar", "Apr",
        "May", "Jun", "Jul", "Aug",
        "Sep", "Oct", "Nov", "Dec"
    ];

    return `${months[parseInt(monthNo) - 1]}-${year}`;
}

function loadExpenseDetailGrid() {

    const payload = {
        // From clicked Summary row
        CategoryId: expenseDetailCategoryId,
        Month: expenseDetailMonth || null,

        // Optional modal filters
        SubCategoryId: parseInt($("#ddlDetailSubCategory").val()) || 0,
        AccountId: parseInt($("#ddlDetailAccount").val()) || 0,
        PaymentModeId: parseInt($("#ddlDetailPaymentMode").val()) || 0,
        VoucherNo: $("#txtDetailVoucherNo").val()?.trim() || null    
    };
    console.log("Expense Detail Payload:", payload);

    $("#expenseDetailTableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2"
                 role="status"></div>
            Loading Expense Details...
        </div>
    `);


    $.ajax({
        url: "/api/Report_Expense/GetExpenseReportGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            if (res.status !== "Success") {
                $("#expenseDetailTableMain").html("");
                Swal.fire("Error", res.message, "error");
                return;
            }


            $("#expenseDetailTableMain").html(`
                <table id="tblExpenseDetail"
                       class="table table-striped table-bordered w-100">

                    <tfoot>
                        <tr>
                            <th></th>
                            <th></th>
                            <th></th>
                            <th></th>
                            <th></th>
                            <th></th>
                            <th></th>
                            <th></th>
                            <th></th>
                        </tr>
                    </tfoot>

                </table>
            `);

            renderExpenseDetailTable(
                res.data?.Rows || [],
                res.data?.Totals || {}
            );
        },

        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Expense Detail Failed");

            $("#expenseDetailTableMain").html(`
                <div class="alert alert-danger">
                    Failed to load expense details.
                    Please try again.
                </div>
            `);
        }
    });
}
function renderExpenseDetailTable(rows, totals) {

    if ($.fn.DataTable.isDataTable("#tblExpenseDetail")) {
        $("#tblExpenseDetail").DataTable().clear().destroy();
    }

    $("#tblExpenseDetail").DataTable({
        data: rows,
        responsive: true,
        paging: false,

        scrollX: true,
        scrollY: "55vh",
        scrollCollapse: true,

        order: [[1, "asc"]],
        columns: [

            {
                data: "SrNo",
                title: "#",
                width: "50px",
                className: "text-center"
            },


            // Date
            {
                data: "TransactionDate",
                title: "Date",
                width: "110px",
                className: "text-center",
                render: function (data, type, row) {
                    if (type === "display") {
                        return row.TransactionDate_Format || data || "-";
                    }
                    return data;
                }
            },

            // Voucher
            {
                data: "VoucherNo",
                title: "Voucher No",
                className: "text-center",
                render: function (data, type, row) {
                    if (type === "display") {
                        return `
                            <span class="badge bg-danger">
                                ${data || "-"}
                            </span>
                        `;
                    }

                    if (type === "sort" || type === "type") {
                        return row.VoucherNo_Num || 0;
                    }
                    return data;
                }
            },

            // Account
            {
                data: "AccountName",
                title: "Account"
            },

            // Category
            {
                data: "CategoryName",
                title: "Expense Category"
            },

            // Sub Category
            {
                data: "SubCategoryName",
                title: "Expense Sub-Category"
            },

            // Notes
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

            // Payment Mode
            {
                data: "PaymentModeName",
                title: "Payment Mode",
                className: "text-center"
            },

            // Amount
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

        // ── Footer ──
        footerCallback: function (tfoot) {
            const $cells = $(tfoot).find("th");
            $cells.eq(7).html(`<strong>TOTAL EXPENSE</strong>`);
            $cells.eq(8).html(`
                <strong class="text-danger">
                    ${formatINR2(totals?.TotalAmount || 0)}
                </strong>
            `);
            $cells.eq(7).addClass("text-end");
            $cells.eq(8).addClass("text-end");
        },


        language: {
            emptyTable:
                "No expense records found."
        },

        initComplete: function () {
            $(this.api().table().header())
                .find("th")
                .addClass(
                    "bg-warning-subtle text-dark"
                );
        }
    });
}
$(document).on("click", "#btnApplyExpenseDetail", function () {
    loadExpenseDetailGrid();
});

$(document).on("keydown", "#txtDetailVoucherNo", function (e) {
    if (e.key === "Enter") {
        e.preventDefault();
        loadExpenseDetailGrid();
    }
});

$("#ddlDetailAccount, #ddlDetailSubCategory, #ddlDetailPaymentMode, #txtDetailVoucherNo").on("input change", function () {
    loadExpenseDetailGrid();
});

// EXPENSE DETAILS -- END



