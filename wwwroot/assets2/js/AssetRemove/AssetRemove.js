let rowIndex = 0;
$(document).ready(function () {
    loadBookingDropdown();
    loadAssetsMasterDropdown();
    loadRemoveReasonMasterDropdown();
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
            $('#assetRemoveModal').modal('hide');
        } else {
            resetFormFields(); // Reset form fields when modal is shown
            $("#RemoveModalTitle").text("Add Asset-Remove Details");
            $("#assetRemoveModal").modal("show");
        }
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

        console.log("rowData:", rowData);

        // =========================================
        // HEADER DATA
        // =========================================

        $("#RemoveId").val(rowData.Id);

        $("#bookingId").val(rowData.BookingId ?? 0);

        $("#removeDate").val(rowData.RemovedDate ?? "");

        $("#remarks").val(rowData.Remarks ?? "");

        // =========================================
        // LOAD ITEMS
        // =========================================

        $("#RemoveItemsBody").empty();
        rowIndex = 0;

        if (Array.isArray(rowData.Items) && rowData.Items.length > 0) {

            rowData.Items.forEach(item => {

                let newRow = buildRowForEdit(item);

                $("#RemoveItemsBody").append(newRow);

            });

        }
        else {

            addRemoveRow();

        }

        updateSrNo();

        // =========================================
        // OPEN MODAL
        // =========================================

        $("#RemoveModalTitle").text("Edit Asset Remove");

        $("#assetRemoveModal").modal("show");

    });

    // SAVE - Handle form submission
    $("#btnSaveRemove").click(function (event) {
        event.preventDefault();
        if (!validateRemoveForm()) return;

        let payload = {
            Id: $("#RemoveId").val(),
            BookingId: $("#bookingId").val(),
            RemovedDate: $("#removeDate").val(),
            Remarks: $("#remarks").val(),
            Items: collectRemoveItems(),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/AssetRemove/SaveAssetRemove",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            success: function (response) {
                if (response.status.toLowerCase() == "success") {
                    handleApiSuccess("Saved!");
                    fetchData();
                    resetFormFields();
                    loadAssetsMasterDropdown();
                }
                else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Asset Remove Save Error");
            }
        });

    });
    function collectRemoveItems() {

        let items = [];

        $("#RemoveItemsBody tr").each(function () {

            let row = $(this);

            items.push({

                AssetMaster_Id: row.find(".assetSelect").val(),

                Qty: parseFloat(row.find(".qtyInput").val()) || 0,

                AssetRemove_ReasonMaster_Id: row.find(".reasonSelect").val()

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
            url: "/api/AssetRemove/DeleteAssetRemove",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess("Deleted!");
                    fetchData();
                    loadAssetsMasterDropdown();
                }
                else {
                    Swal.fire({
                        title: "Delete Fail..!!",
                        text: response.message,
                        icon: "error"
                    });
                }
                $('#deleteRecordModal').modal('hide');
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete AssetRemove failed");
            }
        });
        $('#deleteRecordModal').modal('hide');
    });

    // DELETE ROW
    $(document).on("click", ".btnDeleteRow", function () {
        $(this).closest("tr").remove();
        updateSrNo();
    });

    // ADD NEW ROW BUTTON
    $("#btnAddRemoveItem").on("click", function () {
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

let reasonMasterData = [];
function loadRemoveReasonMasterDropdown() {
    $.ajax({
        url: "/api/AssetRemove_ReasonMaster/GetAssetRemoveReason",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            reasonMasterData = res.data || [];
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Reason Master Failed");
        }
    });
}

function loadBookingDropdown(bookingId = 0, banquetId = 0, eventDate = null) {
    var requestData = {
        BookingId: Number(bookingId),
        BanquetId: Number(banquetId),
        EventDate: eventDate,
        YearId: 0,
        MonthId: 0
    };
    //console.log("GetBookingCalendar requestData:", requestData);
    $.ajax({
        url: "/api/BookingCalendar/GetBookingCalendar",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.status === "Success") {
                var data = response.data; //BookingId, Client_Name (Booking_PAX, ShiftCode) 

                let dropdown = $("#bookingId");
                dropdown.empty();
                dropdown.append(`<option value="0">-- Select Booking --</option>`);
                data.forEach(function (item) {
                    dropdown.append(`<option value="${item.BookingId}">
                                        ${item.Client_Name} (PAX: ${item.Booking_PAX}, Slot: ${item.ShiftCode})
                                    </option>
                    `);
                });
            }
            else {
                Swal.fire({
                    icon: 'warning',
                    title: 'No Data Found',
                    text: response.message || 'No bookings available.',
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "getBooking fetch fail")
        }
    });
}


/// TABLE ITEMS GRID ----- START -------------------------------
// CALL THIS ON RESET OR MODAL OPEN
function addInitialRow() {
    $("#RemoveItemsBody").empty();
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
            <option value="${x.Id}" data-stock="${x.ClosingQty}">
                ${x.AssetName} (Available Stock: ${x.ClosingQty})
            </option>
        `;
    });
    assetDropdown += `</select>`;

    // REMOVE DROPDOWN
    let reasonDropdown = `<select class="form-select form-select-sm reasonSelect">`;
    reasonDropdown += `<option value="0">--Select Reason--</option>`;
    reasonMasterData.forEach(x => {
        reasonDropdown += `
            <option value="${x.Id}">
                ${x.ReasonTitle}
            </option>
        `;
    });
    reasonDropdown += `</select>`;

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
                       min="0" step="0.01" value="0">
            </td>

            <!-- Reason -->
            <td>
                ${reasonDropdown}
            </td>

            <!-- Delete -->
            <td class="text-center">
                <button class="btn btn-sm btn-outline-danger btnDeleteRow">
                    <i class="fa fa-trash"></i>
                </button>
            </td>

        </tr>
    `);

    $("#RemoveItemsBody").append(newRow);

    updateSrNo();
}
function buildRowForEdit(item) {

    rowIndex++;

    let assetId = item.AssetMaster_Id;
    let qty = item.Qty ?? 0;
    let reasonId = item.AssetRemove_ReasonMaster_Id;

    //asset dropdown
    let assetDropdown = `<select class="form-select form-select-sm assetSelect">
        <option value="0">--Select Asset--</option>`;
    assetMasterData.forEach(x => {
        assetDropdown += `<option value="${x.Id}" data-stock="${x.ClosingQty}">
            ${x.AssetName} (Available Stock: ${x.ClosingQty})
        </option>`;
    });
    assetDropdown += `</select>`;

    //Remove dropdown
    let reasonDropdown = `<select class="form-select form-select-sm reasonSelect">
        <option value="0">--Select Remove--</option>`;
    reasonMasterData.forEach(x => {
        reasonDropdown += `<option value="${x.Id}">
            ${x.ReasonTitle}
        </option>`;
    });
    reasonDropdown += `</select>`;

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
                ${reasonDropdown}
            </td>

            <td class="text-center">
                <button class="btn btn-sm btn-outline-danger btnDeleteRow">
                    <i class="fa fa-trash"></i>
                </button>
            </td>

        </tr>

    `);

    row.find(".assetSelect").val(assetId);
    row.find(".reasonSelect").val(reasonId);

    return row;

}

// UPDATE SR NO AFTER DELETE
function updateSrNo() {
    $("#RemoveItemsBody tr").each(function (i) {
        $(this).find(".sr-no").text(i + 1);
    });
}

/// TABLE ITEMS GRID ----- END -------------------------------

//get data from API
function fetchData() {

    var payload = { Id: 0 };
    $.ajax({
        url: "/api/AssetRemove/GetAssetRemove",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("asset Remove Fetch:", response);
            var data = response.data ?? [];
            renderTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "AssetRemove Fetch Error");
        }
    });
}

//SHOW GRID TABLE
function renderTable(data) {

    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    // Add SR No.
    data.forEach((x, i) => x.srno = i + 1);

    console.log("data:", data);
    $('#tableMain table').DataTable({
        data: data,
        columns: [
            { data: "srno", title: "#" },
            {
                data: "BookingClientName",
                title: "Booking Client Name"
            },
            {
                data: "RemovedDate1",
                title: "Date",
                render: function (d) {
                    return d ? `<b>${d}</b>` : "-";
                }
            },
            { data: "TotalItems", title: "Total Items" },
            { data: "TotalQty", title: "Total Qty" },
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
        stateSave: false,
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },

    });

}

// Reset form fields
function resetFormFields(mode="add") {
    $("#RemoveId").val(0);
    $("#bookingId").val(0);
    $("#removeDate").val(new Date().toISOString().split("T")[0]);
    $("#remarks").val("");
    $("#RemoveItemsBody").empty();
    rowIndex = 0;
    addNewRow();
    $(".modal-body").scrollTop(0);
}

//validation
function validateRemoveForm() {

    if ($("#removeDate").val() === "") {
        Swal.fire("Validation", "Remove date is required", "warning");
        $("#removeDate").focus();
        return false;
    }

    if ($("#RemoveItemsBody tr").length === 0) {
        Swal.fire("Validation", "Add at least one asset item", "warning");
        return false;
    }

    let isValid = true;

    $("#RemoveItemsBody tr").each(function () {
        let row = $(this);

        let assetId = row.find(".assetSelect").val();
        let qty = parseFloat(row.find(".qtyInput").val());
        let reasonId = row.find(".reasonSelect").val();

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

        if (!reasonId || reasonId == "0") {
            Swal.fire("Validation", "Please select a Remove Reason", "warning");
            row.find(".reasonSelect").focus();
            isValid = false;
            return false;
        }

        let stock = row.find(".assetSelect option:selected").data("stock");
        if (qty > stock) {
            Swal.fire("Validation", "Qty exceeds available stock", "warning");
            isValid = false;
            return false;
        }
    });

    return isValid;

}

