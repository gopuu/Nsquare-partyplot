$(document).ready(function () {
    fetchData();
    fetchHallDetails();
    loadSlotDropdown();
    loadEnquiryList();
    loadStateList();
    loadSubEventList();
    loadLeadSourceList();
    loadFoodPackageList();

    AppSignalR.subscribeQuotation((data) => {
        console.log("Quotation Page Update", data);
        fetchData();
        //$("#modal").modal("hide");
    });

    // ➤ ADD BUTTON CLICK
    $("#btn-add").click(function () {
        const canAdd = (sessionStorage.getItem("CanAdd") === "true");
        if (!canAdd) return Swal.fire("Access Denied", "You cannot Add.", "error");

        resetForm();
        $("#quotationModal .modal-title").text("Add Quotation");
        $("#quotationModal").modal("show");

    });

    // ➤ SAVE
    $("#btnSaveQuotation").click(function () {

        let payload = {

            // ===== Basic =====
            Id: parseInt($("#quotationId").val()) || 0,
            // ===== Booking Info =====
            EnquiryDealId: parseInt($("#enquiryDealId").val()) || 0,

            BanquetId: parseInt($("#banquetId").val()) || 0,
            SlotId: parseInt($("#slotId").val()) || 0,

            FromDate: $("#fromDate").val() || "",
            TillDate: $("#tillDate").val() || "",
            StartTime: $("#startTime").val() || null,
            EndTime: $("#endTime").val() || null,

            // ===== Client Section =====
            ClientId: parseInt($("#clientId").val()) || 0,
            ClientName: $("#clientName").val()?.trim() || "",

            Address: $("#clientAddress").val() || null,
            Email: $("#clientEmail").val() || null,
            ContactNo: $("#clientContact").val() || null,
            GSTIN: $("#clientGstIn").val() || null,
            StateId: $("#clientStateId").val() ? parseInt($("#clientStateId").val()) : null,

            // ===== Booking Details =====
            EventSubTypeId: $("#eventSubTypeId").val() || null,
            BanquetRent: parseFloat($("#banquetRent").val()) || 0,
            Pax: parseInt($("#pax").val()) || 0,
            ReferenceTypeId: $("#referenceTypeId").val() || null,

            // ===== Charges =====
            DecorationChargesAmount: parseFloat($("#decorationAmount").val()) || 0,
            SoundSystemChargesAmount: parseFloat($("#soundAmount").val()) || 0,
            ProjectorChargesAmount: parseFloat($("#projectorAmount").val()) || 0,

            // ===== Print Options =====
            IsPrintPriceExclusive: $("#isPrintExclusive").is(":checked"),
            IsPrintFoodItems: $("#isPrintFoodItems").is(":checked"),

            // ===== Totals =====
            SubTotal: parseFloat($("#subTotal").val()) || 0,
            TaxAmount: parseFloat($("#taxAmount").val()) || 0,
            GrandTotal: parseFloat($("#grandTotal").val()) || 0,

            // ===== Notes (if added later) =====
            Notes: null, // Add #notes field if needed

            // ===== JSON Child Data =====
            PackagesJson: getPackagesJson(),
            OtherServicesJson: getOtherServicesJson(),

            // ===== Audit =====
            ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
        };
        $("#btnSaveQuotation").prop("disabled", true);
        console.log("data save:", payload);

        $.ajax({
            url: "/api/Quotation/SaveQuotation",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess();
                    $("#quotationModal").modal("hide");
                    //fetchData();
                } else {
                    Swal.fire("Error", res.message, "error");
                }
            },
            error: function(xhr, status, error) {
                handleApiError(xhr, status, error, "Save Failed");
            },
            complete: function () {
                $("#btnSaveQuotation").prop("disabled", false)
            }
        });
    });

    // ➤ EDIT BUTTON
    $(document).on("click", ".btn-edit", function () {

        const canEdit = (sessionStorage.getItem("CanEdit") === "true");
        if (!canEdit)
            return Swal.fire("Access Denied", "You cannot Edit.", "error");

        let table = $('#tableMain table').DataTable();
        let row = table.row($(this).closest('tr')).data();

        if (!row || !row.Id) return;

        fetchData(row.Id, "edit");
    });

    // ➤ DELETE BUTTON
    $(document).on("click", ".btn-delete", function () {

        const canDelete = (sessionStorage.getItem("CanDelete") === "true");
        if (!canDelete) return Swal.fire("Access Denied", "You cannot Delete.", "error");

        let row = $("#tableMain table").DataTable().row($(this).closest("tr")).data();

        $("#deleteRecordModal").data("id", row.Id).modal("show");
    });

    // ➤ CONFIRM DELETE
    $("#delete-record").click(function () {

        let id = $("#deleteRecordModal").data("id");

        const payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/Quotation/DeleteQuotation",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {

                if (res.status === "Success") {
                    handleApiSuccess("Deleted!");
                } else {
                    Swal.fire("Error", res.message, "error");
                }

                $("#deleteRecordModal").modal("hide");
                fetchData();
            },
            error: function(xhr, status, error){
                handleApiError(xhr, status, error, "Delete Failed");
            },
        });
    });


    //SEND
    $(document).on("click", ".btn-send", function () {
        let row = $("#tableMain table").DataTable().row($(this).closest("tr")).data();
        console.log("Send quotation:", row.Id);
    });

    //DOWNLOAD PDF
    $(document).on("click", ".btn-download", function () {
        let row = $("#tableMain table").DataTable().row($(this).closest("tr")).data();
        console.log("Download PDF:", row.Id);
    });

    //BOOK
    $(document).on("click", ".btn-book", function () {
        let row = $("#tableMain table").DataTable().row($(this).closest("tr")).data();
        console.log("Book quotation:", row.Id);
    });

    // Add Service Row
    $("#btnAddService").click(function () {
        let row = `
        <div class="row g-1 service-row mb-1 align-items-center">
            <div class="col-md-3">
                <input type="text" class="form-control service-name" placeholder="Service" />
            </div>
            <div class="col-md-2">
                <input type="number" class="form-control service-amount" placeholder="Amount" value="0" />
            </div>
            <div class="col-md-2">
                <input type="number" class="form-control service-qty" value="0" />
            </div>
            <div class="col-md-2">
                <input type="number" class="form-control service-total bg-light" value="0" readonly />
            </div>
            <div class="col-md-2">
                <textarea class="form-control service-desc" placeholder="Description"></textarea>
            </div>
            <div class="col-md-1 text-center">
                <button type="button" class="btn btn-sm btn-danger btn-remove-service">
                    <i class="fa fa-trash"></i>
                </button>
            </div>
        </div>`;

        $("#otherServicesContainer").append(row);
    });

    // Remove Service Row (IMPORTANT: use event delegation)
    $(document).on("click", ".btn-remove-service", function () {
        $(this).closest(".service-row").remove();
    });

    //Auto calc
    $(document).on("input", ".service-amount, .service-qty", function () {
        let row = $(this).closest(".service-row");

        let amount = parseFloat(row.find(".service-amount").val()) || 0;
        let qty = parseInt(row.find(".service-qty").val()) || 0;

        let total = amount * qty;

        row.find(".service-total").val(total.toFixed(2));

        calculateTotals();
    });

    $(document).on("change input", "#pax, .package-checkbox", function () {
        calculateTotals();
    });

    $("#decorationAmount, #soundAmount, #projectorAmount, #banquetRent").on("input", function () {
        calculateTotals();
    });

    // On Slot Change
    $(document).on("change", "#slotId", function () {
        const selectedOption = $(this).find("option:selected");

        const startTime = selectedOption.data("start");
        const endTime = selectedOption.data("end");

        if ($(this).val() != "0") {
            $("#startTime").val(startTime || "");
            $("#endTime").val(endTime || "");
        } else {
            $("#startTime").val("");
            $("#endTime").val("");
        }
    });

});

function calculateTotals() {

    let pax = parseInt($("#pax").val()) || 0;

    let packageTotal = 0;

    // Calculate selected packages
    $(".package-checkbox:checked").each(function () {
        let rate = parseFloat($(this).data("rate")) || 0;
        packageTotal += rate * pax;
    });

    // Calculate other services
    let serviceTotal = 0;
    $(".service-total").each(function () {
        serviceTotal += parseFloat($(this).val()) || 0;
    });

    // Other charges
    let decoration = parseFloat($("#decorationAmount").val()) || 0;
    let sound = parseFloat($("#soundAmount").val()) || 0;
    let projector = parseFloat($("#projectorAmount").val()) || 0;
    let banquetRent = parseFloat($("#banquetRent").val()) || 0;

    let subTotal = packageTotal + serviceTotal + decoration + sound + projector + banquetRent;

    let tax = 0; // subTotal * 0.18; // 18% GST

    let netTotal = subTotal + tax;

    $("#subTotal").val(subTotal.toFixed(2));
    $("#taxAmount").val(tax.toFixed(2));
    $("#grandTotal").val(netTotal.toFixed(2));
}

function fetchData(id=0, mode="add") {

    $.ajax({
        url: "/api/Quotation/GetQuotation",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: id }),
        success: function (response) {

            if (!response || response.status !== "Success")
                return Swal.fire("Error", "Failed to load quotation", "error");

            let data = response?.data || [];
            console.log("data:", data);

            if (mode === "edit") {
                let q = response.data[0];

                resetForm();

                // ===== Basic =====
                $("#quotationId").val(q.Id);
                $("#enquiryDealId").val(q.EnquiryDealId);

                // ===== Booking Section =====
                $("#banquetId").val(q.BanquetId);
                $("#slotId").val(q.SlotId);

                $("#fromDate").val(q.FromDate ? q.FromDate.split("T")[0] : "");
                $("#tillDate").val(q.TillDate ? q.TillDate.split("T")[0] : "");

                $("#startTime").val(q.StartTime || "");
                $("#endTime").val(q.EndTime || "");

                // ===== Client Section =====
                loadEnquiryList(q.ClientId);
                $("#clientName").val(q.ClientName || "");
                $("#clientAddress").val(q.Address || "");
                $("#clientEmail").val(q.Email || "");
                $("#clientContact").val(q.ContactNo || "");
                $("#clientGstIn").val(q.GSTIN || "");
                $("#clientStateId").val(q.StateId || "0");

                // ===== Booking Details =====
                $("#eventSubTypeId").val(q.EventSubTypeId || "0");
                $("#banquetRent").val(q.BanquetRent || 0);
                $("#pax").val(q.Pax || 0);
                $("#referenceTypeId").val(q.ReferenceTypeId || "0");

                // ===== Charges =====
                $("#decorationAmount").val(q.DecorationChargesAmount || 0);
                $("#soundAmount").val(q.SoundSystemChargesAmount || 0);
                $("#projectorAmount").val(q.ProjectorChargesAmount || 0);

                // ===== Print Flags =====
                $("#isPrintExclusive").prop("checked", q.IsPrintPriceExclusive);
                $("#isPrintFoodItems").prop("checked", q.IsPrintFoodItems);

                // ===== Totals =====
                $("#subTotal").val(q.SubTotal || 0);
                $("#taxAmount").val(q.TaxAmount || 0);
                $("#grandTotal").val(q.GrandTotal || 0);

                // ===== Load Packages =====
                if (q.Packages_JSON) {
                    let packages = q.Packages_JSON;
                    $(".package-checkbox").prop("checked", false);

                    packages.forEach(p => {
                        $(".package-checkbox[value='" + p.PackageId + "']").prop("checked", true);
                    });
                }

                // ===== Load Other Services =====
                if (q.OtherServices_JSON) {
                    let services = q.OtherServices_JSON;

                    $("#otherServicesContainer").html("");

                    services.forEach(s => {

                        $("#otherServicesContainer").append(`
                        <div class="row g-2 service-row mb-2">
                            <div class="col-md-3">
                                <input type="text" class="form-control form-control-sm service-name"
                                       value="${s.ServiceName || ""}" />
                            </div>
                            <div class="col-md-2">
                                <input type="number" class="form-control form-control-sm service-amount"
                                       value="${s.Amount || 0}" />
                            </div>
                            <div class="col-md-2">
                                <input type="number" class="form-control form-control-sm service-qty"
                                       value="${s.Quantity || 1}" />
                            </div>
                            <div class="col-md-2">
                                <input type="number" class="form-control form-control-sm service-total bg-light"
                                       value="${s.TotalAmount || 0}" readonly />
                            </div>
                            <div class="col-md-3">
                                <textarea class="form-control form-control-sm service-desc">
                                    ${s.ServiceDescription || ""}
                                </textarea>
                            </div>
                        </div>
                    `);
                    });
                }

                //calculateTotals();

                $("#quotationModal .modal-title").text("Edit Quotation");
                $("#quotationModal").modal("show");
            }
            else {
                renderTable(data);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Quotation Fetch Failed");
        }
    });
}
function renderTable(data) {

    let tableSelector = '#tableMain table';

    // Safe destroy
    if ($.fn.DataTable && $.fn.DataTable.isDataTable(tableSelector)) {
        $(tableSelector).DataTable().clear().destroy();
    }

    data.forEach((item, index) => {
        item.srno = index + 1;
    });

    $(tableSelector).DataTable({
        data: data,
        columns: [
            { data: 'srno', title: '#' },
            { data: 'ClientName', title: 'Client' },
            { data: 'Email', title: 'Email' },
            { data: 'ContactNo', title: 'Contact' },
            { data: 'EventSubTypeName', title: 'Event' },
            {
                data: 'FromDate2',
                title: 'Event Date'
            },
            {
                data: 'QuotationDate2',
                title: 'Quotation Date'
            },
            { data: 'BanquetName', title: 'Hall' },
            { data: 'SlotName', title: 'Slot' },
            //{ data: 'BanquetRent', title: 'Rent' },
            { data: 'Pax', title: 'PAX' },
            { data: 'ReferenceTypeName', title: 'Medium' },
            {
                data: 'GrandTotal',
                title: 'Total',
                className: "text-end",
                render: function (data, type, row) {
                    if (!data) return "₹ 0.00";

                    let amount = parseFloat(data).toLocaleString('en-IN', {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    });
                    return "₹ " + amount;
                }
            },
            {
                data: null,
                title: 'Action',
                className: "text-center",
                orderable: false,
                render: function () {
                    return `
                        <div class="dropdown text-center">
                            <button class="btn btn-sm btn-light" data-bs-toggle="dropdown">
                                <i class="fa fa-ellipsis-v"></i>
                            </button>
                            <ul class="dropdown-menu">

                                <li>
                                    <a class="dropdown-item btn-edit" href="javascript:void(0)">
                                        <i class="fa fa-edit me-2 text-warning"></i> Edit
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item btn-delete" href="javascript:void(0)">
                                        <i class="fa fa-trash me-2 text-danger"></i> Delete
                                    </a>
                                </li>

                                <li><hr class="dropdown-divider"></li>

                                <li>
                                    <a class="dropdown-item btn-send" href="javascript:void(0)">
                                        <i class="fa fa-paper-plane me-2 text-primary"></i> Send (Coming Soon)
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item btn-download" href="javascript:void(0)">
                                        <i class="fa fa-file-pdf me-2 text-danger"></i> Download (Coming Soon)
                                    </a>
                                </li>

                                <li>
                                    <a class="dropdown-item btn-book" href="javascript:void(0)">
                                        <i class="fa fa-check-circle me-2 text-success"></i> Book (Coming Soon)
                                    </a>
                                </li>

                            </ul>
                        </div>`;
                }
            }
        ],
        language: {
            emptyTable: "No Data Found !!"
        },
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },

    });

}

function resetForm() {

    // ===== Basic =====
    $("#quotationId").val(0);
    $("#enquiryDealId").val(0);

    // ===== Banquet / Slot / Date =====
    $("#banquetId").val("0");
    $("#slotId").val("0");
    $("#fromDate").val("");
    $("#tillDate").val("");

    $("#startTime").val("");
    $("#endTime").val("");

    // ===== Client Section =====
    $("#clientId").val(0).trigger("change");
    $("#clientName").val("");
    $("#clientAddress").val("");
    $("#clientEmail").val("");
    $("#clientContact").val("");
    $("#clientGstIn").val("");
    $("#clientStateId").val("0");

    // ===== Booking Details =====
    $("#eventSubTypeId").val("0");
    $("#banquetRent").val(0);
    $("#pax").val(0);
    $("#referenceTypeId").val("0");

    // ===== Charges =====
    $("#decorationAmount").val(0);
    $("#soundAmount").val(0);
    $("#projectorAmount").val(0);

    // ===== Print Flags =====
    $("#isPrintExclusive").prop("checked", false);
    $("#isPrintFoodItems").prop("checked", false);

    // ===== Totals =====
    $("#subTotal").val(0);
    $("#taxAmount").val(0);
    $("#grandTotal").val(0);

    // ===== Packages =====
    $(".package-checkbox").prop("checked", false);

    // ===== Other Services Reset =====
    $("#otherServicesContainer").html(`
        <div class="row g-1 service-row mb-1">
            <div class="col-md-3">
                <label class="form-label compact-label">Service</label>
                <input type="text" class="form-control form-control-sm service-name p-1" placeholder="Service" />
            </div>
            <div class="col-md-2">
                <label class="form-label compact-label">Amount</label>
                <input type="number" class="form-control form-control-sm service-amount p-1" value="0" />
            </div>
            <div class="col-md-2">
                <label class="form-label compact-label">Qty</label>
                <input type="number" class="form-control form-control-sm service-qty p-1" value="0" />
            </div>
            <div class="col-md-2">
                <label class="form-label compact-label">Total</label>
                <input type="number" class="form-control form-control-sm service-total bg-light p-1" value="0" readonly />
            </div>
            <div class="col-md-3">
                <label class="form-label compact-label">Description</label>
                <textarea class="form-control form-control-sm service-desc p-1" placeholder="Description"></textarea>
            </div>
        </div>
    `);

    // Optional: recalc to ensure totals consistent
    calculateTotals();
}
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
//dropdown -- START ----------------------
function fetchHallDetails(banquetId = 0) {
    $.ajax({
        url: '/api/Company_HallDetails/GetCompanyHallDetailsList_Dropdown',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: banquetId }), // Get all halls
        success: function (response) {
            //console.log("Get Hall res:", response);
            if (response.status === "Success" && response.data && response.data.length > 0) {
                let options = `<option value="0">-- Select --</option>`; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.HallName} (${item.CapacityRangeText})</option>`;
                });
                $("#banquetId").html(options);
                // ✅ If banquetId is passed → select & STOP
                if (banquetId > 0) {
                    $("#banquetId").val(String(banquetId)).trigger("change");
                    return; // ❌ do not render board
                }
            }
            else {
                $("#banquetId").html(`<option value="0">No Hall Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to fetch hall details');
        }
    });
}
function loadSlotDropdown() {
    var payload = { Id: 0, IsActive: true };
    $.ajax({
        url: "/api/SlotMaster/GetSlotList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("response supplier:", response);
            const $slotDropdowns = $("#slotId");
            $slotDropdowns.html(`<option value="0">--Select--</option>`);
            response.data.forEach(s => {
                $slotDropdowns.append(`<option value="${s.Id}" data-start="${s.StartTime}" data-end="${s.EndTime}">${s.Name} (${s.Code})</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Slot Fetch Error");
        }
    });
}
function loadEnquiryList(selectedClientId=0) {
    $.ajax({
        url: "/api/EnquiryDeal/GetEnquiryDealListForDropdown",
        type: "GET",
        success: function (response) {
            if (response.status === "Success") {
                // 🔹 Store currently selected value (if any)
                //let selectedClientId = $("#clientId").val();

                let options = `<option value="">-- Select Client --</option>`;
                options += `<option value="0">+ Add New Client</option>`;

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

                $("#clientId").html(options);

                // Destroy Select2 if already initialized
                if ($.fn.select2 && $("#clientId").hasClass("select2-hidden-accessible")) {
                    $("#clientId").select2("destroy");
                }

                // Initialize Select2
                $("#clientId").select2({
                    dropdownParent: $("#quotationModal"),
                    placeholder: "Select Client",
                    allowClear: true,
                    width: '100%'
                });

                // 🔹 Restore selection if exists
                if (selectedClientId && selectedClientId !== "") {
                    if ($("#clientId option[value='" + selectedClientId + "']").length) {
                        $("#clientId").val(selectedClientId).trigger("change");
                    }
                }

                // Change Event
                $("#clientId").off("change").on("change", function () {

                    const selected = $(this).find(":selected");
                    const value = $(this).val();

                    if (value === "add_new") {
                        resetClientFields();
                        return;
                    }

                    $("#enquiryDealId").val(value);
                    $("#clientName").val(selected.data("name") || "");
                    $("#clientEmail").val(selected.data("email") || "");
                    $("#clientAddress").val(selected.data("address") || "");
                    $("#clientContact").val(selected.data("contact") || "");
                });

            }
            else {
                $("#clientId").html(`<option value="">No Client Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load enquiry list');
        }
    });
}
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
                $("#clientStateId").html(options);
                // Set default selection if found
                if (gujaratId > 0) {
                    $("#clientStateId").val(gujaratId);
                }
            } else {
                $("#clientStateId").html(`<option value="0">No State Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load state list');
        }
    });
}
function loadSubEventList() {
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
                $("#eventSubTypeId").html(options);
            } else {
                $("#eventSubTypeId").html(`<option value="0">No Event Found</option>`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load event list');
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
            }
            else {
                options += `<option value="0">No Reference Found</option>`;
            }

            const ddl = $("#referenceTypeId");
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

let foodPackages = [];
function loadFoodPackageList() {
    $.ajax({
        url: "/api/Company_FoodItem_Package/GetCompanyFoodPackage_Dropdown",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            if (response && response.status === "Success" && response.data?.length > 0) {

                foodPackages = response.data;
                renderFoodPackages(foodPackages);

            } else {

                $("#packageContainer").html(`
                    <div class="text-center text-danger small py-2">
                        No Package Found
                    </div>
                `);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load food packages');
        }
    });
}
function renderFoodPackages(packages) {

    if (!packages || packages.length === 0) {
        $("#packageContainer").html(`
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

    $("#packageContainer").html(html);
}
//dropdown -- END ----------------------
