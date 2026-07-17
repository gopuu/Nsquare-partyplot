let selectedMonthDuration = "thismonth";
const HEADER_KEY = "cashflowPrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadCashflowGrid();
    loadAccountDropdown();
    loadPaymentModeDropdown();

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

    $("#fromDate, #toDate, #filter_transaction_account, #filter_transaction_paymentmode, #filter_voucher_invoice_no, input[name='filter_transaction_type']").on("change", function () {
        // If user manually edits date → switch dropdown to custom
        if ($("#quickRange").val() !== "customrange") {
            $("#quickRange").val("customrange");
            // Optional: update variable also
            selectedMonthDuration = "customrange";
        }
        loadCashflowGrid();
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

function loadCashflowGrid() {

    // 🔹 TYPE (Radio → INT mapping)
    const txnTypeVal = $("input[name='filter_transaction_type']:checked").val();

    let txnType = 0; // default ALL
    if (txnTypeVal === "INCOME") txnType = 1;
    else if (txnTypeVal === "EXPENSE") txnType = 2;

    // 🔹 Duration
    const filterDuration = $("#quickRange").val() || null;

    // 🔹 Dates
    const fromDate = $("#fromDate").val() || null;
    const toDate = $("#toDate").val() || null;

    // 🔹 Account / Payment
    const accountId = $("#filter_transaction_account").val() || 0;
    const paymentModeId = $("#filter_transaction_paymentmode").val() || 0;

    // 🔹 Search fields
    const voucherNo_InvoiceNo = $("#filter_voucher_invoice_no").val()?.trim() || null;

    const payload = {
        FromDate: fromDate,
        ToDate: toDate,
        FilterDuration: filterDuration,

        TxnType: parseInt(txnType),
        PaymentModeId: parseInt(paymentModeId),
        AccountId: parseInt(accountId),

        VoucherNo_InvoiceNo: voucherNo_InvoiceNo,
    };

    $("#tableMain").html(`<div class="text-center py-5">Loading...</div>`);
    $("#cashflowSummarySection").hide();

    $.ajax({
        url: "/api/Report_Cashflow/GetCashflowGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            renderCashflowTable(res.data.LineItems, res.data.Summary);
            renderCashflowSummary(res.data.Summary);
        },
        error: function () {
            $("#tableMain").html(`<div class="alert alert-danger">Failed to load data</div>`);
        }
    });
}

function renderCashflowTable(data, summary) {

    $("#tableMain").html(`
        <table id="tblCashflow" class="table table-bordered table-sm w-100">
            <tfoot>
                <tr id="tblFooter"></tr>
            </tfoot>
        </table>
    `);

    if ($.fn.DataTable.isDataTable("#tblCashflow")) {
        $("#tblCashflow").DataTable().clear().destroy();
    }

    const table = $("#tblCashflow").DataTable({
        data: data,

        paging: false,
        scrollCollapse: false,
        scrollX: true,
        //pageLength: 25,

        order: [[1, "asc"]],

        columns: [
            { data: "SrNo", title: "#", className: "text-center", width: "50px" },

            {
                data: "TxnDate",
                title: "Date",
                className: "text-center",
                render: (d, t, r) => t === "display" ? r.TxnDate_Format : d
            },

            {
                data: "FlowType",
                title: "Type",
                className: "text-center",
                render: d => {
                    const cls = d === "Inflow" ? "success" : "danger";
                    return `<span class="badge bg-${cls}">${d}</span>`;
                }
            },

            { data: "TxnType", className: "text-center", title: "Txn Type" },
            { data: "Description", title: "Description" },
            { data: "VoucherNo", className: "text-center", title: "Voucher" },
            { data: "InvoiceNo", className: "text-center", title: "Invoice" },
            { data: "SourceName", className: "text-center", title: "Source" },
            { data: "ReceiverName", className: "text-center", title: "Receiver" },
            { data: "PaymentModeName", className: "text-center", title: "Payment Mode" },
            {
                data: "Notes",
                title: "Notes",
                className: "text-center",
                render: function (data, type, row) {
                    if (!data) return "";

                    if (data.length > 10) {
                        return `<span class="custom-tooltip-notes" title="${$('<div>').text(data).html()}">
                                    ${data.substring(0, 10)}...
                                </span>`;
                    }

                    return data;
                }
            },

            {
                data: "Amount",
                title: "Amount",
                className: "text-end fw-semibold",
                render: (d, t, r) => {
                    const cls = r.FlowType === "Inflow" ? "text-success" : "text-danger";
                    const sign = r.FlowType === "Inflow" ? "+" : "-";
                    return `<span class="${cls}">${sign} ${formatINR2(d)}</span>`;
                }
            }
        ],

        createdRow: function (row, data) {
            $(row).addClass(data.FlowType === "Inflow" ? "table-success" : "table-danger")
                .css("opacity", "0.85");
        },

        initComplete: function () {

            // 🔥 Footer from API summary (NOT recalculated)
            if (!summary) return;

            const footer = `
                <th colspan="11" class="text-end">TOTAL</th>
                <th class="text-end">
                    <div class="text-success">▲ ${formatINR2(summary.TotalInflow)}</div>
                    <div class="text-danger">▼ ${formatINR2(summary.TotalOutflow)}</div>
                    <div class="fw-bold">Net: ${formatINR2(summary.NetCashflow)}</div>
                </th>
            `;

            $("#tblFooter").html(footer);
        }
    });
}

function renderCashflowSummary(summary) {

    if (!summary) {
        $("#cashflowSummarySection").hide();
        return;
    }

    const totalInflow = parseFloat(summary.TotalInflow) || 0;
    const totalOutflow = parseFloat(summary.TotalOutflow) || 0;
    const netCashflow = parseFloat(summary.NetCashflow) || 0;

    const isPositive = netCashflow >= 0;

    $("#cashflowSummaryCards").html(`
        <div class="col-md-4">
            <div class="card border-success">
                <div class="card-body">
                    <div class="text-muted small">Total Inflow</div>
                    <div class="fw-bold text-success">${formatINR2(totalInflow)}</div>
                </div>
            </div>
        </div>

        <div class="col-md-4">
            <div class="card border-danger">
                <div class="card-body">
                    <div class="text-muted small">Total Outflow</div>
                    <div class="fw-bold text-danger">${formatINR2(totalOutflow)}</div>
                </div>
            </div>
        </div>

        <div class="col-md-4">
            <div class="card ${isPositive ? "border-success" : "border-danger"}">
                <div class="card-body">
                    <div class="text-muted small">Net Cashflow</div>
                    <div class="fw-bold ${isPositive ? "text-success" : "text-danger"}">
                        ${formatINR2(netCashflow)}
                    </div>
                </div>
            </div>
        </div>
    `);

    $("#cashflowSummarySection").show();
}

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewCashflowReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    // 🔹 TYPE (Radio → INT mapping)
    const txnTypeVal = $("input[name='filter_transaction_type']:checked").val();

    let txnType = 0; // default ALL
    if (txnTypeVal === "INCOME") txnType = 1;
    else if (txnTypeVal === "EXPENSE") txnType = 2;

    // 🔹 Duration
    const filterDuration = $("#quickRange").val() || null;

    // 🔹 Dates
    const fromDate = $("#fromDate").val() || null;
    const toDate = $("#toDate").val() || null;

    // 🔹 Account / Payment
    const accountId = $("#filter_transaction_account").val() || 0;
    const paymentModeId = $("#filter_transaction_paymentmode").val() || 0;

    // 🔹 Search fields
    const voucherNo_InvoiceNo = $("#filter_voucher_invoice_no").val()?.trim() || null;

    const payload = {
        FromDate: fromDate,
        ToDate: toDate,
        FilterDuration: filterDuration,

        TxnType: parseInt(txnType),
        PaymentModeId: parseInt(paymentModeId),
        AccountId: parseInt(accountId),

        VoucherNo_InvoiceNo: voucherNo_InvoiceNo,

        WithHeader: withHeader,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_Cashflow/DownloadCashflowReport_Excel"
        : "/api/Report_Cashflow/DownloadCashflowReport_Pdf";

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

// Load Account dropdown dynamically
function loadAccountDropdown(selectedId = "") {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
        success: function (response) {
            let dropdown = $(".ddlAccount");
            dropdown.empty(); // clear previous values
            dropdown.append('<option value="0">--Select--</option>');

            if (response.status === "Success" && response.data.length > 0) {
                response.data.forEach(item => {
                    dropdown.append(
                        `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                                ${item.AccountName}
                        </option>`
                    );
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load account dropdown data");
        }
    });
}
function loadPaymentModeDropdown() {
    const payload = {
        Id: 0
    };
    $.ajax({
        url: "/api/PaymentMode/GetPaymentMode",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("payment mode res:", res);
            const ddl = $(".ddlPaymentMode");
            ddl.empty().append('<option value="0">--Select--</option>');

            if (res?.status === "Success" && Array.isArray(res.data)) {
                res.data
                    .filter(x => x.IsActive)
                    .forEach(pm => {
                        ddl.append(`<option value="${pm.Id}">${pm.PaymentModeName}</option>
                        `);
                    });
            } else {
                Swal.fire("Error", res?.message || "Failed to load payment modes!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Payment Mode Dropdown Error");
        }
    });
}


