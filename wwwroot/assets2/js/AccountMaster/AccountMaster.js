$(document).ready(function () {
    fetchData();
    //loadAccountTypeDropdown();

    $("#btn-add").click(function () {
        if (!checkPermission("CanAdd")) return;
        resetForm();
        $("#accountModalTitle").text("Add Account");
        $("#accountModal").modal("show");
    });
    /*
    function loadAccountTypeDropdown() {
        var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

        $.ajax({
            url: "/api/AccountTypeMaster/GetAccountTypeList",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
            success: function (response) {
                console.log("response:", response);
                if (response.status === "Success" && response.data.length > 0) {

                    let dropdown = $("#accountType");
                    dropdown.empty(); // clear dropdown

                    dropdown.append(`<option value="0">-- Select --</option>`);

                    response.data.forEach(item => {
                        dropdown.append(`<option value="${item.Id}">${item.TypeName}</option>`);
                    });
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Error loading Account Type");
            }
        });
    }
    */
    $("#btnSaveAccount").click(function () {
        if (!validateForm()) return;
        saveAccount();
    });

    $(document).on("click", ".btn-edit", function () {
        // Permission check
        if (!checkPermission("CanEdit")) return;
        resetForm(); // Clear previous data
        // Get datatable row data
        let table = $("#tableMain table").DataTable();
        let rowData = table.row($(this).closest("tr")).data();

        // Fill modal fields
        $("#accountId").val(rowData.Id);
        $("#accountName").val(rowData.AccountName);
        //$("#accountType").val(rowData.AccountType).trigger("change");
        //$("#openingBalance").val(rowData.OpeningBalance);
        //$("#currentBalance").val(rowData.CurrentBalance);

        // Bank fields
        //$("#bankName").val(rowData.BankName);
        //$("#branchName").val(rowData.BranchName);
        //$("#ifsc").val(rowData.IFSC);
        //$("#accountNumber").val(rowData.AccountNumber);
        //$("#accountHolderName").val(rowData.AccountHolderName);

        // UPI
        //$("#upiId").val(rowData.UPI_Id);

        // General fields
        //$("#description").val(rowData.Description);
        //$("#isDefaultAccount").prop("checked", rowData.IsDefaultAccount);

        // Change modal title and open modal
        $("#accountModalTitle").text("Edit Account");
        $("#accountModal").modal("show");
    });

    $(document).on("click", ".btn-delete", function () {
        if (!checkPermission("CanDelete")) return;
        let table = $("#tableMain table").DataTable();
        let data = table.row($(this).closest("tr")).data();
        $("#deleteModal").data("id", data.Id).modal("show");
    });
    $("#confirmDelete").click(deleteAccount);
});

// PERMISSION CHECK
function checkPermission(type) {
    let allowed = (sessionStorage.getItem(type) || localStorage.getItem(type)) === "true";
    if (!allowed) Swal.fire("Access Denied", "Permission Required", "error");
    return allowed;
}

// FETCH DATA
function fetchData() {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
        success: function (res) {
            if (res.status === "Success") {
                renderTable(res.data);
            } else {
                console.warn("API Error:", res.message);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "fetching Account Data failed")
        },
    });
}

// RENDER TABLE
function renderTable(data) {

    if ($.fn.DataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().clear().destroy();
    }

    $("#tableMain table").DataTable({
        data: data,
        responsive: true,
        pageLength: 10,
        scrollX: true,

        paging: false,
        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows

        columns: [
            { data: "Id", title: "Sr. No", visible: true },
            { data: "AccountName", title: "Account Name" },
            /*{
                data: "OpeningBalance",
                title: "Opening Bal",
                render: d => d?.toLocaleString("en-IN")
            },
            {
                data: "CurrentBalance",
                title: "Current Bal",
                render: d => d?.toLocaleString("en-IN")
            },
            { data: "BankName", title: "Bank Name" },
            { data: "BranchName", title: "Branch" },
            { data: "IFSC", title: "IFSC Code" },
            { data: "AccountNumber", title: "Account No." },
            { data: "AccountHolderName", title: "Holder Name" },
            {
                data: "IsDefaultAccount",
                title: "Default",
                render: d => d
                    ? `<span class="badge bg-success">Yes</span>`
                    : `<span class="badge bg-secondary">No</span>`
            },
            */
            {
                data: null,
                title: "Action",
                orderable: false,
                render: (data, type, row) => `
                        <button class="btn btn-sm btn-outline-warning btn-action-xss btn-edit" data-id="${row.Id}">
                        <i class="fa fa-edit"></i>
                    </button>
                        <button class="btn btn-sm btn-outline-danger btn-action-xss btn-delete" data-id="${row.Id}">
                        <i class="fa fa-trash"></i>
                    </button>`
            }
        ]
    });
}

// RESET FORM
function resetForm() {
    //$("input, textarea").val("");
    $("#accountId").val(0);
    $("#accountName").val("");
    //$("#accountType").val("0");
    //$("#isDefaultAccount").prop("checked", false);
}
// FILL FORM ON EDIT
function fillForm(d) {
    $("#accountId").val(d.Id);
    $("#accountName").val(d.AccountName);
    //$("#accountType").val(d.AccountType).trigger("change");
    //$("#openingBalance").val(d.OpeningBalance);
    //$("#bankName").val(d.BankName);
    //$("#upiId").val(d.UPI);
    //$("#description").val(d.Description);
    //$("#isDefaultAccount").prop("checked", d.IsDefault);
}
// SAVE
function saveAccount() {
    let payload = {
        Id: $("#accountId").val(),
        AccountName: $("#accountName").val().trim(),
        //AccountType: $("#accountType").val(),
        //OpeningBalance: $("#openingBalance").val(),
        //CurrentBalance: $("#currentBalance").val(),
        //BankName: $("#bankName").val(),
        //BranchName: $("#branchName").val(),
        //IFSC: $("#ifsc").val(),
        //AccountNumber: $("#accountNumber").val(),
        //AccountHolderName: $("#accountHolderName").val(),
        //UPI_Id: $("#upiId").val(),
        //Description: $("#description").val(),
        //IsDefaultAccount: $("#isDefaultAccount").is(":checked"),
        ActionBy: sessionStorage.getItem("userId") ?? 0
    };
    $.ajax({
        url: "/api/AccountMaster/SaveAccount",
        type: "POST",
        data: JSON.stringify(payload),
        contentType: "application/json",
        success: function (res) {
            handleApiSuccess("Saved.");
            $("#accountModal").modal("hide");
            fetchData();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Save Account Error")
        }
    });
}

// DELETE
function deleteAccount() {
    let id = $("#deleteModal").data("id");
    let actionBy = sessionStorage.getItem("userId") ?? 0;

    $.ajax({
        url: "/api/AccountMaster/DeleteAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: id, ActionBy: actionBy }),
        success: function () {
            handleApiSuccess("Deleted!");
            $("#deleteModal").modal("hide");
            fetchData();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Delete Failed!")
        }
    });
}

// VALIDATION
function validateForm() {
    if ($("#accountName").val().trim() === "") {
        Swal.fire("Required", "Account Name is required", "warning");
        return false;
    }
    return true;
}
