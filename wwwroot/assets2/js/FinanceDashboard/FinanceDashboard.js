$(document).ready(function () {
    $("#fromDate, #toDate").hide();
    let selectedFinanceDuration = "thismonth"; // default

    loadFinanceSummary(selectedFinanceDuration);

    //FILTER --- START ---------------------------------------

    $(".chart-filter-finance .dropdown-item").on("click", function (e) {
        e.preventDefault();

        const newFilter = $(this).data("value");
        if (selectedFinanceDuration === newFilter) return;

        selectedFinanceDuration = newFilter;

        $(".chart-filter-finance .dropdown-item").removeClass("active");
        $(this).addClass("active");

        $("#jobChartFilter").text("Filter : " + $(this).text());

        let today = new Date();
        let fromDate, toDate;

        if (selectedFinanceDuration === "thismonth") {
            fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
            toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
        }
        else if (selectedFinanceDuration === "lastmonth") {
            fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
            toDate = new Date(today.getFullYear(), today.getMonth(), 0);
        }
        else if (selectedFinanceDuration === "days") { // 7 days
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 6);
            toDate = today;
        }
        else if (selectedFinanceDuration === "lastdays") { // 30 days
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 29);
            toDate = today;
        }
        else if (selectedFinanceDuration === "yesterday") {
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 1);
            toDate = fromDate;
        }
        else if (selectedFinanceDuration === "12months") {
            fromDate = new Date(today);
            fromDate.setMonth(today.getMonth() - 12);
            toDate = today;
        }
        else if (selectedFinanceDuration === "customrange") {
            $("#fromDate, #toDate").trigger("change");
            return;
        }

        if (selectedFinanceDuration === "customrange") {
            $("#fromDate, #toDate").show();
        } else {
            $("#fromDate, #toDate").hide();
        }

        $("#fromDate").val(formatDate(fromDate));
        $("#toDate").val(formatDate(toDate));

        loadFinanceSummary(selectedFinanceDuration);
    });

    $("#fromDate, #toDate").on("change", function () {

        let fromDate = $("#fromDate").val();
        let toDate = $("#toDate").val();

        if (fromDate && toDate) {

            selectedFinanceDuration = "customrange";

            $("#jobChartFilter").text("Filter : Custom Range");

            $(".chart-filter-finance .dropdown-item").removeClass("active");

            loadFinanceSummary(selectedFinanceDuration);
        }
    });

    $("#btnReset").on("click", function () {

        selectedFinanceDuration = "thismonth";

        $("#fromDate").val("");
        $("#toDate").val("");

        $("#jobChartFilter").text("Filter : This Month");

        $(".chart-filter-finance .dropdown-item").removeClass("active");
        $('.chart-filter-finance .dropdown-item[data-value="thismonth"]')
            .addClass("active");

        loadFinanceSummary(selectedFinanceDuration);
    });
    //FILTER --- END---------------------------------------

    //Tooltip - on-hover widget ---START ------
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(el => {
        new bootstrap.Tooltip(el, {
            delay: { show: 200, hide: 100 }
        });
    });
    //Tooltip - on-hover widget ---END --------


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
        $("#txtTransactionDate_Expense").val(getTodayLocalDate());
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
            VoucherNo: $("#paymentVoucherNo_Expense").val().trim() || null,

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
                    //$("#ExpenseTransactionModal").modal("hide");

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
        $("#txtTransactionDate_Income").val(getTodayLocalDate());
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
            VoucherNo: $("#paymentVoucherNo_Income").val().trim() || null,

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
                    //$("#incomeTransactionModal").modal("hide");

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

function getTodayLocalDate() {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function loadFinanceSummary(monthDuration, fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };
    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardFinanceSummary",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("res count summaery finance:",res);
            if (res.status === "Success" && res.data) {

                $("#txtTotalRevenue").text(formatINR(res.data.TotalRevenue));
                $("#txtOutstanding").text(formatINR(res.data.OutstandingReceivables));
                $("#txtExpenses").text(formatINR(res.data.TotalExpenses));

                //NET PROFIT 
                let netProfit = parseFloat(res.data.NetProfit || 0);
                $("#txtNetProfit").text(formatINR(netProfit));

                const wrapper = $("#netProfitWrapper");
                const title = wrapper.closest(".card-body").find("h6");

                wrapper.removeClass("text-success text-danger");
                title.removeClass("text-success text-danger");

                if (netProfit < 0) {
                    wrapper.addClass("text-danger");
                    title.addClass("text-danger");
                } else {
                    wrapper.addClass("text-success");
                    title.addClass("text-success");
                }
                //NET PROFIT 
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

function resetFinanceCards() {
    $("#txtTotalRevenue").text("0");
    $("#txtOutstanding").text("0");
    $("#txtExpenses").text("0");
    $("#txtNetProfit").text("0");
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
    loadNextVoucherNo();

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
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
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
    loadNextVoucherNo();
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

//NEXT VOUCHER NO --- START ----------------------
function loadNextVoucherNo() {

    $.ajax({
        url: "/api/IncomeTransactions/GetNextVoucherNo", // adjust controller route if needed
        type: "GET",
        success: function (res) {
            console.log("voucher res:", res);
            if (res.status === "Success" && res.data) {

                var nextIncomeVoucherNo = res.data.LastVoucherNo_Income;
                var nextExpenseVoucherNo = res.data.LastVoucherNo_Expense;

                console.log("Next Income Voucher:", nextIncomeVoucherNo);
                console.log("Next Expense Voucher:", nextExpenseVoucherNo);

                // Example: auto-fill fields
                $("#paymentVoucherNo_Income").val(nextIncomeVoucherNo);
                $("#paymentVoucherNo_Expense").val(nextExpenseVoucherNo);
            }
            else {
                console.warn("Voucher no not received", res);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to fetch next voucher no");
        }
    });
}

//NEXT VOUCHER NO --- END ----------------------