$(document).ready(function () {
    loadFinanceSummary("ThisMonth");
    loadPaymentCollectionStatus("ThisMonth");
    loadUpcomingPayments("ThisMonth");
    loadVendorPayables("thismonth");
    loadTopRevenueEvents("thismonth");

    //EXPENSE TXN --- START -------------------------
    loadExpenseCategoryDropdown();
    loadExpenseSubCategoryDropdown();
    loadAccountDropdown();
    loadPaymentModeDropdown();

    const today = new Date().toISOString().split('T')[0];
    document.getElementById('txtTransactionDate_Expense').value = today;

    // Add button click
    $("#btn-add-debit-transactions").click(function () {
        resetFormFields_Expense();
        $("#ExpenseTransactionModalTitle").text("Add Expense Transactions");
        $("#txtTransactionDate_Expense").val(new Date().toISOString().split('T')[0]); // set today
        $("#ExpenseTransactionModal").modal("show");
    });

    // Save Expense Transaction
    $("#btnSaveExpenseTransaction").click(function (e) {
        e.preventDefault();

        // Validation
        if (!$("#ddlAccount_Expense").val() || $("#ddlAccount_Expense").val() <= 0) {
            Swal.fire("Validation", "Please select Account", "warning");
            return;
        }
        if (!$("#ddlExpenseCategory").val()) {
            Swal.fire("Validation", "Please select Category", "warning");
            return;
        }
        if (!$("#txtAmount_Expense").val() || parseFloat($("#txtAmount_Expense").val()) <= 0) {
            Swal.fire("Validation", "Amount must be greater than 0", "warning");
            return;
        }
        if (!$("#txtTransactionDate_Expense").val()) {
            Swal.fire("Validation", "Please select Transaction Date", "warning");
            return;
        }

        const payload = {
            Id: Number($("#ExpenseTransactionId").val() || 0),
            AccountId: Number($("#ddlAccount_Expense").val()),
            CategoryId: Number($("#ddlExpenseCategory").val()),
            SubCategoryId: $("#ddlSubCategory_Expense").val() ? Number($("#ddlSubCategory_Expense").val()) : null,

            Amount: parseFloat($("#txtAmount_Expense").val()),
            TransactionDate: $("#txtTransactionDate_Expense").val(),

            PaymentModeId: $("#txtPaymentMode_Expense").val() ? Number($("#txtPaymentMode_Expense").val()) : null,

            TransactionId: $("#txtTransactionId_Expense").val().trim() || null,
            Notes: $("#txtNotes_Expense").val().trim() || null,

            ChequeBank: $("#paymentChequebank_Expense").val().trim() || null,
            ChequeNo: $("#paymentChequeNo_Expense").val().trim() || null,
            ChequeDate: $("#paymentChequeDate_Expense").val() || null,

            ActionBy: Number(sessionStorage.getItem("userId") || 0)
        };
        // console.log("SAVE PAYLOAD:", payload);
        $.ajax({
            url: "/api/ExpenseTransactions/SaveExpenseTransaction",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 1000
                    });
                    // Reset form & hide modal
                    resetFormFields_Expense();
                    $("#ExpenseTransactionModal").modal("hide");

                    // Reload table or data
                    //fetchExpenseTransactions();
                } else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save ExpenseTransaction Error")
            }
        });
    });
    //EXPENSE TXN --- END --------------------------

    //INCOME TXN --- START -------------------------
    loadIncomeCategoryDropdown();
    loadIncomeSubCategoryDropdown();
    document.getElementById('txtTransactionDate_Income').value = today;

    // Add button click
    $("#btn-add-credit-transactions").click(function () {
        resetFormFields_Income();
        $("#incomeTransactionModalTitle").text("Add Income Transactions");
        $("#txtTransactionDate_Income").val(new Date().toISOString().split('T')[0]); // set today
        $("#incomeTransactionModal").modal("show");
    });

    // Save Income Transaction
    $("#btnSaveIncomeTransaction").click(function (e) {
        e.preventDefault();

        // Validation
        if (!$("#ddlAccount_Income").val() || $("#ddlAccount_Income").val() <= 0) {
            Swal.fire("Validation", "Please select Account", "warning");
            return;
        }

        if (!$("#ddlIncomeCategory").val()) {
            Swal.fire("Validation", "Please select Category", "warning");
            return;
        }

        if (!$("#txtAmount_Income").val() || parseFloat($("#txtAmount_Income").val()) <= 0) {
            Swal.fire("Validation", "Amount must be greater than 0", "warning");
            return;
        }

        if (!$("#txtTransactionDate_Income").val()) {
            Swal.fire("Validation", "Please select Transaction Date", "warning");
            return;
        }

        const payload = {
            Id: Number($("#incomeTransactionId").val() || 0),
            AccountId: Number($("#ddlAccount_Income").val()),
            CategoryId: Number($("#ddlIncomeCategory").val()),

            SubCategoryId: $("#ddlSubCategory_Income").val()
                ? Number($("#ddlSubCategory_Income").val())
                : null,

            Amount: parseFloat($("#txtAmount_Income").val()),
            TransactionDate: $("#txtTransactionDate_Income").val(),

            PaymentModeId: $("#txtPaymentMode_Income").val()
                ? Number($("#txtPaymentMode_Income").val())
                : null,

            TransactionId: $("#txtTransactionId_Income").val().trim() || null,
            Notes: $("#txtNotes_Income").val().trim() || null,

            ChequeBank: $("#paymentChequebank_Income").val().trim() || null,
            ChequeNo: $("#paymentChequeNo_Income").val().trim() || null,
            ChequeDate: $("#paymentChequeDate_Income").val() || null,

            ActionBy: Number(sessionStorage.getItem("userId") || 0)
        };
        // console.log("SAVE PAYLOAD:", payload);
        $.ajax({
            url: "/api/IncomeTransactions/SaveIncomeTransaction",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 1000
                    });
                    // Reset form & hide modal
                    resetFormFields_Income();
                    $("#incomeTransactionModal").modal("hide");

                    // Reload table or data
                    //fetchIncomeTransactions();
                } else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save IncomeTransaction Error")
            }
        });
    });
    //INCOME TXN --- END ---------------------------

});

function loadFinanceSummary(monthDuration, fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardFinanceSummary",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {
            console.log("res count summaery finance:",res);
            if (res.status === "Success" && res.data) {

                $("#txtTotalRevenue").text(formatINR(res.data.TotalRevenue));
                $("#txtOutstanding").text(formatINR(res.data.OutstandingReceivables));
                $("#txtExpenses").text(formatINR(res.data.TotalExpenses));
                $("#txtNetProfit").text(formatINR(res.data.NetProfit));

                // duration label
              //  $("#lblRevenueDuration").text(getDurationLabel(monthDuration));
            }
            else {
                resetFinanceCards();
            }
        },
        error: function () {
            resetFinanceCards();
        }
    });
}
// 🔹 Dropdown click event (same as event pie)
$(".chart-filter-finance .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".chart-filter-finance .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterText = $(this).text().toLowerCase().replaceAll(" ", "");
    $("#jobChartFilter").text($(this).text());

    loadFinanceSummary(filterText);
});
function formatINR(amount) {
    if (!amount) return "0";
    return amount.toLocaleString("en-IN");
}

function loadUpcomingPayments(monthDuration = "thismonth", fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardUpcomingPayments",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {

            console.log("UpcomingPayments API:", res);

            let html = "";

            if (res.status === "Success" && Array.isArray(res.data) && res.data.length > 0) {

                res.data.forEach(item => {
                    html += `
                        <li class="list-group-item d-flex justify-content-between">
                            <span>${item.EventName}</span>
                            <span class="text-danger">
                                ₹ ${formatINR(item.Amount)}
                            </span>
                        </li>`;
                });

            } else {
                html = `
                    <li class="list-group-item text-muted text-center">
                        No upcoming payments
                    </li>`;
            }

            $("#upcomingPaymentsList").html(html);
        },
        error: function () {
            $("#upcomingPaymentsList").html(`
                <li class="list-group-item text-danger text-center">
                    Error loading data
                </li>
            `);
        }
    });
}

//function getDurationLabel(duration) {
//    switch (duration) {
//        case "ThisMonth": return "Current Month";
//        case "LastMonth": return "Last Month";
//        case "Last30Days": return "Last 30 Days";
//        case "Last12Months": return "Last 12 Months";
//        default: return "Custom Range";
//    }
//}

function resetFinanceCards() {
    $("#txtTotalRevenue").text("0");
    $("#txtOutstanding").text("0");
    $("#txtExpenses").text("0");
    $("#txtNetProfit").text("0");
}

function loadPaymentCollectionStatus(monthDuration, fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardPaymentCollectionStatus",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {
            console.log("res count PaymentCollection finance:", res);

            if (res.status === "Success" && res.data) {

                const paid = res.data.PaidAmount;
               // const partial = res.data.PartialAmount;
                const pending = res.data.PendingAmount;
                const total = res.data.TotalAmount || 1; // avoid divide by zero

                $("#txtPaidAmount").text(formatINR(paid));
               // $("#txtPartialAmount").text(formatINR(partial));
                $("#txtPendingAmount").text(formatINR(pending));

                $("#barPaid").css("width", calcPercent(paid, total) + "%");
                //$("#barPartial").css("width", calcPercent(partial, total) + "%");
                $("#barPending").css("width", calcPercent(pending, total) + "%");
            }
        }
    });
}

// 🔹 Dropdown click event (same as event pie)
$(".chart-filter-Payment .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".chart-filter-Payment .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterText = $(this).text().toLowerCase().replaceAll(" ", "");
    $("#PaymentChartFilter").text($(this).text());

    loadPaymentCollectionStatus(filterText);
});

function calcPercent(value, total) {
    return Math.round((value / total) * 100);
}
function loadVendorPayables(monthDuration, fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardVendorPayables",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {

            console.log("Vendor Payables API:", res);

            let html = "";

            if (res.status === "Success" && Array.isArray(res.data) && res.data.length > 0) {

                res.data.forEach(item => {

                    let badgeClass =
                        item.Status === "Overdue" ? "bg-danger" :
                            item.Status === "Pending" ? "bg-warning" :
                                "bg-secondary";

                    html += `
                        <tr>
                            <td>${item.VendorName}</td>
                            <td>₹ ${formatINR(item.PendingAmount)}</td>
                            <td>
                                <span class="badge ${badgeClass}">
                                    ${item.Status}
                                </span>
                            </td>
                        </tr>`;
                });

            } else {
                html = `
                    <tr>
                        <td colspan="3" class="text-muted text-center">
                            No vendor payables
                        </td>
                    </tr>`;
            }

            $("#vendorPayablesBody").html(html);
        },
        error: function () {
            $("#vendorPayablesBody").html(`
                <tr>
                    <td colspan="3" class="text-danger text-center">
                        Error loading data
                    </td>
                </tr>
            `);
        }
    });
}
function loadTopRevenueEvents(monthDuration, fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardTopRevenueEvents",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {
            console.log("Top Revenue Events:", res);

            let html = "";

            if (res.status === "Success" && res.data && res.data.length > 0) {
                res.data.forEach(item => {
                    html += `
                        <li class="list-group-item d-flex justify-content-between">
                            <span>${item.EventName}</span>
                            <strong>₹ ${formatINR(item.Amount)}</strong>
                        </li>`;
                });
            } else {
                html = `
                    <li class="list-group-item text-muted text-center">
                        No revenue events found
                    </li>`;
            }

            $("#topRevenueEventsList").html(html);
        },
        error: function () {
            $("#topRevenueEventsList").html(`
                <li class="list-group-item text-danger text-center">
                    Error loading data
                </li>
            `);
        }
    });
}


//EXPENSE TRANSACTIONS ----- START --------------------
// Optional: Reset form fields
function resetFormFields_Expense() {
    $("#ExpenseTransactionId").val(0);

    $("#ddlExpenseCategory").val("0").trigger("change");
    $("#ddlSubCategory_Expense").val("0").trigger("change");

    $("#ddlAccount_Expense").val("0").trigger("change");

    $("#txtAmount_Expense").val("");
    $("#txtTransactionDate_Expense").val("");

    $("#txtPaymentMode_Expense").val("0").trigger("change");
    $("#txtTransactionId_Expense").val("");

    $("#paymentChequebank_Expense").val("");
    $("#paymentChequeNo_Expense").val("");
    $("#paymentChequeDate_Expense").val("");

    $("#txtNotes_Expense").val("");
}

function loadExpenseCategoryDropdown(selectedId = "") {
    $.ajax({
        url: "/api/ExpenseCategory/GetExpenseCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            let dropdown = $("#ddlExpenseCategory");
            dropdown.empty(); // clear previous values

            dropdown.append(`<option value="0">-- Select Category --</option>`);

            response.data.forEach(item => {
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                            ${item.CategoryName}
                        </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}

// Load SubCategory dropdown based on selected Expense Category
function loadExpenseSubCategoryDropdown(categoryId = 0, selectedId = "") {
    if (!categoryId) {
        $("#ddlSubCategory_Expense").empty().append('<option value="">-- Select SubCategory --</option>');
        return;
    }
    $.ajax({
        url: "/api/ExpenseSubCategory/GetExpenseSubCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, CategoryId: categoryId }),
        success: function (response) {
            let dropdown = $("#ddlSubCategory_Expense");
            dropdown.empty();
            dropdown.append('<option value="0">-- Select Subcategory --</option>');

            response.data.forEach(item => {
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                                ${item.SubCategoryName}
                            </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}

// Trigger SubCategory load when Expense Category changes
$("#ddlExpenseCategory").change(function () {
    let selectedCategoryId = $(this).val();
    loadExpenseSubCategoryDropdown(selectedCategoryId);
});

// Load Account dropdown dynamically
function loadAccountDropdown(selectedId = "") {
    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }), // 0 = fetch all accounts
        success: function (response) {
            let dropdown = $(".ddlAccount");
            dropdown.empty(); // clear previous values
            dropdown.append('<option value="0">-- Select Account --</option>');

            if (response.status === "Success" && response.data.length > 0) {
                response.data.forEach(item => {
                    dropdown.append(
                        `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                                ${item.AccountName} (${item.AccountType})
                            </option>`
                    );
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}
//EXPENSE TRANSACTIONS ----- END ----------------------

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
            console.log("payment mode res:", res);
            const ddl = $("#txtPaymentMode_Income, #txtPaymentMode_Expense");
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


//INCOME TRANSACTIONS ----- START --------------------
// Optional: Reset form fields
function resetFormFields_Income() {
    $("#incomeTransactionId").val(0);

    $("#ddlIncomeCategory").val("0").trigger("change");
    $("#ddlSubCategory_Income").val("0").trigger("change");

    $("#ddlAccount_Income").val("0").trigger("change");

    $("#txtAmount_Income").val("");
    $("#txtTransactionDate_Income").val("");

    $("#txtPaymentMode_Income").val("0").trigger("change");
    $("#txtTransactionId_Income").val("");

    $("#paymentChequebank_Income").val("");
    $("#paymentChequeNo_Income").val("");
    $("#paymentChequeDate_Income").val("");

    $("#txtNotes_Income").val("");
}


function loadIncomeCategoryDropdown(selectedId = "") {
    $.ajax({
        url: "/api/IncomeCategory/GetIncomeCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            let dropdown = $("#ddlIncomeCategory");
            dropdown.empty(); // clear previous values

            dropdown.append(`<option value="0">-- Select Category --</option>`);

            response.data.forEach(item => {
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                            ${item.CategoryName}
                        </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}

// Load SubCategory dropdown based on selected Income Category
function loadIncomeSubCategoryDropdown(categoryId = 0, selectedId = "") {
    if (!categoryId) {
        $("#ddlSubCategory_Income").empty().append('<option value="">-- Select SubCategory --</option>');
        return;
    }
    $.ajax({
        url: "/api/IncomeSubCategory/GetIncomeSubCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, CategoryId: categoryId }),
        success: function (response) {
            let dropdown = $("#ddlSubCategory_Income");
            dropdown.empty();
            dropdown.append('<option value="0">-- Select Subcategory --</option>');

            response.data.forEach(item => {
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                                ${item.SubCategoryName}
                            </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            console.error("Failed to load dropdown data", error);
        }
    });
}

// Trigger SubCategory load when Income Category changes
$("#ddlIncomeCategory").change(function () {
    let selectedCategoryId = $(this).val();
    loadIncomeSubCategoryDropdown(selectedCategoryId);
});
//INCOME TRANSACTIONS ----- END ----------------------


