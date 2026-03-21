
$(document).ready(function () {
    loadIncomeCategoryDropdown();
    loadIncomeSubCategoryDropdown();
    loadAccountDropdown();
    fetchIncomeTransactions();

    const today = new Date().toISOString().split('T')[0];
    document.getElementById('txtTransactionDate').value = today;

    // Add button click
    $("#btn-add-transactions").click(function () {
        resetFormFields();
        $("#incomeTransactionModalTitle").text("Add Income Transactions");
        $("#txtTransactionDate").val(new Date().toISOString().split('T')[0]); // set today
        $("#incomeTransactionModal").modal("show");
    });

    // Save Income Transaction
    $("#btnSaveIncomeTransaction").click(function (e) {
        e.preventDefault();
        // Basic validation
        if ($("#ddlAccount").val() === "") {
            Swal.fire("Validation", "Please select Account", "warning");
            return;
        }
        if ($("#ddlCategory").val() === "") {
            Swal.fire("Validation", "Please select Category", "warning");
            return;
        }
        if ($("#txtAmount").val().trim() === "" || parseFloat($("#txtAmount").val()) <= 0) {
            Swal.fire("Validation", "Amount must be greater than 0", "warning");
            return;
        }
        if ($("#txtTransactionDate").val() === "") {
            Swal.fire("Validation", "Please select Transaction Date", "warning");
            return;
        }
        let payload = {
            Id: $("#incomeTransactionId").val(),
            AccountId: $("#ddlAccount").val(),
            CategoryId: $("#ddlIncomeCategory").val(), // Corrected
            SubCategoryId: $("#ddlSubCategory").val() || null,
            Amount: parseFloat($("#txtAmount").val()),
            TransactionDate: $("#txtTransactionDate").val(),
            PaymentMode: $("#txtPaymentMode").val().trim() || null,
            ReferenceNo: $("#txtReferenceNo").val().trim() || null,
            Notes: $("#txtNotes").val().trim() || null,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };
        // console.log("SAVE PAYLOAD:", payload);
        $.ajax({
            url: "/api/IncomeTransactions/SaveIncomeTransaction",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess("Income Transaction saved successfully.");
                    // Reset form & hide modal
                    resetFormFields();
                    $("#incomeTransactionModal").modal("hide");

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
    $(document).on('click', '.btn-edit', function (event) {
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

        resetFormFields(); // Clear previous values

        // Get the DataTable instance
        var table = $('#tableMain table').DataTable();

        // Get the row object for the clicked Edit button
        var row = $(this).closest('tr');
        var rowData = table.row(row).data(); // Get the data object for this row

        // Fill modal fields with selected row data
        $("#incomeTransactionId").val(rowData.Id);

        // Load Account dropdown first, then select value
        loadAccountDropdown(rowData.AccountId);

        // Load Income Category dropdown, then select value
        loadIncomeCategoryDropdown(rowData.CategoryId);

        // Load SubCategory based on selected category, then select value
        loadIncomeSubCategoryDropdown(rowData.CategoryId, rowData.SubCategoryId);

        $("#txtAmount").val(rowData.Amount);

        // Format date as yyyy-mm-dd
        if (rowData.TransactionDate) {
            let date = new Date(rowData.TransactionDate);
            let yyyy = date.getFullYear();
            let mm = ("0" + (date.getMonth() + 1)).slice(-2);
            let dd = ("0" + date.getDate()).slice(-2);
            $("#txtTransactionDate").val(`${yyyy}-${mm}-${dd}`);
        } else {
            $("#txtTransactionDate").val(new Date().toISOString().split('T')[0]);
        }

        $("#txtPaymentMode").val(rowData.PaymentMode);
        $("#txtReferenceNo").val(rowData.ReferenceNo);
        $("#txtNotes").val(rowData.Notes);

        // Update modal title
        $("#incomeTransactionModalTitle").text("Edit Income Transactions");

        // Show modal
        $('#incomeTransactionModal').modal('show');
    });

    // Delete button click in table
    $(document).on("click", ".btn-delete", function () {
        // Get DataTable instance
        var table = $("#tableMain table").DataTable(); // make sure this matches your table id
        var rowData = table.row($(this).closest("tr")).data();

        if (!rowData || !rowData.Id) {
            Swal.fire("Error", "Unable to get selected row data.", "error");
            return;
        }

        // Store Id in modal using jQuery data
        $("#deleteRecordModal").data("id", rowData.Id).modal("show");
    });

    // Confirm delete
    $("#delete-record").click(function () {
        var id = $("#deleteRecordModal").data("id");

        if (!id) {
            Swal.fire("Error", "Record Id not found.", "error");
            return;
        }

        let payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

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
                handleApiError(xhr, status, error, "Something went wrong!");
            }
        });
    });

});

// Optional: Reset form fields
function resetFormFields() {
    $("#incomeTransactionId").val(0);
    $("#ddlAccount").val("");
    $("#ddlCategory").val("");
    $("#ddlSubCategory").val("");
    $("#txtAmount").val("");
    $("#txtTransactionDate").val("");
    $("#txtPaymentMode").val("");
    $("#txtReferenceNo").val("");
    $("#txtNotes").val("");
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

            dropdown.append(`<option value="">-- Select Income Category --</option>`);

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
        $("#ddlSubCategory").empty().append('<option value="">-- Select SubCategory --</option>');
        return;
    }
    $.ajax({
        url: "/api/IncomeSubCategory/GetIncomeSubCategoryList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, CategoryId: categoryId }),
        success: function (response) {
            let dropdown = $("#ddlSubCategory");
            dropdown.empty();
            dropdown.append('<option value="">-- Select SubCategory --</option>');

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

// Load Account dropdown dynamically
function loadAccountDropdown(selectedId = "") {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
        success: function (response) {
            let dropdown = $("#ddlAccount");
            dropdown.empty(); // clear previous values
            dropdown.append('<option value="">-- Select Account --</option>');

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
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}

// Fetch Income Transactions
function fetchIncomeTransactions(accountId = 0, categoryId = 0, fromDate = "", toDate = "") {
    $('#btn-loader').show(); // optional loader

    const payload = {
        Id: 0,               // 0 = all transactions, or pass a specific Id
        AccountId: accountId,
        CategoryId: categoryId,
        FromDate: fromDate,  // format: 'yyyy-mm-dd'
        ToDate: toDate       // format: 'yyyy-mm-dd'
    };

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
            handleApiError(xhr, status, error, "Something went wrong fetching Income Transactions");
        }
    });
}

// Render DataTable
function renderIncomeTransactionTable(data) {
    if ($.fn.dataTable.isDataTable("#tableMain  table")) {
        $("#tableMain  table").DataTable().destroy();
    }

    // Add Sr No
    data.forEach((x, i) => x.srno = i + 1);
    $("#tableMain  table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr. No." },
            { data: "AccountName", title: "Account" },
            { data: "CategoryName", title: "Category" },
            { data: "SubCategoryName", title: "Subcategory" },
            { data: "Amount", title: "Amount", className: "text-end" },
            { data: "TransactionDate", title: "Txn Date" },
            { data: "PaymentModeName", title: "Payment Mode" },
            { data: "TransactionId", title: "Txn Id" },
            { data: "Notes", title: "Notes" },
            {
                data: null,
                title: "Actions",
                render: (row) => `
                            <button class="btn btn-outline-warning  btn-action-xss btn-edit"><i class="fa fa-edit"></i></button>
                            <button class="btn btn-outline-danger  btn-action-xss btn-delete"><i class="fa fa-trash"></i></button>
                    `
            }
        ],
        pageLength: 10,
        ordering: true,
        searching: true,
        lengthChange: false,
        pagingType: "full_numbers"
    });
}
