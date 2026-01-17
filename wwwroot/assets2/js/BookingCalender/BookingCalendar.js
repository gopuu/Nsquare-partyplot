// Toggle editors visibility
let editorsInitialized = false;

document.getElementById('OtherCharges_ToggleEditorsBtn').addEventListener('click', function () {
    const container = document.getElementById('OtherCharges_editorsContainer');

    if (container.style.display === 'none') {
        container.style.display = 'block';

        // Initialize editors only once after showing
        if (!editorsInitialized) {
            // Customer editor
            // window.otherCharges_customerEditor = new Quill('#otherCharges_customerEditor', {
            //     theme: 'snow',
            //     modules: { toolbar: toolbarOptions }
            // });

            // // Internal editor
            // window.otherCharges_internalEditor = new Quill('#otherCharges_internalEditor', {
            //     theme: 'snow',
            //     modules: { toolbar: toolbarOptions }
            // });

            editorsInitialized = true;
        }

    } else {
        container.style.display = 'none';
    }
});

// 1️⃣ Import parchment
const Parchment = Quill.import('parchment');

// 2️⃣ Define a custom style attributor for font size (instead of class-based)
const SizeStyle = new Parchment.Attributor.Style('size', 'font-size', {
    scope: Parchment.Scope.INLINE,
    whitelist: ['10px', '12px', '14px', '16px', '18px', '20px', '24px', '30px']
});
Quill.register(SizeStyle, true);

// 3️⃣ Optional: Define custom line height too (if you still want)
const LineHeightStyle = new Parchment.Attributor.Style('lineheight', 'line-height', {
    scope: Parchment.Scope.BLOCK,
    whitelist: ['1.0', '1.2', '1.4', '1.5', '1.6', '2.0', '3.0']
});
Quill.register(LineHeightStyle, true);

// 4️⃣ Toolbar options
var toolbarOptions = [
    ['bold', 'italic', 'underline', 'strike'],
    [{ 'size': ['10px', '12px', '14px', '16px', '18px', '20px', '24px', '30px'] }],
    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
    [{ 'lineheight': ['1.0', '1.2', '1.4', '1.5', '1.6', '2.0', '3.0'] }]
];

// 5️⃣ Initialize editors
var otherCharges_customerEditor = new Quill('#OtherCharges_customerEditor', {
    theme: 'snow',
    modules: { toolbar: toolbarOptions }
});

var noteEditor = new Quill('#noteEditor', {
    theme: 'snow',
    modules: { toolbar: toolbarOptions }
});


var otherCharges_internalEditor = new Quill('#OtherCharges_internalEditor', {
    theme: 'snow',
    modules: { toolbar: toolbarOptions }
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
        BanquetId: Number($("#reschedule_BanquetId").val()) || null,
        SlotId: Number($("#reschedule_SlotId").val()) || null,
        FromDate: $("#reschedule_FromDate").val() || null,
        TillDate: $("#reschedule_TillDate").val() || null,
        StartTime: startTime || null,
        EndTime: endTime || null,
        ShiftCode: null, // optional (can be derived from slot if needed)
        ActionBy: Number(sessionStorage.getItem("userId")) || 0
    };

    console.log("Reschedule Payload:", payload);

    // 🔹 Call API
    $.ajax({
        url: "/api/BookingCalendar/SaveBookingCalendar_Reschedule",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),

        success: function (res) {
            if (res.status === "Success") {
                Swal.fire({
                    icon: "success",
                    title: "Rescheduled",
                    text: res.message
                }).then(() => {
                    $("#bookingRescheduleModal").modal("hide");
                    // refresh calendar / board here if needed
                    loadBookingCalendar();
                });
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
            if (response.status === "Success" && response.data) {
                banquetHallsList = response.data;
                let options = `<option value="0">-- Select Hall --</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.HallName} (${item.CapacityRangeText})</option>`;
                });
                $("#banquetDropdown, #reschedule_BanquetId").html(options);
                // ✅ If banquetId is passed → select & STOP
                if (banquetId > 0) {
                    $("#banquetDropdown, #reschedule_BanquetId")
                        .val(String(banquetId))
                        .trigger("change");

                    return; // ❌ do not render board
                }
                //console.log("fetchHall - passing bookingData:", bookingData);
                renderBoard(bookingData);
            } else {
                $("#banquetDropdown, #reschedule_BanquetId").html(`<option value="0">No Hall Found</option>`);
                //console.error('Failed to fetch hall details:', response.message);
                // Swal.fire({
                //     icon: 'error',
                //     title: 'Error',
                //     text: response?.message || 'Failed to fetch hall details.'
                // });
            }
        },
        error: function (xhr, status, error) {
            printAllErrors(xhr, status, error, 'Failed to fetch hall details');
            //console.error('Error fetching hall details:', xhr.responseText);
            // Swal.fire({
            //     icon: 'error',
            //     title: 'Error',
            //     text: xhr.responseText || 'Failed to fetch hall details.'
            // });
        },
        complete: function () {
            //hideLoader();  // Hide after request completes (success or error)
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
        }
    });
}
//banquet hall details - dropdown -- END ----------------------

//Lead reference - dropdown -- START ----------------------
function loadLeadSourceList2() {
    //showLoader();
    $.ajax({
        url: "/api/LeadSource/GetLeadSourceList",
        type: "GET",
        success: function (response) {
            //console.log("leadSource response:", response);
            if (response.status === "Success") {
                let options = `<option value="0">-- Select Reference --</option>`; // default option

                if (response.data && response.data.length > 0) {
                    $.each(response.data, function (index, item) {
                        options += `<option value="${item.Id}">${item.Name}</option>`;
                    });
                } else {
                    options += `<option value="0">No Reference Found</option>`;
                }
                $("#booking_leadReferenceDropdown").html(options);
            } else {
                $("#booking_leadReferenceDropdown").html(`<option value="0">No Reference Found</option>`);
            }
        },
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Error fetching lead source list.",
                confirmButtonText: "OK",
            });
        },
        complete: function () {
            //hideLoader();
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
        }
    });
}
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
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Error fetching Enquiry list.",
                confirmButtonText: "OK",
            });
        },
        complete: function () {
            //hideLoader();
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
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
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.Name}</option>`;
                });
                $("#clientStateDropdown").html(options);
            } else {
                $("#clientStateDropdown").html(`<option value="0">No State Found</option>`);
            }
        },
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Error fetching State list.",
                confirmButtonText: "OK",
            });
        },
        complete: function () {
            //hideLoader();
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
        }
    });
}
//State - dropdown -- END ------------------------

//Event - dropdown -- START ----------------------
function loadSubEventList() {
    //showLoader();
    $.ajax({
        url: "/api/EventSubType/GetEventSubTypeList", // API endpoint
        type: "POST", // POST request
        contentType: "application/json", // Send data as JSON
        data: JSON.stringify({ EventTypeId: 0 }), // Request body
        success: function (response) {
            //console.log("event subtype list response:", response);
            if (response.status === "Success") {
                let options = `<option value="0">-- Select Event --</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.Name}</option>`;
                });
                $("#booking_subEventDropdown").html(options);
            } else {
                $("#booking_subEventDropdown").html(`<option value="0">No Event Found</option>`);
            }
        },
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Error fetching Event list.",
                confirmButtonText: "OK",
            });
        },
        complete: function () {
            //hideLoader();
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
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
            } else {
                $(".foodPackageDropdown").html(`<option value="0">No Package Found</option>`);
            }
        },
        error: function () {
            Swal.fire({ icon: "error", title: "Error", text: "Error fetching Food Packages." });
        },
        complete: function () {
            //hideLoader();
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
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

// ✅ STEP 3: Attach global delegated change event for ALL dropdowns
$(document).on("change", ".foodPackageDropdown", function () {
    const selectedPackageId = $(this).val();
    if (!selectedPackageId || selectedPackageId === "0") return;

    const $dropdown = $(this);
    const selectedText = $dropdown.find("option:selected").text(); // 👈 get selected text
    const ratePerPerson = $dropdown.find("option:selected").data("rateperperson");
    console.log("selectedPackageId:", selectedPackageId, "selectedText:", selectedText);
    $("#foodPackage_RatePerPlate").val(ratePerPerson);
    updatePackageCost();
    //showLoader();

    $.ajax({
        url: "/api/BookingCalendar/GetPackage_MenuAndFoodItemsList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Package_Id: selectedPackageId }),
        success: function (response) {
            if (response.status === "Success" && response.data.length > 0) {
                renderMenuItems(response.data);
                // update modal title with package name
                $("#menuModal .modal-title").text(`Menu Items [${selectedText}]`);

                $("#menuModal").modal("show");

                // store reference to dropdown that opened modal
                $("#menuModal").data("activeDropdown", $dropdown);
            } else {
                Swal.fire({
                    icon: "info",
                    title: "Menu Items Not Found",
                    text: "No menu items found for this package."
                });
                $(this).val("0");
            }
        },
        error: function () {
            Swal.fire({ icon: "error", title: "Error", text: "Error fetching Package MenuItems list." });
        },
        complete: function () {
            //hideLoader();
        }
    });
});

function updatePackageCost() {
    let pax = parseFloat($("#booking_pax").val()) || 0;
    let rate = parseFloat($("#foodPackage_RatePerPlate").val()) || 0;

    let packageCost = pax * rate;
    $("#foodPackage_PackageCost").val(packageCost);

    let gstAmount = ((packageCost * 5) / 100).toFixed(0);
    $("#foodPackage_GST").val(gstAmount);

    let totalCharges = parseFloat(packageCost) + parseFloat(gstAmount);
    $("#foodPackage_TotalValue").text(totalCharges);
}

$(document).on("input", "#foodPackage_RatePerPlate, #booking_pax", function () {
    updatePackageCost();
});

//Calculate PAckage for Extra added package rows
function calculateFoodRowTotals(rowElement) {
    let pax = parseFloat($(rowElement).find(".pax").val()) || 0;
    let perPlate = parseFloat($(rowElement).find(".perPlate").val()) || 0;
    let addons = parseFloat($(rowElement).find(".addonsCost").val()) || 0;

    console.log("pax:", pax, ", perPlate:", perPlate, ", addons:", addons);

    let packageCost = pax * perPlate;
    $(rowElement).find(".packageCost").val(packageCost);
    console.log("packageCost:", packageCost);

    let gst = ((packageCost * 5) / 100).toFixed(0);
    $(rowElement).find(".gstCost").val(gst);

    let total = packageCost + parseFloat(gst) + addons;
    $(rowElement).find(".foodPackage_TotalValue").text(total);
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
function renderMenuItems(menuData) {
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
                <div class="border rounded p-3 mb-3 bg-light menu-section" data-menu-id="${menu.FoodMenu_Id}">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <h6 class="mb-0 fw-bold">${menu.FoodMenu_Name}</h6>
                        <div>
                            <input type="checkbox" class="form-check-input select-all" id="selectAll_${menu.FoodMenu_Id}">
                            <label for="selectAll_${menu.FoodMenu_Id}" class="form-check-label">Select All</label>
                        </div>
                    </div>
                    <div class="d-flex flex-wrap">${foodItems}</div>
                </div>`;
    });

    $("#menuModalBody").html(html);
}

// ✅ STEP 5: Modal “Select All” and “Save” buttons
$(document).on("change", ".select-all", function () {
    const menuId = $(this).attr("id").split("_")[1];
    const checked = $(this).is(":checked");
    $(`.item-checkbox[data-menu-id='${menuId}']`).prop("checked", checked);
});

//StEP 6: Save MENU
$(document).on("click", ".btn-saveMenu", function () {
    const selectedData = [];

    $(".menu-section").each(function () {
        const menuId = $(this).data("menu-id");
        $(this).find(".item-checkbox:checked").each(function () {
            selectedData.push({
                FoodMenu_Id: parseInt(menuId),
                FoodItem_Id: parseInt($(this).val()),
                IsChecked: $(this).is(":checked")
            });
        });
    });

    // attach selected items to the dropdown that opened modal
    const $activeDropdown = $("#menuModal").data("activeDropdown");
    if ($activeDropdown) {
        $activeDropdown.data("selectedItems", selectedData);
    }

    console.log("Saved Menu Selection:", selectedData);
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
                    <input type="text" class="form-control" placeholder="Addon Food Package Name">
                </div>
                <div class="col-md-1">
                    <label class="form-label">Pax</label>
                    <input type="number" class="form-control pax" placeholder="Pax" value="0">
                </div>
                <div class="col-md-2">
                    <label class="form-label">Date</label>
                    <input type="date" class="form-control">
                </div>
                <div class="col-md-3">
                    <label class="form-label">Time</label>
                    <div class="d-flex gap-1">
                        <input type="number" class="form-control" placeholder="HH" min="1" max="12" style="width: 70px;">
                        <input type="number" class="form-control" placeholder="MM" min="0" max="59" style="width: 70px;">
                        <select class="form-select" style="width: 90px;">
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
                    <select class="form-select foodPackageDropdown"></select>
                    <input type="hidden" class="package_row_id" value="0" />
                </div>
                <div class="col-md-1">
                    <label class="form-label">Per Plate</label>
                    <input type="number" class="form-control perPlate" placeholder="Per Plate" value="0">
                </div>
                <div class="col-md-2">
                    <label class="form-label">Package Cost</label>
                    <input type="number" class="form-control packageCost" placeholder="Package Cost" value="0" disabled>
                </div>
                <div class="col-md-1">
                    <label class="form-label">Addons</label>
                    <input type="number" class="form-control addonsCost" placeholder="Addons" value="0" disabled>
                </div>
                <div class="col-md-1">
                    <label class="form-label">GST</label>
                    <input type="number" class="form-control gstCost" placeholder="GST" value="0" disabled>
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

function loadAddonCharges() {
    //showLoader();
    $.ajax({
        url: "/api/Company_FoodRate_AddonCharge/GetCompanyFoodRateAddonCharge",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            if (res.status === "Success" && res.data && res.data.length > 0) {
                addon_Data = res.data;
                populateAddonModal();
            }
        },
        error: function (xhr, status, error) {
            printAllErrors(xhr, status, error, 'GetCompanyFoodRateAddonCharge');
            Swal.fire({ icon: 'error', title: 'Error', text: 'Failed to load addon charges.' });
        },
        complete: function () {
            //hideLoader();
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
        }
    });
}

function populateAddonModal(savedItems = []) {
    console.log("savedItems AddOns:", savedItems);
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
                    <div style="width:70px;" class="text-center">Rate</div>
                    <div style="width:70px;" class="text-center">Pax</div>
                    <div style="width:90px;" class="text-center">Total</div>
                </div>
            `);
        headerCard.find(`[data-header-body="${header}"]`).append(columnHeader);

        // Populate items
        grouped[header].forEach(item => {
            // Find if this item was saved previously
            let saved = savedItems.find(s => s.AddOn_Item_Id === item.Id && s.AddOn_Header_Id === item.HeaderId);
            console.log("saved AddOns:", saved);
            // If found, prefill values
            let isChecked = saved ? true : false;
            let rate = saved ? saved.AddOn_Rate : item.Amount;
            let pax = saved ? saved.AddOn_Pax : 0;
            let total = saved ? saved.AddOn_Total : 0;

            let row = $(`
                    <div class="d-flex align-items-center justify-content-between mb-2"
                         data-header="${item.HeaderName}" data-headerid="${item.HeaderId}" data-id="${item.Id}">
                        <div class="d-flex align-items-center flex-grow-1">
                            <input type="checkbox" class="form-check-input me-2 addon-checkbox" ${isChecked ? "checked" : ""}>
                            <span>${item.ItemName}</span>
                        </div>
                        <input type="number" class="form-control form-control-sm rate-input text-center mx-1" step="1"
                               value="${rate}" min="0" style="width:70px;">
                        <input type="number" class="form-control form-control-sm pax-input text-center mx-1" step="1"
                               value="${pax}" min="0" style="width:70px;">
                        <input type="number" class="form-control form-control-sm total-input text-center mx-1 bg-light"
                               value="${total.toFixed(2)}" readonly style="width:90px;">
                    </div>
                `);

            row.find(".rate-input, .pax-input, .addon-checkbox").on("input change", function () {
                recalcRow(row);
                recalcSummary();
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

    recalcSummary();
}

function recalcRow(row) {
    let checked = row.find(".addon-checkbox").is(":checked");
    let rate = parseFloat(row.find(".rate-input").val()) || 0;
    let pax = parseFloat(row.find(".pax-input").val()) || 0;
    let total = checked ? rate * pax : 0;
    row.find(".total-input").val(total.toFixed(2));
}

function recalcSummary() {
    console.log("recalcSummary called");
    // Reset
    Object.keys(headerTotals).forEach(h => headerTotals[h] = 0);

    console.log("1 headerTotals:", headerTotals);
    $("[data-header]").each(function () {
        let header = $(this).data("header");
        let total = parseFloat($(this).find(".total-input").val()) || 0;
        headerTotals[header] += total;
    });
    console.log("2 headerTotals:", headerTotals);

    Object.keys(headerTotals).forEach(header => {
        $(`[data-header-summary="${header}"] .header-total`).text(headerTotals[header].toFixed(2));
    });

    let finalTotal = Object.values(headerTotals).reduce((a, b) => a + b, 0);
    $("#finalTotal").text(finalTotal.toFixed(2));
}

// Open modal function
let currentAddonButton = null;
function openAddonModal(booking_foodPackageId = 0, button = null) {
    console.log("booking_foodPackageId:", booking_foodPackageId);
    currentAddonButton = button; // 🔹 store reference to clicked button
    let savedItems = $(button).data("selected-AddOnItems") || [];

    if (addon_Data.length === 0) {
        loadAddonCharges();
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
    console.log("💰 Grand Total:", grandTotal.toFixed(2));

    // 🔹 Update the clicked AddOn button’s total span
    if (currentAddonButton) {
        let $btn = $(currentAddonButton);
        let $totalSpan = $btn.closest(".d-flex").find(".foodPackage_TotalValue");

        // Update total text
        $totalSpan.text(grandTotal.toFixed(2));

        // Store selected items for later retrieval
        $btn.data("selected-AddOnItems", selectedItems);
        $btn.data("total", grandTotal);

        // 🔹 Update button label with count
        let itemCount = selectedItems.length;
        $btn.html(`<i class="fa fa-check"></i> Addons (${itemCount})`);
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
    addonButton.data("selected-AddOnItems", selectedItems);
}
//Food Addons ----- /END ------------------------------------------------------------------

const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
const shiftNames = ["FD", "MO", "AF", "EV", "ON"]; //,"BF","LN","HT","DN"];
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
let cachedDays = [];
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

                // Find current year object
                let currentYearObj = years.find(y => y.YearName == new Date().getFullYear());
                selectedYear = currentYearObj ? parseInt(currentYearObj.YearName) : new Date().getFullYear();

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
        error: function (err) {
            console.error("Error fetching years:", err);
        },
        complete: function () {
            //hideLoader();
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
        }
    });
}

//Year - change event
yearSelect.addEventListener("change", function () {
    selectedYear = yearSelect.options[yearSelect.selectedIndex].text; // display year
    fetchMonthData(yearSelect.value); // fetch months for selected year
});

//Fetch Month by Year
function fetchMonthData(yearId) {
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
        error: function (err) {
            console.error("Error fetching months:", err);
        },
        complete: function () {
            //hideLoader();
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
        }
    });
}

//DISPLAY Months
function populateMonthButtons(months) {
    //console.log("fetched months:", months);
    monthButtons.innerHTML = "";
    months.forEach((m, idx) => {
        const btn = document.createElement("button");
        btn.className = `btn btn-outline-primary btn-sm ${idx === selectedMonth ? "active" : ""}`;
        btn.textContent = m.MonthName;
        btn.dataset.monthId = m.MonthId;
        btn.dataset.yearId = m.YearId;

        btn.addEventListener("click", () => {
            selectedMonth = idx;
            // Highlight selected month
            populateMonthButtons(months);

            console.log("Selected YearId:", m.YearId, "MonthId:", m.MonthId);
            // Call renderBoard() or other logic if needed
            //renderBoard();
            loadBookingCalendar();
        });
        monthButtons.appendChild(btn);
    });
}

//Generate Days for Selected Month
function generateDays2(year, month) {
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const days = [];
    for (let d = 1; d <= daysInMonth; d++) {
        const dateObj = new Date(year, month, d);
        const dayName = dateObj.toLocaleDateString("en-US", { weekday: "short" });
        //days.push({date:d.toString().padStart(2,"0"), day:dayName, dateObj, enquiries:Math.floor(Math.random()*10)+1});
        days.push({
            date: d.toString().padStart(2, "0"),
            day: dayName,
            dateObj,
            enquiries: 2,  // ✅ always 0 (will fetch later)
            enquiryList: [
                {
                    Name: "Ajay Patel",
                    Amount: 15000
                },
                {
                    Name: "Meera Shah",
                    Amount: 22000
                }
            ]
        });
    }
    return days;
}

// -----------------------------
// Async helper: fetch calendar days from API
// -----------------------------
function generateDays(year, month) {
    return new Promise((resolve) => {
        const req = {
            Year: year,
            Month: month + 1 // JS month 0-based -> API month 1-based
        };

        $.ajax({
            url: "/api/BookingCalendar/GetEnquiryCalendarMock",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(req),
            success: function (res) {
                console.log("GetEnquiryCalendarMock res:", res);
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
                    console.error("generateDays.parseError:", err);
                }

                // fallback: return empty days (no enquiries)
                resolve(generateEmptyDays(year, month));
            },
            error: function (err) {
                console.error("Calendar API Error:", err);
                // fallback: return empty days (no enquiries)
                resolve(generateEmptyDays(year, month));
            }
        });
    });
}

// small helper used as fallback when API fails
function generateEmptyDays(year, month) {
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

//Generate Shifts Colors for Days of Month
function generateShiftDataPerDay(dateObj) {
    return shiftNames.map(() => {
        const today = new Date();
        if (dateObj < today) {
            return legendColors.past;    // ✅ Always gray for past
        }
        return legendColors.available;   // ✅ Always green for current & future
    });
}

//main booking board
async function renderBoard(bookingData = null) {
    console.log("renderBoard bookingData:", bookingData);
    //showLoader();
    //console.log("renderBoard banquetHallsList:", banquetHallsList);
    board.innerHTML = "";
    //console.log("renderBoard selectedYear:", selectedYear, ", selectedMonth:", selectedMonth);
    cachedDays = await generateDays(selectedYear, selectedMonth);
    console.log("renderBoard cachedDays:", cachedDays);

    // Pre-generate shift data per resource per day
    cachedShiftData = {};
    banquetHallsList.forEach(hall => {
        cachedShiftData[hall.Id] = cachedDays.map(d => generateShiftDataPerDay(d.dateObj));
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
    cachedDays.forEach(d => {
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

    // Resource Rows
    banquetHallsList.forEach(hall => {
        const row = document.createElement("div");
        row.className = "row-wrapper align-items-center";

        //Hall Name - Left Side
        const nameCell = document.createElement("div");
        nameCell.className = "text-center rounded p-2 name-cell";
        nameCell.textContent = hall.HallName;
        row.appendChild(nameCell);

        cachedShiftData[hall.Id].forEach((shiftArr, dayIndex) => {
            const cell = document.createElement("div");
            cell.className = "border rounded p-1 text-center day-cell"; // ✅ green default

            // ✅ Detect if this is a past day (all shifts will be past)
            // const isPastDay = shiftArr.every(color => color === legendColors.past);
            // if (isPastDay) {
            //     cell.classList.add("bg-secondary"); // gray background
            // } else {
            //     cell.classList.add("bg-success");   // green background for today/future
            // }

            // ✅ Compare with today's date
            const cellDate = cachedDays[dayIndex].dateObj; // assuming dateObj is a real Date object
            const today = new Date();
            today.setHours(0, 0, 0, 0); // normalize to midnight

            let isToday = false;
            if (cellDate < today) {
                // PAST DAY (yesterday & earlier)
                cell.classList.add("bg-secondary", "text-white");
            }
            else if (cellDate.toDateString() === today.toDateString()) {
                cell.classList.remove("bg-success");
                //cell.classList.add("bg-warning", "text-dark"); // Highlight today with yellow
                cell.classList.add("bg-success", "text-white");
                isToday = true; // ✅ mark this column as today
            }
            else {
                // CURRENT & FUTURE DAY
                cell.classList.add("bg-success", "text-white");
            }

            // Populate cell with shift data
            cell.innerHTML = shiftArr.map((color, i) => {
                let finalColor = color;
                if (isToday) {
                    finalColor = "success"; // ✅ Force all shifts green for today
                }

                // const textColor = finalColor === "light" ? "text-black" : "text-white";
                // return `<div class="badge bg-${finalColor} d-block mb-1 ${textColor}">
                //             ${shiftNames[i]}
                //         </div>`;
                const textColor = finalColor === "light" ? "text-black" : "text-white";
                // const dateStr = `${cachedDays[dayIndex].date}-${monthNames[selectedMonth]}-${selectedYear}`;
                // const dateStr_Format = `${cachedDays[dayIndex].date}-${selectedMonth+1}-${selectedYear}`;
                const yyyy = selectedYear;
                const mm = String(selectedMonth + 1).padStart(2, "0"); // ensure 2 digits
                const dd = String(cachedDays[dayIndex].date).padStart(2, "0");

                const dateStr = `${dd}-${monthNames[selectedMonth]}-${yyyy}`;       // for display
                const dateStr_Format = `${dd}-${mm}-${yyyy}`;                       // your current dd-mm-yyyy
                const dateStr_ISO = `${yyyy}-${mm}-${dd}`;                          // ✅ proper yyyy-mm-dd

                const shiftCode = shiftNames[i];
                const shiftFull = fullShiftName(shiftCode); // fallback if not found
                var bookingId = 0;
                var eventName = null;
                var clientName = null;
                var clientContact = null;
                var eventDateRange = null;

                // 🔍 Check if this cell is booked in bookingData
                let isBooked = false;
                if (Array.isArray(bookingData)) {
                    for (const booking of bookingData) {
                        if (booking.BanquetId === hall.Id && booking.ShiftCode === shiftCode) {
                            const fromDate = new Date(booking.FromDate);
                            const tillDate = new Date(booking.TillDate);
                            const currentDate = new Date(dateStr_ISO);
                            // inclusive range
                            if (currentDate >= fromDate && currentDate <= tillDate) {
                                bookingId = booking.BookingId;
                                eventName = booking.Booking_TypeName;
                                clientName = booking.Client_Name;
                                clientContact = booking.Client_Contact;
                                eventDateRange = booking.FromDate_Short + ' ' + booking.StartTime_Full + ' - ' + booking.TillDate_Short + ' ' + booking.EndTime_Full;

                                isBooked = true;
                                break;
                            }
                        }
                    }
                }
                if (isBooked) {
                    finalColor = "danger"; // 🔴 Mark Confirmed Booked
                }

                return `<div
                                class="badge bg-${finalColor} d-block mb-1 ${textColor}"
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
                                ${shiftCode}
                            </div>`;

            }).join("");

            // ✅ Handle click on individual badge inside this cell
            cell.querySelectorAll(".badge").forEach(badge => {
                badge.addEventListener("click", (e) => {
                    e.stopPropagation(); // prevent triggering cell click
                    // console.log("Banquet ID:", hall.Id);
                    // console.log("Banquet Name:", hall.HallName);
                    // console.log("shift short:", badge.dataset.shift); // ✅ only that one badge
                    // console.log("shift full:", badge.dataset.shiftname); // ✅ only that one badge
                    // console.log("Date:", badge.dataset.date);
                    // console.log("Date_Format:", badge.dataset.date_format);
                    // console.log("Date_ISO:", badge.dataset.date_iso);
                    //console.log("Date:", `${cachedDays[dayIndex].date}-${monthNames[selectedMonth]}-${selectedYear}`);
                });
            });

            row.appendChild(cell);
        });
        board.appendChild(row);
    });

    //on cell hover - tooltip ---- START --------------------
    // 🧠 Create a custom tooltip element (only once)
    if (!document.getElementById("customTooltip")) {
        const tooltip = document.createElement("div");
        tooltip.id = "customTooltip";
        document.body.appendChild(tooltip);
    }
    // Reference tooltip
    const tooltip = document.getElementById("customTooltip");

    // 🪄 Attach hover listeners to badges
    document.addEventListener("mouseover", function (e) {
        const badge = e.target.closest(".badge[data-shift]");
        if (badge) {
            const shiftname = badge.dataset.shiftname;
            const date_full = badge.dataset.date;

            const shiftcode = badge.dataset.shift;
            const bookingid = badge.dataset.bookingid;
            const eventName = badge.dataset.eventname || "";
            const clientName = badge.dataset.clientname || "";
            const clientContact = badge.dataset.clientcontact || "";
            const eventDateRange = badge.dataset.eventdaterange || "";

            if (bookingid > 0) {
                // Tooltip rich layout (multi-line, readable)
                tooltip.innerHTML = `
                        <div>
                            <strong><i class="fa-solid fa-calendar-alt"></i> ${eventName} (${shiftcode})</strong>
                        </div>
                        <div>
                            <i class="fa-solid fa-user"></i> ${clientName}
                        </div>
                        <div>
                            <i class="fa-solid fa-phone"></i> ${clientContact}
                        </div>
                        <div>
                            <i class="fa-solid fa-clock"></i> ${eventDateRange}
                        </div>
                    `;
                tooltip.style.background = "#e11d48"; // 🔴 red for booked
                tooltip.style.color = "#fff";
                tooltip.style.display = "block";
            }
            else {
                if (badge.classList.contains("bg-secondary")) {
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
        }
    });

    document.addEventListener("mousemove", function (e) {
        if (tooltip.style.display === "block") {
            const tooltipWidth = tooltip.offsetWidth;
            const tooltipHeight = tooltip.offsetHeight;
            const offset = 15;

            // Keep tooltip inside viewport
            let left = e.pageX + offset;
            let top = e.pageY + offset;
            if (left + tooltipWidth > window.innerWidth) left = e.pageX - tooltipWidth - offset;
            if (top + tooltipHeight > window.innerHeight) top = e.pageY - tooltipHeight - offset;

            tooltip.style.left = left + "px";
            tooltip.style.top = top + "px";
        }
    });

    document.addEventListener("mouseout", function (e) {
        const badge = e.target.closest(".badge[data-shift]");
        if (badge) {
            tooltip.style.display = "none";
        }
    });
    //on cell hover - tooltip ---- END --------------------

    renderListViewTable();
    adjustFontSizeForDays(cachedDays.length);
    attachColumnHoverListeners();
    enableEnquiryTooltips();
    attachShiftClickListeners();
    //hideLoader();
}

//enquiry count - tooltip
function enableEnquiryTooltips() {
    const tooltip = document.getElementById("enquiryTooltip");

    // Show tooltip on hover
    document.addEventListener("mouseover", function (e) {
        const badge = e.target.closest(".badge-enquiries");
        if (!badge) return;

        const list = badge.dataset.enquiryList ? JSON.parse(badge.dataset.enquiryList) : [];
        const count = parseInt(badge.innerText);

        // No tooltip if empty
        if (count === 0 || list.length === 0) {
            tooltip.style.display = "none";
            return;
        }

        // Build tooltip table
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
            html += `
                    <tr>
                        <td>${x.Name}</td>
                        <td class="text-end">₹${(+x.Amount).toLocaleString()}</td>
                    </tr>
                `;
        });

        html += `</tbody></table>`;

        tooltip.innerHTML = html;
        tooltip.style.display = "block";

        const rect = badge.getBoundingClientRect();
        tooltip.style.left = (rect.right + 10 + window.scrollX) + "px";
        tooltip.style.top = (rect.top + window.scrollY) + "px";
    });

    // Hide tooltip when leaving the badge
    document.addEventListener("mouseout", function (e) {
        if (e.target.closest(".badge-enquiries")) {
            tooltip.style.display = "none";
        }
    });
}

function adjustFontSizeForDays(daysCount) {
    const scale = daysCount > 30 ? 0.65 : daysCount > 29 ? 0.7 : 0.75;
    document.querySelectorAll("#booking-board .day-cell").forEach(cell => {
        cell.style.fontSize = `${scale}rem`;
    });
}

// Helper: Convert abbreviation to full name
function fullShiftName(abbr) {
    const map = {
        FD: "Full Day",
        MO: "Morning",
        AF: "Afternoon",
        EV: "Evening",
        ON: "Overnight",
        BF: "Breakfast",
        LN: "Lunch",
        HT: "High Tea",
        DN: "Dinner"
    };
    return map[abbr] || abbr;
}

const shiftTimes = {
    FD: { start: "07:00 AM", end: "11:00 PM" },
    MO: { start: "09:00 AM", end: "12:00 PM" },
    AF: { start: "01:00 PM", end: "05:00 PM" },
    EV: { start: "06:00 PM", end: "10:00 PM" },
    ON: { start: "10:00 PM", end: "06:00 AM" },
    BF: { start: "07:00 AM", end: "10:00 AM" },
    LN: { start: "12:00 PM", end: "03:00 PM" },
    HT: { start: "04:00 PM", end: "06:00 PM" },
    DN: { start: "07:00 PM", end: "11:00 PM" }
};

function renderListViewTable2() {
    listTableBody.innerHTML = "";
    cachedDays.forEach((d, index) => {
        banquetHallsList.forEach(hall => {
            const tr = document.createElement("tr");
            tr.innerHTML = `
                    <td>${d.date}-${monthNames[selectedMonth]}-${selectedYear}</td>
                    <td>${d.day}</td>
                    <td>
                        <div class="badge-container">
                            <span class="badge border border-dark text-black bg-white cursor-pointer">${d.enquiries}</span>
                        </div>
                    </td>
                    <td>${hall.HallName}</td>
                    <td>
                        <div class="badge-container">
                            ${cachedShiftData[hall.Id][index].map((color, i) => {
                const textColor = color === "light" ? "text-black" : "text-white";
                return `<span class="badge bg-${color} ${textColor}">${shiftNames[i]}</span>`;
            }).join(" ")}
                        </div>
                    </td>`;
            listTableBody.appendChild(tr);
        });
    });
}

function renderListViewTable(bookingData = null) {

    listTableBody.innerHTML = "";

    cachedDays.forEach((d, dayIndex) => {

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
                                eventName = booking.Booking_TypeName;
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

document.getElementById("bookmapBtn").addEventListener("click", () => {
    location.reload();
    //console.log("add new booking");
    //bookingModal.show();
});

//shift/cell click event
function attachShiftClickListeners() {
    document.querySelectorAll("#booking-board .badge, #listTable .badge").forEach(badge => {
        // Skip click event for enquiry badges (tooltip-only)
        if (badge.classList.contains("badge-enquiries")) {
            // Ensure cursor style stays pointer
            badge.style.cursor = "pointer";
            return; // skip attaching click event
        }

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

            //Present Day & Future Days
            if (color !== legendColors.past || (color === legendColors.past && bookingid > 0)) {
                resetBooking();
                console.log(`Edit Booking → Shift: ${shift} (${shiftname}) | Date: ${date} (format: ${dateformat}, iso: ${dateiso}) | Hall: ${hallname} (${hallid}) | BookingId: ${bookingid}`);
                //Work Here..
                $("#BookingId").val(bookingid);
                // 🟢 Update label content & attributes - using jQuery
                $("#label_date_shift")
                    .text(`${date}, ${shiftname} Slot`)        // visible text
                    .attr("data-eventdate", dateiso)           // yyyy-mm-dd
                    .attr("data-shiftid", shift);              // FD, MO, etc.
                $("#banquetDropdown").val(hallid);
                setShiftTime(shift);
                $("#PaymentDetails_DueDate").val(dateiso);

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
                    bookingModal.show();
                }
            }
            //For Past Days
        });
    });
}

function setShiftTime(shiftAbbr) {
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
renderBoard = (() => {
    const originalRenderBoard = renderBoard;
    return function (...args) {
        originalRenderBoard(...args);
        //attachShiftClickListeners();
    }
})();

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

    if ($("#gstSwitch").is(":checked")) {
        let gst = getGST(hallRent);
        $("#booking_gst").val(gst.toFixed(0));
    } else {
        $("#booking_gst").val(0);
    }
}

var vendorWorkOrder_eventDate = 0;
var vendorWorkOrder_eventTime = 0;
var vendorWorkOrder_slotId = 0;
// Initial Load
document.addEventListener("DOMContentLoaded", function () {
    populateYearDropdown();
    //populateMonthButtons();
    //fetchHallDetails();
    loadBookingCalendar();
    loadLeadSourceList();
    loadEnquiryList();
    loadStateList();
    loadSubEventList();
    loadFoodPackageList();

    // When Addon Modal is about to open
    const addonModal = document.getElementById('addonChargeModal');
    const bookingModal = document.getElementById('bookingModal');
    const menuModal = document.getElementById('menuModal');
    const bookingReportModal = document.getElementById('bookingReportModal');
    const paymentModal = document.getElementById('paymentModal');
    const bookingRescheduleModal = document.getElementById('bookingRescheduleModal');

    // Create booking modal instance manually
    const bookingModalInstance = new bootstrap.Modal(bookingModal);

    addonModal.addEventListener('show.bs.modal', function () {
        // Just hide visually (do not destroy modal state)
        bookingModal.classList.remove("show");
        bookingModal.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    addonModal.addEventListener('hidden.bs.modal', function () {
        // Show booking modal visually again (no reset, same position)
        bookingModal.classList.add("show");
        bookingModal.style.display = "block";

        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });

    // Create payment modal instance manually
    //const paymentModalInstance = new bootstrap.Modal(paymentModal);

    //hide/show PAYMENT MODAL ----- START --------------------------
    paymentModal.addEventListener('show.bs.modal', function () {
        console.log("paymentModal SHOW");
        // Just hide visually (do not destroy modal state)
        bookingReportModal.classList.remove("show");
        bookingReportModal.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    paymentModal.addEventListener('hidden.bs.modal', function () {
        console.log("paymentModal HIDE");
        // Show booking modal visually again (no reset, same position)
        bookingReportModal.classList.add("show");
        bookingReportModal.style.display = "block";

        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });
    //hide/show PAYMENT MODAL ----- END --------------------------

    //hide/show vendorWorkOrder MODAL ----- START --------------------------
    vendorWorkOrderModal.addEventListener('show.bs.modal', function () {
        // Just hide visually (do not destroy modal state)
        bookingReportModal.classList.remove("show");
        bookingReportModal.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    vendorWorkOrderModal.addEventListener('hidden.bs.modal', function () {
        // Show booking modal visually again (no reset, same position)
        bookingReportModal.classList.add("show");
        bookingReportModal.style.display = "block";

        // Show backdrop again
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "block";
    });
    //hide/show PAYMENT MODAL ----- END --------------------------

    //hide/show bookingModal MODAL ----- START --------------------------
    let openedFromReport = false;
    $("#bookingModal").on("hidden.bs.modal", function () {
        if (openedFromReport) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal.classList.add("show");
            bookingReportModal.style.display = "block";
        }
    });
    //hide/show bookingModal MODAL ----- END--------------------------

    //hide bookingRescheduleModal MODAL ----- START --------------------------
    $("#bookingRescheduleModal").on("hidden.bs.modal", function () {
        if (openedFromReport) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal.classList.add("show");
            bookingReportModal.style.display = "block";
        }
    });
    //hide bookingRescheduleModal MODAL ----- END--------------------------

    //hide prepareOrderModal MODAL ----- START --------------------------
    $("#prepareOrderModal").on("hidden.bs.modal", function () {
        //console.log("openedFromReport:", openedFromReport);
        if (openedFromReport & !openedFromPrepareOrder) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal.classList.add("show");
            bookingReportModal.style.display = "block";
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
        console.log("openedFromPrepareOrder feedback:", openedFromReport);
        if (openedFromReport) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal.classList.add("show");
            bookingReportModal.style.display = "block";
        }
    });
    //hide feedback MODAL ----- END--------------------------

    //hide cancelBookingModal MODAL ----- START --------------------------
    $("#cancelBookingModal").on("hidden.bs.modal", function () {
        //console.log("openedFromReport:", openedFromReport);
        if (openedFromReport) {
            openedFromReport = false;
            //$("#bookingReportModal").modal("show");
            bookingReportModal.classList.add("show");
            bookingReportModal.style.display = "block";
        }
    });
    //hide cancelBookingModal MODAL ----- END--------------------------

    //hide/show MENU MODAL ----- START --------------------------
    menuModal.addEventListener('show.bs.modal', function () {
        // Just hide visually (do not destroy modal state)
        bookingModal.classList.remove("show");
        bookingModal.style.display = "none";

        // Also hide backdrop manually
        const backdrop = document.querySelector('.modal-backdrop');
        if (backdrop) backdrop.style.display = "none";
    });

    menuModal.addEventListener('hidden.bs.modal', function () {
        // Show booking modal visually again (no reset, same position)
        bookingModal.classList.add("show");
        bookingModal.style.display = "block";

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
        console.log("Saved Menu JSON:", jsonString);

        // Close modal
        $("#menuModal").modal("hide");
    });
    //Menu Item - START ----- START ---------------------------

    // 1️⃣ Reset to 0
    $(".btn-outline-info").on("click", function () {
        // Find all number inputs in the parent container (Other Charges) and set value to 0
        $(this).closest(".col-md-7").find("input[type='number']").val(0);
    });

    // 2️⃣ Restore Defaults
    $(".btn-outline-secondary").on("click", function () {
        console.log("reset default triggered");
        // You can later add logic to restore predefined defaults if needed
        const isChecked = $("#gstSwitch").is(":checked");
        resetOtherCharges(isChecked);
    });

    // Using jQuery
    $("#gstSwitch").on("change", function () {
        const isChecked = $(this).is(":checked");
        console.log(isChecked ? "GST Accepted" : "GST Not Accepted");
        resetOtherCharges(isChecked);
        updateHallRentGST();

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
    $("#paymentSummary_BookingAmount_Type").on("change", function () {
        $("#paymentSummary_BookingAmount_Amount").val(0);
        updateBookingDiscount();
    });

    $("#paymentSummary_BookingAmount_Amount").on("input", function () {
        updateBookingDiscount();
    });


    //SAVE BOOKING ------ START ------------------------------
    $("#btnBook").on("click", function () { saveBooking(); });
    $("#btnReset").on("click", function () { resetBooking(); });
    //SAVE BOOKING ------ END --------------------------------

    //BOOKING REPORT MODAL ---- START ------------------------------------------------
    // 🧾 Download Function Prospect
    // document.getElementById("btnDownloadPDF").addEventListener("click", function () {
    //     const bookingId = getBookingId();
    //     console.log("📄 Download Function Prospect clicked - Booking ID:", bookingId);
    // });
    document.getElementById("btnDownloadPDF").addEventListener("click", function () {
        showReportOptions();
    });

    // 🖨️ Print Function Prospect
    document.getElementById("btnEventForm").addEventListener("click", function () {
        //const bookingId = getBookingId();
        //console.log("📢 Request Feedback clicked - Booking ID:", bookingId);

        fetch('/api/Report/DownloadBookingEventReport', {
            method: 'POST',
            headers: {
                'Accept': 'application/json'
            }
        })
        .then(res => res.json())
        .then(res => {
            if (res.status === "Success" && res.data.FileUrl) {
                const link = document.createElement('a');
                link.href = res.data.FileUrl;
                link.download = '';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

            } else {
                alert("PDF Not generate");
            }
        })
        .catch(err => {
            console.error(err);
            alert("Error while downloading PDF");
        });

        //const link = document.createElement("a");
        //link.href = "/assets2/Pdf/Eventform.pdf";
        //link.download = "EventForm.pdf";
        //document.body.appendChild(link);
        //link.click();
        //document.body.removeChild(link);
    });

    // Open payment modal safely
    document.getElementById('btnEnterPayment').addEventListener('click', function () {
        const bookingId = (typeof getBookingId === 'function') ? getBookingId() : null;
        console.log("💳 Enter Payment Details clicked - Booking ID:", bookingId);
        $("#paymentModal").modal("show");
        //paymentModalInstance.show();
    });

    // 📢 Request Feedback
    document.getElementById("btnRequestFeedback").addEventListener("click", function () {
        const bookingId = getBookingId();
        console.log("📢 Request Feedback clicked - Booking ID:", bookingId);

        openedFromReport = true;
        // Just hide visually (do not destroy modal state)
        bookingReportModal.classList.remove("show");
        bookingReportModal.style.display = "none";

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

        console.log("❌ Cancel Booking clicked - Booking ID:", bookingId);
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
        bookingReportModal.classList.remove("show");
        bookingReportModal.style.display = "none";

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

        console.log("📤 Sending Cancel Request:", reqModel);

        // 🔥 API Call
        $.ajax({
            url: "/api/BookingCalendar/CancelBookingCalendar",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(reqModel),
            success: function (response) {
                console.log("✔ API Success:", response);

                if (response.status === "Success") {
                    Swal.fire({
                        icon: "success",
                        title: "Booking Cancelled",
                        text: response.message,
                        //timer: 2000
                    });

                    // Close modal
                    $("#cancelBookingModal").modal("hide");

                    // Reset textarea
                    $("#txtCancelReason").val("");

                    // 🔄 Refresh calendar or UI
                    loadBookingCalendar();
                    //CLOSE MODAL
                    bookingReportModal.hide();

                }
                else {
                    Swal.fire({
                        icon: "error",
                        title: "Failed",
                        text: response.message
                    });
                }
            },
            error: function (xhr) {
                console.error("❌ API Error:", xhr);

                Swal.fire({
                    icon: "error",
                    title: "Error Occurred",
                    text: xhr.responseJSON?.message || "Something went wrong."
                });
            }
        });

    });

    // 🗑️ Delete Booking
    document.getElementById("btnDeleteBooking").addEventListener("click", function () {
        const bookingId = getBookingId();
        console.log("🗑️ Delete Booking clicked - Booking ID:", bookingId);
        let userId = parseInt(sessionStorage.getItem('userId') ? sessionStorage.getItem('userId') : 0);

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
                    console.log("✔ Delete API Success:", res);

                    if (res.status === "Success") {
                        Swal.fire({
                            icon: "success",
                            title: "Booking Deleted",
                            text: res.message,
                            //timer: 900
                        });
                        // 🔄 Refresh calendar or UI
                        loadBookingCalendar();
                        //CLOSE MODAL
                        bookingReportModal.hide();
                    }
                    else {
                        Swal.fire({
                            icon: "error",
                            title: "Failed",
                            text: res.message
                        });
                    }
                },
                error: function (xhr) {
                    console.error("❌ Delete API Error:", xhr);

                    Swal.fire({
                        icon: "error",
                        title: "Error!",
                        text: xhr.responseJSON?.message || "Something went wrong."
                    });
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
        sessionStorage.setItem("FoodPackageData", foodPackageJson);
        // Open label page
        window.open("/BookingCalendar/FoodLabel_Print", "_blank");
    });

    // 🚚 Vendor Work Orders button click
    document.getElementById("btnVendorWorkOrders").addEventListener("click", function () {
        const bookingId = getBookingId();
        console.log("🚚 Vendor Work Orders clicked - Booking ID:", bookingId);
        console.log("bookingId:", bookingId)
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
        console.log("✏️ Edit clicked - Booking ID:", bookingId);

        openedFromReport = true;
        // Just hide visually (do not destroy modal state)
        bookingReportModal.classList.remove("show");
        bookingReportModal.style.display = "none";

        // Load data
        loadBookingCalendar(bookingId, 0, null, "editData");
        $("#bookingModal").modal("show");
    });

    // 📅 Reschedule
    document.getElementById("btnReschedule").addEventListener("click", function () {
        const bookingId = getBookingId();
        console.log("📅 Reschedule clicked - Booking ID:", bookingId);

        openedFromReport = true;
        // Just hide visually (do not destroy modal state)
        bookingReportModal.classList.remove("show");
        bookingReportModal.style.display = "none";

        // Load data
        loadBookingCalendar(bookingId, 0, null, "rescheduleData");
        $("#bookingRescheduleModal").modal("show");
    });

    // 🔔 Post Booking Followups
    // document.getElementById("btnPostBookingFollowups").addEventListener("click", function () {
    //     const bookingId = getBookingId();
    //     console.log("🔔 Post Booking Followups clicked - Booking ID:", bookingId);
    // });

    // 📋 Prepare Order (PO)
    document.getElementById("btnPrepareOrder").addEventListener("click", function () {
        const bookingId = getBookingId();
        console.log("📋 Prepare Order (PO) clicked - Booking ID:", bookingId);
        openedFromReport = true;
        // Just hide visually (do not destroy modal state)
        bookingReportModal.classList.remove("show");
        bookingReportModal.style.display = "none";

        //Load Data
        loadPrepareOrderByBooking(bookingId);
    });

    // 📤 Export Booking
    document.getElementById("btnExportBooking").addEventListener("click", function () {
        const bookingId = getBookingId();
        console.log("📤 Export Booking clicked - Booking ID:", bookingId);
    });

    //BOOKING REPORT MODAL ---- END  -------------------------------------------------
});
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
            console.log("prepareOrder - Booking API Response:", res);
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
                    <input type="number" class="form-control ing-qty" min="0" step="0.01" value="${data.Quantity || ''}">
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

    console.log("before foodItemState:", foodItemState);
    foodItemState[foodItemId].FoodFormula = {
        PeopleCount: parseInt($("#txtFormulaPeople").val()) || 1,
        IsFormulaRequired: !$("#chkFormulaNotRequired").is(":checked"),
        Ingredients: ingredients
    };
    console.log("after foodItemState:", foodItemState);
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

    console.log("Save Prepare Order Payload:", payload);

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
                Swal.fire({
                    icon: "success",
                    title: "Prepare Order Saved",
                    timer: 1200,
                    showConfirmButton: false
                });
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

        console.log("UnitMaster Response:", response);

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

    Swal.fire({
        title: "Generating PDF...",
        allowOutsideClick: false,
        didOpen: () => Swal.showLoading()
    });

    $.ajax({
        url: "/api/BookingCalendar/GeneratePackageReport",
        method: "POST",
        data: JSON.stringify({ BookingId: bookingId, IsWithPrice: isWithPrice }),
        contentType: "application/json",
        success: function (response) {

            Swal.close();

            if (response.status === "Success" && response.data?.FilePath) {
                //window.open(response.data.FilePath, "_blank");
                let fileUrl = response.data.FilePath;

                const link = document.createElement('a');
                link.href = fileUrl;
                link.download = fileUrl.split('/').pop(); // extract filename
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

            }
            else {
                Swal.fire("Error", response.message, "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error");
        }
    });
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

    paymentList.forEach((item, index) => {
        tbody.append(`
                <tr>
                    <td>${index + 1}</td>
                    <td><span class="fw-semibold text-dark">${item.Receipt_No || "—"}</span></td>
                    <td><span class="badge bg-success-subtle text-success border">${item.Mode || "—"}</span></td>
                    <td>${item.Payment_Date || "—"}</td>
                    <td>${item.Txn_Id || "—"}</td>
                    <td>${item.Cheque_Bank || "—"}</td>
                    <td>${item.Cheque_No || "—"}</td>
                    <td>${item.Cheque_Date || "—"}</td>
                    <td class="text-success fw-semibold">₹${formatNumber(item.Amount) || "0"}</td>
                    <td class="text-danger fw-semibold">₹${formatNumber(item.Due_Balance) || "0"}</td>
                    <td class="text-muted small fst-italic">${item.Note || "—"}</td>
                    <td>
                        <button class="btn btn-light btn-sm border" title="View Receipt">
                            <i class="fa-solid fa-file-invoice text-primary"></i>
                        </button>
                    </td>
                    <td>
                        <button class="btn btn-light btn-sm border" title="Delete Record">
                            <i class="fa-solid fa-trash text-danger"></i>
                        </button>
                    </td>
                </tr>
            `);
    });
}

// Helper: Format ₹ numbers with commas
function formatNumber(num) {
    if (!num || isNaN(num)) return "0";
    return Number(num).toLocaleString("en-IN");
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
        BookingId: bookingId,
        BanquetId: banquetId,
        EventDate: eventDate
    };

    $.ajax({
        url: '/api/BookingCalendar/GetBookingCalendar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.status === "Success") {
                var data = response.data;
                //console.log("Booking List:",data);
                if (actionMode === "fullData") {
                    console.log("fullData - Booking API Response:", data);
                    fetchHallDetails(data);
                }
                else if (actionMode === "singleData") {
                    resetBooking();
                    data = data[0];
                    console.log("singleData - Booking API Response:", data);
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
                    $("#bookingReport_Invoice").text("#" + data.InvoiceNo);
                    $("#bookingReport_EventDate").text(data.FromDate_Full || "—");
                    $("#bookingReport_TillDate").text(data.TillDate_Full || "—");
                    $("#bookingReport_Slot").text(data.ShiftCode || "—");
                    $("#bookingReport_BookedOn").text(data.Booked_On || "—");
                    $("#bookingReport_EventName").text(data.Booking_TypeName + "(" + data.Booking_SubEventName + ")" || "—");
                    $("#bookingReport_Banquet").text(data.BanquetPropertyName || "—");
                    $("#bookingReport_Hall").text(data.BanquetName || "—");
                    $("#bookingReport_Pax").text(data.Booking_PAX || "—");

                    vendorWorkOrder_eventDate = data.EventDate;
                    vendorWorkOrder_eventTime = data.StartTime;
                    vendorWorkOrder_slotId = data.SlotId;

                    // Services & Charges
                    $("#bookingReport_HallRent").text(data.Booking_HallRent || "0");
                    if (data.JSON_FoodPackage && data.JSON_FoodPackage.length > 0) {
                        $("#bookingReport_FoodAmount").text(data.JSON_FoodPackage[0].PackageCost || "0");
                        $("#bookingReport_FoodRate").text(data.JSON_FoodPackage[0].PerPlate || "—");
                        //here contnue for food label
                        // ✅ Store FULL package array in button as JSON
                        $("#btnFoodLabels").data("foodpackage", JSON.stringify(data.JSON_FoodPackage));
                        //$("#btnFoodLabels").prop("disabled", false);
                    }
                    else {
                        $("#bookingReport_FoodAmount").text("0");
                        $("#bookingReport_FoodRate").text("—");
                        //$("#btnFoodLabels").prop("disabled", true);
                    }

                    // Payment Trail
                    $("#bookingReport_DealAmount").text(data.PaymentSummary_Total || "0");
                    var taxAmount = (data.PaymentSummary_CGST + data.PaymentSummary_SGST + data.PaymentSummary_IGST);
                    $("#bookingReport_TaxAmount").text(taxAmount || "0");
                    $("#bookingReport_BookingDiscount").text(data.PaymentSummary_BookingDiscount_Total || "0");
                    $("#bookingReport_TotalAmount").text(data.PaymentSummary_PayableAmount || "0");
                    $("#bookingReport_PaidTillDate").text(data.Payment_Amount || "0");
                    $("#bookingReport_Discount").text(data.PaymentSummary_BookingDiscount_Type_Amount || "0");
                    $("#bookingReport_OutstandingAmount").text((data.PaymentSummary_PayableAmount - data.Payment_Amount) || "0");

                    //Payment Details Table
                    // fillPaymentTrailTable(data.PaymentTrailList);

                    // Food Package and Selected Menu
                    $("#bookingReport_FoodPackageName").text(data?.JSON_FoodPackage?.[0]?.FoodPackageName ?? "—");

                    $("#bookingReport_FoodPackageUpdatedBy").text(data?.Booked_By_EmpName || "—");
                    $("#bookingReport_FoodPackageUpdatedOn").text(data?.Booked_On || "—");

                    // $("#bookingReport_ReachedUs").text(data?.ReachedUs || "—");
                    // $("#bookingReport_Reference").text(data?.Reference || "—");

                    //Open REPORT MODAL
                    bookingReportModal.show();
                }
                else if (actionMode === "editData") {
                    resetBooking();
                    data = data[0];
                    console.log("editData - Booking API Response:", data);
                    $("#BookingId").val(bookingId);
                    // 🟢 Update label content & attributes - using jQuery
                    $("#label_date_shift")
                        .text(`${data.EventDate_Format}, ${fullShiftName(data.ShiftCode)} Slot`)        // visible text
                        .attr("data-eventdate", data.EventDate)           // yyyy-mm-dd
                        .attr("data-shiftid", data.ShiftCode);              // FD, MO, etc.

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
                    $("#clientStateDropdown").val(data.Client_StateId);

                    $("#booking_subEventDropdown").val(data.Booking_SubEventId);
                    $("#booking_pax").val(data.Booking_PAX);
                    $("#booking_hallRent").val(data.Booking_HallRent);
                    $("#booking_gst").val(data.Booking_GST);
                    //$("#booking_leadReferenceDropdown").val(data.Booking_LeadReferenceId);
                    loadLeadSourceList(data.Booking_LeadReferenceId);

                    //menu type radio
                    $("input[name='menuType']").prop("checked", false);
                    $("input[name='menuType'][value='" + data.Booking_MenuType_Id + "']").prop("checked", true).trigger("change");

                    var foodpackage = data.JSON_FoodPackage[0];
                    $("#foodPackage_PackageDropdown").val(foodpackage.FoodPackageId);
                    $("#foodPackage_RatePerPlate").val(foodpackage.PerPlate);
                    $("#foodPackage_PackageCost").val(foodpackage.PackageCost);
                    $("#foodPackage_Addons").val(foodpackage.Addons);
                    $("#foodPackage_GST").val(foodpackage.GST);
                    //AddOns btn
                    //console.log("edit AddOnsList:", foodpackage.AddOnsList);
                    var addOnsList = foodpackage.AddOnsList || [];
                    $("#foodPackage_AddOnItems").data("selected-AddOnItems", addOnsList);
                    // Update button UI
                    if (addOnsList.length > 0) {
                        $("#foodPackage_AddOnItems").html(`<i class="fa fa-check text-success"></i> Addons (${addOnsList.length})`);
                    } else {
                        $("#foodPackage_AddOnItems").html(`<i class="fa fa-check"></i> Addons`);
                    }
                    $("#foodPackage_TotalValue").text(foodpackage.TotalCharges);

                    $("#BookingType").val(data.Booking_TypeId);
                    $("#AgentCheck").prop("checked", !!foodpackage.TotalCharges);

                    //othercharges
                    $("#OtherCharges_Decoration_Amount").val(data.OtherCharges_Decoration_Amount);
                    $("#OtherCharges_Decoration_GST").val(data.OtherCharges_Decoration_GST);
                    $("#OtherCharges_RoomCharges_Amount").val(data.OtherCharges_RoomCharges_Amount);
                    $("#OtherCharges_RoomCharges_GST").val(data.OtherCharges_RoomCharges_GST);

                    $("#OtherCharges_SoundSystem_Amount").val(data.OtherCharges_SoundSystem_Amount);
                    $("#OtherCharges_SoundSystem_GST").val(data.OtherCharges_SoundSystem_GST);
                    $("#OtherCharges_ProjectorScreen_Amount").val(data.OtherCharges_ProjectorScreen_Amount);
                    $("#OtherCharges_ProjectorScreen_GST").val(data.OtherCharges_ProjectorScreen_GST);

                    var quillCustomer = new Quill('#OtherCharges_customerEditor', {
                        theme: 'snow'
                    });
                    var quillInternal = new Quill('#OtherCharges_internalEditor', {
                        theme: 'snow'
                    });
                    quillCustomer.root.innerHTML = data.OtherCharges_BookingNoteForCustomer || "";
                    quillInternal.root.innerHTML = data.OtherCharges_InternalOfficeNote || "";

                    //payment summary
                    $("#paymentSummary_TaxableAmount").text(formatAmount(data.PaymentSummary_TaxableAmount));
                    $("#paymentSummary_CGST").text(formatAmount(data.PaymentSummary_CGST));
                    $("#paymentSummary_SGST").text(formatAmount(data.PaymentSummary_SGST));
                    $("#paymentSummary_IGST").text(formatAmount(data.PaymentSummary_IGST));
                    $("#paymentSummary_Total").text(formatAmount(data.PaymentSummary_TotalAmount));
                    // Booking Amount Type & Input
                    $("#paymentSummary_BookingAmount_Type").val(data.PaymentSummary_BookingDiscount_Type_Id || 1).trigger("change");
                    $("#paymentSummary_BookingAmount_Amount").val(data.PaymentSummary_BookingDiscount_Type_Amount || 0).trigger("input");
                    // Booking Amount Calculated
                    $("#paymentSummary_BookingAmount").text(formatAmount(data.PaymentSummary_BookingDiscount_Total));
                    // Payable Amount
                    $("#paymentSummary_payableAmount").text(formatAmount(data.PaymentSummary_PayableAmount));

                    $("#Section_PaymentDetails").hide();
                }
                else if (actionMode === "rescheduleData") {
                    //resetBooking();
                    data = data[0];
                    console.log("rescheduleData - Booking API Response:", data);

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
            handleApiError(xhr, status, error, "getBooking fetch fail")

        },
        complete: function () {
            //hideLoader();
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
        }
    });
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

function saveBooking() {
    //const form = $("#bookingForm");
    const $label = $("#label_date_shift");

    let userId = parseInt(sessionStorage.getItem('userId') ? sessionStorage.getItem('userId') : 0);

    var quillCustomer = new Quill('#OtherCharges_customerEditor', {
        theme: 'snow'
    });

    var quillInternal = new Quill('#OtherCharges_internalEditor', {
        theme: 'snow'
    });

    var customerNote = quillCustomer.root.innerHTML;
    var internalNote = quillInternal.root.innerHTML;

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

        // ✅ Payment Summary
        PaymentSummary_TaxableAmount: parseFloat($("#paymentSummary_TaxableAmount").text() || 0),
        PaymentSummary_CGST: parseFloat($("#paymentSummary_CGST").text() || 0),
        PaymentSummary_SGST: parseFloat($("#paymentSummary_SGST").text() || 0),
        PaymentSummary_IGST: parseFloat($("#paymentSummary_IGST").text() || 0),
        PaymentSummary_Total: parseFloat($("#paymentSummary_Total").text() || 0),
        PaymentSummary_BookingDiscount_Type_Id: parseInt($("#paymentSummary_BookingAmount_Type").val() || 0),
        PaymentSummary_BookingDiscount_Type_Amount: parseFloat($("#paymentSummary_BookingAmount_Amount").val() || 0),
        PaymentSummary_BookingDiscount_Total: parseFloat($("#paymentSummary_BookingAmount").text() || 0),
        PaymentSummary_PayableAmount: parseFloat($("#paymentSummary_payableAmount").text() || 0),

        ActionBy: userId
    };

    // 🔹 2. Build Food Packages JSON --here
    const foodPackages = [];

    $(".foodPackageContainer > .food-block, #foodPackageContainer > .food-row").each(function () {
        const $row = $(this);

        // Check if it's a dynamic block (has food-block class) or first row
        const isDynamicBlock = $row.hasClass('food-block');

        // Get common elements
        const $dropdown = $row.find(".foodPackageDropdown, #foodPackage_PackageDropdown");
        const selectedMenuItems = $dropdown.data("selectedItems") || [];

        const $dropdown_AddOns = $row.find(".foodPackage_AddOnItems");
        const selectedAddOnCharges = $dropdown_AddOns.data("selected-AddOnItems") || [];

        // Create base package object
        const foodPackage = {
            Id: parseInt($row.find("package_row_id").val() || 0),
            PackageId: parseInt($dropdown.val()),
            PerPlate: parseFloat($row.find(".perPlate, #foodPackage_RatePerPlate").val() || 0),
            PackageCost: parseFloat($row.find(".packageCost, #foodPackage_PackageCost").val() || 0),
            AddonsCost: parseFloat($row.find(".addonsCost, #foodPackage_Addons").val() || 0),
            GST: parseFloat($row.find(".gstCost, #foodPackage_GST").val() || 0),
            TotalCharges: parseFloat($row.find(".foodPackage_TotalValue, #foodPackage_TotalValue").text() || 0),
            MenuItems: selectedMenuItems,
            AddOns: selectedAddOnCharges
        };

        // Add additional fields for dynamic blocks
        if (isDynamicBlock) {
            console.log("new row added");
            const $packageNameInput = $row.find('input[placeholder="Addon Food Package Name"]');
            const $paxInput = $row.find('input[placeholder="Pax"]');
            const $dateInput = $row.find('input[type="date"]');
            const $hourInput = $row.find('input[placeholder="HH"]');
            const $minuteInput = $row.find('input[placeholder="MM"]');
            const $ampmSelect = $row.find('select');

            // Construct time string (HH:MM AM/PM)
            const hour = $hourInput.val() || '12';
            const minute = $minuteInput.val() || '00';
            const ampm = $ampmSelect.val() || 'AM';
            const timeString = `${hour}:${minute} ${ampm}`;

            // Add additional fields
            foodPackage.FoodPackageTitleName = $packageNameInput.val() || '';
            foodPackage.Pax = $paxInput.val() || 0;
            foodPackage.FoodPackageDateTime = $dateInput.val() ? `${$dateInput.val()} ${timeString}` : '';
        }
        else {
            console.log("static row only");
            // For first row, set default values for optional fields
            foodPackage.FoodPackageTitleName = '';
            foodPackage.Pax = 0;
            foodPackage.FoodPackageDateTime = '';
        }

        foodPackages.push(foodPackage);
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
            console.log("save response:", response);
            if (response.status === "Success") {
                Swal.fire({
                    position: "top-end",
                    icon: "success",
                    title: "Saved!",
                    showConfirmButton: false,
                    timer: 900
                });
                resetBooking();
                loadBookingCalendar();
                //CLOSE MODAL
                //bookingModal.hide();
                $("#bookingModal").modal("hide");
                if (bookingData.BookingId > 0) {
                    var banquetId = 0;
                    var eventDate = null;
                    var actionMode = "singleData";
                    loadBookingCalendar(bookingData.BookingId, banquetId, eventDate, actionMode);
                }
            }
            else {
                Swal.fire({
                    title: "Booking Save Failed",
                    text: response.message,
                    icon: "error"
                });
            }
        },
        error: function (xhr) {
            console.error("Error saving booking:", xhr.responseText);
            Swal.fire({
                title: "Booking Save Error",
                text: xhr.responseText,
                icon: "error"
            });
        },
        complete: function () {
            $("#btnSaveBooking").prop("disabled", false).text("Save Booking");
            //hideLoader();
        },
        beforeSend: function () {
            //showLoader(); // Optionally, you can use beforeSend here as well
        }
    });
}

function resetBooking() {
    // Reset all form fields
    $("#bookingForm").trigger("reset");
    // Reset any special fields
    $("#gstSwitch").prop("checked", false);
    $("#gstSwitchText").text("Off").removeClass("text-success").addClass("text-danger");  // Red for OFF
    $("#foodPackageContainer .foodPackage_TotalValue").text("0");
    $("#addonItemsContainer").empty();
    $("#headerSummaryContainer").empty();

    // ✅ Store FULL package array in button as JSON
    $("#btnFoodLabels").data("foodpackage", "");

    // Reset amounts to defaults
    resetOtherCharges(false);
    $("#Section_PaymentDetails").show();

    console.log("Booking form reset completed.");
}

function resetOtherCharges(isGSTEnabled) {

    // Base amounts (unchanged)
    decorationAmount = 3000;
    roomchargesAmount = 0;
    soundSystemAmount = 1500;
    projectScreenAmount = 3500;

    // Apply GST If Enabled
    decorationGST = isGSTEnabled ? getGST(decorationAmount) : 0;
    roomchargesGST = isGSTEnabled ? getGST(roomchargesAmount) : 0;
    soundSystemGST = isGSTEnabled ? getGST(soundSystemAmount) : 0;
    projectScreenGST = isGSTEnabled ? getGST(projectScreenAmount) : 0;

    // Update textboxes
    $("#OtherCharges_Decoration_Amount").val(decorationAmount);
    $("#OtherCharges_Decoration_GST").val(decorationGST);

    $("#OtherCharges_RoomCharges_Amount").val(roomchargesAmount);
    $("#OtherCharges_RoomCharges_GST").val(roomchargesGST);

    $("#OtherCharges_SoundSystem_Amount").val(soundSystemAmount);
    $("#OtherCharges_SoundSystem_GST").val(soundSystemGST);

    $("#OtherCharges_ProjectorScreen_Amount").val(projectScreenAmount);
    $("#OtherCharges_ProjectorScreen_GST").val(projectScreenGST);

    // Taxable Amount
    var taxableAmount = decorationAmount + roomchargesAmount + soundSystemAmount + projectScreenAmount;
    $("#paymentSummary_TaxableAmount").text(taxableAmount);

    // GST Calculation
    totalGST = decorationGST + roomchargesGST + soundSystemGST + projectScreenGST;

    cgst = isGSTEnabled ? (totalGST / 2).toFixed(2) : 0;
    sgst = isGSTEnabled ? (totalGST / 2).toFixed(2) : 0;
    igst = 0;

    $("#paymentSummary_CGST").text(cgst);
    $("#paymentSummary_SGST").text(sgst);
    $("#paymentSummary_IGST").text(igst);

    // Total
    total = parseFloat(taxableAmount) + parseFloat(cgst) + parseFloat(sgst) + parseFloat(igst);
    $("#paymentSummary_Total").text(total);

    // Reset Booking Amount (Discount Input)
    $("#paymentSummary_BookingAmount_Type").val("1"); // default flat
    $("#paymentSummary_BookingAmount_Amount").val(0);
    // Reset Display
    $("#paymentSummary_BookingAmount").text("0.00");

    // Booking Discount
    bookingDiscount = 0;
    // $("#paymentSummary_BookingAmount").text(bookingDiscount);

    // Show / Hide booking amount row
    if (isGSTEnabled) {
        $("#paymentSummary_BookingAmount_Row").hide();
    } else {
        $("#paymentSummary_BookingAmount_Row").show();
    }

    // Final payable amount
    payableAmount = total; + bookingDiscount;
    $("#paymentSummary_payableAmount").text(payableAmount);
}

function updateBookingDiscount() {
    let total = parseFloat($("#paymentSummary_Total").text()) || 0;
    let type = $("#paymentSummary_BookingAmount_Type").val();   // 1 = flat, 2 = percent
    let enteredValue = parseFloat($("#paymentSummary_BookingAmount_Amount").val()) || 0;

    let bookingDiscount = 0;

    if (type == "1") {
        // Flat Discount
        bookingDiscount = enteredValue;
    }
    else if (type == "2") {
        // Percentage Discount
        bookingDiscount = (total * enteredValue) / 100;
    }

    // Prevent discount > total
    if (bookingDiscount > total) bookingDiscount = total;

    // Update discount field
    $("#paymentSummary_BookingAmount").text(bookingDiscount.toFixed(2));

    // Update payable amount
    let payableAmount = total - bookingDiscount;
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

    console.log("🚚 Vendor Work Order JSON:", workOrderData);

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
            console.log("Vendor Work Order Save Response:", response);
            if (response.status === "Success") {
                Swal.fire({
                    position: "top-end",
                    icon: "success",
                    title: "Vendor Work Order Saved!",
                    showConfirmButton: false,
                    timer: 900
                });

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

