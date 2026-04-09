let rowIndex = 0;
$(document).ready(function () {
    loadBookingDropdown();
    loadAssetsMasterDropdown();
    loadRepairStatusMasterDropdown();
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
            $('#assetRepairModal').modal('hide');
        } else {
            resetFormFields(); // Reset form fields when modal is shown
            $("#RepairModalTitle").text("Add Asset-Repair Details");
            $("#assetRepairModal").modal("show");
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

        resetFormFields("edit");

        let table = $('#tableMain table').DataTable();
        let rowData = table.row($(this).closest("tr")).data();

        if (!rowData) return;

        console.log("rowData:", rowData);

        // HEADER
        $("#RepairId").val(rowData.Id);

        $("#bookingId").val(rowData.BookingId ?? 0);

        $("#repairVendor").val(rowData.RepairVendor ?? "");

        $("#repairTotalCost").val(rowData.RepairTotalCost ?? 0);

        $("#remarks").val(rowData.Remarks ?? "");

        // ITEMS
        $("#RepairItemsBody").empty();
        rowIndex = 0;
        if (Array.isArray(rowData.Items) && rowData.Items.length > 0) {
            rowData.Items.forEach(item => {
                let newRow = buildRowForEdit(item);
                $("#RepairItemsBody").append(newRow);
            });
        }
        else {
            addNewRow();
        }

        updateSrNo();

        // OPEN MODAL
        $("#RepairModalTitle").text("Edit Asset Repair");

        $("#assetRepairModal").modal("show");

    });

    // SAVE - Handle form submission
    $("#btnSaveRepair").click(function (event) {
        event.preventDefault();
        if (!validateRepairForm()) return;
        let payload = {
            Id: $("#RepairId").val(),
            BookingId: $("#bookingId").val() || null,
            RepairVendor: $("#repairVendor").val(),
            RepairTotalCost: parseFloat($("#repairTotalCost").val()) || 0,
            Remarks: $("#remarks").val(),
            Items: collectRepairItems(),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        console.log("payload:", payload);

        $.ajax({
            url: "/api/AssetRepair/SaveAssetRepair",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            success: function (response) {
                if (response.status.toLowerCase() === "success") {
                    handleApiSuccess("Saved!");
                    fetchData();
                    resetFormFields();
                    $("#assetRepairModal").modal("hide");
                }
                else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Asset Repair Save Error");
            }
        });
    });
    function collectRepairItems() {
        let items = [];
        $("#RepairItemsBody tr").each(function () {
            let row = $(this);
            items.push({
                AssetMaster_Id: row.find(".assetSelect").val(),
                Qty: parseFloat(row.find(".qtyInput").val()) || 0,
                CostPerItem: parseFloat(row.find(".costInput").val()) || 0,
                RepairCost: parseFloat(row.find(".repairCostInput").val()) || 0,
                AssetRepair_StatusMaster_Id: row.find(".repairStatusSelect").val(),
                SentDate: row.find(".sentDateInput").val(),
                ReceivedDate: row.find(".receivedDateInput").val()
            });
        });
        console.log("save items:", items);
        return items;
    }

    // Delete Event handler for repair button click
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
            url: "/api/AssetRepair/DeleteAssetRepair",
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
                handleApiError(xhr, status, error, "Delete AssetRepair failed");
            }
        });
        $('#deleteRecordModal').modal('hide');
    });

    // DELETE ROW
    $(document).on("click", ".btnDeleteRow", function () {
        $(this).closest("tr").repair();
        updateSrNo();
    });

    // ADD NEW ROW BUTTON
    $("#btnAddRepairItem").on("click", function () {
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

let repairStatusMaster = [];
function loadRepairStatusMasterDropdown() {
    $.ajax({
        url: "/api/AssetRepair_StatusMaster/GetAssetRepairStatus",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            repairStatusMaster = res.data || [];
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Status Master Failed");
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
    $("#RepairItemsBody").empty();
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

    // STATUS DROPDOWN
    let statusDropdown = `<select class="form-select form-select-sm repairStatusSelect">`;
    statusDropdown += `<option value="0">-- Status --</option>`;
    repairStatusMaster.forEach(x => {
        statusDropdown += `<option value="${x.Id}">${x.StatusTitle}</option>`;
    });
    statusDropdown += `</select>`;

    let today = new Date().toISOString().split("T")[0];

    let newRow = $(`
        <tr data-index="${rowIndex}">

            <!-- SR NO -->
            <td class="text-center align-middle sr-no">${rowIndex}</td>

            <!-- Asset -->
            <td>${assetDropdown}</td>

            <!-- Qty -->
            <td>
                <input type="number" class="form-control form-control-sm qtyInput" min="0" step="1" value="0">
            </td>

            <td>
                <input type="number" class="form-control form-control-sm costInput" value="0">
            </td>

            <td>
                <input type="number" class="form-control form-control-sm repairCostInput" value="0" readonly>
            </td>

            <!-- Status -->
            <td>${statusDropdown}</td>

            <td>
                <div class="input-group input-group-sm">
                    <input type="date" class="form-control sentDateInput" value="${today}">
                    <button class="btn btn-outline-secondary btnClearDate" type="button">
                        <i class="fa fa-times"></i>
                    </button>
                </div>
            </td>

            <td>
                <div class="input-group input-group-sm">
                    <input type="date" class="form-control receivedDateInput">
                    <button class="btn btn-outline-secondary btnClearDate" type="button">
                        <i class="fa fa-times"></i>
                    </button>
                </div>
            </td>

            <!-- Delete -->
            <td class="text-center">
                <button class="btn btn-xs btn-outline-danger btnDeleteRow">
                    <i class="fa fa-trash"></i>
                </button>
            </td>

        </tr>
    `);

    $("#RepairItemsBody").append(newRow);

    updateSrNo();
}
function buildRowForEdit(item) {

    rowIndex++;
    console.log("edit item:", item);

    let assetId = item.AssetMaster_Id;
    let qty = item.Qty ?? 0;
    let cost = item.CostPerItem ?? 0;
    let repairCost = item.RepairCost ?? 0;
    let statusId = item.AssetRepair_StatusMaster_Id ?? 0;
    let sentDate = item.SentDate ?? "";
    let receivedDate = item.ReceivedDate ?? "";

    // ASSET DROPDOWN
    let assetDropdown = `<select class="form-select form-select-sm assetSelect">
        <option value="0">--Select Asset--</option>`;

    assetMasterData.forEach(x => {
        assetDropdown += `
            <option value="${x.Id}" data-stock="${x.ClosingQty}">
                ${x.AssetName} (Available Stock: ${x.ClosingQty})
            </option>`;
    });

    assetDropdown += `</select>`;

    // STATUS DROPDOWN
    let statusDropdown = `<select class="form-select form-select-sm repairStatusSelect">
        <option value="0">-- Status --</option>`;

    repairStatusMaster.forEach(x => {
        statusDropdown += `<option value="${x.Id}">
            ${x.StatusTitle}
        </option>`;
    });

    statusDropdown += `</select>`;

    let row = $(`
        <tr data-index="${rowIndex}">

            <td class="text-center sr-no">${rowIndex}</td>

            <td>${assetDropdown}</td>

            <td>
                <input type="number"
                       class="form-control form-control-sm qtyInput"
                       value="${qty}">
            </td>

            <td>
                <input type="number"
                       class="form-control form-control-sm costInput"
                       value="${cost}">
            </td>

            <td>
                <input type="number"
                       class="form-control form-control-sm repairCostInput"
                       value="${repairCost}"
                       readonly>
            </td>

            <td>${statusDropdown}</td>

            <td>
                <div class="input-group input-group-sm">
                    <input type="date"
                           class="form-control sentDateInput"
                           value="${sentDate}">
                    <button class="btn btn-outline-secondary btnClearDate" type="button">
                        <i class="fa fa-times"></i>
                    </button>
                </div>
            </td>

            <td>
                <div class="input-group input-group-sm">
                    <input type="date"
                       class="form-control form-control-sm receivedDateInput"
                       value="${receivedDate}">
                    <button class="btn btn-outline-secondary btnClearDate" type="button">
                        <i class="fa fa-times"></i>
                    </button>
                </div>
            </td>

            <td class="text-center">
                <button class="btn btn-xs btn-outline-danger btnDeleteRow">
                    <i class="fa fa-trash"></i>
                </button>
            </td>

        </tr>
    `);

    row.find(".assetSelect").val(assetId);
    row.find(".repairStatusSelect").val(statusId);

    return row;
}
// UPDATE SR NO AFTER DELETE
function updateSrNo() {
    $("#RepairItemsBody tr").each(function (i) {
        $(this).find(".sr-no").text(i + 1);
    });
}

/// TABLE ITEMS GRID ----- END -------------------------------

//get data from API
function fetchData() {

    var payload = { Id: 0 };
    $.ajax({
        url: "/api/AssetRepair/GetAssetRepair",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("asset Repair Fetch:", response);
            var data = response.data ?? [];
            renderTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "AssetRepair Fetch Error");
        }
    });
}

//SHOW GRID TABLE
function renderTable(data) {

    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    // Add SR No + calculate totals
    data.forEach((x, i) => {

        x.srno = i + 1;

        if (Array.isArray(x.Items)) {

            x.TotalItems = x.Items.length;

            x.TotalQty = x.Items.reduce((sum, item) => {
                return sum + (parseFloat(item.Qty) || 0);
            }, 0);

        } else {

            x.TotalItems = 0;
            x.TotalQty = 0;
        }

    });

    $('#tableMain table').DataTable({
        data: data,
        columns: [
            { data: "srno", title: "#" },
            {
                data: "BookingClientName",
                title: "Booking Client",
                render: function (d) {
                    return d ? `<b>${d}</b>` : "-";
                }
            },
            {
                data: "RepairVendor",
                title: "Vendor",
                render: function (d) {
                    return d ? d : "-";
                }
            },
            {
                data: "TotalItems",
                title: "Total Items",
                className: "text-center"
            },

            {
                data: "TotalQty",
                title: "Total Qty",
                className: "text-center"
            },

            {
                data: "RepairTotalCost",
                title: "Repair Cost",
                render: function (d) {
                    return `₹ ${parseFloat(d || 0).toFixed(2)}`;
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
                                    data-bs-toggle="dropdown">
                                <i class="fa fa-ellipsis-v"></i>
                            </button>

                            <ul class="dropdown-menu dropdown-menu-end">

                                <li>
                                    <a class="dropdown-item btn-edit"
                                       href="javascript:void(0)"
                                       data-id="${row.Id}">
                                        <i class="fa fa-edit text-warning me-2"></i> Edit
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item btn-delete"
                                       href="javascript:void(0)"
                                       data-id="${row.Id}">
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
function resetFormFields(mode = "add") {

    $("#RepairId").val(0);

    $("#bookingId").val(0);

    $("#repairVendor").val("");

    $("#repairTotalCost").val("0");

    $("#remarks").val("");

    $("#RepairItemsBody").empty();

    rowIndex = 0;

    addNewRow();

    $(".modal-body").scrollTop(0);
}
//validation
function validateRepairForm() {

    if ($("#RepairItemsBody tr").length === 0) {

        Swal.fire("Validation", "Add at least one repair item", "warning");

        return false;

    }

    let isValid = true;

    $("#RepairItemsBody tr").each(function () {

        let row = $(this);

        let assetId = row.find(".assetSelect").val();
        let qty = parseFloat(row.find(".qtyInput").val());
        let cost = parseFloat(row.find(".costInput").val());
        let statusId = row.find(".repairStatusSelect").val();

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

        if (cost === undefined || cost < 0) {

            Swal.fire("Validation", "Cost per item must be valid", "warning");

            row.find(".costInput").focus();

            isValid = false;

            return false;
        }

        if (!statusId || statusId == "0") {

            Swal.fire("Validation", "Please select repair status", "warning");

            row.find(".repairStatusSelect").focus();

            isValid = false;

            return false;
        }

        // stock validation
        let stock = row.find(".assetSelect option:selected").data("stock") || 0;

        if (qty > stock) {

            Swal.fire("Validation", "Qty exceeds available stock", "warning");

            row.find(".qtyInput").focus();

            isValid = false;

            return false;
        }

    });

    return isValid;
}

$(document).on("input", ".qtyInput, .costInput", function () {

    let row = $(this).closest("tr");

    let qty = parseFloat(row.find(".qtyInput").val()) || 0;
    let cost = parseFloat(row.find(".costInput").val()) || 0;

    let total = qty * cost;

    row.find(".repairCostInput").val(total.toFixed(2));

    calculateRepairTotal();

});

function calculateRepairTotal() {

    let total = 0;

    $("#RepairItemsBody tr").each(function () {

        total += parseFloat($(this).find(".repairCostInput").val()) || 0;

    });

    $("#repairTotalCost").val(total.toFixed(2));
}

//btnClearDate
$(document).on("click", ".btnClearDate", function () {
    $(this).closest(".input-group").find("input[type='date']").val("");
});
