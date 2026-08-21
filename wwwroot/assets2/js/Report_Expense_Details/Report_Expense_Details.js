/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
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
let selectedColumns = REPORT_COLUMNS.map(c => c.key);

/* ═══════════════════════════════════════════════════
   OTHER SETTINGS
═══════════════════════════════════════════════════ */
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "expensePrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {
    loadAccountDropdown();
    loadPaymentModeDropdown();

    loadCategoryDropdown();
    loadSubCategoryDropdown();

    setCurrentMonthDates();
    resetGrid(); //loadExpenseGrid();
    loadColumnCheckboxes();

    /* ── Category change → reload sub category ── */
    $("#ddlDetailCategory").on("change", function () {
        var categoryId = $(this).val();
        loadSubCategoryDropdown(categoryId);
    });

    $("#fromDate, #toDate, #ddlDetailAccount, #ddlDetailSubCategory, #ddlDetailPaymentMode").on("input change", function () {
        // If user manually edits date → switch dropdown to custom
        if ($("#quickRange").val() !== "customrange") {
            $("#quickRange").val("customrange");
            // Optional: update variable also
            selectedMonthDuration = "customrange";
        }
        loadExpenseDetailsGrid();
    });
    $(document).on("keydown", "#txtDetailVoucherNo", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            loadExpenseDetailsGrid();
        }
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
        loadExpenseDetailsGrid();
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
        loadExpenseDetailsGrid();
    });

    // ─────────────────────────────────────────────
    // RESET FILTER
    // ─────────────────────────────────────────────
    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        setCurrentMonthDates();

        resetGrid();
        loadExpenseDetailsGrid();
    });

    // ─────────────────────────────────────────────
    // MONTH CHANGE
    // ─────────────────────────────────────────────
    $("#expenseMonth").on("change", function () {
        updateExpenseReportTitle();
        loadExpenseDetailsGrid();
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
        loadExpenseDetailsGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

function resetGrid(){
    // Reset filters
    $("#ddlCategory").val("0").trigger("change");

    // Update title
    updateExpenseReportTitle();

    // Reload grid
    loadExpenseDetailsGrid();
}

// ─────────────────────────────────────────────
// UPDATE REPORT TITLE
// ─────────────────────────────────────────────
function updateExpenseReportTitle() {
    const selectedMonth = $("#expenseMonth").val(); // yyyy-MM
    if (!selectedMonth) {
        $("#expenseReportTitle").text("Expense Detail Report");
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
        `Expense Detail Report - ${monthYear}`
    );
}


/* ═══════════════════════════════════════════════════
   DROPDOWNS
═══════════════════════════════════════════════════ */
function loadCategoryDropdown(selectedId = 0) {
    $.ajax({
        url: "/api/ExpenseCategory/GetExpenseCategoryList",
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
   LOAD INCOME DETAIL GRID
═══════════════════════════════════════════════════ */
function loadExpenseDetailsGrid() {

    // 🔹 Duration
    const filterDuration = $("#quickRange").val() || null;
    // 🔹 Dates
    const fromDate = $("#fromDate").val() || null;
    const toDate = $("#toDate").val() || null;

    const payload = {
        FromDate: fromDate,
        ToDate: toDate,
        FilterDuration: filterDuration,

        VoucherNo: $("#txtDetailVoucherNo").val()?.trim() || null,
        CategoryId: parseInt($("#ddlDetailCategory").val()) || 0,
        SubCategoryId: parseInt($("#ddlDetailSubCategory").val()) || 0,
        AccountId: parseInt($("#ddlDetailAccount").val()) || 0,
        PaymentModeId: parseInt($("#ddlDetailPaymentMode").val()) || 0
    };

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading Expense Detail...
        </div>
    `);

    console.log("Expense Detail Payload:", payload);

    $.ajax({
        url: "/api/Report_Expense_Details/GetExpenseDetailReport_Grid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),

        success: function (res) {

            if (res.status !== "Success") {
                $("#tableMain").html("");
                Swal.fire("Error", res.message, "error");
                return;
            }

            $("#tableMain").html(`
                <table id="tblExpenseDetail"
                       class="table table-striped table-bordered w-100">

                    <tfoot class="table-secondary fw-bold">
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

            handleApiError(
                xhr,
                status,
                error,
                "Fetch Expense Detail Failed"
            );

            $("#tableMain").html(`
                <div class="alert alert-danger">
                    Failed to load expense detail. Please try again.
                </div>
            `);
        }
    });
}
/* ═══════════════════════════════════════════════════
   RENDER INCOME DETAIL TABLE
═══════════════════════════════════════════════════ */
function renderExpenseDetailTable(rows, totals) {

    if ($.fn.DataTable.isDataTable("#tblExpenseDetail")) {
        $("#tblExpenseDetail")
            .DataTable()
            .clear()
            .destroy();
    }

    console.log("Expense Detail Rows:", rows);
    console.log("Expense Detail Totals:", totals);


    $("#tblExpenseDetail").DataTable({

        data: rows,

        responsive: true,

        paging: false,

        scrollY: "60vh",
        scrollX: true,
        scrollCollapse: true,

        order: [
            [0, "asc"]
        ],

        columns: [

            // Sr No
            {
                data: "SrNo",
                title: "#",
                width: "70px",
                className: "text-center"
            },

            // Voucher No
            {
                data: "VoucherNo_Num",
                title: "Voucher No",
                width: "120px",
                className: "text-center",
                render: function (data, type, row) {

                    if (type === "sort" || type === "type") {
                        return data || 0;
                    }

                    return row.VoucherNo || "-";
                }
            },

            // Date
            {
                data: "TransactionDate",
                title: "Date",
                width: "120px",
                className: "text-center",
                render: function (data, type, row) {

                    if (type === "sort" || type === "type") {
                        return row.TransactionDate_Format || "";
                    }

                    return row.TransactionDate_Format || data || "-";
                }
            },

            // Account
            {
                data: "AccountName",
                title: "Account",
                className: "text-start",
                render: function (data) {
                    return data || "-";
                }
            },

            // Category
            {
                data: "CategoryName",
                title: "Category",
                className: "text-start",
                render: function (data) {
                    return data || "-";
                }
            },

            // Sub Category
            {
                data: "SubCategoryName",
                title: "Sub Category",
                className: "text-start",
                render: function (data) {
                    return data || "-";
                }
            },

            // Payment Mode
            {
                data: "PaymentModeName",
                title: "Payment Mode",
                className: "text-center",
                render: function (data) {
                    return data || "-";
                }
            },

            // Amount
            {
                data: "Amount",
                title: "Amount",
                width: "150px",
                className: "text-end fw-semibold text-danger",

                render: function (data, type) {

                    const amount = parseFloat(data) || 0;

                    // Keep numeric for sorting
                    if (type === "sort" || type === "type") {
                        return amount;
                    }

                    return formatINR2(amount);
                }
            },

            // Notes
            {
                data: "Notes",
                title: "Notes",
                className: "text-start",
                render: function (data) {

                    if (!data) {
                        return "-";
                    }

                    const text = String(data);
                    const shortText = text.length > 20
                        ? text.substring(0, 20) + "..."
                        : text;

                    if (text.length > 20) {
                        return `
                            <span
                                data-bs-toggle="tooltip"
                                data-bs-placement="top"
                                title="${$('<div>').text(text).html()}">
                                ${$('<div>').text(shortText).html()}
                            </span>
                        `;
                    }

                    return text;
                }
            }

        ],


        /* ═══════════════════════════════════════════
           FOOTER TOTAL
        ═══════════════════════════════════════════ */
        footerCallback: function (tfoot) {

            const $cells = $(tfoot).find("th");

            // Sr No
            $cells.eq(0).html("");

            // Voucher
            $cells.eq(1).html("");

            // Date
            $cells.eq(2).html("");

            // Account
            $cells.eq(3).html("");

            // Category
            $cells.eq(4).html("");

            // Sub Category
            $cells.eq(5).html("");

            // Payment Mode
            $cells.eq(6).html(`
                <strong>TOTAL</strong>
            `);

            // Amount
            $cells.eq(7).html(`
                <strong class="text-danger">
                    ${formatINR2(totals?.TotalAmount || 0)}
                </strong>
            `);

            // Notes
            $cells.eq(8).html("");

            $cells.eq(6).addClass("text-end");
            $cells.eq(7).addClass("text-end fw-semibold");
        },


        /* ═══════════════════════════════════════════
           EMPTY MESSAGE
        ═══════════════════════════════════════════ */
        language: {
            emptyTable: "No expense detail found for the selected filters."
        },


        /* ═══════════════════════════════════════════
           HEADER STYLE
        ═══════════════════════════════════════════ */
        initComplete: function () {

            $(this.api().table().header())
                .find("th")
                .addClass("bg-warning-subtle text-dark");
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


/* ═══════════════════════════════════════════════════
   DOWNLOAD EXPENSE DETAIL PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewExpenseReport(actionName) {

    // 🔹 Duration
    const filterDuration = $("#quickRange").val() || null;
    // 🔹 Dates
    const fromDate = $("#fromDate").val() || null;
    const toDate = $("#toDate").val() || null;

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId")) || 0;

    const payload = {
        FromDate: fromDate,
        ToDate: toDate,
        FilterDuration: filterDuration,

        VoucherNo: $("#txtDetailVoucherNo").val()?.trim() || null,

        CategoryId: parseInt($("#ddlDetailCategory").val()) || 0,
        SubCategoryId: parseInt($("#ddlDetailSubCategory").val()) || 0,

        AccountId: parseInt($("#ddlDetailAccount").val()) || 0,
        PaymentModeId: parseInt($("#ddlDetailPaymentMode").val()) || 0,

        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const isExcel = actionName === "downloadExcel";

    const url = isExcel
        ? "/api/Report_Expense_Details/DownloadExpenseDetailReport_Excel"
        : "/api/Report_Expense_Details/DownloadExpenseDetailReport_Pdf";

    console.log("Expense Detail Report Payload:", payload);
    downloadReport(url, payload, 
        loaderMsg = actionName === "downloadExcel" ? "Generating Expense Detail Excel Report..." : "Generating Expense Detail PDF Report...", 
        failedMsg = "Failed to generate Expense Detail Report");
}

// EXPENSE DETAILS -- END





