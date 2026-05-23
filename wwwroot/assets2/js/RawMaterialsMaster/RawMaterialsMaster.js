$(document).ready(function () {
    GetUnitMasterData();
    GetCategoryMasterData();
    GetDayFrequencyMasterData();
    //loadDropdowns(); // Units, Category, Tax Types
    fetchData();
    fetchCategorySummaryData();

    // Add button click
    $(document).on('click', '#btn-add, #resetbtn', function (event) {
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
            $('#addEventModal').modal('hide');
        } else {
            resetFormFields(); // Reset form fields when modal is shown
            $("#addEventModalTitle").text("Add Raw Material");
            $("#addEventModal").modal("show");
        }
    });

    // Edit button click
    $(document).on("click", ".btn-edit", function () {

        const canEdit = (sessionStorage.getItem("CanEdit") || localStorage.getItem("CanEdit")) === "true";
        if (!canEdit) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Edit. Please contact admin."
            });
            return;
        }

        resetFormFields();   // clear everything first

        // ===========================
        // Get row data
        // ===========================
        const table = $("#tableMain table").DataTable();
        const rowData = table.row($(this).closest("tr")).data();

        if (!rowData) return;

        console.log("EDIT DATA:", rowData);

        // ===========================
        // BASIC FIELDS
        // ===========================
        $("#rmId").val(rowData.Id);
        $("#rmName").val(rowData.Name);
        $("#itemCategoryId").val(rowData.ItemCategoryId);

        $("#currentQty").val(rowData.CurrentQty);

        // ===========================
        // PURCHASE UNITS (MULTI-SELECT)
        // ===========================
        // Clear all first
        $(".purchase-unit-checkbox").prop("checked", false);
        // Check selected units
        rowData.JSON_PurchaseUnits.forEach(pu => {
            $(`.purchase-unit-checkbox[value="${pu.UnitId}"]`).prop("checked", true);
        });
        // Update dropdown + render rows
        updatePurchaseUnitSelection();

        // ===========================
        // CONSUMPTION UNIT
        // ===========================
        $("#consumptionUnitId").val(rowData.ConsumptionUnitId);
        // Re-render conversion rows (important after setting CU)
        renderConversionRows();

        // ===========================
        // SET CONVERSION VALUES
        // ===========================
        rowData.JSON_PurchaseUnits.forEach(pu => {
            let row = $(`.conversion-item[data-unitid="${pu.UnitId}"]`);
            let input = row.find(".conversion-input");
            input.val(pu.ConversionValue);
            // If same unit → readonly
            if (parseInt(pu.UnitId) === parseInt(rowData.ConsumptionUnitId)) {
                input.prop("readonly", true);
            }
        });        

        // ===========================
        // PRICES
        // ===========================
        $("#purchasePrice").val(rowData.PurchasePrice);
        $("#transferPrice").val(rowData.TransferPrice);
        $("#reconPrice").val(rowData.ReconciliationPrice);

        // ===========================
        // TAXES
        // ===========================
        if (rowData.TaxTypeId == 1) $("#gstRadio").prop("checked", true);
        else if (rowData.TaxTypeId == 2) $("#vatRadio").prop("checked", true);

        $("#taxPercent").val(rowData.TaxPercent);

        // Excise Duty only allowed when VAT is selected
        if (rowData.TaxTypeId == 2) {
            $("#exciseWrapper").removeClass("d-none");
            $("#exciseDutyAmount").val(rowData.ExciseDutyAmount);
        } else {
            $("#exciseWrapper").addClass("d-none");
            $("#exciseDutyAmount").val("");
        }

        // ===========================
        // STOCK LEVELS
        // ===========================
        $("#minStockUnitId").val(rowData.MinStockUnitId);
        $("#minStockLevel").val(rowData.MinStockLevel);
        $("#atParUnitId").val(rowData.AtParStockUnitId);
        $("#atParLevel").val(rowData.AtParStockLevel);

        // ===========================
        // CLOSING STOCK UPDATED ON
        // ===========================
        $(".dayfreq-checkbox").prop("checked", false);

        rowData.JSON_ClosingStockUpdatedOn.forEach(df => {
            $(`.dayfreq-checkbox[value="${df.FrequencyTitleId}"]`).prop("checked", true);
        });

        updateDayFrequencySelection();

        // ===========================
        // RESTOCK TABLE
        // ===========================
        $("#allowRestock").prop("checked", rowData.AllowReStockLevel);

        $("#restockBody").empty();

        if (rowData.AllowReStockLevel) {
            $("#restockTableWrapper").removeClass("d-none");

            if (rowData.JSON_RestockLevel.length > 0) {
                rowData.JSON_RestockLevel.forEach(r => {
                    addRestockRow(r.RestockQty, r.RestockUnitId);
                });
            } else {
                addRestockRow("", "");
            }
        } else {
            $("#restockTableWrapper").addClass("d-none");
        }

        $("#barcode").val(rowData.Barcode);
        $("#hsn").val(rowData.HSNCode);

        // ===========================
        // OTHER DETAILS
        // ===========================
        $("#exclusiveRest").val(rowData.ExclusiveToRestaurant ? "1" : "0");
        $("#isExpiry").val(rowData.IsExpiry ? "1" : "0");
        $("#allowDecimal").val(rowData.AllowDecimalQty ? "1" : "0");

        $("#description").val(rowData.Description);
        $("#lossPercent").val(rowData.NormalLossPercent);

        // ===========================
        // EXCISE REPORT TAB
        // ===========================
        $("#qtyGmMl").val(rowData.QuantityInGmMl);
        $("#gtin").val(rowData.GTIN);
        $("#brand").val(rowData.Brand);

        // ===========================
        // UI UPDATE
        // ===========================
        $("#addEventModalTitle").text("Edit Raw Material");
        showTab(0); // go to first tab
        $('#addEventModal').modal("show");
    });

    // SAVE - Handle form submission
    $("#btnSaveFinal").click(function (event) {
        event.preventDefault(); // Prevent the default form submission
        validateAddEventForm();

        // 1️⃣ Purchase Units JSON
        //let purchaseUnits = [];
        //$(".purchase-unit-checkbox:checked").each(function () {
        //    purchaseUnits.push({
        //        UnitId: parseInt($(this).val()),
        //        ConversionValue: 0
        //    });
        //});
        let purchaseUnits = [];
        $(".conversion-item").each(function () {
            let unitId = $(this).data("unitid");
            let conversionValue = parseFloat($(this).find(".conversion-input").val()) || 0;
            purchaseUnits.push({
                UnitId: parseInt(unitId),
                ConversionValue: conversionValue
            });
        });

        // 2️⃣ Closing Stock Updated On
        let closingStock = [];
        $(".dayfreq-checkbox:checked").each(function () {
            closingStock.push({ FrequencyTitleId: parseInt($(this).val()) });
        });

        // 3️⃣ Restock Levels
        let restock = [];
        $("#restockBody tr").each(function () {
            let qty = $(this).find(".restockQty").val();
            let unit = $(this).find(".restockUnit").val();

            if (qty && unit) {
                restock.push({ RestockQty: parseFloat(qty), RestockUnitId: parseInt(unit) });
            }
        });

        let payload = {
            Id: $("#rmId").val(),
            Name: $("#rmName").val(),

            JSON_PurchaseUnits: purchaseUnits,
            ConsumptionUnitId: $("#consumptionUnitId").val() == "0" ? 0 : parseInt($("#consumptionUnitId").val()),
            JSON_ClosingStockUpdatedOn: closingStock,

            ItemCategoryId: $("#itemCategoryId").val() == "0" ? 0 : parseInt($("#itemCategoryId").val()),

            CurrentQty: $("#currentQty").val() || 0,
            PurchasePrice: $("#purchasePrice").val() || 0,
            TransferPrice: $("#transferPrice").val() || 0,
            ReconciliationPrice: $("#reconPrice").val() || 0,

            TaxTypeId: $("input[name='taxType']:checked").val() || 1,
            TaxPercent: $("#taxPercent").val() || 0,
            ExciseDutyAmount: $("#exciseDutyAmount").val() || 0,

            MinStockUnitId: $("#minStockUnitId").val() == "0" ? 0 : parseInt($("#minStockUnitId").val()),
            MinStockLevel: parseInt($("#minStockLevel").val() || 0),
            AtParStockUnitId: $("#atParUnitId").val() == "0" ? 0 : parseInt($("#atParUnitId").val()),
            AtParStockLevel: parseInt($("#atParLevel").val() || 0),

            AllowReStockLevel: $("#allowRestock").is(":checked"),
            JSON_RestockLevel: restock,

            Barcode: $("#barcode").val(),
            HSNCode: $("#hsn").val(),
            GTIN: $("#gtin").val(),
            Brand: $("#brand").val(),

            ExclusiveToRestaurant: ($("#exclusiveRest").val() == "1"),
            IsExpiry: ($("#isExpiry").val() == "1"),
            AllowDecimalQty: ($("#allowDecimal").val() == "1"),

            Description: $("#description").val(),
            NormalLossPercent: $("#lossPercent").val() || 0,
            QuantityInGmMl: $("#qtyGmMl").val() || 0,

            IsActive: true,
            ActionBy: sessionStorage.getItem("userId") || 0
        };

        console.log("inputData:", payload)
        // Send AJAX request
        $.ajax({
            url: "/api/RawMaterialsMaster/SaveRawMaterial",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            success: function (response) {
                //var obj = JSON.parse(response);
                var obj = response;
                //console.log("submit obj:", obj);
                if (obj.status.toLowerCase() == "success") {
                    handleApiSuccess("Saved!");
                    fetchData();
                    resetFormFields();  // Reset the form fields
                    $('#addEventModal').modal('hide');  // Close the modal
                } else {
                    $('#message').removeAttr('hidden');
                    $('#message').text(obj.message).css('color', 'red').fadeIn(300).delay(5000).fadeOut(300);
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save RawMaterials Error")
            }
        });
    });

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
    $('#btnDeleteConfirm').click(function () {
        var id = $('#deleteRecordModal').data('id');

        let payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $('#btn-loader').show();
        $.ajax({
            url: "/api/RawMaterialsMaster/DeleteRawMaterial",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                //console.log("Delete Data:", response);
                // Parse the response if it's a string
                if (typeof response === 'string') {
                    response = JSON.parse(response);
                }

                //debugger
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
                //$('#btn-loader').hide();
                $('#deleteRecordModal').modal('hide');
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete RawMaterial Error")
            },
            complete: function () {
                $('#btn-loader').hide(); // Ensure loader is hidden in any case
            }
        });
        $('#deleteRecordModal').modal('hide');
    });

    //Tooltip
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    //VAT & Excise duty
    // Default state: hide excise duty
    $("#exciseWrapper").addClass("d-none");

    // When GST or VAT is changed
    $("input[name='taxType']").change(function () {

        let selected = $("input[name='taxType']:checked").val();

        if (selected == "2") {
            // VAT → show excise duty
            $("#exciseWrapper").removeClass("d-none");
        }
        else {
            // GST or unselected → hide excise duty
            $("#exciseWrapper").addClass("d-none");
            $("#exciseDutyAmount").val("0"); // clear value
        }
    });

    //set favourite
    $(document).on("change", ".toggle-fav", function () {
        console.log("called 1");
        let id = $(this).data("id");
        let isFav = $(this).is(":checked");

        let payload = {
            Id: id,
            IsFavourite: isFav,
            ActionBy: sessionStorage.getItem("userId") || 0
        };

        $.ajax({
            url: "/api/RawMaterialsMaster/SaveRawMaterials_IsFavourite",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {

                if (response.status === "Success") {
                    handleApiSuccess("Favourite updated");
                } else {
                    Swal.fire("Error", response.message, "error");
                }

            },
            error: function (xhr, error, status) {
                handleApiError(xhr, status, error, "Failed to Save Favorite Materials");
            }
        });

    });

    //set active
    $(document).on("change", ".toggle-active", function () {
        console.log("called 2");

        let id = $(this).data("id");
        let isActive = $(this).is(":checked");

        let payload = {
            Id: id,
            IsActive: isActive,
            ActionBy: sessionStorage.getItem("userId") || 0
        };

        $.ajax({
            url: "/api/RawMaterialsMaster/SaveRawMaterials_IsActive",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {

                if (response.status === "Success") {
                    handleApiSuccess("Active status updated");
                } else {
                    Swal.fire("Error", response.message, "error");
                }

            },
            error: function (xhr, error, status) {
                handleApiError(xhr, status, error, "Failed to Save Active Status");
            }
        });

    });

});
function renderTable(data) {
    console.log("data:", data);

    // Destroy existing DataTable if present
    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    // Check if data is null or empty
    if (!data || data.length === 0) {
        console.log("No Data Found");

        // Ensure table headers remain visible and add "No Data Found" row in the tbody
        $('#tableMain tbody').html(`
                        <tr>
                            <td colspan="6" class="text-center fw-bold text-danger">No Data Found !!</td>
                        </tr>
                    `);

        // Initialize an empty DataTable with headers intact
        $('#tableMain table').DataTable({
            data: [],
            columns: [
                { data: 'srno', title: 'Sr. No.' },
                { data: "Name", title: "Name" },
                { data: "ItemCategoryName", title: "Category" },
                { data: "CurrentQty", title: "CurrentQty" },
                {
                    data: "IsFavourite",
                    title: "Set As Favourite",
                    render: function (d, type, row) {
                        return `
                                <input type="checkbox" class="toggle-fav"
                                       data-id="${row.Id}" ${d ? "checked" : ""} />
                            `;
                    }
                },
                {
                    data: "IsActive",
                    title: "Active",
                    render: function (d, type, row) {
                        return `
                                <input type="checkbox" class="toggle-active"
                                       data-id="${row.Id}" ${d ? "checked" : ""} />
                            `;
                    }
                },
                {
                    data: null,
                    title: 'Actions',
                    render: function (data, type, row) {
                        return `
                                <button class="btn btn-outline-warning btn-sm me-1 mb-0 btn-edit">
                                    <i class="fa fa-edit"></i>
                                </button>
                                <button class="btn btn-outline-danger btn-sm me-1 mb-0 btn-delete">
                                    <i class="fa fa-trash"></i>
                                </button>
                            `;
                    }
                }
            ],
            columnDefs: [
                {
                    targets: 0
                }
            ],
            stateSave: false,
            pageLength: 10,           // Items per page
            ordering: true,          // Enable sorting
            searching: true,         // Enable searching
            lengthChange: false,     // Hide page length change
            pagingType: "full_numbers" // Show Prev, Next, and Page Numbers
        });

        return; // Exit function
    }

    let processData = data;
    // Adding Sr. No.
    processData.forEach((item, index) => {
        item.srno = index + 1;
    });

    console.log("processData:", processData);

    // Destroy the previous DataTable instance before reinitializing
    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy(); // Destroy the existing DataTable instance
    }

    $('#tableMain table').DataTable({
        data: processData,
        columns: [
            { data: 'srno', title: 'Sr. No.' },
            { data: "Name", title: "Name" },
            { data: "ItemCategoryName", title: "Category" },
            { data: "CurrentQty", title: "CurrentQty" },
            {
                data: "IsFavourite",
                title: "Set As Favourite",
                render: function (d, type, row) {
                    return `
                            <input type="checkbox" class="toggle-fav"
                                    data-id="${row.Id}" ${d ? "checked" : ""} />
                        `;
                }
            },
            {
                data: "IsActive",
                title: "Active",
                render: function (d, type, row) {
                    return `
                            <input type="checkbox" class="toggle-active"
                                    data-id="${row.Id}" ${d ? "checked" : ""} />
                        `;
                }
            },
            {
                data: null,
                title: 'Actions',
                render: function (data, type, row) {
                    return `
                        <div class="dropdown">
                            <button class="btn btn-sm btn-light btn-action-xss dropdown-toggle"
                                    type="button"
                                    data-bs-toggle="dropdown"
                                    aria-expanded="false">
                                <i class="fa fa-ellipsis-v"></i>
                            </button>
                            <ul class="dropdown-menu">
                                <li>
                                    <button class="dropdown-item btn-edit">
                                        <i class="fa fa-edit me-2 text-warning"></i> Edit
                                    </button>
                                </li>

                                <li>
                                    <button class="dropdown-item btn-delete">
                                        <i class="fa fa-trash me-2 text-danger"></i> Delete
                                    </button>
                                </li>
                            </ul>
                        </div>
                    `;
                }
            }
        ],
        columnDefs: [
            {
                targets: 0
            },
            {
                targets: 3
            },
            {
                targets: 4
            }

        ],
        stateSave: false,
        pageLength: 10,           // Items per page
        ordering: true,          // Enable sorting
        searching: true,         // Enable searching

        paging: false,
        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows
        scrollX: true,            // horizontal scroll if needed

        lengthChange: false,     // Hide page length change
        pagingType: "full_numbers" // Show Prev, Next, and Page Numbers
    });
}

// Initialize sorting
function initializeSorting() {
    const options = {
        valueNames: ["SrNo", "Name", "Category", "Set As Favourite", "Active", "Action"],
        page: 10,
        pagination: true
    };
    const list = new List('tableMain', options); // Initialize List.js or your sorting library
}

//get data from API
function fetchData(categoryid=0) {
    var payload = {
        Id: 0,
        ItemCategoryId: categoryid
    };

    $.ajax({
        url: "/api/RawMaterialsMaster/GetRawMaterialList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //data = response; // Store the fetched data
            console.log("response fetchData:", response);
            var data = response.data;
            renderTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Get DayFrequency Error")
        }
    });
}

// Reset form fields
function resetFormFields() {

    $("#rmId").val(0);
    $("#currentQty").val(0);

    $("#rmName,#purchaseEqualsQty,#purchasePrice,#transferPrice,#reconPrice,#taxPercent,#minStockLevel,#atParLevel,#barcode,#hsn,#gtin,#brand,#lossPercent,#qtyGmMl,#description").val("");

    $("#itemCategoryId,#consumptionUnitId,#minStockUnitId,#atParUnitId").val("0");

    // SET INPUT NUMBER = 0
    $("#purchaseEqualsQty,#purchasePrice,#transferPrice,#reconPrice,#taxPercent,#exciseDutyAmount,#minStockLevel,#atParLevel,#lossPercent,#qtyGmMl").val("0");

    $("#allowRestock,#exclusiveRest,#isExpiry,#allowDecimal").prop("checked", false);

    // Default GST
    $("#gstRadio").prop("checked", true).trigger("change");
    $("#vatRadio").prop("checked", false);

    $("#restockBody").empty();
    addRestockRow();
    resetDayFrequencySelection();

    $("#restockTableWrapper").addClass("d-none");

    // ==========================
    // PURCHASE UNIT RESET
    // ==========================
    $(".purchase-unit-checkbox").prop("checked", false);
    $("#selectAllPurchaseUnits").prop("checked", false);

    // Clear selected array
    selectedPurchaseUnits = [];

    // Reset dropdown text
    $("#purchaseUnitsDropdown").text("Select Units");

    // ==========================
    // CONVERSION RESET
    // ==========================
    $("#conversionRows").empty();                 // remove rows
    $("#conversionContainer").addClass("d-none"); // hide section

    // Optional: reset consumption dropdown UI text
    $("#consumptionUnitId").val("0");

    showTab(0);
}

// Validate form fields
function validateAddEventForm() {
    let name = $("#rmName").val().trim();

    if (name === "") {
        Swal.fire("Validation", "Raw Material Name is required", "warning");
        return;
    }
}

//Category Summary ----- START ------------------------------------------------------
function fetchCategorySummaryData() {
    $.ajax({
        url: "/api/RawMaterialsMaster/GetRawMaterial_CategorySummary",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ CategoryId: 0 }),   // 0 = fetch all
        success: function (response) {

            console.log("Category Summary:", response);

            if (response.status === "Success") {
                let list = response.data;
                renderCategorySummary(list);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Get Category Summary Error");
        }
    });
}
function renderCategorySummary(list) {
    let container = $("#categorySummaryContainer");
    container.empty();
    list.forEach((c, index) => {
        let box = `
                <div class="cat-box ${index === 0 ? "active" : ""}"
                     data-id="${c.CategoryId}">
                    <div class="cat-title">${c.CategoryName}</div>
                    <div class="cat-count">${c.RawMaterialsCount} Ingredients</div>
                </div>
            `;
        container.append(box);
    });
    attachCategorySummaryEvents();
}

function attachCategorySummaryEvents() {
    $(".cat-box").off("click").on("click", function () {
        $(".cat-box").removeClass("active");
        $(this).addClass("active");
        let catId = $(this).data("id");
        fetchData(catId); // <--- your existing fetch list function
    });
}

$("#catScrollNext").click(function () {
    document.getElementById("categorySummaryContainer")
        .scrollBy({ left: 250, behavior: "smooth" });
});

$("#catScrollPrev").click(function () {
    document.getElementById("categorySummaryContainer")
        .scrollBy({ left: -250, behavior: "smooth" });
});

//Category Summary ----- END ------------------------------------------------------

//DROPDOWN FILLUPS
var unitsData;
function GetUnitMasterData() {
    //$('#btn-loader').show();
    $('.search').val('');

    $.ajax({
        url: "/api/UnitMaster/GetUnitMasterList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            console.log("response fetchData:", response);
            unitsData = response.data;
            loadUnitDropdown("#consumptionUnitId");
            loadPurchaseUnitsDropdown();
            loadUnitDropdown("#minStockUnitId, #atParUnitId");
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch UnitMaster Error!");
        }
    });
}

function loadUnitDropdown(selector) {
    let s = $(selector);
    s.empty().append(`<option value="0">Select Unit</option>`);
    unitsData.forEach(u => {
        s.append(`<option value="${u.Id}">${u.UnitName}</option>`);
    });
}

$("#consumptionUnitId").change(function () {
    loadFilteredUnitDropdowns();
    renderConversionRows();
});

function loadFilteredUnitDropdowns() {
    let selectedCU = $("#consumptionUnitId").val();   // selected Consumption Unit ID

    // If no consumption unit selected → reset dropdowns normally
    if (!selectedCU || selectedCU === "0") {
        loadUnitDropdown("#minStockUnitId");
        loadUnitDropdown("#atParUnitId");
        return;
    }

    // Filtered dropdown
    let html = `<option value="${selectedCU}">` +
        unitsData.find(u => u.Id == selectedCU).UnitName +
        `</option>`;

    $("#minStockUnitId").html(html);
    $("#atParUnitId").html(html);
}

//DYNAMIC PURCHASE UNITS CHECKBOXES -- START ------------------------
let selectedPurchaseUnits = [];
let allPurchaseUnitCheckboxes = [];

// Load checkbox list dynamically
function loadPurchaseUnitsDropdown() {

    let html = "";

    unitsData.forEach(u => {
        html += `
                <li class="mb-1 purchase-unit-item">
                    <label class="w-100">
                        <input type="checkbox"
                               class="form-check-input me-2 purchase-unit-checkbox"
                               value="${u.Id}"
                               data-name="${u.UnitName}">
                        ${u.UnitName}
                    </label>
                </li>`;
    });

    $("#purchaseUnitsList").html(html);

    // store reference to checkboxes
    allPurchaseUnitCheckboxes = $(".purchase-unit-checkbox");

    // Attach events
    $(".purchase-unit-checkbox").on("change", updatePurchaseUnitSelection);

    $("#selectAllPurchaseUnits").on("change", function () {
        let state = $(this).prop("checked");

        allPurchaseUnitCheckboxes.prop("checked", state);
        updatePurchaseUnitSelection();
    });

    // Search functionality
    $("#purchaseUnitSearch").on("keyup", function () {
        let keyword = $(this).val().toLowerCase();

        $(".purchase-unit-item").each(function () {
            let name = $(this).text().toLowerCase();
            $(this).toggle(name.includes(keyword));
        });
    });
}

// Update button label + selected array
function updatePurchaseUnitSelection() {
    //here2
    selectedPurchaseUnits = [];
    let text = "";

    $(".purchase-unit-checkbox").each(function () {
        if ($(this).prop("checked")) {
            selectedPurchaseUnits.push({
                UnitId: $(this).val(),
                UnitName: $(this).data("name")
            });

            text += $(this).data("name") + ", ";
        }
    });

    $("#purchaseUnitsDropdown").text(
        selectedPurchaseUnits.length > 0
            ? text.slice(0, -2)
            : "Select Units"
    );

    // Update Select All checkbox state
    let total = allPurchaseUnitCheckboxes.length;
    let checked = $(".purchase-unit-checkbox:checked").length;

    $("#selectAllPurchaseUnits").prop("checked", total > 0 && total === checked);
    renderConversionRows();
}

function renderConversionRows() {

    let consumptionId = $("#consumptionUnitId").val();
    let consumptionName = $("#consumptionUnitId option:selected").text();

    if (!consumptionId || selectedPurchaseUnits.length === 0) {
        $("#conversionContainer").addClass("d-none");
        return;
    }

    let html = "";

    selectedPurchaseUnits.forEach(p => {

        let isSame = p.UnitId == consumptionId;

        html += `
            <div class="input-group mb-2 conversion-item"
                 data-unitid="${p.UnitId}">
                 
                <span class="input-group-text">1 ${p.UnitName}</span>
                <span class="input-group-text">=</span>

                <input type="number"
                       step="0.01"
                       class="form-control conversion-input"
                       value="${isSame ? 1 : ""}"
                       ${isSame ? "readonly" : ""}>

                <span class="input-group-text">${consumptionName}</span>
            </div>
        `;
    });

    $("#conversionRows").html(html);
    $("#conversionContainer").removeClass("d-none");
}
//DYNAMIC PURCHASE UNITS CHECKBOXES -- END ------------------------

// ITEM CATEGORY DROPDOWN
function GetCategoryMasterData() {
    $.ajax({
        url: "/api/ItemCategoryMaster/GetItemCategoryList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            $("#itemCategoryId").empty().append(`<option value="0">Select Category</option>`);
            response.data.forEach(c => {
                $("#itemCategoryId").append(`
                        <option value="${c.Id}">${c.CategoryName}</option>
                    `);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Unable to Fetch Category Data!");
        }
    });
}

//Fetch Day Frequency ---- START --------------------------------------------------------
var dayFrequencyData = [];
function GetDayFrequencyMasterData() {
    $.ajax({
        url: "/api/DayFrequencyMaster/GetDayFrequencyList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            console.log("response dayFrequency:", response);
            dayFrequencyData = response.data;
            loadDayFrequencyDropdown();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Get DayFrequency Error")
        }
    });
}
// ===================== DAY FREQUENCY MULTI SELECT =====================
let selectedDayFrequencies = [];
let allDayFreqCheckboxes = [];

// Load Day Frequency Dropdown
function loadDayFrequencyDropdown() {

    let html = "";

    dayFrequencyData.forEach(df => {
        html += `
                <li class="mb-1 dayfreq-item">
                    <label class="w-100">
                        <input type="checkbox"
                               class="form-check-input me-2 dayfreq-checkbox"
                               value="${df.Id}"
                               data-name="${df.FrequencyTitle}">
                        ${df.FrequencyTitle}
                    </label>
                </li>`;
    });

    $("#dayFreqList").html(html);

    // store reference to checkboxes
    allDayFreqCheckboxes = $(".dayfreq-checkbox");

    // Attach change events
    $(".dayfreq-checkbox").on("change", updateDayFrequencySelection);

    // Select All
    $("#dayFreqSelectAll").on("change", function () {
        let state = $(this).prop("checked");
        allDayFreqCheckboxes.prop("checked", state);
        updateDayFrequencySelection();
    });

    // Search Filter
    $("#dayFreqSearch").on("keyup", function () {
        let keyword = $(this).val().toLowerCase();

        $(".dayfreq-item").each(function () {
            let name = $(this).text().toLowerCase();
            $(this).toggle(name.includes(keyword));
        });
    });

    // Make sure first option stays selected
    updateDayFrequencySelection();
}

// Update Selection
function updateDayFrequencySelection() {

    selectedDayFrequencies = [];
    let text = "";

    $(".dayfreq-checkbox").each(function () {

        if ($(this).prop("checked")) {
            selectedDayFrequencies.push({
                FrequencyTitleId: $(this).val(),
                FrequencyTitle: $(this).data("name")
            });

            text += $(this).data("name") + ", ";
        }
    });

    $("#dayFreqDropdown").text(
        selectedDayFrequencies.length > 0
            ? text.slice(0, -2)
            : "Select Frequency"
    );

    // Update Select All checkbox
    let total = allDayFreqCheckboxes.length;
    let checked = $(".dayfreq-checkbox:checked").length;
    $("#dayFreqSelectAll").prop("checked", total > 0 && total === checked);
    //handleDayFreqSelectAll();
}

function resetDayFrequencySelection() {

    if (dayFrequencyData.length === 0) {
        $("#dayFreqDropdown").text("Select Frequency");
        return;
    }

    // Uncheck all first
    $(".dayfreq-checkbox").prop("checked", false);

    // Check FIRST item always
    let firstCheckbox = $(".dayfreq-checkbox").first();
    firstCheckbox.prop("checked", true);

    // Update label
    $("#dayFreqDropdown").text(firstCheckbox.data("name"));

    // Update Select All checkbox
    let total = allDayFreqCheckboxes.length;
    let checked = $(".dayfreq-checkbox:checked").length;
    $("#dayFreqSelectAll").prop("checked", total > 0 && total === checked);

    // Update selected list
    updateDayFrequencySelection();
}

//Fetch Day Frequency ---- END --------------------------------------------------------

//SHOW CONVERSION ROW WHEN BOTH UNITS SELECTED
// SHOW / HIDE conversion row based on selections
$("#consumptionUnitId").change(() => handleConversionVisibility());
$(document).on("change", ".purchase-unit-checkbox", handleConversionVisibility);

function handleConversionVisibility() {

    let selectedPurchaseUnits = $(".purchase-unit-checkbox:checked");
    let consumptionUnitId = $("#consumptionUnitId").val();
    let consumptionUnitName = $("#consumptionUnitId option:selected").text();

    // No purchase unit selected OR no consumption unit selected
    if (selectedPurchaseUnits.length === 0 || !consumptionUnitId) {
        $("#conversionRow").addClass("d-none");
        return;
    }

    // ✔ Get LAST selected purchase unit
    let lastPU = selectedPurchaseUnits.last();
    let puName = lastPU.data("name");
    let puId = lastPU.val();

    // Set labels
    $("#purchaseUnitLbl").text(puName);
    $("#consumptionUnitLbl").text(consumptionUnitName);

    // ✔ If same unit selected for both → value = 1
    if (parseInt(puId) === parseInt(consumptionUnitId)) {
        $("#purchaseEqualsQty").val(1);
        $("#purchaseEqualsQty").prop("disabled", true);
    } else {
        $("#purchaseEqualsQty").prop("disabled", false);
        // Clear only if previous value was auto-set
        if ($("#purchaseEqualsQty").val() == 1) {
            $("#purchaseEqualsQty").val("");
        }
    }

    $("#conversionRow").removeClass("d-none");
}

//RESTOCK TABLE – ADD ROW
function addRestockRow(qty = "0", unitId = "") {
    let row = `
            <tr>
                <td><input type="number" step="0.01" class="form-control restockQty" value="${qty}"></td>
                <td>
                    <select class="form-select restockUnit">
                        ${$("#consumptionUnitId").html()}
                    </select>
                </td>
                <td><button class="btn btn-outline-danger btn-sm removeRestockRow">X</button></td>
            </tr>`;
    $("#restockBody").append(row);

    // Set selected unit if edit mode
    if (unitId) {
        $("#restockBody tr:last .restockUnit").val(unitId);
    }
}

$("#addRestockRow").click(() => addRestockRow());

// Remove row
$(document).on("click", ".removeRestockRow", function () {
    $(this).closest("tr").remove();
});

//RESTOCK TABLE SHOW/HIDE
$("#allowRestock").change(function () {
    if ($(this).is(":checked")) {
        $("#restockTableWrapper").removeClass("d-none");
    } else {
        $("#restockTableWrapper").addClass("d-none");
    }
});

//<!--ADD MODAL TAB -- START -------------------------------------------------->
let currentTab = 0;
const totalTabs = 7;

function showTab(index) {
    currentTab = index;

    let tabs = document.querySelectorAll("#rmTabs .nav-link");
    let tabContents = document.querySelectorAll(".tab-pane");

    tabs.forEach(t => t.classList.remove("active"));
    tabContents.forEach(tc => tc.classList.remove("show", "active"));

    tabs[index].classList.add("active");
    tabContents[index].classList.add("show", "active");

    // Manually trigger the Bootstrap tab shown event
    const event = new Event('shown.bs.tab');
    tabs[index].dispatchEvent(event);

    updateTabButtons();
}

function updateTabButtons2() {
    $("#btnPrev").toggle(currentTab !== 0);
    $("#btnNext").toggle(currentTab !== totalTabs - 1);
    $("#btnSaveFinal").toggle(currentTab === totalTabs - 1);
}

function updateTabButtons() {

    if (currentTab === 0) {
        $("#btnPrev").hide();
    } else {
        $("#btnPrev").show();
    }

    if (currentTab === totalTabs - 1) {
        $("#btnNext").hide();
        $("#btnSaveFinal").show();   // SHOW SAVE
    }
    else {
        $("#btnNext").show();
        $("#btnSaveFinal").hide();   // HIDE SAVE
    }
}

// BUTTONS
$("#btnPrev").click(() => showTab(currentTab - 1));
$("#btnNext").click(() => showTab(currentTab + 1));

// ⭐ IMPORTANT: When user clicks a tab manually
$('#rmTabs .nav-link').on('shown.bs.tab', function (e) {
    let tabs = $("#rmTabs .nav-link");
    currentTab = tabs.index(this);
    updateTabButtons();
});

// Initialize buttons on page load
$(document).ready(function () {
    updateTabButtons();
});

//<!--ADD MODAL TAB -- END -------------------------------------------------->
