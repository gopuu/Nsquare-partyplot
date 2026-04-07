let rowIndex = 0;
let nextInvoiceNo = 0;
$(document).ready(function () {
    loadAssetsMasterDropdown();
    fetchData();

    // Add button click
    $(document).on('click', '#btn-add', function (event) {
        // Check Add Permission from Session or Local Storage
        var canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';
        if (!canAdd) {
            // Display Access Denied Message
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Add. Please contact admin.",
                confirmButtonText: "OK"
            });
            event.preventDefault(); // Stop the modal from opening
            $('#assetPurchaseModal').modal('hide');
        } else {
            resetFormFields(); // Reset form fields when modal is shown
            $("#purchaseModalTitle").text("Add Purchase Order");
            $("#assetPurchaseModal").modal("show");
        }
    });

    $("#btnResetForm").on("click", function () {
        resetPurchaseForm();
    });

    // Edit button click
    $(document).on("click", ".btn-edit", function (event) {

        event.preventDefault();

        var canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';

        if (!canEdit) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Edit. Please contact admin."
            });
            return;
        }

        resetFormFields('edit');

        let table = $('#tableMain table').DataTable();
        let rowData = table.row($(this).closest("tr")).data();

        if (!rowData) return;

        // =========================================
        // HEADER DATA
        // =========================================
        console.log("rowData:", rowData);

        $("#purchaseId").val(rowData.Id);

        $("#purchaseDate").val(rowData.PurchaseDate ?? "");
        $("#vendorName").val(rowData.VendorName ?? "");
        $("#invoiceNo").val(rowData.InvoiceNo ?? "");
        $("#remarks").val(rowData.Remarks ?? "");
        $("#netTotalAmount").val(rowData.NetTotalAmount ?? "0");

        // =========================================
        // LOAD ITEMS
        // =========================================

        $("#purchaseItemsBody").empty();
        rowIndex = 0;

        if (Array.isArray(rowData.Items) && rowData.Items.length > 0) {
            rowData.Items.forEach(item => {
                let newRow = buildRowForEdit(item);
                $("#purchaseItemsBody").append(newRow);
            });
        }
        else {
            addNewRow();
        }
        updateSrNo();
        //calculateSubTotals();

        // =========================================
        // OPEN MODAL
        // =========================================

        $("#purchaseModalTitle").text("Edit Asset Purchase");
        $("#assetPurchaseModal").modal("show");

    });

    // SAVE - Handle form submission
    $("#btnSavePurchase").click(function (event) {
        event.preventDefault();
        if (!validatePurchaseForm()) return;

        let purchaseId = $("#purchaseId").val();
        let payload = {
            Id: purchaseId,
            PurchaseDate: $("#purchaseDate").val(),
            VendorName: $("#vendorName").val(),
            InvoiceNo: $("#invoiceNo").val(),
            Remarks: $("#remarks").val(),
            NetTotalAmount: parseFloat($("#netTotalAmount").val()) || 0,
            Items: collectItems(),
            ActionBy: sessionStorage.getItem("userId") || 0
        };

        $.ajax({
            url: "/api/AssetPurchase/SaveAssetPurchase",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            success: function (response) {
                if (response.status.toLowerCase() == "success") {
                    handleApiSuccess("Saved!");
                    fetchData();
                    resetFormFields();
                }
                else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Asset Purchase Save Error");
            }
        });

    });    
    function collectItems() {
        let items = [];
        $("#purchaseItemsBody tr").each(function () {
            let row = $(this);
            items.push({
                AssetMaster_Id: row.find(".assetSelect").val(),
                Qty: parseFloat(row.find(".qtyInput").val()) || 0,
                UnitPrice: parseFloat(row.find(".priceInput").val()) || 0,
                GrossTotalAmount: parseFloat(row.find(".amountInput").val()) || 0
            });
        });
        return items;
    }

    // Delete Event handler for remove button click
    $(document).on('click', '.btn-delete', function (event) {
        var canDelete = (sessionStorage.getItem('CanDelete') || localStorage.getItem('CanDelete')) === 'true';
        if (!canDelete) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Delete. Please contact admin.",
                confirmButtonText: "OK"
            });
            event.preventDefault(); // Stop navigation
            $('#deleteRecordModal').modal('hide');
        }
        else {
            // Get the DataTable instance
            var table = $('#tableMain table').DataTable();

            // Get the row object for the clicked Edit button
            var row = $(this).closest('tr');
            var rowData = table.row(row).data(); // Get the data object for this row

            // Extract values from the row data object
            var id = rowData.Id;

            $('#deleteRecordModal').data('id', id).modal('show');
        }
    });

    // Handle delete confirmation
    $('#delete-record').click(function () {
        var id = $('#deleteRecordModal').data('id');

        let payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $('#btn-loader').show();
        $.ajax({
            url: "/api/AssetPurchase/DeleteAssetPurchase",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess("Deleted!");
                }
                else {
                    Swal.fire({
                        title: "Delete Fail..!!",
                        text: response.message,
                        icon: "error"
                    });
                }
                fetchData();
                $('#deleteRecordModal').modal('hide');
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete Purchase failed");
            }
        });
        $('#deleteRecordModal').modal('hide');
    });
    
    // DELETE ROW
    $(document).on("click", ".btnDeleteRow", function () {
        $(this).closest("tr").remove();
        updateSrNo();
        calculateSubTotals();
    });

    // ADD NEW ROW BUTTON
    $("#btnAddPurchaseItem").on("click", function () {
        addNewRow();
    });

});

let assetMasterData = [];
function loadAssetsMasterDropdown() {
    $.ajax({
        url: "/api/AssetMaster/GetAssetMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            assetMasterData = res.data || [];
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Asset Master Failed");
        }
    });
}

/// TABLE ITEMS GRID ----- START -------------------------------
// CALL THIS ON RESET OR MODAL OPEN
function addInitialRow() {
    $("#purchaseItemsBody").empty();
    rowIndex = 0;
    addNewRow();
}

// ADD NEW ROW
function addNewRow() {

    rowIndex++;

    // ASSET DROPDOWN
    let assetDropdown = `<select class="form-select form-select-sm assetSelect">`;
    assetDropdown += `<option value="0">--Select Asset--</option>`;

    assetMasterData.forEach(x => {

        assetDropdown += `
            <option value="${x.Id}">
                ${x.AssetName ?? x.Name}
            </option>
        `;

    });
    assetDropdown += `</select>`;

    let newRow = $(`
        <tr data-index="${rowIndex}">

            <!-- SR NO -->
            <td class="text-center align-middle sr-no">${rowIndex}</td>

            <!-- Asset -->
            <td>
                ${assetDropdown}
            </td>

            <!-- Qty -->
            <td>
                <input type="number" 
                       class="form-control form-control-sm qtyInput" 
                       min="0" step="1" value="0">
            </td>

            <!-- Unit Price -->
            <td>
                <input type="number" 
                       class="form-control form-control-sm priceInput" 
                       min="0" step="1" value="0">
            </td>

            <!-- Amount -->
            <td>
                <input type="number" 
                       class="form-control form-control-sm amountInput" 
                       readonly value="0">
            </td>

            <!-- Delete -->
            <td class="text-center">
                <button class="btn btn-sm btn-outline-danger btnDeleteRow">
                    <i class="fa fa-trash"></i>
                </button>
            </td>

        </tr>
    `);

    $("#purchaseItemsBody").append(newRow);

    updateSrNo();

}
function buildRowForEdit(item) {

    rowIndex++;

    let assetId = item.AssetMaster_Id;
    let qty = item.Qty ?? 0;
    let price = item.UnitPrice ?? 0;
    let amount = item.GrossTotalAmount ?? 0;

    let assetDropdown = `<select class="form-select form-select-sm assetSelect">
        <option value="0">--Select Asset--</option>`;

    assetMasterData.forEach(x => {

        assetDropdown += `<option value="${x.Id}">
            ${x.AssetName ?? x.Name}
        </option>`;

    });

    assetDropdown += `</select>`;


    let row = $(`

        <tr data-index="${rowIndex}">

            <td class="text-center sr-no">${rowIndex}</td>

            <td>
                ${assetDropdown}
            </td>

            <td>
                <input type="number"
                       class="form-control form-control-sm qtyInput"
                       value="${qty}">
            </td>

            <td>
                <input type="number"
                       class="form-control form-control-sm priceInput"
                       value="${price}">
            </td>

            <td>
                <input type="number"
                       class="form-control form-control-sm amountInput"
                       value="${amount}" readonly>
            </td>

            <td class="text-center">
                <button class="btn btn-sm btn-outline-danger btnDeleteRow">
                    <i class="fa fa-trash"></i>
                </button>
            </td>

        </tr>

    `);

    row.find(".assetSelect").val(assetId);

    return row;

}

// UPDATE SR NO AFTER DELETE
function updateSrNo() {
    $("#purchaseItemsBody tr").each(function (i) {
        $(this).find(".sr-no").text(i + 1);
    });
}

/// TABLE ITEMS GRID ----- END -------------------------------

//get data from API
function fetchData() {

    var payload = { Id: 0 };
    $.ajax({
        url: "/api/AssetPurchase/GetAssetPurchase",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("asset Purchase Fetch:", response);
            var data = response.data ?? [];
            renderTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "AssetPurchase Fetch Error");
        }
    });
}

//SHOW GRID TABLE
function renderTable(data) {

    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    if (!data || data.length === 0) {

        $('#tableMain tbody').html(`
            <tr>
                <td colspan="7" class="text-center fw-bold text-danger">
                    No Data Found !!
                </td>
            </tr>
        `);

        $('#tableMain table').DataTable({
            data: [],
            columns: [
                { title: "#" },
                { title: "Purchase Date" },
                { title: "Vendor" },
                { title: "Invoice No" },
                { title: "Total Amount" },
                { title: "Action" }
            ]
        });

        return;
    }

    // Add SR No.
    data.forEach((x, i) => x.srno = i + 1);

    $('#tableMain table').DataTable({

        data: data,

        columns: [

            { data: "srno", title: "#" },

            {
                data: "PurchaseDate1",
                title: "Purchase Date"
            },

            {
                data: "VendorName",
                title: "Vendor",
                render: function (d) {
                    return d ? `<b>${d}</b>` : "-";
                }
            },

            {
                data: "InvoiceNo",
                title: "Invoice No",
                render: function (d) {
                    if (!d) return "-";
                    return `#${d}`;
                }
            },

            {
                data: "NetTotalAmount",
                title: "Total Amount",
                render: function (x) {
                    return `₹ ${x ?? 0}`;
                }
            },

            {
                data: null,
                title: "Action",
                render: function (data, type, row) {
                    return `
                        <div class="dropdown">

                            <button class="btn btn-sm btn-outline-secondary dropdown-toggle btn-action-xss"
                                    type="button"
                                    data-bs-toggle="dropdown"
                                    aria-expanded="false">
                                <i class="fa fa-ellipsis-v"></i>
                            </button>

                            <ul class="dropdown-menu dropdown-menu-end">

                                <li>
                                    <a class="dropdown-item btn-edit" href="javascript:void(0)" data-id="${row.Id}">
                                        <i class="fa fa-edit text-warning me-2"></i> Edit
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item btn-delete" href="javascript:void(0)" data-id="${row.Id}">
                                        <i class="fa fa-trash text-danger me-2"></i> Delete
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
        stateSave: false

    });

}

function fetchInvoiceNo() {

    $.ajax({
        url: "/api/AssetPurchase/GetNextAssetPurchaseInvoiceNumber",
        method: "GET",
        contentType: "application/json",
        success: function (response) {
            var data = response.data ?? [];
            nextInvoiceNo = data.NextInvoiceNumber;
            $("#invoiceNo").val(nextInvoiceNo);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "AssetPurchase Fetch InvoiceNo Error");
        }
    });
}

// Reset form fields
function resetFormFields(flag='add') {
    $("#purchaseId").val(0);
    $("#purchaseDate").val(new Date().toISOString().split("T")[0]);
    $("#vendorName").val("");
    if (flag === 'add') {
        fetchInvoiceNo();
    }
    $("#remarks").val("");
    $("#netTotalAmount").val("0");
    $("#purchaseItemsBody").empty();

    rowIndex = 0;
    addNewRow();   // important
    $(".modal-body").scrollTop(0);
}
function validatePurchaseForm() {

    // Vendor required
    if ($("#vendorName").val().trim() === "") {

        Swal.fire("Validation", "Vendor name is required", "warning");

        $("#vendorName").focus();

        return false;

    }

    // Purchase Date required
    if ($("#purchaseDate").val() === "") {

        Swal.fire("Validation", "Purchase date is required", "warning");

        $("#purchaseDate").focus();

        return false;

    }

    // Ensure items exist
    if ($("#purchaseItemsBody tr").length === 0) {

        Swal.fire("Validation", "Add at least one asset item", "warning");

        return false;

    }

    let isValid = true;

    $("#purchaseItemsBody tr").each(function () {

        let row = $(this);

        let assetId = row.find(".assetSelect").val();
        let qty = parseFloat(row.find(".qtyInput").val());
        let price = parseFloat(row.find(".priceInput").val());

        if (!assetId || assetId == "0") {

            Swal.fire("Validation", "Please select an Asset in all rows", "warning");

            row.find(".assetSelect").focus();

            isValid = false;

            return false;

        }

        if (!qty || qty <= 0) {

            Swal.fire("Validation", "Qty must be greater than 0", "warning");

            row.find(".qtyInput").focus();

            isValid = false;

            return false;

        }

        if (price === undefined || price < 0) {

            Swal.fire("Validation", "Unit price must be valid", "warning");

            row.find(".priceInput").focus();

            isValid = false;

            return false;

        }

    });

    return isValid;

}

//<!--BEHAVIOUR - START -->
$(document).on("input", ".qtyInput, .priceInput", function () {

    let row = $(this).closest("tr");

    let qty = parseFloat(row.find(".qtyInput").val()) || 0;
    let price = parseFloat(row.find(".priceInput").val()) || 0;

    let amount = qty * price;

    row.find(".amountInput").val(amount.toFixed(2));

    calculateSubTotals();

});
function calculateSubTotals() {
    let total = 0;
    $("#purchaseItemsBody tr").each(function () {
        let amount = parseFloat($(this).find(".amountInput").val()) || 0;
        total += amount;
    });
    $("#netTotalAmount").val(total.toFixed(2));
}

//<!--BEHAVIOUR - END -->

