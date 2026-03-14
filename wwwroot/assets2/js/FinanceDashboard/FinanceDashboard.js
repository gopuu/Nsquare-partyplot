$(document).ready(function () {
    $("#fromDate, #toDate").hide();
    let selectedFinanceDuration = "thismonth"; // default

    loadFinanceSummary(selectedFinanceDuration);
    loadNextVoucherNo();
    loadAccountDropdown();
    loadPaymentModeDropdown();

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

    //INCOME TABLE - FILTER -- START -------------------------
    $("#filter_income_apply").on("click", function () {
        var categoryId = Number($("#filter_income_category").val() || 0);
        var subCategoryId = Number($("#filter_income_subcategory").val() || 0);
        var accountId = Number($("#filter_income_account").val() || 0);
        var paymentModeId = Number($("#filter_income_paymentmode").val() || 0);
        var fromDate = $("#filter_income_fromdate").val();
        var toDate = $("#filter_income_todate").val();

        fetchIncomeTransactions(accountId, paymentModeId, categoryId, subCategoryId, fromDate, toDate);
    });
    $("#filter_income_reset").on("click", function () {

        $("#filter_income_category").val("0");
        $("#filter_income_subcategory").val("0");
        $("#filter_income_fromdate").val("");
        $("#filter_income_todate").val("");

        console.log("Income filters reset");
        fetchIncomeTransactions();
    });
    //INCOME TABLE - FILTER -- END -------------------------

    //EXPENSE TABLE - FILTER -- START -------------------------
    $("#filter_expense_apply").on("click", function () {
        var categoryId = Number($("#filter_expense_category").val() || 0);
        var subCategoryId = Number($("#filter_expense_subcategory").val() || 0);
        var accountId = Number($("#filter_expense_account").val() || 0);
        var paymentModeId = Number($("#filter_expense_paymentmode").val() || 0);
        var fromDate = $("#filter_expense_fromdate").val();
        var toDate = $("#filter_expense_todate").val();

        fetchExpenseTransactions(accountId, paymentModeId, categoryId, subCategoryId, fromDate, toDate);
    });
    $("#filter_expense_reset").on("click", function () {

        $("#filter_expense_category").val("0");
        $("#filter_expense_subcategory").val("0");
        $("#filter_expense_fromdate").val("");
        $("#filter_expense_todate").val("");

        console.log("Expense filters reset");
        fetchExpenseTransactions();
    });

    //EXPENSE TABLE - FILTER -- END -------------------------


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
    fetchExpenseTransactions();

    const today = new Date().toISOString().split('T')[0];
    document.getElementById('txtTransactionDate_Expense').value = today;
    // Add button click
    $("#btn-add-debit-transactions").click(function () {
        resetFormFields_Expense();
        $("#ExpenseTransactionModalTitle").text("Add Expense Transactions");
        $("#txtTransactionDate_Expense").val(formatDate2());
        $("#ExpenseTransactionModal").modal("show");
    });
    // Save Expense Transaction
    $("#btnSaveExpenseTransaction").click(function (e) {
        e.preventDefault();

        // Validation
        let mode = parseInt($("#txtPaymentMode_Expense").val()) || 0;
        let account = parseInt($("#ddlAccount_Expense").val()) || 0;
        let category = $("#ddlExpenseCategory").val();
        let amount = parseFloat($("#txtAmount_Expense").val());
        let transactionDate = $("#txtTransactionDate_Expense").val();

        // Mode
        if (mode <= 0) {
            Swal.fire("Validation", "Please select Mode", "warning");
            $("#txtPaymentMode_Expense").focus();
            return false;
        }

        // Account
        if (account <= 0) {
            Swal.fire("Validation", "Please select Account", "warning");
            $("#ddlAccount_Expense").focus();
            return false;
        }

        // Category
        if (!category) {
            Swal.fire("Validation", "Please select Category", "warning");
            $("#ddlExpenseCategory").focus();
            return false;
        }

        // Amount
        if (!amount || isNaN(amount) || amount <= 0) {
            Swal.fire("Validation", "Amount must be greater than 0", "warning");
            $("#txtAmount_Expense").focus();
            return false;
        }

        // Transaction Date
        if (!transactionDate) {
            Swal.fire("Validation", "Please select Transaction Date", "warning");
            $("#txtTransactionDate_Expense").focus();
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
                    //$("#ExpenseTransactionModal").modal("hide");
                    loadNextVoucherNo();
                    // Reload table or data
                    fetchExpenseTransactions();
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
        $("#txtTransactionId_Expense").val(rowData.TransactionId);
        $("#paymentChequebank_Expense").val(rowData.ChequeBank);
        $("#paymentChequeNo_Expense").val(rowData.ChequeNo);
        $("#paymentChequeDate_Expense").val(rowData.ChequeDate);
        $("#txtNotes_Expense").val(rowData.Notes);

        // Update modal title
        $("#ExpenseTransactionModalTitle").text("Edit Expense Transaction");

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

    //txtPaymentMode_Income - change
    $(document).on("change", "#txtPaymentMode_Income", function () {
        const selectedText = $("#txtPaymentMode_Income option:selected").text().toLowerCase().trim();
        // Hide all conditional fields
        $(".incomePayment_Fields").addClass("d-none");
        // Clear hidden values (recommended)
        $(".incomePayment_Fields input").val("");
        // CASH
        if (selectedText.includes("cash")) {
            // Nothing extra required
        }
        // BANK TRANSFER
        else if (selectedText.includes("bank transfer")) {
            //$("#txtTransactionId_Income").closest(".incomePayment_Fields").removeClass("d-none");
        }
        // CHEQUE
        else if (selectedText.includes("cheque")) {
            $("#paymentChequebank_Income").closest(".incomePayment_Fields").removeClass("d-none");
            $("#paymentChequeNo_Income").closest(".incomePayment_Fields").removeClass("d-none");
            $("#paymentChequeDate_Income").closest(".incomePayment_Fields").removeClass("d-none");
        }
        // UPI / CARD / ONLINE
        else if (selectedText.includes("upi") || selectedText.includes("card") || selectedText.includes("online")) {
            $("#txtTransactionId_Income").closest(".incomePayment_Fields").removeClass("d-none");
        }
    });
    //EXPENSE TXN --- END --------------------------

    //INCOME TXN --- START -------------------------
    loadIncomeCategoryDropdown();
    loadIncomeSubCategoryDropdown();
    fetchIncomeTransactions();

    document.getElementById('txtTransactionDate_Income').value = today;

    // Add button click
    $("#btn-add-credit-transactions").click(function () {
        resetFormFields_Income();
        $("#incomeTransactionModalTitle").text("Add Income Transactions");
        $("#txtTransactionDate_Income").val(formatDate2());
        $("#incomeTransactionModal").modal("show");
    });

    // Save Income Transaction
    $("#btnSaveIncomeTransaction").click(function (e) {
        e.preventDefault();

        // Validation
        let mode = parseInt($("#txtPaymentMode_Income").val()) || 0;
        let account = parseInt($("#ddlAccount_Income").val()) || 0;
        let category = $("#ddlIncomeCategory").val();
        let amount = parseFloat($("#txtAmount_Income").val());
        let transactionDate = $("#txtTransactionDate_Income").val();

        // Mode
        if (mode <= 0) {
            Swal.fire("Validation", "Please select Mode", "warning");
            $("#txtPaymentMode_Income").focus();
            return false;
        }

        // Account
        if (account <= 0) {
            Swal.fire("Validation", "Please select Account", "warning");
            $("#ddlAccount_Income").focus();
            return false;
        }

        // Category
        if (!category) {
            Swal.fire("Validation", "Please select Category", "warning");
            $("#ddlIncomeCategory").focus();
            return false;
        }

        // Amount
        if (!amount || isNaN(amount) || amount <= 0) {
            Swal.fire("Validation", "Amount must be greater than 0", "warning");
            $("#txtAmount_Income").focus();
            return false;
        }

        // Transaction Date
        if (!transactionDate) {
            Swal.fire("Validation", "Please select Transaction Date", "warning");
            $("#txtTransactionDate_Income").focus();
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
                    //$("#incomeTransactionModal").modal("hide");
                    loadNextVoucherNo();
                    // Reload table or data
                    fetchIncomeTransactions();
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
        var table = $('#tableMain_Income table').DataTable();

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
        $("#txtTransactionId_Income").val(rowData.TransactionId);
        $("#paymentChequebank_Income").val(rowData.ChequeBank);
        $("#paymentChequeNo_Income").val(rowData.ChequeNo);
        $("#paymentChequeDate_Income").val(rowData.ChequeDate);
        $("#txtNotes_Income").val(rowData.Notes);

        // Update modal title
        $("#incomeTransactionModalTitle").text("Edit Income Transactions");

        // Show modal
        $('#incomeTransactionModal').modal('show');
    });

    // Delete button click in table
    $(document).on("click", ".btn-delete-income", function () {
        // Get DataTable instance
        var table = $("#tableMain_Income table").DataTable(); // make sure this matches your table id
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
                        Swal.fire("Deleted!", response.message, "success");
                        $("#deleteRecordModal").modal("hide");

                        // Reload table
                        fetchIncomeTransactions();
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
                        Swal.fire("Deleted!", response.message, "success");
                        $("#deleteRecordModal").modal("hide");

                        // Reload table
                        fetchExpenseTransactions();
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

    //txtPaymentMode_Expense - change
    $(document).on("change", "#txtPaymentMode_Expense", function () {
        const selectedText = $("#txtPaymentMode_Expense option:selected").text().toLowerCase().trim();
        // Hide all conditional fields
        $(".expensePayment_Fields").addClass("d-none");
        // Clear hidden values (important to avoid stale data)
        $(".expensePayment_Fields input").val("");

        // CASH
        if (selectedText.includes("cash")) {
            // Nothing extra required
        }
        // BANK TRANSFER
        else if (selectedText.includes("bank transfer")) {
            //$("#txtTransactionId_Expense").closest(".expensePayment_Fields").removeClass("d-none");
        }
        // CHEQUE
        else if (selectedText.includes("cheque")) {
            $("#paymentChequebank_Expense").closest(".expensePayment_Fields").removeClass("d-none");
            $("#paymentChequeNo_Expense").closest(".expensePayment_Fields").removeClass("d-none");
            $("#paymentChequeDate_Expense").closest(".expensePayment_Fields").removeClass("d-none");
        }
        // UPI / CARD / ONLINE
        else if (selectedText.includes("upi") || selectedText.includes("card") || selectedText.includes("online")) {
            $("#txtTransactionId_Expense").closest(".expensePayment_Fields").removeClass("d-none");
        }
    });
    //INCOME TXN --- END ---------------------------
});

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
            //console.log("res count summaery finance:",res);
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
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fail to load FinanceSummary data")
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
// Fetch Expense Transactions
function fetchExpenseTransactions(accountId = 0, paymentModeId = 0, categoryId = 0, subCategoryId = 0, fromDate = "", toDate = "") {
    const payload = {
        Id: 0,               // 0 = all transactions, or pass a specific Id
        AccountId: accountId,
        PaymentModeId: paymentModeId,
        CategoryId: categoryId,
        SubCategoryId: subCategoryId,
        FromDate: fromDate,  // format: 'yyyy-mm-dd'
        ToDate: toDate       // format: 'yyyy-mm-dd'
    };

    $.ajax({
        url: "/api/ExpenseTransactions/GetExpenseTransactionList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("Expense Transactions:", response);
            var data = response.data || [];
            renderExpenseTransactionTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fail fetching Expense Transactions.");
        }
    });
}

// Render DataTable
function renderExpenseTransactionTable(data) {
    if ($.fn.dataTable.isDataTable("#tableMain_Expense table")) {
        $("#tableMain_Expense table").DataTable().destroy();
    }

    // Add Sr No
    data.forEach((x, i) => x.srno = i + 1);

    $("#tableMain_Expense table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "#" },
            { data: "AccountName", title: "Account" },
            //{ data: "CategoryName", title: "Category" },
            { data: "SubCategoryName", title: "Subcategory" },
            { data: "TransactionDate2", title: "Txn Date" },
            //{ data: "PaymentModeName", title: "Payment Mode" },
            //{ data: "TransactionId", title: "Txn Id" },
            //{ data: "Notes", title: "Notes" },
            { data: "Amount", title: "Amount", className: "text-end" },
            {
                data: null,
                title: "Actions",
                orderable: false,
                searchable: false,
                className: "text-center",
                render: function (data, type, row) {
                    return `
                        <div class="dropdown">
                            <button class="btn btn-sm btn-light border-0"
                                    type="button"
                                    data-bs-toggle="dropdown"
                                    aria-expanded="false">
                                <i class="fa fa-ellipsis-v"></i>
                            </button>
                            <ul class="dropdown-menu dropdown-menu-end">
                                <li>
                                    <a class="dropdown-item btn-edit-expense" href="#">
                                        <i class="fa fa-edit me-2 text-warning"></i> Edit
                                    </a>
                                </li>
                                <li>
                                    <a class="dropdown-item btn-delete-expense" href="#">
                                        <i class="fa fa-trash me-2 text-danger"></i> Delete
                                    </a>
                                </li>
                            </ul>
                        </div>
                    `;
                }
            }
        ],
        pageLength: 10,
        ordering: true,

        paging: false,
        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows
        scrollX: true,            // horizontal scroll if needed

        searching: true,
        lengthChange: false,
        pagingType: "full_numbers"
    });
}

// Optional: Reset form fields
function resetFormFields_Expense() {
    $("#ExpenseTransactionId").val(0);

    $("#ddlExpenseCategory").val("0"); //.trigger("change");
    $("#ddlSubCategory_Expense").val("0"); //.trigger("change");

    $("#ddlAccount_Expense").val("0"); //.trigger("change");

    $("#txtAmount_Expense").val("0");
    $("#txtTransactionDate_Expense").val("");

    $("#txtPaymentMode_Expense").val("0").trigger("change");
    $("#txtTransactionId_Expense").val("");

    $("#paymentChequebank_Expense").val("");
    $("#paymentChequeNo_Expense").val("");
    $("#paymentChequeDate_Expense").val("");

    $("#txtNotes_Expense").val("");

    $("#paymentVoucherNo_Expense").val(nextExpenseVoucherNo);
    //loadNextVoucherNo();
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
        }
    });
}

// Trigger SubCategory load when Expense Category changes
$("#ddlExpenseCategory, #filter_expense_category").change(function () {
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
            handleApiError(xhr, status, error, "Failed to load account dropdown data");
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
// Fetch Income Transactions
function fetchIncomeTransactions(accountId = 0, paymentModeId = 0, categoryId = 0, subCategoryId = 0, fromDate = "", toDate = "") {
    const payload = {
        Id: 0,               // 0 = all transactions, or pass a specific Id
        AccountId: accountId,
        PaymentModeId: paymentModeId,
        CategoryId: categoryId,
        SubCategoryId: subCategoryId,
        FromDate: fromDate,  // format: 'yyyy-mm-dd'
        ToDate: toDate       // format: 'yyyy-mm-dd'
    };
    //console.log("payload:", payload);
    $.ajax({
        url: "/api/IncomeTransactions/GetIncomeTransactionList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //  console.log("Income Transactions:", response);
            var data = response.data || [];
            renderIncomeTransactionTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "fail fetching Income Transactions");
        }
    });
}

// Render DataTable
function renderIncomeTransactionTable(data) {
    if ($.fn.dataTable.isDataTable("#tableMain_Income table")) {
        $("#tableMain_Income table").DataTable().destroy();
    }

    // Add Sr No
    data.forEach((x, i) => x.srno = i + 1);
    $("#tableMain_Income  table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "#" },
            { data: "AccountName", title: "Account" },
            //{ data: "CategoryName", title: "Category" },
            { data: "SubCategoryName", title: "Subcategory" },
            { data: "TransactionDate2", title: "Txn Date" },
            //{ data: "PaymentModeName", title: "Payment Mode" },
            { data: "Amount", title: "Amount", className: "text-end" },
            //{ data: "TransactionId", title: "Txn Id" },
            {
                data: null,
                title: "Actions",
                orderable: false,
                searchable: false,
                className: "text-center",
                render: function (data, type, row) {
                    return `
                        <div class="dropdown">
                            <button class="btn btn-sm btn-light border-0"
                                    type="button"
                                    data-bs-toggle="dropdown"
                                    aria-expanded="false">
                                <i class="fa fa-ellipsis-v"></i>
                            </button>
                            <ul class="dropdown-menu dropdown-menu-end">
                                <li>
                                    <a class="dropdown-item btn-edit-income" href="#">
                                        <i class="fa fa-edit me-2 text-warning"></i> Edit
                                    </a>
                                </li>
                                <li>
                                    <a class="dropdown-item btn-delete-income" href="#">
                                        <i class="fa fa-trash me-2 text-danger"></i> Delete
                                    </a>
                                </li>
                            </ul>
                        </div>
                    `;
                }
            }
        ],
        pageLength: 10,
        ordering: true,
        searching: true,
        lengthChange: false,

        paging: false,
        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows
        scrollX: true,            // horizontal scroll if needed

        pagingType: "full_numbers"
    });
}

// Optional: Reset form fields
function resetFormFields_Income() {
    $("#incomeTransactionId").val(0);

    $("#ddlIncomeCategory").val("0"); //.trigger("change");
    $("#ddlSubCategory_Income").val("0"); //.trigger("change");

    $("#ddlAccount_Income").val("0"); //.trigger("change");

    $("#txtAmount_Income").val("0");
    $("#txtTransactionDate_Income").val("");

    $("#txtPaymentMode_Income").val("0").trigger("change");
    $("#txtTransactionId_Income").val("");

    $("#paymentChequebank_Income").val("");
    $("#paymentChequeNo_Income").val("");
    $("#paymentChequeDate_Income").val("");

    $("#txtNotes_Income").val("");

    // Example: auto-fill fields
    $("#paymentVoucherNo_Income").val(nextIncomeVoucherNo);

    //loadNextVoucherNo();
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