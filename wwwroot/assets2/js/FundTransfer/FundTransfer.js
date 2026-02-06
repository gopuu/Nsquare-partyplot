$(document).ready(function () {
    loadAccountDropdowns();
    fetchFundTransfers();

    $(document).on("click", "#btn-add-fund-transfer", function () {
        var canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';

        if (!canAdd) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to add new record. Please contact admin.",
            });
            return;
        }

        resetFundTransferForm();
        $("#FundTransferModalTitle").text("Add Fund Transfer");
        $("#txtTransferDate").val(new Date().toISOString().split('T')[0]);
        //loadAccountDropdowns();
        $("#FundTransferModal").modal("show");
    });
    // SAVE FUND TRANSFER
    $("#btnSaveFundTransfer").click(function () {
        let model = {
            Id: $("#FundTransferId").val(),
            FromAccountId: $("#ddlFromAccount").val(),
            ToAccountId: $("#ddlToAccount").val(),
            Amount: $("#txtAmount").val(),
            TransferDate: $("#txtTransferDate").val(),
            ReferenceNo: $("#txtReferenceNo").val(),
            Notes: $("#txtNotes").val(),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };
        if (!model.FromAccountId || !model.ToAccountId || !model.Amount || !model.TransferDate) {
            Swal.fire("Required", "Please fill all mandatory fields.", "warning");
            return;
        }
        if (model.FromAccountId === model.ToAccountId) {
            Swal.fire("Invalid", "From Account and To Account cannot be same!", "warning");
            return;
        }
        $.ajax({
            url: "/api/FundTransfer/SaveFundTransfer",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(model),
            success: function (response) {
                if (response.status === "Success") {
                    Swal.fire("Success", response.message, "success");
                    $("#FundTransferModal").modal("hide");
                    resetFundTransferForm();
                    fetchFundTransfers(); // Reload grid
                } else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save FundTransfer Error")
            }
        });
    });
    // Edit button click for Fund Transfer
    $(document).on("click", ".btn-edit", function (event) {

        var canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';

        if (!canEdit) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Edit. Please contact admin.",
            });
            event.preventDefault();
            return;
        }

        resetFundTransferForm(); // Clear previous modal values

        // Get DataTable instance
        var table = $('#tableMain table').DataTable();

        // Get clicked row data
        var row = $(this).closest('tr');
        var rowData = table.row(row).data();

        //console.log("Editing Fund Transfer:", rowData);

        // Fill hidden ID field
        $("#FundTransferId").val(rowData.Id);

        // Load dropdown and then select values
        bindDropdown("#ddlFromAccount", accountList);
        $("#ddlFromAccount").val(rowData.FromAccountId).trigger("change");

        bindDropdown("#ddlToAccount", accountList, rowData.FromAccountId);
        $("#ddlToAccount").val(rowData.ToAccountId);

        // Amount
        $("#txtAmount").val(rowData.Amount);

        // Set date (yyyy-mm-dd format)
        if (rowData.TransferDate) {
            let date = new Date(rowData.TransferDate);
            let yyyy = date.getFullYear();
            let mm = ("0" + (date.getMonth() + 1)).slice(-2);
            let dd = ("0" + date.getDate()).slice(-2);
            $("#txtTransferDate").val(`${yyyy}-${mm}-${dd}`);
        } else {
            $("#txtTransferDate").val(new Date().toISOString().split('T')[0]);
        }

        $("#txtReferenceNo").val(rowData.ReferenceNo);
        $("#txtNotes").val(rowData.Notes);

        // Update Title
        $("#FundTransferModalTitle").text("Edit Fund Transfer");

        // Show Modal
        $("#FundTransferModal").modal("show");
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

    $("#delete-record").click(function () {
        var id = $("#deleteRecordModal").data("id");
        if (!id) {
            Swal.fire("Error", "Record Id not found.", "error");
            return;
        }
        $.ajax({
            url: `/api/FundTransfer/DeleteFundTransfer`,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: id }),
            success: function (response) {
                Swal.fire("Deleted!", response.message || "Record deleted.", "success");
                $("#deleteRecordModal").modal("hide");
                fetchFundTransfers();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Something went wrong!")
            }
        });
    });
});

let accountList = [];
function loadAccountDropdowns() {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
        success: function (response) {
            accountList = response?.data ?? [];
            //console.log("accountList", accountList);

            // Only accounts allowed for transfer
            var staffIds = Number(sessionStorage.getItem("userId") ?? 0);
            loadStaffAccountsRights([staffIds]).then(() => {
                // Only accounts allowed for transfer
                const transferableAccounts = getAccountsByPermission("CanTransfer");
                console.log("transferableAccounts:", transferableAccounts);

                bindDropdown("#ddlFromAccount", transferableAccounts);
                bindDropdown("#ddlToAccount", transferableAccounts);
            });
            // bindDropdown("#ddlFromAccount", accountList);
            // bindDropdown("#ddlToAccount", accountList);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading account list")
        }
    });
}
function bindDropdown(selectId, data, excludeId = null, selectedValue = null) {
    $(selectId).empty();
    $(selectId).append(`<option value="">-- Select Account --</option>`);

    data.forEach(item => {
        // allow showing selected value even if matching excludeId
        if (excludeId == null || item.Id != excludeId || item.Id == selectedValue) {
            const selected = item.Id == selectedValue ? "selected" : "";
            $(selectId).append(`<option value="${item.Id}" ${selected}>${item.AccountName}</option>`);
        }
    });
}

//----- ACCOUNTS RIGHTS -- START ------------------------------------
var accountRightsCache = [];
/* Load account rights for selected staff */
function loadStaffAccountsRights(staffIds = []) {
    console.log("layout- staffIds:", staffIds);

    return $.ajax({
        url: "/api/AccountRights/GetAccountRights",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ StaffIds: staffIds }),
        success: function (res) {
            console.log("layout- AccountRights res:", res);
            accountRightsCache = res?.data ?? [];
        },
        error: function () {
            accountRightsCache = [];
        }
    });
}
function getAccountsByPermission(permission) {
    return (accountRightsCache || []).filter(a => a[permission] === true);
}
//----- ACCOUNTS RIGHTS -- END ------------------------------------




$(document).on("change", "#ddlFromAccount", function () {
    let selectedFrom = $(this).val();
    let selectedTo = $("#ddlToAccount").val();
    bindDropdown("#ddlToAccount", accountList, selectedFrom, selectedTo);
});

$(document).on("change", "#ddlToAccount", function () {
    let selectedTo = $(this).val();
    let selectedFrom = $("#ddlFromAccount").val();
    bindDropdown("#ddlFromAccount", accountList, selectedTo, selectedFrom);
});

function resetFundTransferForm() {
    $("#FundTransferForm")[0].reset(); // Agar form id hai
    bindDropdown("#ddlFromAccount", accountList);
    bindDropdown("#ddlToAccount", accountList);
}

// RESET FORM
function resetFundTransferForm() {
    $("#FundTransferId").val(0);
    $("#ddlFromAccount").val("");
    $("#ddlToAccount").val("");
    $("#txtAmount").val("");
    $("#txtReferenceNo").val("");
    $("#txtNotes").val("");
    // Reset to today's date
    document.getElementById("txtTransferDate").valueAsDate = new Date();
}

// FETCH Fund Transfer Records
function fetchFundTransfers(fromAccountId = 0, toAccountId = 0, fromDate = "", toDate = "") {
    $('#btn-loader').show(); // loader dikhao
    const payload = {
        Id: 0, // 0 means get all
        FromAccountId: fromAccountId,
        ToAccountId: toAccountId,
        FromDate: fromDate || null,
        ToDate: toDate || null
    };
    $.ajax({
        url: "/api/FundTransfer/GetFundTransferList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            // console.log("Fund Transfers:", response);

            if (response.status === "Success") {
                renderFundTransferTable(response.data || []);
            } else {
                Swal.fire("Error!", response.message, "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Something went wrong fetching Fund Transfers.")
        },
        complete: function () {
            $('#btn-loader').hide(); // loader hide
        }
    });
}

function renderFundTransferTable(data) {

    if ($.fn.dataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }

    // add Sr.No dynamically
    data.forEach((x, i) => x.srno = i + 1);

    $("#tableMain table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr. No." },
            { data: "FromAccountName", title: "From Account" },
            { data: "ToAccountName", title: "To Account" },
            {
                data: "Amount",
                title: "Amount",
                className: "text-end",
                render: (val) => `<b>₹ ${val.toLocaleString()}</b>`
            },
            {
                data: "TransferDate",
                title: "Transfer Date",
                render: (val) => val ? new Date(val).toLocaleDateString("en-IN") : ""
            },
            { data: "ReferenceNo", title: "Reference #" },
            { data: "Notes", title: "Notes" },
            // ACTION BUTTONS
            {
                data: null,
                title: "Actions",
                className: "text-center",
                render: (row) => `
                            <button class="btn btn-outline-warning btn-sm  btn-action-xss  btn-edit" data-id="${row.Id}">
                        <i class="fa fa-edit"></i>
                    </button>
                            <button class="btn btn-outline-danger  btn-action-xss  btn-sm btn-delete" data-id="${row.Id}">
                        <i class="fa fa-trash"></i>
                    </button>
                `
            }
        ],
        pageLength: 10,
        ordering: true,
        searching: true,
        pagingType: "full_numbers",
        responsive: true
    });
}
