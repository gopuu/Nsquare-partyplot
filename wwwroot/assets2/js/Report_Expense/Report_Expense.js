/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No" },
    { key: "TransactionDate", label: "Date" },
    { key: "AccountName", label: "Account" },
    { key: "CategoryName", label: "Category" },
    { key: "SubCategoryName", label: "Sub Category" },
    { key: "PaymentModeName", label: "Payment Mode" },
    { key: "Amount", label: "Amount" },
    { key: "Notes", label: "Notes" }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "expensePrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadAccountDropdown();
    loadCategoryDropdown();
    loadPaymentModeDropdown();
    loadSubCategoryDropdown(0);
    loadExpenseGrid();
    loadColumnCheckboxes();

    /* ── Category change → reload sub category ── */
    $("#ddlCategory").on("change", function () {
        loadSubCategoryDropdown($(this).val());
        loadExpenseGrid();
    });

    /* ── Other filter dropdowns ── */
    $("#ddlAccount, #ddlSubCategory, #ddlPaymentMode").on("change", function () {
        loadExpenseGrid();
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
            case "days":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 6);
                toDate = today;
                break;
            case "lastdays":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 29);
                toDate = today;
                break;
            case "yesterday":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 1);
                toDate = new Date(fromDate);
                break;
            case "12months":
                fromDate = new Date(today);
                fromDate.setMonth(today.getMonth() - 12);
                toDate = today;
                break;
        }

        $("#fromDate").val(formatDate2(fromDate));
        $("#toDate").val(formatDate2(toDate));
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

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        $("#ddlAccount").val("0");
        $("#ddlCategory").val("0");
        $("#ddlPaymentMode").val("0");
        loadSubCategoryDropdown(0);
        setCurrentMonthDates();
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
        error: (xhr, s, e) => console.error("Account dropdown failed", e)
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
    const dd = $("#ddlSubCategory");
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
        CategoryId: parseInt($("#ddlCategory").val()) || 0,
        SubCategoryId: parseInt($("#ddlSubCategory").val()) || 0,
        AccountId: parseInt($("#ddlAccount").val()) || 0,
        PaymentModeId: parseInt($("#ddlPaymentMode").val()) || 0,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
    };

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading Expense data...
        </div>
    `);

    $.ajax({
        url: "/api/Report_Expense/GetExpenseReportGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            $("#tableMain").html(`
                <table id="tblExpense" class="table table-striped table-bordered w-100">
                    <tfoot>
                        <tr>
                            <th></th><th></th><th></th><th></th>
                            <th></th><th></th><th></th><th></th>
                        </tr>
                    </tfoot>
                </table>
            `);

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            renderExpenseTable(res.data);
        },
        error: function (xhr, status, error) {
            console.error("Fetch Expense Data Failed", xhr, status, error);
            $("#tableMain").html(`<div class="alert alert-danger">Failed to load data. Please try again.</div>`);
        }
    });
}

/* ═══════════════════════════════════════════════════
   RENDER TABLE
═══════════════════════════════════════════════════ */
function renderExpenseTable(data) {

    if ($.fn.DataTable.isDataTable("#tblExpense")) {
        $("#tblExpense").DataTable().clear().destroy();
    }

    $("#tblExpense").DataTable({
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
                data: "TransactionDate",
                title: "Date",
                width: "110px"
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
                title: "Sub Category"
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
                render: d => formatINR2(d)
            },
            {
                data: "Notes",
                title: "Notes",
                render: function (data) {
                    if (!data || data === "-") return `<span class="text-muted">-</span>`;
                    return `<span title="${data}">${data.length > 40 ? data.substring(0, 40) + "…" : data}</span>`;
                }
            },
            // hidden
            { data: "TransactionId", visible: false },
            { data: "CategoryId", visible: false },
            { data: "SubCategoryId", visible: false }
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
            $cells.eq(2).html("");
            $cells.eq(3).html("");
            $cells.eq(4).html("");
            $cells.eq(5).html("");
            $cells.eq(6).html(`<strong class="text-danger">${formatINR2(sumCol(6))}</strong>`);
            $cells.eq(7).html("");
        },

        language: {
            emptyTable: "No expense records found for the selected period."
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
        CategoryId: parseInt($("#ddlCategory").val()) || 0,
        SubCategoryId: parseInt($("#ddlSubCategory").val()) || 0,
        AccountId: parseInt($("#ddlAccount").val()) || 0,
        PaymentModeId: parseInt($("#ddlPaymentMode").val()) || 0,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration,
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_Expense/DownloadExpenseReport_Excel"
        : "/api/Report_Expense/DownloadExpenseReport";

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
            link.download = `Expense_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            Swal.close();
            console.error("Download Expense Report Failed", xhr, status, error);
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