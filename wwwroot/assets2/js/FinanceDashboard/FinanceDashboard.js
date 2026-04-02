$(document).ready(function () {
    $("#fromDate, #toDate").hide();
    let selectedFinanceDuration = "thismonth"; // default

    const defaultType = $("input[name='filter_transaction_type']:checked").val();
    toggleCategoryFilters(defaultType);

    loadFinanceSummary(selectedFinanceDuration);
    loadNextVoucherNo();
    loadAccountDropdown();
    loadPaymentModeDropdown();
    fetchTransactions();

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
        else if (selectedFinanceDuration === "last7days") { // 7 days
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 6);
            toDate = today;
        }
        else if (selectedFinanceDuration === "last30days") { // 30 days
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 29);
            toDate = today;
        }
        else if (selectedFinanceDuration === "yesterday") {
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 1);
            toDate = fromDate;
        }
        else if (selectedFinanceDuration === "last12months") {
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

        $("#fromDate").val(formatDate2(fromDate));
        $("#toDate").val(formatDate2(toDate));

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

    //TRANSACTIONS GRID - FILTER -- START -------------------------
    $("#filter_transaction_apply").on("click", function () {

        const transactionType = $("input[name='filter_transaction_type']:checked").val();
        const voucherNo = $("#filter_voucher_no").val()?.trim();

        const payload = {
            AccountId: Number($("#filter_transaction_account").val() || 0),
            PaymentModeId: Number($("#filter_transaction_paymentmode").val() || 0),

            IncomeCategoryId: Number($("#filter_income_category").val() || 0),
            IncomeSubCategoryId: Number($("#filter_income_subcategory").val() || 0),

            ExpenseCategoryId: Number($("#filter_expense_category").val() || 0),
            ExpenseSubCategoryId: Number($("#filter_expense_subcategory").val() || 0),

            FromDate: $("#filter_transaction_fromdate").val() || null,
            ToDate: $("#filter_transaction_todate").val() || null,

            FromAmount: $("#filter_transaction_fromamount").val() || null,
            ToAmount: $("#filter_transaction_toamount").val() || null,

            TransactionType: transactionType,
            VoucherNo: voucherNo || null
        };

        console.log("Filter Payload:", payload);

        fetchTransactions(payload);
    });

    $("#filter_transaction_reset").on("click", function () {

        // 🔒 Preserve
        const selectedType = $("input[name='filter_transaction_type']:checked").val();
        const voucherNo = $("#filter_voucher_no").val();

        // 🔄 Reset others
        $("#filter_income_category").val("0");
        $("#filter_income_subcategory").val("0");

        $("#filter_expense_category").val("0");
        $("#filter_expense_subcategory").val("0");

        $("#filter_transaction_account").val("0");
        $("#filter_transaction_paymentmode").val("0");

        $("#filter_transaction_fromdate").val("");
        $("#filter_transaction_todate").val("");

        $("#filter_transaction_fromamount").val("");
        $("#filter_transaction_toamount").val("");

        // 🔁 Restore
        $("input[name='filter_transaction_type'][value='" + selectedType + "']").prop("checked", true);
        $("#filter_voucher_no").val(voucherNo);

        // Optional: reload with current preserved filters
        $("#filter_transaction_apply").click();
    });

    $("input[name='filter_transaction_type']").on("change", function () {
        const type = $(this).val();
        toggleCategoryFilters(type);
    });
    //TRANSACTIONS GRID - FILTER -- START -------------------------


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

    const today = new Date().toISOString().split('T')[0];
    document.getElementById('txtTransactionDate_Expense').value = today;

    $('#expenseTransactionModal').on('shown.bs.modal', function () {
        $('#txtAmount_Expense').focus();
    });

    // Add button click
    $("#btn-add-debit-transactions").click(function () {
        resetFormFields_Expense();
        $("#expenseTitleText").text("Add Expense Transactions");
        $("#ExpenseTransactionModal").modal("show");
    });
    // Save Expense Transaction
    $("#btnSaveExpenseTransaction").click(function (e) {
        e.preventDefault();

        // Validation
        let mode = parseInt($("#txtPaymentMode_Expense").val()) || 0;
        let account = parseInt($("#ddlAccount_Expense").val()) || 0;
        let category = $("#ddlExpenseCategory").val();
        let subcategory = $("#ddlSubCategory_Expense").val();
        let amount = parseFloat($("#txtAmount_Expense").val());
        let transactionDate = $("#txtTransactionDate_Expense").val();

        // Amount
        if (!amount || isNaN(amount) || amount <= 0) {
            Swal.fire("Validation", "Amount must be greater than 0", "warning");
            $("#txtAmount_Expense").focus();
            return false;
        }
        // Category
        if (!category || category <= 0) {
            Swal.fire("Validation", "Please select Category", "warning");
            $("#ddlExpenseCategory").focus();
            return false;
        }
        // SubCategory
        if (!subcategory || subcategory <= 0) {
            Swal.fire("Validation", "Please select SubCategory", "warning");
            $("#ddlSubCategory_Expense").focus();
            return false;
        }
        // Transaction Date
        if (!transactionDate || transactionDate =="") {
            Swal.fire("Validation", "Please select Transaction Date", "warning");
            $("#txtTransactionDate_Expense").focus();
            return false;
        }
        // Account
        if (account <= 0) {
            Swal.fire("Validation", "Please select Account", "warning");
            $("#ddlAccount_Expense").focus();
            return false;
        }
        // Mode
        if (mode <= 0) {
            Swal.fire("Validation", "Please select Payment Mode", "warning");
            $("#txtPaymentMode_Expense").focus();
            return false;
        }

        const payload = {
            Id: Number($("#ExpenseTransactionId").val() || 0),
            AccountId: Number($("#ddlAccount_Expense").val()),
            CategoryId: Number($("#ddlExpenseCategory").val()),
            SubCategoryId: $("#ddlSubCategory_Expense").val() ? Number($("#ddlSubCategory_Expense").val()) : 0,

            Amount: parseFloat($("#txtAmount_Expense").val()),
            TransactionDate: $("#txtTransactionDate_Expense").val(),

            PaymentModeId: $("#txtPaymentMode_Expense").val() ? Number($("#txtPaymentMode_Expense").val()) : 0,

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
                    handleApiSuccess("Saved.");
                    // Reset form & hide modal
                    resetFormFields_Expense();
                    $("#ExpenseTransactionModal").modal("hide");
                    loadNextVoucherNo();
                    // Reload table or data
                    fetchTransactions();
                    loadFinanceSummary(selectedFinanceDuration);
                } else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save ExpenseTransaction Error")
            }
        });
    });

    // Edit button click for Expense Transactions
    $(document).on('click', '.btn-edit-expense', function (event) {
        var canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';
        if (!canEdit) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Edit. Please contact admin.",
                confirmButtonText: "OK"
            });
            event.preventDefault();
            return;
        }

        resetFormFields_Expense(); // Clear previous values

        // Get the DataTable instance
        var table = $('#tableMain_Expense table').DataTable();

        // Get the row object for the clicked Edit button
        var row = $(this).closest('tr');
        var rowData = table.row(row).data(); // Get the data object for this row

        console.log("edit rawdata:", rowData);
        // Fill modal fields with selected row data
        $("#ExpenseTransactionId").val(rowData.Id);
        $("#ddlAccount_Expense").val(rowData.AccountId);
        $("#ddlExpenseCategory").val(rowData.CategoryId);
        $("#ddlSubCategory_Expense").val(rowData.SubCategoryId);
        $("#txtAmount_Expense").val(rowData.Amount);
        $("#txtTransactionDate_Expense").val(rowData.TransactionDate);
        $("#txtPaymentMode_Expense").val(rowData.PaymentModeId).trigger("change");

        $("#paymentVoucherNo_Expense").val(rowData.VoucherNo);
        $("#expenseVoucherDisplay").text(rowData.VoucherNo || "-");

        $("#txtTransactionId_Expense").val(rowData.TransactionId);
        $("#paymentChequebank_Expense").val(rowData.ChequeBank);
        $("#paymentChequeNo_Expense").val(rowData.ChequeNo);
        $("#paymentChequeDate_Expense").val(rowData.ChequeDate);
        $("#txtNotes_Expense").val(rowData.Notes);

        // Update modal title
        $("#expenseTitleText").text("Edit Expense Transactions");

        // Show modal
        $('#ExpenseTransactionModal').modal('show');
    });

    // Delete button click in table
    $(document).on("click", ".btn-delete-expense", function () {
        // Get DataTable instance
        var table = $("#tableMain_Expense table").DataTable(); // make sure this matches your table id
        var rowData = table.row($(this).closest("tr")).data();

        if (!rowData || !rowData.Id) {
            Swal.fire("Error", "Unable to get selected row data.", "error");
            return;
        }

        // Store Id in modal using jQuery data
        $("#deleteRecordModal").data("id", rowData.Id).data("src", "expense").modal("show");
    });

    $(document).on("change", "#txtPaymentMode_Expense", function () {

        const selectedText = $("#txtPaymentMode_Expense option:selected").text().toLowerCase().trim();

        // Hide everything
        $("#expensePaymentSection").addClass("d-none");
        $(".expensePayment_Fields").addClass("d-none");

        // Clear values
        $(".expensePayment_Fields input").val("");

        // CASH / BANK TRANSFER → nothing
        if (selectedText.includes("cash") || selectedText.includes("bank transfer")) {
            return;
        }

        // Show section
        $("#expensePaymentSection").removeClass("d-none");

        // CHEQUE
        if (selectedText.includes("cheque")) {
            $("#expense_bank_wrap").removeClass("d-none");
            $("#expense_chequeNo_wrap").removeClass("d-none");
            $("#expense_chequeDate_wrap").removeClass("d-none");
        }

        // UPI / CARD / ONLINE
        else if (selectedText.includes("upi") || selectedText.includes("card") || selectedText.includes("online")) {
            $("#expense_txnId_wrap").removeClass("d-none");
        }
    });
    //EXPENSE TXN --- END --------------------------

    //INCOME TXN --- START -------------------------
    loadIncomeCategoryDropdown();
    loadIncomeSubCategoryDropdown();

    document.getElementById('txtTransactionDate_Income').value = today;

    $('#incomeTransactionModal').on('shown.bs.modal', function () {
        $('#txtAmount_Income').focus();
    });

    // Add button click
    $("#btn-add-credit-transactions").click(function () {
        resetFormFields_Income();
        $("#incomeTitleText").text("Add Income Transactions");
        $("#incomeTransactionModal").modal("show");
    });

    // Save Income Transaction
    $("#btnSaveIncomeTransaction").click(function (e) {
        e.preventDefault();

        // Validation
        let mode = parseInt($("#txtPaymentMode_Income").val()) || 0;
        let account = parseInt($("#ddlAccount_Income").val()) || 0;
        let category = $("#ddlIncomeCategory").val();
        let subcategory = $("#ddlSubCategory_Income").val();
        let amount = parseFloat($("#txtAmount_Income").val());
        let transactionDate = $("#txtTransactionDate_Income").val();

        // Amount
        if (!amount || isNaN(amount) || amount <= 0) {
            Swal.fire("Validation", "Amount must be greater than 0", "warning");
            $("#txtAmount_Income").focus();
            return false;
        }
        // Category
        if (!category || category <= 0) {
            Swal.fire("Validation", "Please select Category", "warning");
            $("#ddlIncomeCategory").focus();
            return false;
        }
        // SubCategory
        if (!subcategory || subcategory <= 0) {
            Swal.fire("Validation", "Please select SubCategory", "warning");
            $("#ddlSubCategory_Income").focus();
            return false;
        }
        // Transaction Date
        if (!transactionDate) {
            Swal.fire("Validation", "Please select Transaction Date", "warning");
            $("#txtTransactionDate_Income").focus();
            return false;
        }
        // Account
        if (account <= 0) {
            Swal.fire("Validation", "Please select Account", "warning");
            $("#ddlAccount_Income").focus();
            return false;
        }
        // Mode
        if (mode <= 0) {
            Swal.fire("Validation", "Please select Mode", "warning");
            $("#txtPaymentMode_Income").focus();
            return false;
        }

        const payload = {
            Id: Number($("#incomeTransactionId").val() || 0),
            AccountId: Number($("#ddlAccount_Income").val()),
            CategoryId: Number($("#ddlIncomeCategory").val()),

            SubCategoryId: $("#ddlSubCategory_Income").val()
                ? Number($("#ddlSubCategory_Income").val())
                : 0,

            Amount: parseFloat($("#txtAmount_Income").val()),
            TransactionDate: $("#txtTransactionDate_Income").val(),

            PaymentModeId: $("#txtPaymentMode_Income").val()
                ? Number($("#txtPaymentMode_Income").val())
                : 0,

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
                    handleApiSuccess("Saved.");
                    // Reset form & hide modal
                    resetFormFields_Income();
                    $("#incomeTransactionModal").modal("hide");
                    loadNextVoucherNo();
                    // Reload table or data
                    fetchTransactions();
                    loadFinanceSummary(selectedFinanceDuration);
                } else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save IncomeTransaction Error")
            }
        });
    });

    // Edit button click for Income Transactions
    $(document).on('click', '.btn-edit-income', function (event) {
        var canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';
        if (!canEdit) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Edit. Please contact admin.",
                confirmButtonText: "OK"
            });
            event.preventDefault();
            return;
        }

        resetFormFields_Income(); // Clear previous values

        // Get the DataTable instance
        var table = $('#tableMain_Transactions table').DataTable();

        // Get the row object for the clicked Edit button
        var row = $(this).closest('tr');
        var rowData = table.row(row).data(); // Get the data object for this row

        console.log("rowData income:", rowData);
        // Fill modal fields with selected row data
        $("#incomeTransactionId").val(rowData.Id);
        $("#ddlAccount_Income").val(rowData.AccountId);
        $("#ddlIncomeCategory").val(rowData.CategoryId);
        $("#ddlSubCategory_Income").val(rowData.SubCategoryId);
        $("#txtAmount_Income").val(rowData.Amount);
        $("#txtTransactionDate_Income").val(rowData.TransactionDate);
        $("#txtPaymentMode_Income").val(rowData.PaymentModeId).trigger("change");

        $("#paymentVoucherNo_Income").val(rowData.VoucherNo);
        $("#incomeVoucherDisplay").text(rowData.VoucherNo || "-");

        $("#txtTransactionId_Income").val(rowData.TransactionId);
        $("#paymentChequebank_Income").val(rowData.ChequeBank);
        $("#paymentChequeNo_Income").val(rowData.ChequeNo);
        $("#paymentChequeDate_Income").val(rowData.ChequeDate);
        $("#txtNotes_Income").val(rowData.Notes);

        // Update modal title
        $("#incomeTitleText").text("Edit Income Transactions");

        // Show modal
        $('#incomeTransactionModal').modal('show');
    });

    // Delete button click in table
    $(document).on("click", ".btn-delete-income", function () {
        // Get DataTable instance
        var table = $("#tableMain_Transactions table").DataTable(); // make sure this matches your table id
        var rowData = table.row($(this).closest("tr")).data();

        if (!rowData || !rowData.Id) {
            Swal.fire("Error", "Unable to get selected row data.", "error");
            return;
        }
        // Store Id in modal using jQuery data
        $("#deleteRecordModal").data("id", rowData.Id).data("src", "income").modal("show");
    });

    // Confirm delete
    $("#delete-record").click(function () {
        var id = $("#deleteRecordModal").data("id");
        var src = $("#deleteRecordModal").data("src");

        if (!id) {
            Swal.fire("Error", "Record Id not found.", "error");
            return;
        }

        let payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        if (src === "income") {
            $.ajax({
                url: "/api/IncomeTransactions/DeleteIncomeTransaction",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (response) {
                    if (response.status === "Success") {
                        handleApiSuccess("Deleted.");
                        $("#deleteRecordModal").modal("hide");

                        // Reload table
                        fetchTransactions();
                        loadNextVoucherNo();
                    } else {
                        Swal.fire("Error", response.message, "error");
                    }
                },
                error: function (xhr, status, error) {
                    handleApiError(xhr, status, error, "Delete Income Transaction Failed!");
                }
            });
        }

        if (src === "expense") {
            $.ajax({
                url: "/api/ExpenseTransactions/DeleteExpenseTransaction",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (response) {
                    if (response.status === "Success") {
                        handleApiSuccess("Deleted.");
                        $("#deleteRecordModal").modal("hide");

                        // Reload table
                        fetchTransactions();
                        loadNextVoucherNo();
                    } else {
                        Swal.fire("Error", response.message, "error");
                    }
                },
                error: function (xhr, status, error) {
                    handleApiError(xhr, status, error, "Delete Expense Transaction FAILED!")
                }
            });

        }
    });

    //txtPaymentMode_Income - change
    $(document).on("change", "#txtPaymentMode_Income", function () {

        const selectedText = $("#txtPaymentMode_Income option:selected").text().toLowerCase().trim();

        // Hide everything
        $("#incomePaymentSection").addClass("d-none");
        $(".incomePayment_Fields").addClass("d-none");

        // Clear values
        $(".incomePayment_Fields input").val("");

        // CASH / BANK TRANSFER → nothing
        if (selectedText.includes("cash") || selectedText.includes("bank transfer")) {
            return;
        }

        // Show section
        $("#incomePaymentSection").removeClass("d-none");

        // CHEQUE
        if (selectedText.includes("cheque")) {
            $("#income_bank_wrap").removeClass("d-none");
            $("#income_chequeNo_wrap").removeClass("d-none");
            $("#income_chequeDate_wrap").removeClass("d-none");
        }

        // UPI / CARD / ONLINE
        else if (selectedText.includes("upi") || selectedText.includes("card") || selectedText.includes("online")) {
            $("#income_txnId_wrap").removeClass("d-none");
        }
    });    
    //INCOME TXN --- END ---------------------------
});

function toggleCategoryFilters(type) {

    if (type === "INCOME") {
        $("#incomeFilters").show();
        $("#expenseFilters").hide();

        $("#filter_expense_category").val(0);
        $("#filter_expense_subcategory").val(0);
    }
    else if (type === "EXPENSE") {
        $("#incomeFilters").hide();
        $("#expenseFilters").show();

        $("#filter_income_category").val(0);
        $("#filter_income_subcategory").val(0);
    }
    else {
        $("#incomeFilters").show();
        $("#expenseFilters").show();
    }
}
function loadFinanceSummary(monthDuration, fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };
    console.log("finance summmary payload:", payload);
    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardFinanceSummary",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("res count summaery finance:",res);
            if (res.status === "Success" && res.data) {

                $("#txtTotalIncomes").text(formatINR(res.data.TotalIncomes));
                $("#txtTotalExpenses").text(formatINR(res.data.TotalExpenses));
                // GROSS PROFIT -- START
                var grossProfit = res.data.GrossProfit || 0;
                $("#txtGrossProfit").text(formatINR(grossProfit));

                const wrapper = $("#grossProfitWrapper");
                const title = wrapper.closest(".card-body").find("h6");

                wrapper.removeClass("text-success text-danger");
                title.removeClass("text-success text-danger");

                if (grossProfit < 0) {
                    wrapper.addClass("text-danger");
                    title.addClass("text-danger");
                }
                else {
                    wrapper.addClass("text-success");
                    title.addClass("text-success");
                }
                // GROSS PROFIT -- END
            }
            else {
                resetFinanceCards();
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fail to load FinanceSummary data")
            resetFinanceCards();
        }
    });
}

function resetFinanceCards() {
    $("#txtTotalIncomes").text("0");
    $("#txtTotalExpenses").text("0");
    $("#txtGrossProfit").text("0");
}

//TRANSACTIONS GRID --START
function fetchTransactions(payload = {}) {
    // Default payload (important for first load)
    const defaultPayload = {
        AccountId: 0,
        PaymentModeId: 0,
        IncomeCategoryId: 0,
        IncomeSubCategoryId: 0,
        ExpenseCategoryId: 0,
        ExpenseSubCategoryId: 0,
        FromDate: null,
        ToDate: null,
        FromAmount: null,
        ToAmount: null,
        TransactionType: "ALL",
        VoucherNo: null
    };

    const finalPayload = { ...defaultPayload, ...payload };

    console.log("API Payload:", finalPayload);

    $.ajax({
        url: "/api/FinanceDashboard/GetIncomeExpenseTransactionList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(finalPayload),
        beforeSend: function () {
            showTempLoader("fetching Transactions...");
            $("#tableMain_Transactions table").html("<tr><td class='text-center p-3'>Loading...</td></tr>");
        },
        success: function (res) {
            if (res.status === "Success") {
                renderTransactions(res.data);
            } else {
                $("#tableMain_Transactions table").html(`<tr><td class='text-danger p-3'>${res.message}</td></tr>`);
            }
        },
        error: function (err, xhr, status) {
            handleApiError(err, xhr, status,"Error loading Txn data");
            $("#tableMain_Transactions table").html("<tr><td class='text-danger p-3'>Error loading data</td></tr>");
        },
        complete: function () {
            Swal.close();
        }
    });
}
function renderTransactions(data) {

    let table = $("#tableMain_Transactions table");

    // Destroy if already initialized
    if ($.fn.DataTable.isDataTable(table)) {
        table.DataTable().destroy();
    }

    table.empty();

    table.DataTable({
        data: data,
        pageLength: 10,
        responsive: true,
        columns: [
            {
                data: null,
                title: "#",
                visible: false,
                className: "text-center",
                render: (data, type, row, meta) => meta.row + 1
            },
            {
                data: "VoucherNo",
                title: "Voucher No",
                className: "text-center",
                render: function (data, type, row) {
                    let badgeClass = row.TransactionType === "INCOME" ? "bg-success" : "bg-danger";
                    return `<span class="badge ${badgeClass}">${data}</span>`;
                }
            },
            { data: "TransactionDate2", title: "Date", className: "text-center" },
            {
                data: "Amount",
                title: "Amount",
                className: "text-end fw-semibold",
                render: function (data, type, row) {
                    if (row.TransactionType === "INCOME")
                        return `<span class="text-success">+ ${data}</span>`;
                    else
                        return `<span class="text-danger">- ${data}</span>`;
                }
            },
            { data: "CategoryName", title: "Category", className: "text-center" },
            { data: "SubCategoryName", title: "Sub-Category", className: "text-center" },
            { data: "AccountName", title: "Source / Receiver", className: "text-center" },
            { data: "PaymentModeName", title: "Payment Mode", className: "text-center" },
            {
                data: "Notes",
                title: "Notes",
                render: function (data) {

                    if (!data) return "";

                    const fullText = data;
                    const shortText = data.length > 10 ? data.substring(0, 10) + "..." : data;

                    return `<span data-bs-toggle="tooltip" title="${fullText}">
                    ${shortText}
                </span>`;
                }
            }
        ]
    });
}
//TRANSACTIONS GRID --END


//EXPENSE TRANSACTIONS ----- START --------------------
// Optional: Reset form fields
function resetFormFields_Expense() {
    $("#ExpenseTransactionId").val(0);

    $("#ddlExpenseCategory").val("0"); //.trigger("change");
    $("#ddlSubCategory_Expense").val("0"); //.trigger("change");

    $("#ddlAccount_Expense").val("0"); //.trigger("change");

    $("#txtAmount_Expense").val("");
    $("#txtTransactionDate_Expense").val(formatDate2());

    $("#txtPaymentMode_Expense").val("0").trigger("change");
    $("#txtTransactionId_Expense").val("");

    // Hide everything
    $("#expensePaymentSection").addClass("d-none");
    $(".expensePayment_Fields").addClass("d-none");
    // Clear values
    $(".expensePayment_Fields input").val("");

    $("#paymentChequebank_Expense").val("");
    $("#paymentChequeNo_Expense").val("");
    $("#paymentChequeDate_Expense").val("");

    $("#txtNotes_Expense").val("");

    //$("#paymentVoucherNo_Expense").val(nextExpenseVoucherNo);
    // Hidden field (actual value)
    $("#paymentVoucherNo_Expense").val(nextExpenseVoucherNo);
    // Header display
    $("#expenseVoucherDisplay").text(nextExpenseVoucherNo || "-");
}

function loadExpenseCategoryDropdown(selectedId = "") {
    $.ajax({
        url: "/api/ExpenseCategory/GetExpenseCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            let dropdown = $("#ddlExpenseCategory, #filter_expense_category");
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
            handleApiError(xhr, status, error, "Failed to load expense category data");
        }
    });
}

// Load SubCategory dropdown based on selected Expense Category
function loadExpenseSubCategoryDropdown(categoryId = 0, selectedId = "") {
    //if (!categoryId) {
    //    $("#ddlSubCategory_Expense").empty().append('<option value="">-- Select SubCategory --</option>');
    //    return;
    //}

    $.ajax({
        url: "/api/ExpenseSubCategory/GetExpenseSubCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, CategoryId: categoryId }),
        beforeSend: function () {
            showTempLoader("fetching Expense Sub-Category...");
        },
        success: function (response) {
            let dropdown = $("#ddlSubCategory_Expense, #filter_expense_subcategory");
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
            handleApiError(xhr, status, error, "Failed to load subcategory dropdown data");
        },
        complete: function () {
            Swal.close();
        }
    });
}

// Trigger SubCategory load when Expense Category changes
$("#ddlExpenseCategory, #filter_expense_category").change(function () {
    let selectedCategoryId = $(this).val();
    loadExpenseSubCategoryDropdown(selectedCategoryId);
});
//EXPENSE TRANSACTIONS ----- END ----------------------

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

//INCOME TRANSACTIONS ----- START --------------------
// Optional: Reset form fields
function resetFormFields_Income() {
    $("#incomeTransactionId").val(0);

    $("#ddlIncomeCategory").val("0"); //.trigger("change");
    $("#ddlSubCategory_Income").val("0"); //.trigger("change");

    $("#ddlAccount_Income").val("0"); //.trigger("change");

    $("#txtAmount_Income").val("");
    $("#txtTransactionDate_Income").val(formatDate2());

    $("#txtPaymentMode_Income").val("0").trigger("change");
    $("#txtTransactionId_Income").val("");

    // Hide everything
    $("#incomePaymentSection").addClass("d-none");
    $(".incomePayment_Fields").addClass("d-none");
    // Clear values
    $(".incomePayment_Fields input").val("");

    $("#paymentChequebank_Income").val("");
    $("#paymentChequeNo_Income").val("");
    $("#paymentChequeDate_Income").val("");

    $("#txtNotes_Income").val("");

    // Hidden field (actual value)
    $("#paymentVoucherNo_Income").val(nextIncomeVoucherNo);
    // Header display
    $("#incomeVoucherDisplay").text(nextIncomeVoucherNo || "-");
}
function loadIncomeCategoryDropdown(selectedId = "") {
    $.ajax({
        url: "/api/IncomeCategory/GetIncomeCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            let dropdown = $("#ddlIncomeCategory, #filter_income_category");
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
            handleApiError(xhr, status, error, "Failed to load income category dropdown data");
        }
    });
}
function loadIncomeSubCategoryDropdown(categoryId = 0, selectedId = "") {
    //if (!categoryId) {
    //    $("#ddlSubCategory_Income").empty().append('<option value="">-- Select SubCategory --</option>');
    //    return;
    //}
    $.ajax({
        url: "/api/IncomeSubCategory/GetIncomeSubCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, CategoryId: categoryId }),
        beforeSend: function () {
            showTempLoader("fetching Income Sub-Category...");
        },
        success: function (response) {

            console.log("GetIncomeSubCategoryList response:", response);
            let dropdown = $("#ddlSubCategory_Income, #filter_income_subcategory");
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
            handleApiError(xhr, status, error, "Fail to load Income SubCategory")
        },
        complete: function(){
            Swal.close();
        }
    });
}

// Trigger SubCategory load when Income Category changes
$("#ddlIncomeCategory, #filter_income_category").change(function () {
    let selectedCategoryId = $(this).val();
    loadIncomeSubCategoryDropdown(selectedCategoryId);
});
//INCOME TRANSACTIONS ----- END ----------------------

//NEXT VOUCHER NO --- START ----------------------
var nextIncomeVoucherNo = null;
var nextExpenseVoucherNo = null;
function loadNextVoucherNo() {

    $.ajax({
        url: "/api/IncomeTransactions/GetNextVoucherNo", // adjust controller route if needed
        type: "GET",
        success: function (res) {
            console.log("voucher res:", res);
            if (res.status === "Success" && res.data) {

                nextIncomeVoucherNo = res.data.LastVoucherNo_Income;
                nextExpenseVoucherNo = res.data.LastVoucherNo_Expense;

                console.log("Next Income Voucher:", nextIncomeVoucherNo);
                console.log("Next Expense Voucher:", nextExpenseVoucherNo);
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