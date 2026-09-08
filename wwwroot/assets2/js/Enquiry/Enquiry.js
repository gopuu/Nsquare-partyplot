function setShiftTime(shiftAbbr) {
    //console.log("shiftTimes:", shiftTimes, ", shiftAbbr:", shiftAbbr);

    const times = shiftTimes[shiftAbbr];
    if (!times) return;

    //$("#eventStart").val(convertTo24Hour(times.start));
    //$("#eventEnd").val(convertTo24Hour(times.end));

    $("#eventStart").val(times.start);
    $("#eventEnd").val(times.end);
}

//QUOTATION - START ------------------------------------------------------------------------------------------
function resetQuotationForm() {

    // ===== Basic =====
    $("#quotationId").val(0);
    $("#quotation_enquiryDealId").val(0);

    // ===== Banquet / Slot / Date =====
    $("#quotation_banquetId").val("0");
    $("#quotation_slotId").val("0");
    $("#quotation_fromDate").val("");
    $("#quotation_tillDate").val("");

    $("#quotation_startTime").val("");
    $("#quotation_endTime").val("");

    // ===== Client Section =====
    $("#quotation_clientId").val(null).trigger("change");
    $("#quotation_clientName").val("");
    $("#quotation_clientAddress").val("");
    $("#quotation_clientEmail").val("");
    $("#quotation_clientContact").val("");
    $("#quotation_clientGstIn").val("");
    $("#quotation_clientStateId").val("0");

    // ===== Booking Details =====
    $("#quotation_eventTypeId").val("0");
    $("#quotation_subEventTypeId").val("0");
    $("#quotation_banquetRent").val(0);
    $("#quotation_pax").val(0);
    $("#quotation_referenceTypeId").val("0");

    // ===== Charges =====
    $("#quotation_decorationAmount").val(0);
    $("#quotation_soundAmount").val(0);
    $("#quotation_projectorAmount").val(0);

    // ===== Print Flags =====
    $("#quotation_isPrintExclusive").prop("checked", false);
    $("#quotation_isPrintFoodItems").prop("checked", false);

    // ===== Totals =====
    $("#quotation_subTotal").val(0);
    $("#quotation_taxAmount").val(0);
    $("#quotation_grandTotal").val(0);

    // ===== Packages =====
    $(".package-checkbox").prop("checked", false);

    // ===== Other Services Reset =====
    $("#quotation_otherServicesContainer").html(`
        <div class="row g-1 quotation-service-row mb-1">
            <div class="col-md-3">
                <label class="form-label compact-label">Service</label>
                <input type="text" class="form-control form-control-sm quotation-service-name p-1" placeholder="Service" />
            </div>
            <div class="col-md-2">
                <label class="form-label compact-label">Amount</label>
                <input type="number" class="form-control form-control-sm quotation-service-amount p-1" value="0" />
            </div>
            <div class="col-md-2">
                <label class="form-label compact-label">Qty</label>
                <input type="number" class="form-control form-control-sm quotation-service-qty p-1" value="0" />
            </div>
            <div class="col-md-2">
                <label class="form-label compact-label">Total</label>
                <input type="number" class="form-control form-control-sm quotation-service-total bg-light p-1" value="0" readonly />
            </div>
            <div class="col-md-3">
                <label class="form-label compact-label">Description</label>
                <textarea class="form-control form-control-sm quotation-service-desc p-1" placeholder="Description"></textarea>
            </div>
        </div>
    `);

    // Optional: recalc to ensure totals consistent
    quotation_calculateTotals();

    $("#quotation_btnSave").show();
    $("#quotationModal").removeClass("view-mode");
    $("#quotation_action").val("");

    enableQuotationForm();
}
function openQuotation(enquiryDealId, action = "add", isleadlost=false, isleadbooked=false) {
    resetQuotationForm();

    var payload = { EnquiryId: enquiryDealId, QuotationId: 0 };

    $.ajax({
        url: "/api/Quotation/GetQuotationByEnquiryId",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            console.log("API Response:", response);

            if (response.status !== "Success") return;

            const data = response.data[0]; // single enquiry
            const quotations = data.Quotations || [];

            console.log("Quotations:", quotations);

            /* ================= CASE 1: NO QUOTATION ================= */
            if (quotations.length === 0 || action.toLowerCase() === "add") {

                console.log("Case 1: No quotation");

                fillEnquiryToQuotationForm(data);

                $("#quotationModal .modal-title").text("Add Quotation");
                $("#quotation_action").val("add");
                $("#quotationModal").modal("show");
                
                if(isleadbooked || isleadlost){
                    $("#quotation_btnSaveQuotation").addClass("d-none");                
                }
                else{
                    $("#quotation_btnSaveQuotation").removeClass("d-none");                                
                }
            }

            /* ================= CASE 2: ONE QUOTATION ================= */
            /*
            else if(quotations.length === 1) {

                console.log("Case 2: Single quotation");

                fillEnquiryToQuotationForm(data);
                fillQuotationToForm(quotations[0]);

                $("#quotationModal .modal-title").text("Edit Quotation");
                $("#quotationModal").modal("show");
            }
            */

            /* ================= CASE 3: MULTIPLE ================= */
            else {
                console.log("Case 3: Multiple quotations");
                // 🔹 store enquiryId for reuse
                $("#quotationList_enquiryDealId").val(enquiryDealId);
                bindQuotationList(data, quotations, isleadlost, isleadbooked);
                $("#quotationListModal").modal("show");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Quotation Fetch Error");
        }
    });
}
function fillEnquiryToQuotationForm(e) {
    //console.log("enquiry data:", e);
    $("#quotation_enquiryDealId").val(e.EnquiryId);

    $("#quotation_banquetId").val(e.BanquetHallId);
    $("#quotation_slotId").val(e.SlotId);
    $("#quotation_fromDate").val(e.EventDate);
    $("#quotation_tillDate").val(e.EventDate);
    $("#quotation_startTime").val(e.EventStartTime1);
    $("#quotation_endTime").val(e.EventEndTime1);

    $("#quotation_clientId").val(e.EnquiryId);
    $("#quotation_clientName").val(e.Name);
    $("#quotation_clientContact").val(e.Contact);
    $("#quotation_clientEmail").val(e.Email);
    $("#quotation_clientAddress").val(e.Address);
    //$("#quotation_clientGstIn").val("");
    $("#quotation_clientStateId").val(e.StateId);

    $("#quotation_eventTypeId").val(e.EventTypeId);
    $("#quotation_subEventTypeId").val(e.EventSubTypeId);

    //$("#quotation_banquetRent").val(0);
    $("#quotation_pax").val(e.Pax);
    $("#quotation_referenceTypeId").val(e.LeadSourceId);
}
function fillQuotationToForm(q) {

    $("#quotationId").val(q.Id);

    //$("#quotation_banquetId").val(q.BanquetId);
    //$("#quotation_slotId").val(q.SlotId);

    //$("#quotation_fromDate").val(q.FromDate);
    //$("#quotation_tillDate").val(q.TillDate);

    //$("#quotation_startTime").val(q.StartTime);
    //$("#quotation_endTime").val(q.EndTime);

    $("#quotation_banquetRent").val(q.BanquetRent);
    //$("#quotation_pax").val(q.Pax);

    $("#quotation_decorationAmount").val(q.DecorationChargesAmount);
    $("#quotation_soundAmount").val(q.SoundSystemChargesAmount);
    $("#quotation_projectorAmount").val(q.ProjectorChargesAmount);

    $("#quotation_isPrintExclusive").prop("checked", q.IsPrintPriceExclusive);
    $("#quotation_isPrintFoodItems").prop("checked", q.IsPrintFoodItems);

    //food package - q.Packages_JSON
    // Reset first
    $(".package-checkbox").prop("checked", false);
    // Set selected
    if (q.Packages_JSON && q.Packages_JSON.length > 0) {
        q.Packages_JSON.forEach(p => {
            $(`.package-checkbox[value='${p.PackageId}']`).prop("checked", true);
        });
    }

    //other services - q.OtherServices_JSON
    let html = "";
    if (q.OtherServices_JSON && q.OtherServices_JSON.length > 0) {
        q.OtherServices_JSON.forEach(s => {
            html += `
                <div class="row g-1 quotation-service-row mb-1">
            
                    <div class="col-md-3">
                        <input type="text" class="form-control form-control-sm quotation-service-name p-1"
                               value="${s.ServiceName || ""}" />
                    </div>

                    <div class="col-md-2">
                        <input type="number" class="form-control form-control-sm quotation-service-amount p-1"
                               value="${s.Amount || 0}" />
                    </div>

                    <div class="col-md-2">
                        <input type="number" class="form-control form-control-sm quotation-service-qty p-1"
                               value="${s.Quantity || 0}" />
                    </div>

                    <div class="col-md-2">
                        <input type="number" class="form-control form-control-sm quotation-service-total bg-light p-1"
                               value="${s.TotalAmount || 0}" readonly />
                    </div>

                    <div class="col-md-3">
                        <textarea class="form-control form-control-sm quotation-service-desc p-1">${s.ServiceDescription || ""}</textarea>
                    </div>

                </div>`;
        });

    }
    else {
        // fallback (empty row)
        html = `
            <div class="row g-1 quotation-service-row mb-1">
                <div class="col-md-3">
                    <input type="text" class="form-control form-control-sm quotation-service-name p-1" />
                </div>
                <div class="col-md-2">
                    <input type="number" class="form-control form-control-sm quotation-service-amount p-1" value="0" />
                </div>
                <div class="col-md-2">
                    <input type="number" class="form-control form-control-sm quotation-service-qty p-1" value="0" />
                </div>
                <div class="col-md-2">
                    <input type="number" class="form-control form-control-sm quotation-service-total bg-light p-1" value="0" readonly />
                </div>
                <div class="col-md-3">
                    <textarea class="form-control form-control-sm quotation-service-desc p-1"></textarea>
                </div>
            </div>`;
    }
    $("#quotation_otherServicesContainer").html(html);

    $("#quotation_subTotal").val(q.SubTotal);
    $("#quotation_taxAmount").val(q.TaxAmount || 0);
    $("#quotation_grandTotal").val(q.GrandTotal);
}
function bindQuotationList(enquiry, quotations, isleadlost = false, isleadbooked = false) {
    $("#quotationsListTitle").text(`Quotations List - ${enquiry.Name}`);
    let html = "";
    quotations.forEach(q => {

        html += `
        <tr>
            <td>${q.Id}</td>
            <td>${q.SlotName} (${q.StartTime1}-${q.EndTime1})</td>
            <td>${q.FromDate2}</td>
            <td>${enquiry.Name}</td>
            <td>₹ ${q.GrandTotal}</td>
            <td>
                <div class="d-flex gap-1">
                    <button class="btn btn-xs btn-outline-warning btn-edit-quotation" data-id="${q.Id}" title="Edit">
                        <i class="fa fa-edit"></i>
                    </button>

                    <button class="btn btn-xs btn-outline-danger btn-delete-quotation" data-id="${q.Id}" title="Delete">
                        <i class="fa fa-trash"></i>
                    </button>

                    <button class="btn btn-xs btn-outline-info btn-view-quotation" data-id="${q.Id}" title="View">
                        <i class="fa fa-eye"></i>
                    </button>
                </div>
            </td>
        </tr>`;
    });

    if (isleadlost || isleadbooked) {
        $("#btnAddNewQuotation, .btn-edit-quotation, .btn-delete-quotation").addClass("d-none");
    }
    else {
        $("#btnAddNewQuotation, .btn-edit-quotation, .btn-delete-quotation").removeClass("d-none");
    }

    $("#quotationListBody").html(html);
}

$(document).on("click", "#btnAddNewQuotation", function () {
    let enquiryId = $("#quotationList_enquiryDealId").val();
    console.log("Add New Quotation for:", enquiryId);
    $("#quotationListModal").modal("hide");
    // 🔥 Call your function
    openQuotation(enquiryId, "add");
});

$(document).on("click", ".btn-edit-quotation, .btn-view-quotation", function () {

    const id = $(this).data("id");
    const isEdit = $(this).hasClass("btn-edit-quotation");
    const isView = $(this).hasClass("btn-view-quotation");

    console.log("Quotation Action Clicked:", id);

    /* ================= EDIT / VIEW ================= */
    var payload = {
        EnquiryId: 0,
        QuotationId: id // 🔥 new param (quotationId)
    };

    console.log("payload:", payload);
    $.ajax({
        url: "/api/Quotation/GetQuotationByEnquiryId",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            console.log("edit-view response:", response);
            if (response.status !== "Success") return;

            $("#quotationListModal").modal("hide");

            const data = response.data[0];
            const quotations = data.Quotations || [];

            if (quotations.length === 0) return;

            const q = quotations[0];

            console.log("Loaded Quotation:", q);

            // 🔹 Reset & Fill
            resetQuotationForm();
            fillEnquiryToQuotationForm(data);
            fillQuotationToForm(q);

            // 🔹 Mode Handling
            if (isEdit) {
                console.log("Mode: EDIT");
                $("#quotation_action").val("edit");

                enableQuotationForm();

                $("#quotationModal .modal-title").text("Edit Quotation");
            }

            if (isView) {
                console.log("Mode: VIEW");
                $("#quotation_action").val("view");

                disableQuotationForm();
                $("#quotation_btnSave").hide(); // or disable

                $("#quotationModal").addClass("view-mode");
                $("#quotationModal .modal-title").text("View Quotation");
            }

            $("#quotationModal").modal("show");
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Quotation Fetch Error");
        }
    });
});
function enableQuotationForm() {
    $("#quotationModal")
        .find("input, select, textarea, button")
        .prop("disabled", false);
}
function disableQuotationForm() {

    $("#quotationModal")
        .find("input, select, textarea, button")
        .prop("disabled", true);

    // Keep close button enabled
    $("#quotationModal .btn-close").prop("disabled", false);
}

$(document).on("click", ".btn-delete-quotation", function () {
    let $btn = $(this); // ✅ store reference
    let quotationId = $btn.data("id");
    let enquiryId = $("#quotationList_enquiryDealId").val(); // or pass from context

    console.log("Delete Quotation Clicked:", quotationId);

    Swal.fire({
        title: "Are you sure?",
        text: "This quotation will be permanently deleted.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6",
        confirmButtonText: "Yes, delete it"
    }).then((result) => {

        if (result.isConfirmed) {

            const deleteObj = {
                Id: quotationId,
                ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
            };

            $.ajax({
                url: "/api/Quotation/DeleteQuotation",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(deleteObj),
                success: function (res) {
                    if (res.status === "Success") {
                        handleApiSuccess("Deleted!");
                        console.log("Quotation deleted:", quotationId);

                        // 🔥 Refresh list modal (IMPORTANT)
                        loadEnquiryDeals();
                        //openQuotation(enquiryId, "view");

                        $btn.closest("tr").remove();
                        // Optional: close modal if needed
                        //$("#quotationListModal").modal("hide");
                    }
                },
                error: function (xhr, error, status) {
                    handleApiError(xhr, error, status, "Failed to delete quotation");
                }
            });
        }
    });
});

function quotation_calculateTotals() {
    let pax = parseInt($("#quotation_pax").val()) || 0;

    let packageTotal = 0;

    // Calculate selected packages
    $(".package-checkbox:checked").each(function () {
        let rate = parseFloat($(this).data("rate")) || 0;
        packageTotal += rate * pax;
    });

    // Calculate other services
    let serviceTotal = 0;
    $(".quotation-service-total").each(function () {
        serviceTotal += parseFloat($(this).val()) || 0;
    });

    // Other charges
    let decoration = parseFloat($("#quotation_decorationAmount").val()) || 0;
    let sound = parseFloat($("#quotation_soundAmount").val()) || 0;
    let projector = parseFloat($("#quotation_projectorAmount").val()) || 0;
    let banquetRent = parseFloat($("#quotation_banquetRent").val()) || 0;

    let subTotal = packageTotal + serviceTotal + decoration + sound + projector + banquetRent;

    let tax = 0; // subTotal * 0.18; // 18% GST

    let netTotal = subTotal + tax;

    $("#quotation_subTotal").val(subTotal.toFixed(2));
    $("#quotation_taxAmount").val(tax.toFixed(2));
    $("#quotation_grandTotal").val(netTotal.toFixed(2));
}

// Add Service Row
$("#quotation_btnAddService").click(function () {
    let row = `
        <div class="row g-1 quotation-service-row mb-1 align-items-center">
            <div class="col-md-3">
                <input type="text" class="form-control quotation-service-name p-1" placeholder="Service" />
            </div>
            <div class="col-md-2">
                <input type="number" class="form-control quotation-service-amount p-1" placeholder="Amount" value="0" />
            </div>
            <div class="col-md-2">
                <input type="number" class="form-control quotation-service-qty p-1" value="0" />
            </div>
            <div class="col-md-2">
                <input type="number" class="form-control quotation-service-total bg-light p-1" value="0" readonly />
            </div>
            <div class="col-md-2">
                <textarea class="form-control quotation-service-desc p-1" placeholder="Description"></textarea>
            </div>
            <div class="col-md-1 text-center">
                <button type="button" class="btn btn-sm btn-danger p-2 quotation-btn-remove-service">
                    <i class="fa fa-trash"></i>
                </button>
            </div>
        </div>`;

    $("#quotation_otherServicesContainer").append(row);
});

// Remove Service Row (IMPORTANT: use event delegation)
$(document).on("click", ".quotation-btn-remove-service", function () {
    $(this).closest(".quotation-service-row").remove();
    quotation_calculateTotals();
});

//Auto calc
$(document).on("input", ".quotation-service-amount, .quotation-service-qty", function () {
    let row = $(this).closest(".quotation-service-row");

    let amount = parseFloat(row.find(".quotation-service-amount").val()) || 0;
    let qty = parseInt(row.find(".quotation-service-qty").val()) || 0;

    let total = amount * qty;

    row.find(".quotation-service-total").val(total.toFixed(2));

    quotation_calculateTotals();
});

$(document).on("change input", "#quotation_pax, .package-checkbox", function () {
    quotation_calculateTotals();
});

$("#quotation_decorationAmount, #quotation_soundAmount, #quotation_projectorAmount, #quotation_banquetRent").on("input", function () {
    quotation_calculateTotals();
});

// On Slot Change
$(document).on("change", "#quotation_slotId", function () {
    const selectedOption = $(this).find("option:selected");

    const startTime = selectedOption.data("start");
    const endTime = selectedOption.data("end");

    if ($(this).val() != "0") {
        $("#quotation_startTime").val(startTime || "");
        $("#quotation_endTime").val(endTime || "");
    } else {
        $("#quotation_startTime").val("");
        $("#quotation_endTime").val("");
    }
});

function getPackagesJson() {
    let list = [];
    $(".package-checkbox:checked").each(function () {
        list.push({
            PackageId: parseInt($(this).val())
        });
    });

    return list; // ✅ NOT string
}

function getOtherServicesJson() {
    let list = [];
    $(".service-row").each(function (index) {

        let name = $(this).find(".service-name").val();
        let amount = parseFloat($(this).find(".service-amount").val()) || 0;
        let qty = parseInt($(this).find(".service-qty").val()) || 0;
        let total = parseFloat($(this).find(".service-total").val()) || 0;
        let desc = $(this).find(".service-desc").val();

        if (name) {
            list.push({
                SrNo: index + 1,
                ServiceName: name,
                ServiceDescription: desc,
                Amount: amount,
                Quantity: qty
            });
        }
    });

    return list; // ✅ NOT string
}

// ➤ SAVE
$("#quotation_btnSaveQuotation").click(function () {

    let payload = {

        // ===== Basic =====
        Id: parseInt($("#quotationId").val()) || 0,
        // ===== Booking Info =====
        EnquiryDealId: parseInt($("#quotation_enquiryDealId").val()) || 0,

        BanquetId: parseInt($("#quotation_banquetId").val()) || 0,
        SlotId: parseInt($("#quotation_slotId").val()) || 0,

        FromDate: $("#quotation_fromDate").val() || "",
        TillDate: $("#quotation_tillDate").val() || "",
        StartTime: $("#quotation_startTime").val() || null,
        EndTime: $("#quotation_endTime").val() || null,

        // ===== Client Section =====
        ClientId: parseInt($("#quotation_clientId").val()),
        ClientName: $("#quotation_clientName").val()?.trim() || "",

        Address: $("#quotation_clientAddress").val() || null,
        Email: $("#quotation_clientEmail").val() || null,
        ContactNo: $("#quotation_clientContact").val() || null,
        GSTIN: $("#quotation_clientGstIn").val() || null,
        StateId: parseInt($("#quotation_clientStateId").val()) || 0,

        // ===== Booking Details =====
        EventSubTypeId: $("#quotation_subEventTypeId").val() || 0,
        BanquetRent: parseFloat($("#quotation_banquetRent").val()) || 0,
        Pax: parseInt($("#quotation_pax").val()) || 0,
        ReferenceTypeId: $("#quotation_referenceTypeId").val() || 0,

        // ===== Charges =====
        DecorationChargesAmount: parseFloat($("#quotation_decorationAmount").val()) || 0,
        SoundSystemChargesAmount: parseFloat($("#quotation_soundAmount").val()) || 0,
        ProjectorChargesAmount: parseFloat($("#quotation_projectorAmount").val()) || 0,

        // ===== Print Options =====
        IsPrintPriceExclusive: $("#quotation_isPrintExclusive").is(":checked"),
        IsPrintFoodItems: $("#quotation_isPrintFoodItems").is(":checked"),

        // ===== Totals =====
        SubTotal: parseFloat($("#quotation_subTotal").val()) || 0,
        TaxAmount: parseFloat($("#quotation_taxAmount").val()) || 0,
        GrandTotal: parseFloat($("#quotation_grandTotal").val()) || 0,

        // ===== Notes (if added later) =====
        Notes: null, // Add #notes field if needed

        // ===== JSON Child Data =====
        PackagesJson: getPackagesJson(),
        OtherServicesJson: getOtherServicesJson(),

        // ===== Audit =====
        ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
    };

    $("#quotation_btnSaveQuotation").prop("disabled", true);
    console.log("Quotation Data save:", payload);

    $.ajax({
        url: "/api/Quotation/SaveQuotation",
        type: "POST",
        data: JSON.stringify(payload),
        contentType: "application/json",
        success: function (res) {
            if (res.status === "Success") {
                handleApiSuccess();
                $("#quotationModal").modal("hide");
                //loadEnquiryDeals();
            } else {
                Swal.fire("Error", res.message, "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Save Quotation Failed");
        },
        complete: function () {
            $("#quotation_btnSaveQuotation").prop("disabled", false);
        }
    });
});

//QUOTATION - END------------------------------------------------------------------------------------------

//SLOT MASTER
loadSlotMaster();
function loadSlotMaster() {
    var payload = { Id: 0, IsActive: true };
    $.ajax({
        url: "/api/SlotMaster/GetSlotList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("response supplier:", response);

            const $slotDropdowns = $("#quotation_slotId");
            $slotDropdowns.html(`<option value="0">--Select--</option>`);

            shiftMap = {};
            shiftTimes = {};
            shiftNames = [];   // reset
            response.data.forEach(s => {
                // SLOT ABBREVIATIONS
                const code = s.Code;

                shiftNames.push(s.Code);
                // Dynamic full name mapping
                shiftMap[code] = s.Name;
                // Dynamic time mapping
                shiftTimes[code] = {
                    start: s.StartTime2 || s.StartTime,
                    end: s.EndTime2 || s.EndTime
                };
                $slotDropdowns.append(`<option value="${s.Id}" data-start="${s.StartTime}" data-end="${s.EndTime}">${s.Name} (${s.Code})</option>`);
            });
            //console.log("Shift Map:", shiftMap);
            //console.log("Shift Times:", shiftTimes);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Slot Fetch Error");
        }
    });
}

//<!--MAIN-->
let userId = 0;
// Sample data (replace later with API fetch)
let processes = [];

//City - dropdown -- START ------------------------
let cityList = [];
function loadCityList() {
    var payload = {
        Id: 0,
        StateId: 0
    };

    $.ajax({
        url: "/api/City/GetCityList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            cityList = res.data || [];
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Load City Error");
        }
    });
}
//City - dropdown -- END ------------------------

//FOLLOWUP - START ------------------------------------------------------------------------------------------
function loadFollowUpTypes() {
    $.ajax({
        url: "/api/EnquiryFollowup/GetEnquiryFollowUp_Types",
        type: "GET",
        success: function (res) {
            //console.log("res type:", res);
            if (res.status === "Success") {
                let ddl = $("#ddlFollowUpType");
                ddl.empty();

                ddl.append('<option value="0">-- Select Type --</option>');

                $.each(res.data, function (i, item) {
                    ddl.append(`<option value="${item.Id}">${item.Name}</option>`);
                });
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch follow-up types");
        }
    });
}
function loadFollowUpStatus() {
    $.ajax({
        url: '/api/StatusMaster/GetStatusMaster',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0, StatusType: "Followup" }),
        success: function (res) {
            //console.log("res status:", res);
            if (res.status === "Success") {
                let ddl = $("#ddlStatus");
                ddl.empty();

                ddl.append('<option value="0">-- Select Status --</option>');

                $.each(res.data, function (i, item) {
                    ddl.append(`<option value="${item.Id}">${item.StatusName}</option>`);
                });
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch follow-up status");
        }
    });
}
function loadMoodList() {
    $.ajax({
        url: "/api/EnquiryFollowup/GetEnquiryFollowUp_MoodList",
        type: "GET",
        success: function (res) {
            //console.log("res mood:", res);
            if (res.status === "Success") {
                let ddl = $("#ddlMood");
                ddl.empty();

                ddl.append('<option value="0">-- Select Mood --</option>');

                $.each(res.data, function (i, item) {
                    ddl.append(`<option value="${item.Id}">${item.Name}</option>`);
                });
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch follow-up mood list");
        }
    });
}
function loadProbabilityList() {
    $.ajax({
        url: "/api/EnquiryFollowup/GetEnquiryFollowUp_ProbabilityList",
        type: "GET",
        success: function (res) {
            //console.log("res mood:", res);
            if (res.status === "Success") {
                let ddl = $("#ddlProbability");
                ddl.empty();

                ddl.append('<option value="0">-- Select Probability --</option>');

                $.each(res.data, function (i, item) {
                    ddl.append(`<option value="${item.Id}">${item.Name}</option>`);
                });
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch follow-up Probability list");
        }
    });
}
function loadLeadLostReasonCategoryList() {
    $.ajax({
        url: "/api/EnquiryFollowup/GetEnquiryFollowUp_LeadLostReasonCategoryList",
        type: "GET",
        success: function (res) {
            //console.log("res mood:", res);
            if (res.status === "Success") {
                let ddl = $("#ddlLeadLostReasonCategory, #ddlLeadLostReasonCategory_enquiry");
                ddl.empty();

                ddl.append('<option value="0">-- Select Reason Category --</option>');

                $.each(res.data, function (i, item) {
                    ddl.append(`<option value="${item.Id}">${item.Name}</option>`);
                });
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch reason category list");
        }
    });
}
function loadLeadLostReasonList(categoryId=0) {

    $.ajax({
        url: "/api/EnquiryFollowup/GetEnquiryFollowUp_LeadLostReasonList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            LeadLostCategoryId: categoryId
        }),
        success: function (res) {
            console.log("res LeadLost Reason List:", res);
            if (res.status === "Success") {
                let ddl = $("#ddlLeadLostReason, #ddlLeadLostReason_enquiry");
                ddl.empty();

                ddl.append('<option value="0">-- Select Reason --</option>');

                $.each(res.data, function (i, item) {
                    ddl.append(`<option value="${item.Id}" data-categoryid="${item.LeadLostCategoryId}">${item.Name}</option>`);
                });
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch lead lost reason list");
        }
    });
}

function loadFollowUpHistoryById(enquiryId) {
    const requestObj = {
        FollowUpId: 0,
        EnquiryId: enquiryId
    };
    console.log("GetEnquiryFollowUp payload:", requestObj);
    $.ajax({
        url: "/api/EnquiryFollowUp/GetEnquiryFollowUp",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestObj),
        success: function (response) {
            console.log("GetEnquiryFollowUp response:", response);
            if (response.status.toLowerCase() === "success") {

                const enquiry = response.data.Enquiry;
                const followups = response.data.FollowUps;

                // -----------------------------
                // Bind enquiry Detail Section
                // -----------------------------
                $("#inqName").text(enquiry.Enquiry_Name || "-");
                $("#inqContact").text(enquiry.Enquiry_Contact || "-");
                let slot = enquiry.Enquiry_Slot || "-";
                $("#inqSlot").text(`${slot}`);

                $("#inqPax").text(enquiry.Enquiry_Pax || "-");

                // event
                let eventName = enquiry.Enquiry_EventName || "-";
                $("#inqEvent").text(`${eventName}`);

                // event date
                let eventDate = (enquiry.Enquiry_EventDate_Format1 || enquiry.Enquiry_EventDate) || "-";
                let start = enquiry.Enquiry_EventStartTime || "-";
                let end = enquiry.Enquiry_EventEndTime || "-";
                $("#inqEventDate").text(`${eventDate} (${start} - ${end})`);

                // halls
                $("#inqHalls").text(enquiry.Enquiry_Halls || "-");

                // email, address, note
                $("#inqEmail").text(enquiry.Enquiry_Email || "-");
                $("#inqAddress").text(enquiry.Enquiry_Address || "-");
                $("#inqNote").text(enquiry.Enquiry_Remarks || "-");

                // Bind Follow-Up History Table
                renderFollowUpHistory(followups);
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch follow-up history");
        }
    });
}

function renderFollowUpHistory(list) {
    let html = "";

    if (!list || list.length === 0) {
        $("#followupHistoryBody").html(`
                <tr>
                    <td colspan="10" class="text-center text-muted py-2">No follow-ups found</td>
                </tr>
            `);
        return;
    }

    list.forEach((item, index) => {
        let notes = item.DiscussionNotes || "-";
        let shortNotes = notes.length > 50 ? notes.substring(0, 50) + "..." : notes;
        html += `
            <tr>
                <td>${index + 1}</td>
                <td>${item.FollowUpDateTime2 || '-'}</td>
                <td>${item.FollowUpTypeName}</td>
                <td>${item.StatusName}</td>
                <td>${item.MoodName}</td>
                <td>${item.NextFollowUpDateTime2 || '-'}</td>
                <td data-bs-toggle="tooltip" data-bs-placement="top" title="${notes.replace(/" /g, '&quot;')}">
                    ${shortNotes}
                </td>
                <td>${item.AssignedToName}</td>
                <td>${item.ProbabilityName}</td>
                <td>${item.FollowUpLeadLostReasonName}</td>
                <td>
                    <button class="btn btn-action-xs btn-outline-warning edit-followup" data-id="${item.FollowUpId}">
                        <i class="fa fa-pencil"></i>
                    </button>
                    <button class="btn btn-action-xs btn-outline-danger delete-followup" data-id="${item.FollowUpId}">
                        <i class="fa fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    });

    $("#followupHistoryBody").html(html);

    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

}

function loadTimeDropdown() {
    let ddl = $("#nextFollowUpTime_modalFollowup");
    ddl.empty();

    ddl.append(`<option value = "">Time</option> `);
    for (let hour = 0; hour < 24; hour++) {
        for (let min = 0; min < 60; min += 30) {

            let hour12 = hour % 12;
            if (hour12 === 0) hour12 = 12;

            let ampm = hour < 12 ? "AM" : "PM";
            let hh = String(hour12).padStart(2, "0");
            let mm = String(min).padStart(2, "0");

            let display = `${hh}:${mm} ${ampm}`;
            let value = `${String(hour).padStart(2, "0")}:${mm}`; // 24-hour format
            //console.log("time: display:", display, ", value:", value);
            ddl.append(`<option value = "${value}"> ${display}</option> `);
        }
    }
}

function resetFollowUpForm() {
    $("#followup_form, #followup_title").removeClass("d-none");

    // Select first option dynamically (if exists)
    const followUpType = $("#ddlFollowUpType option:first").val();
    const status = $("#ddlStatus option:first").val();
    const mood = $("#ddlMood option:first").val();
    const probability = $("#ddlProbability option:first").val();

    $("#ddlFollowUpType").val(followUpType || "");
    $("#ddlStatus").val(status || "");
    $("#ddlMood").val(mood || "");
    $("#ddlProbability").val(probability || "");

    $("#txtDiscussionNotes").val("");
    // Set today's date
    //let today = new Date().toISOString().split("T")[0];
    $("#nextFollowUpDate_modalFollowup").val("");

    // Set first time option
    $("#nextFollowUpTime_modalFollowup").val("");
    //$("#nextFollowUpTime_modalFollowup").prop("selectedIndex", 0).trigger("change");

    //updateNextFollowUpValue();

    $("#hfIsLeadLost").val(0);
    $(".leadLostReasonSection").hide();

    loadLeadLostReasonCategoryList();
    loadLeadLostReasonList();
    $("#ddlLeadLostReasonCategory").val("0");
    $("#ddlLeadLostReason").val("0");

}

function updateNextFollowUpValue() {
    let date = $("#nextFollowUpDate_modalFollowup").val();
    let time = $("#nextFollowUpTime_modalFollowup").val(); // already 24-hour format

    if (!date || !time) {
        $("#txtNextFollowUpDateTime").val("");
        return;
    }
    $("#txtNextFollowUpDateTime").val(`${date}T${time} `);
}
// Trigger formatting automatically
$("#nextFollowUpDate_modalFollowup, #nextFollowUpTime_modalFollowup").on("change keyup", updateNextFollowUpValue);

$(document).on("click", ".edit-followup", function () {
    let followUpId = $(this).data("id");
    let enquiryId = $("#hfEnquiryDealId").val();

    const requestObj = {
        FollowUpId: Number(followUpId),
        EnquiryId: Number(enquiryId)
    };
    console.log("flwup edit requestObj", requestObj);
    $.ajax({
        url: "/api/EnquiryFollowUp/GetEnquiryFollowUp",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestObj),
        success: function (res) {
            console.log("edit res:", res);
            if (res.status === "Success" && res.data && res.data.FollowUps.length > 0) {
                let f = res.data.FollowUps;
                let item = f[0];

                resetFollowUpForm();
                // Fill modal fields
                $("#ddlFollowUpType").val(item.FollowUpTypeId);
                $("#ddlStatus").val(item.StatusId);
                $("#ddlMood").val(item.MoodId);
                $("#ddlProbability").val(item.ProbabilityId);
                $("#txtDiscussionNotes").val(item.DiscussionNotes);

                if (item.NextFollowUpDateTime) {
                    $("#nextFollowUpDate_modalFollowup").val(item.NextFollowUpDate);
                    $("#nextFollowUpTime_modalFollowup").val(item.NextFollowUpTime); // HH:mm
                }
                $("#txtNextFollowUpDateTime").val(item.NextFollowUpDate + 'T' + item.NextFollowUpTime);

                if (item.FollowUpLeadLostReasonCategoryId > 0) {
                    $(".leadLostReasonSection").show();
                    $("#hfIsLeadLost").val(1);
                }
                $("#ddlLeadLostReasonCategory").val(item.FollowUpLeadLostReasonCategoryId);
                $("#ddlLeadLostReason").val(item.FollowUpLeadLostReasonId);
                // Store ID for update
                $("#hfFollowUpId").val(followUpId);
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to load follow-up for editing");
        }
    });
});

$(document).on("click", ".delete-followup", function () {
    let followUpId = $(this).data("id");
    let enquiryId = $("#hfEnquiryDealId").val();
    let actionBy = sessionStorage.getItem("userId") ?? 0;

    Swal.fire({
        title: "Are you sure?",
        text: "This follow-up will be permanently deleted.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6",
        confirmButtonText: "Yes, delete it"
    }).then((result) => {
        if (result.isConfirmed) {

            const deleteObj = {
                FollowUpId: followUpId,
                ActionBy: actionBy
            };

            $.ajax({
                url: "/api/EnquiryFollowUp/DeleteEnquiryFollowUp",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(deleteObj),
                success: function (res) {
                    if (res.status === "Success") {
                        handleApiSuccess('Deleted!');

                        loadFollowUpHistoryById(enquiryId); // Reload table
                    }
                },
                error: function (xhr, error, status) {
                    handleApiError(xhr, error, status, "Failed to delete follow-up");
                }
            });
        }
    });
});

//FOLLOWUP - END ------------------------------------------------------------------------------------------

// Load Enquiry Deals (Drag & Drop view)
function loadEnquiryDeals(processId = 0) {
    const duration = $("#ddlDuration").val();
    const fromAmt = $("#fromAmount_filter").val();
    const toAmt = $("#toAmount_filter").val();

    const inputData = {
        ProcessId: Number(processId),
        Duration: duration,
        FromAmount: fromAmt ? Number(fromAmt) : null,
        ToAmount: toAmt ? Number(toAmt) : null,
        FromDate: duration === "customrange" ? $("#fromDate_filter").val() : null,
        ToDate: duration === "customrange" ? $("#toDate_filter").val() : null
    };
    console.log("EnquiryDeal inpuData:", inputData);

    $.ajax({
        url: "/api/EnquiryDeal/GetEnquiryDealList_forDragNDrop",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(inputData),
        success: function (response) {
            //console.log("EnquiryDeal response:", response);
            if (response.status === "Success") {
                processes = response.data; //GetProcessList
                renderBoard();
            } else {
                console.warn("No data received or API returned error:", response.message);
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to fetch enquiry deals");
        }
    });
}

//SORTING DATA
let currentSortField = "EventDate";
let currentSortDirection = "asc";
function sortDeals() {
    processes.forEach(process => {
        process.EnquiryDealsJson.sort((a, b) => {
            let valueA;
            let valueB;

            switch (currentSortField) {
                case "EventDate":
                    valueA = new Date(a.EventDate);
                    valueB = new Date(b.EventDate);
                    break;

                case "Amount":
                    valueA = Number(a.Amount || 0);
                    valueB = Number(b.Amount || 0);
                    break;

                case "Pax":
                    valueA = Number(a.Pax || 0);
                    valueB = Number(b.Pax || 0);
                    break;

                case "EnquiryID":
                    valueA = Number(a.EnquiryDealId || 0);
                    valueB = Number(b.EnquiryDealId || 0);
                    break;
            }

            if (valueA < valueB)
                return currentSortDirection === "asc" ? -1 : 1;

            if (valueA > valueB)
                return currentSortDirection === "asc" ? 1 : -1;

            return 0;
        });
    });
    renderBoard();
    refreshSortUI();
}

function refreshSortUI() {

    // Reset all links
    $('#sortEventDate, #sortAmount, #sortPax, #sortEnquiryID')
        .removeClass('text-warning fw-bold')
        .addClass('text-dark');

    // Reset all icons
    $('#sortEventDate_icon, #sortAmount_icon, #sortPax_icon, #sortEnquiryID_icon')
        .removeClass('fa-sort-up fa-sort-down text-warning')
        .addClass('fa-sort');

    let $link;
    let $icon;

    switch (currentSortField) {

        case 'EventDate':
            $link = $('#sortEventDate');
            $icon = $('#sortEventDate_icon');
            break;

        case 'Amount':
            $link = $('#sortAmount');
            $icon = $('#sortAmount_icon');
            break;

        case 'Pax':
            $link = $('#sortPax');
            $icon = $('#sortPax_icon');
            break;

        case 'EnquiryID':
            $link = $('#sortEnquiryID');
            $icon = $('#sortEnquiryID_icon');
            break;
    }

    $link.removeClass('text-dark')
        .addClass('text-warning fw-bold');

    $icon.removeClass('fa-sort');

    if (currentSortDirection === 'asc') {
        $icon.addClass('fa-sort-up text-warning');
    }
    else {
        $icon.addClass('fa-sort-down text-warning');
    }
}


$('#sortEventDate').click(function () {
    if (currentSortField === "EventDate")
        currentSortDirection = currentSortDirection === "asc" ? "desc" : "asc";
    else {
        currentSortField = "EventDate";
        currentSortDirection = "asc";
    }
    sortDeals();
});

$('#sortAmount').click(function () {
    if (currentSortField === "Amount")
        currentSortDirection = currentSortDirection === "asc" ? "desc" : "asc";
    else {
        currentSortField = "Amount";
        currentSortDirection = "desc";
    }
    sortDeals();
});

$('#sortPax').click(function () {
    if (currentSortField === "Pax")
        currentSortDirection = currentSortDirection === "asc" ? "desc" : "asc";
    else {
        currentSortField = "Pax";
        currentSortDirection = "desc";
    }
    sortDeals();
});

$('#sortEnquiryID').click(function () {
    if (currentSortField === "EnquiryID")
        currentSortDirection = currentSortDirection === "asc" ? "desc" : "asc";
    else {
        currentSortField = "EnquiryID";
        currentSortDirection = "desc";
    }
    sortDeals();
});



// Render board dynamically
function renderBoard() {
    const board = document.getElementById("processBoard");
    const processSelect = document.getElementById("dealProcess");
    board.innerHTML = "";
    processSelect.innerHTML = "";

    processes.forEach(process => {
        let dealsHtml = "";

        if (process.EnquiryDealsJson.length > 0) {
            process.EnquiryDealsJson.forEach(deal => {
                console.log("ProcessName:", process.ProcessName, ", deal render:", deal);
                const hideFollowUp = (deal.Is_LeadBooked); // || deal.Is_LeadLost);
                dealsHtml += `
                    <div class="card shadow-sm mb-2 deal-card cursor-pointer"
                        draggable = "true"
                        data-dealid="${deal.EnquiryDealId}"
                        data-bookingid="${deal.BookingId}"
                        data-deal='${JSON.stringify(deal)}'
                        data-processid="${process.ProcessId}"   
                        data-isleadlost="${deal.Is_LeadLost}" 
                        data-isleadbooked="${deal.Is_LeadBooked}">
                            <div class="card shadow-sm border-0 deal-open">

                                <!-- CARD HEADER -->
                                <div class="card-header bg-rating d-flex align-items-center justify-content-between p-1 py-2">
                                    <h6 class="mb-0 text-dark text-truncate flex-grow-1 me-0">
                                        ${deal.EnquiryDealTitle}
                                    </h6>

                                    <span class="fw-bold text-white small flex-shrink-0"
                                          title="Enquiry Date">
                                        <i class="fa fa-question-circle me-1"></i>
                                        ${deal.EnquiryDate_Format1}
                                    </span>
                                </div>

                                <!-- CARD BODY -->
                                <div class="card-body p-2">
                                    <!-- Row 1 : Deal Name -->
                                    <div class="d-flex justify-content-between align-items-center mb-1 small">
                                        <span class="fw-bold text-muted text-truncate me-2">
                                            ${deal.EnquiryDealName}
                                        </span>

                                        <span class="fw-bold text-warning flex-shrink-0 fs-9">
                                            <i class="fa fa-users me-1"></i>
                                            ${deal.Pax}
                                        </span>
                                    </div>

                                    <!-- Row 2 : Amount + Event Date -->
                                    <div class="d-flex justify-content-between align-items-center small">
                                        <span class="text-muted flex-shrink-0" title="Event Date">
                                            <i class="fa fa-calendar-check me-1"></i>
                                            ${deal.EventDate_Format1}
                                        </span>

                                        <span class="fw-bold text-warning flex-shrink-0 fs-9">
                                            ₹${formatINR(deal.Amount)}
                                        </span>
                                    </div>

                                    <!-- Row 3 : PAX + Time -->
                                    <div class="d-flex justify-content-between align-items-center small">
                                        <span class="text-muted">
                                            <i class="fa fa-clock"></i> ${deal.EventStartTime}-${deal.EventEndTime}
                                        </span>
                                    </div>
                                </div>
                            </div>

                            <!--Follow-up Row-->
                            <div class="card-footer py-1 px-2 d-flex align-items-center text-primary fw-semibold">
                                <div class="d-flex align-items-center flex-nowrap w-100 justify-content-between small-text">

                                    <!-- Followups -->
                                    <div class="action-btn open-followup d-flex align-items-center" data-dealid="${deal.EnquiryDealId}" 
                                        data-isleadlost="${deal.Is_LeadLost}" data-isleadbooked="${deal.Is_LeadBooked}">
                                        <span class="badge bg-dark me-1 px-1 py-0 badge-sm">${deal.FollowUpCount}</span>
                                        <span class="text-truncate">Followups</span>
                                    </div>

                                    <span class="mx-1 text-muted">|</span>

                                    <!-- Quotations -->
                                    <div class="action-btn open-quotation d-flex align-items-center" data-dealid="${deal.EnquiryDealId}"
                                        data-isleadlost="${deal.Is_LeadLost}" data-isleadbooked="${deal.Is_LeadBooked}">
                                        <span class="badge bg-dark me-1 px-1 py-0 badge-sm">${deal.QuotationCount}</span>
                                        <span class="text-truncate">Quotations</span>
                                    </div>

                                </div>
                            </div>
                        </div>
                    `;
            });
        }
        else {
            dealsHtml = `<p class="text-muted fst-italic"> This stage is empty</p> `;
        }

        board.innerHTML += `
                <div class="card process-column"
                    data-processid="${process.ProcessId}"
                    data-isconfirmed="${process.IsConfirmed}"
                    data-islost="${process.IsLost}">
                    <div class="card-header bg-warning p-3">
                        <h6 class="mb-0 text-white">${process.ProcessName}</h6>
                        <small class="text-white">₹${formatINR(process.TotalAmount)} • ${process.TotalEnquiryDeals} Deal(s)</small>
                    </div>
                    <div class="card-body deal-dropzone p-2" style="min-height: 500px;">
                        ${dealsHtml}
                    </div>
                </div>
    `;

        processSelect.innerHTML += `<option value="${process.ProcessId}" data-isconfirmed="${process.IsConfirmed}" data-islost="${process.IsLost}"> ${process.ProcessName}</option> `;
    });

    enableDragDrop();

    // 🔥 Add click event listener for all deal cards
    $(document).off("click", ".deal-open").on("click", ".deal-open", function () {
        const dealId = $(this).closest(".deal-card").data("dealid");
        const bookingId = $(this).closest(".deal-card").data("bookingid");
        const isleadlost = $(this).closest(".deal-card").data("isleadlost");
        const isleadbooked = $(this).closest(".deal-card").data("isleadbooked");
        console.log("Clicked dealId:", dealId);

        // later you can call your API here
        // 🔥 Reset before fetching data
        resetDealForm();
        
        $.ajax({
            url: `/api/EnquiryDeal/GetEnquiryDealById/${dealId} `,
            type: "GET",
            success: function (response) {
                if (response.status === "Success" && response.data) {
                    const data = response.data;

                    // 🔒 Validation: Prevent edit if already booked
                    if (bookingId > 0 || isleadlost || isleadbooked) {
                        // 🔥 Change modal title for edit mode
                        $("#dealModalLabel").text("Lead Details (Enquiry)");
                        $("#deleteDealBtn").addClass("d-none").data("deal-id", "0"); // hide delete btn
                        $("#saveDealBtn").addClass("d-none"); // hide delete btn

                        // 🔒 Make all fields readonly
                        $("#dealForm")
                            .find("input, textarea, select, button")
                            .not("#deleteDealBtn, #saveDealBtn")
                            .prop("disabled", true);
                    }
                    else {
                        // 🔥 Change modal title for edit mode
                        $("#dealModalLabel").text("Edit Deal (Enquiry)");
                        // Show delete button
                        $("#deleteDealBtn").removeClass("d-none").data("deal-id", dealId);
                        // 🔒 Make all fields non-readonly
                        $("#dealForm") 
                            .find("input, textarea, select, button")
                            .not("#deleteDealBtn, #saveDealBtn")
                            .prop("disabled", false);
                        $("#saveDealBtn").removeClass("d-none"); // hide delete btn

                        //initLeadLostSection();
                    }

                    // Populate form fields
                    $("#dealId").val(data.Id);
                    $("#dealProcess").val(data.ProcessId).prop("disabled", true);;

                    if (!isleadlost) {
                        $("#dealProcess").trigger("change");
                    }

                    $("#dealName").val(data.Name);
                    $("#dealContact").val(data.Contact);
                    $("#dealAddress").val(data.Address || "");
                    $("#dealEmail").val(data.Email || "");

                    // First, set the event type
                    $("#eventTypeDropdown").val(data.EventTypeId).trigger("change");

                    // Wait until subtypes are populated (assuming loadSubTypes is async)
                    setTimeout(() => {
                        $("#eventSubTypeDropdown").val(data.EventSubTypeId);
                    }, 300); // small delay to ensure subtypes are loaded

                    $("#banquetHallDropdown").val(data.BanquetHallId);
                    $("#eventDate").val(data.EventDate_Format2);

                    $("#eventStart").val(data.EventStartTime);
                    $("#eventEnd").val(data.EventEndTime);
                    $("#pax").val(data.Pax);

                    //console.log("edit data:", data);
                    //$("#packageDropdown").val(data.PackageId).trigger("change");
                    loadFoodPackageList(data.PackageId);
                    $("#followUpDate").val(data.FollowUpDate_Format2);

                    $("#estimatedAmount").val(data.EstimatedAmount).prop("readonly", true);
                    $("#leadSourceDropdown").val(data.LeadSourceId);
                    $("#remark").val(data.Remarks || "");

                    // finally, open modal
                    // const modal = new bootstrap.Modal(document.getElementById("dealModal"));
                    // modal.show();
                    $("#dealModal").modal("show"); // reopen only if it was opened from deal
                } else {
                    Swal.fire({
                        icon: "info",
                        title: "Not Found",
                        text: response.message || "No data available for this deal."
                    });
                }
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, error, status, "EnquiryId Fetch Fail");
            }
        });

    });

    $(document).off("click", ".open-followup").on("click", ".open-followup", function (e) {
        e.preventDefault();
        e.stopPropagation();

        const enquiryDealId = $(this).data("dealid");
        const isleadlost = $(this).data("isleadlost");
        const isleadbooked = $(this).data("isleadbooked");

        openFollowUp(enquiryDealId, null, isleadlost, isleadbooked);
    });

    function validateFollowUp() {

        const followUpDateTime = new Date(); // current datetime
        const nextFollowUpInput = $("#txtNextFollowUpDateTime").val();

        let isLeadLost = $("#hfIsLeadLost").val() === "1";
        if (isLeadLost) {
            console.log("validate isLeadLost");

            let categoryId = $("#ddlLeadLostReasonCategory").val();
            let reasonId = $("#ddlLeadLostReason").val();

            if (!categoryId || categoryId == "0") {
                Swal.fire(
                    "Lead Lost Reason Required",
                    "Please select Lead Lost Reason Category.",
                    "warning"
                );
                $("#ddlLeadLostReasonCategory").focus();
                return false;
            }

            if (!reasonId || reasonId == "0") {
                Swal.fire(
                    "Lead Lost Reason Required",
                    "Please select Lead Lost Reason.",
                    "warning"
                );
                $("#ddlLeadLostReason").focus();
                return false;
            }
        }

        // If no next follow-up entered → valid
        if (!nextFollowUpInput) return true;

        const nextFollowUpDate = new Date(nextFollowUpInput);

        if (nextFollowUpDate <= followUpDateTime) {
            Swal.fire(
                "Invalid Next Follow-Up",
                "Next FollowUp Date-Time must be greater than the current FollowUp Date-Time.",
                "error"
            );
            return false;
        }

        return true;
    }

    $("#btnSaveFollowUp").off("click").on("click", function () {
        const enquiryId = $("#hfEnquiryDealId").val();

        if (!enquiryId || enquiryId == "0") {
            Swal.fire("Invalid", "Enquiry ID missing.", "warning");
            return;
        }

        if (!validateFollowUp()) {
            return; // stop if invalid
        }
        const fileUpload_base64 = document.getElementById("attachmentBase64_followUp").value.trim();

        const obj = {
            FollowUpId: Number($("#hfFollowUpId").val() || 0),  // always insert new record
            EnquiryId: Number(enquiryId),
            FollowUpTypeId: Number($("#ddlFollowUpType").val() || 0),
            FollowUpLeadLostReasonCategoryId: Number( $("#ddlLeadLostReasonCategory").val() || 0 ),
            FollowUpLeadLostReasonId: Number( $("#ddlLeadLostReason").val() || 0),
            FollowUpDateTime: formatDateTime(),
            NextFollowUpDateTime: $("#txtNextFollowUpDateTime").val() || null,
            StatusId: Number( $("#ddlStatus").val() || 0),
            MoodId: Number( $("#ddlMood").val() || 0),
            ProbabilityId: Number($("#ddlProbability").val() || 0),
            OutcomeId: null,             // you can add later
            DiscussionNotes: $("#txtDiscussionNotes").val(),
            InternalRemarks: null,
            CustomerRemarks: null,
            FileUpload: fileUpload_base64,
            AssignedTo: 0,               // dynamic later
            IsClosed: false,
            ActionBy: Number(sessionStorage.getItem("userId") ?? 0)
        };
        console.log("Saving Follow-Up:", obj);

        $.ajax({
            url: "/api/EnquiryFollowUp/SaveEnquiryFollowUp",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(obj),
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess('Saved!');
                    saveFollowUpAndMoveDeal();
                    resetFollowUpForm();
                    loadFollowUpHistoryById(enquiryId);
                    loadEnquiryDeals();

                    $("#followup_title").text("(Optional) Skip If not needed");
                    $("#followup_closebutton").removeClass("d-none");
                   
                    // Recreate modal with normal behavior
                    let modalInstance = bootstrap.Modal.getInstance(document.getElementById('followupModal'));
                    if (modalInstance) {
                        //modalInstance.dispose();
                        modalInstance.hide();
                    }

                    // ✅ Call your moveDeal function with parameters
                    //moveDeal(
                    //    pendingMoveData.dealId,
                    //    pendingMoveData.fromProcessId,
                    //    pendingMoveData.toProcessId,
                    //    nextFollowUp,
                    //    remarks,
                    //    attachmentBase64_enquiryMove
                    //);
                }
                else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, error, status, "Failed to save follow-up.");
            }
        });

    });
}

//custom tooltip - on-hover -- START --------------------
const tooltip = document.getElementById("dealTooltip");
const board = document.getElementById("processBoard");

board.addEventListener("mouseover", function (e) {
    const card = e.target.closest(".deal-card");
    if (!card) return;

    // 🚫 Do not show tooltip while dragging
    if (card.classList.contains("dragging")) return;

    // Prevent re-trigger when moving inside same card
    if (card.contains(e.relatedTarget)) return;

    const deal = JSON.parse(card.dataset.deal);
    console.log("deal mouseover:", deal);

    tooltip.innerHTML = `
        <div class="title d-flex justify-content-between align-items-center">

            <!-- LEFT SIDE -->
            <div>
                📌 ${deal.EnquiryDealTitle}
                ${deal.BookingId > 0 ? ` - BK #${deal.BookingId}` : ""}
            </div>

            <!-- RIGHT SIDE -->
            <div>
                <strong class="me-1">Status:</strong>
                ${deal.Is_LeadBooked
                    ? `<span class="badge bg-success">Booked</span>`
                    : deal.Is_LeadLost
                        ? `<span class="badge bg-danger">Lost</span>`
                        : `<span class="badge bg-warning text-dark">Open</span>`
                }
            </div>
        </div>

        <div class="row-item"><strong>Name:</strong> ${deal.EnquiryDealName || "-"}</div>
        <div class="row-item"><strong>Contact:</strong> ${deal.EnquiryDealContact || "-"}</div>
        <div class="row-item"><strong>Email:</strong> ${deal.EnquiryDealEmail || "-"}</div>
        <div class="row-item"><strong>Address:</strong> ${deal.EnquiryDealAddress || "-"}</div>

        <hr style="margin:6px 0; opacity:0.3;">

        <div class="row-item"><strong>Event Type:</strong> ${deal.EventTypeName || "-"}</div>
        <div class="row-item"><strong>Event SubType:</strong> ${deal.EventSubTypeName || "-"}</div>
        <div class="row-item"><strong>Hall:</strong> ${deal.HallName || "-"}</div>
        <div class="row-item"><strong>Lead Source:</strong> ${deal.LeadSourceName || "-"}</div>

        <hr style="margin:6px 0; opacity:0.3;">

        <div class="row-item"><strong>Event Date:</strong> ${deal.EventDate_Format1 || "-"}</div>
        <div class="row-item"><strong>Event Time:</strong> 
            ${deal.EventStartTime || "-"} - ${deal.EventEndTime || "-"}
        </div>

        <div class="row-item"><strong>PAX:</strong> ${deal.Pax || 0}</div>
        <div class="row-item"><strong>Package:</strong> ${deal.PackageName || 0}</div>
        <div class="row-item"><strong>Pakage Amount:</strong> ${deal.EstimatedPackageAmount || 0}</div>
        <div class="row-item"><strong>Estimated Amount:</strong> 
            ₹${deal.Amount ? deal.Amount.toLocaleString() : "0"}
        </div>

        <hr style="margin:6px 0; opacity:0.3;">

        <div class="row-item"><strong>Enquiry Date:</strong> ${deal.EnquiryDate_Format1 || "-"}</div>
        <div class="row-item d-none"><strong>FollowUps:</strong> ${deal.FollowUpCount || 0}</div>

        ${deal.Remarks ? `
            <hr style="margin:6px 0; opacity:0.3;">
            <div class="row-item"><strong>Remarks:</strong></div>
            <div style="font-size:12px; opacity:0.9;">
                ${deal.Remarks}
            </div>
        ` : ""}
    `;

    tooltip.style.display = "block";
    const rect = card.getBoundingClientRect();
    const screenWidth = window.innerWidth;
    const tooltipRect = tooltip.getBoundingClientRect();
    const gap = 12;

    let leftPosition;

    if (rect.left < screenWidth / 2) {
        leftPosition = rect.right + gap + window.scrollX;
    }    
    else {
        leftPosition = rect.left - tooltipRect.width - gap + window.scrollX;
    }

    tooltip.style.left = leftPosition + "px";
    tooltip.style.top = rect.top + window.scrollY + "px";
});

board.addEventListener("mouseout", function (e) {
    const card = e.target.closest(".deal-card");
    if (!card) return;

    // Prevent hide when moving inside same card
    if (card.contains(e.relatedTarget)) return;

    tooltip.style.display = "none";
});
//custom tooltip - on-hover -- END --------------------

// Drag & Drop setup
let pendingMoveData = null;
//const bookingModal = new bootstrap.Modal(document.getElementById("bookingModal"));
//bookingModal.show();
//bookingModal.hide();

function enableDragDrop() {
    const cards = document.querySelectorAll(".deal-card");
    const dropzones = document.querySelectorAll(".deal-dropzone");

    cards.forEach(card => {
        card.addEventListener("dragstart", e => {
            const bookingId = parseInt(card.dataset.bookingid) || 0;

            const column = card.closest(".process-column");
            const isConfirmed = column.dataset.isconfirmed === "true";
            //const isLost = column.dataset.islost === "true";

            // 🔒 Block drag if already booked
            if (bookingId > 0 && isConfirmed) {
                e.preventDefault(); // stop drag
                Swal.fire({
                    icon: "warning",
                    title: "Action Not Allowed",
                    text: "This enquiry has already been booked and cannot be moved."
                });
                return;
            }

            // 🛑 Hide tooltip while dragging
            const tooltip = document.getElementById("dealTooltip");
            tooltip.style.display = "none";
            // Optional: disable tooltip temporarily
            card.classList.add("dragging");

            // ✅ Normal drag
            e.dataTransfer.setData("dealId", card.dataset.dealid);
            e.dataTransfer.setData("fromProcessId", card.dataset.processid);
        });
    });

    dropzones.forEach(zone => {
        zone.addEventListener("dragover", e => e.preventDefault());

        zone.addEventListener("drop", e => {
            e.preventDefault();

            //Re-enable Tooltip After Drop
            const tooltip = document.getElementById("dealTooltip");
            tooltip.style.display = "none";
            document.querySelectorAll(".deal-card.dragging").forEach(c => c.classList.remove("dragging"));

            const dealId = parseInt(e.dataTransfer.getData("dealId"));
            const fromProcessId = parseInt(e.dataTransfer.getData("fromProcessId"));

            const column = zone.closest(".process-column");

            const toProcessId = parseInt(column.dataset.processid);
            const isConfirmed = column.dataset.isconfirmed === "true";
            const isLost = column.dataset.islost === "true";
            console.log("isConfirmed:", isConfirmed, ", isLost:", isLost, ", toProcessId:", toProcessId);
            //const toProcessId = parseInt(
            //    zone.closest(".process-column").dataset.processid
            //);

            if (fromProcessId !== toProcessId) {

                // 🧠 Store pending move ONLY
                pendingMoveData = {
                    EnquiryDealId: dealId,
                    ProcessId: toProcessId
                };

                // 🎯 If dropped in Confirmed column
                if (isConfirmed) {
                    console.log("Booked");
                    resetBooking();
                    //Fetch Lead-Deal Data
                    const draggedCard = document.querySelector(`.deal-card[data-dealid="${dealId}"]`);
                    if (!draggedCard) return;
                    var dealData = null;
                    try {
                        dealData = JSON.parse(draggedCard.dataset.deal);
                        console.log("=== CONFIRMED DEAL DATA ===");
                        // Optional: pretty print
                        //console.table(dealData);
                    }
                    catch (err) {
                        console.error("Error parsing deal data:", err);
                        return;
                    }

                    //OPEN BOOKING MODAL
                    const dateStr = dealData.EventDate;       // for display
                    const dateStr_Format = dealData.EventDate_Format1;
                    const dateStr_ISO = dealData.EventDate;
                    const shift = dealData.ShiftCode;
                    const shiftname = dealData.ShiftFullName; // fallback if not found

                    const date = dateStr;
                    const dateiso = dateStr_ISO;
                    const hallid = dealData.HallId;
                    const booking_subEventDropdown = dealData.EventSubTypeId;

                    //console.log("bookingId:", bookingId);
                    $("#BookingId").val(bookingId);
                    // 🟢 Update label content & attributes - using jQuery
                    $("#label_date_shift")
                        .text(`${dateStr_Format}, ${shiftname} Slot`)        // visible text
                        .attr("data-eventdate", dateiso)           // yyyy-mm-dd
                        .attr("data-shiftid", shift);              // FD, MO, etc.
                    $("#banquetDropdown").val(hallid);
                    $("#clientEnquiryDropdown").val(dealData.EnquiryDealId).trigger("change").prop("disabled", true);

                    $("#booking_pax").val(dealData.Pax);
                    //console.log("booking_pax:", dealData.Pax);

                    console.log("dealData:", dealData);
                    setShiftTime(dealData.ShiftCode);
                    //console.log("B - After setShiftTime_enq");

                    $("#PaymentDetails_DueDate").val('');

                    $("#fromDate").val(dateiso); //.prop("disabled", true);
                    $("#toDate").val(dateiso);

                    $("#booking_subEventDropdown").val(booking_subEventDropdown).trigger("change");

                    $("#title_bookingModal").text("Add Booking Details");
                    console.log("open booking modal");

                    moveOn();
                    $("#bookingModal").modal("show");

                    return;
                }

                // 🎯 If dropped in Lost column (optional handling)
                if (isLost) {
                    console.log("Lost");
                    $("#followup_title").text("(Compulsory, If Lead Lost)");
                    // Open follow-up modal
                    openFollowUp(dealId, "lost");
                    return;
                }

                // Open follow-up modal
                openFollowUp(dealId);
            }
        });
    });

}

function moveOn(){
    saveFollowUpAndMoveDeal();
    bookingId = 0;
    pendingMoveData = null;
}

$("#followupModal").on("hidden.bs.modal", function () {
    moveOn();
});
var bookingId = 0;
function saveFollowUpAndMoveDeal() {
    console.log("pendingMoveData:", pendingMoveData);
    if (!pendingMoveData) return;

    const payload = {
        EnquiryDealId: pendingMoveData.EnquiryDealId,
        ProcessId: pendingMoveData.ProcessId,
        FollowUpDate: $("#nextFollowUpDate_modalFollowup").val() || null,
        Remarks: $("#txtDiscussionNotes").val() || null,
        FileUpload: $("#attachmentBase64_followUp").val() || null,
        ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
    };

    console.log("SaveEnquiryDeal_forDragNDrop payload:", payload);
    $.ajax({
        url: "/api/EnquiryDeal/SaveEnquiryDeal_forDragNDrop",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                handleApiSuccess('Saved!');

                // Close modal
                $("#followupModal").modal("hide");

                //OPEN BOOKING MODAL IF BOOKED
                //if (pendingMoveData.ProcessId === processes.ProcessId && processes.ProcessName === "Event Booked") {
                //    //here7

                //}
                console.log("res data:", res.data);
                bookingId = res.data.BookingId;
                // Clear pending state
                pendingMoveData = null;

                // Reload board (or move card visually)
                //renderBoard();
                loadEnquiryDeals();
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Deal Save Failed!");
        }
    });
}

function openFollowUp(enquiryDealId, moveContext = null, isleadlost=false, isleadbooked=false) {
    resetFollowUpForm();
    //console.log("Opening Follow-Up. EnquiryDealId =", enquiryDealId);
    $("#hfEnquiryDealId").val(enquiryDealId);

    // Optional: store drag-drop move info if exists
    //if (moveContext) {
    //    window.pendingMoveData = moveContext;
    //} else {
    //    window.pendingMoveData = null;
    //}

    loadFollowUpHistoryById(enquiryDealId);

    console.log("isleadlost, isleadbooked:", isleadlost, isleadbooked);

    $("#followup_closebutton").removeClass("d-none");

    if (isleadlost || isleadbooked) {
        $("#followup_form, #followup_title").addClass("d-none");
    }
    else {
        $("#followup_form, #followup_title").removeClass("d-none");
    }

    if (moveContext == "lost") {
        $(".leadLostReasonSection").show();
        $("#hfIsLeadLost").val(1);

        $("#followup_closebutton").addClass("d-none");
        // Dispose existing instance
        let modalInstance = bootstrap.Modal.getInstance(document.getElementById('followupModal'));
        if (modalInstance) {
            modalInstance.dispose();
        }

        const followupModal = new bootstrap.Modal(document.getElementById('followupModal'), {
            keyboard: false,
            backdrop: 'static'
        });

        followupModal.show();
        return;
    }
    else {
        $(".leadLostReasonSection").hide();
        $("#hfIsLeadLost").val(0);

        // Dispose existing instance
        let modalInstance = bootstrap.Modal.getInstance(document.getElementById('followupModal'));
        if (modalInstance) {
            modalInstance.dispose();
        }

        const followupModal = new bootstrap.Modal(document.getElementById('followupModal'), {
            keyboard: true,
            backdrop: true
        });

        followupModal.show();
    }

    $("#followupModal").modal("show");
}

// Reset Deal Form
function resetDealForm() {
    $("#dealModalLabel").text("Add Deal (Enquiry)");
    $("#deleteDealBtn").addClass("d-none").data("deal-id", "0"); // hide delete btn
    $("#saveDealBtn").removeClass("d-none"); // show save btn

    // Reset all fields
    $("#dealId").val(0);
    $("#dealProcess").val(1).prop("disabled", false);;

    $("#dealName").val("");
    $("#dealContact").val("");
    $("#dealAddress").val("");
    $("#dealEmail").val("");
    $("#eventTypeDropdown").val("0");
    $("#eventSubTypeDropdown").val("0");
    $("#banquetHallDropdown").val("0");
    $("#eventDate").val("");
    $("#eventStart").val("");
    $("#eventEnd").val("");
    $("#pax").val("0");
    $("#leadSourceDropdown").val("0");
    $("#followUpDate").val("");
    $("#estimatedAmount").val("0").prop("readonly", true);
    $("#remark").val("");

    // Hide all error messages
    $("#dealProcess").siblings(".error-msg").hide();
    $("#dealName").siblings(".error-msg").hide();
    $("#dealContact").siblings(".error-msg").hide();
    $("#eventTypeDropdown").siblings(".error-msg").hide();
    $("#eventSubTypeDropdown").siblings(".error-msg").hide();
    $("#banquetHallDropdown").siblings(".error-msg").hide();
    $("#eventDate").siblings(".error-msg").hide();
    $("#pax").siblings(".error-msg").hide();
    $("#leadSourceDropdown").siblings(".error-msg").hide();
    $("#estimatedAmount").siblings(".error-msg").hide();

    $("#dealForm")
        .find("input, textarea, select, button")
        .not("#deleteDealBtn, #saveDealBtn")
        .prop("disabled", false);


    loadLeadLostReasonCategoryList();
    loadLeadLostReasonList();
    initLeadLostSection();
}
function initLeadLostSection() {
    $("#ddlLeadLostReasonCategory_enquiry").val("0");
    $("#ddlLeadLostReason_enquiry").val("0");

    const category = $("#ddlLeadLostReasonCategory_enquiry");
    const reason = $("#ddlLeadLostReason_enquiry");

    // Disable by default
    category.prop('disabled', true);
    reason.prop('disabled', true);

    // Hide message
    $("#leadLostMsg_enquiry").hide();

    // HIDE
    $('.leadLostReasonSection_enquiry').addClass('d-none');
}

// Validate Deal Form
function validateDealForm() {
    let isValid = true;

    // (0) Process / Stage
    const dealProcess = $("#dealProcess");
    const dealProcessErr = $("#dealProcessErr");

    const selectedOption = dealProcess.find(':selected');
    const isLost = selectedOption.data('islost') === true || selectedOption.data('islost') === "true";

    if (dealProcess.val() === "" || dealProcess.val() === "0") {
        dealProcessErr.removeClass("d-none");
        isValid = false;
    } else {
        dealProcessErr.addClass("d-none");
    }

    // (1) Name
    const dealName = $("#dealName");
    const dealNameErr = $("#dealNameErr");

    if (dealName.val().trim() === "") {
        dealNameErr.removeClass("d-none");
        isValid = false;
    } else {
        dealNameErr.addClass("d-none");
    }

    // (2) Contact
    const dealContact = $("#dealContact").val().trim();
    const dealContactErr = $("#dealContactErr");
    if (!dealContact || dealContact.length !== 10 || !/^\d+$/.test(dealContact)) {
        dealContactErr.removeClass("d-none");
        isValid = false;
    } else {
        dealContactErr.addClass("d-none");
    }


    // (5) Event Type
    const eventType = $("#eventTypeDropdown");
    const eventTypeErr = $("#eventTypeErr");

    if (eventType.val() === "" || eventType.val() === "0") {
        eventTypeErr.removeClass("d-none");
        isValid = false;
    } else {
        eventTypeErr.addClass("d-none");
    }


    // (6) Event Sub-Type
    const eventSubType = $("#eventSubTypeDropdown");
    const eventSubTypeErr = $("#eventSubTypeErr");

    if (eventSubType.val() === "" || eventSubType.val() === "0") {
        eventSubTypeErr.removeClass("d-none");
        isValid = false;
    } else {
        eventSubTypeErr.addClass("d-none");
    }


    // (7) Banquet Hall
    const banquetHall = $("#banquetHallDropdown");
    const banquetHallErr = $("#banquetHallErr");

    if (banquetHall.val() === "" || banquetHall.val() === "0") {
        banquetHallErr.removeClass("d-none");
        isValid = false;
    } else {
        banquetHallErr.addClass("d-none");
    }

    // (7) Event Date
    const eventDate = $("#eventDate");
    const eventDateErr = $("#eventDateErr");
    const val = eventDate.val().trim();
    console.log("e-date val:", val);
    if (val === "") {
        eventDateErr.removeClass("d-none");
        isValid = false;
    } else {
        eventDateErr.addClass("d-none");
    }

    // (8) Start Time
    const eventStart = $("#eventStart");
    const eventStartErr = eventStart.nextAll('.error-msg').first();

    if (eventStart.val() === "") {
        eventStartErr.removeClass("d-none");
        isValid = false;
    } else {
        eventStartErr.addClass("d-none");
    }


    // (9) End Time
    const eventEnd = $("#eventEnd");
    const eventEndErr = eventEnd.nextAll('.error-msg').first();

    if (eventEnd.val() === "") {
        eventEndErr.removeClass("d-none");
        isValid = false;
    } else {
        eventEndErr.addClass("d-none");
    }

    // (8) Compare Start & End Time
    const startTime = $("#eventStart").val();
    const endTime = $("#eventEnd").val();

    // Run comparison only if both fields are filled
    if (startTime && endTime) {
        // Parse times into comparable Date objects
        const start = parseTime(startTime);
        const end = parseTime(endTime);

        if (end <= start) {
            eventEndErr.text("End Time must be after Start Time");
            eventEndErr.removeClass("d-none");
            $("#eventEnd").addClass("is-invalid");
            isValid = false;
        } else {
            eventEndErr.addClass("d-none");
            $("#eventEnd").removeClass("is-invalid");
        }
    }

    // (10) Lead Source
    const leadSource = $("#leadSourceDropdown").val();
    const leadSourceErr = $("#leadSourceErr");
    //console.log("leadSource:", leadSource);
    if (leadSource === "" || leadSource === "0") {
        leadSourceErr.removeClass("d-none");
        isValid = false;
    } else {
        leadSourceErr.addClass("d-none");
    }

    // (9) Pax
    const pax = $("#pax");
    const paxErr = $("#paxErr");

    const paxVal = parseInt(pax.val());
    console.log("paxVal val:", paxVal);

    if (pax.val().trim() === "") {
        paxErr.text("Please enter Pax").removeClass("d-none");
        pax.addClass("is-invalid");
        isValid = false;

    } else if (isNaN(paxVal) || paxVal <= 0) {
        paxErr.text("PAX must be greater than 0").removeClass("d-none");
        pax.addClass("is-invalid");
        isValid = false;

    } else {
        paxErr.addClass("d-none");
        pax.removeClass("is-invalid");
    }


    // (11) Estimated Package Amount
    const packageAmount = $("#packageAmount");
    const packageAmountErr = $("#packageAmountErr");

    const pkgVal = parseInt(packageAmount.val());

    if (packageAmount.val().trim() === "") {
        packageAmountErr.text("Please enter Package Amount").removeClass("d-none");
        packageAmount.addClass("is-invalid");
        isValid = false;

    } else if (isNaN(pkgVal) || pkgVal <= 0) {
        packageAmountErr.text("Package Amount must be greater than 0").removeClass("d-none");
        packageAmount.addClass("is-invalid");
        isValid = false;

    } else {
        packageAmountErr.addClass("d-none");
        packageAmount.removeClass("is-invalid");
    }


    // (12) Estimated Amount
    const estimatedAmount = $("#estimatedAmount");
    const estimatedAmountErr = $("#estimatedAmountErr");

    const amtVal = parseInt(estimatedAmount.val());

    if (estimatedAmount.val().trim() === "") {
        estimatedAmountErr.text("Please enter Estimated Amount").removeClass("d-none");
        estimatedAmount.addClass("is-invalid");
        isValid = false;

    } else if (isNaN(amtVal) || amtVal <= 0) {
        estimatedAmountErr.text("Estimated Amount must be greater than 0").removeClass("d-none");
        estimatedAmount.addClass("is-invalid");
        isValid = false;

    } else {
        estimatedAmountErr.addClass("d-none");
        estimatedAmount.removeClass("is-invalid");
    }

    // (13) Lead Lost Reason Category (ONLY if isLost = true)
    if (isLost) {
        const category = $("#ddlLeadLostReasonCategory_enquiry");
        const categoryErr = category.nextAll('.error-msg').first();

        if (category.val() === "" || category.val() === "0") {
            categoryErr.removeClass("d-none");
            isValid = false;
        } else {
            categoryErr.addClass("d-none");
        }

        // Lead Lost Reason
        const reason = $("#ddlLeadLostReason_enquiry");
        const reasonErr = reason.nextAll('.error-msg').first();

        if (reason.val() === "" || reason.val() === "0") {
            reasonErr.removeClass("d-none");
            isValid = false;
        } else {
            reasonErr.addClass("d-none");
        }
    }
    else {

        // Hide errors when not required
        $("#ddlLeadLostReasonCategory_enquiry").nextAll('.error-msg').first().addClass("d-none");
        $("#ddlLeadLostReason_enquiry").nextAll('.error-msg').first().addClass("d-none");
    }

    return isValid;
}
// Utility: convert "hh:mm AM/PM" → Date object for comparison
function parseTime(timeStr) {
    // e.g. "07:30 PM"
    const [time, modifier] = timeStr.split(" ");
    let [hours, minutes] = time.split(":").map(Number);

    if (modifier === "PM" && hours < 12) hours += 12;
    if (modifier === "AM" && hours === 12) hours = 0;

    const date = new Date();
    date.setHours(hours, minutes, 0, 0);
    return date;
}

function calculateEstimatedAmount() {
    const pax = parseFloat(document.getElementById("pax").value) || 0;
    const packageAmount = parseFloat(document.getElementById("packageAmount").value) || 0;

    const total = pax * packageAmount;

    $("#estimatedAmount").val(total > 0 ? total.toFixed(2) : "0").prop("readonly", true);

    //$("#packageAmount, #estimatedAmount").removeClass("is-invalid");
    //$("#packageAmountErr, #estimatedAmountErr").addClass("d-none").removeClass("is-invalid");
}

// Initial load
document.addEventListener("DOMContentLoaded", function () {

    // Step 1: Get session value from Page 1
    const selectedDuration = sessionStorage.getItem("SelectedFilter_DashboardWidget");
    //console.log("selectedDuration:", selectedDuration);
    if (selectedDuration) {
        // Step 2: Set dropdown value
        $("#ddlDuration").val(selectedDuration);
        // Trigger change event to handle custom range visibility
        $("#ddlDuration").trigger("change");
        // Step 3: Remove session (one-time use)
        sessionStorage.removeItem("SelectedFilter_DashboardWidget");
    }
    loadEnquiryDeals();
    loadEnquiryList();
    loadStateList();

    AppSignalR.subscribeQuotation((data) => {
        //console.log("Enquiry Page Update", data);
        loadEnquiryDeals();
    });

    loadEventTypeList();
    loadEventSubTypeList();
    //loadBanquetHallList();
    loadLeadSourceList();

    loadFollowUpTypes();
    loadFollowUpStatus();
    loadMoodList();
    loadProbabilityList();
    loadLeadLostReasonCategoryList();
    loadLeadLostReasonList();
    loadTimeDropdown();

    fetchHallDetails();
    loadFoodPackageList();

    // Trigger on input
    // 🔹 Input events
    $("#pax").on("input", calculateEstimatedAmount);
    $("#packageAmount").on("input", calculateEstimatedAmount);

    // 🔹 Dropdown change
    $("#packageDropdown").on("change", function () {
        const $selected = $(this).find("option:selected");
        const ratePerPerson = parseFloat($selected.data("rateperperson")) || 0;

        //console.log("packageDropdown change ratePerPerson:", ratePerPerson);
        // Set value
        $("#packageAmount").val(ratePerPerson);
        // Recalculate
        calculateEstimatedAmount();
    });

    //$(document).on('input change', '#packageAmount, #estimatedAmount', function () {
    //    $("#packageAmountErr, #estimatedAmountErr").addClass("d-none").removeClass("is-invalid");
    //    $("#packageAmount, #estimatedAmount").removeClass("is-invalid");
    //});

    $("#ddlDuration").on("change", function () {
        const duration = $(this).val();

        if (duration === "customrange") {
            $("#customDateRange").removeClass("d-none");
        } else {
            $("#customDateRange").addClass("d-none");
            $("#fromDate_filter").val("");
            $("#toDate_filter").val("");
        }
        loadEnquiryDeals();
    });

    $("#ddlLeadLostReasonCategory").on("change", function () {
        const cid = $(this).val();
        loadLeadLostReasonList(cid);
    });
    $("#ddlLeadLostReason").on("change", function () {
        const selectedOption = $(this).find("option:selected");
        const categoryId = selectedOption.data("categoryid");

        if (categoryId && categoryId != 0) {
            $("#ddlLeadLostReasonCategory").val(categoryId);
        }
    });

    $("#ddlLeadLostReasonCategory_enquiry").on("change", function () {
        const cid = $(this).val();
        loadLeadLostReasonList(cid);
    });
    $("#ddlLeadLostReason_enquiry").on("change", function () {
        const selectedOption = $(this).find("option:selected");
        const categoryId = selectedOption.data("categoryid");

        if (categoryId && categoryId != 0) {
            $("#ddlLeadLostReasonCategory_enquiry").val(categoryId);
        }
    });

    $(document).on('change', '#dealProcess', function () {

        const selectedOption = $(this).find(':selected');
        const isLost = selectedOption.data('islost') === true || selectedOption.data('islost') === "true";

        const category = $("#ddlLeadLostReasonCategory_enquiry");
        const reason = $("#ddlLeadLostReason_enquiry");

        const categoryLabel = $("label[for='ddlLeadLostReasonCategory_enquiry']");
        const reasonLabel = $("label[for='ddlLeadLostReason_enquiry']");

        if (isLost) {
            // SHOW
            $('.leadLostReasonSection_enquiry').removeClass('d-none');

            // Enable
            category.prop('disabled', false);
            reason.prop('disabled', false);

            // Add *
            if (!categoryLabel.find('.req-star').length) {
                categoryLabel.append(' <span class="text-danger req-star">*</span>');
            }
            if (!reasonLabel.find('.req-star').length) {
                reasonLabel.append(' <span class="text-danger req-star">*</span>');
            }

            $("#leadLostMsg_enquiry").show();

        }
        else {
            // HIDE
            $('.leadLostReasonSection_enquiry').addClass('d-none');

            // Disable
            category.prop('disabled', true);
            reason.prop('disabled', true);

            // Reset
            category.val("0"); //.trigger('change');
            reason.val("0"); //.trigger('change');

            // Remove *
            categoryLabel.find('.req-star').remove();
            reasonLabel.find('.req-star').remove();

            $("#leadLostMsg_enquiry").hide();
        }
    });
    $(document).on('change', '#ddlLeadLostReasonCategory_enquiry, #ddlLeadLostReason_enquiry', function () {
        $(this).nextAll('.error-msg').first().addClass('d-none');
    });

    $("#btnApplyFilters").on("click", function () {
        loadEnquiryDeals();
        $("#filterDropdown").dropdown("hide");
    });

    $("#btnResetFilters").on("click", function () {
        $("#ddlDuration").val("thismonth");
        $("#fromAmount_filter").val("");
        $("#toAmount_filter").val("");
        $("#fromDate_filter").val("");
        $("#toDate_filter").val("");
        $("#customDateRange").addClass("d-none");

        loadEnquiryDeals();
    });

    //Modal Behaviour -- START ---------------------------------------------------
    //Deal form
    // 1️⃣ Handle + Add Deal button (New Deal)
    document.querySelector('[data-bs-target="#dealModal"]').addEventListener("click", function () {
        resetDealForm(); // always reset before showing modal
    });

    // 2️⃣ Handle modal open (just a safeguard)
    $('#dealModal').on('shown.bs.modal', function () {
        console.log("Modal opened");
        // You can keep additional logic here if needed
    });

    let openedFromDeal = false;

    // When Process Modal closes
    // $("#processModal").on("hidden.bs.modal", function () {
    //     if (openedFromDeal) {
    //         $("#dealModal").modal("show"); // reopen only if it was opened from deal
    //         openedFromDeal = false; // reset flag
    //     }
    // });

    // Open from Deal Modal (+ button)
    $(document).on("click", "#openEventTypeModal", function () {
        openedFromDeal = true; // mark flag
        $("#dealModal").modal("hide"); // hide deal modal
        $("#eventTypeModal").modal("show");
        resetEventTypeForm();
    });

    // When eventType Modal closes
    $("#eventTypeModal").on("hidden.bs.modal", function () {
        if (openedFromDeal) {
            $("#dealModal").modal("show"); // reopen only if it was opened from deal
            openedFromDeal = false; // reset flag
        }
    });

    // Open from Deal Modal (+ button)
    $(document).on("click", "#openEventSubTypeModal", function () {
        openedFromDeal = true; // mark flag
        $("#dealModal").modal("hide"); // hide deal modal
        $("#eventSubTypeModal").modal("show");
        resetEventSubTypeForm();
    });

    // When eventSubType Modal closes
    $("#eventSubTypeModal").on("hidden.bs.modal", function () {
        if (openedFromDeal) {
            $("#dealModal").modal("show"); // reopen only if it was opened from deal
            openedFromDeal = false; // reset flag
        }
    });

    // Open from Deal Modal (+ button)
    $(document).on("click", "#openBanquetHallModal", function () {
        openedFromDeal = true; // mark flag
        $("#dealModal").modal("hide"); // hide deal modal
        $("#banquetHallModal").modal("show");
        resetBanquetHallForm();
    });

    // When Banquet Hall Modal closes
    $("#banquetHallModal").on("hidden.bs.modal", function () {
        if (openedFromDeal) {
            $("#dealModal").modal("show"); // reopen only if it was opened from deal
            openedFromDeal = false; // reset flag
        }
    });

    // Open from Deal Modal (+ button)
    $(document).on("click", "#openLeadSourceModal", function () {
        openedFromDeal = true; // mark flag
        $("#dealModal").modal("hide"); // hide deal modal
        $("#leadSourceModal").modal("show");
        resetLeadSourceForm();
    });

    // When Lead Source Modal closes
    $("#leadSourceModal").on("hidden.bs.modal", function () {
        if (openedFromDeal) {
            $("#dealModal").modal("show"); // reopen only if it was opened from deal
            openedFromDeal = false; // reset flag
        }
    });

    // Open from Quotation Modal (+ button)
    $(document).off("click", ".open-quotation").on("click", ".open-quotation", function (e) {
        e.preventDefault();
        e.stopPropagation();

        const enquiryDealId = $(this).data("dealid");
        const isleadlost = $(this).data("isleadlost");
        const isleadbooked = $(this).data("isleadbooked");

        console.log("quotation enquiryDealId:", enquiryDealId);
        openQuotation(enquiryDealId, "view", isleadlost, isleadbooked);
    });

    // When Quotation Modal closes
    $("#quotationModal").on("hidden.bs.modal", function () {
        let action = $("#quotation_action").val();

        if (action == "view" || action == "edit") {
            let enquiryId = $("#quotation_enquiryDealId").val();
            console.log("View List Quotation for:", enquiryId);
            $("#quotationListModal").modal("hide");
            // 🔥 Call your function
            openQuotation(enquiryId, "view");
        }
    });

    //Modal Behaviour -- START ---------------------------------------------------

    const addonModal = document.getElementById('addonChargeModal');
    const bookingModal = document.getElementById('bookingModal');
    const menuModal = document.getElementById('menuModal');

    //hide/show Add-On MODAL ----- START --------------------------
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

    //hide/show Add-On MODAL ----- END --------------------------

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
    //Menu Item - START ----- END ---------------------------


    // Enquiry/Deal -- START --------------------------------------------------------------------
    // Save Deal Form
    document.getElementById("saveDealBtn").addEventListener("click", function (e) {
        e.preventDefault();
        if (!validateDealForm()) {
            console.log("Deal Save - Validation failed");
            return;
        }
        const $selectedOption = $("#dealProcess option:selected");
        const isConfirmed = $selectedOption.data("isconfirmed") === true || $selectedOption.data("isconfirmed") === "true";
        //const isLost = $selectedOption.data("islost") === true || $selectedOption.data("islost") === "true";

        const enquiryData = {
            Id: Number($("#dealId").val() || 0), // ensure number
            ProcessId: parseInt($("#dealProcess").val()) || 0,
            Name: $("#dealName").val().trim(),
            Contact: $("#dealContact").val().trim(),
            Address: $("#dealAddress").val().trim(),

            Email: $("#dealEmail").val().trim(),
            EventTypeId: parseInt($("#eventTypeDropdown").val()) || 0,
            EventSubTypeId: parseInt($("#eventSubTypeDropdown").val()) || 0,
            BanquetHallId: parseInt($("#banquetHallDropdown").val()) || 0,
            //EventDate: $("#eventDate").val(),

            EventDate: $("#eventDate").val(), // if required separately
            EventStartTime: $("#eventStart").val(),
            EventEndTime: $("#eventEnd").val(),
            Pax: parseInt($("#pax").val()) || 0,
            FollowUpDate: $("#followUpDate").val() || null,

            EstimatedAmount: parseFloat($("#estimatedAmount").val()) || 0,
            LeadSourceId: parseInt($("#leadSourceDropdown").val()) || 0,
            Remarks: $("#remark").val().trim(),
            PackageId: parseInt($("#packageDropdown").val()) || 0,
            ActionBy: Number(sessionStorage.getItem("userId") ?? 0),
            CompanyId: Number(sessionStorage.getItem("companyid") ?? 0),

            LeadLostReasonCategoryId: parseInt($("#ddlLeadLostReasonCategory_enquiry").val()) || 0,
            LeadLostReasonId: parseInt($("#ddlLeadLostReason_enquiry").val()) || 0
        };
        console.log("Posting EnquiryDeal:", enquiryData);

        $.ajax({
            url: "/api/EnquiryDeal/SaveEnquiryDeal",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(enquiryData),
            success: function (response) {
                if (response.status === "Success") {
                    console.log("response data:", response.data);

                    loadEnquiryList();
                    handleApiSuccess('Saved!');
                    loadEnquiryDeals();
                    resetDealForm();
                    bootstrap.Modal.getInstance(document.getElementById("dealModal")).hide();

                    if (isConfirmed) {
                        resetBooking();

                        //OPEN BOOKING MODAL
                        const dateStr = enquiryData.EventDate;       // for display
                        //const dateStr_Format = enquiryData.EventDate_Format1;
                        const dateStr_ISO = formatDate_DMYToYMD(enquiryData.EventDate);

                        const shift = response.data.ShiftCode;
                        const shiftname = response.data.ShiftFullname;
                        const date = dateStr;
                        const dateiso = dateStr_ISO;
                        const hallid = enquiryData.BanquetHallId;
                        const bookingId = response.data.BookingId;

                        $("#BookingId").val(bookingId);
                        // 🟢 Update label content & attributes - using jQuery
                        $("#label_date_shift")
                            .text(`${date}, ${shiftname} Slot`)        // visible text
                            .attr("data-eventdate", dateiso)           // yyyy-mm-dd
                            .attr("data-shiftid", shift);              // FD, MO, etc.
                        $("#banquetDropdown").val(hallid);

                        //set after 5sec
                        setTimeout(function () {
                            $("#clientEnquiryDropdown")
                                .val(response.data.EnquiryDealId)
                                .trigger("change").prop("disabled", true);
                        }, 5000);

                        $("#booking_pax").val(enquiryData.Pax);

                        setShiftTime(shift);

                        $("#PaymentDetails_DueDate").val('');
                        console.log("ib-dateiso:", dateiso);
                        $("#fromDate").val(dateiso); //.prop("disabled", true);
                        $("#toDate").val(dateiso);
                        $("#booking_subEventDropdown").val(enquiryData.EventSubTypeId).trigger("change");

                        $("#title_bookingModal").text("Add Booking Details");
                        $("#bookingModal").modal("show");

                    }
                    // TODO: refresh deal list if you have one
                    // loadDealList();
                } else {
                    Swal.fire({
                        icon: "info",
                        title: "Failed",
                        text: response.message
                    });
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Enquiry Save failed");
            }
        });
    });

    // Delete Deal
    $(document).on("click", "#deleteDealBtn", function () {
        let dealId = $(this).data("deal-id");

        Swal.fire({
            title: "Are you sure?",
            text: "Do you really want to delete this deal?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {

                var inputData = {
                    Id: dealId,
                    ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
                };

                $.ajax({
                    url: "/api/EnquiryDeal/DeleteEnquiryDeal",
                    type: "POST", // adjust if API expects DELETE
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(inputData),
                    success: function (response) {
                        if (response.status === "Success") {
                            handleApiSuccess('Deal Deleted!');
                            loadEnquiryDeals();
                            resetDealForm();
                            //bootstrap.Modal.getInstance(document.getElementById("dealModal")).hide();
                            $("#dealModal").modal("hide");
                        } else {
                            Swal.fire({
                                icon: "error",
                                title: "Error",
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: "Error deleting deal."
                        });
                    },
                    complete: function () {
                        //hideLoader();
                    }
                });
            }
        });
    });

    // Enquiry/Deal -- END ----------------------------------------------------------------------

    //EVENT TYPE MASTER -- START --------------------------------------------------------------------
    // Add Event Type
    document.getElementById("eventTypeForm").addEventListener("submit", function (e) {
        e.preventDefault();

        if (!validateEventTypeForm()) {
            return; // stop submission if validation fails
        }
        ////showLoader()();
        let id = Number($("#eventTypeId").val()) ?? 0;
        let name = $("#eventTypeName").val();

        var inputData = {
            Id: id,
            Name: name,
            ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
        };

        console.log("inputData EventType:", inputData);
        $.ajax({
            url: "/api/EventType/SaveEventType",   // your API for saving event type
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(inputData),
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess('Saved!');
                    resetEventTypeForm();
                    loadEventTypeList();
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: response.message
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Error saving event type."
                });
            },
            complete: function () {
                //hideLoader();
            }
        });

    });

    // Edit Event Type
    $(document).on("click", ".edit-eventType", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");
        let name = row.find("td:eq(1)").text();

        resetEventTypeForm();
        $("#eventTypeId").val(id);
        $("#eventTypeName").val(name);
    });

    // Delete Event Type
    $(document).on("click", ".delete-eventType", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");

        Swal.fire({
            title: "Are you sure?",
            text: "Do you really want to delete this event type?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                ////showLoader()();

                var inputData = {
                    Id: id,
                    ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
                }

                $.ajax({
                    url: "/api/EventType/DeleteEventType",
                    type: "POST", // backend expects POST
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(inputData),
                    success: function (response) {
                        if (response.status === "Success") {
                            handleApiSuccess('Deleted!');
                            loadEventTypeList();
                        } else {
                            Swal.fire({
                                icon: "error",
                                title: "Error",
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: "Error deleting event type."
                        });
                    },
                    complete: function () {
                        //hideLoader();
                    }
                });
            }
        });
    });

    // Event Type dropdown change event
    $(document).on("change", "#eventTypeDropdown, #eventTypeDropdown_SubTypeModal", function () {
        let selectedEventTypeId = $(this).val();
        $("#eventTypeDropdown, #eventTypeDropdown_SubTypeModal").val(selectedEventTypeId);
        if (selectedEventTypeId && selectedEventTypeId !== "0") {
            loadEventSubTypeList(selectedEventTypeId); // Pass selected EventTypeId
        } else {
            // Clear the Event Sub-Type table if no event type selected
            $("#eventSubTypeTable tbody").html(
                `<tr> <td colspan="4" class="text-center text-muted">Please select an Event Type</td></tr> `
            );
        }
    });

    // When SubType changes
    $(document).on("change", "#eventSubTypeDropdown", function () {
        const selectedOption = $(this).find("option:selected");
        const eventTypeId = selectedOption.data("eventtypeid");

        // If no event selected OR mismatch → auto assign
        if (eventTypeId && $("#eventTypeDropdown").val() != eventTypeId) {
            $("#eventTypeDropdown").val(eventTypeId);
                //.trigger("change");
        }
    });

    // When Sub Event Type changes
    $(document).on("change", "#quotation_subEventTypeId", function () {
        const selectedOption = $(this).find("option:selected");
        const eventTypeId = selectedOption.data("eventtypeid");

        const eventDropdown = $("#quotation_eventTypeId");

        // Auto assign EventType if not selected OR mismatch
        if (eventTypeId && eventDropdown.val() != eventTypeId) {
            eventDropdown.val(eventTypeId);
                //.trigger("change");
        }
    });

    //EVENT TYPE MASTER -- END ----------------------------------------------------------------------

    //EVENT SUB-TYPE MASTER -- START --------------------------------------------------------------------
    // Save / Update Event Sub-Type
    document.getElementById("eventSubTypeForm").addEventListener("submit", function (e) {
        e.preventDefault();

        if (!validateEventSubTypeForm()) {
            return; // stop submission if validation fails
        }
        ////showLoader()();

        let id = $("#eventSubTypeId").val();
        let name = $("#eventSubTypeName").val();
        let eventTypeId = $("#eventTypeDropdown_SubTypeModal").val();

        var inputData = {
            Id: id,
            Name: name,
            EventTypeId: eventTypeId,
            ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
        };

        $.ajax({
            url: "/api/EventSubType/SaveEventSubType", // your API endpoint
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(inputData),
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess("Saved!");
                    resetEventSubTypeForm();
                    loadEventSubTypeList(eventTypeId);
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: response.message
                    });
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Error saving event sub-type");
            }
        });
    });

    // Edit Event Sub-Type
    $(document).on("click", ".edit-eventSubType", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");
        let eventtypeid = row.data("eventtypeid");
        let name = row.find("td:eq(1)").text();

        resetEventSubTypeForm();
        $("#eventSubTypeId").val(id);
        $("#eventSubTypeName").val(name);
        $("#eventTypeDropdown_SubTypeModal").val(eventtypeid);
    });

    // Delete Event Sub-Type
    $(document).on("click", ".delete-eventSubType", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");

        Swal.fire({
            title: "Are you sure?",
            text: "Do you really want to delete this event sub-type?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                ////showLoader()();

                var inputData = {
                    Id: id,
                    ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
                };

                $.ajax({
                    url: "/api/EventSubType/DeleteEventSubType", // API endpoint
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(inputData),
                    success: function (response) {
                        if (response.status === "Success") {
                            handleApiSuccess('Deleted!');
                            loadEventSubTypeList();
                        } else {
                            Swal.fire({
                                icon: "error",
                                title: "Error",
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: "Error deleting event sub-type."
                        });
                    },
                    complete: function () {
                        //hideLoader();
                    }
                });
            }
        });
    });

    //EVENT SUB-TYPE MASTER -- END ----------------------------------------------------------------------

    //EVENT LEAD SOURCE MASTER -- START ----------------------------------------------------------------------
    // ✅ Add / Update Lead Source
    document.getElementById("leadSourceForm").addEventListener("submit", function (e) {
        e.preventDefault();

        if (!validateLeadSourceForm()) {
            return; // stop submission if validation fails
        }
        ////showLoader()();

        let id = $("#leadSourceId").val();
        let name = $("#leadSourceName").val();

        var inputData = {
            Id: id,
            Name: name,
            ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
        };

        $.ajax({
            url: "/api/LeadSource/SaveLeadSource",   // your API for saving lead source
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(inputData),
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess('Saved!');
                    resetLeadSourceForm();
                    loadLeadSourceList();
                } else {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: response.message
                    });
                }
            },
            error: function () {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: "Error saving lead source."
                });
            },
            complete: function () {
                //hideLoader();
            }
        });
    });

    // ✅ Edit Lead Source
    $(document).on("click", ".edit-leadSource", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");
        let name = row.find("td:eq(1)").text();

        resetLeadSourceForm();
        $("#leadSourceId").val(id);
        $("#leadSourceName").val(name);
    });

    // ✅ Delete Lead Source
    $(document).on("click", ".delete-leadSource", function () {
        let row = $(this).closest("tr");
        let id = row.data("id");

        Swal.fire({
            title: "Are you sure?",
            text: "Do you really want to delete this lead source?",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#3085d6",
            confirmButtonText: "Yes, delete it!",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                ////showLoader()();

                var inputData = {
                    Id: id,
                    ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
                }

                $.ajax({
                    url: "/api/LeadSource/DeleteLeadSource",
                    type: "POST", // backend expects POST
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify(inputData),
                    success: function (response) {
                        if (response.status === "Success") {
                            handleApiSuccess('Deleted!');
                            loadLeadSourceList();
                        } else {
                            Swal.fire({
                                icon: "error",
                                title: "Error",
                                text: response.message
                            });
                        }
                    },
                    error: function () {
                        Swal.fire({
                            icon: "error",
                            title: "Error",
                            text: "Error deleting lead source."
                        });
                    },
                    complete: function () {
                        //hideLoader();
                    }
                });
            }
        });
    });
    //EVENT LEAD SOURCE MASTER -- END ----------------------------------------------------------------------

    flatpickr("#eventDate", {
        dateFormat: "d/m/Y",
        disableMobile: true,
        onChange: function () {
            $("#eventDate").trigger("change");
        }
    });

    $("#eventDate, #eventStart, #eventEnd, #banquetHallDropdown").on("change", function () {

        let eventDate = $("#eventDate").val().trim();
        let eventStart = $("#eventStart").val().trim();
        let eventEnd = $("#eventEnd").val().trim();   
        let banquetId = parseInt($("#banquetHallDropdown").val(), 10) || 0;

        $("#slotStatusMsg").addClass("d-none");

        if (!eventDate || !eventStart || !eventEnd || banquetId <= 0) {
            return;
        }

        var payload = {
            EventDate: eventDate,
            StartTime: eventStart, // ✅ 12-hr AM/PM
            EndTime: eventEnd,   // ✅ 12-hr AM/PM
            BanquetId: banquetId
        };

        console.log("READY TO CALL API", payload);

        var loaderMsg = "Checking Availability";
        const loaderId = showTempLoader(loaderMsg);

        $.ajax({
            url: "/api/EnquiryDeal/CheckBookingSlotAvailability",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                Swal.close(); // Close loader
                if (res.status === "Success") {
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
                            //.addClass("bg-warning-subtle text-dark")
                            .addClass("alert text-white")
                            .css({
                                "background-color": "#e5780b",
                                "border": "1px solid #e5780b",
                            })
                            .html(res.data.WarningMessage);
                        $("#slotStatusWarning_div").removeClass("d-none");
                    }
                    else {
                        warningBox.addClass("d-none");
                        $("#slotStatusWarning_div").addClass("d-none");
                    }
                }
            },
            error: function (xhr, status, error) {
                Swal.close(); // Close loader
                $("#slotStatusMsg")
                    .removeClass("d-none text-success")
                    .addClass("text-danger")
                    .text("Unable to check slot availability");
            },
            complete: function () {
                hideTempLoader(loaderId);
            }
        });
    });

});

//package dropdown -- START --------------------
let foodPackages = [];
function loadFoodPackageList(selectedPackageId = null) {
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
                populateFoodPackageDropdown($("#packageDropdown, #foodPackage_PackageDropdown"));
                renderFoodPackages(foodPackages);

                //console.log("package:", foodPackages);

                // ✅ SET VALUE AFTER DROPDOWN IS READY
                if (selectedPackageId) {
                    //console.log("selectedPackageId:", selectedPackageId);
                    $("#packageDropdown").val(String(selectedPackageId)).trigger("change");
                }
            }
            else {
                $("#packageDropdown, #foodPackage_PackageDropdown, #quotation_packageContainer").html(`<option value="0">No Package Found</option>`);
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
function renderFoodPackages(packages) {

    if (!packages || packages.length === 0) {
        $("#quotation_packageContainer").html(`
            <div class="text-center text-danger small py-2">
                No Package Found
            </div>
        `);
        return;
    }

    let html = `
        <div class="d-flex justify-content-between mb-2 border-bottom pb-1">
            <small class="fw-semibold">Select Food Package</small>
            <small class="fw-semibold">Rate/PP</small>
        </div>
    `;

    $.each(packages, function (i, pkg) {

        let rate = parseFloat(pkg.RatePerPerson) || 0;
        let name = pkg.PackageName || pkg.Name || "Package";

        html += `
            <div class="d-flex justify-content-between align-items-center mb-1 package-row">

                <div class="form-check">
                    <input type="checkbox"
                           class="form-check-input package-checkbox"
                           value="${pkg.Id}"
                           data-rate="${rate}" />

                    <label class="form-check-label">
                        ${name}
                    </label>
                </div>

                <input type="text"
                       class="form-control form-control-sm text-end bg-light p-1"
                       value="${rate}"
                       readonly
                       style="width:75px;" />

            </div>
        `;
    });

    $("#quotation_packageContainer").html(html);
}

//package dropdown -- END --------------------

//banquet hall details - dropdown -- START ----------------------
function fetchHallDetails() {
    $.ajax({
        url: '/api/Company_HallDetails/GetCompanyHallDetailsList',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0 }), // Get all halls
        success: function (response) {
            if (response.status === "Success" && response.data) {

                banquetHallsList = response.data;
                let options = `<option value="0">-- Select Hall --</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.HallName_Display}</option>`;
                });
                $("#banquetHallDropdown, #banquetDropdown, #quotation_banquetId").html(options);
            }
            else {
                $("#banquetHallDropdown, #banquetDropdown, #quotation_banquetId").html(`<option value="0">No Hall Found</option>`);
            }

        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to fetch hall details');
        }
    });
}
//banquet hall details - dropdown -- END ----------------------

// Load event type list from API
function loadEventTypeList() {
    $.ajax({
        url: "/api/EventType/GetEventTypeList",
        type: "GET",
        success: function (response) {
            console.log("eventType response:", response);
            if (response.status === "Success") {
                let rows = "";
                let srNo = 1;
                let options = `<option value = "0"> --Select Event Type--</option> `; // default option
                let options1 = `<option value = "0">--Select Event--</option> `; // default option

                $.each(response.data, function (index, item) {
                    rows += `
                            <tr data-id="${item.Id}">
                                <td class="text-center">${srNo++}</td>
                                <td>${item.Name}</td>
                                <td class="text-center">
                                    ${item.IsEditable ? `
                                        <button class="btn btn-outline-warning btn-action-xs me-1 edit-eventType">
                                            <i class="fa fa-pencil"></i>
                                        </button>
                                        <button class="btn btn-outline-danger btn-action-xs delete-eventType">
                                            <i class="fa fa-trash"></i>
                                        </button>
                                    ` : 
                                    `<span class="text-danger small fw-semibold">
                                        Modification Not Allowed
                                    </span>`
                                }
                                </td>
                            </tr>
    `;
                    options += `<option value = "${item.Id}"> ${item.Name}</option>`;
                    options1 += `<option value = "${item.Id}"> ${item.Name}</option>`;
                });

                $("#eventTypeTable tbody").html(rows);
                $("#eventTypeDropdown,#booking_eventDropdown, #eventTypeDropdown_SubTypeModal").html(options);
                $("#quotation_eventTypeId").html(options1);
                //loadEventSubTypeList(eventTypeId);
            } else {
                $("#eventTypeTable tbody").html(
                    `<tr> <td colspan="3" class="text-center text-danger">No records found</td></tr> `
                );
                $("#eventTypeDropdown, #booking_eventDropdown, #eventTypeDropdown_SubTypeModal, #quotation_eventTypeId").html(`<option value = "0"> No Event Type Found</option> `);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load EventType List");
        }
    });
}

// Event Type dropdown change event
$(document).on("change", "#booking_eventDropdown", function () {
    let selectedEventTypeId = $(this).val();
    loadEventSubTypeList(selectedEventTypeId); // Pass selected EventTypeId
});
// When Sub Event changes
$(document).on("change", "#booking_subEventDropdown", function () {
    const selectedOption = $(this).find("option:selected");
    const eventTypeId = selectedOption.data("eventtypeid");

    const eventDropdown = $("#booking_eventDropdown");

    // Auto assign EventType if not selected OR mismatch
    if (eventTypeId && eventDropdown.val() != eventTypeId) {
        eventDropdown.val(eventTypeId)
            //.trigger("change")
            ;
    }
});


// Reset Event Type Form
function resetEventTypeForm() {
    $("#eventTypeForm")[0].reset();
    $("#eventTypeId").val(0);
    // Reset error message
    $("#eventTypeName").siblings(".error-msg").addClass("d-none");

}

// ✅ Validation Function for Event Type Form
function validateEventTypeForm() {
    let isValid = true;

    let name = $("#eventTypeName").val().trim();

    // Reset error message
    $("#eventTypeName").siblings(".error-msg").addClass("d-none");

    // Validate Event Type Name
    if (name === "" || name === null) {
        $("#eventTypeName").siblings(".error-msg").removeClass("d-none").text("Please enter event type");
        isValid = false;
    }

    return isValid;
}

// Load event sub-type list from API
function loadEventSubTypeList(eventTypeId = 0) {
    var inputData = {
        EventTypeId: eventTypeId // match your C# EventSubType_Request model
    };

    $.ajax({
        url: "/api/EventSubType/GetEventSubTypeList",
        type: "POST", // Must be POST since backend expects POST
        contentType: "application/json",
        data: JSON.stringify(inputData),
        success: function (response) {
            //console.log("eventSubType response:", response);
            if (response.status === "Success") {
                let rows = "";
                let srNo = 1;
                let options = `<option value = "0"> --Select SubEvent--</option> `;

                $.each(response.data, function (index, item) {
                    rows += `
                            <tr data-id="${item.Id}" data-eventtypeid="${item.EventTypeId}">
                                <td class="text-center">${srNo++}</td>
                                <td>${item.Name}</td>
                                <td class="text-center">
                                    <button class="btn btn-outline-warning btn-action-xs me-1 edit-eventSubType">
                                        <i class="fa fa-pencil"></i>
                                    </button>
                                    <button class="btn btn-outline-danger btn-action-xs delete-eventSubType">
                                        <i class="fa fa-trash"></i>
                                    </button>
                                </td>
                            </tr>
                    `;
                    options += `<option value = "${item.Id}" data-eventtypeid="${item.EventTypeId}">${item.Name}</option> `;
                });

                $("#eventSubTypeTable tbody").html(rows);
                $("#booking_subEventDropdown, #quotation_subEventTypeId, #eventSubTypeDropdown").html(options);
            } else {
                $("#eventSubTypeTable tbody").html(
                    `<tr> <td colspan="3" class="text-center text-danger">No records found</td></tr> `
                );
                $("#eventSubTypeDropdown, #quotation_subEventTypeId, #booking_subEventDropdown").html(`<option value = "0">No SubEvent Found</option> `);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching event sub-type list.");
        }
    });
}

// Reset Event Sub-Type Form
function resetEventSubTypeForm() {
    //$("#eventSubTypeForm")[0].reset();
    $("#eventSubTypeId").val(0);
    $("#eventSubTypeName").val("");

    // Reset error message
    $("#eventSubTypeName").siblings(".error-msg").addClass("d-none");
    $("#eventTypeDropdown_SubTypeModal").siblings(".error-msg").addClass("d-none");
}

// ✅ Event Sub-Type Validation Function
function validateEventSubTypeForm() {
    let isValid = true;

    let eventTypeId = $("#eventTypeDropdown_SubTypeModal").val();
    let name = $("#eventSubTypeName").val().trim();

    // Reset error messages
    $("#eventSubTypeForm .error-msg").addClass("d-none");

    // Validate Event Type
    if (eventTypeId === "0" || !eventTypeId) {
        $("#eventTypeDropdown_SubTypeModal")
            .closest(".flex-grow-1")   // go to parent container
            .find(".error-msg")
            .removeClass("d-none");
        isValid = false;
    }

    // Validate Event Sub-Type Name
    if (!name) {
        $("#eventSubTypeName")
            .closest(".flex-grow-1")   // go to parent container
            .find(".error-msg")
            .removeClass("d-none");
        isValid = false;
    }

    return isValid;
}

// Load lead source list from API
function loadLeadSourceList() {
    $.ajax({
        url: "/api/LeadSource/GetLeadSourceList",
        type: "GET",
        success: function (response) {
            //console.log("leadSource response:", response);
            if (response.status && response.status.toLowerCase() === "success") {
                let rows = "";
                let srNo = 1;
                let options = `<option value = "0"> --Select Lead Source--</option> `; // default option

                $.each(response.data, function (index, item) {
                    rows += `
                            <tr data-id="${item.Id}">
                                <td class="text-center">${srNo++}</td>
                                <td>${item.Name}</td>
                                <td class="text-center">
                                    <button class="btn btn-outline-warning btn-action-xs me-1 edit-leadSource">
                                        <i class="fa fa-pencil"></i>
                                    </button>
                                    <button class="btn btn-outline-danger btn-action-xs delete-leadSource">
                                        <i class="fa fa-trash"></i>
                                    </button>
                                </td>
                            </tr>
    `;
                    options += `<option value = "${item.Id}"> ${item.Name}</option> `;
                });

                $("#leadSourceTable tbody").html(rows);
                $("#leadSourceDropdown, #booking_leadReferenceDropdown, #quotation_referenceTypeId").html(options);
            }
            else {
                $("#leadSourceTable tbody").html(
                    `<tr> <td colspan="3" class="text-center text-danger">No records found</td></tr> `
                );
                $("#leadSourceDropdown, #booking_leadReferenceDropdown, #quotation_referenceTypeId").html(`<option value = "0">No Lead Source Found</option> `);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching lead source list.");
        }
    });
}

// Reset Lead Source Form
function resetLeadSourceForm() {
    $("#leadSourceForm")[0].reset();
    $("#leadSourceId").val(0);
    // Reset error message
    $("#leadSourceName").siblings(".error-msg").addClass("d-none");
}

// ✅ Validation Function for Lead Source Form
function validateLeadSourceForm() {
    let isValid = true;
    let name = $("#leadSourceName").val().trim();

    // Reset error message
    $("#leadSourceName").siblings(".error-msg").addClass("d-none");

    // Validate Lead Source Name
    if (name === "" || name === null) {
        $("#leadSourceName").siblings(".error-msg").removeClass("d-none").text("Please enter lead source");
        isValid = false;
    }

    return isValid;
}

//<!--file upload / drag-before Moving Enquiry -- START -->


// Reusable function for any dropzone + file input + hidden base64 input
function initDropzone(dropzoneId, fileInputId, hiddenBase64Id) {

    const dz = document.getElementById(dropzoneId);
    const fi = document.getElementById(fileInputId);
    const hb = document.getElementById(hiddenBase64Id);

    // console.log("Init Dropzone Debug:");
    // console.log("Dropzone Element:", dz);
    // console.log("File Input Element:", fi);
    // console.log("Hidden Base64 Input:", hb);

    // On click → open file dialog
    dz.addEventListener("click", () => fi.click());

    // Dragging over
    dz.addEventListener("dragover", (e) => {
        e.preventDefault();
        dz.classList.add("bg-primary", "text-white");
    });

    // Drag leave
    dz.addEventListener("dragleave", () => {
        dz.classList.remove("bg-primary", "text-white");
    });

    // File dropped
    dz.addEventListener("drop", (e) => {
        e.preventDefault();
        dz.classList.remove("bg-primary", "text-white");

        const file = e.dataTransfer.files[0];
        handleFile(file, dz, hb, fi);
    });

    // File selected manually
    fi.addEventListener("change", () => {
        const file = fi.files[0];
        handleFile(file, dz, hb, fi);
    });
}

// Core file handling logic
function handleFile(file, dz, hb, fi) {
    if (!file) return;

    // Validate size (max 5 MB)
    if (file.size > 5 * 1024 * 1024) {
        alert("File size exceeds 5 MB limit!");
        fi.value = "";
        hb.value = "";
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        const base64 = e.target.result.split(',')[1];
        hb.value = base64;

        // Update UI with filename
        dz.querySelector("p.fw-semibold").innerText = `Selected: ${file.name} `;
    };

    reader.readAsDataURL(file);
}

// Initialize Dropzone for FollowUp Form
//initDropzone("attachmentDropzone", "attachment", "attachmentBase64_enquiryMove");
initDropzone("attachmentDropzone_followUp", "attachment_followUp", "attachmentBase64_followUp");

/* =========================================================
   CLOCK PICKER REUSABLE FUNCTION
========================================================= */

function initializeClockPicker(config) {

    const input = document.getElementById(config.inputId);
    const picker = document.getElementById(config.pickerId);
    const hourDisplay = document.getElementById(config.hourDisplayId);
    const minuteDisplay = document.getElementById(config.minuteDisplayId);
    const hourHand = document.getElementById(config.hourHandId);
    const minuteHand = document.getElementById(config.minuteHandId);
    const amBtn = document.getElementById(config.amBtnId);
    const pmBtn = document.getElementById(config.pmBtnId);
    const setBtn = document.getElementById(config.setBtnId);

    if (!input || !picker || !hourDisplay || !minuteDisplay || !hourHand || !minuteHand || !amBtn || !pmBtn || !setBtn) {
        console.warn("Clock picker element not found:", config);
        return;
    }

    let hour = 12;
    let minute = 0;
    let ampm = "AM";
    let mode = "hour";


    /* =========================================================
       SHOW / HIDE PICKER
    ========================================================= */

    function showPicker() {

        picker.classList.remove("d-none");

        picker.classList.add("picker-transition");
        picker.classList.add("picker-hide");

        requestAnimationFrame(() => {
            picker.classList.remove("picker-hide");
        });
    }


    function hidePicker() {

        picker.classList.add("picker-transition");
        picker.classList.add("picker-hide");

        setTimeout(() => {
            picker.classList.add("d-none");
            picker.classList.remove("picker-hide");
        }, 300);
    }


    /* =========================================================
       INITIALIZE TIME
    ========================================================= */

    function initializeTime(value) {

        let time = (value || "").trim();

        if (!time) time = "12:00 AM";

        const parts = time.split(" ");

        const timePart = parts[0] || "12:00";
        const modifier = parts[1] || "AM";

        let [h, m] = timePart.split(":").map(Number);

        hour = h || 12;
        minute = m || 0;
        ampm = modifier;


        hourDisplay.textContent = hour.toString().padStart(2, "0");
        minuteDisplay.textContent = minute.toString().padStart(2, "0");


        hourHand.style.transform = `rotate(${(hour % 12) * 30}deg)`;
        minuteHand.style.transform = `rotate(${minute * 6}deg)`;


        if (ampm === "AM") {

            amBtn.classList.add("active");
            pmBtn.classList.remove("active");

        } else {

            pmBtn.classList.add("active");
            amBtn.classList.remove("active");

        }

        setMode("hour");
    }


    /* =========================================================
       HOUR / MINUTE MODE
    ========================================================= */

    function setMode(selectedMode) {

        mode = selectedMode;

        if (mode === "hour") {

            hourDisplay.classList.add("active-text");
            hourDisplay.classList.remove("inactive-text");

            minuteDisplay.classList.remove("active-text");
            minuteDisplay.classList.add("inactive-text");

            hourHand.classList.add("active-hand");
            minuteHand.classList.remove("active-hand");

        } else {

            minuteDisplay.classList.add("active-text");
            minuteDisplay.classList.remove("inactive-text");

            hourDisplay.classList.remove("active-text");
            hourDisplay.classList.add("inactive-text");

            minuteHand.classList.add("active-hand");
            hourHand.classList.remove("active-hand");

        }
    }


    /* =========================================================
       INPUT CLICK
    ========================================================= */

    input.addEventListener("click", function (e) {

        e.stopPropagation();

        initializeTime(input.value);

        if (picker.classList.contains("d-none")) {
            showPicker();
        } else {
            hidePicker();
        }

    });


    /* =========================================================
       HOUR / MINUTE DISPLAY CLICK
    ========================================================= */

    hourDisplay.addEventListener("click", function () {
        setMode("hour");
    });


    minuteDisplay.addEventListener("click", function () {
        setMode("minute");
    });


    /* =========================================================
       CLOCK NUMBER CLICK
    ========================================================= */

    picker.querySelectorAll(".clock-number").forEach(function (num) {

        num.addEventListener("click", function () {

            const value = parseInt(num.textContent);

            if (mode === "hour") {

                hour = value;

                hourDisplay.textContent = value.toString().padStart(2, "0");

                hourHand.style.transform = `rotate(${(value % 12) * 30}deg)`;

            } else {

                minute = (value % 12) * 5;

                minuteDisplay.textContent = minute.toString().padStart(2, "0");

                minuteHand.style.transform = `rotate(${minute * 6}deg)`;

            }

        });

    });


    /* =========================================================
       AM BUTTON
    ========================================================= */

    amBtn.addEventListener("click", function () {

        ampm = "AM";

        amBtn.classList.add("active");
        pmBtn.classList.remove("active");

    });


    /* =========================================================
       PM BUTTON
    ========================================================= */

    pmBtn.addEventListener("click", function () {

        ampm = "PM";

        pmBtn.classList.add("active");
        amBtn.classList.remove("active");

    });


    /* =========================================================
       SET TIME
    ========================================================= */

    function setTime() {

        input.value = `${hourDisplay.textContent}:${minuteDisplay.textContent} ${ampm}`;

        input.dispatchEvent(new Event("change", {
            bubbles: true
        }));

        hidePicker();
    }


    setBtn.addEventListener("click", setTime);


    /* =========================================================
       OUTSIDE CLICK
    ========================================================= */

    document.addEventListener("click", function (e) {

        if (!picker.contains(e.target) && e.target !== input) {

            if (!picker.classList.contains("d-none")) {
                setTime();
            }

        }

    });


    /* =========================================================
       DEFAULT MODE
    ========================================================= */

    setMode("hour");

}


/* =========================================================
   🟢 DEAL / ENQUIRY - START TIME
========================================================= */

initializeClockPicker({

    inputId: "eventStart_enquiry",
    pickerId: "clockPicker_enquiry",

    hourDisplayId: "hourDisplay_enquiry",
    minuteDisplayId: "minuteDisplay_enquiry",

    hourHandId: "hourHand_enquiry",
    minuteHandId: "minuteHand_enquiry",

    amBtnId: "amBtn_enquiry",
    pmBtnId: "pmBtn_enquiry",

    setBtnId: "setTimeBtn_enquiry"

});


/* =========================================================
   🟢 DEAL / ENQUIRY - END TIME
========================================================= */

initializeClockPicker({

    inputId: "eventEnd_enquiry",
    pickerId: "clockPickerEnd_enquiry",

    hourDisplayId: "hourDisplayEnd_enquiry",
    minuteDisplayId: "minuteDisplayEnd_enquiry",

    hourHandId: "hourHandEnd_enquiry",
    minuteHandId: "minuteHandEnd_enquiry",

    amBtnId: "amBtnEnd_enquiry",
    pmBtnId: "pmBtnEnd_enquiry",

    setBtnId: "setTimeBtnEnd_enquiry"

});


/* =========================================================
   🔵 BOOKING - START TIME
========================================================= */

initializeClockPicker({

    inputId: "eventStart",
    pickerId: "clockPicker",

    hourDisplayId: "hourDisplay",
    minuteDisplayId: "minuteDisplay",

    hourHandId: "hourHand",
    minuteHandId: "minuteHand",

    amBtnId: "amBtn",
    pmBtnId: "pmBtn",

    setBtnId: "setTimeBtn"

});


/* =========================================================
   🔵 BOOKING - END TIME
========================================================= */

initializeClockPicker({

    inputId: "eventEnd",
    pickerId: "clockPickerEnd",

    hourDisplayId: "hourDisplayEnd",
    minuteDisplayId: "minuteDisplayEnd",

    hourHandId: "hourHandEnd",
    minuteHandId: "minuteHandEnd",

    amBtnId: "amBtnEnd",
    pmBtnId: "pmBtnEnd",

    setBtnId: "setTimeBtnEnd"

});




//BOOKING CALENDAR DESIGN - START ----------------------------------------------------------------------
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

    $("#eventStart").val("");
    $("#eventEnd").val("");
    
    // Reset any special fields
    $("#gstSwitch").prop("checked", false);
    $("#gstSwitchText").text("Off").removeClass("text-success").addClass("text-danger");  // Red for OFF
    $("#foodPackageContainer .foodPackage_TotalValue").text("₹ 0");
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
 
    recalculatePaymentSummary();

    //guest
    $("#bookingGuestContainer").empty();
    addGuestRow();

    $("#btnBook").prop("disabled", false);
}

function resetFoodPackages() {
    // Remove all dynamically added blocks
    $("#foodPackageContainer .food-block").remove();

    // Reset first (static) row inputs
    const $firstRow = $("#foodPackageContainer > .food-row");

    $firstRow.find("select").val('0'); //.trigger('change');
    $firstRow.find("input").val(0);
    $firstRow.find(".foodPackage_TotalValue").text("₹ 0");

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
        let value = parseFloat($(this).text().replace(/₹|,/g, "").trim()) || 0;
        taxableAmount += value;
    });

    // 🔹 Other Charges
    let decoration = parseFloat($("#OtherCharges_Decoration_Amount").val()) || 0;
    let room = parseFloat($("#OtherCharges_RoomCharges_Amount").val()) || 0;
    let sound = parseFloat($("#OtherCharges_SoundSystem_Amount").val()) || 0;
    let projector = parseFloat($("#OtherCharges_ProjectorScreen_Amount").val()) || 0;
    let electricityBill = parseFloat($("#OtherCharges_ElectricityBill_Amount").val()) || 0;
    let generatorCharge = parseFloat($("#OtherCharges_GeneratorCharge_Amount").val()) || 0;
    let otherCharges = parseFloat($("#OtherCharges_Amount").val()) || 0;

    taxableAmount += decoration + room + sound + projector + electricityBill + generatorCharge + otherCharges;

    //console.log("decoration:", decoration);
    //console.log("room:", room);
    //console.log("sound:", sound);
    //console.log("projector:", projector);

    $("#paymentSummary_TaxableAmount").text("₹ " + formatINR(taxableAmount));
    //console.log("taxableAmount:", taxableAmount);

    // 🔹 GST
    let totalGST = isGSTEnabled ? (taxableAmount * gstRate) / 100 : 0;

    let cgst = isGSTEnabled ? totalGST / 2 : 0;
    let sgst = isGSTEnabled ? totalGST / 2 : 0;
    let igst = 0;

    $("#paymentSummary_CGST").text("₹ " + formatINR(cgst));
    $("#paymentSummary_SGST").text("₹ " + formatINR(sgst));
    $("#paymentSummary_IGST").text("₹ " + formatINR(igst));

    let total = taxableAmount + cgst + sgst + igst;

    $("#paymentSummary_Total").text("₹ " + formatINR(total));
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

    $("#paymentSummary_BookingAmount").text("₹ " + formatINR(bookingDiscount));

    let payableAmount = totalAmount - bookingDiscount;
    //console.log("payableAmount:", formatINR(payableAmount));
    $("#paymentSummary_payableAmount").text("₹ " + formatINR(payableAmount));
}


//GUEST -- START
//FOR BOOKING MODAL - GUEST
function loadGuestCityDropdown(ddl, selectedCityId = 0) {
    ddl.empty();
    ddl.append('<option value="">Select City</option>');

    $.each(cityList, function (i, city) {
        ddl.append(`
            <option value="${city.Id}">
                ${city.Name} (${city.StateName})
            </option>
        `);
    });

    if (selectedCityId > 0) {
        ddl.val(selectedCityId);
    }

    //ddl.trigger("change");
}

function addGuestRow(selectedCityId = 0) {
    const row = $("#guestRowTemplate").children().first().clone();
    $("#bookingGuestContainer").append(row);

    //row.find(".guest-city").select2({
        //width: "100%"
    //});

    loadGuestCityDropdown(row.find(".guest-city"), selectedCityId);
    updateGuestSrNo();
    return row;
}

//ADD GUEST
addGuestRow();
$("#btnAddGuestRow").click(function () {
    addGuestRow();
});
//REMOVE GUEST
$(document).on("click", ".btnRemoveGuestRow", function () {
    if ($("#bookingGuestContainer .guest-row").length <= 1) {
        Swal.fire(
            "Guest Required",
            "At least one guest is required.",
            "warning"
        );
        return;
    }
    $(this).closest(".guest-row").remove();
    updateGuestSrNo();
});

//clear-dates
$(document).on("click", ".btn-clear-dates", function () {
    const row = $(this).closest(".guest-row"); // or adjust to your row class

    row.find(".guest-dob").val("");
    row.find(".guest-anniversary").val("");
});
//copy - Whatsapp No = Mobile No
$(document).on("click", ".btn-copy-mobile", function () {
    const row = $(this).closest(".guest-row"); // or your guest container
    const mobile = row.find(".guest-mobile").val();
    row.find(".guest-whatsapp").val(mobile);
});
function updateGuestSrNo() {
    $("#bookingGuestContainer")
        .children()
        .each(function (index) {
            $(this).find(".guest-srno").text("#" + (index + 1));
        });
}
// Guest Modal -- END --------------------
//GUEST -- END

//SAVE BOOKING ------ START ------------------------------
$("#btnBook").on("click", function () { saveBooking(); });
$("#btnReset").on("click", function () { resetBooking(); });
//SAVE BOOKING ------ END --------------------------------

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

loadBookingNotesMaster();
function loadBookingNotesMaster() {
    $.ajax({
        url: '/api/BookingCalendar/GetBookingCalendar_NotesMaster',
        type: 'GET',
        success: function (response) {
            //console.log("Notes Master Response:", response);
            if (!response || response.status !== 'Success') {
                console.error("Invalid response");
                return;
            }
            renderNotesMaster(response.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load Notes Master Data");
        }
    });
}

function renderNotesMaster(categories) {
    let html = '';
    (categories || []).forEach(category => {
        html += `
            <div class="note-category-card mb-2">

                <div class="note-category-header d-flex justify-content-between align-items-center cursor-pointer" data-categoryid="${category.CategoryId}">
                    <span class="fw-bold">${category.Name}</span>
                    <i class="fa fa-chevron-down rotate-icon"></i>
                </div>

                <div class="note-category-body p-3 border-top">
        `;

        (category.SubCategories || []).forEach(sub => {
            html += `
                <div class="note-subcategory row align-items-center mb-1"
                     data-categoryid="${category.CategoryId}"
                     data-subcategoryid="${sub.SubCategoryId}">

                    <div class="col-md-3">
                        <label class="fw-semibold text-secondary mb-0">
                            ${sub.Name}
                        </label>
                    </div>

                    <div class="col-md-3">
            `;

            //------------------------------------------
            // MultiSelect
            //------------------------------------------

            if (sub.InputType === "MultiSelect") {
                html += `<div class="d-flex flex-wrap gap-3">`;
                (sub.Options || []).forEach(option => {
                    html += `
                        <div class="form-check">
                            <input
                                class="form-check-input note-option"
                                type="checkbox"
                                data-categoryid="${category.CategoryId}"
                                data-subcategoryid="${sub.SubCategoryId}"
                                data-optionid="${option.SubCategoryOptionId}">

                            <label class="form-check-label">
                                ${option.OptionName}
                            </label>
                        </div>
                    `;
                });

                html += `</div>`;
            }

            //------------------------------------------
            // Select
            //------------------------------------------
            else if (sub.InputType === "Select") {
                html += `<div class="d-flex flex-wrap gap-3">`;
                (sub.Options || []).forEach(option => {
                    html += `
                        <div class="form-check">
                            <input
                                class="form-check-input note-option"
                                type="radio"
                                name="radio_${sub.SubCategoryId}"
                                data-categoryid="${category.CategoryId}"
                                data-subcategoryid="${sub.SubCategoryId}"
                                data-optionid="${option.SubCategoryOptionId}">

                            <label class="form-check-label">
                                ${option.OptionName}
                            </label>
                        </div>
                    `;
                });
                html += `</div>`;
            }

            //------------------------------------------
            // Dropdown
            //------------------------------------------
            else if (sub.InputType === "Dropdown") {
                html += `
                    <select class="form-select form-select-sm note-dropdown"
                        data-categoryid="${category.CategoryId}"
                        data-subcategoryid="${sub.SubCategoryId}">

                        <option value="">Select</option>
                `;
                (sub.Options || []).forEach(option => {
                    html += `
                        <option value="${option.SubCategoryOptionId}">
                            ${option.OptionName}
                        </option>
                    `;
                });
                html += `</select>`;
            }

            //------------------------------------------
            // Text
            //------------------------------------------
            else if (sub.InputType === "Text") {
                html += `
                    <input
                        type="text"
                        class="form-control form-control-sm note-text-value"
                        data-categoryid="${category.CategoryId}"
                        data-subcategoryid="${sub.SubCategoryId}">
                `;
            }

            //------------------------------------------
            // Number
            //------------------------------------------
            else if (sub.InputType === "Number") {
                html += `
                    <input
                        type="number"
                        class="form-control form-control-sm note-text-value"
                        data-categoryid="${category.CategoryId}"
                        data-subcategoryid="${sub.SubCategoryId}">
                `;
            }

            //------------------------------------------
            // Date
            //------------------------------------------
            else if (sub.InputType === "Date") {
                html += `
                    <input
                        type="date"
                        class="form-control form-control-sm note-text-value"
                        data-categoryid="${category.CategoryId}"
                        data-subcategoryid="${sub.SubCategoryId}">
                `;
            }

            //------------------------------------------
            // Time
            //------------------------------------------
            else if (sub.InputType === "Time") {
                html += `
                    <select
                        class="form-select form-select-sm note-time-value"
                        data-categoryid="${category.CategoryId}"
                        data-subcategoryid="${sub.SubCategoryId}">
                        <option value="">Select Time</option>
                    </select>
                `;
            }

            //------------------------------------------
            // Boolean
            //------------------------------------------
            else if (sub.InputType === "Boolean") {
                html += `
                    <div class="form-check form-switch">
                        <input
                            class="form-check-input note-boolean"
                            type="checkbox"
                            data-categoryid="${category.CategoryId}"
                            data-subcategoryid="${sub.SubCategoryId}">
                    </div>
                `;
            }

            //------------------------------------------
            // Close Input Column
            //------------------------------------------
            html += `
                    </div>
                    <div class="col-md-6">
                        <input type="text"
                            class="form-control form-control-sm note-remarks"
                            placeholder="Remarks"
                            data-categoryid="${category.CategoryId}"
                            data-subcategoryid="${sub.SubCategoryId}">
                    </div>
                </div>
            `;
        });

        html += `
                </div>
            </div>
        `;
    });

    $('#notesContainer').html(html);

    $('#notesContainer .note-time-value').each(function () {
        fillTimeDropdown(this, 5, false);
    });
}

$(document).on('click', '.note-category-header', function () {
    const body = $(this).next('.note-category-body');
    body.slideToggle(100);
    $(this).find('.rotate-icon').toggleClass('open');
});


$("#BookingReferenceFile").on("change", function () {
    const file = this.files[0];
    if (!file) {
        $("#BookingReferenceFile_Base64").val("");
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        const base64WithMime = e.target.result;

        // Remove MIME type
        // Example:
        // data:application/pdf;base64,JVBERi0x...
        // Result:
        // JVBERi0x...
        const base64 = base64WithMime.split(",")[1];
        $("#BookingReferenceFile_Base64").val(base64);
    };
    reader.readAsDataURL(file);
});

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
        TotalCharges: parseFloat($row.find(".foodPackage_TotalValue").text().replace(/₹|,/g, "").trim()) || 0,
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

function saveBooking() {
    console.log("saveBooking called");

    if (!validateBookingForm()) {
        return;
    }

    //const form = $("#bookingForm");
    const $label = $("#label_date_shift");

    userId = Number(sessionStorage.getItem('userId')) || 0;

    var customerNote = $("#OtherCharges_customerNote").val().trim();
    var internalNote = $("#OtherCharges_internalNote").val().trim();

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
        OtherCharges_ElectricityBill_Amount: parseFloat($("#OtherCharges_ElectricityBill_Amount").val() || 0),
        OtherCharges_ElectricityBill_GST: parseFloat($("#OtherCharges_ElectricityBill_GST").val() || 0),

        OtherCharges_GeneratorCharge_Amount: parseFloat($("#OtherCharges_GeneratorCharge_Amount").val() || 0),
        OtherCharges_GeneratorCharge_GST: parseFloat($("#OtherCharges_GeneratorCharge_GST").val() || 0),

        OtherCharges_Amount: parseFloat($("#OtherCharges_Amount").val() || 0),
        OtherCharges_GST: parseFloat($("#OtherCharges_GST").val() || 0),

        OtherCharges_BookingNoteForCustomer: customerNote,
        OtherCharges_InternalOfficeNote: internalNote,

        // ✅ Payment Summary
        PaymentSummary_TaxableAmount: parseFloat($("#paymentSummary_TaxableAmount").text().replace(/₹|,/g, "").trim()) || 0,
        PaymentSummary_CGST: parseFloat($("#paymentSummary_CGST").text().replace(/₹|,/g, "").trim()) || 0,
        PaymentSummary_SGST: parseFloat($("#paymentSummary_SGST").text().replace(/₹|,/g, "").trim()) || 0,
        PaymentSummary_IGST: parseFloat($("#paymentSummary_IGST").text().replace(/₹|,/g, "").trim()) || 0,
        PaymentSummary_Total: parseFloat($("#paymentSummary_Total").text().replace(/₹|,/g, "").trim()) || 0,

        PaymentSummary_BookingDiscount_Type_Id: parseInt($("#paymentSummary_BookingDiscount_Type").val() || 0),
        PaymentSummary_BookingDiscount_Type_Amount: parseFloat($("#paymentSummary_BookingDiscount_Amount").val() || 0),

        PaymentSummary_BookingDiscount_Total: parseFloat($("#paymentSummary_BookingAmount").text().replace(/₹|,/g, "").trim()) || 0,
        PaymentSummary_PayableAmount: parseFloat($("#paymentSummary_payableAmount").text().replace(/₹|,/g, "").trim()) || 0,

        ReferenceImage: $("#BookingReferenceFile_Base64").val(),

        ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
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

    //NOTES
    let notes = [];
    $('.note-subcategory').each(function () {
        const categoryId = $(this).data('categoryid');
        const subCategoryId = $(this).data('subcategoryid');
        const remarks = $(this).find('.note-remarks').val()?.trim() || '';

        //------------------------------------
        // Selected Options
        //------------------------------------
        let selectedOptions = [];
        $(this).find('.note-option:checked').each(function () {
            selectedOptions.push({
                OptionId: $(this).data('optionid')
            });
        });
        //------------------------------------
        // Dropdown
        //------------------------------------
        let dropdownValue = $(this).find('.note-dropdown').val();
        if (dropdownValue) {
            selectedOptions.push({
                OptionId: dropdownValue
            });
        }

        //------------------------------------
        // Text
        //------------------------------------
        let value = '';

        // Text / Number / Date
        const textValue = $(this).find('.note-text-value').val();
        if (textValue !== undefined && textValue !== null && textValue !== '') {
            value = textValue.toString().trim();
        }

        // Time
        const timeValue = $(this).find('.note-time-value').val();
        if (timeValue) {
            value = timeValue;
        }

        // Boolean
        const booleanControl = $(this).find('.note-boolean');
        if (booleanControl.length > 0) {
            value = booleanControl.is(':checked') ? 'true' : '';
        }

        // Skip if nothing entered
        if (remarks === '' && value === '' && selectedOptions.length === 0) {
            return;
        }

        notes.push({
            CategoryId: categoryId,
            SubCategoryId: subCategoryId,
            Value: value,
            Remarks: remarks,
            Options: selectedOptions
        });

    });
    bookingData.JSON_Notes = notes;

    const guests = [];
    $("#bookingGuestContainer .guest-row").each(function () {
        guests.push({
            Id: 0,
            Name: $(this).find(".guest-name").val(),
            MobileNo: $(this).find(".guest-mobile").val(),
            WhatsappContact: $(this).find(".guest-whatsapp").val(),
            Email: $(this).find(".guest-email").val(),
            Address: $(this).find(".guest-address").val(),
            CityId: parseInt($(this).find(".guest-city").val()) || 0,
            Dob: $(this).find(".guest-dob").val() || null,
            AnniversaryDate: $(this).find(".guest-anniversary").val() || null
        });
    });
    //console.log(JSON.stringify(guests));
    bookingData.JSON_Guests = guests;


    //console.log("Final Booking JSON:", bookingData);

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

                handleApiSuccess();
                resetBooking();
                loadEnquiryList();
                $("#bookingModal").modal("hide");
            }
            else {
                Swal.fire({
                    title: "Booking Save Failed",
                    text: response.message,
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
function setGSTSwitch(isGSTAccepted) {
    const gstSwitch = document.getElementById("gstSwitch");
    const gstText = document.getElementById("gstSwitchText");

    gstSwitch.checked = !!isGSTAccepted;

    if (gstSwitch.checked) {
        gstText.textContent = "On";
        gstText.classList.remove("text-danger");
        gstText.classList.add("text-success");
    }
    else {
        gstText.textContent = "Off";
        gstText.classList.remove("text-success");
        gstText.classList.add("text-danger");
    }
}


//Enquiry Client - dropdown -- START ----------------------
function loadEnquiryList() {
    //showLoader();
    $.ajax({
        url: "/api/EnquiryDeal/GetEnquiryDealListForDropdown",
        type: "GET",
        success: function (response) {
            //console.log("enquiry list response:", response);
            if (response.status === "Success") {
                let options = `<option value="0">-- Select Client --</option>`; // default option
                //options += `<option value="0">+ Add New Client</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `
                               <option
                                   value="${item.ClientEnquiryId}"
                                   data-name="${item.ClientName || ''}"
                                   data-email="${item.ClientEmail || ''}"
                                   data-address="${item.ClientAddress || ''}"
                                   data-contact="${item.ClientContact || ''}"
                                   data-gstin="${item.ClientGSTIN || ''}"
                                   data-stateid="${item.ClientStateId || ''}"
                               >${item.ClientName} (${item.ClientContact})</option>`;
                });
                $("#clientEnquiryDropdown, #quotation_clientId").html(options);

                $("#clientEnquiryDropdown").on("change", function () {
                    const selected = $(this).find(":selected");

                    $("#clientName").val(selected.data("name") || "");
                    $("#clientEmail").val(selected.data("email") || "");
                    $("#clientAddress").val(selected.data("address") || "");
                    $("#clientContact").val(selected.data("contact") || "");
                }); 

                $("#quotation_clientId").on("change", function () {
                    const selected = $(this).find(":selected");

                    $("#quotation_clientName").val(selected.data("name") || "");
                    $("#quotation_clientEmail").val(selected.data("email") || "");
                    $("#quotation_clientAddress").val(selected.data("address") || "");
                    $("#quotation_clientContact").val(selected.data("contact") || "");
                }); 
            }
            else {
                $("#clientEnquiryDropdown, #quotation_clientId").html(`<option value="0">No Enquiry Client Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load enquiry list');
        }
    });
}

function loadEnquiryList2() {
    //showLoader();
    $.ajax({
        url: "/api/EnquiryDeal/GetEnquiryDealListForDropdown",
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0 }), // Get all halls
        success: function (response) {
            //console.log("enquiry list response:", response);
            if (response.status === "Success") {
                let options = `<option value="0">-- Select Client --</option>`; // default option
                //options += `<option value="0">+ Add New Client</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `
                               <option
                                   value="${item.Id}"
                                   data-name="${item.Name || ''}"
                                   data-email="${item.Email || ''}"
                                   data-address="${item.Address || ''}"
                                   data-contact="${item.Contact || ''}"
                                   data-gstin="${item.GSTIN || ''}"
                                   data-stateid="${item.StateId || ''}"
                               >${item.Name} (${item.Contact})</option>`;
                });
                $("#clientEnquiryDropdown, #quotation_clientId").html(options);

                $("#clientEnquiryDropdown").on("change", function () {
                    const selected = $(this).find(":selected");

                    $("#clientName").val(selected.data("name") || "");
                    $("#clientEmail").val(selected.data("email") || "");
                    $("#clientAddress").val(selected.data("address") || "");
                    $("#clientContact").val(selected.data("contact") || "");
                }); 

                $("#quotation_clientId").on("change", function () {
                    const selected = $(this).find(":selected");

                    $("#quotation_clientName").val(selected.data("name") || "");
                    $("#quotation_clientEmail").val(selected.data("email") || "");
                    $("#quotation_clientAddress").val(selected.data("address") || "");
                    $("#quotation_clientContact").val(selected.data("contact") || "");
                }); 
            }
            else {
                $("#clientEnquiryDropdown, #quotation_clientId").html(`<option value="0">No Enquiry Client Found</option>`);
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
                $("#clientStateDropdown, #quotation_clientStateId").html(options);
                // Set default selection if found
                if (gujaratId > 0) {
                    $("#clientStateDropdown, #quotation_clientStateId").val(gujaratId);
                }
            } else {
                $("#clientStateDropdown, #quotation_clientStateId").html(`<option value="0">No State Found</option>`);
            }
        }, 
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load state list');
        }
    });
}
//State - dropdown -- END ------------------------

// food Package Dropdown -------------------------------------------
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

    $.ajax({
        url: "/api/BookingCalendar/GetPackage_MenuAndFoodItemsList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Package_Id: selectedPackageId }),
        success: function (response) {
            if (response.status === "Success" && response.data.length > 0) {
                renderMenuItems(response.data);
                $("#menuModal .modal-title").text(`Menu Items [${selectedText}]`);
                //$("#bookingModal").modal("hide");
                $("#menuModal").modal("show");
                // Store dropdown reference safely
                $("#menuModal").data("activeDropdown", $dropdown);

                // 🔥 RESTORE SELECTION
                const savedItems = $dropdown.data("selectedItems");
                //console.log("savedItems:", savedItems);
                restoreSavedMenuSelection(savedItems);
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
        const section = $(`#menuModal .menu-section[data-menu-id='${item.FoodMenu_Id}']`);
        if (!section.length) return;

        // 🔹 EXISTING ITEM
        if (item.FoodItem_Id && item.FoodItem_Id !== 0) {
            section.find(`.item-checkbox[value='${item.FoodItem_Id}']`).prop("checked", true);
        }

        // 🔹 NEW CUSTOM ITEM (ID = 0)
        else if (item.FoodItem_Id === 0 && item.FoodItem_Name_NewAdded) {
            const newItemHtml = `
                <div class="form-check me-3 mb-2">
                    <input class="form-check-input item-checkbox new-item"
                           type="checkbox"
                           value="0"
                           data-menu-id="${item.FoodMenu_Id}"
                           data-item-name="${item.FoodItem_Name_NewAdded}"
                           checked>
                    <label class="form-check-label">${item.FoodItem_Name_NewAdded}</label>
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

$(document).on("input change", "#paymentSummary_BookingDiscount_Type, #paymentSummary_BookingDiscount_Amount",
    function () {
        recalculatePaymentSummary();
    });

$(document).on("input", "#OtherCharges_Decoration_Amount, #OtherCharges_RoomCharges_Amount, #OtherCharges_SoundSystem_Amount, #OtherCharges_ProjectorScreen_Amount, #OtherCharges_ElectricityBill_Amount, #OtherCharges_GeneratorCharge_Amount, #OtherCharges_Amount",
    function () {
        recalculateAllOtherCharges();
        recalculatePaymentSummary();
    });

function recalculateAllOtherCharges() {
    calculateOtherCharge("#OtherCharges_Decoration_Amount", "#OtherCharges_Decoration_GST");
    calculateOtherCharge("#OtherCharges_RoomCharges_Amount", "#OtherCharges_RoomCharges_GST");
    calculateOtherCharge("#OtherCharges_SoundSystem_Amount", "#OtherCharges_SoundSystem_GST");
    calculateOtherCharge("#OtherCharges_ProjectorScreen_Amount", "#OtherCharges_ProjectorScreen_GST");
    calculateOtherCharge("#OtherCharges_ElectricityBill_Amount", "#OtherCharges_ElectricityBill_GST");
    calculateOtherCharge("#OtherCharges_GeneratorCharge_Amount", "#OtherCharges_GeneratorCharge_GST");
    calculateOtherCharge("#OtherCharges_Amount", "#OtherCharges_GST");
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
                <div class="d-flex flex-wrap item-container">${foodItems}</div>
                <div class="d-flex mt-2">
                    <input type="text" class="form-control form-control-sm me-2 new-item-input" placeholder="Add New Item">
                    <button type="button" class="btn btn-sm btn-primary btn-addNewItem">Add</button>
                </div>
            </div>`;
    });

    $("#menuModalBody").html(html);
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
    const menuId = $(this).attr("id").split("_")[1];
    const checked = $(this).is(":checked");
    $(`.item-checkbox[data-menu-id='${menuId}']`).prop("checked", checked);
});

//StEP 6: Save MENU
$(document).on("click", ".btn-saveMenu", function () {
    let selectedMenuData = [];

    $(".menu-section").each(function () {
        const menuId = $(this).data("menu-id");
        $(this).find(".item-checkbox:checked").each(function () {
            let isNew = $(this).hasClass("new-item");
            selectedMenuData.push({
                FoodMenu_Id: parseInt(menuId),
                FoodItem_Id: parseInt($(this).val()),
                FoodItem_Name_NewAdded: isNew ? $(this).data("item-name") : null,
                IsChecked: $(this).is(":checked")
            });
        });
    });

    // attach selected items to the dropdown that opened modal
    const $activeDropdown = $("#menuModal").data("activeDropdown");
    if ($activeDropdown) {
        $activeDropdown.data("selectedItems", selectedMenuData);
        $activeDropdown.attr("data-selected-items", JSON.stringify(selectedMenuData));
    }

    //console.log("Saved Menu Selection:", selectedMenuData);
    $("#menuModal").modal("hide");
    //$("#bookingModal").modal("show");
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
            handleApiError(xhr, status, error, 'Failed to load addon charges');
        }
    });
}

function populateAddonModal(savedItems = []) {
    //console.log("savedItems AddOns:", savedItems);
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
                    <div class="card-header d-flex justify-content-between align-items-center bg-light py-2">
                        <span class="fw-bold">${header}</span>
                        <button class="btn btn-xs btn-outline-secondary toggle-btn">−</button>
                    </div>
                    <div class="card-body p-2" data-header-body="${header}"></div>
                </div>
            `);

        // Column headers
        let columnHeader = $(`
                <div class="d-flex fw-bold text-secondary border-bottom pb-1 mb-2">
                    <div class="flex-grow-1">Food Item</div>
                    <div style="width:90px;" class="text-center">Rate</div>
                    <div style="width:90px;" class="text-center">Pax</div>
                    <div style="width:90px;" class="text-center">Total</div>
                </div>
            `);
        headerCard.find(`[data-header-body="${header}"]`).append(columnHeader);

        // Populate items
        grouped[header].forEach(item => {
            // Find if this item was saved previously
            let saved = savedItems.find(s => s.AddOn_Item_Id === item.Id && s.AddOn_Header_Id === item.HeaderId);
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

                        <input type="number" class="form-control form-control-sm rate-input text-center mx-1" step="0.01"
                               value="${rate}" min="0" style="width:90px;">

                        <input type="number" class="form-control form-control-sm pax-input text-center mx-1" step="0.01"
                               value="${pax}" min="0" style="width:90px;">

                        <input type="number" class="form-control form-control-sm total-input text-center mx-1 bg-light"
                               value="${total.toFixed(2)}" readonly style="width:90px;">
                    </div>
                `);

            row.find(".rate-input, .pax-input").on("input", function () {
                //console.log("here1");
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
                <div class="d-flex justify-content-between mb-1 fs-9" data-header-summary="${header}">
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
    //$("#bookingModal").modal("hide");
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

    //console.log("✅ Selected Addon Items:", selectedItems);
    //console.log("💰 Grand Total:", grandTotal.toFixed(2));

    // 🔹 Update the clicked AddOn button’s total span
    if (currentAddonButton) {
        //console.log("here addon btn 1");
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
    //$("#bookingModal").modal("show");

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

var otherCharges_internalEditor = new Quill('#OtherCharges_internalEditor', {
    theme: 'snow',
    modules: { toolbar: toolbarOptions }
});
//< !--Booking Note - text Editor-- - END---->

$("#otherCharges_Reset").on("click", function () {
    // Reset all number inputs inside Other Charges section
    $("#otherCharges").find("input[type='number']").val(0);
    recalculatePaymentSummary();
});


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
$("#paymentSummary_BookingAmount_Type").on("change", function () {
    $("#paymentSummary_BookingAmount_Amount").val(0);
    //updateBookingDiscount();
    recalculatePaymentSummary();
});

$("#paymentSummary_BookingAmount_Amount").on("input", function () {
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
            console.log("payment mode res:", res);
            const ddl = $("#PaymentDetails_Mode");
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

loadAccountDropdown();
function loadAccountDropdown() {

    var payload = {
        Id: 0,
        ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
    };
    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("GetAccount res:", res);

            const ddl = $("#PaymentDetails_AccountId");
            ddl.empty().append('<option value="0">--Select--</option>');

            if (res?.status === "Success" && Array.isArray(res.data)) {
                res.data.forEach(acc => {

                    const accountName = (acc.AccountName || "").trim();
                    //const holderName = ""; //  (acc.AccountHolderName || "").trim();
                    //let holderDisplay = "";

                    //if (holderName && accountName.toLowerCase() !== holderName.toLowerCase()) {
                    //    holderDisplay = ` (${holderName})`;
                    //}

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


//BOOKING CALENDAR DESIGN - END ----------------------------------------------------------------------

//FOLLOW UP REMAIDNER - START --------------------------------------------------------------------
function loadFollowUpReminder(eventId, enquiryId) {
    var payload = {
        enquiryId: enquiryId
    };

    $.ajax({
        url: "/api/EnquiryFollowup/GetFollowUpReminderStatus",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            if (response.status !== "Success")
                return;

            const status = response.data.Status;
            const history = response.data.History;
            bindReminderSwitches(status);
            bindReminderHistory(history);
        },
        error: function (status, error, api) {
            handleApiError(status, error, api, "Unable to load reminder data.");
        }
    });
}

function bindReminderSwitches(status) {
    $('#switchReminder1')
        .prop('checked', status.Is1stSent)
        .prop('disabled', !status.Enable1st);

    $('#switchReminder2')
        .prop('checked', status.Is2ndSent)
        .prop('disabled', !status.Enable2nd);

    $('#switchReminder3')
        .prop('checked', status.Is3rdSent)
        .prop('disabled', !status.Enable3rd);
}

function bindReminderHistory(history) {
    let tbody = $('#reminderHistoryBody');
    tbody.empty();
    if (!history || history.length === 0) {
        $('#reminderHistoryContainer').hide();
        return;
    }

    $('#reminderHistoryContainer').show();
    $.each(history, function (index, item) {
        let reminderText = '';
        switch (item.FollowUpStage) {
            case 1: reminderText = '1st Reminder'; break;
            case 2: reminderText = '2nd Reminder'; break;
            case 3: reminderText = '3rd Reminder'; break;
        }
        tbody.append(`
            <tr>
                <td>${index + 1}</td>
                <td>${reminderText}</td>
                <td>${item.LastModifiedOn}</td>
                <td>${item.LastModifiedBy}</td>
            </tr>
        `);
    });
}

$(document).on('change', '.reminderToggle', function () {
    const $switch = $(this);
    if (!$switch.is(':checked'))
    {
        $switch.prop('checked', true);
        return;
    }

    let followUpNo = Number($switch.data('followup'));
    let actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    let payload = {
        enquiryId: Number($('#hfEnquiryDealId').val()),
        followUpNo: followUpNo,
        actionBy: actionBy
    };

    $switch.prop('disabled', true);

    $.ajax({
        url: "api/EnquiryFollowup/SaveFollowUpReminder",
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        data: JSON.stringify(payload),

        success: function (response) {
            console.log("response:", response);
            if (response.status !== "Success") {
                $switch.prop('checked', false);
                $switch.prop('disabled', false);

                Swal.fire({
                    icon: 'error',
                    title: 'Failed',
                    text: response.message || 'Reminder sending failed.'
                });
                return;
            }

            let whatsappStatus = response.data?.whatsappStatus || "Success";
            if (whatsappStatus !== "Success") {
                $switch.prop('checked', false);
                $switch.prop('disabled', false);

                Swal.fire({
                    icon: 'error',
                    title: 'Failed',
                    text: response.message || 'WhatsApp sending failed.'
                });
                return;
            }

            handleApiSuccess(`Follow-up Reminder ${followUpNo} sent successfully.`);
            loadFollowUpReminder(Number($('#hfEnquiryDealId').val()));
        },
        error: function (xhr, status, error) {
            $switch.prop('checked', false);
            $switch.prop('disabled', false);

            handleApiError(
                status,
                error,
                xhr,
                "Unable to save reminder."
            );
        }
    });
});

//FOLLOW UP REMAIDNER - END ----------------------------------------------------------------------

