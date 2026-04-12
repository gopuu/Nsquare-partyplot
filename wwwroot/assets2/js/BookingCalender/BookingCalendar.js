
// Toggle editors visibility
var invoiceNo = 0;
var proformaInvoiceNo = 0;
var gstInvoiceNo = 0;
let invoiceType = null;

//< !--Booking Note - text Editor-- - START---->
// ✅ Register formats FIRST (only once)
const Parchment = Quill.import('parchment');

const SizeStyle = new Parchment.Attributor.Style('size', 'font-size', {
    scope: Parchment.Scope.INLINE,
    whitelist: ['10px', '12px', '14px', '16px', '18px', '20px', '24px', '30px']
});
Quill.register(SizeStyle, true);

const LineHeightStyle = new Parchment.Attributor.Style('lineheight', 'line-height', {
    scope: Parchment.Scope.BLOCK,
    whitelist: ['1.0', '1.2', '1.4', '1.5', '1.6', '2.0', '3.0']
});
Quill.register(LineHeightStyle, true);

// Toolbar
const toolbarOptions = [
    ['bold', 'italic', 'underline'],
    [{ size: ['10px', '12px', '14px', '16px', '18px', '20px', '24px', '30px'] }],
    [{ list: 'ordered' }, { list: 'bullet' }]
];

// Store editors
const editors = {};
let notesData = null; // store once

// Toggle logic

$(document).on("click", ".toggle-editor", function () {
    const targetId = $(this).data("target");
    const box = $("#box_" + targetId);

    if (box.hasClass("d-none")) {
        box.removeClass("d-none");
        // Init if not exists
        if (!editors[targetId]) {
            editors[targetId] = new Quill("#" + targetId, {
                theme: "snow",
                modules: { toolbar: toolbarOptions }
            });
        }

        // ✅ ALWAYS SET DATA (important fix)
        let value = "";

        if (notesData) {
            switch (targetId) {
                case "OtherCharges_customerEditor":
                    value = notesData.OtherCharges_BookingNoteForCustomer;
                    break;
                case "OtherCharges_internalEditor":
                    value = notesData.OtherCharges_InternalOfficeNote;
                    break;
                case "Notes_FoodBeverage_customerEditor":
                    value = notesData.Notes_FoodBeverage;
                    break;
                case "Notes_EventTiming_customerEditor":
                    value = notesData.Notes_EventTiming;
                    break;
                case "Notes_DecorationSetup_customerEditor":
                    value = notesData.Notes_DecorationSetup;
                    break;
                case "Notes_TechnicalService_customerEditor":
                    value = notesData.Notes_TechnicalService;
                    break;
                case "Notes_GuestCustom_customerEditor":
                    value = notesData.Notes_GuestCustom;
                    break;
                case "Notes_FinancialInternal_internalEditor":
                    value = notesData.Notes_FinancialInternal;
                    break;
            }
        }

        editors[targetId].root.innerHTML = value || "";
    }
    else {
        box.addClass("d-none");
        // ❌ Do NOT clear here (important)
        // otherwise edit mode loses data
    }
});

//< !--Booking Note - text Editor-- - END---->

// Optional: Auto-correct minute/hour inputs

// <!-- Time format  --START- Include Flatpickr CSS & JS -->

function fixTimeInput(idHour, idMinute) {
    const hour = document.getElementById(idHour);
    const minute = document.getElementById(idMinute);

    hour.addEventListener('input', () => {
        if (hour.value > 12) hour.value = 12;
        if (hour.value < 1) hour.value = 1;
    });

    minute.addEventListener('input', () => {
        if (minute.value > 59) minute.value = 59;
        if (minute.value < 0) minute.value = 0;
    });
}

fixTimeInput('startHour', 'startMinute');
fixTimeInput('endHour', 'endMinute');

//<!-- Time format  --END- Include Flatpickr CSS & JS -->

// <!--Reschedule-->

function to24Hour(hour, minute, ampm) {
    hour = parseInt(hour, 10);
    minute = parseInt(minute || 0, 10);

    if (ampm === "PM" && hour !== 12) hour += 12;
    if (ampm === "AM" && hour === 12) hour = 0;

    return `${hour.toString().padStart(2, '0')}:${minute.toString().padStart(2, '0')}`;
}
$("#btnSaveReschedule").on("click", function () {

    const bookingId = Number($("#reschedule_BookingId").val());

    if (!bookingId) {
        Swal.fire("Invalid", "Booking Id missing.", "warning");
        return;
    }

    // 🔹 Build Start & End Time (24-hour)
    const startTime = to24Hour(
        $("#reschedule_startHour").val(),
        $("#reschedule_startMinute").val(),
        $("#reschedule_startAMPM").val()
    );

    const endTime = to24Hour(
        $("#reschedule_endHour").val(),
        $("#reschedule_endMinute").val(),
        $("#reschedule_endAMPM").val()
    );

    // 🔹 Build payload (match API model)
    const payload = {
        BookingId: bookingId,
        BanquetId: Number($("#reschedule_BanquetId").val()) || 0,
        SlotId: 0,                                                      //Number($("#reschedule_SlotId").val()) || null,
        FromDate: $("#reschedule_FromDate").val() || null,
        TillDate: $("#reschedule_TillDate").val() || null,
        StartTime: startTime || null,
        EndTime: endTime || null,
        ShiftCode: null,                                                // optional (can be derived from slot if needed)
        ActionBy: Number(sessionStorage.getItem("userId")) || 0
    };

    //console.log("Reschedule Payload:", payload);

    // 🔹 Call API
    $.ajax({
        url: "/api/BookingCalendar/SaveBookingCalendar_Reschedule",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                handleApiSuccess("Booking Rescheduled");
                $("#bookingRescheduleModal").modal("hide");
                loadBookingCalendar();
            } else {
                Swal.fire("Error", res.message || "Failed to reschedule booking.", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Reschedule Save failed");
        }
    });
});


$(document).ready(function () {
    $("#btnwritrnote").click(function () {
        let editor = $(".Payment_editorsContainer");
        if (editor.is(":visible")) {
            editor.hide();
            $(this).val("Write Note");
        } else {
            editor.show();
            $(this).val("Write Note");
        }
    });

    $("#otherCharges_Reset").on("click", function () {
        // Reset all number inputs inside Other Charges section
        $("#otherCharges").find("input[type='number']").val(0);
    });

});

// NEW
var banquetHallsList = [
    // { id: 1, name: "LAXMAN" },
    // { id: 2, name: "RAMA" },
    // { id: 3, name: "SITA" }
];
//banquet hall details - dropdown -- START ----------------------
function fetchHallDetails(bookingData, banquetId = 0) {
    //showLoader();
    $.ajax({
        url: '/api/Company_HallDetails/GetCompanyHallDetailsList_Dropdown',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: banquetId }), // Get all halls
        success: function (response) {
            //console.log("Get Hall res:", response);
            if (response.status === "Success" && response.data && response.data.length > 0) {
                banquetHallsList = response.data;
                let options = `<option value="0">-- Select Hall --</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.HallName} (${item.CapacityRangeText})</option>`;
                });
                $("#banquetDropdown, #reschedule_BanquetId").html(options);

                // ✅ If banquetId is passed → select & STOP
                if (banquetId > 0) {
                    $("#banquetDropdown, #reschedule_BanquetId").val(String(banquetId)).trigger("change");
                    return; // ❌ do not render board
                }
                //console.log("fetchHall - passing bookingData:", bookingData);
                renderBoard(bookingData);
            }
            else {
                $("#banquetDropdown, #reschedule_BanquetId").html(`<option value="0">No Hall Found</option>`);
                $("#booking-board").html(`<div class="alert alert-danger">No Hall Found. Please add halls in Company Settings.</div>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to fetch hall details');
        }
    });
}
//banquet hall details - dropdown -- END ----------------------

//Lead reference - dropdown -- START ----------------------
function loadLeadSourceList(selectedId = null) {
    $.ajax({
        url: "/api/LeadSource/GetLeadSourceList",
        type: "GET",
        success: function (response) {
            let options = `<option value="0">-- Select Reference --</option>`;

            if (response.status === "Success" && response.data?.length > 0) {
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.Name}</option>`;
                });
            } else {
                options += `<option value="0">No Reference Found</option>`;
            }

            const ddl = $("#booking_leadReferenceDropdown");
            ddl.html(options);

            // ✅ select AFTER options are loaded
            if (selectedId && selectedId != 0) {
                ddl.val(String(selectedId));
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load lead sources');  
        }
    });
}

//Lead reference - dropdown -- END ------------------------

//Enquiry Client - dropdown -- START ----------------------
function loadEnquiryList() {
    //showLoader();
    $.ajax({
        url: "/api/EnquiryDeal/GetEnquiryDealList",
        type: "GET",
        success: function (response) {
            //console.log("enquiry list response:", response);
            if (response.status === "Success") {
                let options = `<option value="0">-- Select Client --</option>`; // default option
                options += `<option value="0">+ Add New Client</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `
                               <option
                                   value="${item.Id}"
                                   data-name="${item.Name || ''}"
                                   data-email="${item.Email || ''}"
                                   data-address="${item.Address || ''}"
                                   data-contact="${item.Contact || ''}"
                               >${item.Name} (${item.Contact})</option>`;
                });
                $("#clientEnquiryDropdown").html(options);

                $("#clientEnquiryDropdown").on("change", function () {
                    const selected = $(this).find(":selected");

                    $("#clientName").val(selected.data("name") || "");
                    $("#clientEmail").val(selected.data("email") || "");
                    $("#clientAddress").val(selected.data("address") || "");
                    $("#clientContact").val(selected.data("contact") || "");
                });
            } else {
                $("#clientEnquiryDropdown").html(`<option value="0">No Enquiry Client Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load enquiry list');  
        }
    });
}
//Enquiry Client - dropdown -- END ------------------------

//State - dropdown -- START ----------------------
function loadStateList() {
    //showLoader();
    $.ajax({
        url: "/api/State/GetStateList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: 0     // or specific Id if needed
        }),
        success: function (response) {
            //console.log("state list response:", response);
            if (response.status === "Success") {
                let options = `<option value="0">-- Select State --</option>`; // default option
                let gujaratId = 0;
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.Name}</option>`;
                    if (item.Name.toLowerCase() === "gujarat") {
                        gujaratId = item.Id;
                    }
                });
                $("#clientStateDropdown, #invoiceModal_customerState").html(options);
                // Set default selection if found
                if (gujaratId > 0) {
                    $("#clientStateDropdown, #invoiceModal_customerState").val(gujaratId);
                }
            } else {
                $("#clientStateDropdown, #invoiceModal_customerState").html(`<option value="0">No State Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load state list');
        }
    });
}
//State - dropdown -- END ------------------------

//Event - dropdown -- START ----------------------
function loadEventTypeList() {
    $.ajax({
        url: "/api/EventType/GetEventTypeList",
        type: "GET",
        success: function (response) {
            //console.log("eventType response:", response);
            if (response.status === "Success") {
                let options = `<option value = "0">-- Select Event --</option> `; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value = "${item.Id}"> ${item.Name}</option> `;
                });
                $("#booking_eventDropdown").html(options);
                //loadSubEventList(eventTypeId);
            } else {
                $("#eventTypeDropdown").html(`<option value = "0"> No Event Type Found</option> `);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load EventType List");
        }
    });
}
function loadSubEventList(eventTypeId=0) {
    $.ajax({
        url: "/api/EventSubType/GetEventSubTypeList", // API endpoint
        type: "POST", // POST request
        contentType: "application/json", // Send data as JSON
        data: JSON.stringify({ EventTypeId: eventTypeId }), // Request body
        success: function (response) {
            //console.log("event subtype list response:", response);
            if (response.status === "Success") {
                let options = `<option value="0">-- Select SubEvent --</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.Name}</option>`;
                });
                $("#booking_subEventDropdown").html(options);
            } else {
                $("#booking_subEventDropdown").html(`<option value="0">No Event Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load sub-event list');
        }
    });
}
//Event - dropdown -- END ------------------------

//Food Package - dropdown -- START ----------------------
let foodPackages = [];

// ✅ STEP 1: Load dropdown data once
function loadFoodPackageList() {
    //showLoader();
    $.ajax({
        url: "/api/Company_FoodItem_Package/GetCompanyFoodPackage_Dropdown",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            if (response.status === "Success") {
                foodPackages = response.data;
                // populate first dropdown
                populateFoodPackageDropdown($("#foodPackage_PackageDropdown"));
            }
            else {
                $("#foodPackageDropdown").html(`<option value="0">No Package Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load food packages'); 
        }
    });
}

// ✅ STEP 2: Reusable dropdown populate
function populateFoodPackageDropdown($dropdown) {
    let options = `<option value="0">-- Select Package --</option>`;
    $.each(foodPackages, (i, pkg) => {
        options += `<option value="${pkg.Id}" data-rateperperson="${pkg.RatePerPerson}">${pkg.PackageName}</option>`;
    });
    $dropdown.html(options);
}

$(document).on("change", ".foodPackageDropdown", function () {
    const $row = $(this).closest(".food-row");

    const $dropdown = $row.find(".foodPackageDropdown");

    const selectedPackageId = $dropdown.val();

    if (!selectedPackageId || selectedPackageId === "0") {
        Swal.fire({
            icon: "warning",
            title: "Select Package",
            text: "Please select a food package first."
        });
        return;
    }
    const selectedOption = $dropdown.find("option:selected");
    const selectedText = selectedOption.text();
    const ratePerPerson = selectedOption.data("rateperperson") || 0;

    //console.log("selectedPackageId:", selectedPackageId, "selectedText:", selectedText);

    // Set Per Plate inside same row
    $row.find(".perPlate, #foodPackage_RatePerPlate").val(ratePerPerson);

    updatePackageCost($row);  // Pass row if needed

})

// ✅ STEP 3: Attach global delegated change event for ALL dropdowns
$(document).on("click", ".loadMenuBtn", function () {
    const $row = $(this).closest(".food-row");
    const $dropdown = $row.find(".foodPackageDropdown");
    const selectedPackageId = $dropdown.val();

    if (!selectedPackageId || selectedPackageId === "0") {
        Swal.fire({
            icon: "warning",
            title: "Select Package",
            text: "Please select a food package first."
        });
        return;
    }

    const selectedOption = $dropdown.find("option:selected");
    const selectedText = selectedOption.text();
    const ratePerPerson = selectedOption.data("rateperperson") || 0;

    //console.log("selectedPackageId:", selectedPackageId, "selectedText:", selectedText);

    // Set Per Plate inside same row
    //$row.find(".perPlate, #foodPackage_RatePerPlate").val(ratePerPerson);

    //updatePackageCost($row);  // Pass row if needed
    var payload = { Package_Id: Number(selectedPackageId) };
    //console.log("payload GetPackage_MenuAndFoodItemsList:", payload);

    $.ajax({
        url: "/api/BookingCalendar/GetPackage_MenuAndFoodItemsList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("response GetPackage_MenuAndFoodItemsList:", response);
            if (response.status === "Success" && response.data && response.data.length > 0)
            {
                renderMenuItems(response.data, () => {
                    $("#menuModal").data("activeDropdown", $dropdown);
                    // 🔥 RESTORE SELECTION
                    const savedItems = $dropdown.data("selected-items");
                    //console.log("restore savedItems:", savedItems);
                    restoreSavedMenuSelection(savedItems);

                    $("#menuModal .modal-title").text(`Menu Items [${selectedText}]`);
                    $("#menuModal").modal("show");
                });
            }
            else {
                Swal.fire({
                    icon: "info",
                    title: "Menu Items Not Found",
                    text: "No menu items found for this package."
                });
                $dropdown.val("0");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching Package MenuItems list");
        }
    });
});

function restoreSavedMenuSelection(savedItems) {
    if (!savedItems || savedItems.length === 0) return;
    savedItems.forEach(item => {

        const section = $(`#menuModal .menu-section[data-menu-id='${item.MenuHeader_Id}']`);
        if (!section.length) return;

        // 🔹 EXISTING ITEM
        if (item.MenuItem_Id && item.MenuItem_Id !== 0) {
            section.find(`.item-checkbox[value='${item.MenuItem_Id}']`).prop("checked", item.IsChecked);
        }

        // 🔹 NEW CUSTOM ITEM
        else if (item.MenuItem_Id === 0 && item.MenuItem_Name_NewAdded ) {
            const newItemHtml = `
                <div class="form-check me-3 mb-2">
                    <input class="form-check-input item-checkbox new-item"
                           type="checkbox"
                           value="0"
                           data-menu-id="${item.MenuHeader_Id}"
                           data-item-name="${item.MenuItem_Name_NewAdded}"
                           checked>
                    <label class="form-check-label">${item.MenuItem_Name_NewAdded}</label>
                </div>
            `;
            section.find(".item-container").append(newItemHtml);
        }
    });

    updateSelectAllState();
}
function updateSelectAllState() {
    $(".menu-section").each(function () {
        const allItems = $(this).find(".item-checkbox");
        const checkedItems = $(this).find(".item-checkbox:checked");

        const selectAllCheckbox = $(this).find(".select-all");

        if (allItems.length === checkedItems.length && allItems.length > 0) {
            selectAllCheckbox.prop("checked", true);
        } else {
            selectAllCheckbox.prop("checked", false);
        }
    });
}

function updatePackageCost() {

    let pax = parseFloat($("#booking_pax").val()) || 0;
    let rate = parseFloat($("#foodPackage_RatePerPlate").val()) || 0;

    let addOnsCost = parseFloat($("#foodPackage_Addons").val()) || 0;

    let packageCost = pax * rate;

    $("#foodPackage_PackageCost").val(packageCost.toFixed(2));
    packageCost = packageCost + addOnsCost;

    let isGSTEnabled = $("#gstSwitch").is(":checked");
    //let gst = isGSTEnabled ? getGST(hallRent) : 0;

    let gstAmount = isGSTEnabled ? ((packageCost * 5) / 100) : 0;
    $("#foodPackage_GST").val(gstAmount.toFixed(2));

    let totalCharges = packageCost + gstAmount;

    $("#foodPackage_TotalValue").text(totalCharges.toFixed(2));

    recalculatePaymentSummary();  // 🔥 IMPORTANT
}

$(document).on("input", "#foodPackage_RatePerPlate, #booking_pax", function () {
    updatePackageCost();
});
$(document).on("input", ".food-block .pax, .food-block .perPlate, .food-block .addonsCost", function () {
    const row = $(this).closest(".food-block");
    calculateFoodRowTotals(row);
});
$(document).on("input change","#paymentSummary_BookingDiscount_Type, #paymentSummary_BookingDiscount_Amount",
    function () {
        recalculatePaymentSummary();
});

$(document).on("input", "#OtherCharges_Decoration_Amount, #OtherCharges_RoomCharges_Amount, #OtherCharges_SoundSystem_Amount, #OtherCharges_ProjectorScreen_Amount",
    function () {
        recalculateAllOtherCharges();
        recalculatePaymentSummary();
    });

function recalculateAllOtherCharges() {
    calculateOtherCharge("#OtherCharges_Decoration_Amount", "#OtherCharges_Decoration_GST");
    calculateOtherCharge("#OtherCharges_RoomCharges_Amount", "#OtherCharges_RoomCharges_GST");
    calculateOtherCharge("#OtherCharges_SoundSystem_Amount", "#OtherCharges_SoundSystem_GST");
    calculateOtherCharge("#OtherCharges_ProjectorScreen_Amount", "#OtherCharges_ProjectorScreen_GST");
}

function calculateOtherCharge(amountSelector, gstSelector) {

    let amount = parseFloat($(amountSelector).val()) || 0;
    let isGSTEnabled = $("#gstSwitch").is(":checked");
    let gstRate = 5;

    let gstAmount = 0;

    if (isGSTEnabled) {
        gstAmount = (amount * gstRate) / 100;
    }

    $(gstSelector).val(gstAmount.toFixed(2));
}

//Calculate PAckage for Extra added package rows
function calculateFoodRowTotals(rowElement) {

    let pax = parseFloat($(rowElement).find(".pax").val()) || 0;
    let perPlate = parseFloat($(rowElement).find(".perPlate").val()) || 0;
    let addOnsCost = parseFloat($(rowElement).find(".addonsCost").val()) || 0;

    let packageCost = pax * perPlate;
    $(rowElement).find(".packageCost").val(packageCost.toFixed(2));

    packageCost = packageCost + addOnsCost;

    let isGSTEnabled = $("#gstSwitch").is(":checked");
    let gst = isGSTEnabled ? ((packageCost * 5) / 100) : 0;

    $(rowElement).find(".gstCost").val(gst.toFixed(2));

    let total = packageCost + gst;
    //console.log("total:", total, ", packageCost:", packageCost, ", gst:", gst);
    $(rowElement).find(".foodPackage_TotalValue").text(total.toFixed(2));

    recalculatePaymentSummary(); // 🔥 IMPORTANT
}


// When dropdown changes → fill Per Plate and recalc
$(document).on("change", ".foodPackageDropdown", function () {
    const row = $(this).closest(".food-block");
    const rate = $(this).find("option:selected").data("rateperperson") || 0;

    row.find(".perPlate").val(rate);
    calculateFoodRowTotals(row);
});

// When Pax OR Per Plate OR Addons change → recalc
$(document).on("input", ".pax, .perPlate, .addonsCost", function () {
    const row = $(this).closest(".food-block");
    calculateFoodRowTotals(row);
});

// ✅ STEP 4: Build modal dynamically
function renderMenuItems(menuData, callback) {

    //console.log("render Menu:", menuData);
    let html = "";
    menuData.forEach(menu => {
        const foodItems = menu.FoodItemsList?.length
            ? menu.FoodItemsList.map(item => `
                <div class="form-check me-3 mb-2">
                    <input class="form-check-input item-checkbox" type="checkbox"
                        value="${item.Item_Id}" data-menu-id="${menu.FoodMenu_Id}">
                    <label class="form-check-label">${item.Item_Name}</label>
                </div>`).join("")
            : `<p class="text-muted">No items found.</p>`;

        html += `
            <div class="border rounded p-3 mb-3 bg-light menu-section" data-menu-id="${menu.FoodMenu_Id}" data-inclusion-id="${menu.Inclusion_Id}">
                <div class="d-flex justify-content-between align-items-center mb-2">
                    <h6 class="mb-0 fw-bold">${menu.FoodMenu_Name} (${menu.Count_Size})</h6>
                    <div>
                        <input type="checkbox" class="form-check-input select-all" id="selectAll_${menu.FoodMenu_Id}">
                        <label for="selectAll_${menu.FoodMenu_Id}" class="form-check-label">Select All</label>
                    </div>
                </div>
                <div class="d-flex flex-wrap item-container">${foodItems}</div>
                <div class="d-flex mt-2">
                    <input type="text" class="form-control form-control-sm me-2 new-item-input" placeholder="Add New Item">
                    <button type="button" class="btn btn-sm btn-primary btn-addNewItem">Add</button>
                </div>
            </div>`;
    });

    $("#menuModalBody").html(html);
    // ✅ render complete
    if (callback) callback();
}

//ADD NEW ITEM
$(document).on("click", ".btn-addNewItem", function () {

    let section = $(this).closest(".menu-section");
    let menuId = section.data("menu-id");

    let input = section.find(".new-item-input");
    let itemName = input.val().trim();

    if (!itemName) {
        alert("Please enter item name");
        return;
    }

    let newItemHtml = `
        <div class="form-check me-3 mb-2">
            <input class="form-check-input item-checkbox new-item"
                   type="checkbox"
                   value="0"
                   data-menu-id="${menuId}"
                   data-item-name="${itemName}"
                   checked>
            <label class="form-check-label">${itemName}</label>
        </div>
    `;

    section.find(".item-container").append(newItemHtml);

    input.val("");
});

// ✅ STEP 5: Modal “Select All” and “Save” buttons
$(document).on("change", ".select-all", function () {

    const section = $(this).closest(".menu-section"); // ✅ current block only
    const checked = $(this).is(":checked");

    // ✅ apply only inside this section
    section.find(".item-checkbox").prop("checked", checked);
});

//StEP 6: Save MENU
$(document).on("click", ".btn-saveMenu", function () {
    let selectedMenuData = [];

    $(".menu-section").each(function () {
        const menuId = $(this).data("menu-id");
        const inclusionId = $(this).data("inclusion-id") || 0;

        $(this).find(".item-checkbox:checked").each(function () {
            let isNew = $(this).hasClass("new-item");
            let itemId = parseInt($(this).val()) || 0;

            selectedMenuData.push({
                Inclusion_Id: inclusionId,
                MenuHeader_Id: parseInt(menuId),   // ✅ FIX
                MenuItem_Id: itemId, //parseInt($(this).val()), // ✅ FIX
                MenuItem_Name_NewAdded: isNew ? $(this).data("item-name") : null,
                IsChecked: true
            });
        });
    });

    // attach selected items to the dropdown that opened modal
    const $activeDropdown = $("#menuModal").data("activeDropdown");
    if ($activeDropdown) {
        $activeDropdown.data("selected-items", selectedMenuData);
        //$activeDropdown.attr("data-selected-items", JSON.stringify(selectedMenuData));
    }

    console.log("Saved Menu Selection:", selectedMenuData);
    $("#menuModal").modal("hide");
});

//Food Package - dropdown -- END----------------------

//Booking - Food Package --- START --------------------------------------------------
const addFoodBtn = document.getElementById('addFoodBtn');
const foodContainer = document.getElementById('foodPackageContainer');
let addonCounter = 1;

addFoodBtn.addEventListener('click', () => {
    const newRow = document.createElement('div');
    newRow.className = 'food-block border rounded p-3 mt-3';

    newRow.innerHTML = `
            <!-- Row 1 -->
            <div class="row g-2 align-items-end p-2 mb-1">
                <div class="col-md-4">
                    <label class="form-label">Addon Food Package #${addonCounter}</label>
                    <input type="text" class="form-control form-control-sm" placeholder="Addon Food Package Name">
                </div>
                <div class="col-md-1">
                    <label class="form-label">Pax</label>
                    <input type="number" class="form-control form-control-sm pax" placeholder="Pax" value="0">
                </div>
                <div class="col-md-2">
                    <label class="form-label">Date</label>
                    <input type="date" class="form-control form-control-sm">
                </div>
                <div class="col-md-3">
                    <label class="form-label">Time</label>
                    <div class="d-flex gap-1">
                        <input type="number" class="form-control form-control-sm" placeholder="HH" min="1" max="12" style="width: 70px;">
                        <input type="number" class="form-control form-control-sm" placeholder="MM" min="0" max="59" style="width: 70px;">
                        <select class="form-select form-select-sm" style="width: 90px;">
                            <option value="AM">AM</option>
                            <option value="PM">PM</option>
                        </select>
                    </div>
                </div>
                <div class="col-md-2 d-flex align-items-end justify-content-end">
                    <button type="button" class="btn btn-outline-danger btn-sm remove-btn">&times;</button>
                </div>
            </div>

            <!-- Row 2 -->
            <div class="row g-2 food-row align-items-end mt-1">
                <div class="col-md-3">
                    <label class="form-label">Food Package</label>
                    <div class="d-flex gap-2">
                        <select class="form-select form-select-sm foodPackageDropdown"></select>
                        <button type="button"
                                class="btn btn-outline-primary btn-sm loadMenuBtn">
                            Menu
                        </button>
                    </div>
                    <input type="hidden" class="package_row_id" value="0" />
                </div>
                <div class="col-md-1">
                    <label class="form-label">Per Plate</label>
                    <input type="number" class="form-control form-control-sm perPlate" placeholder="Per Plate" value="0">
                </div>
                <div class="col-md-2">
                    <label class="form-label">Package Cost</label>
                    <input type="number" class="form-control form-control-sm packageCost" placeholder="Package Cost" value="0" readonly>
                </div>
                <div class="col-md-3">
                    <label class="form-label">Addons</label>
                    <input type="number" class="form-control form-control-sm addonsCost" placeholder="Addons" value="0" readonly>
                </div>
                <div class="col-md-1 d-none">
                    <label class="form-label">GST</label>
                    <input type="number" class="form-control form-control-sm gstCost" placeholder="GST" value="0" readonly>
                </div>
                <div class="col-md-4 d-flex align-items-center justify-content-between">
                    <button type="button" class="btn btn-outline-success btn-sm mt-2 foodPackage_AddOnItems" onclick="event.preventDefault(); openAddonModal(0, this)"><i class="fa fa-check"></i> Addons</button>
                    <span class="fw-bold">Total: <span class="foodPackage_TotalValue">0</span></span>
                </div>
            </div>
        `;

    foodContainer.appendChild(newRow);

    // Populate dropdown in new row
    const dropdown = newRow.querySelector(".foodPackageDropdown");
    populateFoodPackageDropdown($(dropdown));

    // On change of dropdown, set Per Plate rate
    // dropdown.addEventListener('change', function () {
    //     const selected = this.options[this.selectedIndex];
    //     const rate = selected.dataset.rateperperson || 0;
    //     newRow.querySelector(".perPlate").value = rate;
    // });

    // Remove functionality
    newRow.querySelector('.remove-btn').addEventListener('click', () => {
        foodContainer.removeChild(newRow);
    });

    addonCounter++;
});
//Booking - Food Package --- /END --------------------------------------------------

//Food Addons ----- START ----------------------------------------------------------------
let addon_Data = [];
let headerTotals = {};
loadAddonCharges();
function loadAddonCharges() {
    $.ajax({
        url: "/api/Company_FoodRate_AddonCharge/GetCompanyFoodRateAddonCharge",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            //console.log("res AddOnCharges:", res);
            if (res.status === "Success" && res.data && res.data.length > 0) {
                addon_Data = res.data;
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load addon charges');
        }
    });
}

function populateAddonModal(savedItems = []) {
    console.log("populate AddOns:", savedItems);
    let container = $("#addonItemsContainer");
    let summaryContainer = $("#headerSummaryContainer");
    container.empty();
    summaryContainer.empty();
    headerTotals = {};

    // Group data by header
    let grouped = {};
    addon_Data.forEach(item => {
        if (!grouped[item.HeaderName]) grouped[item.HeaderName] = [];
        grouped[item.HeaderName].push(item);
    });

    Object.keys(grouped).forEach(header => {
        headerTotals[header] = 0;

        let headerCard = $(`
                <div class="card shadow-sm border-0">
                    <div class="card-header d-flex justify-content-between align-items-center bg-light">
                        <span class="fw-bold">${header}</span>
                        <button class="btn btn-sm btn-outline-secondary toggle-btn">−</button>
                    </div>
                    <div class="card-body p-2" data-header-body="${header}"></div>
                </div>
            `);

        // Column headers
        let columnHeader = $(`
                <div class="d-flex fw-bold text-secondary border-bottom pb-1 mb-2">
                    <div class="flex-grow-1">Food Item</div>
                    <div style="width:100px;" class="text-center">Rate</div>
                    <div style="width:100px;" class="text-center">Pax</div>
                    <div style="width:130px;" class="text-center">Total</div>
                </div>
            `);
        headerCard.find(`[data-header-body="${header}"]`).append(columnHeader);

        // Populate items
        grouped[header].forEach(item => {
            // Find if this item was saved previously
            let saved = savedItems.find(s =>
                (s.AddOn_Item_Id === item.Id) &&
                (s.AddOn_Header_Id === item.HeaderId)
            );
            //console.log("saved AddOns:", saved);
            // If found, prefill values
            let isChecked = saved ? true : false;
            let rate = saved ? saved.AddOn_Rate : item.Amount;
            //let pax = saved ? saved.AddOn_Pax : ($("#booking_pax").val() || 0);

            let rowPax = 0;
            if (currentAddonRow && currentAddonRow.length > 0) {
                let paxInput = currentAddonRow.find(".pax");

                if (paxInput.length > 0) {
                    rowPax = parseFloat(paxInput.val()) || 0;
                }
                else {
                    // fallback for first default row (which doesn't have .pax)
                    rowPax = parseFloat($("#booking_pax").val()) || 0;
                }
            }
            let pax = saved ? saved.AddOn_Pax : rowPax;

            let total = saved ? saved.AddOn_Total : 0;

            let row = $(`
                    <div class="d-flex align-items-center justify-content-between mb-2"
                         data-header="${item.HeaderName}" data-headerid="${item.HeaderId}" data-id="${item.Id}">

                        <div class="d-flex align-items-center flex-grow-1">
                            <input type="checkbox" class="form-check-input me-2 addon-checkbox" ${isChecked ? "checked" : ""}>
                            <span>${item.ItemName}</span>
                        </div>

                        <input type="number" class="form-control form-control-sm rate-input text-center mx-1 p-1" step="1"
                               value="${rate}" min="0" style="width:90px;">

                        <input type="number" class="form-control form-control-sm pax-input text-center mx-1 p-1" step="1"
                               value="${pax}" min="0" style="width:90px;">

                        <input type="number" class="form-control form-control-sm total-input text-center mx-1 bg-light p-1"
                               value="${total.toFixed(2)}" readonly style="width:120px;">
                    </div>
                `);

            row.find(".rate-input, .pax-input").on("input", function () {
                recalcRow_AddOn(row);
            });

            row.find(".addon-checkbox").on("change", function () {
                recalcRow_AddOn(row);
            });

            headerCard.find(`[data-header-body="${header}"]`).append(row);
        });

        container.append(headerCard);

        // Summary row
        summaryContainer.append(`
                <div class="d-flex justify-content-between mb-1" data-header-summary="${header}">
                    <span>${header}</span>
                    <span class="header-total">0</span>
                </div>
            `);

        // Toggle button
        headerCard.find(".toggle-btn").on("click", function () {
            let body = headerCard.find(`[data-header-body="${header}"]`);
            let isVisible = body.is(":visible");
            body.toggle();
            $(this).text(isVisible ? "+" : "−");
        });
    });

    $("[data-header]").each(function () {
        recalcRow_AddOn($(this));
    });
}

function recalcRow_AddOn(row) {
    //console.log("here3");
    let checked = row.find(".addon-checkbox").is(":checked");
    let rate = parseFloat(row.find(".rate-input").val()) || 0;
    let pax = parseFloat(row.find(".pax-input").val()) || 0;
    let total = checked ? rate * pax : 0;

    //console.log("---- Addon Row Calculation ----");
    //console.log("Checked:", checked);
    //console.log("Rate:", rate);
    //console.log("Pax:", pax);
    //console.log("Computed Total:", total);
    //console.log("-------------------------------");

    row.find(".total-input").val(total.toFixed(2));
    //console.log("here4");
    recalcSummary_AddOn();
}

function recalcSummary_AddOn() {
    //console.log("recalcSummary called");
    // Reset
    Object.keys(headerTotals).forEach(h => headerTotals[h] = 0);

    //console.log("1 headerTotals:", headerTotals);
    $("[data-header]").each(function () {
        let header = $(this).data("header");
        let total = parseFloat($(this).find(".total-input").val()) || 0;
        headerTotals[header] += total;
    });

    Object.keys(headerTotals).forEach(header => {
        $(`[data-header-summary="${header}"] .header-total`).text(headerTotals[header].toFixed(2));
    });

    //console.log("2 headerTotals:", headerTotals);
    let finalTotal = Object.values(headerTotals).reduce((a, b) => a + b, 0);
    //console.log("finalTotal:", finalTotal);
    $("#finalTotal").text(finalTotal.toFixed(2));

    if (currentAddonButton) {
        const $row = $(currentAddonButton).closest(".food-row, .food-block");
        $row.find("#foodPackage_Addons, .addonsCost").val(finalTotal.toFixed(2));

    // Recalculate that specific row only
    //    if ($row.hasClass("food-block")) {
    //        calculateFoodRowTotals($row);
    //    }
    //    else {
    //        updatePackageCost();
    //    }
    }
    updatePackageCost();
}

// Open modal function
let currentAddonButton = null;
let currentAddonRow = null;

function openAddonModal(booking_foodPackageId = 0, button = null) {
    //console.log("booking_foodPackageId:", booking_foodPackageId);
    currentAddonButton = button; // 🔹 store reference to clicked button
    //currentAddonRow = $(button).closest(".food-row, .food-block");

    currentAddonRow = $(button).closest(".food-block");   // for dynamic rows
    if (currentAddonRow.length === 0) {
        // default first row (no .food-block wrapper)
        currentAddonRow = $(button).closest(".food-row");
    }

    let savedItems = $(button).data("selected-addonitems") || [];
    console.log("addon data saved:", savedItems);
    console.log("addon data addon_Data:", addon_Data);

    if (addon_Data.length === 0) {
        populateAddonModal();
    } else {
        populateAddonModal(savedItems);

        // Reset button text if no items saved
        if (savedItems.length === 0) {
            $(button).html('<i class="fa fa-check"></i> Addons');
        }
    }
    $("#addonChargeModal").modal("show");
}

//SAVE AddOns Modal
$(document).on("click", ".btn-saveAddOn", function () {
    let selectedItems = [];
    let grandTotal = 0;

    // Loop through all checked rows
    $("#addonItemsContainer .addon-checkbox:checked").each(function () {
        let row = $(this).closest("[data-header]");
        let header = parseInt(row.data("headerid"));
        let itemId = parseInt(row.data("id"));
        let rate = parseFloat(row.find(".rate-input").val() || 0);
        let pax = parseFloat(row.find(".pax-input").val() || 0);
        let total = parseFloat(row.find(".total-input").val() || 0);

        grandTotal += total;

        selectedItems.push({
            AddOn_Header_Id: header,
            AddOn_Item_Id: itemId,
            AddOn_Rate: rate,
            AddOn_Pax: pax,
            AddOn_Total: total
        });
    });

    // No selection warning
    if (selectedItems.length === 0) {
        Swal.fire({
            icon: "warning",
            title: "No Addons Selected",
            text: "Please select at least one addon item."
        });
        return;
    }

    console.log("✅ Selected Addon Items:", selectedItems);
    //console.log("💰 Grand Total:", grandTotal.toFixed(2));

    // 🔹 Update the clicked AddOn button’s total span
    if (currentAddonButton) {
        //console.log("here addon btn 1");
        let $btn = $(currentAddonButton);
        let $totalSpan = $btn.closest(".d-flex").find(".foodPackage_TotalValue");

        // Update total text
        $totalSpan.text(grandTotal.toFixed(2));

        // Store selected items for later retrieval
        $btn.data("selected-addonitems", selectedItems);
        $btn.data("total", grandTotal);

        // 🔹 Update button label with count
        let itemCount = selectedItems.length;
        $btn.html(`<i class="fa fa-check"></i> Addons (${itemCount})`);

        //Update Amount
        let row = $(currentAddonButton).closest(".food-block");
        // fallback for default row (no .food-block wrapper)
        if (row.length === 0) {
            row = $(currentAddonButton).closest(".food-row");
        }
        //console.log("grandTotal 2:", grandTotal);

        updatePackageCost();
        //calculateFoodRowTotals(row);
        //recalculatePaymentSummary();

        //console.log("grandTotal 3:", grandTotal);
    }
    // Close the modal
    $("#addonChargeModal").modal("hide");
});

// Function to update the Addon button with the selected items and total
function updateAddonButton(selectedItems) {
    // Find the button that was clicked (assuming we pass it as a context)
    let addonButton = $(".btn-outline-success").last();  // Adjust as needed to target the correct button
    let total = selectedItems.reduce((sum, item) => sum + item.total, 0);

    // Update the button's total value
    addonButton.next().find(".foodPackage_TotalValue").text(total.toFixed(2));

    // Store the selected items for later use
    addonButton.data("selected-addonitems", selectedItems);
}
//Food Addons ----- /END ------------------------------------------------------------------

const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
let shiftNames = ["FD", "MO", "AF", "EV", "ON"]; //,"BF","LN","HT","DN"];
const legendColors = { available: "success", past: "secondary", confirmed: "danger", online: "primary", tentative: "light", blocked: "dark", mahurat: "pink" };

const yearSelect = document.getElementById("yearSelect");
const monthButtons = document.getElementById("monthButtons");
const board = document.getElementById("booking-board");
const listTableBody = document.querySelector("#listTable tbody");
const calendarView = document.getElementById("calendarView");
const listView = document.getElementById("listView");
const toggleViewBtn = document.getElementById("toggleViewBtn");

let selectedYear = new Date().getFullYear();
let selectedMonth = new Date().getMonth();
let selectedYearId = 0;
let selectedMonthId = 0;
let cacheddays = [];
let cachedShiftData = {};

//Fetch Year
function populateYearDropdown() {
    $.ajax({
        url: '/api/YearMaster/GetYearList', // replace with actual controller route
        type: 'GET',
        contentType: 'application/json',
        success: function (res) {
            if (res.status === "Success") {
                yearSelect.innerHTML = "";
                const years = res.data;
                //console.log("years list:", years);

                // Find current year object
                let currentYearObj = years.find(y => y.YearName == new Date().getFullYear());
                selectedYear = currentYearObj ? parseInt(currentYearObj.YearName) : new Date().getFullYear();
                selectedYearId = currentYearObj ? parseInt(currentYearObj.Id) : 0;
                //console.log("selectedYear:", selectedYear, ", selectedYearId:", selectedYearId);

                years.forEach(y => {
                    const option = document.createElement("option");
                    option.value = y.Id;        // yearId
                    option.textContent = y.YearName;
                    if (y.YearName == selectedYear) option.selected = true;
                    yearSelect.appendChild(option);
                });

                // Fetch month data for selected year
                fetchMonthData(yearSelect.value);
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, status, error, "Failed to Load Year");
        }
    });
}

//Year - change event
yearSelect.addEventListener("change", function () {
    selectedYear = yearSelect.options[yearSelect.selectedIndex].text; // display year
    selectedYearId = yearSelect.options[yearSelect.selectedIndex].value;
    //console.log("yearChange - selectedYear:", selectedYear, ", selectedYearId:", selectedYearId);

    fetchMonthData(yearSelect.value); // fetch months for selected year
});

//Fetch Month by Year
function fetchMonthData(yearId) {
    //console.log("yearId in MonthData:", yearId);
    $.ajax({
        url: '/api/MonthMaster/GetMonthList', // replace with actual controller route
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ YearId: parseInt(yearId), MonthId: 0 }),
        success: function (res) {
            if (res.status === "Success") {
                populateMonthButtons(res.data);
                //console.log("getMonths:", res.data);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching months");
        }
    });
}

//DISPLAY Months
function populateMonthButtons(months) {
    selectedMonthId = months[selectedMonth].MonthId;
    //console.log("fetched months:", months, ", selectedMonth:", selectedMonth, ", selectedMonthId:", selectedMonthId);

    monthButtons.innerHTML = "";
    months.forEach((m, idx) => {
        const btn = document.createElement("button");
        btn.className = `btn btn-outline-primary btn-sm ${idx === selectedMonth ? "active" : ""}`;
        btn.textContent = m.MonthName;
        btn.dataset.monthId = m.MonthId;
        btn.dataset.yearId = m.YearId;

        //console.log("DEFAULT YearId:", m.YearId, "MonthId:", m.MonthId);
        btn.addEventListener("click", () => {
            selectedMonth = idx;
            // Highlight selected month
            populateMonthButtons(months);

            //console.log("Selected YearId:", selectedYearId, "MonthId:", selectedMonthId);
            // Call renderBoard() or other logic if needed
            //renderBoard();
            //loadBookingCalendar();
        });
        monthButtons.appendChild(btn);
    });
    //console.log("DEFAULT selectedYear:", selectedYear, ", selectedMonth:", selectedMonth);
    loadBookingCalendar();
}

//Generate days for Selected Month
function generatedays(year, month) {
    return new Promise((resolve) => {
        const req = {
            Year: year,
            Month: month + 1 // JS month 0-based -> API month 1-based
        };
        //console.log("GetEnquiryCalendarMock payload:", req);
        $.ajax({
            url: "/api/BookingCalendar/GetEnquiryCalendarMock",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(req),
            success: function (res) {
                //console.log("GetEnquiryCalendarMock res:", res);
                try {
                    if (res && res.status === "Success" && Array.isArray(res.data)) {
                        const days = res.data.map(item => {
                            const dd = item.Date;                         // "01"
                            const day = item.Day;                         // "Mon"
                            const fullDate = new Date(year, month, parseInt(dd, 10));
                            return {
                                date: String(dd).padStart(2, "0"),
                                day: day,
                                dateObj: fullDate,
                                enquiries: Number(item.Enquiries) || 0,
                                enquiryList: Array.isArray(item.EnquiryList) ? item.EnquiryList : []
                            };
                        });
                        resolve(days);
                        return;
                    }
                } catch (err) {
                    console.error("generatedays.parseError:", err);
                }

                // fallback: return empty days (no enquiries)
                resolve(generateEmptydays(year, month));
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, status, error, "Failed to Load Calendar Days");
                resolve(generateEmptydays(year, month));
            }
        });
    });
}

// small helper used as fallback when API fails
function generateEmptydays(year, month) {
    const days = [];
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    for (let d = 1; d <= daysInMonth; d++) {
        const dateObj = new Date(year, month, d);
        const dayName = dateObj.toLocaleDateString("en-US", { weekday: "short" });
        days.push({
            date: String(d).padStart(2, "0"),
            day: dayName,
            dateObj,
            enquiries: 0,
            enquiryList: []
        });
    }
    return days;
}

//Generate Shifts Colors for days of Month
function generateShiftDataPerDay(dateObj) {
    return shiftNames.map(() => {
        const today = new Date();
        if (dateObj < today) {
            return legendColors.past;    // ✅ Always gray for past
        }
        return legendColors.available;   // ✅ Always green for current & future
    });
}

var global_bookingData = null;
//main booking board
async function renderBoard(bookingData = null) {
    global_bookingData = bookingData;
    board.innerHTML = "";
    //console.log("renderBoard selectedYear:", selectedYear, ", selectedMonth:", selectedMonth);
    cacheddays = await generatedays(selectedYear, selectedMonth);

    console.log("renderBoard bookingData:", bookingData);

    // Pre-generate shift data per resource per day
    cachedShiftData = {};
    banquetHallsList.forEach(hall => {
        cachedShiftData[hall.Id] = cacheddays.map(d => generateShiftDataPerDay(d.dateObj));
    });
    //console.log("renderBoard cachedShiftData:", cachedShiftData);

    //Title Label - Month-Year Enquiries
    const dateRow = document.createElement("div");
    dateRow.className = "row-wrapper";
    const headerLabel = document.createElement("div");
    headerLabel.className = "text-center";
    headerLabel.style.minWidth = "80px";
    headerLabel.innerHTML = `<div class="fw-bold">${monthNames[selectedMonth]}-${selectedYear}</div><div class="mt-2">Enquiries</div>`;
    dateRow.appendChild(headerLabel);

    //Column Header -> Date - Day - Enquiry Count
    cacheddays.forEach(d => {
        const cell = document.createElement("div");
        cell.className = "text-center small fw-bold border rounded p-1 bg-light day-cell";

        const badge = document.createElement("span");
        badge.className = "badge badge-enquiries border border-dark text-black bg-white";
        badge.style.cursor = "pointer";
        badge.style.padding = "0.15rem 0.25rem"; // smaller than p-1
        badge.style.fontSize = "0.6rem";        // ✅ reduced font size
        badge.innerText = d.enquiries;

        // Make sure enquiryList is always an array
        if (!Array.isArray(d.enquiryList)) d.enquiryList = [];
        badge.dataset.enquiryList = JSON.stringify(d.enquiryList);

        cell.innerHTML = `${d.date}<br><span class="text-muted" style="font-size:0.6rem">${d.day}</span><br>`;
        cell.appendChild(badge);
        dateRow.appendChild(cell);
    });
    board.appendChild(dateRow);

    //console.log("shiftTimes:", shiftTimes);
    // Resource Rows
    // Loop through each banquet hall object in the list
    banquetHallsList.forEach(hall => {
        // Create a wrapper <div> that will represent one row (one hall + its days/shifts)
        const row = document.createElement("div");
        // Assign CSS classes:
        // - row-wrapper → custom layout styling
        // - align-items-center → vertically center content (Bootstrap utility)
        row.className = "row-wrapper align-items-center";

        // =========================
        // Hall Name Section (Left)
        // =========================

        // Create a <div> to display the hall name
        const nameCell = document.createElement("div");
        // Apply styling classes:
        // - text-center → center text horizontally
        // - rounded → rounded corners
        // - p-2 → padding
        // - name-cell → custom styling hook
        // - bg-warning-subtle → light background color (Bootstrap)
        nameCell.className = "text-center rounded p-2 name-cell bg-warning-subtle";
        // Set the visible text as the hall's name
        nameCell.textContent = hall.HallName;
        // Append the name cell to the row (leftmost column)
        row.appendChild(nameCell);
        
        // =======================================
        // Loop through shift data for this hall
        // =======================================

        // Access cached shift data using hall.Id as key
        // shiftArr = data for a specific day
        // dayIndex = index of the day (e.g., 0 = Sunday, 1 = Monday, etc.)
        cachedShiftData[hall.Id].forEach((shiftArr, dayIndex) => {
            // Create a <div> for each day cell
            const cell = document.createElement("div");
            // Apply styling:
            // - border → adds border
            // - rounded → rounded corners
            // - p-1 → small padding
            // - text-center → center content
            // - day-cell → custom class for targeting
            // Default state assumed "available" (green as per your comment)
            cell.className = "border rounded p-1 text-center day-cell"; // ✅ green default

            // ===============================
            // ✅ Optimized Date Comparison
            // ===============================

            // Normalize today's date to midnight once
            const todayTime = new Date().setHours(0, 0, 0, 0);
            // Normalize current cell date to midnight
            const cellTime = new Date(cacheddays[dayIndex].dateObj).setHours(0, 0, 0, 0);
            // Flag to track if current cell is today
            let isToday = false;
            // Remove any previous state classes (important for re-render cases)
            cell.classList.remove("bg-secondary", "bg-success", "text-white");

            // ===============================
            // Case 1: Past Date
            // ===============================
            if (cellTime < todayTime) {
                cell.classList.add("bg-secondary", "text-white");
            }

            // ===============================
            // Case 2: Today
            // ===============================
            else if (cellTime === todayTime) {
                cell.classList.add("bg-success", "text-white");
                isToday = true;
            }

            // ===============================
            // Case 3: Future Date
            // ===============================
            else {
                cell.classList.add("bg-success", "text-white");
            }

            // Extract selected year (e.g., 2026)
            const yyyy = selectedYear;

            // Convert month index (0–11) → (01–12) with leading zero
            const mm = String(selectedMonth + 1).padStart(2, "0");

            // Get day from cached days and ensure 2-digit format (01–31)
            const dd = String(cacheddays[dayIndex].date).padStart(2, "0");

            // Format 1: Human readable (e.g., 12-Apr-2026)
            const dateStr = `${dd}-${monthNames[selectedMonth]}-${yyyy}`;

            // Format 2: Numeric (e.g., 12-04-2026)
            const dateStr_Format = `${dd}-${mm}-${yyyy}`;

            // Format 3: ISO format (e.g., 2026-04-12) → best for APIs / DB
            const dateStr_ISO = `${yyyy}-${mm}-${dd}`;

            // Initialize empty array to store bookings applicable for this hall + day
            let bookingsToday = [];

            // Ensure bookingData is a valid array before processing
            if (Array.isArray(bookingData)) {

                // Filter bookings for:
                // 1. Current hall
                // 2. Current date falling between FromDate and TillDate
                bookingsToday = bookingData.filter(b => {

                    // Skip booking if it belongs to a different hall
                    if (b.BanquetId !== hall.Id)
                        return false;

                    const normalize = (d) => new Date(d).setHours(0, 0, 0, 0);
                    const fromDate = normalize(b.FromDate);
                    const tillDate = normalize(b.TillDate);
                    const currentDate = normalize(dateStr_ISO);
                    // Keep booking if current date lies within booking range (inclusive)
                    return currentDate >= fromDate && currentDate <= tillDate;
                });
            }

            // Set to track slots that should be skipped (used later for FD merge logic)
            let skipNextSlots = new Set();

            // Check if ANY booking for today is Full Day (FD)
            const hasFDBooking = bookingsToday.some(b => b.ShiftCode === "FD");

            // Get unique Booking IDs (avoid duplicate counting if same booking spans multiple entries)
            const uniqueIds = [...new Set(bookingsToday.map(b => b.BookingId))];

            // Count total unique bookings for the day
            const bookingCount = uniqueIds.length;

            // Flag to indicate if ANY booking exists for that day
            const hasAnyBooking = bookingCount > 0;

            // ===============================
            // Group bookings by ShiftCode
            // ===============================
            const shiftBookingMap = {};
            bookingsToday.forEach(b => {
                const shift = (b.ShiftCode || "").trim().toUpperCase();
                if (!shiftBookingMap[shift]) {
                    shiftBookingMap[shift] = [];
                }
                shiftBookingMap[shift].push(b);
            });

            // ===============================
            // Populate cell with shift data
            // ===============================

            // Generate inner HTML for each shift (FD, MO, AF, EV, ON)
            // shiftArr → array representing visual state placeholders for each shift
            cell.innerHTML = shiftArr.map((color, i) => {

                // If this slot index is marked to be skipped (e.g., FD merged view),
                // return empty string → no rendering for this slot
                if (skipNextSlots.has(i)) return "";

                // Default color coming from shiftArr (predefined state)
                let finalColor = color;

                // If this column is TODAY, override color to "success" (green)
                // ⚠️ This is a GLOBAL override → can suppress booking colors later
                if (isToday) {
                    finalColor = "success";
                }

                // Decide text color based on background:
                // - light bg → black text
                // - others → white text
                var textColor = finalColor === "light" ? "text-black" : "text-white";

                // Get shift code based on index (e.g., 0=FD, 1=MO, 2=AF, 3=EV, 4=ON)
                const shiftCode = shiftNames[i];

                // Get full readable name (e.g., "Evening" for EV)
                const shiftFull = fullShiftName(shiftCode);

                var bookingId = 0;
                var eventName = null;
                var clientName = null;
                var clientContact = null;
                var eventDateRange = null;

                const slotStart = timeToMinutes(shiftTimes[shiftCode].start);
                const slotEnd = timeToMinutes(shiftTimes[shiftCode].end);

                let isBookingConfirmed = false;
                let isBookingTentative = false;
                let isBlocked = false;

                // Will hold the matched booking for this specific slot (FD/MO/AF/EV/ON)
                let bookingMatch = null;

                // Default display text is the shift code (e.g., FD, MO, EV...)
                let displayText = shiftCode;

                // Ensure bookingData is valid before processing
                if (Array.isArray(bookingData)) {

                    // ===============================
                    // Filter bookings for current hall + current date
                    // ===============================
                    const bookingsToday = bookingData.filter(b => {

                        // Skip if booking belongs to another hall
                        if (b.BanquetId !== hall.Id)
                            return false;

                        // Convert booking date range
                        const fromDate = new Date(b.FromDate);
                        const tillDate = new Date(b.TillDate);

                        // Current cell date
                        const currentDate = new Date(dateStr_ISO);

                        // Keep only bookings active on this date
                        return currentDate >= fromDate && currentDate <= tillDate;
                    });

                    // ===============================
                    // Determine display text for cell
                    // ===============================

                    // If FULL DAY booking exists → override display text
                    const fdBooking = bookingsToday.find(b => b.ShiftCode === "FD");

                    if (fdBooking) {
                        displayText = "FD"; // show FD
                    }
                    else if (bookingsToday.length > 0) {

                        // Count unique bookings for that day
                        const uniqueBookingIds = [...new Set(bookingsToday.map(b => b.BookingId))];

                        // Show count (e.g., "1", "2", etc.)
                        displayText = uniqueBookingIds.length;
                    }

                    // ===============================
                    // Loop through ALL bookings (⚠️ not just bookingsToday)
                    // ===============================
                    for (const booking of bookingsToday) {

                        // Skip other halls
                        if (booking.BanquetId !== hall.Id)
                            continue;

                        // Date filtering again (duplicate logic)
                        const fromDate = new Date(booking.FromDate);
                        const tillDate = new Date(booking.TillDate);
                        const currentDate = new Date(dateStr_ISO);

                        // ===============================
                        // DIRECT SHIFT MATCH (CRITICAL FIX)
                        // ===============================
                        if (booking.ShiftCode === shiftCode) {

                            bookingMatch = booking;

                            if (booking.BookingCalendar_IsBlocked) {
                                isBlocked = true;
                                break;
                            }

                            bookingId = booking.BookingId;
                            eventName = booking.Booking_SubEventName;
                            clientName = booking.Client_Name;
                            clientContact = booking.Client_Contact;

                            eventDateRange =
                                booking.FromDate_Short + ' ' +
                                booking.StartTime_Full + ' - ' +
                                booking.TillDate_Short + ' ' +
                                booking.EndTime_Full;

                            if (booking.Booking_TypeId == 2) {
                                isBookingTentative = true;
                            } else {
                                isBookingConfirmed = true;
                            }

                            break;
                        }


                        if (!(currentDate >= fromDate && currentDate <= tillDate))
                            continue;

                        // ===============================
                        // FULL DAY BLOCK handling
                        // ===============================
                        if (booking.BookingCalendar_IsBlocked && booking.ShiftCode === "FD") {
                            isBlocked = true;
                            bookingMatch = booking;
                            break;
                        }

                        // ===============================
                        // Time-based matching
                        // ===============================

                        // Convert booking start/end time to minutes
                        const bookingStart = timeToMinutes(booking.StartTime_Full);
                        const bookingEnd = timeToMinutes(booking.EndTime_Full);

                        // Check if booking overlaps with current slot
                        if (isTimeOverlap(bookingStart, bookingEnd, slotStart, slotEnd)) {

                            // Found matching booking for this slot
                            bookingMatch = booking;

                            // If blocked → mark and stop
                            if (booking.BookingCalendar_IsBlocked) {
                                isBlocked = true;
                                break;
                            }

                            // Assign booking details for UI / tooltip
                            bookingId = booking.BookingId;
                            eventName = booking.Booking_SubEventName;
                            clientName = booking.Client_Name;
                            clientContact = booking.Client_Contact;

                            // Build display date-time range string
                            eventDateRange =
                                booking.FromDate_Short + ' ' +
                                booking.StartTime_Full + ' - ' +
                                booking.TillDate_Short + ' ' +
                                booking.EndTime_Full;

                            // Booking type handling
                            if (booking.Booking_TypeId == 2) {
                                isBookingTentative = true;   // Tentative booking
                            } else {
                                isBookingConfirmed = true;   // Confirmed booking
                            }

                            // Stop after first match
                            break;
                        }
                    }
                }

                // ===============================
                // 🎯 MULTI-SLOT MERGE LOGIC (Improved)
                // ===============================
                let spanCount = 1;
                // Guard: only proceed if valid booking exists and not blocked
                if (bookingMatch && !isBlocked) {

                    const bookingShift = (bookingMatch.ShiftCode || "").trim().toUpperCase();

                    // ===============================
                    // ✅ Case 1: FULL DAY booking → merge ALL remaining slots
                    // ===============================
                    if (bookingShift === "FD") {

                        for (let j = i + 1; j < shiftNames.length; j++) {
                            spanCount++;
                            skipNextSlots.add(j); // hide all next slots
                        }
                    }

                    // ===============================
                    // ✅ Case 2: Non-FD → DO NOT MERGE (important fix)
                    // ===============================
                    else {
                        // No merging for EV / MO / AF / ON
                        // Each slot should render independently
                        spanCount = 1;
                    }
                }

                // ===============================
                // 🚨 DAY-LEVEL OVERRIDE (Final Color Decision)
                // ===============================

                // 🎯 CASE 1: Full Day booking exists → only FD slot should be red
                if (hasFDBooking && shiftCode === "FD") {
                    finalColor = "danger";       // red background
                    textColor = "text-white";    // white text
                }

                // 🎯 CASE 2: Non-FD bookings exist → show count in FD slot (red)
                else if (!hasFDBooking && hasAnyBooking && shiftCode === "FD") {
                    finalColor = "danger";
                    textColor = "text-white";
                }

                // 🎯 CASE 3: Actual booked slot (EV / MO / AF / ON)
                else if (shiftBookingMap[shiftCode]?.length > 0) { //isBookingConfirmed) {
                    finalColor = "danger";
                    textColor = "text-white";
                }

                // 🎯 CASE 4: Tentative booking (soft hold)
                else if (isBookingTentative) {
                    finalColor = "white";
                    textColor = "text-dark";
                }

                // 🎯 CASE 5: Blocked slot (admin/manual block)
                else if (isBlocked) {
                    finalColor = "dark";
                    textColor = "text-white";
                }

                // ===============================
                // Dynamic height for merged slots
                // ===============================

                // If multiple slots are merged (FD case),
                // increase height proportionally (28px per slot)
                const mergeStyle = spanCount > 1
                    ? `style="height:${spanCount * 28}px; line-height:${spanCount * 28}px;"`
                    : "";

                // ===============================
                // Render slot badge (UI element)
                // ===============================
                return `<div
                    class="badge bg-${finalColor} d-block mb-1 ${textColor}" 
                    ${mergeStyle}                                              
                    data-bookingid="${bookingId}"          
                    data-eventname="${eventName}"          
                    data-clientname="${clientName}"        
                    data-clientcontact="${clientContact}"  
                    data-eventdaterange="${eventDateRange}"

                    data-shift="${shiftCode}"              
                    data-shiftname="${shiftFull}"          

                    data-date="${dateStr}"                 
                    data-date_format="${dateStr_Format}"   
                    data-date_iso="${dateStr_ISO}"         

                    data-hallname="${hall.HallName}"       
                    data-hallid="${hall.Id}">              

                    ${getDisplayText()}                   
                </div>`;

                // ===============================
                // Function: Decide what text to show inside cell
                // ===============================
                function getDisplayText() {

                    // Case 1: If FD booking exists → always show FD
                    if (hasFDBooking) return "FD";

                    // Case 2: If bookings exist → show count ONLY in FD slot
                    if (bookingCount > 0 && shiftCode === "FD") {
                        return bookingCount;
                    }

                    // Case 3: Default → show shift code (MO, AF, EV, etc.)
                    return shiftCode;
                }

            }).join("");

            row.appendChild(cell);
        });
        board.appendChild(row);
    });
    //console.log("bookingCount:", bookingCount, ", bookingCountTemp:", bookingCountTemp);

    //on cell hover - tooltip ---- START --------------------
    // 🧠 Create a custom tooltip element (only once)
    if (!document.getElementById("customTooltip")) {
        const tooltip = document.createElement("div");
        tooltip.id = "customTooltip";
        document.body.appendChild(tooltip);
    }
    // Reference tooltip
    const tooltip = document.getElementById("customTooltip");

    let tooltipLocked = false;
    // 🪄 Attach hover listeners to badges
    document.addEventListener("mouseover", function (e) {
        const badge = e.target.closest(".badge[data-shift]");
        console.log("cell badge:", badge);
        if (!badge) return;

        //const isBlocked = badge.dataset.isblocked;
        const shiftname = badge.dataset.shiftname;
        const date_full = badge.dataset.date;

        const shiftcode = badge.dataset.shift;
        const bookingid = badge.dataset.bookingid;
        const eventName = badge.dataset.eventname || "";
        const clientName = badge.dataset.clientname || "";
        const clientContact = badge.dataset.clientcontact || "";
        const eventDateRange = badge.dataset.eventdaterange || "";

        const badgeText = badge.textContent.trim();
        const dateISO = badge.dataset.date_iso;
        const hallId = parseInt(badge.dataset.hallid);

        let isTentative = false;
        if (badge.classList.contains("bg-white")) {
            isTentative = true;
            //console.log("tentative");
        }

        let isBlocked = false;
        if (badge.classList.contains("bg-dark")) {
            isBlocked = true;
            //console.log("isBlocked");
        }

        // 🔎 Get all bookings for this hall & date
        const bookingsForDay = bookingData.filter(b => {

            if (b.BanquetId !== hallId)
                return false;

            const fromDate = new Date(b.FromDate);
            const tillDate = new Date(b.TillDate);
            const currentDate = new Date(dateISO);

            return currentDate >= fromDate && currentDate <= tillDate;
        });


        // ✅ If badge shows COUNT (numeric)
        if (!isNaN(badgeText) && badgeText !== "") {
            if (bookingsForDay.length === 0) return;

            // ✅ Sort bookings
            bookingsForDay.sort((a, b) => {

                const shiftOrder = {
                    FD: 1,
                    MO: 2,
                    AF: 3,
                    EV: 4,
                    ON: 5
                };

                const shiftDiff = (shiftOrder[a.ShiftCode] || 99) - (shiftOrder[b.ShiftCode] || 99);

                if (shiftDiff !== 0)
                    return shiftDiff;

                // ✅ SAME SHIFT → sort by START TIME
                const aStart = timeToMinutes(a.StartTime_Full);
                const bStart = timeToMinutes(b.StartTime_Full);

                return aStart - bStart;
            });            

            let html = "";
            bookingsForDay.forEach((b, index) => {

                html += `
                    <div style="padding-bottom:8px;">
                        <strong>
                            <i class="fa-solid fa-calendar-alt" style="color:#e11d48;"></i>
                            ${b.Booking_SubEventName} (${b.ShiftCode})
                            ${isTentative ? `<span style="margin-left:6px; font-size:11px; color:#be123c; background:#ffe4e6; padding:2px 6px; border-radius:6px;">Tentative</span>` : ``}
                        </strong><br>
                        <i class="fa-solid fa-user" style="color:#fb7185;"></i> ${b.Client_Name}<br>
                        <i class="fa-solid fa-phone" style="color:#fb7185;"></i> ${b.Client_Contact}<br>
                        <i class="fa-solid fa-clock" style="color:#fb7185;"></i>
                        ${b.FromDate_Short} ${b.StartTime_Full} - 
                        ${b.TillDate_Short} ${b.EndTime_Full}
                    </div>
                `;

                // Add separator except for last item
                if (index !== bookingsForDay.length - 1) {
                    html += `
                        <hr style="
                            border: 1px solid #fff;
                            margin: 6px 0;
                        ">
                    `;
                }
            });            

            tooltip.innerHTML = html;

            // 🌸 Light gradient background
            // 🎨 Default (Confirmed Booking)
            tooltip.style.background = "linear-gradient(135deg, #fff1f2, #ffe4e6)";
            tooltip.style.color = "#881337";
            tooltip.style.border = "1px solid #fecdd3";

            // 🔄 Tentative Style (subtle differentiation)
            if (isTentative) {
                tooltip.style.background = "#ffffff";
                tooltip.style.color = "#9f1239";
                tooltip.style.border = "1px dashed #fda4af";
            }

            // ✨ subtle border
            tooltip.style.border = "1px solid #fecdd3";
            // 🎯 better UI feel
            tooltip.style.borderRadius = "10px";
            tooltip.style.padding = "10px 12px";
            // 🌫️ soft shadow
            tooltip.style.boxShadow = "0 4px 12px rgba(225, 29, 72, 0.15)";
            // 📏 layout control
            tooltip.style.display = "block";
            tooltip.style.maxHeight = "500px";
            tooltip.style.overflowY = "auto";
            // 🎬 smooth appearance
            tooltip.style.transition = "opacity 0.15s ease";

            tooltipLocked = true;

            // Position near badge instead of cursor
            //If badge is in left 50 % of screen → show tooltip on right
            //If badge is in right 50 % of screen → show tooltip on left
            const rect = badge.getBoundingClientRect();
            const screenMid = window.innerWidth / 2;

            const tooltipWidth = tooltip.offsetWidth || 280;
            const tooltipHeight = tooltip.offsetHeight || 120;

            const gap = 12;

            let left;
            let top;

            // ✅ Use center instead of left edge
            const elementCenter = rect.left + rect.width / 2;

            // 👉 Decide LEFT / RIGHT
            if (elementCenter > screenMid) {
                left = rect.left - tooltipWidth - gap;
            } else {
                left = rect.right + gap;
            }

            // 👉 Vertical alignment
            top = rect.top;

            // ✅ Boundary checks
            if (left + tooltipWidth > window.innerWidth) {
                left = window.innerWidth - tooltipWidth - 10;
            }
            if (left < 0) {
                left = 10;
            }
            if (top + tooltipHeight > window.innerHeight) {
                top = window.innerHeight - tooltipHeight - 10;
            }
            if (top < 0) {
                top = 10;
            }

            // ✅ Apply final position (important for scroll)
            tooltip.style.left = `${left + window.scrollX}px`;
            tooltip.style.top = `${top + window.scrollY}px`;

            return;
        }

        // 🔴 Normal single booking tooltip
        //const bookingid = badge.dataset.bookingid;
        //console.log("isBlocked:", isBlocked, ", bookingid: ",bookingid);
        if (bookingid > 0) {

            const booking = bookingsForDay.find(b => b.BookingId == bookingid);
            if (!booking) return;

            tooltip.innerHTML = `
                <div style="font-weight:600; margin-bottom:6px;">
                    <i class="fa-solid fa-calendar-alt" style="color:#e11d48;"></i>
                    ${booking.Booking_SubEventName} (${booking.ShiftCode})
                    ${isTentative ? `<span style="margin-left:6px; font-size:11px; color:#be123c; background:#ffe4e6; padding:2px 6px; border-radius:6px;">Tentative</span>` : ``}
                </div>

                <div style="margin-bottom:4px;">
                    <i class="fa-solid fa-user" style="color:#fb7185;"></i>
                    ${booking.Client_Name}
                </div>

                <div style="margin-bottom:4px;">
                    <i class="fa-solid fa-phone" style="color:#fb7185;"></i>
                    ${booking.Client_Contact}
                </div>

                <div>
                    <i class="fa-solid fa-clock" style="color:#fb7185;"></i>
                    ${booking.FromDate_Short} ${booking.StartTime_Full} -
                    ${booking.TillDate_Short} ${booking.EndTime_Full}
                </div>
            `;

            // 🎨 Default (Confirmed Booking)
            tooltip.style.background = "linear-gradient(135deg, #fff1f2, #ffe4e6)";
            tooltip.style.color = "#881337";
            tooltip.style.border = "1px solid #fecdd3";

            // 🔄 Tentative Style (subtle differentiation)
            if (isTentative) {
                tooltip.style.background = "#ffffff";
                tooltip.style.color = "#9f1239";
                tooltip.style.border = "1px dashed #fda4af";
            }

            // ✨ Common UI styling
            tooltip.style.borderRadius = "10px";
            tooltip.style.padding = "10px 12px";
            tooltip.style.boxShadow = "0 4px 12px rgba(225, 29, 72, 0.15)";
            tooltip.style.display = "block";
            tooltip.style.maxHeight = "250px";
            tooltip.style.overflowY = "auto";
            tooltip.style.transition = "opacity 0.15s ease";

        }
        else {
            if (badge.classList.contains("bg-secondary") || isBlocked) {
                tooltip.style.display = "none";
                return; // stop tooltip processing
            }

            const shiftIcons = {
                FD: "fa-solid fa-sun",
                MO: "fa-solid fa-cloud-sun",
                AF: "fa-solid fa-sun",          // or fa-sun-haze if available
                EV: "fa-solid fa-cloud-moon",
                ON: "fa-solid fa-moon"
            };
            const iconClass = shiftIcons[shiftcode] || "fa-solid fa-clock"; // fallback
            tooltip.innerHTML = `
                <div>
                    <strong><i class="${iconClass}"></i> ${shiftname} (${shiftcode})</strong>
                </div>
                <div>
                    <i class="fa-solid fa-calendar"></i> ${date_full}
                </div>
            `;
            tooltip.style.background = "#22c55e"; // ✅ light green (Tailwind green-500)
            tooltip.style.color = "#fff";
            tooltip.style.display = "block";
        }
        
    });

    document.addEventListener("mousemove", function (e) {

        if (!tooltip || tooltip.style.display !== "block")
            return;

        // If locked (scrollable tooltip), don't follow cursor
        if (tooltipLocked)
            return;

        const tooltipWidth = tooltip.offsetWidth;
        const tooltipHeight = tooltip.offsetHeight;
        const offset = 15;

        let left = e.pageX + offset;
        let top = e.pageY + offset;

        if (left + tooltipWidth > window.innerWidth)
            left = e.pageX - tooltipWidth - offset;

        if (top + tooltipHeight > window.innerHeight)
            top = e.pageY - tooltipHeight - offset;

        tooltip.style.left = left + "px";
        tooltip.style.top = top + "px";
    });

    document.addEventListener("mouseout", function (e) {

        if (!tooltip) return;

        const related = e.relatedTarget;

        // If moving between badge and tooltip, don't hide
        if (related && (related.closest?.(".badge[data-shift]") || related.id === "customTooltip")) {
            return;
        }

        tooltip.style.display = "none";
        tooltip.style.transition = "opacity 0.15s ease";
        tooltipLocked = false;
    });
    //on cell hover - tooltip ---- END --------------------

    renderListViewTable();
    adjustFontSizeFordays(cacheddays.length);
    attachColumnHoverListeners();
    enableEnquiryTooltips();
    attachShiftClickListeners();
}
function timeToMinutes2(timeStr) {
    // Supports "07:00", "6:00AM", "02:00 PM"
    const d = new Date("1970-01-01 " + timeStr);
    return d.getHours() * 60 + d.getMinutes();
}
function timeToMinutes(timeStr) {

    if (!timeStr) return 0;

    timeStr = timeStr.trim().toUpperCase();

    // Handle 24-hour format (07:00 or 14:30)
    if (timeStr.includes(":") && !timeStr.includes("AM") && !timeStr.includes("PM")) {
        const [h, m] = timeStr.split(":");
        return parseInt(h, 10) * 60 + parseInt(m, 10);
    }

    // Handle 12-hour format (9:00PM or 9:00 PM)
    const match = timeStr.match(/(\d+):(\d+)\s*(AM|PM)/);

    if (!match) return 0;

    let hours = parseInt(match[1], 10);
    const minutes = parseInt(match[2], 10);
    const period = match[3];

    if (period === "PM" && hours !== 12) {
        hours += 12;
    }

    if (period === "AM" && hours === 12) {
        hours = 0;
    }

    return hours * 60 + minutes;
}
function isTimeOverlap(start1, end1, start2, end2) {
    return start1 < end2 && end1 > start2;
}

//enquiry count - tooltip
function enableEnquiryTooltips() {

    const tooltip = document.getElementById("enquiryTooltip");

    document.addEventListener("mouseover", function (e) {

        const badge = e.target.closest(".badge-enquiries");
        if (!badge) return;

        const list = badge.dataset.enquiryList
            ? JSON.parse(badge.dataset.enquiryList)
            : [];

        const count = parseInt(badge.innerText);

        if (count === 0 || list.length === 0) {
            tooltip.style.display = "none";
            return;
        }

        let totalAmount = 0;

        let html = `
            <table class="table table-sm table-bordered m-0">
                <thead>
                    <tr>
                        <th>Client</th>
                        <th class="text-end">Amount</th>
                    </tr>
                </thead>
                <tbody>
        `;

        list.forEach(x => {

            const amount = parseFloat(x.Amount) || 0;
            totalAmount += amount;

            html += `
                <tr>
                    <td>${x.Name}</td>
                    <td class="text-end">₹${amount.toLocaleString()}</td>
                </tr>
            `;
        });

        // ✅ TOTAL ROW (Bold)
        html += `
            <tr class="fw-bold">
                <td>Total</td>
                <td class="text-end">₹${totalAmount.toLocaleString()}</td>
            </tr>
        `;

        html += `</tbody></table>`;

        tooltip.innerHTML = html;
        tooltip.style.display = "block";

        //tooltip positions - START
        const rect = badge.getBoundingClientRect();
        const screenWidth = window.innerWidth;
        const tooltipWidth = tooltip.offsetWidth || 250; // fallback width

        const badgeCenter = rect.left + (rect.width / 2);

        // 👉 70% threshold
        const isRightZone = badgeCenter > (screenWidth * 0.7);

        let left;

        if (isRightZone) {
            // ✅ Show on LEFT side
            left = rect.left - tooltipWidth - 10;
        }
        else {
            // ✅ Show on RIGHT side
            left = rect.right + 10;
        }

        // Prevent overflow (extra safety)
        left = Math.max(10, Math.min(left, screenWidth - tooltipWidth - 10));

        tooltip.style.left = (left + window.scrollX) + "px";
        tooltip.style.top = (rect.top + window.scrollY) + "px";
        //tooltip positions - END
    });

    document.addEventListener("mouseout", function (e) {
        if (e.target.closest(".badge-enquiries")) {
            tooltip.style.display = "none";
        }
    });
}
//enquiry count - tooltip

function adjustFontSizeFordays(daysCount) {
    const scale = daysCount > 30 ? 0.65 : daysCount > 29 ? 0.7 : 0.75;
    document.querySelectorAll("#booking-board .day-cell").forEach(cell => {
        cell.style.fontSize = `${scale}rem`;
    });
}

function renderListViewTable(bookingData = null) {

    listTableBody.innerHTML = "";

    cacheddays.forEach((d, dayIndex) => {

        const yyyy = selectedYear;
        const mm = String(selectedMonth + 1).padStart(2, "0");
        const dd = String(d.date).padStart(2, "0");

        const dateStr_Display = `${dd}-${monthNames[selectedMonth]}-${yyyy}`;
        const dateStr_ISO = `${yyyy}-${mm}-${dd}`;
        const currentDate = new Date(dateStr_ISO);

        banquetHallsList.forEach(hall => {

            // Build shift badges for this hall + date
            const shiftBadges = cachedShiftData[hall.Id][dayIndex].map((color, i) => {

                let finalColor = color;
                let bookingId = 0;
                let eventName = "";
                let clientName = "";
                let clientContact = "";
                let eventDateRange = "";

                const shiftCode = shiftNames[i];
                const shiftFull = fullShiftName(shiftCode);

                // 🔍 Check bookingData (same logic as board)
                if (Array.isArray(bookingData)) {
                    for (const booking of bookingData) {
                        if (
                            booking.BanquetId === hall.Id &&
                            booking.ShiftCode === shiftCode
                        ) {
                            const fromDate = new Date(booking.FromDate);
                            const tillDate = new Date(booking.TillDate);

                            if (currentDate >= fromDate && currentDate <= tillDate) {
                                finalColor = "danger";
                                bookingId = booking.BookingId;
                                eventName = booking.Booking_SubEventName;
                                clientName = booking.Client_Name;
                                clientContact = booking.Client_Contact;
                                eventDateRange =
                                    booking.FromDate_Short + " " +
                                    booking.StartTime_Full + " - " +
                                    booking.TillDate_Short + " " +
                                    booking.EndTime_Full;
                                break;
                            }
                        }
                    }
                }

                const textColor = finalColor === "light" ? "text-black" : "text-white";

                return `
                        <span class="badge bg-${finalColor} ${textColor} me-1 mb-1"
                              data-bookingid="${bookingId}"
                              data-eventname="${eventName}"
                              data-clientname="${clientName}"
                              data-clientcontact="${clientContact}"
                              data-eventdaterange="${eventDateRange}"
                              data-shift="${shiftCode}"
                              data-shiftname="${shiftFull}"
                              data-date="${dateStr_Display}"
                              data-date_iso="${dateStr_ISO}"
                              data-hallid="${hall.Id}"
                              data-hallname="${hall.HallName}">
                            ${shiftCode}
                        </span>`;
            }).join("");

            const tr = document.createElement("tr");
            tr.innerHTML = `
                    <td>${dateStr_Display}</td>
                    <td>${d.day}</td>
                    <td class="text-center">
                        <span class="badge border border-dark text-black bg-white cursor-pointer">
                            ${d.enquiries}
                        </span>
                    </td>
                    <td>${hall.HallName}</td>
                    <td>
                        <div class="badge-container">
                            ${shiftBadges}
                        </div>
                    </td>
                `;

            listTableBody.appendChild(tr);
        });
    });
}

function attachColumnHoverListeners() {
    const rows = document.querySelectorAll(".row-wrapper");

    if (!rows.length) return;

    // Number of columns (days) - subtract 1 because first cell is name-cell
    const columnCount = rows[0].querySelectorAll(".day-cell").length;

    for (let colIndex = 0; colIndex < columnCount; colIndex++) {
        // Collect all cells in this column across all rows
        const columnCells = [];
        rows.forEach(row => {
            const dayCells = row.querySelectorAll(".day-cell");
            if (dayCells[colIndex]) columnCells.push(dayCells[colIndex]);
        });

        // Attach hover events to each cell in this column
        columnCells.forEach(cell => {
            cell.addEventListener("mouseenter", () => {
                columnCells.forEach(c => c.classList.add("highlight-column"));
            });
            cell.addEventListener("mouseleave", () => {
                columnCells.forEach(c => c.classList.remove("highlight-column"));
            });
        });
    }
}

// Toggle View
toggleViewBtn.addEventListener("click", () => {
    const isList = !calendarView.classList.contains("d-none");
    calendarView.classList.toggle("d-none", isList);
    listView.classList.toggle("d-none", !isList);
    toggleViewBtn.textContent = isList ? "Default View" : "List View";
});

//<!-- Booking Modal -- START ------------------------------------------------>
const bookingModalEl = document.getElementById("bookingModal");
const bookingModal = new bootstrap.Modal(bookingModalEl);

const bookingReportModalEl = document.getElementById("bookingReportModal");
const bookingReportModal = new bootstrap.Modal(bookingReportModalEl);
const bookingReportModal_inDOM = document.getElementById('bookingReportModal');

const invoiceModal = new bootstrap.Modal(document.getElementById('invoiceModal'));

function closeBookingReportModal() {
    //console.log("booking info modal closed");
    bookingReportModal.hide();

    bookingReportModal_inDOM.classList.remove("show");
    bookingReportModal_inDOM.style.display = "none";

    // Show backdrop again
    const backdrop = document.querySelector('.modal-backdrop');
    if (backdrop) backdrop.style.display = "none";

    //    if (isBookingInfoModal_OpenFromBookingList) {
//        isBookingInfoModal_OpenFromBookingList = false;
//        $("#bookingListModal").modal("show");
//    }
}
function openBookingReportModal() {
    const bookingReportModal_local = document.getElementById('bookingReportModal');
    bookingReportModal_local.style.display = "block";
    bookingReportModal_local.classList.add("show");
    bookingReportModal_local.removeAttribute("aria-hidden");

    document.body.classList.add("modal-open");
}


document.getElementById("bookmapBtn").addEventListener("click", () => {
    location.reload();
    //console.log("add new booking");
    //bookingModal.show();
});

var isBookingInfoModal_OpenFromBookingList = false;
//shift/cell click event
function attachShiftClickListeners() {
    document.querySelectorAll("#booking-board .badge, #listTable .badge").forEach(badge => {
        // Skip click event for enquiry badges (tooltip-only)
        if (badge.classList.contains("badge-enquiries")) {
            // Ensure cursor style stays pointer
            badge.style.cursor = "pointer";
            return; // skip attaching click event
        }

        const badgeText = badge.textContent.trim();
        // If badge text is numeric and greater than 1 → disable click
        //if (!isNaN(badgeText) && parseInt(badgeText) > 1) {
        //    badge.style.cursor = "pointer";
        //    //console.log("booking data:", global_bookingData);
        //    return; // do not attach click
        //}

        badge.style.cursor = "pointer";
        badge.addEventListener("click", () => {
            const color = badge.className.match(/bg-([a-z]+)/)?.[1];
            if (!color) return;

            const bookingid = badge.dataset.bookingid;
            const shift = badge.dataset.shift;
            const shiftname = badge.dataset.shiftname;
            const date = badge.dataset.date;
            const dateformat = badge.dataset.date_format;
            const dateiso = badge.dataset.date_iso;
            const hallname = badge.dataset.hallname;
            const hallid = badge.dataset.hallid;

            // ===============================
            // 🎯 HANDLE FD COUNT CLICK
            // ===============================
            if (!isNaN(badgeText) && badgeText !== "") {

                const count = parseInt(badgeText);

                const normalize = d => new Date(d).setHours(0, 0, 0, 0);
                const matchedBookings = global_bookingData.filter(item =>
                    Number(item.BanquetId) === Number(hallid) &&
                    normalize(item.EventDate) === normalize(dateiso)
                );

                // ✅ CASE 1: Only ONE booking → open directly
                if (count === 1 && matchedBookings.length === 1) {

                    const b = matchedBookings[0];

                    resetBooking();

                    $("#BookingId").val(b.BookingId);

                    $("#label_date_shift")
                        .text(`${date}, ${fullShiftName(b.ShiftCode)} Slot`)
                        .data("eventdate", dateiso)
                        .data("shiftid", b.ShiftCode);

                    $("#banquetDropdown").val(hallid);

                    setShiftTime(b.ShiftCode);

                    $("#fromDate").val(dateiso).prop("disabled", true);
                    $("#toDate").val(dateiso);

                    loadBookingCalendar(b.BookingId, 0, null, "singleData");

                    $("#title_bookingModal").text("Edit Booking Details");

                    return;
                }

                // ✅ CASE 2: Multiple bookings → show list modal
                if (count > 1 && matchedBookings.length > 0) {

                    let html = `
                        <table class="table table-bordered table-hover">
                            <thead class="table-light bg-danger">
                                <tr>
                                    <th>#</th>
                                    <th>Event</th>
                                    <th>Shift</th>
                                    <th>Client</th>
                                    <th>Contact</th>
                                    <th>Date</th>
                                    <th>Time</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                            <tbody>
                    `;

                    matchedBookings.forEach((b, index) => {
                        html += `
                            <tr>
                                <td class="text-center">${index + 1}</td>
                                <td>${b.Booking_SubEventName || ""}</td>
                                <td>${b.ShiftCode}</td>
                                <td>${b.Client_Name}</td>
                                <td>${b.Client_Contact}</td>
                                <td>
                                    ${b.FromDate_Short === b.TillDate_Short
                                            ? b.FromDate_Short
                                            : `${b.FromDate_Short} - ${b.TillDate_Short}`}
                                </td>
                                <td>${b.StartTime_Full} - ${b.EndTime_Full}</td>
                                <td>
                                    <button 
                                        class="btn btn-sm btn-danger view-booking-btn"
                                        data-bookingid="${b.BookingId}">
                                        View
                                    </button>
                                </td>
                            </tr>
                        `;
                    });

                    html += `</tbody></table>`;

                    $("#bookingListModalBody").html(html);
                    $("#bookingListModal").modal("show");

                    return;
                }
            }

            //console.log("color cell:", color);
            //Present Day & Future days
            //if (color !== legendColors.past || (color === legendColors.past && bookingid > 0)) {
            if (color !== legendColors.blocked &&
                (
                    color !== legendColors.past ||
                    //color === legendColors.present ||
                    //color === legendColors.future ||
                    (color === legendColors.past && bookingid > 0)
                )
            ) {

                resetBooking();
                //console.log(`Edit Booking → Shift: ${shift} (${shiftname}) | Date: ${date} (format: ${dateformat}, iso: ${dateiso}) | Hall: ${hallname} (${hallid}) | BookingId: ${bookingid}`);
                //console.log("event dateiso:", dateiso);
                $("#BookingId").val(bookingid);
                // 🟢 Update label content & attributes - using jQuery
                $("#label_date_shift")
                    .text(`${date}, ${shiftname} Slot`)
                    .data("eventdate", dateiso)
                    .data("shiftid", shift);

                $("#banquetDropdown").val(hallid);
                setShiftTime(shift);
                $("#PaymentDetails_DueDate").val('');

                $("#fromDate").val(dateiso).prop("disabled", true);
                $("#toDate").val(dateiso);

                if (bookingid > 0) {
                    var banquetId = 0;
                    var eventDate = null;
                    var actionMode = "singleData";
                    loadBookingCalendar(bookingid, banquetId, eventDate, actionMode);
                    $("#title_bookingModal").text("Edit Booking Details"); 
                }
                else {
                    //Open MODAL
                    $("#title_bookingModal").text("Add Booking Details");
                    //bookingModal.show();
                    const bookingModal2 = new bootstrap.Modal(document.getElementById('bookingModal'));
                    bookingModal2.show();
                    $("#fromDate, #startHour, #startMinute, #startAMPM, #endHour, #endMinute, #endAMPM, #banquetDropdown").trigger("change");
                }
            }
            //For Past days
        });
    });
}

function setShiftTime(shiftAbbr) {
    //console.log("shiftTimes:", shiftTimes);
    const times = shiftTimes[shiftAbbr];
    if (!times) return; // invalid shift — do nothing

    // Parse start time
    const [startH, startM, startAMPM] = times.start.match(/(\d+):(\d+)\s*(AM|PM)/).slice(1);
    $("#startHour").val(startH);
    $("#startMinute").val(startM);
    $("#startAMPM").val(startAMPM);

    // Parse end time
    const [endH, endM, endAMPM] = times.end.match(/(\d+):(\d+)\s*(AM|PM)/).slice(1);
    $("#endHour").val(endH);
    $("#endMinute").val(endM);
    $("#endAMPM").val(endAMPM);
}

// Call this function after rendering board or table
/* here1
renderBoard = (() => {
    const originalRenderBoard = renderBoard;
    return function (...args) {
        originalRenderBoard(...args);
        //attachShiftClickListeners();
    }
})();
*/
//<!-- Booking Modal -- /END ------------------------------------------------>

// Global variables
var decorationAmount = 0;
var decorationGST = 0;
var roomchargesAmount = 0;
var roomchargesGST = 0;
var soundSystemAmount = 0;
var soundSystemGST = 0;
var projectScreenAmount = 0;
var projectScreenGST = 0;

var totalGST = 0;
var cgst = 0;
var sgst = 0;
var igst = 0;
var total = 0;
var bookingDiscount = 0;
var payableAmount = 0;

// GST Formula (18%)
function getGST(amount) {
    return amount * 0.18;
}

// Recalculate GST when Hall Rent changes or GST switch toggles
function updateHallRentGST() {

    let hallRent = parseFloat($("#booking_hallRent").val()) || 0;
    let isGSTEnabled = $("#gstSwitch").is(":checked");

    let gst = isGSTEnabled ? getGST(hallRent) : 0;

    $("#booking_gst").val(gst.toFixed(2));

    recalculatePaymentSummary();   // 🔥 IMPORTANT
}

var vendorWorkOrder_eventDate = 0;
var vendorWorkOrder_eventTime = 0;
var vendorWorkOrder_slotId = 0;
let hasInvoice = false;
let openedFromReport = false;
// Initial Load
document.addEventListener("DOMContentLoaded", function () {
    $("#btnBook").prop("disabled", true);

    //const bookingId = getBookingId();

    populateYearDropdown();
    //populateMonthButtons();
    //loadBookingCalendar();
    //loadPaymentHistory(bookingId);
    loadLeadSourceList();
    loadEnquiryList();
    loadStateList();
    loadEventTypeList();
    loadSubEventList();
    loadFoodPackageList();

    getNextInvoiceNumbers();

    AppSignalR.subscribeSettings((data) => {
        //console.log("Settings Page Update", data);
        //window.location.reload();
        //console.log("page refresh");
        //loadBookingCalendar();
        populateYearDropdown();
    });

    // When Addon Modal is about to open
    const addonModal = document.getElementById('addonChargeModal');
    const bookingModal_inDOM = document.getElementById('bookingModal');
    const menuModal = document.getElementById('menuModal');
    //const bookingReportModal_inDOM = document.getElementById('bookingReportModal');
    const paymentModal = document.getElementById('paymentModal');
    const bookingRescheduleModal = document.getElementById('bookingRescheduleModal');
    const foodwestageModal = document.getElementById('foodWastageModal');
    const vendorWorkOrderModal = document.getElementById('vendorWorkOrderModal');

    //hide booking report modal --- START ----------------
    // ESC key handler
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            //console.log("Escape Pressed");
            //console.log("openedFromReport:", openedFromReport);
            if (bookingReportModal_inDOM.classList.contains("show")) {
                //console.log("closeBookingReportModal");
                closeBookingReportModal();
            }
        }
    });
    //hide booking report modal --- END ----------------

    // Create booking modal instance manually
    //const bookingModalInstance = new bootstrap.Modal(bookingModal);

    addonModal.addEventListener('show.bs.modal', function () {
        // Just hide visually (do not destroy modal state)
        bookingModal_inDOM.classList.remove("show");
        bookingModal_inDOM.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    addonModal.addEventListener('hidden.bs.modal', function () {
        // Show booking modal visually again (no reset, same position)
        bookingModal_inDOM.classList.add("show");
        bookingModal_inDOM.style.display = "block";

        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });

    // Create payment modal instance manually
    //const paymentModalInstance = new bootstrap.Modal(paymentModal);

    //hide/show PAYMENT MODAL ----- START --------------------------
    paymentModal.addEventListener('show.bs.modal', function () {
        //console.log("paymentModal SHOW");
        // Just hide visually (do not destroy modal state)
        bookingReportModal_inDOM.classList.remove("show");
        bookingReportModal_inDOM.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    paymentModal.addEventListener('hidden.bs.modal', function () {
        //console.log("paymentModal HIDE");
        // Show booking modal visually again (no reset, same position)
        bookingReportModal_inDOM.classList.add("show");
        bookingReportModal_inDOM.style.display = "block";

        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });
    //hide/show PAYMENT MODAL ----- END --------------------------

    //hide/show FoodWastage MODAL ----- START --------------------------
    foodwestageModal.addEventListener('show.bs.modal', function () {
        //console.log("foodWastageModal SHOW");
        // Just hide visually (do not destroy modal state)
        bookingReportModal_inDOM.classList.remove("show");
        bookingReportModal_inDOM.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    foodwestageModal.addEventListener('hidden.bs.modal', function () {
        //console.log("foodWastageModal HIDE");
        // Show booking modal visually again (no reset, same position)
        bookingReportModal_inDOM.classList.add("show");
        bookingReportModal_inDOM.style.display = "block";

        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });
    //hide/show FoodWastage MODAL ----- END --------------------------

    //hide BOOKING REPORT MODAL ----- START --------------------------
    bookingReportModal_inDOM.addEventListener('hidden.bs.modal', function () {
        //console.log("bookingReportModal HIDE");
        // Show booking modal visually again (no reset, same position)
        //bookingReportModal_inDOM.classList.add("show");
        //bookingReportModal_inDOM.style.display = "block";

        if (isBookingInfoModal_OpenFromBookingList) {
            isBookingInfoModal_OpenFromBookingList = false;
            $("#bookingListModal").modal("show");
        }

        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });
    //hide BOOKING REPORT MODAL ----- END --------------------------

    //hide/show vendorWorkOrder MODAL ----- START --------------------------
    vendorWorkOrderModal.addEventListener('show.bs.modal', function () {
        //console.log("vendorWorkOrderModal shown");
        // Just hide visually (do not destroy modal state)
        bookingReportModal_inDOM.classList.remove("show");
        bookingReportModal_inDOM.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    vendorWorkOrderModal.addEventListener('hidden.bs.modal', function () {
        // Show booking modal visually again (no reset, same position)
        bookingReportModal_inDOM.classList.add("show");
        bookingReportModal_inDOM.style.display = "block";

        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });
    //hide/show PAYMENT MODAL ----- END --------------------------

    //hide/show bookingModal MODAL ----- START --------------------------
    $("#bookingModal").on("hidden.bs.modal", function () {
        if (openedFromReport) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal_inDOM.classList.add("show");
            bookingReportModal_inDOM.style.display = "block";
        }
    });
    //hide/show bookingModal MODAL ----- END--------------------------

    //hide bookingRescheduleModal MODAL ----- START --------------------------
    $("#bookingRescheduleModal").on("hidden.bs.modal", function () {
        if (openedFromReport) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal_inDOM.classList.add("show");
            bookingReportModal_inDOM.style.display = "block";
        }
    });
    //hide bookingRescheduleModal MODAL ----- END--------------------------

    //hide prepareOrderModal MODAL ----- START --------------------------
    $("#prepareOrderModal").on("hidden.bs.modal", function () {
        //console.log("openedFromReport:", openedFromReport);
        if (openedFromReport & !openedFromPrepareOrder) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal_inDOM.classList.add("show");
            bookingReportModal_inDOM.style.display = "block";
        }
    });
    //hide prepareOrderModal MODAL ----- END--------------------------

    //hide Formula MODAL ----- START --------------------------
    $("#formulaModal").on("hidden.bs.modal", function () {
        if (openedFromPrepareOrder) {
            openedFromPrepareOrder = false;
            setTimeout(() => {
                $("#prepareOrderModal").modal("show");
            }, 200);
        }
    });
    //hide Formula MODAL ----- END--------------------------

    //hide feedback MODAL ----- START --------------------------
    $("#feedbackModal").on("hidden.bs.modal", function () {
        //console.log("openedFromPrepareOrder feedback:", openedFromReport);
        if (openedFromReport) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal_inDOM.classList.add("show");
            bookingReportModal_inDOM.style.display = "block";
        }
    });
    //hide feedback MODAL ----- END--------------------------

    //hide cancelBookingModal MODAL ----- START --------------------------
    $("#cancelBookingModal").on("hidden.bs.modal", function () {
        //console.log("openedFromReport:", openedFromReport);
        if (openedFromReport) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal_inDOM.classList.add("show");
            bookingReportModal_inDOM.style.display = "block";
        }
    });
    //hide cancelBookingModal MODAL ----- END--------------------------

    //hide invoice MODAL ----- START --------------------------
    $("#invoiceModal").on("hidden.bs.modal", function () {
        //console.log("openedFromReport:", openedFromReport);
        if (openedFromReport) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal_inDOM.classList.add("show");
            bookingReportModal_inDOM.style.display = "block";
        }
    });
    //hide invoice MODAL ----- END--------------------------

    //hide/show MENU MODAL ----- START --------------------------
    menuModal.addEventListener('show.bs.modal', function () {
        // Just hide visually (do not destroy modal state)
        bookingModal_inDOM.classList.remove("show");
        bookingModal_inDOM.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    menuModal.addEventListener('hidden.bs.modal', function () {
        bookingModal_inDOM.classList.add("show");
        bookingModal_inDOM.style.display = "block";
        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });
    //hide/show MENU MODAL ----- END --------------------------

    //Menu Item - START ----- START ---------------------------
    $('#menuModal').on('click', '.btn-success', function () {
        const container = $(this).closest('.border');
        const input = $(this).siblings('input[type="text"]');
        const newItemName = input.val().trim();
        if (newItemName === '') return;

        const itemsContainer = container.find('.items-container');
        const newId = newItemName.replace(/\s+/g, '_') + '_' + Date.now();

        const newItemHtml = `
                <div class="new-item">
                    <input class="form-check-input me-1" type="checkbox" value="${newItemName}" id="${newId}">
                    <label class="form-check-label" for="${newId}">${newItemName}</label>
                    <button type="button" class="btn btn-sm btn-outline-danger delete-item">✖</button>
                </div>
            `;

        itemsContainer.append(newItemHtml);
        input.val(''); // clear input
    });

    // Delete new item
    $('#menuModal').on('click', '.delete-item', function () {
        $(this).closest('.new-item').remove();
    });

    // Select All toggle
    $('#menuModal').on('change', 'input[id^="selectAll"]', function () {
        const isChecked = $(this).is(':checked');
        const container = $(this).closest('.border');
        container.find('.items-container input.form-check-input').prop('checked', isChecked);
    });

    // Update Select All if manual changes
    $('#menuModal').on('change', '.items-container input.form-check-input', function () {
        const container = $(this).closest('.border');
        const all = container.find('.items-container input.form-check-input');
        const selectAllBox = container.find('input[id^="selectAll"]');
        selectAllBox.prop('checked', all.length === all.filter(':checked').length);
    });

    // When Save Selection button is clicked
    /*
    $("#menuModal .btn-saveMenu").on("click", function () {
        let menuData = [];

        // Loop through each header section
        $("#menuModal .border.rounded").each(function () {
            let section = $(this);
            let headerTitle = section.find("h6").text().trim();
            let selectedItems = [];

            // Collect checked items under this header
            section.find(".items-container input[type='checkbox']:checked").each(function () {
                selectedItems.push($(this).val());
            });

            // Push only if items are selected
            if (selectedItems.length > 0) {
                menuData.push({
                    header: headerTitle,
                    items: selectedItems
                });
            }
        });

        // Convert to JSON string
        let jsonString = JSON.stringify(menuData);

        // Save JSON string in dropdown (data attribute)
        $("#foodPackage_PackageDropdown").data("menuitems", jsonString);

        // Optionally show summary text in dropdown (or console)
        //console.log("Saved Menu JSON:", jsonString);

        // Close modal
        $("#menuModal").modal("hide");
    });
    */
    //Menu Item - START ----- END ---------------------------

    // 1️⃣ Reset to 0
    //$(".btn-outline-info").on("click", function () {
        // Find all number inputs in the parent container (Other Charges) and set value to 0
        //$(this).closest(".col-md-7").find("input[type='number']").val(0);
    //});

    // 2️⃣ Restore Defaults
    //$(".btn-outline-secondary").on("click", function () {
        //console.log("reset default triggered");
        // You can later add logic to restore predefined defaults if needed
        //const isChecked = $("#gstSwitch").is(":checked");
        //resetOtherCharges(isChecked);
        //recalculatePaymentSummary();
    //});

    // Using jQuery
    $("#gstSwitch").on("change", function () {
        const isChecked = $(this).is(":checked");
        //console.log(isChecked ? "GST Accepted" : "GST Not Accepted");
        updatePackageCost();
        // Recalculate dynamic rows
        $(".food-block").each(function () {
            calculateFoodRowTotals($(this));
        });
        //resetOtherCharges(isChecked);

        updateHallRentGST();

        // Recalculate all Other Charges GST
        recalculateAllOtherCharges();

        recalculatePaymentSummary();

        if (isChecked) {
            $("#gstSwitchText").text("On").removeClass("text-danger").addClass("text-success");  // Green for ON
        } else {
            $("#gstSwitchText").text("Off").removeClass("text-success").addClass("text-danger");  // Red for OFF
        }
    });

    // When Hall Rent value is typed or changed
    $("#booking_hallRent").on("input", function () {
        updateHallRentGST();
    });

    //discount change
    $("#paymentSummary_BookingDiscount_Type").on("change", function () {
        $("#paymentSummary_BookingDiscount_Amount").val(0);
        //updateBookingDiscount();
        recalculatePaymentSummary();
    });

    $("#paymentSummary_BookingDiscount_Amount").on("input", function () {
        //updateBookingDiscount();
        recalculatePaymentSummary();

    });

    $(document).on("change", "#PaymentDetails_Mode", function () {
        let selectedText = $("#PaymentDetails_Mode option:selected").text().toLowerCase();

        // Hide everything first
        $("#PaymentDetails_TransactionId").closest(".col-md-6").addClass("d-none");
        $("#PaymentDetails_ChequeBank").closest(".col-md-6").addClass("d-none");
        $("#PaymentDetails_ChequeNo").closest(".col-md-6").addClass("d-none");
        $("#PaymentDetails_ChequeDate").closest(".col-md-6").addClass("d-none");

        // CASH
        if (selectedText.includes("cash")) {
            // Nothing to show
        }

        // BANK TRANSFER
        else if (selectedText.includes("bank transfer")) {
            // Nothing to show
        }

        // CHEQUE
        else if (selectedText.includes("cheque")) {
            $("#PaymentDetails_ChequeBank").closest(".col-md-6").removeClass("d-none");
            $("#PaymentDetails_ChequeNo").closest(".col-md-6").removeClass("d-none");
            $("#PaymentDetails_ChequeDate").closest(".col-md-6").removeClass("d-none");
        }

        // UPI OR CREDIT
        else if (selectedText.includes("upi") || selectedText.includes("credit")) {
            $("#PaymentDetails_TransactionId").closest(".col-md-6").removeClass("d-none");
        }
    });

    $(document).on("change", "#PaymentModal_Mode", function () {
        //console.log("PaymentModal_Mode called");
        let selectedText = $("#PaymentModal_Mode option:selected").text().toLowerCase();
        //console.log("selectedText:", selectedText);

        // Hide all first
        $(".paymentModal_Fields").addClass("d-none");

        // CASH
        if (selectedText.includes("cash")) {
            // Nothing to show
        }

        // BANK TRANSFER
        else if (selectedText.includes("bank transfer")) {
            // Nothing to show
        }

        // CHEQUE
        else if (selectedText.includes("cheque")) {
            $("#paymentChequebank").closest(".paymentModal_Fields").removeClass("d-none");
            $("#paymentChequeNo").closest(".paymentModal_Fields").removeClass("d-none");
            $("#paymentChequeDate").closest(".paymentModal_Fields").removeClass("d-none");
        }

        // UPI OR CREDIT
        else if (selectedText.includes("upi") || selectedText.includes("credit")) {
            $("#paymentTxn").closest(".paymentModal_Fields").removeClass("d-none");
        }
    });

    //invoicePayment_Mode - change
    $(document).on("change", "#invoicePayment_Mode", function () {

        //console.log("invoicePayment_Mode called");

        let selectedText = $("#invoicePayment_Mode option:selected")
            .text()
            .toLowerCase();

        //console.log("selectedText:", selectedText);

        // 🔹 Hide all invoice dynamic fields first
        $(".invoicePayment_Fields").addClass("d-none");
        $(".invoicePayment_Fields input").val("");
        // CASH
        if (selectedText.includes("cash")) {
            // Nothing to show
        }

        // BANK TRANSFER
        else if (selectedText.includes("bank transfer")) {
            // Nothing to show
        }

        // CHEQUE
        else if (selectedText.includes("cheque")) {

            $("#invoicePayment_ChequeBank")
                .closest(".invoicePayment_Fields")
                .removeClass("d-none");

            $("#invoicePayment_ChequeNo")
                .closest(".invoicePayment_Fields")
                .removeClass("d-none");

            $("#invoicePayment_ChequeDate")
                .closest(".invoicePayment_Fields")
                .removeClass("d-none");
        }

        // UPI OR CREDIT
        else if (selectedText.includes("upi") || selectedText.includes("credit")) {

            $("#invoicePayment_Txn")
                .closest(".invoicePayment_Fields")
                .removeClass("d-none");
        }

    });

    //invoicePayment_Amount - Input
    $(document).on("input", "#invoicePayment_Amount", function () {
        const originalDue = Number($("#invoicePayment_originalDueAmount").val()) || 0;
        let payAmount = Number($(this).val()) || 0;

        // 🔒 Prevent overpayment
        if (payAmount > originalDue) {
            //payAmount = originalDue;
            $(this).val(originalDue);
        }
        const remaining = originalDue - payAmount;
        $("#invoicePayment_DueBalance, #balanceAmount_InvoiceModal").val(remaining);
    });

    //invoicePayment_btnWriteNote - click
    $(document).on("click", "#invoicePayment_btnWriteNote", function () {
        const editor = $(".invoicePayment_EditorContainer");
        editor.toggleClass("d-none");
        if (editor.hasClass("d-none")) {
            $(this).val("Payment Notes");
        }
        else {
            $(this).val("Hide Notes");
        }

    });


    //SAVE BOOKING ------ START ------------------------------
    $("#btnBook").on("click", function () { saveBooking(); });
    $("#btnReset").on("click", function () { resetBooking(); });
    //SAVE BOOKING ------ END --------------------------------

    //BOOKING REPORT MODAL ---- START ------------------------------------------------
    //const invoiceModal = new bootstrap.Modal(document.getElementById('invoiceModal'));


    // 🧾 Invoice Generate
    const btnInvoice = document.getElementById("btnInvoiceGenerate");
    const menu = document.getElementById("invoiceOptionsMenu");
    btnInvoice.addEventListener("click", function (e) {
        const bookingId = getBookingId();
        const btn_invoiceNo = getInvoiceNoFromButton("btnInvoiceGenerate");

        // ================= CASE 1: EXISTING INVOICE =================
        if (btn_invoiceNo > 0) {
            const userId = parseInt(sessionStorage.getItem('userId') || 0);

            const request = {
                BookingId: bookingId,
                InvoiceNo: btn_invoiceNo,
                ProformaInvoiceNo: null,
                WithHeader: true,
                ActionBy: userId
            };
            //ShowHide_DeleteButton();
            generateInvoicePdf(request);
            return;
        }

        // ================= CASE 2: NO INVOICE → SHOW MENU =================
        if (!hasInvoice) {
            menu.classList.toggle("d-none");

            const rect = btnInvoice.getBoundingClientRect();
            menu.style.top = (rect.bottom - 20) + "px";
            menu.style.left = (rect.left - 200) + "px";

            return; // 🚨 IMPORTANT (stop further execution)
        }

        // ================= CASE 3: FALLBACK =================
        openInvoiceModal(bookingId);
    });
    document.querySelectorAll("#invoiceOptionsMenu button").forEach(btn => {
        btn.addEventListener("click", function () {

            const type = this.getAttribute("data-type");
            const bookingId = getBookingId();

            //console.log("Invoice Type Selected:", type);

            menu.classList.add("d-none");
            // Pass flag
            openInvoiceModal(bookingId, type);
        });
    });
    document.addEventListener("click", function (e) {
        if (!btnInvoice.contains(e.target) && !menu.contains(e.target)) {
            menu.classList.add("d-none");
        }
    });
    function openInvoiceModal(bookingId, type = null) {
        //openedFromReport = true;

        //bookingReportModal_inDOM.classList.remove("show");
        //bookingReportModal_inDOM.style.display = "none";

        resetForm_InvoiceModal();
        $('#invoiceModal_bookingId').val(bookingId);

        invoiceType = "invoice";

        let isInclusiveGST = (type == "inclusive") ? true : false; 
        fixedSubTotal = isInclusiveGST ? Number($('#bookingReport_TotalAmount').text()) : 0;
        //const gstLabel = isInclusiveGST ? 'Rates Inclusive GST' : 'Rates Exclusive GST';
        //$('#invoiceModal_ModalTitle').text(`Create Invoice #${invoiceNo} (${gstLabel})`);
        $('#invoiceModal_invoiceNo').data('isinclusivegst', isInclusiveGST);
        fetchInvoiceByBookingId(bookingId, isInclusiveGST, false);
    }

    // 🧾 Proforma Invoice Generate
    document.getElementById("btnProformaInvoiceGenerate").addEventListener("click", function () {
        const bookingId = getBookingId();
        //console.log("📄 Proforma Invoice Generate clicked - Booking ID:", bookingId);

        const proformaNo = getInvoiceNoFromButton("btnProformaInvoiceGenerate");
        //console.log("Proforma Invoice No:", proformaNo);

        //download invoice
        if (proformaNo > 0) {
            const request = {
                BookingId: bookingId,
                InvoiceNo: null,
                ProformaInvoiceNo: proformaNo,
                WithHeader: true
            };
            generateInvoicePdf(request);
            return;
        }

        //openedFromReport = true;
        // Just hide visually (do not destroy modal state)
        //bookingReportModal_inDOM.classList.remove("show");
        //bookingReportModal_inDOM.style.display = "none";

        resetForm_InvoiceModal();
        $('#invoiceModal_bookingId').val(bookingId);

        invoiceType = "proforma";

        let isInclusiveGST = false;
        $('#invoiceModal_ModalTitle').text('Create Proforma Invoice #' + proformaInvoiceNo);
        //invoiceModal.show();
        fetchInvoiceByBookingId(bookingId, isInclusiveGST, true);
    });

    function getInvoiceNoFromButton(buttonId) {
        const text = document
            .querySelector(`#${buttonId} .btn-text`)
            ?.textContent
            ?.trim();

        // Expected formats:
        // "Invoice #123"
        // "Proforma #456"
        if (!text) return 0;

        const match = text.match(/#(\d+)/);
        return match ? parseInt(match[1], 10) : 0;
    }

    // 🖨️ Print Booking Receipt
    document.getElementById("btnBookingReceipt").addEventListener("click", function () {
        const bookingId = getBookingId();
        const userId = parseInt(sessionStorage.getItem('userId') || 0);
        var payload = {
            BookingId: bookingId,
            PaymentId: 0,
            ActionBy: userId
        };
        //console.log("payload EventForm:", payload);
        
        var url = "/api/Booking_Payments/DownloadPaymentReceipt";
        // ✅ Call download only if YES
        downloadReport(url, payload, "Generating Receipt...");
    });

    // 🧾 Download Function Prospect
    document.getElementById("btnFunctionProspect").addEventListener("click", function () {
        showReportOptions();
    });

    // 🖨️ Print Function Prospect
    document.getElementById("btnEventForm").addEventListener("click", function () {
        const bookingId = getBookingId();
        const userId = parseInt(sessionStorage.getItem('userId') || 0);
        var payload = {
            BookingId: bookingId,
            ActionBy: userId
        };
        //console.log("payload EventForm:", payload);
        var url = "/api/Report/Report_BookingEventForm";
        downloadReport(url, payload, "Generating Report...");
    });

    // Open foodwestage modal safely
    document.getElementById('btnFoodWestage').addEventListener('click', function () {
        const bookingId = (typeof getBookingId === 'function') ? getBookingId() : null;
        //console.log("💳 Enter Food Westage clicked - Booking ID:", bookingId);
        $("#tblFoodWastage tbody").empty();
        $("#txtTotalWastage").val("0.00");

        $.ajax({
            url: "/api/FoodWastage/GetFoodWastageByBookingId",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ bookingId: bookingId }),
            success: function (res) {
                if (res.status !== "Success") {
                    Swal.fire({
                        icon: 'info',
                        title: 'Failed',
                        text: res.message || 'Failed to get FoodWastage',
                        confirmButtonText: 'OK'
                    });
                    return;
                }
                let total = 0;
                res.data.forEach((item, index) => {
                    let wastage = parseFloat(item.WastageKg || 0);
                    total += wastage;
                    let row = `
                        <tr>
                            <td class="ps-2">${index + 1}</td>
                            <td>
                                ${item.MenuItem_Name}
                                <input type="hidden" class="menuItemId" value="${item.MenuItem_Id}">
                            </td>
                            <td>
                                <input type="number"
                                       class="form-control text-end wastageInput"
                                       value="${wastage}"
                                       min="0"
                                       step="1">
                            </td>
                        </tr>
                    `;

                    $("#tblFoodWastage tbody").append(row);
                });
                $("#txtTotalWastage").val(total.toFixed(2));
                $("#foodWastageModal").modal("show");
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, status, error, "Failed to Load Food Wastage");
            }
        });

    });

    // Open payment modal safely
    document.getElementById('btnEnterPayment').addEventListener('click', function () {
        const bookingId = (typeof getBookingId === 'function') ? getBookingId() : null;
        //console.log("💳 Enter Payment Details clicked - Booking ID:", bookingId);
        resetPaymentModal();

        $("#paymentModal").modal("show");

        const dueText = $("#bookingReport_OutstandingAmount").text();
        // Remove ₹ and commas
        let cleaned = dueText.replace(/[₹,\s]/g, '');
        let dueAmount = Number(cleaned) || 0;
        if (dueAmount === 0) {
            $("#dueMessage").removeClass("d-none");
            $("#paymentAmount").prop("disabled", true);
        }
        else {
            $("#dueMessage").addClass("d-none");
            $("#paymentAmount").prop("disabled", false);
        }
        $("#paymentDueBalance, #originalDueAmount").val(dueAmount);

        setCurrentPaymentDateTime();
        loadPaymentHistory(bookingId);
        //paymentModalInstance.show();
    });
    $("#paymentAmount").on("input", function () {
        const originalDue = Number($("#originalDueAmount").val()) || 0;
        let payAmount = Number($(this).val()) || 0;

        //if (payAmount > originalDue) {
        //    payAmount = originalDue;
        //    $(this).val(originalDue);
        //}

        const remaining = originalDue - payAmount;
        $("#paymentDueBalance").val(remaining);
    });

    // 📢 Request Feedback
    document.getElementById("btnRequestFeedback").addEventListener("click", function () {
        const bookingId = getBookingId();
        //console.log("📢 Request Feedback clicked - Booking ID:", bookingId);

        openedFromReport = true;
        // Just hide visually (do not destroy modal state)
        bookingReportModal_inDOM.classList.remove("show");
        bookingReportModal_inDOM.style.display = "none";

        $("#feedbackModal").modal("show");
    });
    function generateFeedbackUrl(bookingId, name, whatsapp) {

        const eBookingId = encodeValue(bookingId);
        const eName = encodeValue(name);
        const eWhatsapp = encodeValue(whatsapp);

        return `${window.location.origin}/FeedbackForm` +
            `?bid=${eBookingId}` +
            `&n=${eName}` +
            `&w=${eWhatsapp}`;
    }
    function validateFeedbackForm() {

        const nameInput = document.getElementById("fbName");
        const whatsappInput = document.getElementById("fbWhatsapp");

        const name = nameInput.value.trim();
        const whatsapp = whatsappInput.value.trim();

        let isValid = true;

        // Reset previous errors
        clearErrors();

        // Name validation
        if (!name) {
            showError(nameInput, "fbNameError", "Name is required");
            isValid = false;
        }

        // WhatsApp validation
        if (!whatsapp) {
            showError(whatsappInput, "fbWhatsappError", "WhatsApp number is required");
            isValid = false;
        }
        else if (!/^\d{10}$/.test(whatsapp)) {
            showError(whatsappInput, "fbWhatsappError", "Enter valid WhatsApp number with country code");
            isValid = false;
        }
        if (!isValid) {
            document.querySelector(".is-invalid").focus();
        }

        return isValid;
    }
    function showError(input, errorId, message) {
        input.classList.add("is-invalid");
        document.getElementById(errorId).innerText = message;
    }
    function clearErrors() {
        document.querySelectorAll(".is-invalid")
            .forEach(el => el.classList.remove("is-invalid"));

        document.querySelectorAll(".invalid-feedback")
            .forEach(el => el.innerText = "");
    }
    document.getElementById("fbName").addEventListener("input", clearErrors);
    document.getElementById("fbWhatsapp").addEventListener("input", clearErrors);

    document.getElementById("btnWhatsapp").addEventListener("click", function () {
        if (!validateFeedbackForm()) {
            return;
        }

        const name = document.getElementById("fbName").value.trim();
        const whatsapp = document.getElementById("fbWhatsapp").value.trim();
        const whatsappFullNumber = "91" + whatsapp;

        const bookingId = getBookingId(); // already available
        const feedbackUrl = generateFeedbackUrl(bookingId, name, whatsappFullNumber);

        const message =
            `Hello ${name},

Thank you for your booking.

We request you to share your feedback using the link below:
${feedbackUrl}

Your feedback helps us improve our service.`;

        const whatsappUrl = `https://wa.me/${whatsapp}?text=${encodeURIComponent(message)}`;
        window.open(whatsappUrl, "_blank");
        //bootstrap.Modal.getInstance(document.getElementById("feedbackModal")).hide();
        $("#feedbackModal").modal("hide");
    });

    // ❌ Cancel Booking
    document.getElementById("btnCancelBooking").addEventListener("click", function () {
        const bookingId = getBookingId();
        //let userId = parseInt( sessionStorage.getItem('userId') ? sessionStorage.getItem('userId') : 0 );

        //console.log("❌ Cancel Booking clicked - Booking ID:", bookingId);
        if (!bookingId || bookingId == 0) {
            Swal.fire({
                icon: "warning",
                title: "No Booking Selected",
                text: "Please select a booking to cancel."
            });
            return;
        }
        openedFromReport = true;
        // Just hide visually (do not destroy modal state)
        bookingReportModal_inDOM.classList.remove("show");
        bookingReportModal_inDOM.style.display = "none";

        $("#cancelBookingModal").modal("show");
    });

    // ✔ Confirm Booking Cancellation
    document.getElementById("btnConfirmCancelBooking").addEventListener("click", function () {

        const bookingId = getBookingId();
        const userId = parseInt(sessionStorage.getItem('userId') || 0);
        const cancelReason = $("#txtCancelReason").val().trim();

        //console.log("🟠 Confirm Cancel Clicked");

        // 🧪 Validate
        // Hide previous error
        $("#txtCancelReasonError").addClass("d-none");
        $("#txtCancelReason").removeClass("is-invalid");
        if (!cancelReason) {
            $("#txtCancelReasonError").text("Please enter a reason.").removeClass("d-none");
            $("#txtCancelReason").addClass("is-invalid");
            return;
        }

        // 📦 Prepare Request Body
        let reqModel = {
            BookingId: bookingId,
            CancelReason: cancelReason,
            ActionBy: userId
        };

        //console.log("📤 Sending Cancel Request:", reqModel);

        // 🔥 API Call
        $.ajax({
            url: "/api/BookingCalendar/CancelBookingCalendar",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(reqModel),
            success: function (response) {
                //console.log("✔ API Success:", response);

                if (response.status === "Success") {
                    handleApiSuccess("Booking Cancelled");

                    // Close modal
                    $("#cancelBookingModal").modal("hide");

                    // Reset textarea
                    $("#txtCancelReason").val("");

                    // 🔄 Refresh calendar or UI
                    loadBookingCalendar();
                    //CLOSE MODAL
                    bookingReportModal_inDOM.hide();

                }
                else {
                    Swal.fire({
                        icon: "error",
                        title: "Failed",
                        text: response.message
                    });
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Booking Cancel failed");
            }
        });

    });

    // 🗑️ Delete Booking
    var isSuperUser = sessionStorage.getItem("isSuperUser") === "true";
    if (isSuperUser) {
        document.getElementById("btnDeleteBooking").classList.remove("d-none");
    }

    document.getElementById("btnDeleteBooking").addEventListener("click", function () {
        const bookingId = getBookingId();
        //console.log("🗑️ Delete Booking clicked - Booking ID:", bookingId);
        let userId = parseInt(sessionStorage.getItem('userId') || 0);

        if (!bookingId || bookingId == 0) {
            Swal.fire({
                icon: 'warning',
                title: 'No Booking Selected',
                text: 'Please select a booking to delete.'
            });
            return;
        }

        // 🔄 Confirm delete (optional but recommended)
        Swal.fire({
            title: "Are you sure?",
            text: "Do you really want to delete this booking?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!"
        }).then((result) => {
            if (!result.isConfirmed) return;

            // 🟢 Call API
            $.ajax({
                url: "/api/BookingCalendar/DeleteBookingCalendar",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    BookingId: bookingId,
                    ActionBy: userId
                }),
                success: function (res) {
                    //console.log("✔ Delete API Success:", res);
                    if (res.status === "Success") {
                        handleApiSuccess("Booking Deleted");
                        // 🔄 Refresh calendar or UI
                        loadBookingCalendar();
                        //CLOSE MODAL
                        //bookingReportModal_inDOM.hide();
                        closeBookingReportModal();
                    }
                    else {
                        Swal.fire({
                            icon: "error",
                            title: "Failed",
                            text: res.message
                        });
                    }
                },
                error: function (xhr, status, error) {
                    handleApiError(xhr, status, error, "Booking Delete Failed")
                }
            });
        });
    });

    // 🍽️ Food Labels
    $("#btnFoodLabels").on("click", function () {
        const foodPackageJson = $(this).data("foodpackage");
        if (!foodPackageJson) {
            Swal.fire({
                icon: 'warning',
                title: 'No Data Available',
                text: 'No food package data available.'
            });
            return;
        }
        // Store temporarily in sessionStorage
        sessionStorage.setItem("bookingid", getBookingId());
        // Open label page
        window.open("/BookingCalendar/FoodLabel_Print", "_blank");
    });

    // 🚚 Vendor Work Orders button click
    document.getElementById("btnVendorWorkOrders").addEventListener("click", function () {
        const bookingId = getBookingId();
        //console.log("🚚 Vendor Work Orders clicked - Booking ID:", bookingId);
        //console.log("bookingId:", bookingId)

        // Optional: store bookingId in hidden field
        $("#VendorBookingId").val(bookingId);
        $("#VendorWorkOrderId").val(0);
        const today = new Date().toISOString().split('T')[0];
        $("#PODate").val(today);
        $("#EventDate").val(vendorWorkOrder_eventDate);
        $("#EventTime").val(vendorWorkOrder_eventTime);
        $("#SlotId").val(vendorWorkOrder_slotId);

        $("#vendorWorkOrderModal").modal("show");
    });
    $("#btnSaveVendorWorkOrder").on("click", function () {
        saveVendorWorkOrder();
    });

    // ✏️ Edit
    document.getElementById("btnEdit").addEventListener("click", function () {
        const bookingId = getBookingId();
        //console.log("✏️ Edit clicked - Booking ID:", bookingId);

        openedFromReport = true;
        // Just hide visually (do not destroy modal state)
        bookingReportModal_inDOM.classList.remove("show");
        bookingReportModal_inDOM.style.display = "none";

        // Load data
        loadBookingCalendar(bookingId, 0, null, "editData");
        $("#bookingModal").modal("show");
    });

    // 📅 Reschedule
    document.getElementById("btnReschedule").addEventListener("click", function () {
        const bookingId = getBookingId();
        //console.log("📅 Reschedule clicked - Booking ID:", bookingId);

        openedFromReport = true;
        // Just hide visually (do not destroy modal state)
        bookingReportModal_inDOM.classList.remove("show");
        bookingReportModal_inDOM.style.display = "none";

        // Load data
        loadBookingCalendar(bookingId, 0, null, "rescheduleData");
        $("#bookingRescheduleModal").modal("show");
    });

    // 🔔 Post Booking Followups
    // document.getElementById("btnPostBookingFollowups").addEventListener("click", function () {
    //     const bookingId = getBookingId();
    //     //console.log("🔔 Post Booking Followups clicked - Booking ID:", bookingId);
    // });

    // 📋 Prepare Order (PO)
    document.getElementById("btnPrepareOrder").addEventListener("click", function () {
        const bookingId = getBookingId();
        //console.log("📋 Prepare Order (PO) clicked - Booking ID:", bookingId);
        openedFromReport = true;
        // Just hide visually (do not destroy modal state)
        bookingReportModal_inDOM.classList.remove("show");
        bookingReportModal_inDOM.style.display = "none";

        //Load Data
        loadPrepareOrderByBooking(bookingId);
    });

    // 📤 Export Booking
    document.getElementById("btnExportBooking").addEventListener("click", function () {
        const bookingId = getBookingId();
        //console.log("📤 Export Booking clicked - Booking ID:", bookingId);
    });

    //BOOKING REPORT MODAL ---- END  -------------------------------------------------

    //Hall Block --- START ----------------------------
    $("#btnBlockHall").click(function () {
        $("#hallBlockModal").modal("show");
        resetForm_HallBlock();
        loadHallBlocks();
        loadHallList_BlockHall();
    });
    function loadHallList_BlockHall() {
        let container = $("#hallCheckboxContainer");
        container.empty();

        banquetHallsList.forEach(hall => {
            container.append(`
                <div class="form-check">
                    <input class="form-check-input hall-checkbox" 
                           type="checkbox" value="${hall.Id}">
                    <label class="form-check-label">${hall.HallName}</label>
                </div>
            `);
        });
    }

    $("#btnSaveHallBlock").click(function () {

        let selectedHalls = [];
        $(".hall-checkbox:checked").each(function () {
            selectedHalls.push({ HallId: parseInt($(this).val()) });
        });

        if (selectedHalls.length === 0) {
            Swal.fire({
                icon: "warning",
                title: "Hall Required",
                text: "Please select at least one hall.",
                confirmButtonColor: "#3085d6"
            });
            return;
        }
            
        let payload = {
            Id: parseInt($("#blockId_hallBlock").val()),
            FromDate: $("#fromDate_hallBlock").val(),
            ToDate: $("#toDate_hallBlock").val(),
            Reason: $("#reason_hallBlock").val(),
            Remarks: $("#remarks_hallBlock").val(),
            HallBlockDetailsJson: JSON.stringify(selectedHalls),
            ActionBy: Number(sessionStorage.getItem('userId') || 0)
        };

        $.ajax({
            url: "/api/HallBlock/SaveHallBlock",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess();
                    resetForm_HallBlock();
                    loadHallBlocks();
                }
                else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: res.message
                    });
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save Hall Block Failed");
            }
        });
    });
    function loadHallBlocks() {

        let tbody = $("#hallBlockTable tbody");
        tbody.html(`
            <tr>
                <td colspan="5" class="text-center text-muted">
                    <span class="spinner-border spinner-border-sm"></span>
                    Loading hall blocks...
                </td>
            </tr>
        `);

        $.ajax({
            url: "/api/HallBlock/GetHallBlock",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: 0 }),
            success: function (res) {
                tbody.empty();
                if (res.status !== "Success") {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: res.message || "Failed to fetch hall blocks."
                    });
                    return;
                }
                if (!res.data || res.data.length === 0) {
                    tbody.html(`
                        <tr>
                            <td colspan="5" class="text-center text-muted">
                                No hall blocks found.
                            </td>
                        </tr>
                    `);
                    return;
                }

                //console.log("hallBlock data:", res.data);
                res.data.forEach(item => {

                    let hallList = item.HallListJson || [];

                    let hallNames = hallList
                        .map(x => x.HallName)
                        .filter(name => name) // remove null/undefined
                        .join(", ");

                    tbody.append(`
                    <tr>
                        <td>${item.FromDate2}</td>
                        <td>${item.ToDate2}</td>
                        <td>${item.Reason || ""}</td>
                        <td>${hallNames}</td>
                        <td>
                            <button class="btn btn-sm btn-outline-warning edit-block" data-id="${item.Id}" title="Edit">
                                <i class="fa fa-pen-to-square"></i>
                            </button>

                            <button class="btn btn-sm btn-outline-danger delete-block" data-id="${item.Id}" title="Delete">
                                <i class="fa fa-trash"></i>
                            </button>
                        </td>
                    </tr>
                `);
                });
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Load Hall Blocks Failed");

                tbody.html(`
                <tr>
                    <td colspan="5" class="text-center text-danger">
                        Failed to load data.
                    </td>
                </tr>
            `);
            }
        });
    }

    $(document).on("click", ".edit-block", function () {

        let id = $(this).data("id");

        $.ajax({
            url: "/api/HallBlock/GetHallBlock",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: id }),

            success: function (res) {

                if (res.status !== "Success" || !res.data || res.data.length === 0) {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: res.message || "Unable to fetch hall block data."
                    });
                    return;
                }

                let data = res.data[0];

                // ✅ Use correct case
                $("#blockId_hallBlock").val(data.Id || 0);
                $("#fromDate_hallBlock").val(data.FromDate ? data.FromDate.split("T")[0] : "");
                $("#toDate_hallBlock").val(data.ToDate ? data.ToDate.split("T")[0] : "");
                $("#reason_hallBlock").val(data.Reason || "");
                $("#remarks_hallBlock").val(data.Remarks || "");

                $(".hall-checkbox").prop("checked", false);

                let hallList = data.HallListJson || [];

                hallList.forEach(h => {
                    $(`.hall-checkbox[value='${h.HallId}']`).prop("checked", true);
                });

                $("#btnSaveHallBlock").text("Update");

                $(".modal-body").animate({ scrollTop: 0 }, 300);
            },

            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Load Hall Block For Edit Failed");
            }
        });
    });

    $(document).on("click", ".delete-block", function () {
        let id = $(this).data("id");
        Swal.fire({
            title: "Are you sure?",
            text: "This will delete the hall block.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#6c757d",
            confirmButtonText: "Yes, delete it!"
        }).then((result) => {

            if (!result.isConfirmed) return;

            $.ajax({
                url: "/api/HallBlock/DeleteHallBlock",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({
                    Id: id,
                    ActionBy: Number(sessionStorage.getItem('userId') || 0)
                }),

                success: function (res) {

                    if (res.status === "Success") {
                        handleApiSuccess("Hall Block Deleted");
                        loadHallBlocks();
                    }
                    else {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: res.message || "Unable to delete."
                        });
                    }
                },

                error: function (xhr, status, error) {
                    handleApiError(xhr, status, error, "Delete Hall Block Failed");
                }
            });

        });
    });

    function resetForm_HallBlock() {
        $("#blockId_hallBlock").val(0);
        let today = new Date();
        // Format to yyyy-MM-dd (required for input[type="date"])
        let formatDateInput = (date) => {
            return date.toISOString().split("T")[0];
        };
        $("#fromDate_hallBlock").val(formatDateInput(today));
        $("#toDate_hallBlock").val(formatDateInput(today));
        $("#reason_hallBlock").val("");
        $("#remarks_hallBlock").val("");
        $(".hall-checkbox").prop("checked", false);
        $("#btnSaveHallBlock").text("Save");
    }
    //Hall Block --- END ------------------------------

    //Check Slot Availability ------- START ------------------------------------
    // Trigger on date, time, banquet change
    $("#fromDate, #startHour, #startMinute, #startAMPM, #endHour, #endMinute, #endAMPM, #banquetDropdown").on("change", function () {
        
        let eventDate = $("#fromDate").val()?.trim();
        let banquetId = parseInt($("#banquetDropdown").val()) || 0;
        let bookingId = getBookingId();

        // Get start time parts
        let sh = $("#startHour").val();
        let sm = $("#startMinute").val();
        let sampm = $("#startAMPM").val();

        // Get end time parts
        let eh = $("#endHour").val();
        let em = $("#endMinute").val();
        let eampm = $("#endAMPM").val();

        $("#slotStatusMsg").addClass("d-none");

        // Basic validation
        if (!eventDate || !sh || !sm || !eh || !em) {
            return;
        }

        // Format 12-hour time properly
        let eventStart = `${String(sh).padStart(2, '0')}:${String(sm).padStart(2, '0')} ${sampm}`;
        let eventEnd = `${String(eh).padStart(2, '0')}:${String(em).padStart(2, '0')} ${eampm}`;

        // Optional: prevent same time
        if (eventStart === eventEnd) {
            $("#slotStatusMsg")
                .removeClass("d-none text-success")
                .addClass("text-danger")
                .text("Start time and End time cannot be same.");
            return;
        }

        let payload = {
            EventDate: eventDate,
            StartTime: eventStart,   // 12-hour format
            EndTime: eventEnd,
            BanquetId: banquetId,
            BookingId: bookingId
        };

        //console.log("CHECKING SLOT:", payload);

        showTempLoader("Checking Availability");

        $("#btnBook").prop("disabled", true);

        $.ajax({
            url: "/api/EnquiryDeal/CheckBookingSlotAvailability",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                Swal.close();

                let isAvailable = res.data?.IsAvailable === 1;

                let msgBox = $("#slotStatusMsg");
                let warningBox = $("#slotStatusWarning");

                msgBox
                    .removeClass("d-none text-danger text-success bg-success-subtle bg-danger-subtle")
                    .addClass(isAvailable
                        ? "text-success bg-success-subtle"
                        : "text-danger bg-danger-subtle")
                    .text(res.message);

                // Show warning only if WarningMessage exists
                if (res.data && res.data.WarningMessage && res.data.WarningMessage.trim() !== "") {
                    warningBox
                        .removeClass("d-none")
                        .addClass("bg-warning-subtle text-dark")
                        .text(res.data.WarningMessage);
                }
                else {
                    warningBox.addClass("d-none");
                }

                // 🔹 Enable / Disable Book Button
                $("#btnBook").prop("disabled", !isAvailable);                
            },
            error: function () {
                Swal.close();
                $("#slotStatusMsg")
                    .removeClass("d-none text-success")
                    .addClass("text-danger")
                    .text("Unable to check slot availability");
            }
        });

    });
    //Check Slot Availability ------- END ------------------------------------

    // Event Type dropdown change event
    $(document).on("change", "#booking_eventDropdown", function () {
        let selectedEventTypeId = $(this).val();
        loadSubEventList(selectedEventTypeId); // Pass selected EventTypeId
    });

    //Booking List MODAL -- START
    $(document).on("click", ".view-booking-btn", function () {
        const bookingid = $(this).data("bookingid");
        const hallname = $(this).data("hallname");
        var banquetId = 0;
        var eventDate = null;
        var actionMode = "singleData";
        loadBookingCalendar(bookingid, banquetId, eventDate, actionMode);
        $("#bookingListModalTitle").text("Bookings (" + hallname + ")"); 

        isBookingInfoModal_OpenFromBookingList = true;
        $("#bookingListModal").modal("hide");
    });
    //Booking List MODAL -- END
});

// Invoice GENEARTE -- START --------------------------------

//ITEMS ROW --- START
$('#invoiceModal_btnAddItem').on('click', function () {
    addItemRow();
});

// On input change
$(document).on('input change', '.calc', function () {
    let row = $(this).closest('tr');
    recalcRow(row);
    recalcAll(row); // pass active row
});

$(document).on('input change', '#invoiceModal_gstin', function () {
    // GSTIN or global change → recalc all rows
    $('#invoiceModal_invoiceItemsBody tr').each(function () {
        recalcRow($(this));
    });
    recalcAll();
});

// On Advance input change
$(document).on('input', '#invoiceModal_advancePaid, #invoiceModal_bookingDiscount', function () {
    recalcAll();
});

// On delete row
$(document).on('click', '.btn-delete-row-invoice', function () {
    $(this).closest('tr').remove();
    updateRowSequence();
    recalcAll();
});
//ITEMS ROW --- END


function ShowHide_DeleteButton() {
    //Delete button HIDE/SHOW
    const isSuperUser = sessionStorage.getItem("isSuperUser") === "true";
    if (isSuperUser) {
        document.getElementById("invoiceModal_btnDeleteInvoice").classList.remove("d-none");
    }
}
function addItemRow() {
    const gstOptions = buildGSTDropdown();
    const row = `
        <tr>
            <td class="text-center col-index p-1"></td>

            <td class="col-desc p-1">
                <input type="text" class="form-control form-control-sm desc p-1">
            </td>

            <td class="col-hsn p-1">
                <input type="text" class="form-control form-control-sm hsn p-1">
            </td>

            <td class="col-rate p-1">
                <input type="number" class="form-control form-control-sm text-end rate calc p-1" value="0">
            </td>

            <td class="col-qty p-1">
                <input type="number" class="form-control form-control-sm text-end qty calc p-1" value="1">
            </td>

            <td class="col-subtotal p-1">
                <input type="number" class="form-control form-control-sm text-end subtotal calc p-1" value="0">
            </td>

            <td class="col-discount-rate p-1">
                <input type="number" class="form-control form-control-sm text-end discountrate calc p-1" value="0">
            </td>

            <td class="col-discount-type p-1">
                <select class="form-select form-select-sm discounttype calc p-1">
                    <option value="1">%</option>
                    <option value="2">Flat</option>
                </select>
            </td>

            <td class="col-discount-amount p-1">
                <input type="number" class="form-control form-control-sm text-end discountamount calc p-1" value="0">
            </td>

            <td class="col-taxable p-1">
                <input type="number" class="form-control form-control-sm text-end taxable calc p-1" value="0">
            </td>

            <td class="col-gst p-1">
                <select class="form-select form-select-sm gst calc p-1">
                    ${gstOptions}
                </select>
            </td>

            <td class="col-tax p-1">
                <input type="number" class="form-control form-control-sm text-end cgst p-1" readonly>
            </td>

            <td class="col-tax p-1">
                <input type="number" class="form-control form-control-sm text-end sgst p-1" readonly>
            </td>

            <td class="col-tax p-1">
                <input type="number" class="form-control form-control-sm text-end igst p-1" readonly>
            </td>

            <td class="col-total p-1">
                <input type="number" class="form-control form-control-sm text-end fw-bold total p-1" readonly>
            </td>

            <td class="text-center col-action p-1">
                <i class="fa fa-times text-danger btn-delete-row-invoice" role="button"></i>
            </td>
        </tr>
    `;

    $('#invoiceModal_invoiceItemsBody').append(row);
    updateRowSequence();
}

var gstList = [];
loadGSTMaster();
function loadGSTMaster() {
    $.ajax({
        url: "/api/GSTMaster/GetGSTMaster",
        type: "GET",
        success: function (res) {
            if (res.status === "Success") {
                gstList = res.data;
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load GST master.");
        }
    });
}

function buildGSTDropdown(selectedGST = 0) {
    let options = '';
    gstList.forEach(g => {
        const selected = parseFloat(g.GSTValue) === parseFloat(selectedGST)
            ? 'selected'
            : '';
        options += `<option value="${g.GSTValue}" ${selected}>${g.GSTText}</option>`;
    });
    return options;
}

function updateRowSequence() {
    $('#invoiceModal_invoiceItemsBody tr').each(function (index) {
        $(this).find('.col-index').text(index + 1);
    });
}
function adjustLastRow(diff) {
    const rows = $('#invoiceModal_invoiceItemsBody tr');
    if (rows.length === 0) return;

    //const lastRow = rows.last();
    const lastRow = $('#invoiceModal_invoiceItemsBody tr')
        .filter(function () {
            return (parseFloat($(this).find('.qty').val()) || 0) > 0;
        })
        .last();

    let rate = parseFloat(lastRow.find('.rate').val()) || 0;
    let qty = parseFloat(lastRow.find('.qty').val()) || 1;
    let gstRate = parseFloat(lastRow.find('.gst').val()) || 0;

    if (qty === 0) qty = 1;

    const factor = 1 + (gstRate / 100);

    // ✅ Correct adjustment considering GST
    let rateAdjustment = diff / (qty * factor);

    rate = rate + rateAdjustment;

    if (rate < 0) rate = 0;

    lastRow.find('.rate').val(rate.toFixed(2));

    recalcRow(lastRow);
}
function round2(val) {
    return Math.round(val * 100) / 100;
}

function recalcRow(row) {

    const rate = parseFloat(row.find('.rate').val()) || 0;
    const qty = parseFloat(row.find('.qty').val()) || 0;
    const gstRate = parseFloat(row.find('.gst').val()) || 0;

    const discountRate = parseFloat(row.find('.discountrate').val()) || 0;
    const discountType = row.find('.discounttype').val();

    const gstFactor = 1 + (gstRate / 100);

    // Step 1: Subtotal
    let subTotal = round2(rate * qty);
    row.find('.subtotal').val(subTotal.toFixed(2));

    // Step 2: Discount Calculation
    let discountAmount = 0;
    if (discountType === "1") {
        discountAmount = subTotal * discountRate / 100;
    } else {
        discountAmount = discountRate;
    }
    discountAmount = round2(discountAmount);
    row.find('.discountamount').val(discountAmount.toFixed(2));

    // Step 3: Taxable after discount
    let taxable = round2(subTotal - discountAmount);
    if (taxable < 0) taxable = 0;
    row.find('.taxable').val(taxable.toFixed(2));

    // Step 4: GST Calculation
    let gstAmount = round2(taxable * gstRate / 100);

    const gstin = ($('#invoiceModal_gstin').val() || '').trim();
    const stateCode = gstin.length >= 2 ? gstin.substring(0, 2) : "";

    let cgst = 0;
    let sgst = 0;
    let igst = 0;

    if (gstin && stateCode !== "24") {
        // Inter-state → IGST
        igst = gstAmount;
    }
    else {
        // Intra-state → CGST + SGST
        cgst = gstAmount / 2;
        sgst = gstAmount / 2;
    }
    let total = round2(taxable + gstAmount);

    row.find('.cgst').val(cgst.toFixed(2));
    row.find('.sgst').val(sgst.toFixed(2));
    row.find('.igst').val(igst.toFixed(2));
    row.find('.total').val(total.toFixed(2));
}
function recalcAll() {

    let subTotal = 0;
    let totalTaxable = 0;
    let totalGST = 0;
    let totalDiscount = 0;

    const isInclusiveGST = $('#invoiceModal_invoiceNo').data('isinclusivegst') === true;

    let rows = $('#invoiceModal_invoiceItemsBody tr');

    // ✅ Step 1: Sum totals
    rows.each(function () {
        const row = $(this);

        let total = parseFloat(row.find('.total').val()) || 0;
        let taxable = parseFloat(row.find('.taxable').val()) || 0;
        let cgst = parseFloat(row.find('.cgst').val()) || 0;
        let sgst = parseFloat(row.find('.sgst').val()) || 0;
        let igst = parseFloat(row.find('.igst').val()) || 0;
        let discountamount = parseFloat(row.find('.discountamount').val()) || 0;

        subTotal += total;
        totalTaxable += taxable;
        totalGST += (cgst + sgst + igst);
        totalDiscount += discountamount;
    });

    // ✅ Step 2: Set fixed subtotal once
    //if (isInclusiveGST && fixedSubTotal === 0) {
    //    fixedSubTotal = subTotal;
    //}

    // ✅ Step 3: Show totals (NO clamp, NO adjustment)
    $('#invoiceModal_bookingDiscount').val(totalDiscount.toFixed(2));
    $('#invoiceModal_subTotal').val(subTotal.toFixed(2));
    $('#invoiceModal_totalTaxable').val(totalTaxable.toFixed(2));
    $('#invoiceModal_totalGST').val(totalGST.toFixed(2));

    updateInvoiceTitle(totalGST);

    // ✅ Step 4: Grand total
    const discount = parseFloat($('#invoiceModal_bookingDiscount').val()) || 0;
    const grandTotal = totalTaxable + totalGST - discount;
    $('#invoiceModal_grandTotal').val(grandTotal.toFixed(2));

    let roundedTotal = Math.round(grandTotal);
    $('#invoiceModal_roundOff').val(roundedTotal.toFixed(2));

    const advance = parseFloat($('#invoiceModal_advancePaid').val()) || 0;
    $('#invoiceModal_balanceAmount, #invoiceModal_invoicePayment_DueBalance, #invoiceModal_invoicePayment_originalDueAmount')
        .val((roundedTotal - advance).toFixed(2));
}

let fixedSubTotal = 0;
function setFixedSubTotal() {
    fixedSubTotal = parseFloat($('#invoiceModal_subTotal').val()) || 0;
}
function updateInvoiceTitle(totalGST) {
    const isInclusiveGST = $('#invoiceModal_invoiceNo').data('isinclusivegst') === true;
    //console.log("isInclusiveGST:", isInclusiveGST);
    const gstLabel = isInclusiveGST ? 'Rates Inclusive GST' : 'Rates Exclusive GST';

    //console.log("gstInvoiceNo:", gstInvoiceNo, ", totalGST:", totalGST, ", invoiceType:", invoiceType);

    if (gstInvoiceNo !== null && gstInvoiceNo !== '' && parseFloat(totalGST) === 0 && invoiceType == "invoice") {
        $('#invoiceModal_ModalTitle').text(`Create Invoice #${gstInvoiceNo} (${gstLabel})`);
    }
    else if (invoiceType == "invoice") {
        $('#invoiceModal_ModalTitle').text(`Create Invoice #${invoiceNo} (${gstLabel})`);
    }
}

//proforma & invoice -- fetch -- START
function fetchInvoiceByBookingId(bookingId, isInclusiveGST = false, isProformaInvoice = false) {
    $("#invoiceModal_paymentSection").removeClass("d-none");
    var payload = {
        BookingId: bookingId,
        IsInclusiveGST: isInclusiveGST,
        IsProformaInvoice: isProformaInvoice
    };
    //console.log("payload fetchInvoiceByBookingId:", payload);
    $.ajax({

        url: '/api/CustomerInvoice/GetCustomerInvoiceByBookingId', // adjust if needed
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("fetch ByBookingId res:", res);
            if (res.status !== 'Success' || !res.data) {
                Swal.fire({
                    icon: 'warning',
                    title: 'Invoice Not Found',
                    text: 'Invoice data not found',
                    confirmButtonText: 'OK'
                });
                return;
            }

            var data = res.data;
            /* ---------- MASTER FIELDS ---------- */
            $('#invoiceModal_invoiceNo').val(invoiceNo);
            $('#invoiceModal_proformaInvoiceNo').val(proformaInvoiceNo);

            $('#invoiceModal_customerName').val(data.ClientName);
            $('#invoiceModal_invoiceDate').val(data.InvoiceDate);
            $('#invoiceModal_gstin').val(data.GSTIN);
            $('#invoiceModal_emailId').val(data.Email);
            $('#invoiceModal_address').val(data.Address);
            $('#invoiceModal_customerState').val(data.ClientStateId);
            $('#invoiceModal_isExclusiveOfGST').val(data.IsExclusiveOfGST);
            $('#invoiceModal_isInclusiveOfGST').val(data.IsInclusiveOfGST);

            //$('#isGstIncluded').prop('checked', data.IsGSTIncluded === true);

            $('#invoiceModal_invoiceItemsBody').empty();

            if (data.ItemsJson && data.ItemsJson.length > 0) {

                data.ItemsJson.forEach(item => {

                    const taxable = parseFloat(item.Taxable) || 0;
                    //const gstPercent = taxable > 0 && item.GSTRate ? ((item.GSTRate / taxable) * 100).toFixed(2) : 0;
                    const gstOptions = buildGSTDropdown(item.GSTRate);

                    const row = `
                        <tr>
                            <td class="text-center col-index p-1"></td>

                            <td class="p-1">
                                <input type="text" class="form-control form-control-sm desc p-1"
                                       value="${item.Item_Description ?? ''}">
                            </td>

                            <td class="p-1">
                                <input type="text" class="form-control form-control-sm hsn p-1"
                                       value="${item.HSN ?? ''}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end rate calc p-1"
                                       value="${item.Rate ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end qty calc p-1"
                                       value="${item.Qty ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end subtotal calc p-1"
                                       value="${item.SubTotal ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end discountrate calc p-1"
                                       value="${item.DiscountRate ?? 0}">
                            </td>

                            <td class="p-1">
                                <select class="form-select form-select-sm discounttype calc p-1">
                                    <option value="1" ${(item.DiscountType ?? 1) == 1 ? "selected" : ""}>%</option>
                                    <option value="2" ${(item.DiscountType ?? 1) == 2 ? "selected" : ""}>Flat</option>
                                </select>
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end discountamount p-1 calc"
                                       value="${item.DiscountAmount ?? 0}">
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end taxable p-1 calc"
                                       value="${item.Taxable ?? 0}">
                            </td>

                            <td class="col-gst p-1">
                                <select class="form-select form-select-sm gst calc p-1">
                                    ${gstOptions}
                                </select>
                            </td>


                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end cgst p-1"
                                       value="${(item.CGST ?? 0).toFixed(2)}" readonly>
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end sgst p-1"
                                       value="${(item.SGST ?? 0).toFixed(2)}" readonly>
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end igst p-1"
                                       value="${(item.IGST ?? 0).toFixed(2)}" readonly>
                            </td>

                            <td class="p-1">
                                <input type="number" class="form-control form-control-sm text-end fw-bold total p-1"
                                       value="${(item.Total ?? 0).toFixed(2)}" readonly>
                            </td>

                            <td class="text-center p-1">
                                <i class="fa fa-times text-danger btn-delete-row-invoice" role="button"></i>
                            </td>
                        </tr>
                        `;

                    $('#invoiceModal_invoiceItemsBody').append(row);
                });

                updateRowSequence();
                recalcAll();
                $('#invoiceModal_advancePaid').val(data.PaidAmount).attr('readonly', true);
            } else {
                addItemRow(); // empty invoice
            }
            /* ---------- ITEMS ---------- */
            //bookingReportModal.hide();
            bookingReportModal_inDOM.classList.remove("show");
            bookingReportModal_inDOM.style.display = "none";

            openedFromReport = true;
            invoiceModal.show();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Invoice Generation Failed");
            //openedFromReport = false;
            //bookingReportModal.hide();
            //loadBookingCalendar(bookingId, 0, null, "singleData");
        }
    });
}

//proforma & invoice -- fetch -- END
function resetForm_InvoiceModal() {
    invoiceType = null;
    
    $('#invoiceModal_invoiceId').val(0);
    $('#invoiceModal_bookingId').val(0);
    $('#invoiceModal_invoiceNo').val('').removeData('isinclusivegst');
    $('#invoiceModal_gstInvoiceNo').val('');
    $('#invoiceModal_proformaInvoiceNo').val('');

    $('#invoiceModal_isExclusiveOfGST').val(false);
    $('#invoiceModal_isInclusiveOfGST').val(false);

    $('#invoiceModal_customerName').val('');
    $('#invoiceModal_gstin').val('');
    $('#invoiceModal_emailId').val('');
    $('#invoiceModal_address').val('');

    $('#invoiceModal_invoicePayment_Mode, #invoiceModal_invoicePayment_Account').val('0');
    $('#invoiceModal_invoicePayment_Txn, #invoiceModal_invoicePayment_ChequeBank, #invoiceModal_invoicePayment_ChequeNo, #invoiceModal_invoicePayment_ChequeDate, #invoiceModal_invoicePayment_NextDueDate').val('');
    $('#invoiceModal_invoicePayment_Amount, #invoiceModal_invoicePayment_originalDueAmount').val(0);
    $('#invoiceModal_invoicePayment_DueBalance').val("0.00").prop('readonly', false);
    $('#invoiceModal_advancePaid').val("0.00").attr('readonly', true);

    $('#invoiceModal_subTotal, #invoiceModal_bookingDiscount, #invoiceModal_totalTaxable, #invoiceModal_totalGST, #invoiceModal_grandTotal, #invoiceModal_roundOff, #invoiceModal_balanceAmount').val('0.00');

    $('#invoiceModal_invoiceItemsBody').empty();
    $('#invoiceModal_btnSaveInvoice').prop('disabled', false);

    const now = new Date();
    // Format Date → YYYY-MM-DD
    const date = now.toISOString().split("T")[0];
    // Format Time → HH:MM (24hr)
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const time = `${hours}:${minutes}`;
    // Set values
    $('#invoiceModal_invoiceDate').val(date);
    $('#invoiceModal_invoicePayment_Date').val(date);
    $('#invoiceModal_invoicePayment_Time').val(time);
}

//SAVE INVOICE
$('#invoiceModal_btnSaveInvoice').on('click', function () {
    if (!validateBeforeSave()) return;

    const items = [];
    $('#invoiceModal_invoiceItemsBody tr').each(function (index) {
        const row = $(this);

        items.push({
            SrNo: index + 1,
            Description: row.find('.desc').val(),
            HSN: row.find('.hsn').val(),
            Rate: parseFloat(row.find('.rate').val()) || 0,
            Qty: parseFloat(row.find('.qty').val()) || 0,
            SubTotal: parseFloat(row.find('.subtotal').val()) || 0,

            DiscountRate: parseFloat(row.find('.discountrate').val()) || 0,
            DiscountTypeId: parseInt(row.find('.discounttype').val()) || 0,
            DiscountAmount: parseFloat(row.find('.discountamount').val()) || 0,

            TaxableAmount: parseFloat(row.find('.taxable').val()) || 0,
            GSTRate: parseFloat(row.find('.gst').val()) || 0,
            CGST: parseFloat(row.find('.cgst').val()) || 0,
            SGST: parseFloat(row.find('.sgst').val()) || 0,
            IGST: parseFloat(row.find('.igst').val()) || 0,

            Total: parseFloat(row.find('.total').val()) || 0
        });
    });

    //VALIDATION -- START ------------------------------
    let paymentAmount = parseFloat($("#invoiceModal_invoicePayment_Amount").val()) || 0;
    let paymentMode = parseInt($("#invoiceModal_invoicePayment_Mode").val()) || 0;
    let paymentAccount = parseInt($("#invoiceModal_invoicePayment_Account").val()) || 0;

    if (paymentAmount > 0) {

        if (paymentMode === 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Payment Mode Required',
                text: 'Please select a payment mode.',
                confirmButtonColor: '#d33'
            });
            return false;
        }

        if (paymentAccount === 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Account Required',
                text: 'Please select an account.',
                confirmButtonColor: '#d33'
            });
            return false;
        }
    }
    //VALIDATION -- END ------------------------------

    //const isGstIncluded = $('#isGstIncluded').is(':checked');
    var totalGst = parseFloat($('#invoiceModal_totalGST').val()) || 0;
    const gstInvoiceNoVal = gstInvoiceNo;
    const invoiceNoVal = $('#invoiceModal_invoiceNo').val();
    const proformaNoVal = $('#invoiceModal_proformaInvoiceNo').val();

    const isExclusiveOfGST = $('#invoiceModal_isExclusiveOfGST').val() === 'true' || $('#invoiceModal_isExclusiveOfGST').val() === '1';
    const isInclusiveOfGST = $('#invoiceModal_isInclusiveOfGST').val() === 'true' || $('#invoiceModal_isInclusiveOfGST').val() === '1';

    const request = {
        Id: parseInt($('#invoiceModal_invoiceId').val()) || 0,
        BookingId: parseInt($('#invoiceModal_bookingId').val()) || 0,

        InvoiceNo: (invoiceType === "invoice" && (totalGst > 0 || gstInvoiceNo == null))
            ? (invoiceNoVal ? Number(invoiceNoVal) : null)
            : null,

        ProformaInvoiceNo: (invoiceType === "proforma")
            ? (proformaNoVal ? Number(proformaNoVal) : null)
            : null,

        GSTInvoiceNo: (invoiceType === "invoice"
            && Math.abs(totalGst) < 0.01
            && gstInvoiceNoVal !== '')
            ? gstInvoiceNoVal
            : null,

        IsExclusiveOfGST: isExclusiveOfGST,
        IsInclusiveOfGST: isInclusiveOfGST,

        CustomerName: $('#invoiceModal_customerName').val(),
        InvoiceDate: $('#invoiceModal_invoiceDate').val(),
        GSTIN: $('#invoiceModal_gstin').val(),
        Address: $('#invoiceModal_address').val(),
        EmailId: $('#invoiceModal_emailId').val(),
        CustomerState: parseInt($('#invoiceModal_customerState').val()) || null,

        Payment_ModeId: parseInt($('#invoiceModal_invoicePayment_Mode').val()) || 0,
        Payment_AccountId: parseInt($('#invoiceModal_invoicePayment_Account').val()) || 0,
        Payment_TransactionId: $('#invoiceModal_invoicePayment_Txn').val()?.trim() || null,
        Payment_ChequeBank: $('#invoiceModal_invoicePayment_ChequeBank').val()?.trim() || null,
        Payment_ChequeNo: $('#invoiceModal_invoicePayment_ChequeNo').val()?.trim() || null,
        Payment_ChequeDate: $('#invoiceModal_invoicePayment_ChequeDate').val() || null,
        Payment_Amount: parseFloat($('#invoiceModal_invoicePayment_Amount').val()) || 0,
        Payment_PaidDate: $('#invoiceModal_invoicePayment_Date').val() || null,
        Payment_PaidTime: $('#invoiceModal_invoicePayment_Time').val() || null,
        Payment_DueDate: $('#invoiceModal_invoicePayment_NextDueDate').val() || null,
        Payment_Note: $('#invoiceModal_invoicePayment_noteEditor textarea').val()?.trim() || null,

        SubTotal: parseFloat($('#invoiceModal_subTotal').val()) || 0,
        BookingDiscount: parseFloat($('#invoiceModal_bookingDiscount').val()) || 0,
        TotalTaxableAmount: parseFloat($('#invoiceModal_totalTaxable').val()) || 0,
        TotalDiscountAcrossAllServices: 0,     // if not used
        TotalGST: totalGst,
        TotalVAT: 0,
        GrandTotal: parseFloat($('#invoiceModal_grandTotal').val()) || 0,
        RoundOff: parseFloat($('#invoiceModal_roundOff').val()) || 0,
        TotalAdvancePaid: parseFloat($('#invoiceModal_advancePaid').val()) || 0,
        BalanceAmount: parseFloat($('#invoiceModal_balanceAmount').val()) || 0,

        JSON_Items: items,
        ActionBy: Number(sessionStorage.getItem('userId') || 0)
    };
    //console.log("save payload:", request);

    $.ajax({
        url: '/api/CustomerInvoice/SaveCustomerInvoice', // adjust if needed
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: function (res) {
            if (res.status === 'Success') {
                handleApiSuccess('Invoice saved successfully');
                loadBookingCalendar();
                getNextInvoiceNumbers();
                //resetForm();
                $('#invoiceModal_btnSaveInvoice').prop('disabled', true);

                openedFromReport = false;
                invoiceModal.hide();
                bookingReportModal.hide();

                loadBookingCalendar(request.BookingId, 0, null, "singleData");
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Failed',
                    text: res.message || 'Failed to save invoice'
                });
            }
        },
        error: function (xhr, statue, error) {
            handleApiError(xhr, statue, error, "Save Invoice Failed");
        }
    });
});

$('#invoiceModal_btnAdjustDiff').on('click', function () {
    const currentSubTotal = parseFloat($('#invoiceModal_subTotal').val()) || 0;
    const diff = fixedSubTotal - currentSubTotal;

    if (Math.abs(diff) <= 0.01) return;

    adjustLastRow(diff);
    recalcAll();
});


function validateBeforeSave() {
    const isInclusiveGST = $('#invoiceModal_invoiceNo').data('isinclusivegst') === true;
    if (!isInclusiveGST) return true;

    const currentSubTotal = parseFloat($('#invoiceModal_subTotal').val()) || 0;
    const diff = fixedSubTotal - currentSubTotal;

    // ✅ Auto-fix small difference
    if (Math.abs(diff) <= 0.05 && Math.abs(diff) > 0.01) {
        adjustLastRow(diff);
        recalcAll();
    }

    // ❌ Still mismatch → block
    if (Math.abs(diff) > 0.05) {
        $("#invoiceModal_btnAdjustDiff").removeClass("d-none");
        $("#invoiceModal_diffValue").text(diff.toFixed(2));
        Swal.fire({
            icon: 'error',
            title: 'Invalid Total',
            text: `Difference ₹${diff.toFixed(2)}. Please adjust items.`
        });
        return false;
    }
    return true;
}


function getNextInvoiceNumbers() {

    $.ajax({
        url: '/api/CustomerInvoice/GetCustomerInvoice_NextInvoiceNumbers',
        type: 'GET',
        success: function (res) {
            if (res.status === 'Success' && res.data) {
                invoiceNo = res.data.NextInvoiceNo;
                proformaInvoiceNo = res.data.NextProformaInvoiceNo;
                gstInvoiceNo = res.data.NextGSTInvoiceNo;

                //console.log('Invoice No:', invoiceNo);
                //console.log('Proforma Invoice No:', proformaInvoiceNo);
                //console.log('GST Invoice No:', gstInvoiceNo);

                // OPTIONAL: Assign to inputs
                //$('#invoiceNo').val(invoiceNo);
                //$('#proformaInvoiceNo').val(proformaInvoiceNo);
                //$('#gstInvoiceNo').val(gstInvoiceNo);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Invoice Numbers Failed");
        }
    });
}
// Invoice GENERATE -- /END --------------------------------


let openedFromPrepareOrder = false;
function loadPrepareOrderByBooking(bookingId) {
    var payload = {
        BookingId: bookingId,
        PrepareOrderId: 0   // always 0 in this case
    };
    $.ajax({
        url: "/api/BookingCalendar_PrepareOrder/GetBookingCalendar_PrepareOrder",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("prepareOrder - Booking API Response:", res);
            if (!res || res.status !== "Success" || !res.data) {
                console.warn("No prepare order data found");
                return;
            }
            const po = res.data;
            const foodItems = res.data.FoodItems || [];

            /* ------------------------------------
               Hidden Fields
            ------------------------------------ */
            $("#PO_BookingId").val(bookingId);
            $("#PO_Id").val(po?.Id || 0);
            $("#PO_EnquiryDealId").val(po?.EnquiryDealId || 0);

            /* ------------------------------------
               Header / Client Details
            ------------------------------------ */
            // Vendor name might come from another API – keeping safe
            $("#PO_ClientName").val(po?.EnquiryDealName || "—");

            $("#PO_Pax").val(po?.PAX || 0);
            $("#PO_Pax_lbl").text(po?.PAX || 0);

            /* ------------------------------------
               PO Fields
            ------------------------------------ */
            $("#PO_No").val(po?.PONumber || "");
            $("#PO_Date").val(po?.PODate || "");
            $("#PO_EventDate").val(po?.EventDate || "");
            $("#PO_Slot").val(po?.SlotId || "");
            $("#PO_Notes").val(po?.Notes || "");

            /* ------------------------------------
               Food Items (Edit Mode)
            ------------------------------------ */
            bindFoodItemsForEdit(foodItems);

            /* ------ Open Modal ----- */
            $("#lblPrepareOrder").text("Prepare Order");


            $("#prepareOrderModal").modal("show");
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load prepare order");
        }
    });
}
let foodItemState = {};
function bindFoodItemsForEdit(foodItemsFromApi) {

    foodItemState = {}; // reset ONCE here

    foodItemsFromApi.forEach(fi => {
        foodItemState[fi.FoodItemId] = {
            PrepareOrderFoodItemId: fi.PrepareOrderFoodItemId || 0,
            PrepareOrderId: fi.PrepareOrderId || 0,

            FoodItemId: fi.FoodItemId,
            FoodItemName: fi.FoodItemName,

            HeaderId: fi.HeaderId,
            HeaderName: fi.HeaderName,

            IsAdded: fi.IsAdded === true,
            FoodFormula: fi.FoodFormula?.[0] || null
        };
    });

    renderFoodItems(); // render AFTER state ready
}
function renderFoodItems() {

    const container = $("#foodItemsContainer");
    container.empty();

    // destroy old tooltips safely
    $('[data-bs-toggle="tooltip"]').tooltip('dispose');

    // group by header
    const grouped = {};
    Object.values(foodItemState).forEach(item => {
        const header = item.HeaderName?.trim() || "Food Items";
        grouped[header] ??= [];
        grouped[header].push(item);
    });

    Object.keys(grouped).forEach(header => {

        container.append(`
                <div class="food-header w-100 mt-1 mb-1">
                    ${header}
                </div>
            `);

        const itemsRow = $('<div class="d-flex flex-wrap gap-4 ps-3 mb-1"></div>');

        grouped[header].forEach(item => {
            const hasFormula =
                item.FoodFormula &&
                item.FoodFormula.IsFormulaRequired === true &&
                item.FoodFormula.Ingredients &&
                item.FoodFormula.Ingredients.length > 0;
            // Tooltip only when formula is missing
            const tooltipAttr = hasFormula
                ? ''
                : 'data-bs-toggle="tooltip" title="Empty formula"';

            itemsRow.append(`
                    <div class="d-flex align-items-center gap-2">
                        <input type="checkbox"
                               class="form-check-input"
                               ${item.IsAdded ? "checked" : ""}
                               onchange="toggleFoodItem(${item.FoodItemId}, this.checked)">

                        <span class="food-item-name"
                              ${tooltipAttr}
                              onclick="openFormulaModal(${item.FoodItemId}, '${escapeHtml(item.FoodItemName)}')">
                            ${item.FoodItemName}
                        </span>
                    </div>
                `);
        });

        container.append(itemsRow);
    });

    // enable tooltips ONLY on spans
    new bootstrap.Tooltip(document.body, {
        selector: '.food-item-name[data-bs-toggle="tooltip"]'
    });
}
$("#PO_Pax").on("input", function () {
    $("#PO_Pax_lbl").text($(this).val());
});
function toggleFoodItem(foodItemId, isChecked) {
    if (!foodItemState[foodItemId]) return;
    foodItemState[foodItemId].IsAdded = isChecked;
}
function openFormulaModal(foodItemId, foodName) {

    const item = foodItemState[foodItemId];
    if (!item) return;

    $("#formulaFoodName").text(foodName);
    $("#hfFormulaFoodItemId").val(foodItemId);

    const f = item.FoodFormula;

    $("#chkFormulaNotRequired").prop("checked", f?.IsFormulaRequired === false);
    $("#txtFormulaPeople").val(f?.PeopleCount || 1);

    $("#tblIngredients tbody").empty();

    if (f?.Ingredients?.length > 0) {
        f.Ingredients.forEach(i => addIngredientRow(i));
    }

    openedFromPrepareOrder = true;
    $("#prepareOrderModal").modal("hide");
    setTimeout(() => {
        $("#formulaModal").modal("show");
    }, 200);
}
function addIngredientRow(data = {}) {
    $("#tblIngredients tbody").append(`
            <tr>
                <td>
                    <input class="form-control ing-name" placeholder="Ingredient name" value="${data.IngredientName || ''}">
                </td>

                <td>
                    <input type="number" class="form-control ing-qty" min="0" step="1" value="${data.Quantity || ''}">
                </td>

                <td>
                    <select class="form-select ing-unit">
                        ${getUnitOptionsHtml(data.UnitMaster_Id)}
                    </select>
                </td>

                <td class="text-center">
                    <i class="fa fa-trash text-danger"
                       style="cursor:pointer"
                       onclick="$(this).closest('tr').remove()"></i>
                </td>
            </tr>
        `);
}
$("#chkFormulaNotRequired").on("change", function () {
    const disabled = $(this).is(":checked");

    $("#txtFormulaPeople").prop("disabled", disabled);
    $("#tblIngredients :input").prop("disabled", disabled);
});
function saveFormula() {
    const foodItemId = $("#hfFormulaFoodItemId").val();
    if (!foodItemState[foodItemId]) return;

    let ingredients = [];
    if (!$("#chkFormulaNotRequired").is(":checked")) {
        $("#tblIngredients tbody tr").each(function () {
            const name = $(this).find(".ing-name").val()?.trim();
            const qty = parseFloat($(this).find(".ing-qty").val());
            const unit = $(this).find(".ing-unit").val();

            if (!name || isNaN(qty) || qty <= 0) return;

            ingredients.push({
                IngredientName: name,
                Quantity: Number(qty),
                UnitMaster_Id: Number(unit)
            });
        });
    }

    //console.log("before foodItemState:", foodItemState);
    foodItemState[foodItemId].FoodFormula = {
        PeopleCount: parseInt($("#txtFormulaPeople").val()) || 1,
        IsFormulaRequired: !$("#chkFormulaNotRequired").is(":checked"),
        Ingredients: ingredients
    };
    //console.log("after foodItemState:", foodItemState);
    $("#formulaModal").modal("hide");
}
function escapeHtml(text) {
    if (text === null || text === undefined) return "";

    return String(text)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

function savePrepareOrder() {

    let userId = Number(sessionStorage.getItem('userId') || 0);

    // Build FoodItems payload exactly as SP expects
    const foodItemsPayload = Object.values(foodItemState)
        .map(item => {
            let formula = null;

            // Only attach formula object if it exists
            if (item.FoodFormula) {
                formula = {
                    PeopleCount: item.FoodFormula.PeopleCount || 1,
                    IsFormulaRequired: item.FoodFormula.IsFormulaRequired === true,
                    Ingredients: (item.FoodFormula.Ingredients || []).map(i => ({
                        IngredientName: i.IngredientName,
                        Quantity: i.Quantity,
                        UnitMasterId: i.UnitMaster_Id   // matches SP
                    }))
                };
            }

            return {
                FoodItemId: item.FoodItemId,
                IsAdded: item.IsAdded === true,   // ✅ true OR false
                FoodFormula: formula              // may be null
            };
        });

    const payload = {
        Id: Number($("#PO_Id").val() || 0),
        BookingId: Number($("#PO_BookingId").val() || 0),
        EnquiryDealId: Number($("#PO_EnquiryDealId").val() || 0),
        PAX: $("#PO_Pax").val(),
        PONumber: $("#PO_No").val(),
        PODate: $("#PO_Date").val(),
        EventDate: $("#PO_EventDate").val(),
        SlotId: Number($("#PO_Slot").val() || 0),
        Notes: $("#PO_Notes").val(),
        Action_By: userId,
        FoodItems: foodItemsPayload
    };

    //console.log("Save Prepare Order Payload:", payload);

    $.ajax({
        url: "/api/BookingCalendar_PrepareOrder/SaveBookingCalendar_PrepareOrder",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        beforeSend: function () {
            $("#btnSavePrepareOrder")
                .prop("disabled", true)
                .text("Saving...");
        },
        success: function (response) {
            if (response.status === "Success") {
                handleApiSuccess();
                // Close modal safely
                $("#prepareOrderModal").modal("hide");
            }
            else {
                Swal.fire({
                    icon: "error",
                    title: "Save Failed",
                    text: response.message
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Prepare Order Save Error");
        },
        complete: function () {
            $("#btnSavePrepareOrder")
                .prop("disabled", false)
                .text("Save");
        }
    });
}



let unitMasterList = [];
fetchUnitMaster();
function fetchUnitMaster() {

    return $.ajax({
        url: "/api/UnitMaster/GetUnitMasterList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 })
    }).then(function (response) {

        //console.log("UnitMaster Response:", response);

        if (response.status === "Success") {
            unitMasterList = response.data || [];
        } else {
            unitMasterList = [];
        }

    }).catch(function (xhr, status, error) {
        handleApiError(xhr, status, error, "UnitMaster Fetch Error");
        unitMasterList = [];
    });
}
function getUnitOptionsHtml(selectedUnitId) {
    let options = `<option value="">-- Select Unit --</option>`;
    unitMasterList.forEach(u => {
        options += `
                <option value="${u.Id}" ${u.Id == selectedUnitId ? "selected" : ""}>
                    ${u.UnitName}
                </option>
            `;
    });
    return options;
}

//Prepare Order & Formula -- END ---------------------------------------------

// GenerateWithoutPrice & GenerateWithPrice
function showReportOptions() {

    Swal.fire({
        title: "Select Report Type",
        icon: "info",
        showDenyButton: true,
        showCancelButton: true,
        confirmButtonText: "With Price",
        denyButtonText: "Without Price",
        cancelButtonText: "Close",

        confirmButtonColor: "#16a34a",   // green (professional success)
        denyButtonColor: "#f59e0b",      // amber (neutral warning)
        cancelButtonColor: "#6b7280",     // gray

        customClass: {
            confirmButton: "text-white",
            denyButton: "text-dark",
            cancelButton: "text-white"
        }
    }).then((result) => {

        if (result.isConfirmed) {
            generateReport(true);  // With Price
        }
        else if (result.isDenied) {
            generateReport(false); // Without Price
        }

    });
}

function generateReport(isWithPrice) {
    let bookingId = getBookingId();
    var payload = {
        BookingId: bookingId,
        IsWithPrice: isWithPrice,
        WithHeader : true,
        ActionBy: Number(sessionStorage.getItem("userId") || 0)
    };
    //console.log("payload PackageForm:", payload);
    var url = "/api/Report/Report_FunctionProspectusReport";
    downloadReport(url, payload, "Generating Report...");
}

// 🧠 Helper to get the modal booking ID
function getBookingId() {
    return parseInt($("#bookingReport_BookingId").val() || 0);
}

// Fill Payment Trail dynamically
function fillPaymentTrailTable(paymentList = []) {
    const tbody = $("#bookingReport_PaymentTrailBody");
    tbody.empty(); // Clear old data

    if (!paymentList || paymentList.length === 0) {
        tbody.append(`
                <tr>
                    <td colspan="13" class="text-muted small fst-italic text-center">
                        No payment records found
                    </td>
                </tr>
            `);
        return;
    }

    //console.log("paymentList:", paymentList);

    paymentList.forEach((item, index) => {
        tbody.append(`
                <tr>
                    <td>${index + 1}</td>
                    <td><span class="fw-semibold text-dark">#${item.PaymentId || "—"}</span></td>
                    <td><span class="badge bg-success-subtle text-success border">${item.ModeName || "—"}</span></td>
                    <td>${item.DateOfPayment2 || "—"}</td>
                    <td>${item.TxnId || "—"}</td>
                    <td>${item.ChequeBank || "—"}</td>
                    <td>${item.ChequeNo || "—"}</td>
                    <td>${item.ChequeDate2 || "—"}</td>
                    <td class="text-success fw-semibold">₹${formatINR(item.Amount) || "0"}</td>
                    <td class="text-danger fw-semibold">₹${formatINR(item.DueBalance) || "0"}</td>
                    <td class="text-muted small">
                        ${
                            item.PaymentNotes
                                ? `<span class="text-primary cursor-pointer" title="${item.PaymentNotes}">
                                    View Notes
                                </span>`
                                : "—"
                        }
                    </td>
                    <td>
                        <button class="btn btn-light btn-sm border btn-payment-download" title="View Receipt" data-id="${item.PaymentId}">
                            <i class="fa-solid fa-file-invoice text-primary"></i>
                        </button>
                    </td>
                </tr>
            `);
    });
}

function setGSTSwitch(isGSTAccepted) {
    const gstSwitch = document.getElementById("gstSwitch");
    const gstText = document.getElementById("gstSwitchText");

    gstSwitch.checked = !!isGSTAccepted;

    if (gstSwitch.checked) {
        gstText.textContent = "On";
        gstText.classList.remove("text-danger");
        gstText.classList.add("text-success");
    } else {
        gstText.textContent = "Off";
        gstText.classList.remove("text-success");
        gstText.classList.add("text-danger");
    }
}

function loadBookingCalendar(bookingId = 0, banquetId = 0, eventDate = null, actionMode = "fullData") {
    var requestData = {
        BookingId: Number(bookingId),
        BanquetId: Number(banquetId),
        EventDate: eventDate,
        YearId: Number(selectedYearId) ?? 0,
        MonthId: Number(selectedMonthId) ?? 0
    };
    //console.log("GetBookingCalendar requestData:", requestData);
    $.ajax({
        url: "/api/BookingCalendar/GetBookingCalendar",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.status === "Success") {
                var data = response.data;
                //console.log("Booking List:",data);
                if (actionMode === "fullData") {
                    //console.log("fullData - Booking API Response:", data);
                    fetchHallDetails(data);
                }
                else if (actionMode === "singleData") {
                    resetBooking();
                    data = data[0];
                    //console.log("singleData - Booking API Response:", data);
                    $("#bookingReport_BookingId").val(bookingId);
                    //Client Details
                    $("#bookingReport_clientName").text(data.Client_Name || "—");
                    $("#bookingReport_clientContact").text(data.Client_Contact || "—");
                    $("#bookingReport_clientGSTIN").text(data.Client_GSTIN || "—");
                    $("#bookingReport_clientEmail").text(data.Client_Email || "—");
                    $("#bookingReport_clientBookedBy").text(data.Booked_By_EmpName || "—");
                    $("#bookingReport_internalOfficeNote").html(data.OtherCharges_InternalOfficeNote && data.OtherCharges_InternalOfficeNote.trim() !== ""
                        ? data.OtherCharges_InternalOfficeNote
                        : "— No internal notes available —"
                    );

                    $("#bookingReport_bookingNoteCustomer").html(data.OtherCharges_BookingNoteForCustomer && data.OtherCharges_BookingNoteForCustomer.trim() !== ""
                        ? data.OtherCharges_BookingNoteForCustomer
                        : "— No customer booking notes available —"
                    );
                    $("#VendorWorkOrder_Count").text(data.VendorWorkOrder_Count);

                    //Booking Details
                    $("#bookingReport_BookingRef").text(bookingId || "—");
                    //$("#bookingReport_Invoice").text("#" + data.InvoiceNo);
                    if (data.InvoiceNo && data.InvoiceNo !== 0) {
                        $("#bookingReport_Invoice")
                            .removeClass()
                            .addClass("badge bg-success")
                            .text("#" + data.InvoiceNo);
                    } else {
                        $("#bookingReport_Invoice")
                            .removeClass()
                            .addClass("badge bg-secondary")
                            .text("Invoice Pending");
                    }
                    $("#bookingReport_EventDate").text(data.FromDate_Full || "—");
                    $("#bookingReport_TillDate").text(data.TillDate_Full || "—");

                    const start = data.StartTime_Full ?? "-";
                    const end = data.EndTime_Full ?? "-";
                    $("#bookingReport_EventTime").text(`${start} - ${end}`);

                    $("#bookingReport_Slot").text(`${data.ShiftCode} (${data.ShiftFullName})` || "—");
                    $("#bookingReport_BookedOn").text(data.Booked_On || "—");
                    $("#bookingReport_EventName").text(data.Booking_EventName || "—");
                    $("#bookingReport_SubEventName").text(data.Booking_SubEventName || "—");
                    $("#bookingReport_Banquet").text(data.BanquetPropertyName || "—");
                    $("#bookingReport_Hall").text(data.BanquetName || "—");
                    $("#bookingReport_Pax").text(data.Booking_PAX || "—");

                    vendorWorkOrder_eventDate = data.EventDate;
                    vendorWorkOrder_eventTime = data.StartTime;
                    vendorWorkOrder_slotId = data.SlotId;

                    // Services & Charges
                    $("#bookingReport_HallRent").text(data.Booking_HallRent || "0");
                    $("#bookingReport_Decoraton").text((data.OtherCharges_Decoration_Amount + data.OtherCharges_Decoration_GST) || "0");
                    $("#bookingReport_RoomCharges").text((data.OtherCharges_RoomCharges_Amount + data.OtherCharges_RoomCharges_GST) || "0");
                    $("#bookingReport_SoundSystem").text((data.OtherCharges_SoundSystem_Amount + data.OtherCharges_SoundSystem_GST) || "0");
                    $("#bookingReport_ProjectorScreen").text((data.OtherCharges_ProjectorScreen_Amount + data.OtherCharges_ProjectorScreen_GST) || "0");

                    // Payment Trail
                    $("#bookingReport_DealAmount").text(data.PaymentSummary_Total || "0");
                    var taxAmount = (data.PaymentSummary_CGST + data.PaymentSummary_SGST + data.PaymentSummary_IGST);
                    $("#bookingReport_TaxAmount").text(taxAmount || "0");
                    $("#bookingReport_BookingDiscount").text(data.PaymentSummary_BookingDiscount_Total || "0");
                    $("#bookingReport_TotalAmount").text(data.PaymentSummary_PayableAmount || "0");

                    let amount = parseFloat(data.Payment_Amount) || 0;
                    $("#bookingReport_PaidTillDate").text(amount).toggleClass("text-success", amount > 0);

                    $("#bookingReport_OutstandingAmount").text((data.PaymentSummary_PayableAmount - data.Payment_Amount) || "0");

                    //Payment Details Table
                    fillPaymentTrailTable(data.PaymentTrailList);

                    // Food Package and Selected Menu
                    const packages = data?.JSON_FoodPackage ?? [];
                    //const foodPackageText = packages.length === 0
                    //    ? "—"
                    //    : packages.map(p => p.FoodPackageName).join(", ");
                    //$("#bookingReport_FoodPackageName").text(foodPackageText);
                    renderFoodPackages(packages);

                    $("#bookingReport_FoodPackageUpdatedBy").text(data?.Booked_By_EmpName || "—");
                    $("#bookingReport_FoodPackageUpdatedOn").text(data?.Booked_On || "—");
                    //console.log("summary data:", data);
                    $("#bookingReport_ReachedUs").text(data?.ReachedUs || "—");
                    $("#bookingReport_Reference").text(data?.Reference || "—");

                    //data.InvoiceNo, data.ProformaInvoiceNo
                    setInvoiceButtons(data);
                    //Open REPORT MODAL
                    //console.log("bookingReportModal show");
                    bookingReportModal.show();
                }
                else if (actionMode === "editData") {
                    resetBooking();
                    data = data[0];
                    //console.log("editData - Booking API Response:", data);
                    $("#BookingId").val(bookingId);
                    // 🟢 Update label content & attributes - using jQuery
                    $("#label_date_shift")
                        .text(`${data.EventDate_Format}, ${fullShiftName(data.ShiftCode)} Slot`)        // visible text
                        .data("eventdate", data.EventDate)
                        .data("shiftid", data.ShiftCode);

                    $("#gstSwitch").prop("checked", !!data.GSTAccepted).trigger("change");

                    $("#banquetDropdown").val(data.BanquetId);
                    $("#fromDate").val(data.FromDate);
                    $("#toDate").val(data.TillDate);
                    setTimeFrom24Hr(data.StartTime, "startHour", "startMinute", "startAMPM");
                    setTimeFrom24Hr(data.EndTime, "endHour", "endMinute", "endAMPM");

                    $("#clientEnquiryDropdown").val(data.Client_EnquiryId);
                    $("#clientName").val(data.Client_Name);
                    $("#clientAddress").val(data.Client_Address);
                    $("#clientEmail").val(data.Client_Email);
                    $("#clientContact").val(data.Client_Contact);
                    $("#clientGSTIN").val(data.Client_GSTIN);
                    if (data.Client_StateId > 0) {
                        $("#clientStateDropdown").val(data.Client_StateId);
                    }

                    $("#booking_eventDropdown_div").hide();
                    $("#booking_subEventDropdown").val(data.Booking_SubEventId);
                    $("#booking_pax").val(data.Booking_PAX);
                    $("#booking_hallRent").val(data.Booking_HallRent);
                    $("#booking_gst").val(data.Booking_GST);
                    //$("#booking_leadReferenceDropdown").val(data.Booking_LeadReferenceId);
                    loadLeadSourceList(data.Booking_LeadReferenceId);

                    //menu type radio
                    $("input[name='menuType']").prop("checked", false);
                    $("input[name='menuType'][value='" + data.Booking_MenuType_Id + "']").prop("checked", true).trigger("change");

                    const foodPackages = data.JSON_FoodPackage;
                    if (!Array.isArray(foodPackages) || foodPackages.length === 0) return;
                    //console.log("foodPackages:", foodPackages);
                    foodPackages.forEach((pkg, index) => {
                        if (index === 0) {
                            bindStaticFoodRow(pkg);
                        }
                        else {
                            const newBlock = createFoodBlock(); // custom function (below)
                            bindDynamicFoodRow(newBlock, pkg);
                        }
                    });

                    //othercharges
                    $("#OtherCharges_Decoration_Amount").val(data.OtherCharges_Decoration_Amount);
                    $("#OtherCharges_Decoration_GST").val(data.OtherCharges_Decoration_GST);

                    $("#OtherCharges_RoomCharges_Amount").val(data.OtherCharges_RoomCharges_Amount);
                    $("#OtherCharges_RoomCharges_GST").val(data.OtherCharges_RoomCharges_GST);

                    $("#OtherCharges_SoundSystem_Amount").val(data.OtherCharges_SoundSystem_Amount);
                    $("#OtherCharges_SoundSystem_GST").val(data.OtherCharges_SoundSystem_GST);

                    $("#OtherCharges_ProjectorScreen_Amount").val(data.OtherCharges_ProjectorScreen_Amount);
                    $("#OtherCharges_ProjectorScreen_GST").val(data.OtherCharges_ProjectorScreen_GST);

                    notesData = data; // store once

                    //payment summary
                    $("#paymentSummary_TaxableAmount").text(formatAmount(data.PaymentSummary_TaxableAmount));
                    $("#paymentSummary_CGST").text(formatAmount(data.PaymentSummary_CGST));
                    $("#paymentSummary_SGST").text(formatAmount(data.PaymentSummary_SGST));
                    $("#paymentSummary_IGST").text(formatAmount(data.PaymentSummary_IGST));
                    $("#paymentSummary_Total").text(formatAmount(data.PaymentSummary_TotalAmount));
                    // Booking Amount Type & Input
                    $("#paymentSummary_BookingDiscount_Type").val(data.PaymentSummary_BookingDiscount_Type_Id || 1).trigger("change");
                    $("#paymentSummary_BookingDiscount_Amount").val(data.PaymentSummary_BookingDiscount_Type_Amount || 0).trigger("input");
                    // Booking Amount Calculated
                    $("#paymentSummary_BookingAmount").text(formatAmount(data.PaymentSummary_BookingDiscount_Total));
                    // Payable Amount
                    $("#paymentSummary_payableAmount").text(formatAmount(data.PaymentSummary_PayableAmount));
                    
                    $("#PaymentDetails_Mode").val(data.Payment_ModeId).trigger("change");
                    $("#PaymentDetails_AccountId").val(data.PaymentDetails_AccountId);
                    if (data.Payment_PaidTime !== "" && data.Payment_PaidTime !== null) {
                        $("#PaymentDetails_PaidTime").val(data.Payment_PaidTime);
                    }
                    $("#PaymentDetails_Amount").val(data.Payment_Amount);
                    $("#PaymentDetails_TransactionId").val(data.PaymentDetails_TransactionId);
                    $("#PaymentDetails_ChequeBank").val(data.PaymentDetails_ChequeBank);
                    $("#PaymentDetails_ChequeNo").val(data.PaymentDetails_ChequeNo);
                    $("#PaymentDetails_ChequeDate").val(data.PaymentDetails_ChequeDate);
                    $("#PaymentDetails_DueDate").val(data.Payment_DueDate);
                    $("#PaymentDetails_PaymentNote").val(data.Payment_Note);

                    $("#BookingType").val(data.Booking_TypeId ?? "");
                    $("#AgentCheck").prop("checked", data.Booking_IsAgent);
                    //$("#fromDate, #startHour, #startMinute, #startAMPM, #endHour, #endMinute, #endAMPM, #banquetDropdown").trigger("change");

                    //$("#Section_PaymentDetails").hide();
                }
                else if (actionMode === "rescheduleData") {
                    //resetBooking();
                    data = data[0];
                    //console.log("rescheduleData - Booking API Response:", data);

                    /* ------------------------------------Hidden Booking Id--------------------------------- */
                    $("#reschedule_BookingId").val(bookingId);

                    /* ------------------------------------Client Details (Read-only)------------------------ */
                    $("#reschedule_clientName").text(data.Client_Name || "—");
                    $("#reschedule_clientContact").text(data.Client_Contact || "—");
                    $("#reschedule_clientEmail").text(data.Client_Email || "—");
                    $("#reschedule_bookedBy").text(data.Booked_By_EmpName || "—");

                    /* ------------------------------------Banquet & Slot------------------------------------ */
                    $("#reschedule_BanquetId").val(data.BanquetId).trigger("change");
                    $("#reschedule_SlotId").val(data.SlotId).trigger("change");

                    /* ------------------------------------Dates-------------------------------------------- */
                    $("#reschedule_FromDate").val(data.FromDate);     // readonly
                    $("#reschedule_TillDate").val(data.TillDate);

                    /* ------------------------------------Time--------------------------------------------- */
                    setTimeFrom24Hr(data.StartTime, "reschedule_startHour", "reschedule_startMinute", "reschedule_startAMPM");
                    setTimeFrom24Hr(data.EndTime, "reschedule_endHour", "reschedule_endMinute", "reschedule_endAMPM");
                }
            }
            else {
                console.warn("API returned error:", response.message);
                Swal.fire({
                    icon: 'warning',
                    title: 'No Data Found',
                    text: response.message || 'No bookings available.',
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Booking fetch fail")
        }
    });
}
//Bookimg Summary - Package DATA -- START ------------------
function renderFoodPackages(packages) {
    let html_services = "";

    if (!packages || packages.length === 0) {
        $("#bookingReport_FoodPackageName").html("—");

        html_services = `
            <div class="d-flex justify-content-between py-1 border-bottom">
                <span class="fw-semibold">Food</span>
                <span>₹0</span>
            </div>
        `;
        $("#foodPackageContainer_summaryservices").html(html_services);
        return;
    }
    $("#btnFoodLabels").data("foodpackage", JSON.stringify(packages));

    let html = "";
    packages.forEach((pkg, index) => {

        const perPlate = pkg.PerPlate ?? "—";
        const pax = pkg.Pax ?? 0;
        const addon = pkg.Addons ?? 0;
        const total = pkg.TotalCharges ?? 0;
        html_services += `
            <div class="d-flex justify-content-between py-1 border-bottom ${index === 0 ? '' : 'mt-1'}">
                
                <span class="fw-semibold">
                    ${`Food Package ${index + 1}`}
                    
                    <small class="text-muted">
                        (₹${perPlate}/Plate)
                        ${pax > 0 ? `(Pax: ${pax})` : ``}
                        ${addon > 0 ? `(AddOn: ₹${addon})` : ``}
                    </small>
                </span>

                <span>₹${total}</span>
            </div>
        `;

        // 🔹 Group Menu Items by Header
        const menuGrouped = {};
        (pkg.MenuItemsList || []).forEach(item => {
            if (!menuGrouped[item.MenuHeader_Name]) {
                menuGrouped[item.MenuHeader_Name] = [];
            }
            menuGrouped[item.MenuHeader_Name].push(item.MenuItem_Name);
        });

        // 🔹 Group AddOns by Header
        const addonGrouped = {};
        (pkg.AddOnsList || []).forEach(item => {
            if (!addonGrouped[item.AddOn_Header_Name]) {
                addonGrouped[item.AddOn_Header_Name] = [];
            }
            addonGrouped[item.AddOn_Header_Name].push(item.AddOn_Item_Name);
        });

        html += `
            <div style="margin-bottom:12px; padding:10px; border:1px solid #fecdd3; border-radius:10px; background:#fff1f2;">
                
                <!-- 📦 Package Title -->
                <div style="font-weight:600; color:#9f1239; margin-bottom:6px;">
                    ${pkg.FoodPackageName} <span style="font-size:12px;">(₹${pkg.PerPlate}/Plate)</span>
                </div>
        `;

        // 🍽️ Menu Section
        if (Object.keys(menuGrouped).length > 0) {
            html += `<div style="margin-bottom:6px;">`;
            html += `<div style="font-size:12px; color:#be123c; font-weight:600;">🍽️ Menu</div>`;
            for (let header in menuGrouped) {
                const items = menuGrouped[header]
                    .map((item, index) => `(${index + 1}) ${item}`)
                    .join(", ");

                html += `
                    <div style="font-size:13px; margin-left:6px;">
                        <strong>${header}:</strong> ${items}
                    </div>
                `;
            }
            html += `</div>`;
        }

        // ➕ Addons Section
        if (Object.keys(addonGrouped).length > 0) {
            html += `<div>`;
            html += `<div style="font-size:12px; color:#be123c; font-weight:600;">➕ Add-ons</div>`;
            for (let header in addonGrouped) {

                const items = addonGrouped[header]
                    .map((item, index) => `(${index + 1}) ${item}`)
                    .join(", ");

                html += `
                    <div style="font-size:13px; margin-left:6px;">
                        <strong>${header}:</strong> ${items}
                    </div>
                `;
            }
            html += `</div>`;
        }

        html += `</div>`;
    });
    $("#foodPackageContainer_summaryservices").html(html_services);

    $("#bookingReport_FoodPackageName").html(html);
}
//Bookimg Summary - Package DATA -- END ------------------

function bindStaticFoodRow(pkg) {

    $("#foodPackage_PackageDropdown").val(pkg.FoodPackageId ?? "0"); //.trigger('change');
    $("#foodPackage_RatePerPlate").val(pkg.PerPlate ?? 0);
    $("#foodPackage_PackageCost").val(pkg.PackageCost ?? 0);
    $("#foodPackage_Addons").val(pkg.Addons ?? 0);
    $("#foodPackage_GST").val(pkg.GST ?? 0);
    $("#foodPackage_TotalValue").text(pkg.TotalCharges ?? 0);

    // Addons
    const addOnsList = pkg.AddOnsList || [];
    //console.log("#addOnsList:", addOnsList);
    $("#foodPackage_AddOnItems").data("selected-addonitems", addOnsList);
    updateAddonButton($("#foodPackage_AddOnItems"), addOnsList);

    // Menu
    const menuItemsList = pkg.MenuItemsList || [];
    //console.log("#menuItemsList:", menuItemsList);
    $("#foodPackage_PackageDropdown").data("selected-items", menuItemsList);
}
function createFoodBlock() {
    const newRow = document.createElement('div');
    newRow.className = 'food-block border rounded p-3 mt-3';

    newRow.innerHTML = `
            <!-- Row 1 -->
            <div class="row g-2 align-items-end p-2 mb-1">
                <div class="col-md-4">
                    <label class="form-label">Addon Food Package #${addonCounter}</label>
                    <input type="text" class="form-control form-control-sm" placeholder="Addon Food Package Name">
                </div>
                <div class="col-md-1">
                    <label class="form-label">Pax</label>
                    <input type="number" class="form-control form-control-sm pax" placeholder="Pax" value="0">
                </div>
                <div class="col-md-2">
                    <label class="form-label">Date</label>
                    <input type="date" class="form-control form-control-sm">
                </div>
                <div class="col-md-3">
                    <label class="form-label">Time</label>
                    <div class="d-flex gap-1">
                        <input type="number" class="form-control form-control-sm" placeholder="HH" min="1" max="12" style="width: 70px;">
                        <input type="number" class="form-control form-control-sm" placeholder="MM" min="0" max="59" style="width: 70px;">
                        <select class="form-select form-select-sm" style="width: 90px;">
                            <option value="AM">AM</option>
                            <option value="PM">PM</option>
                        </select>
                    </div>
                </div>
                <div class="col-md-2 d-flex align-items-end justify-content-end">
                    <button type="button" class="btn btn-outline-danger btn-sm remove-btn">&times;</button>
                </div>
            </div>

            <!-- Row 2 -->
            <div class="row g-2 food-row align-items-end mt-1">
                <div class="col-md-3">
                    <label class="form-label">Food Package</label>
                    <div class="d-flex gap-2">
                        <select class="form-select form-select-sm foodPackageDropdown"></select>
                        <button type="button"
                                class="btn btn-outline-primary btn-sm loadMenuBtn">
                            Menu
                        </button>
                    </div>
                    <input type="hidden" class="package_row_id" value="0" />
                </div>
                <div class="col-md-1">
                    <label class="form-label">Per Plate</label>
                    <input type="number" class="form-control form-control-sm perPlate" placeholder="Per Plate" value="0">
                </div>
                <div class="col-md-2">
                    <label class="form-label">Package Cost</label>
                    <input type="number" class="form-control form-control-sm packageCost" placeholder="Package Cost" value="0" readonly>
                </div>
                <div class="col-md-1">
                    <label class="form-label">Addons</label>
                    <input type="number" class="form-control form-control-sm addonsCost" placeholder="Addons" value="0" readonly>
                </div>
                <div class="col-md-1">
                    <label class="form-label">GST</label>
                    <input type="number" class="form-control form-control-sm gstCost" placeholder="GST" value="0" readonly>
                </div>
                <div class="col-md-4 d-flex align-items-center justify-content-between">
                    <button type="button" class="btn btn-outline-success btn-sm mt-2 foodPackage_AddOnItems" onclick="event.preventDefault(); openAddonModal(0, this)"><i class="fa fa-check"></i> Addons</button>
                    <span class="fw-bold">Total: <span class="foodPackage_TotalValue">0</span></span>
                </div>
            </div>
        `;

    foodContainer.appendChild(newRow);

    const dropdown = newRow.querySelector(".foodPackageDropdown");
    populateFoodPackageDropdown($(dropdown));

    newRow.querySelector('.remove-btn').addEventListener('click', () => {
        foodContainer.removeChild(newRow);
    });

    addonCounter++;

    return $(newRow); // ✅ IMPORTANT
}
function bindDynamicFoodRow($block, pkg) {
    const $row = $block.find(".food-row");

    // Row 2 (main values)
    //console.log("pkg.FoodPackageId:", pkg.FoodPackageId);
    $row.find(".foodPackageDropdown").val(pkg.FoodPackageId ?? "0"); //.trigger('change');

    const $dropdown = $row.find(".foodPackageDropdown");
    populateFoodPackageDropdown($dropdown);
    // Delay OR callback
    setTimeout(() => {
        $dropdown.val(pkg.FoodPackageId); //.trigger('change');
    }, 0);

    $row.find(".perPlate").val(pkg.PerPlate ?? 0);
    $row.find(".packageCost").val(pkg.PackageCost ?? 0);
    $row.find(".addonsCost").val(pkg.Addons ?? 0);
    $row.find(".gstCost").val(pkg.GST ?? 0);
    $row.find(".foodPackage_TotalValue").text(pkg.TotalCharges ?? 0);

    // Addons
    const addOnsList = pkg.AddOnsList || [];
    console.log(".addOnsList:", addOnsList);
    const $addonBtn = $row.find(".foodPackage_AddOnItems");
    $addonBtn.data("selected-addonitems", addOnsList);
    updateAddonButton($addonBtn, addOnsList);

    // Menu
    const menuItemsList = pkg.MenuItemsList || [];
    //console.log(".menuItemsList:", menuItemsList);
    $row.find(".foodPackageDropdown").data("selected-items", menuItemsList);

    // Row 1 (extra fields)
    $block.find('input[placeholder="Addon Food Package Name"]').val(pkg.FoodPackageTitleName || '');
    $block.find('.pax').val(pkg.Pax || 0);

    if (pkg.FoodPackageDateTime) {
        const { date, hh, mm, ampm } = parseDateTime_pkg(pkg.FoodPackageDateTime);

        $block.find('input[type="date"]').val(date);
        $block.find('input[placeholder="HH"]').val(hh);
        $block.find('input[placeholder="MM"]').val(mm);
        $block.find('select').val(ampm);
    }
}
function updateAddonButton($btn, list) {
    if (list.length > 0) {
        $btn.html(`<i class="fa fa-check text-success"></i> Addons (${list.length})`);
    } else {
        $btn.html(`<i class="fa fa-check"></i> Addons`);
    }
}
function parseDateTime_pkg(dateTimeStr) {
    if (!dateTimeStr) return {};

    const parts = dateTimeStr.split(' ');

    return {
        date: parts[0] || '',
        hh: parts[1]?.split(':')[0] || '',
        mm: parts[1]?.split(':')[1] || '',
        ampm: parts[2] || ''
    };
}



function setInvoiceButtons(data) {
    hasInvoice = (data.InvoiceNo ?? 0) > 0;
    const hasProforma = (data.ProformaInvoiceNo ?? 0) > 0;

    // ================= Invoice Text =================
    if (hasInvoice) {
        document.querySelector("#btnInvoiceGenerate .btn-text").textContent =
            `Invoice #${data.InvoiceNo}`;
    }
    else {
        document.querySelector("#btnInvoiceGenerate .btn-text").textContent =
            `Invoice`;
    }

    // ================= Proforma Text =================
    if (hasProforma) {
        document.querySelector("#btnProformaInvoiceGenerate .btn-text").textContent =
            `Proforma #${data.ProformaInvoiceNo}`;
    } 
    else {
        document.querySelector("#btnProformaInvoiceGenerate .btn-text").textContent =
            `Proforma Invoice`;
    }

    // ================= Disable Buttons Logic =================
    const shouldDisable = hasInvoice; // || hasProforma;

    const buttonsToDisable = [
        "#btnFoodLabels",
        "#btnVendorWorkOrders",
        "#btnEdit",
        "#btnReschedule",
        "#btnPostBookingFollowups",
        "#btnPrepareOrder",
        "#btnExportBooking",
        "#btnCancelBooking",
        "#btnDeleteBooking"
    ];

    buttonsToDisable.forEach(selector => {
        const btn = document.querySelector(selector);
        if (btn) {
            btn.disabled = shouldDisable;

            // Optional: visual style
            btn.classList.toggle("opacity-50", shouldDisable);
        }
    });

    //DISABLE Invoice Generate button ---START
    const eventDate = new Date(data.EventDate);
    const today = new Date();
    // Normalize time (important to avoid time mismatch bugs)
    eventDate.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);
    const isBeforeEvent = today < eventDate;

    const $btn = $('#btnInvoiceGenerate');
    if (isBeforeEvent) {
        $btn.prop('disabled', true);
        $btn.attr('title', 'You can create Invoice only on or after the event date.');
    }
    else {
        $btn.prop('disabled', false);
        $btn.removeAttr('title');
    }
    //DISABLE Invoice Generate button ---END

}

function setTimeFrom24Hr(time24, hourId, minuteId, ampmId) {
    if (!time24) return;

    let [hh, mm] = time24.split(":").map(Number);

    const ampm = hh >= 12 ? "PM" : "AM";
    hh = hh % 12 || 12;

    // 🔹 Pad with leading zero
    const hourStr = hh.toString().padStart(2, "0");
    const minuteStr = mm.toString().padStart(2, "0");

    $("#" + hourId).val(hourStr);
    $("#" + minuteId).val(minuteStr);
    $("#" + ampmId).val(ampm);
}

function formatAmount(val) {
    return Number(val || 0).toFixed(2);
}

function validateBookingForm() {

    clearAllErrors();
    let isValid = true;

    /* ================= BASIC BOOKING ================= */

    if (!$("#fromDate").val()) {
        showFieldError($("#fromDate"), "Please select from date");
        isValid = false;
    }

    if (!$("#toDate").val()) {
        showFieldError($("#toDate"), "Please select to date");
        isValid = false;
    }

    if (!$("#startHour").val() || !$("#startMinute").val()) {
        showFieldError($("#startHour"), "Please select start time");
        isValid = false;
    }

    if (!$("#endHour").val() || !$("#endMinute").val()) {
        showFieldError($("#endHour"), "Please select end time");
        isValid = false;
    }

    if (!$("#clientName").val().trim()) {
        showFieldError($("#clientName"), "Please enter client name");
        isValid = false;
    }

    let contact = $("#clientContact").val().trim();
    if (!contact || contact.length !== 10 || !/^\d+$/.test(contact)) {
        showFieldError($("#clientContact"), "Enter valid 10 digit contact");
        isValid = false;
    }

    if (parseInt($("#booking_subEventDropdown").val()) <= 0) {
        showFieldError($("#booking_subEventDropdown"), "Please select event");
        isValid = false;
    }

    if (parseInt($("#booking_pax").val()) <= 0) {
        showFieldError($("#booking_pax"), "Please enter valid PAX");
        isValid = false;
    }

    if (parseInt($("#booking_leadReferenceDropdown").val()) <= 0) {
        showFieldError($("#booking_leadReferenceDropdown"), "Please select reference type");
        isValid = false;
    }


    /* ================= PAYMENT VALIDATION ================= */

    let paymentAmount = parseFloat($("#PaymentDetails_Amount").val()) || 0;
    let paymentMode = parseInt($("#PaymentDetails_Mode").val()) || 0;
    let paymentAccount = parseInt($("#PaymentDetails_AccountId").val()) || 0;

    if (paymentAmount > 0) {

        if (paymentMode === 0) {
            showFieldError($("#PaymentDetails_Mode"), "Select payment mode");
            isValid = false;
        }

        if (paymentAccount === 0) {
            showFieldError($("#PaymentDetails_AccountId"), "Select account");
            isValid = false;
        }
    }

    return isValid;
}
function getCleanHtml(editor) {
    if (!editor) return "";

    let html = editor.root.innerHTML
        .replace(/\s+/g, " ")
        .replace(/<p><br><\/p>/gi, "")
        .replace(/<p><\/p>/gi, "")
        .trim();

    return html;
}
function saveBooking() {
    if (!validateBookingForm()) {
        return;
    }
    //const form = $("#bookingForm");
    const $label = $("#label_date_shift");

    let userId = parseInt(sessionStorage.getItem('userId') ? sessionStorage.getItem('userId') : 0);

    // ✅ Use existing editors (DO NOT re-init)
    var customerNote = getCleanHtml(editors["OtherCharges_customerEditor"]);
    var internalNote = getCleanHtml(editors["OtherCharges_internalEditor"]);

    var notes_FoodBeverage = getCleanHtml(editors["Notes_FoodBeverage_customerEditor"]);
    var notes_EventTiming = getCleanHtml(editors["Notes_EventTiming_customerEditor"]);
    var notes_DecorationSetup = getCleanHtml(editors["Notes_DecorationSetup_customerEditor"]);
    var notes_TechnicalService = getCleanHtml(editors["Notes_TechnicalService_customerEditor"]);
    var notes_GuestCustom = getCleanHtml(editors["Notes_GuestCustom_customerEditor"]);
    var notes_FinancialInternal = getCleanHtml(editors["Notes_FinancialInternal_internalEditor"]);


    // 🔹 1. Collect Booking Data
    const bookingData = {
        BookingId: parseInt($("#BookingId").val() || 0),
        EventDate: $label.data("eventdate"),
        ShiftCode: $label.data("shiftid"),
        GSTAccepted: $("#gstSwitch").is(":checked"),

        //Dates
        BanquetId: parseInt($("#banquetDropdown").val() || 0),
        FromDate: $("#fromDate").val(),
        TillDate: $("#toDate").val(),

        //Time
        StartTime: `${$("#startHour").val()}:${$("#startMinute").val()} ${$("#startAMPM").val()}`,
        EndTime: `${$("#endHour").val()}:${$("#endMinute").val()} ${$("#endAMPM").val()}`,

        // Client Info
        Client_EnquiryId: parseInt($("#clientEnquiryDropdown").val() || 0),
        Client_Name: $("#clientName").val(),
        Client_Email: $("#clientEmail").val(),
        Client_Address: $("#clientAddress").val(),
        Client_Contact: $("#clientContact").val(),
        Client_GSTIN: $("#clientGSTIN").val(),
        Client_StateId: parseInt($("#clientStateDropdown").val() || 0),

        // Booking Details
        Booking_SubEventId: parseInt($("#booking_subEventDropdown").val() || 0),
        Booking_PAX: parseInt($("#booking_pax").val() || 0),
        Booking_HallRent: parseInt($("#booking_hallRent").val() || 0),
        Booking_GST: parseFloat($("#booking_gst").val() || 0),
        Booking_LeadReferenceId: parseInt($("#booking_leadReferenceDropdown").val() || 0),
        Booking_MenuType_Id: parseInt($("input[name='menuType']:checked").val() || 0),
        Booking_TypeId: parseInt($("#BookingType").val() || 0),
        Booking_IsAgent: $("#AgentCheck").is(":checked"),

        //Payment
        Payment_ModeId: parseInt($("#PaymentDetails_Mode").val() || 0),
        PaymentDetails_AccountId: parseInt($("#PaymentDetails_AccountId").val() || 0),
        PaymentDetails_TransactionId: $("#PaymentDetails_TransactionId").val(),
        PaymentDetails_ChequeBank: $("#PaymentDetails_ChequeBank").val(),
        PaymentDetails_ChequeNo: $("#PaymentDetails_ChequeNo").val(),
        PaymentDetails_ChequeDate: $("#PaymentDetails_ChequeDate").val(),
        Payment_Amount: parseFloat($("#PaymentDetails_Amount").val()) || 0,
        Payment_PaidTime: $("#PaymentDetails_PaidTime").val(),
        Payment_DueDate: $("#PaymentDetails_DueDate").val(),
        Payment_Note: $("#PaymentDetails_PaymentNote").val(),

        // ✅ Other Charges
        OtherCharges_Decoration_Amount: parseFloat($("#OtherCharges_Decoration_Amount").val() || 0),
        OtherCharges_Decoration_GST: parseFloat($("#OtherCharges_Decoration_GST").val() || 0),
        OtherCharges_RoomCharges_Amount: parseFloat($("#OtherCharges_RoomCharges_Amount").val() || 0),
        OtherCharges_RoomCharges_GST: parseFloat($("#OtherCharges_RoomCharges_GST").val() || 0),
        OtherCharges_SoundSystem_Amount: parseFloat($("#OtherCharges_SoundSystem_Amount").val() || 0),
        OtherCharges_SoundSystem_GST: parseFloat($("#OtherCharges_SoundSystem_GST").val() || 0),
        OtherCharges_ProjectorScreen_Amount: parseFloat($("#OtherCharges_ProjectorScreen_Amount").val() || 0),
        OtherCharges_ProjectorScreen_GST: parseFloat($("#OtherCharges_ProjectorScreen_GST").val() || 0),

        OtherCharges_BookingNoteForCustomer: customerNote,
        OtherCharges_InternalOfficeNote: internalNote,
        Notes_FoodBeverage: notes_FoodBeverage,
        Notes_EventTiming: notes_EventTiming,
        Notes_DecorationSetup: notes_DecorationSetup,
        Notes_TechnicalService: notes_TechnicalService,
        Notes_GuestCustom: notes_GuestCustom,
        Notes_FinancialInternal: notes_FinancialInternal,

        // ✅ Payment Summary
        PaymentSummary_TaxableAmount: parseFloat($("#paymentSummary_TaxableAmount").text() || 0),
        PaymentSummary_CGST: parseFloat($("#paymentSummary_CGST").text() || 0),
        PaymentSummary_SGST: parseFloat($("#paymentSummary_SGST").text() || 0),
        PaymentSummary_IGST: parseFloat($("#paymentSummary_IGST").text() || 0),
        PaymentSummary_Total: parseFloat($("#paymentSummary_Total").text() || 0),
        PaymentSummary_BookingDiscount_Type_Id: parseInt($("#paymentSummary_BookingDiscount_Type").val() || 0),
        PaymentSummary_BookingDiscount_Type_Amount: parseFloat($("#paymentSummary_BookingDiscount_Amount").val() || 0),
        PaymentSummary_BookingDiscount_Total: parseFloat($("#paymentSummary_BookingAmount").text() || 0),
        PaymentSummary_PayableAmount: parseFloat($("#paymentSummary_payableAmount").text() || 0),

        ActionBy: userId
    };

    // 🔹 2. Build Food Packages JSON --here
    const foodPackages = [];
    // 1. FIRST ROW (static)
    $("#foodPackageContainer > .food-row").each(function () {
        const $row = $(this);
        foodPackages.push(buildFoodPackageObject($row, false));
    });

    // 2. DYNAMIC ROWS
    $("#foodPackageContainer > .food-block").each(function () {
        const $block = $(this);
        const $row = $block.find(".food-row"); // important
        foodPackages.push(buildFoodPackageObject($row, true, $block));
    });
    // 🔹 3. Attach as JSON string
    bookingData.JSON_FoodPackage = foodPackages;

    console.log("Final Booking JSON:", bookingData);

    // 🔹 4. AJAX POST
    $.ajax({
        url: "/api/BookingCalendar/SaveBookingCalendar",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(bookingData),
        beforeSend: function () {
            $("#btnSaveBooking").prop("disabled", true).text("Saving...");
        },
        success: function (response) {
            //console.log("save response:", response);
            if (response.status === "Success") {
                handleApiSuccess("Booking Saved.");

                var paymentId = response.data.PaymentId;
                if (paymentId > 0 && bookingData.BookingId == 0) {
                    // ✅ Swal Confirmation
                    Swal.fire({
                        title: 'Download Receipt?',
                        text: 'Do you want to download the payment receipt now?',
                        icon: 'question',
                        showCancelButton: true,
                        confirmButtonText: 'Yes, Download',
                        cancelButtonText: 'No',
                        confirmButtonColor: '#3085d6',
                        cancelButtonColor: '#d33'
                    }).then((result) => {
                        var payload = {
                            PaymentId: paymentId,
                            BookingId: response.data.BookingId,
                            ActionBy: userId
                        }
                        var url = "/api/Booking_Payments/DownloadPaymentReceipt";
                        if (result.isConfirmed) {
                            // ✅ Call download only if YES
                            downloadReport(url, payload, "Generating Receipt...");
                        }
                        // ❌ If NO → do nothing (flow continues)
                                                
                        //CLOSE MODAL
                        //bookingModal.hide();
                        $("#bookingModal").modal("hide");
                        if (bookingData.BookingId > 0) {
                            //console.log("singleData");
                            openedFromReport = false;
                            bookingReportModal.hide();
                            loadBookingCalendar(bookingData.BookingId, 0, null, "singleData");
                        }
                        else {
                            resetBooking();
                            loadBookingCalendar();
                        }
                    });
                }
                else {
                    resetBooking();
                    loadBookingCalendar();
                    //CLOSE MODAL
                    //bookingModal.hide();
                    $("#bookingModal").modal("hide");

                    openedFromReport = false;
                    invoiceModal.hide();
                    bookingReportModal.hide();
                    loadBookingCalendar(bookingData.BookingId, 0, null, "singleData");
                }
            }
            else {
                Swal.fire({
                    title: "Failed",
                    text: response.message || "Booking Save",
                    icon: "error"
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Booking Save Error");
        },
        complete: function () {
            $("#btnSaveBooking").prop("disabled", false).text("Save Booking");
        }
    });
}
function buildFoodPackageObject($row, isDynamic, $block = null) {

    const $dropdown = $row.find(".foodPackageDropdown");
    const selectedMenuItems = $dropdown.data("selected-items") || [];

    const $addonBtn = $row.find(".foodPackage_AddOnItems");
    const selectedAddOnCharges = $addonBtn.data("selected-addonitems") || [];

    const foodPackage = {
        Id: parseInt($row.find(".package_row_id").val() || 0),
        PackageId: parseInt($dropdown.val() || 0),
        PerPlate: parseFloat($row.find(".perPlate, #foodPackage_RatePerPlate").val() || 0),
        PackageCost: parseFloat($row.find(".packageCost, #foodPackage_PackageCost").val() || 0),
        AddonsCost: parseFloat($row.find(".addonsCost, #foodPackage_Addons").val() || 0),
        GST: parseFloat($row.find(".gstCost, #foodPackage_GST").val() || 0),
        TotalCharges: parseFloat($row.find(".foodPackage_TotalValue").text() || 0),
        MenuItems: selectedMenuItems,
        AddOns: selectedAddOnCharges
    };

    if (isDynamic && $block) {
        const name = $block.find('input[placeholder="Addon Food Package Name"]').val() || '';
        const pax = $block.find('.pax').val() || 0;
        const date = $block.find('input[type="date"]').val();

        const hour = $block.find('input[placeholder="HH"]').val() || '12';
        const minute = $block.find('input[placeholder="MM"]').val() || '00';
        const ampm = $block.find('select').val() || 'AM';

        foodPackage.FoodPackageTitleName = name;
        foodPackage.Pax = pax;
        foodPackage.FoodPackageDateTime = date ? `${date} ${hour}:${minute} ${ampm}` : '';
    }
    else {
        foodPackage.FoodPackageTitleName = '';
        foodPackage.Pax = 0;
        foodPackage.FoodPackageDateTime = '';
    }

    return foodPackage;
}

function showFieldError($field, message) {
    $field.addClass("is-invalid");
    $field.closest(".col-md-2, .col-md-3, .col-md-5")
        .find(".error-msg")
        .removeClass("d-none")
        .text(message);
}

function clearFieldError($field) {
    $field.removeClass("is-invalid");
    $field.closest(".col-md-2, .col-md-3, .col-md-5")
        .find(".error-msg")
        .addClass("d-none");
}

function clearAllErrors() {
    $(".error-msg").addClass("d-none");
    $(".form-control, .form-select").removeClass("is-invalid");
}

function resetBooking() {
    clearAllErrors();
    // Reset all form fields
    $("#bookingForm").trigger("reset");
    $("#label_date_shift")
        .text("")                     // clear visible text
        .data("eventdate", null)      // clear stored date
        .data("shiftid", null);       // clear stored shift

    // Reset any special fields
    $("#gstSwitch").prop("checked", false);
    $("#gstSwitchText").text("Off").removeClass("text-success").addClass("text-danger");  // Red for OFF
    $("#foodPackageContainer .foodPackage_TotalValue").text("0");
    $("#addonItemsContainer").empty();
    $("#headerSummaryContainer").empty();

    $("#booking_eventDropdown_div").show();

    // ✅ Store FULL package array in button as JSON
    $("#btnFoodLabels").data("foodpackage", "");

    $('#foodPackage_PackageDropdown')
        .removeAttr('data-selected-items')
        .removeData('selected-items');
    resetFoodPackages();

    const now = new Date();
    const paidTime =
        String(now.getHours()).padStart(2, '0') + ":" +
        String(now.getMinutes()).padStart(2, '0'); // + ":" +
        //String(now.getSeconds()).padStart(2, '0');
    $("#PaymentDetails_PaidTime").val(paidTime);

    // Reset amounts to defaults
    //resetOtherCharges(false);
    recalculatePaymentSummary();
    $("#Section_PaymentDetails").show();
    //console.log("Booking form reset completed.");

    // Hide dynamic sections manually
    $("#PaymentDetails_TransactionId").closest(".col-md-6").addClass("d-none");
    $("#PaymentDetails_ChequeBank").closest(".col-md-6").addClass("d-none");
    $("#PaymentDetails_ChequeNo").closest(".col-md-6").addClass("d-none");
    $("#PaymentDetails_ChequeDate").closest(".col-md-6").addClass("d-none");
    // Optionally reset dropdown to default
    $("#PaymentDetails_Mode").val("0").trigger("change");

    // Re-select default account manually
    const defaultOption = $("#PaymentDetails_AccountId option").filter(function () {
        return $(this).data("isdefault") === true;
    });
    if (defaultOption.length) {
        $("#PaymentDetails_AccountId").val(defaultOption.val());
    }

    //SET STATE
    const $stateDropdown = $("#clientStateDropdown");
    // Find option with text "Gujarat"
    const gujaratOption = $stateDropdown.find("option").filter(function () {
        return $(this).text().trim().toLowerCase() === "gujarat";
    });
    if (gujaratOption.length > 0) {
        $stateDropdown.val(gujaratOption.val()).trigger("change");
    }
    else {
        $stateDropdown.val("0").trigger("change");
    }

    //Clear editors notes
    resetAllEditors();

    $("#btnBook").prop("disabled", false);
}

// Reset all editors safely
function resetAllEditors() {

    // 1. Clear all initialized editors
    Object.keys(editors).forEach(key => {
        if (editors[key]) {
            editors[key].setContents([]);
        }
    });

    // 2. Hide all editor boxes
    $(".editor-box").addClass("d-none");

    // 3. (Optional) Reset stored data
    notesData = null;
}

function resetFoodPackages() {
    // Remove all dynamically added blocks
    $("#foodPackageContainer .food-block").remove();

    // Reset first (static) row inputs
    const $firstRow = $("#foodPackageContainer > .food-row");

    $firstRow.find("select").val('0'); //.trigger('change');
    $firstRow.find("input").val(0);
    $firstRow.find(".foodPackage_TotalValue").text("0");

    // Clear stored data (important)
    $firstRow.find(".foodPackageDropdown").removeData("selected-items");
    $firstRow.find(".foodPackage_AddOnItems").removeData("selected-addonitems");
    addonCounter = 1;
}
function recalculatePaymentSummary() {

    let isGSTEnabled = $("#gstSwitch").is(":checked");
    let gstRate = 5;

    let taxableAmount = 0;

    // 🔹 Hall Rent
    let hallRent = parseFloat($("#booking_hallRent").val()) || 0;
    taxableAmount += hallRent;
    //console.log("hallRent:", hallRent);

    // 🔹 Default Food
    //let defaultFoodTotal = parseFloat($("#foodPackage_TotalValue").text()) || 0;
    //taxableAmount += defaultFoodTotal;
    //console.log("defaultFoodTotal:", defaultFoodTotal);

    // 🔹 Extra Food Rows
    $(".foodPackage_TotalValue").each(function () {
        let value = parseFloat($(this).text()) || 0;
        taxableAmount += value;
        //console.log("defaultFoodTotal rows:", value);
    });

    // 🔹 Other Charges
    let decoration = parseFloat($("#OtherCharges_Decoration_Amount").val()) || 0;
    let room = parseFloat($("#OtherCharges_RoomCharges_Amount").val()) || 0;
    let sound = parseFloat($("#OtherCharges_SoundSystem_Amount").val()) || 0;
    let projector = parseFloat($("#OtherCharges_ProjectorScreen_Amount").val()) || 0;

    taxableAmount += decoration + room + sound + projector;
    //console.log("decoration:", decoration);
    //console.log("room:", room);
    //console.log("sound:", sound);
    //console.log("projector:", projector);

    $("#paymentSummary_TaxableAmount").text(taxableAmount.toFixed(2));
    //console.log("taxableAmount:", taxableAmount);

    // 🔹 GST
    let totalGST = isGSTEnabled ? (taxableAmount * gstRate) / 100 : 0;

    let cgst = isGSTEnabled ? totalGST / 2 : 0;
    let sgst = isGSTEnabled ? totalGST / 2 : 0;
    let igst = 0;

    $("#paymentSummary_CGST").text(cgst.toFixed(2));
    $("#paymentSummary_SGST").text(sgst.toFixed(2));
    $("#paymentSummary_IGST").text(igst.toFixed(2));

    let total = taxableAmount + cgst + sgst + igst;

    $("#paymentSummary_Total").text(total.toFixed(2));
    //console.log("total:", total);

    applyBookingDiscount(total);
}

function applyBookingDiscount(totalAmount) {

    let type = $("#paymentSummary_BookingDiscount_Type").val();
    let enteredValue = parseFloat($("#paymentSummary_BookingDiscount_Amount").val()) || 0;

    let bookingDiscount = 0;

    if (type == "1") {
        bookingDiscount = enteredValue;
    } else if (type == "2") {
        bookingDiscount = (totalAmount * enteredValue) / 100;
    }

    if (bookingDiscount > totalAmount)
        bookingDiscount = totalAmount;

    $("#paymentSummary_BookingAmount").text(bookingDiscount.toFixed(2));

    let payableAmount = totalAmount - bookingDiscount;

    $("#paymentSummary_payableAmount").text(payableAmount.toFixed(2));
}


//Vendor work order
function saveVendorWorkOrder() {

    let userId = parseInt(sessionStorage.getItem('userId') || 0);

    // 🔹 1. Collect Vendor Work Order Data
    const workOrderData = {
        Id: parseInt($("#VendorWorkOrderId").val() || 0),
        BookingId: parseInt($("#VendorBookingId").val() || 0),
        ServiceId: parseInt($("#VendorServiceId").val() || 0),
        VendorId: parseInt($("#VendorId").val() || 0),
        ConcernPerson: $("#ConcernPerson").val(),
        PODate: $("#PODate").val() || null,
        EventDate: $("#EventDate").val() || null,
        EventTime: $("#EventTime").val() || null,
        SlotId: $("#SlotId").val() ? parseInt($("#SlotId").val()) : null,
        Note: $("#VendorNote").val(),
        ActionBy: userId
    };

    // 🔹 Basic Validation
    if (!workOrderData.BookingId || !workOrderData.ServiceId || !workOrderData.VendorId) {
        Swal.fire("Required", "Booking, Service and Vendor are mandatory.", "warning");
        return;
    }

    // 🔹 2. Build Particulars JSON
    const particulars = [];

    $("#vendorParticularsTable tbody tr").each(function () {

        const $row = $(this);

        const service = $row.find(".vw-service").val();
        const qty = parseFloat($row.find(".vw-qty").val() || 0);
        const amount = parseFloat($row.find(".vw-amount").val() || 0);
        const total = parseFloat($row.find(".vw-total").val() || 0);
        const description = $row.find(".vw-description").val();

        if (service) {
            particulars.push({
                Service: service,
                Qty: qty,
                Amount: amount,
                Total: total,
                Description: description
            });
        }
    });

    if (particulars.length === 0) {
        Swal.fire("Required", "Please add at least one work order particular.", "warning");
        return;
    }

    // 🔹 3. Attach Particulars
    workOrderData.Particulars = particulars;

    //console.log("🚚 Vendor Work Order JSON:", workOrderData);

    // 🔹 4. AJAX SAVE
    $.ajax({
        url: "/api/BookingCalendar_VendorWorkOrder/SaveBookingCalendar_VendorWorkOrder",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(workOrderData),
        beforeSend: function () {
            $("#btnSaveVendorWorkOrder")
                .prop("disabled", true)
                .text("Saving...");
        },
        success: function (response) {
            //console.log("Vendor Work Order Save Response:", response);
            if (response.status === "Success") {
                handleApiSuccess();

                // 🔹 Close modal
                const modalEl = document.getElementById("vendorWorkOrderModal");
                const modal = bootstrap.Modal.getInstance(modalEl);
                modal.hide();

                // 🔹 Optional refresh hooks
                // loadVendorWorkOrders(workOrderData.BookingId);
                // updateVendorWorkOrderCount(workOrderData.BookingId);

            } else {
                Swal.fire({
                    title: "Save Failed",
                    text: response.message,
                    icon: "error"
                });
            }
        },

        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Work Order Save Error");
        },

        complete: function () {
            $("#btnSaveVendorWorkOrder")
                .prop("disabled", false)
                .text("Save");
        }
    });
}


$("#btnSaveFoodWastage").on("click", function () {

    const bookingId = (typeof getBookingId === "function") ? getBookingId() : 0;
    const userId = sessionStorage.getItem('userId');

    if (!bookingId || bookingId <= 0) {
        alert("Invalid booking");
        return;
    }

    let foodList = [];
    let isValid = true;

    $("#tblFoodWastage tbody tr").each(function () {

        let menuItemId = parseInt($(this).find(".menuItemId").val());
        let wastageKg = parseFloat($(this).find(".wastageInput").val());

        if (isNaN(wastageKg) || wastageKg < 0) {
            alert("Wastage cannot be negative");
            isValid = false;
            return false; // break loop
        }

        foodList.push({
            foodmenuid: menuItemId,
            weightinkg: wastageKg
        });
    });

    if (!isValid) return;

    let payload = {
        BookingId: bookingId,
        UserId: userId,
        FoodJson: JSON.stringify(foodList)
    };

    //console.log("📦 Save Food Wastage Payload:", payload);

    $.ajax({
        url: "/api/FoodWastage/SaveFoodWastage",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res?.status === "Success") {
                handleApiSuccess();
                $('#foodWastageModal').modal('hide');
                 loadBookingCalendar();
            } else {
                Swal.fire("Error", res?.message || "Save Failed!", "error");
            }

            $("#foodWastageModal").modal("hide");
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Save foodWastage Error");
        }
    });
});


$(document).on("input", ".wastageInput", function () {
    let total = 0;
    $(".wastageInput").each(function () {
        let val = parseFloat($(this).val());
        if (!isNaN(val)) {
            total += val;
        }
    });
    $("#txtTotalWastage").val(total.toFixed(2));
});

function resetPaymentModal() {
    $("#paymentForm")[0].reset();
    $("#PaymentModal").modal("hide");

    // Hide dynamic sections manually
    $("#paymentTxn").closest(".col-md-6").addClass("d-none");
    $("#paymentChequebank").closest(".col-md-6").addClass("d-none");
    $("#paymentChequeNo").closest(".col-md-6").addClass("d-none");
    $("#paymentChequeDate").closest(".col-md-6").addClass("d-none");
    // Optionally reset dropdown to default
    $("#PaymentModal_Mode").val("0").trigger("change");

    $('#paymentDueBalance').val(0).prop('disabled', true);
    
    // Re-select default account manually
    const defaultOption = $("#PaymentModal_AccountId option").filter(function () {
        return $(this).data("isdefault") === true;
    });
    if (defaultOption.length) {
        $("#PaymentModal_AccountId").val(defaultOption.val());
    }
    $("#noteEditor .ql-editor").html("");
    setCurrentPaymentDateTime();
}
function setCurrentPaymentDateTime() {

    const now = new Date();

    // Format date → yyyy-mm-dd (required for input[type=date])
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');

    const formattedDate = `${year}-${month}-${day}`;

    // Format time → HH:mm (required for input[type=time])
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');

    const formattedTime = `${hours}:${minutes}`;

    $("#paymentDate").val(formattedDate);
    $("#paymentTime").val(formattedTime);
}


$("#btnSavePayment").click(function () {
    const bookingId = getBookingId();
    // 🔹 Get values
    // let bookingId = Number($("#paymentBookingId").val() || 0);
    let accountId = Number($("#PaymentModal_AccountId").val());
    let modeId = Number($("#PaymentModal_Mode").val());
    let paymentDate = $("#paymentDate").val();
    let paymentTime = $("#paymentTime").val();
    let txnId = $("#paymentTxn").val();
    let chequeBank = $("#paymentChequebank").val();
    let chequeNo = $("#paymentChequeNo").val();
    let chequeDate = $("#paymentChequeDate").val();
    let amount = Number($("#paymentAmount").val() || 0);
    let dueBalance = Number($("#paymentDueBalance").val() || 0);
    let nextDueDate = $("#NextDueDate").val();
    let paymentNotes = $("#noteEditor .ql-editor").text();

    // 🔹 Basic Validation
    if (!bookingId) {
        Swal.fire("Validation", "Booking Id missing!", "warning");
        return;
    }
    if (!accountId || accountId <= 0 ) {
        Swal.fire("Validation", "Payment Account is required!", "warning");
        return;
    }
    if (!modeId || modeId <= 0 ) {
        Swal.fire("Validation", "Payment Mode is required!", "warning");
        return;
    }
    if (!paymentDate) {
        Swal.fire("Validation", "Payment date is required!", "warning");
        return;
    }
    if (!amount || amount <= 0) {
        Swal.fire("Validation", "Valid amount is required!", "warning");
        return;
    }

    // 🔹 Payload (matches SaveBooking_Payments procedure)
    const payload = {
        BookingId: bookingId,
        AccountId: accountId,
        ModeId: modeId,
        DateOfPayment: paymentDate,
        PaymentTime: paymentTime || null,
        TxnId: txnId || null,
        ChequeBank: chequeBank || null,
        ChequeNo: chequeNo || null,
        ChequeDate: chequeDate || null,
        Amount: amount,
        DueBalance: dueBalance || 0,
        NextDueDate: nextDueDate || null,
        PaymentNotes: paymentNotes || null,
        ActionBy: Number(sessionStorage.getItem("userId") ?? "0")
    };

    //console.log("Save Payment Payload :", payload);

    $("#btnSavePayment").prop("disabled", true);
    //console.log("save pay payload:", payload);
    $.ajax({
        url: "/api/Booking_Payments/SaveBookingPayment", // 🔁 API URL
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            if (response?.status === "Success") {
                handleApiSuccess();

                // ✅ Swal Confirmation
                Swal.fire({
                    title: 'Download Receipt?',
                    text: 'Do you want to download the payment receipt now?',
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonText: 'Yes, Download',
                    cancelButtonText: 'No',
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33'
                }).then((result) => {
                    var payload = {
                        PaymentId: response.data.PaymentId,
                        BookingId: bookingId,
                        ActionBy: Number(sessionStorage.getItem("userId") ?? "0")
                    }
                    var url = "/api/Booking_Payments/DownloadPaymentReceipt";
                    if (result.isConfirmed) {
                        // ✅ Call download only if YES
                        downloadReport(url, payload, "Generating Receipt...");
                    }
                    // ❌ If NO → do nothing (flow continues)

                    resetPaymentModal();
                    loadPaymentHistory(bookingId);
                });

            }
            else {
                Swal.fire("Error", response?.message || "Save failed!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Save Payment Error");
        },
        complete: function () {
            $("#btnSavePayment").prop("disabled", false);
        }
    });
});

// ----------------- FETCH PAYMENT DATA -----------------
loadAccountDropdown();
function loadAccountDropdown() {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);
    var payload = { Id: 0, ActionBy: actionBy };
    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("GetAccount res:", res);

            const ddl = $("#PaymentModal_AccountId, #PaymentDetails_AccountId, #invoiceModal_invoicePayment_Account");
            ddl.empty().append('<option value="0">-- Select Account --</option>');

            if (res?.status === "Success" && Array.isArray(res.data)) {
                res.data.forEach(acc => {
                    const accountName = (acc.AccountName || "").trim(); 
                    ddl.append(`
                        <option value="${acc.Id}" data-isdefault="${acc.IsDefaultAccount}">
                            ${accountName}
                        </option>
                    `);
                });
            }
            else {
                Swal.fire("Error", res?.message || "Failed to load accounts!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Account Dropdown Error");
        }
    });
}

loadPaymentModeDropdown();
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
            const ddl = $("#PaymentModal_Mode, #PaymentDetails_Mode, #invoiceModal_invoicePayment_Mode");
            ddl.empty().append('<option value="0">--Select--</option>');

            if (res?.status === "Success" && Array.isArray(res.data)) {
                res.data
                    .filter(x => x.IsActive)
                    .forEach(pm => {
                        ddl.append(`<option value="${pm.Id}">${pm.PaymentModeName}</option>
                        `);
                    });
            }
            else {
                Swal.fire("Error", res?.message || "Failed to load payment modes!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Payment Mode Dropdown Error");
        }
    });
}

//invoicePayment_Mode - change
$(document).on("change", "#invoiceModal_invoicePayment_Mode", function () {

    //console.log("invoicePayment_Mode called");

    let selectedText = $("#invoiceModal_invoicePayment_Mode option:selected")
        .text()
        .toLowerCase();

    //console.log("selectedText:", selectedText);

    // 🔹 Hide all invoice dynamic fields first
    $(".invoicePayment_Fields").addClass("d-none");
    $(".invoicePayment_Fields input").val("");
    // CASH
    if (selectedText.includes("cash")) {
        // Nothing to show
    }

    // BANK TRANSFER
    else if (selectedText.includes("bank transfer")) {
        // Nothing to show
    }

    // CHEQUE
    else if (selectedText.includes("cheque")) {

        $("#invoiceModal_invoicePayment_ChequeBank")
            .closest(".invoicePayment_Fields")
            .removeClass("d-none");

        $("#invoiceModal_invoicePayment_ChequeNo")
            .closest(".invoicePayment_Fields")
            .removeClass("d-none");

        $("#invoiceModal_invoicePayment_ChequeDate")
            .closest(".invoicePayment_Fields")
            .removeClass("d-none");
    }

    // UPI OR CREDIT
    else if (selectedText.includes("upi") || selectedText.includes("credit")) {
        $("#invoiceModal_invoicePayment_Txn")
            .closest(".invoicePayment_Fields")
            .removeClass("d-none");
    }

});
//invoicePayment_btnWriteNote - click
$(document).on("click", "#invoiceModal_invoicePayment_btnWriteNote", function () {
    const editor = $(".invoiceModal_invoicePayment_EditorContainer");
    editor.toggleClass("d-none");
    if (editor.hasClass("d-none")) {
        $(this).val("Payment Notes");
    }
    else {
        $(this).val("Hide Notes");
    }

});
//invoicePayment_Amount - Input
$(document).on("input", "#invoiceModal_invoicePayment_Amount", function () {
    const originalDue = Number($("#invoiceModal_invoicePayment_originalDueAmount").val()) || 0;
    let payAmount = Number($(this).val()) || 0;

    // 🔒 Prevent overpayment
    //if (payAmount > originalDue) {
    //payAmount = originalDue;
    //$(this).val(originalDue);
    //}
    //$(this).val(payAmount);
    const remaining = originalDue - payAmount;
    $("#invoiceModal_invoicePayment_DueBalance, #invoiceModal_balanceAmount").val(remaining.toFixed(2));
});



function loadPaymentHistory(bookingId) {
    const payload = {
        PaymentId: 0,      
        BookingId: bookingId        
    };
    //console.log("loadPaymentHistory payload:", payload);
    $.ajax({
        url: "/api/Booking_Payments/GetBookingPayments",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("Booking_Payments res:", res);
            if (res?.status === "Success") {
                // HIDE download Alll btn
                $(".btn-payment-download-all").addClass("d-none");
                if (res.data.length > 0) {
                    // SHOW download Alll btn
                    $(".btn-payment-download-all").removeClass("d-none");
                }
                renderPaymentTable(res.data || []);
            } else {
                Swal.fire("Error", res?.message || "Failed to fetch payments!", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Payments Error");
        }
    });
}

// ----------------- RENDER TABLE -----------------
function renderPaymentTable(data) {

    // Destroy existing DataTable if exists
    if ($.fn.dataTable && $.fn.dataTable.isDataTable("#tblPaymentHistory table")) {
        $("#tblPaymentHistory table").DataTable().destroy();
    }

    // Add serial numbers
    data.forEach((item, i) => item.srno = i + 1);
    $("#tblPaymentHistory table").DataTable({
        data: data,
        autoWidth: false, 
        columns: [
            //{ data: "srno", title: "#", class:"text-center" },
            { data: "PaymentId", title: "Txn Id", className: "text-center" },
            { data: "PaymentDateTimeFormat", title: "Txn Datetime" },
            {
                data: "Amount",
                title: "Amount (₹)",
                className: "text-end",
                render: $.fn.dataTable.render.number(',', '.', 2, '₹ ')
            },
            {
                data: null,
                title: "Action",
                className: "text-center",
                render: function (row) {
                    return `<button class="btn btn-sm btn-outline-primary btn-payment-download" data-id="${row.PaymentId}">
                                <i class="fas fa-download"></i>
                            </button>`;
                }
            }
        ],
        pageLength: 5,
        ordering: true,
        searching: false
    });
}

// ----------------- OPTIONAL: Download button click -----------------
$(document).on("click", ".btn-payment-download", function () {
    let paymentId = $(this).data("id");
    let bookingId = getBookingId();
    let actionBy = Number(sessionStorage.getItem("userId") ?? "0");

    var payload = {
        PaymentId: paymentId,
        BookingId: bookingId,
        ActionBy: actionBy
    };

    var url = "/api/Booking_Payments/DownloadPaymentReceipt";
    downloadReport(url, payload, "Generating Receipt...");
});

$(document).on("click", ".btn-payment-download-all", function () {
    let paymentId = 0;
    let bookingId = getBookingId();
    let actionBy = Number(sessionStorage.getItem("userId") ?? "0");

    var payload = {
        PaymentId: paymentId,
        BookingId: bookingId,
        ActionBy: actionBy
    };

    var url = "/api/Booking_Payments/DownloadPaymentReceipt";
    downloadReport(url, payload, "Generating Receipt...");
});


// Vendor Work Order Modal -- START --------------------
// =========================================
// ADD NEW PARTICULAR ROW
// =========================================
$("#btnAddVendorParticular").on("click", function () {

    const newRow = `
                        <tr>
                            <td>
                                <input type="text"
                                       class="form-control form-control-sm vw-service"
                                       placeholder="Service">
                            </td>
                            <td>
                                <input type="number"
                                       class="form-control form-control-sm vw-qty"
                                       value="1"
                                       min="0">
                            </td>
                            <td>
                                <input type="number"
                                       class="form-control form-control-sm vw-amount"
                                       value="0"
                                       min="0"
                                       step="1">
                            </td>
                            <td>
                                <input type="number"
                                       class="form-control form-control-sm vw-total"
                                       value="0"
                                       readonly>
                            </td>
                            <td>
                                <input type="text"
                                       class="form-control form-control-sm vw-description"
                                       placeholder="Description">
                            </td>
                            <td class="text-center">
                                <button type="button"
                                        class="btn btn-outline-danger btn-sm btnRemoveRow">
                                    ×
                                </button>
                            </td>
                        </tr>
                    `;

    $("#vendorParticularsTable tbody").append(newRow);
});

// =========================================
// REMOVE ROW (LAST COLUMN ACTION)
// =========================================
$(document).on("click", ".btnRemoveRow", function () {

    const rowCount = $("#vendorParticularsTable tbody tr").length;

    if (rowCount <= 1) {
        Swal.fire(
            "Not Allowed",
            "At least one particular row is required.",
            "warning"
        );
        return;
    }

    $(this).closest("tr").remove();
});

// =========================================
// AUTO CALCULATE TOTAL = QTY × AMOUNT
// =========================================
$(document).on("input", ".vw-qty, .vw-amount", function () {

    const $row = $(this).closest("tr");

    const qty = parseFloat($row.find(".vw-qty").val()) || 0;
    const amount = parseFloat($row.find(".vw-amount").val()) || 0;

    const total = qty * amount;

    $row.find(".vw-total").val(total.toFixed(2));
});

// =========================================
// LOAD VENDOR WORK ORDER SERVICES
// =========================================
loadVendorWorkOrderServices();
function loadVendorWorkOrderServices(selectedId = 0) {
    $.ajax({
        url: "/api/BookingCalendar_VendorWorkOrder_Service/GetBookingCalendar_VendorWorkOrder_Service",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {

            const $ddl = $("#VendorServiceId");
            $ddl.empty();
            $ddl.append('<option value="0">Select Service</option>');

            res.data.forEach(function (item) {
                const selected =
                    selectedId && item.Id == selectedId
                        ? "selected"
                        : "";

                $ddl.append(
                    `<option value="${item.Id}" ${selected}>
                                        ${item.Name}
                                    </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            handleAjaxError(xhr, status, error, 'Failed to load vendor services');
        }
    });
}

//VENDOR DROPDOWN
loadSupplierDropdown();
function loadSupplierDropdown() {
    var payload = { Id: 0 };
    $.ajax({
        url: "/api/Supplier/GetSupplierList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("response supplier:", response);
            $("#VendorId").html(`<option value="0">-- Select Vendor --</option>`);
            response.data.forEach(s => {
                $("#VendorId").append(`<option value="${s.Id}">${s.Name}</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Vendor Fetch Error");
        }
    });
}

//SLOT DROPDOWN
loadSlotDropdown();
function loadSlotDropdown() {
    var payload = { Id: 0, IsActive: true };
    $.ajax({
        url: "/api/SlotMaster/GetSlotList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("response SlotMaster:", response);
            const $slotDropdowns = $("#SlotId, #reschedule_SlotId, #PO_Slot");
            const $abbrContainer = $("#slotAbbreviations");

            $slotDropdowns.html(`<option value="0">-- Select Slot --</option>`);
            $abbrContainer.html(""); // clear old abbreviations

            shiftMap = {};
            shiftTimes = {};
            shiftNames = [];   // reset
            response.data.forEach(s => {
                $slotDropdowns.append(`
                    <option value="${s.Id}" 
                            data-start="${s.StartTime}" 
                            data-end="${s.EndTime}" 
                            data-start2="${s.StartTime2}" 
                            data-end2="${s.EndTime2}">${s.Name} (${s.Code})
                    </option>`);

                // 2️⃣ Abbreviation text
                const timeRange = `${s.StartTime2}-${s.EndTime2}`;
                const code = s.Code;
                // fallback class if new code comes
                const cssClass = `shift-${code}`;
                $abbrContainer.append(`
                    <div class="shift-badge ${cssClass}">
                        <div class="shift-code">${code} - ${s.Name}</div>
                        <div class="shift-time">${timeRange}</div>
                    </div>
                `);

                // SLOT ABBREVIATIONS
                //const code = s.Code;
                // collect dynamic shift codes
                shiftNames.push(s.Code);

                // Dynamic full name mapping
                shiftMap[code] = s.Name;
                // Dynamic time mapping
                shiftTimes[code] = {
                    start: s.StartTime2 || s.StartTime,
                    end: s.EndTime2 || s.EndTime
                };

            });
            //console.log("Shift Map:", shiftMap);
            //console.log("Shift Times:", shiftTimes);
            //console.log("Shift Names:", shiftNames);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Slot Fetch Error");
        }
    });
}

// Slot Change for Reschedule
$(document).on("change", "#reschedule_SlotId", function () {

    const selectedOption = $(this).find("option:selected");

    const start2 = selectedOption.data("start2"); // 9:00AM or 09:00AM
    const end2 = selectedOption.data("end2");

    if ($(this).val() != "0" && start2 && end2) {

        const startMatch = start2.match(/^(\d{1,2}):(\d{2})(AM|PM)$/);
        const endMatch = end2.match(/^(\d{1,2}):(\d{2})(AM|PM)$/);

        if (startMatch) {
            $("#reschedule_startHour").val(startMatch[1]);
            $("#reschedule_startMinute").val(startMatch[2]);
            $("#reschedule_startAMPM").val(startMatch[3]);
        }

        if (endMatch) {
            $("#reschedule_endHour").val(endMatch[1]);
            $("#reschedule_endMinute").val(endMatch[2]);
            $("#reschedule_endAMPM").val(endMatch[3]);
        }

    } else {
        $("#reschedule_startHour, #reschedule_startMinute, #reschedule_endHour, #reschedule_endMinute").val("");
        $("#reschedule_startAMPM, #reschedule_endAMPM").val("AM");
    }
});
// Vendor Work Order Modal -- END --------------------
