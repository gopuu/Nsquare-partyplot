$(document).ready(function () {

    // 🔹 Initial load (no filters)
    fetchPurchaseTransactions();
    loadSupplierDropdown();
    loadAccountDropdown();
    loadPaymentModeDropdown();

    // ================= FILTER UI HANDLING =================
    $(document).on("change", "#ddlDuration", function () {
        let val = $(this).val();

        if (val === "customrange") {
            $("#customDateRange").removeClass("d-none");
        } else {
            $("#customDateRange").addClass("d-none");
            $("#fromDate, #toDate").val("");
        }
    });

    // APPLY FILTERS
    $("#btnApplyFilters").on("click", function () {
        fetchPurchaseTransactions(getFilters());
    });

    // RESET FILTERS
    $("#btnResetFilters").on("click", function () {
        resetFilters();
        fetchPurchaseTransactions(); // reload default
    });

});

function getFilters() {
    return {
        monthDuration: $("#ddlDuration").val(),
        fromDate: $("#fromDate").val(),
        toDate: $("#toDate").val(),

        supplierId: Number($("#ddlSupplier").val() || 0),
        paymentModeId: Number($("#ddlPaymentMode").val() || 0),
        accountId: Number($("#ddlAccount").val() || 0),

        fromAmount: $("#fromAmount").val(),
        toAmount: $("#toAmount").val()
    };
}
function resetFilters() {

    $("#ddlDuration").val("thismonth");
    $("#fromDate, #toDate").val("");
    $("#fromAmount, #toAmount").val("");

    $("#ddlSupplier").val("0");
    $("#ddlPaymentMode").val("0");
    $("#ddlAccount").val("0");

    $("#customDateRange").addClass("d-none");
}

// ================= FETCH DATA =================
function fetchPurchaseTransactions(filters = getFilters()) {

    let payload = {
        PurchaseId: 0,
        MonthDuration: filters?.monthDuration || null,
        FromDate: filters?.fromDate || null,
        ToDate: filters?.toDate || null,
        SupplierId: filters?.supplierId || 0,
        PaymentModeId: filters?.paymentModeId || 0,
        AccountId: filters?.accountId || 0,
        FromAmount: filters?.fromAmount || 0,
        ToAmount: filters?.toAmount || 0
    };

    console.log("API Payload:", payload);

    $.ajax({
        url: "/api/Purchase/GetPurchasePaymentTransactions",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                renderTable(res.data || []);
            } else {
                console.warn("API Error:", res.message);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load purchase transactions");
        }
    });
}

// ================= RENDER TABLE =================
function renderTable(data) {

    if ($.fn.DataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().clear().destroy();
    }

    $("#tableMain table").DataTable({
        data: data,
        responsive: true,
        pageLength: 10,
        scrollX: true,
        order: [[5, "desc"]], // Payment Date desc

        columns: [

            { data: "PaymentId", title: "Txn ID" },

            {
                data: "InvoiceNo",
                title: "Invoice No",
                render: function (data) {
                    return data ? `#${data}` : '';
                }
            },
            {
                data: "PONo",
                title: "PO No",
                render: function (data) {
                    return data ? `#${data}` : '';
                }
            },

            { data: "SupplierName", title: "Supplier" },

            {
                data: "PaymentDate_Format",
                title: "Payment Date"
            },

            {
                data: "PaidAmount",
                title: "Paid Amount",
                className: "text-end",
                render: d => `₹ ${Number(d).toLocaleString("en-IN")}`
            },

            { data: "PaymentMode", title: "Mode" },
            { data: "AccountName", title: "Account" },

            { data: "ReferenceNo", title: "Reference No" },

            {
                data: "ChequeNo",
                title: "Cheque No",
                render: d => d ? d : "-"
            },

            { data: "PaidBy", title: "Paid By" },

            {
                data: "PaidOn",
                title: "Created On"
            }
        ]
    });
}

//PAYMENT MODE DROPDOWN
function loadPaymentModeDropdown() {
    $.ajax({
        url: "/api/PaymentMode/GetPaymentMode",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            //console.log("response PaymentMode:", response);                

            if (response.status !== "Success") {
                console.warn("PaymentMode API Error:", response.message);
                return;
            }

            const $ddlPaymentMode = $("#ddlPaymentMode");

            if (!response.data || response.data.length === 0) {
                $ddlPaymentMode.prop("disabled", true).html(`<option value="0">No PaymentMode Found</option>`);
            }

            $ddlPaymentMode.html(`<option value="0">-- Select Payment Mode --</option>`);
            response.data.forEach(s => {
                $ddlPaymentMode.append(`<option value="${s.Id}">${s.PaymentModeName}</option>`);
            });
            $ddlPaymentMode.val(response.data[0].Id); // first item by ID
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "PaymentMode Fetch Error")
        }
    });
}

//ACCOUNT DROPDOWN
function loadAccountDropdown() {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
        success: function (res) {

            if (res.status !== "Success") {
                console.warn("Account API Error:", res.message);
                return;
            }

            const $ddlAccount = $("#ddlAccount");

            if (!res.data || res.data.length === 0) {
                $ddlAccount.prop("disabled", true).html(`<option value="0">No Accounts Found</option>`);
            }
            // Clear & add default
            $ddlAccount.html(`<option value="0">-- Select Account --</option>`);
            res.data.forEach(a => {
                $ddlAccount.append(`<option value="${a.Id}">${a.AccountName}</option>`);
            });

            // Optional: auto-select default account
            const defaultAcc = res.data.find(x => x.IsDefaultAccount === true);
            if (defaultAcc) {
                $ddlAccount.val(defaultAcc.Id);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Account Fetch Error");
        }
    });
}

//SUPPLIER DROPDOWN
function loadSupplierDropdown() {
    var payload = { Id: 0 };
    $.ajax({
        url: "/api/Supplier/GetSupplierList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("response supplier:", response);
            $("#ddlSupplier").html(`<option value="0">-- Select Supplier --</option>`);
            response.data.forEach(s => {
                $("#ddlSupplier").append(`<option value="${s.Id}">${s.Name}</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Supplier Fetch Error");
        }
    });
}
