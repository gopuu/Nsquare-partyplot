//<!-- TAB 1-2: Company Profile & Property Details -- START -->

let companyId = sessionStorage.getItem('companyId');
if (!companyId) {
    companyId = 0;
    sessionStorage.setItem('companyId', companyId); // default value
}
let userId = sessionStorage.getItem('userId') || 0;

// Add more booking step input fields
function addBookingStep() {
    let container = document.getElementById("PropertyDetails_bookingStepsContainer");
    let div = document.createElement("div");
    div.className = "input-group mb-2";

    let input = document.createElement("input");
    input.type = "text";
    input.className = "form-control";
    input.placeholder = "Enter step";

    let button = document.createElement("button");
    button.type = "button";
    button.className = "btn btn-danger";
    button.innerHTML = "×";
    button.onclick = function () {
        container.removeChild(div);
    };

    div.appendChild(input);
    div.appendChild(button);
    container.appendChild(div);
}

//for rules & regulations
function toggleRuleOption(option) {
    const wrapper = option.closest('.rule-wrapper');
    const options = wrapper.querySelectorAll('.rule-option');

    options.forEach(opt => {
        opt.classList.remove('selected');
        opt.querySelector('input[type="checkbox"]').checked = false;
    });

    option.classList.add('selected');
    option.querySelector('input[type="checkbox"]').checked = true;
}

//for facility
function toggleFacility(cell) {
    const checkbox = cell.querySelector('input[type="checkbox"]');
    checkbox.checked = !checkbox.checked;
    cell.classList.toggle('checked', checkbox.checked);
}

async function loadAndSetCity(stateId, cityId) {
    try {
        // First load cities for the state
        await loadCities(stateId);

        // Then set the city value
        if (cityId) {
            $("#PropertyDetails_City").val(cityId);

            // Optional: Trigger locality loading if needed
            if (typeof loadLocalities === 'function') {
                loadLocalities(cityId, 0); // Assuming 0 is some default value
            }
        }
    } catch (error) {
        console.error("Error in loadAndSetCity:", error);
    }
}
// Usage:
// loadAndSetCity(d.CompanyState, d.PropertyCity);

// 1. Get & Fill Company Profile
function getCompanyProfile() {
    $.ajax({
        url: `/api/Company_ProfileAndPropertyDetails/GetCompanyProfileAndPropertyDetailsList`,
        type: "GET",
        contentType: "application/json",
        dataType: "json", // Expecting JSON response
        //data: JSON.stringify({ Id: companyId }),
        success: function (res) {
            //console.log("GetCompanyProfileAndPropertyDetailsList() res:", res);
            if (res.status === "Success" && res.data.length > 0) {
                let d = res.data[0];
                companyId = d.Id; // Update company ID if it was 0
                sessionStorage.setItem('companyId', companyId); // default value
                getTermsAndConditions();

                // Company Profile Tab
                $("#CompanyProfile_CompanyName").val(d.CompanyName);

                let banquetType = d.BanquetType;
                if (!banquetType || banquetType === "0") {
                    $("#CompanyProfile_BanquetType option").each(function () {
                        if ($(this).text().trim() === "Banquet Hall") {
                            banquetType = $(this).val();
                        }
                    });
                }
                $("#CompanyProfile_BanquetType").val(d.BanquetType);

                //loadCities(d.CompanyState);
                $("#CompanyProfile_State").val(d.CompanyState);
                $("#CompanyProfile_City").val(d.CompanyCity);
                $("#CompanyProfile_Email").val(d.CompanyEmail);
                $("#CompanyProfile_Address").val(d.CompanyAddress);
                $("#CompanyProfile_PanNo").val(d.CompanyPanNo);
                $("#CompanyProfile_Mobile").val(d.CompanyMobile);
                $("#CompanyProfile_ReviewUrl").val(d.ReviewUrl);
                $("#CompanyProfile_AcceptGST").prop("checked", d.AcceptGST);
                $("#CompanyProfile_GSTIN").val(d.GSTIN);
                // Show/Hide GSTIN input based on checkbox
                if (d.AcceptGST) {
                    $("#gstinContainer").show();
                } else {
                    $("#gstinContainer").hide();
                }
                $("#CompanyProfile_VATNumber").val(d.VATNumber);
                $("#CompanyProfile_QuotationsWithTax").prop("checked", d.QuotationsWithTax);
                $("#CompanyProfile_MultipleFoodGST").prop("checked", d.MultipleFoodGST);
                $("#CompanyProfile_PrintCustomerDetails").prop("checked", d.PrintCustomerDetails);

                // Property Details Tab
                //loadAndSetCity(d.CompanyState, d.PropertyCity);
                $("#PropertyDetails_City").val(d.PropertyCity);
                // loadLocalities(d.PropertyCity);
                $("#PropertyDetails_Locality").val(d.Locality);
                $("#PropertyDetails_Pincode").val(d.Pincode);
                $("#PropertyDetails_ManagerName").val(d.ManagerName);
                $("#PropertyDetails_PropertyName").val(d.PropertyName);
                $("#PropertyDetails_PropertyType").val(d.PropertyType);
                $("#PropertyDetails_Email").val(d.PropertyEmail);
                $("#PropertyDetails_Address").val(d.PropertyAddress);
                $("#PropertyDetails_ManagerMobile").val(d.ManagerMobile);
                $("#PropertyDetails_MonopolyPreference").val(d.MonopolyPreference);
                $("#PropertyDetails_HallRatesNegotiable").prop("checked", d.HallRatesNegotiable);
                $("#PropertyDetails_FoodPreference").prop("checked", d.FoodPreference);
                $("#PropertyDetails_searchLocation").val(d.SearchLocation);
                $("#PropertyDetails_tnc").val(d.AboutUsAndTnC);

                // Set Facilities
                if (d.Facilities && Array.isArray(d.Facilities)) {
                    // Get all facility cells that actually contain facility names
                    const facilityCells = document.querySelectorAll('.facility-cell:has(span)');

                    // Convert the API response to lowercase for case-insensitive comparison
                    const apiFacilities = d.Facilities.map(f => f.toLowerCase().trim());

                    facilityCells.forEach(cell => {
                        // Safely get the facility name
                        const spanElement = cell.querySelector('span');
                        if (!spanElement) return; // Skip if no span found

                        const facilityName = spanElement.textContent.trim().toLowerCase();
                        const checkbox = cell.querySelector('.PropertyDetails_facility-checkbox');

                        // Check if this facility exists in the API response
                        if (apiFacilities.includes(facilityName)) {
                            checkbox.checked = true;
                            cell.classList.add('checked');
                        } else {
                            checkbox.checked = false;
                            cell.classList.remove('checked');
                        }
                    });
                }

                // Set Rules
                if (d.Rules && Array.isArray(d.Rules)) {
                    // Get all rule wrappers
                    const ruleWrappers = document.querySelectorAll('.rule-wrapper');

                    // Process each rule wrapper
                    ruleWrappers.forEach(wrapper => {
                        // Get both options (Allowed/Not Allowed)
                        const options = wrapper.querySelectorAll('.rule-option');
                        const allowedOption = options[0];
                        const notAllowedOption = options[1];

                        // Get the rule name (without "Allowed"/"Not Allowed")
                        const ruleName = allowedOption.querySelector('span').textContent
                            .replace('Allowed', '').replace('Not Allowed', '').trim();

                        // Check if this rule exists in the API response
                        const apiRule = d.Rules.find(r => r.includes(ruleName));

                        if (apiRule) {
                            // Determine which option to select based on the API response
                            const isAllowed = apiRule.includes('Not Allowed') ? false : true;

                            // Toggle the selected class and checkbox state
                            if (isAllowed) {
                                allowedOption.classList.add('selected');
                                allowedOption.querySelector('.PropertyDetails_rule-checkbox').checked = true;
                                notAllowedOption.classList.remove('selected');
                                notAllowedOption.querySelector('.PropertyDetails_rule-checkbox').checked = false;
                            } else {
                                notAllowedOption.classList.add('selected');
                                notAllowedOption.querySelector('.PropertyDetails_rule-checkbox').checked = true;
                                allowedOption.classList.remove('selected');
                                allowedOption.querySelector('.PropertyDetails_rule-checkbox').checked = false;
                            }
                        }
                    });
                }

                // Set Booking Steps
                if (d.BookingSteps && Array.isArray(d.BookingSteps)) {
                    const container = document.getElementById("PropertyDetails_bookingStepsContainer");
                    container.innerHTML = ''; // Clear existing inputs

                    d.BookingSteps.forEach(step => {
                        if (step && step.trim() !== '') {
                            addBookingStepWithValue(step.trim());
                        }
                    });
                }

                // Initialize map if location exists
                // if (d.SearchLocation) {
                //     initMapWithLocation(d.SearchLocation);
                // }

                // Example: during edit
                //console.log("url:", d.CompanyPanCardUpload);

                const previewImg = document.getElementById("PanCardPreview");
                const fullViewImg = document.getElementById("modalImage");

                // if API gives full URL, use it directly
                if (d.CompanyPanCardUpload && d.CompanyPanCardUpload !== "") {
                    let fileUrl = d.CompanyPanCardUpload;
                    //console.log("fileUrl:",fileUrl);
                    // Check if it's a PDF file
                    if (fileUrl.toLowerCase().endsWith(".pdf")) {
                        previewImg.src = "/assets2/img/pdf_icon.png"; // show icon
                        fullViewImg.src = "/assets2/img/pdf_icon.png";
                        previewImg.setAttribute("data-file-url", fileUrl); // keep actual url for download/view
                    }
                    else {
                        // const previewImg = document.getElementById("previewImg");
                        // const fullViewImg = document.getElementById("fullViewImg");
                        if (previewImg) {
                            previewImg.onerror = function () {
                                previewImg.src = "";
                            };
                            previewImg.src = fileUrl;
                        }

                        if (fullViewImg) {
                            fullViewImg.onerror = function () {
                                fullViewImg.src = "";
                            };
                            fullViewImg.src = fileUrl;
                        }
                    }
                }
                else {
                    // No file → reset to placeholder
                    previewImg.src = "/assets2/img/image_icon.png";
                    fullViewImg.src = "";
                }

            }
            else {
                //Swal.fire("Info", "No company profile found.", "info");
                //Swal.fire("Info", "Please add company profile data.", "info");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to fetch company profile');
            //Swal.fire("Error", "Failed to fetch company profile.", "error");
        }
    });
}

// Unified save function for both tabs
async function saveCompanyProfileData() {
    let fileInput = document.getElementById("CompanyProfile_PanCardUpload");
    let file = fileInput.files.length > 0 ? fileInput.files[0] : null;

    if (file) {
        let base64Image = await getBase64(file);  // use same helper as 5-image case
        saveCompanyProfileDataToApi(base64Image);
    } else {
        saveCompanyProfileDataToApi("");
    }
}

function saveCompanyProfileDataToApi(panCardImage) {
    const payload = {
        Id: companyId,
        // Company Profile Data
        CompanyName: $("#CompanyProfile_CompanyName").val(),
        BanquetType: parseInt($("#CompanyProfile_BanquetType").val()) || 0,
        CompanyState: parseInt($("#CompanyProfile_State").val()) || 0,
        CompanyCity: $("#CompanyProfile_City").val(),
        CompanyEmail: $("#CompanyProfile_Email").val(),
        CompanyAddress: $("#CompanyProfile_Address").val(),
        CompanyPanNo: $("#CompanyProfile_PanNo").val(),
        CompanyPanCardUpload: panCardImage,
        CompanyMobile: $("#CompanyProfile_Mobile").val(),
        ReviewUrl: $("#CompanyProfile_ReviewUrl").val(),
        AcceptGST: $("#CompanyProfile_AcceptGST").is(":checked"),
        GSTIN: $("#CompanyProfile_GSTIN").val(),
        VATNumber: $("#CompanyProfile_VATNumber").val(),
        QuotationsWithTax: $("#CompanyProfile_QuotationsWithTax").is(":checked"),
        MultipleFoodGST: $("#CompanyProfile_MultipleFoodGST").is(":checked"),
        PrintCustomerDetails: $("#CompanyProfile_PrintCustomerDetails").is(":checked"),
        ActionBy: Number(sessionStorage.getItem('userId') || 0)
    };
    console.log("company profile payload:", payload);

    $.ajax({
        url: `/api/Company_ProfileAndPropertyDetails/SaveCompanyProfile`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                handleApiSuccess();
                getCompanyProfile();
                if (companyId === 0 && res.data?.Id) {
                    companyId = res.data.Id; // Update company ID if it was a new record
                }
            }
            else {
                Swal.fire("Error", res.message || "Failed to save data", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed Save Company Profile');
        }
    });
}

// Unified save function for both tabs
function saveCompanyPropertyData() {
    saveCompanyPropertyDataToApi();
}

function saveCompanyPropertyDataToApi() {
    //showLoader();
    const payload = {
        Id: companyId,
        // Property Details Data
        PropertyCity: $("#PropertyDetails_City").val(),
        Locality: $("#PropertyDetails_Locality").val(),
        Pincode: $("#PropertyDetails_Pincode").val(),
        ManagerName: $("#PropertyDetails_ManagerName").val(),
        PropertyName: $("#PropertyDetails_PropertyName").val(),
        PropertyType: $("#PropertyDetails_PropertyType").val(),
        PropertyEmail: $("#PropertyDetails_Email").val(),
        PropertyAddress: $("#PropertyDetails_Address").val(),
        ManagerMobile: $("#PropertyDetails_ManagerMobile").val(),
        MonopolyPreference: $("#PropertyDetails_MonopolyPreference").val() || 0,
        HallRatesNegotiable: $("#PropertyDetails_HallRatesNegotiable").is(":checked"),
        FoodPreference: $("#PropertyDetails_FoodPreference").is(":checked"),
        Facilities: getFacilitiesArray(),
        Rules: getRulesArray(),
        SearchLocation: $("#PropertyDetails_searchLocation").val(),
        BookingSteps: getBookingStepsArray(),
        AboutUsAndTnC: $("#PropertyDetails_tnc").val(),
        ActionBy: Number(sessionStorage.getItem('userId') || 0)
    };
    console.log("company property payload:", payload);
    $.ajax({
        url: `/api/Company_ProfileAndPropertyDetails/SaveCompanyPropertyDetails`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                handleApiSuccess("Saved!");
                if (companyId === 0 && res.data?.Id) {
                    companyId = res.data.Id; // Update company ID if it was a new record
                }
            } else {
                Swal.fire("Error", res.message || "Failed to save data", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to save property data.');
        }
    });
}

//SAVE SLOST MASTER
$("#btnSaveSlots").click(function () {
    //Save Slots
    let slots = [];
    $("#slotContainer .time-slot").each(function () {
        let container = this; // IMPORTANT
        let startTime = getTimeFromFields(container, 'start');
        let endTime = getTimeFromFields(container, 'end');
        slots.push({
            Id: parseInt($(this).find(".slot-id").val()) || 0,
            Name: $(this).find(".slot-name").val().trim(),
            Code: $(this).find(".slot-code").val().trim(),
            StartTime: startTime, //$(this).find(".slot-start").val(),
            EndTime: endTime, //$(this).find(".slot-end").val(),
            IsActive: $(this).find(".slot-active").is(":checked")
        });
    });

    var payload = {
        Slots: slots,
        ActionBy: Number(sessionStorage.getItem("userId") ?? 0)
    };
    console.log("payload:", payload);

    $.ajax({
        url: "/api/SlotMaster/SaveSlot",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                handleApiSuccess("Saved");
                loadSlots();
            }
            else {
                Swal.fire("Saved Failed", res.message, "info");
            }
        },
        error: function (xhr, state, error) {
            handleApiError(xhr, state, error, "Save Slot Failed");
        }
    });

});

//get data from API
function getSettingsData() {
    $.ajax({
        url: "/api/Settings/GetSettings",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            //data = response; // Store the fetched data
            //console.log("response fetchData:", response);
            var data = response.data;
            data = data.filter(item =>
                item.Name?.trim().toLowerCase() === "Booking Grace Time (in Mins)".toLowerCase()
            );

            if (data.length > 0) {

                const graceMins = parseInt(data[0].Value) || 0;

                let hours = Math.floor(graceMins / 60);
                let minutes = graceMins % 60;

                let hrText = "";

                if (hours > 0 && minutes > 0) {
                    hrText = `${hours} hr ${minutes} min`;
                }
                else if (hours > 0) {
                    hrText = `${hours} hr`;
                }
                else {
                    hrText = `${minutes} min`;
                }

                let explanation =
                    "Grace Time is extra buffer time automatically added after each booking " +
                    "to allow cleaning and preparation before the next event. " +
                    "You can modify this duration from the Settings page.";

                $("#graceTimeLabel")
                    .text(`Grace Time: ${graceMins} mins (${hrText})`)
                    .attr("title", explanation);

                // Initialize tooltip
                var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
                tooltipTriggerList.map(function (tooltipTriggerEl) {
                    return new bootstrap.Tooltip(tooltipTriggerEl);
                });
            }
        },
        error: function (xhr, statue, error) {
            handleApiError(xhr, statue, error, "Fetch Settings Failed");
        }
    });
}

// Helper functions
function getFacilitiesArray() {
    const facilities = [];
    $('.PropertyDetails_facility-checkbox:checked').each(function () {
        facilities.push($(this).closest('.facility-cell').text().trim());
    });
    return facilities;
}

function getRulesArray() {
    const rules = [];
    $('.PropertyDetails_rule-checkbox:checked').each(function () {
        rules.push($(this).closest('.rule-option').text().trim());
    });
    return rules;
}

function getBookingStepsArray() {
    const steps = [];
    $('#PropertyDetails_bookingStepsContainer input').each(function () {
        if ($(this).val().trim() !== '') {
            steps.push($(this).val().trim());
        }
    });
    return steps;
}

function addBookingStepWithValue(value = '') {
    let container = document.getElementById("PropertyDetails_bookingStepsContainer");
    let div = document.createElement("div");
    div.className = "input-group mb-2";

    let input = document.createElement("input");
    input.type = "text";
    input.className = "form-control";
    input.placeholder = "Enter step";
    input.value = value;

    let button = document.createElement("button");
    button.type = "button";
    button.className = "btn btn-danger";
    button.innerHTML = "×";
    button.onclick = function () {
        container.removeChild(div);
    };

    div.appendChild(input);
    div.appendChild(button);
    container.appendChild(div);
}

const fileInput = document.getElementById("CompanyProfile_PanCardUpload");
const previewImg = document.getElementById("PanCardPreview");   // ✅ correct id
const fullViewImg = document.getElementById("modalImage");

fileInput.addEventListener("change", function () {
    const file = this.files[0];

    if (!file) {
        previewImg.src = "/assets2/img/image_icon.png";
        fullViewImg.src = "";
        return;
    }

    const reader = new FileReader();

    reader.onload = function (e) {
        if (file.type === "application/pdf") {
            previewImg.src = "/assets2/img/pdf_icon.png";
            fullViewImg.src = "/assets2/img/pdf_icon.png";
        } else {
            previewImg.src = e.target.result;
            fullViewImg.src = e.target.result;
        }
    };

    reader.readAsDataURL(file);
});

// Show modal on preview click
previewImg.addEventListener("click", function () {
    if (fullViewImg.src) {
        new bootstrap.Modal(document.getElementById("imageModal")).show();
    }
});

// Initialize when page loads
$(document).ready(function () {

    // Test tab functionality
    const testTab = document.querySelector('#companyTabs .nav-link');
    if (testTab) {
        testTab.addEventListener('click', function (e) {
            console.log('Tab clicked:', this.getAttribute('href'));
        });
    }

    // Load data if companyId is set
    loadSlots();
    getSettingsData();
    Promise.all([
        loadBanquetTypes(),
        loadStates(),
        loadCities(),
        loadLocalities(),
        loadPropertyTypes(),
        loadMonopolyPreferences()
    ])
        .then(() => {
            // All loading is complete, now get company profile
            getCompanyProfile();
        })
        .catch(error => {
            console.error("Error loading initial data:", error);
            Swal.fire({
                icon: 'error',
                title: 'Initialization Error',
                text: 'Failed to load required data. Please refresh the page.'
            });
        });

    //addBookingStep
    if ($("#bookingStepContainer .booking-step").length === 0) {
        addBookingStep();
    }

    // Initialize map
    //initMapWithLocation();

    // Set up location search - for MAP
    // $("#PropertyDetails_searchLocation").on("change", function() {
    //     initMapWithLocation(this.value);
    // });

    // modal - banquetType -- START  ------------------------------------------------------------------------
    //loadBanquetTypes();

    // Save / Update modal - banquetType
    $("#btnSaveBanquetType").click(function () {
        //showLoader();
        let id = $("#banquetTypeId").val();
        let name = $("#banquetTypeName").val().trim();

        if (!name) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Input',
                text: 'Please enter a banquet type name.',
            });
            return;
        }

        $.ajax({
            url: "/api/banquettype/SaveBanquetType", // API endpoint for insert/update
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({ id: id, name: name }),
            success: function (res) {
                handleApiSuccess("Saved");
                clearBanquetTypeForm();
                loadBanquetTypes();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while saving banquet type data');
            }
        });
    });

    // Clear form modal - banquetType
    $("#btnClearBanquetType").click(function () {
        clearBanquetTypeForm();
    });

    // Edit modal - banquetType
    $(document).on("click", ".edit-banquet", function () {
        let id = $(this).data("id");
        let name = $(this).data("name");
        $("#banquetTypeId").val(id);
        $("#banquetTypeName").val(name);
    });

    // Delete modal - banquetType
    $(document).on("click", ".delete-banquet", function () {
        Swal.fire({
            title: 'Are you sure?',
            text: 'Do you want to delete this banquet type?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                // ✅ Proceed with delete
                let id = $(this).data("id");

                //showLoader();
                $.ajax({
                    url: "/api/banquettype/DeleteBanquetType/" + id, // API endpoint for delete
                    method: "DELETE",
                    success: function (res) {
                        handleApiSuccess("Deleted");
                        loadBanquetTypes();
                    },
                    error: function (xhr, status, error) {
                        handleApiError(xhr, status, error, 'Error while deleting banquet data');
                    }
                });
            }
        });
    });
    // modal - banquetType -- END /--------------------------------------------------------------------------

    // modal - State -- START ------------------------------------------------------------------------------
    //loadStates();

    // Save / Update State
    $("#btnSaveState").click(function () {
        let id = $("#stateId").val();
        let name = $("#stateName").val().trim();
        let code = $("#stateCode").val().trim();

        if (!name) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Input',
                text: 'Please enter a state name.',
            });
            return;
        }

        var payload = {
            id: id,
            name: name,
            code: code,
            ActionBy: Number(sessionStorage.getItem('userId') || 0)
        };

        //showLoader();
        $.ajax({
            url: "/api/state/SaveState",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                handleApiSuccess("Saved");
                clearStateForm();
                loadStates();
                //$('#stateModal').modal('hide');
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while saving state data');
            }
        });
    });

    // Clear State Form
    $("#btnClearState").click(function () {
        clearStateForm();
    });

    // Edit State
    $(document).on("click", ".edit-state", function () {
        let id = $(this).data("id");
        let name = $(this).data("name");
        let code = $(this).data("code");
        $("#stateId").val(id);
        $("#stateName").val(name);
        $("#stateCode").val(code);
        $('#stateModal').modal('show');
    });

    // Delete State
    $(document).on("click", ".delete-state", function () {
        let id = $(this).data("id");
        let button = $(this);

        Swal.fire({
            title: 'Are you sure?',
            text: 'Do you want to delete this state?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                //showLoader();
                let userId = parseInt(sessionStorage.getItem('userId') ? sessionStorage.getItem('userId') : 0);
                $.ajax({
                    url: "/api/State/DeleteState",
                    type: "POST",
                    contentType: "application/json",
                    data: JSON.stringify({
                        Id: id,
                        ActionBy: Number(sessionStorage.getItem('userId') || 0)
                    }),
                    success: function (res) {
                        handlepApiSuccess("Deleted");
                        loadStates();
                    },
                    error: function (xhr, status, error) {
                        handleApiError(xhr, status, error, 'Error while deleting state data');
                    }
                });
            }
        });
    });
    // modal - State -- END /--------------------------------------------------------------------------------

    // modal - City -- START -------------------------------------------------------------------------------
    //loadCities();

    // Save / Update City
    $("#btnSaveCity").click(function () {
        let id = $("#cityId").val();
        let name = $("#cityName").val().trim();
        let pincode = $("#cityPincode").val().trim();
        let stateId = $("#cityState").val();

        if (!name) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Input',
                text: 'Please enter a city name.',
            });
            return;
        }

        if (!stateId) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Input',
                text: 'Please select a state.',
            });
            return;
        }

        var payload = {
            Id: id,
            Name: name,
            StateId: stateId,
            Pincode: pincode,
            ActionBy: Number(sessionStorage.getItem('userId') || 0)
        };
        //showLoader();
        $.ajax({
            url: "/api/city/SaveCity",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                handleApiSuccess("Saved");
                const stateId = $("#cityState").val() || 0;
                loadCities(stateId);
                clearCityForm(stateId);
                //$('#cityModal').modal('hide');
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while saving city data');
            }
        });
    });

    // Clear City Form
    $("#btnClearCity").click(function () {
        clearCityForm();
    });

    // Edit City
    $(document).on("click", ".edit-city", function () {
        let id = $(this).data("id");
        let name = $(this).data("name");
        let stateId = $(this).data("stateid");
        let stateName = $(this).data("statename");

        $("#cityId").val(id);
        $("#cityName").val(name);
        $("#cityState").val(stateId); //.trigger('change');
        $('#cityModal').modal('show');
    });

    // Delete City
    $(document).on("click", ".delete-city", function () {
        let id = $(this).data("id");
        let button = $(this);

        Swal.fire({
            title: 'Are you sure?',
            text: 'Do you want to delete this city?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                //showLoader();
                $.ajax({
                    url: "/api/city/DeleteCity/" + id,
                    method: "DELETE",
                    success: function (res) {
                        handleApiSuccess("Deleted");
                        const stateId = $("#cityState").val() || 0;
                        loadCities(stateId);
                    },
                    error: function (xhr, status, error) {
                        handleApiError(xhr, status, error, 'Error while deleting City');
                    }
                });
            }
        });
    });
    // modal - City -- END /---------------------------------------------------------------------------------

    // modal - Locality -- START ---------------------------------------------------------------------------
    //loadLocalities();
    // Save / Update Locality
    $("#btnSaveLocality").click(function () {
        let id = $("#localityId").val();
        let name = $("#localityName").val().trim();
        let cityId = $("#localityCity").val();

        if (!name) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Input',
                text: 'Please enter a locality name.',
            });
            return;
        }

        if (!cityId) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Input',
                text: 'Please select a city.',
            });
            return;
        }

        //showLoader();
        $.ajax({
            url: "/api/locality/SaveLocality",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                Id: id,
                Name: name,
                CityId: cityId,
                ActionBy: Number(sessionStorage.getItem('userId') || 0)
            }),
            success: function (res) {
                handleApiSuccess("Saved");
                const cityId = $("#localityCity").val() || 0;
                loadLocalities(cityId);
                clearLocalityForm(cityId);
                //$('#localityModal').modal('hide');
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while saving Locality data');
            }
        });
    });

    // Clear Locality Form
    $("#btnClearLocality").click(function () {
        clearLocalityForm();
    });

    // Edit Locality
    $(document).on("click", ".edit-locality", function () {
        let id = $(this).data("id");
        let name = $(this).data("name");
        let cityId = $(this).data("cityid");
        let cityName = $(this).data("cityname");

        $("#localityId").val(id);
        $("#localityName").val(name);
        $("#localityCity").val(cityId); //.trigger('change');
        $('#localityModal').modal('show');
    });

    // Delete Locality
    $(document).on("click", ".delete-locality", function () {
        let id = $(this).data("id");
        let button = $(this);

        Swal.fire({
            title: 'Are you sure?',
            text: 'Do you want to delete this locality?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {

                let payload = {
                    Id: id,
                    ActionBy: sessionStorage.getItem("userId") ?? 0
                };
                $.ajax({
                    url: "/api/Locality/DeleteLocality",
                    method: "POST",
                    data: JSON.stringify(payload),
                    contentType: "application/json",
                    success: function (res) {
                        handleApiSuccess("Deleted!");
                        const cityId = $("#localityCity").val() || 0;
                        loadLocalities(cityId);
                    },
                    error: function (xhr, status, error) {
                        handleApiError(xhr, status, error, 'Error while deleting Locality data');
                    }
                });
            }
        });
    });
    // modal - Locality -- END /-----------------------------------------------------------------------------

    // modal - PropertyType -- START ---------------------------------------------------------------------------
    //loadPropertyTypes();

    // Save / Update Property Type
    $("#btnSavePropertyType").click(function () {
        let id = $("#propertyTypeId").val();
        let name = $("#propertyTypeName").val().trim();

        if (!name) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Input',
                text: 'Please enter a property type name.',
            });
            return;
        }
        //showLoader();
        $.ajax({
            url: "/api/propertytype/SavePropertyType",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                Id: id,
                Name: name,
                ActionBy: Number(sessionStorage.getItem('userId') || 0)
            }),
            success: function (res) {
                handleApiSuccess("Saved");
                clearPropertyTypeForm();
                loadPropertyTypes();
                //$('#propertyTypeModal').modal('hide');
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while saving PropertyType data');
            }
        });
    });

    // Clear Property Type Form
    $("#btnClearPropertyType").click(function () {
        clearPropertyTypeForm();
    });

    // Edit Property Type
    $(document).on("click", ".edit-property-type", function () {
        console.log("edit proprty..");
        let id = $(this).data("id");
        let name = $(this).data("name");

        $("#propertyTypeId").val(id);
        $("#propertyTypeName").val(name);
        $('#propertyTypeModal').modal('show');
    });

    // Delete Property Type
    $(document).on("click", ".delete-property-type", function () {
        let id = $(this).data("id");
        let button = $(this);

        Swal.fire({
            title: 'Are you sure?',
            text: 'Do you want to delete this property type?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                //showLoader();

                $.ajax({
                    url: "/api/propertytype/DeletePropertyType/" + id,
                    method: "DELETE",
                    success: function (res) {
                        handleApiSuccess(res.message || "Deleted!");
                        loadPropertyTypes();
                    },
                    error: function (xhr, status, error) {
                        handleApiError(xhr, status, error, 'Error while deleting PropertyType data');
                    }
                });
            }
        });
    });
    // modal - PropertyType -- END /-----------------------------------------------------------------------------

    // modal - MonopolyPreference -- START ---------------------------------------------------------------------------
    //loadMonopolyPreferences();

    // Save / Update Monopoly Preference
    $("#btnSaveMonopolyPreference").click(function () {
        let id = $("#monopolyPreferenceId").val();
        let name = $("#monopolyPreferenceName").val().trim();

        if (!name) {
            Swal.fire({
                icon: 'warning',
                title: 'Missing Input',
                text: 'Please enter a monopoly preference name.',
            });
            return;
        }
        //showLoader();
        $.ajax({
            url: "/api/monopolypreference/SaveMonopolyPreference",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                Id: id,
                Name: name,
                ActionBy: Number(sessionStorage.getItem('userId') || 0)
            }),
            success: function (res) {
                handleApiSuccess("Saved");
                clearMonopolyPreferenceForm();
                loadMonopolyPreferences();
                //$('#monopolyPreferenceModal').modal('hide');
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while saving MonopolyPreference data');
            }
        });
    });

    // Clear Monopoly Preference Form
    $("#btnClearMonopolyPreference").click(function () {
        clearMonopolyPreferenceForm();
    });

    // Edit Monopoly Preference
    $(document).on("click", ".edit-monopoly-preference", function () {
        let id = $(this).data("id");
        let name = $(this).data("name");

        $("#monopolyPreferenceId").val(id);
        $("#monopolyPreferenceName").val(name);
        $('#monopolyPreferenceModal').modal('show');
    });

    // Delete Monopoly Preference
    $(document).on("click", ".delete-monopoly-preference", function () {
        let id = $(this).data("id");
        let button = $(this);

        Swal.fire({
            title: 'Are you sure?',
            text: 'Do you want to delete this monopoly preference?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {

                let payload = {
                    Id: id,
                    ActionBy: sessionStorage.getItem("userId") ?? 0
                };

                $.ajax({
                    url: "/api/MonopolyPreference/DeleteMonopolyPreference",
                    method: "POST",
                    data: JSON.stringify(payload),
                    contentType: "application/json",
                    success: function (res) {
                        handleApiSuccess(res.message || "Deleted!");    
                        loadMonopolyPreferences();
                    },
                    error: function (xhr, status, error) {
                        handleApiError(xhr, status, error, 'Error while deleting MonopolyPreference data');
                    }
                });
            }
        });
    });
    // modal - MonopolyPreference -- END /-----------------------------------------------------------------------------
});

// Load List - BanquetTypes Modal
function loadBanquetTypes() {
    return new Promise((resolve, reject) => {
        //showLoader();
        $.ajax({
            url: "/api/banquettype/GetBanquetTypeList", // API endpoint for fetching list
            method: "GET",
            success: function (res) {
                //console.log("res banquettype:", res);
                res = res.data;
                let tbody = $("#banquetTypeTable tbody");
                tbody.empty();

                // Fill dropdown
                let banquetDropdown = $("#CompanyProfile_BanquetType");
                banquetDropdown.empty();
                banquetDropdown.append(`<option value="">Select Banquet Type</option>`);

                if (res && res.length > 0) {
                    res.forEach((item, index) => {
                        tbody.append(`
                                <tr>
                                    <td class="text-center">${index + 1}</td>
                                    <td>${item.Name}</td>
                                    <td class="text-center">
                                        <div class="dropdown">

                                            <button class="btn btn-xs btn-outline-secondary p-1"
                                                    type="button"
                                                    data-bs-toggle="dropdown"
                                                    aria-expanded="false">
                                                <i class="bi bi-three-dots-vertical"></i>
                                            </button>

                                            <ul class="dropdown-menu dropdown-menu-end">

                                                <li>
                                                    <a class="dropdown-item edit-banquet"
                                                       data-id="${item.Id}"
                                                       data-name="${item.Name}">
                                                        <i class="bi bi-pencil-square text-warning me-2"></i> Edit
                                                    </a>
                                                </li>

                                                <li>
                                                    <a class="dropdown-item delete-banquet"
                                                       data-id="${item.Id}">
                                                        <i class="bi bi-trash text-danger me-2"></i> Delete
                                                    </a>
                                                </li>

                                            </ul>

                                        </div>
                                    </td>
                                </tr>
                            `);
                        // Dropdown option
                        banquetDropdown.append(`<option value="${item.Id}">${item.Name}</option>`);
                    });
                } else {
                    tbody.append(`<tr><td colspan="3" class="text-center">No banquet types found.</td></tr>`);
                }
                resolve();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while loading BanquetTypeList');
                const errMsg = getErrorMessageFromResponse(xhr.responseText, 'Error while loading BanquetTypeList');
                reject(errMsg);
            }
        });
    });
}
// Reset Form - BanquetTypes Modal
function clearBanquetTypeForm() {
    $("#banquetTypeId").val(0);
    $("#banquetTypeName").val("");
}

// Load States
function loadStates() {
    return new Promise((resolve, reject) => {
        //showLoader();
        $.ajax({
            url: "/api/State/GetStateList",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                Id: 0     // or specific Id if needed
            }),
            success: function (res) {
                //console.log("res states:", res);
                res = res.data;
                let tbody = $("#stateTable tbody");
                tbody.empty();

                // Fill dropdowns (both main form and city modal)
                let stateDropdown = $("#CompanyProfile_State, #cityState");
                stateDropdown.empty();
                stateDropdown.append(`<option value="">Select State</option>`);

                if (res && res.length > 0) {
                    res.forEach((item, index) => {
                        // Add to table
                        tbody.append(`
                                <tr>
                                    <td class="text-center">${index + 1}</td>
                                    <td>${item.Name}</td>
                                    <td>${item.Code}</td>
                                    <td>
                                        <button class="btn btn-sm btn-outline-warning edit-state"
                                            data-id="${item.Id}" data-name="${item.Name}" 
                                            data-code="${item.Code}">
                                            <i class="bi bi-pencil-square"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-danger delete-state"
                                            data-id="${item.Id}">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            `);

                        // Add to dropdowns
                        stateDropdown.append(`<option value="${item.Id}">${item.Name}</option>`);
                    });
                } else {
                    tbody.append(`<tr><td colspan="3" class="text-center">No states found.</td></tr>`);
                }
                resolve();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while loading StateList');
                const errMsg = getErrorMessageFromResponse(xhr.responseText, 'Error while loading StateList.');
                reject(errMsg);
            },
            complete: function () {
                //hideLoader();  // Hide after request completes (success or error)
            }
        });
    });
}
// Reset State Form
function clearStateForm() {
    $("#stateId").val(0);
    $("#stateName").val("");
    $("#stateCode").val("");
}

// Load Cities
function loadCities(stateId = 0, isFromMasterModalDropdown = 0) {
    return new Promise((resolve, reject) => {
        clearCityForm(stateId);
        var inputData = {
            Id: 0,
            StateId: stateId || 0
        };
        //showLoader();
        //console.log("inputData loadCities:",inputData);
        $.ajax({
            url: "/api/city/GetCityList",
            method: "POST",  // Changed to POST
            contentType: "application/json",
            data: JSON.stringify(inputData),
            success: function (res) {
                //console.log("res cities:", res);
                res = res.data;
                let tbody = $("#cityTable tbody");
                tbody.empty();

                if (isFromMasterModalDropdown == 0) {
                    // Fill dropdowns (both main form and locality modal)
                    let cityDropdown = $("#PropertyDetails_City, #localityCity");
                    cityDropdown.empty();
                    cityDropdown.append(`<option value="">Select City</option>`);
                }

                if (res && res.length > 0) {
                    res.forEach((item, index) => {
                        // Add to table
                        tbody.append(`
                                <tr>
                                    <td class="text-center">${index + 1}</td>
                                    <td>${item.StateName || 'N/A'}</td>
                                    <td>${item.Name}</td>
                                    <td>${item.Pincode}</td>
                                    <td>
                                        <button class="btn btn-sm btn-outline-warning edit-city"
                                            data-id="${item.Id}"
                                            data-name="${item.Name}"
                                            data-pincode="${item.Pincode}"
                                            data-stateid="${item.StateId}"
                                            data-statename="${item.StateName}">
                                            <i class="bi bi-pencil-square"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-danger delete-city"
                                            data-id="${item.Id}">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            `);

                        // Add to dropdowns
                        if (isFromMasterModalDropdown == 0) {
                            $("#PropertyDetails_City, #localityCity").append(`<option value="${item.Id}">${item.Name}</option>`);
                        }
                    });
                } else {
                    tbody.append(`<tr><td colspan="4" class="text-center">No cities found.</td></tr>`);
                }
                resolve();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while loading CityList');
                const errMsg = getErrorMessageFromResponse(xhr.responseText, 'Error while loading localities.');
                reject(errMsg);
            }
        });
    });
}

// Reset City Form
function clearCityForm(stateId = 0) {
    //console.log("stateId in clearCityForm():",stateId);
    $("#cityId").val(0);
    $("#cityName").val("");
    $("#cityState").val(stateId == 0 ? "" : stateId); //.trigger('change');
}

// Load Localities
function loadLocalities(cityId = 0, isFromMasterModalDropdown = 0) {
    return new Promise((resolve, reject) => {
        clearLocalityForm(cityId);
        var inputData = {
            Id: 0,
            CityId: cityId || 0
        };
        //console.log("inputData loadLocalities:",inputData);
        //showLoader();
        $.ajax({
            url: "/api/locality/GetLocalityList",
            method: "POST",  // Changed to POST
            contentType: "application/json",
            data: JSON.stringify(inputData),
            success: function (res) {
                //console.log("res localities:", res);
                res = res.data;
                let tbody = $("#localityTable tbody");
                tbody.empty();

                if (isFromMasterModalDropdown == 0) {
                    // Fill dropdown
                    let localityDropdown = $("#PropertyDetails_Locality");
                    localityDropdown.empty();
                    localityDropdown.append(`<option value="">Select Locality</option>`);
                }

                if (res && res.length > 0) {
                    res.forEach((item, index) => {
                        // Add to table
                        tbody.append(`
                                <tr>
                                    <td class="text-center">${index + 1}</td>
                                    <td>${item.CityName || 'N/A'}</td>
                                    <td>${item.Name}</td>
                                    <td>
                                        <button class="btn btn-sm btn-outline-warning edit-locality"
                                            data-id="${item.Id}"
                                            data-name="${item.Name}"
                                            data-cityid="${item.CityId}"
                                            data-cityname="${item.CityName}">
                                            <i class="bi bi-pencil-square"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-danger delete-locality"
                                            data-id="${item.Id}">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            `);

                        // Add to dropdown
                        if (isFromMasterModalDropdown == 0) {
                            $("#PropertyDetails_Locality").append(`<option value="${item.Id}">${item.Name}</option>`);
                        }
                    });
                } else {
                    tbody.append(`
                            <tr>
                                <td colspan="4" class="text-center">
                                    No localities found${cityId ? ' for selected city' : ''}.
                                </td>
                            </tr>
                        `);
                }
                resolve();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while loading LocalityList');
                const errMsg = getErrorMessageFromResponse(xhr.responseText, 'Error while loading localities.');
                reject(errMsg);
            }
        });
    });
}

// Reset Locality Form
function clearLocalityForm(cityId = 0) {
    $("#localityId").val(0);
    $("#localityName").val("");
    $("#localityCity").val(cityId == 0 ? "" : cityId); //.trigger('change');
}

// Load Property Types
function loadPropertyTypes() {
    return new Promise((resolve, reject) => {
        //showLoader();
        $.ajax({
            url: "/api/propertytype/GetPropertyTypeList",
            method: "GET",
            success: function (res) {
                //console.log("res property types:", res);
                res = res.data;

                let tbody = $("#propertyTypeTable tbody");
                tbody.empty();

                // Fill dropdown
                let propertyTypeDropdown = $("#PropertyDetails_PropertyType");
                propertyTypeDropdown.empty();
                propertyTypeDropdown.append(`<option value="">Select Property Type</option>`);

                if (res && res.length > 0) {
                    res.forEach((item, index) => {
                        tbody.append(`
                                <tr>
                                    <td class="text-center">${index + 1}</td>
                                    <td>${item.Name}</td>
                                    <td>
                                        <button class="btn btn-sm btn-outline-warning edit-property-type"
                                            data-id="${item.Id}" data-name="${item.Name}">
                                            <i class="bi bi-pencil-square"></i>
                                        </button>
                                        <button class="btn btn-sm btn-outline-danger delete-property-type"
                                            data-id="${item.Id}">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            `);
                        // Dropdown option
                        propertyTypeDropdown.append(`<option value="${item.Id}">${item.Name}</option>`);
                    });
                } else {
                    tbody.append(`<tr><td colspan="3" class="text-center">No Property Type found.</td></tr>`);
                }
                resolve();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while loading PropertyTypeList');
                const errMsg = getErrorMessageFromResponse(xhr.responseText, 'Error while loading PropertyTypeList.');
                reject(errMsg);
            },
            complete: function () {
                //hideLoader();  // Hide after request completes (success or error)
            }
        });
    });
}

// Reset Property Type Form
function clearPropertyTypeForm() {
    $("#propertyTypeId").val(0);
    $("#propertyTypeName").val("");
}

// Load Monopoly Preferences
function loadMonopolyPreferences() {
    return new Promise((resolve, reject) => {
        //showLoader();

        $.ajax({
            url: "/api/monopolypreference/GetMonopolyPreferenceList",
            method: "GET",
            success: function (res) {
                //console.log("res monopoly preferences:", res);
                res = res.data;
                let tbody = $("#monopolyPreferenceTable tbody");
                tbody.empty();

                // Fill dropdown
                let monopolyPreferenceDropdown = $("#PropertyDetails_MonopolyPreference");
                monopolyPreferenceDropdown.empty();
                monopolyPreferenceDropdown.append(`<option value="0">Select Monopoly Preference</option>`);

                if (res && res.length > 0) {
                    res.forEach((item, index) => {
                        tbody.append(`
                                <tr>
                                    <td class="text-center">${index + 1}</td>
                                    <td>${item.Name}</td>
                                    <td>
                                        <button class="btn btn-xs btn-outline-warning edit-monopoly-preference"
                                            data-id="${item.Id}" data-name="${item.Name}">
                                            <i class="bi bi-pencil-square"></i>
                                        </button>
                                        <button class="btn btn-xs btn-outline-danger delete-monopoly-preference"
                                            data-id="${item.Id}">
                                            <i class="bi bi-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            `);
                        // Dropdown option
                        monopolyPreferenceDropdown.append(`<option value="${item.Id}">${item.Name}</option>`);
                    });
                } else {
                    tbody.append(`<tr><td colspan="3" class="text-center">No Monopoly Preference found.</td></tr>`);
                }
                resolve();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while loading MonopolyPreferenceList');
                const errMsg = getErrorMessageFromResponse(xhr.responseText, 'Error while loading MonopolyPreferenceList.');
                reject(errMsg);
            }
        });
    });
}

// Reset Monopoly Preference Form
function clearMonopolyPreferenceForm() {
    $("#monopolyPreferenceId").val(0);
    $("#monopolyPreferenceName").val("");
}

// ✅ Reusable function to parse text and get error message
function getErrorMessageFromResponse(responseText, fallbackMsg = 'An error occurred.') {
    try {
        const errObj = JSON.parse(responseText);
        if (errObj?.message) {
            return errObj.message;
        }
    } catch (e) {
        console.error("Error parsing JSON:", e);
    }
    return fallbackMsg;
}

$("#CompanyProfile_AcceptGST").change(function () {
    if ($(this).is(":checked")) {
        $("#gstinContainer").show();
    } else {
        $("#gstinContainer").hide();
        //$("#CompanyProfile_GSTIN").val(""); // optional: clear value if unchecked
    }
});
 
// KEEP TAB OPEN - START
$('#companyTabs a').on('click', function () {
    let tabId = $(this).attr('href'); // e.g. #tab-company-profile
    sessionStorage.setItem('activeTab', tabId);
});

let activeTab = sessionStorage.getItem('activeTab') || '#tab-company-profile';
let tab = new bootstrap.Tab(document.querySelector('#companyTabs a[href="' + activeTab + '"]'));
tab.show();
// KEEP TAB OPEN - END


function loadSlots() {

    $.ajax({
        url: "/api/SlotMaster/GetSlotList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, IsActive: false }),
        success: function (res) {
            console.log("res slot:", res);
            if (res.status !== "Success") return;

            let container = $("#slotContainer");
            container.empty();

            // ✅ HEADER ROW
            let header = `
                <div class="d-flex align-items-center gap-1 border-bottom fw-semibold pb-2 mb-2 text-primary fs-9">
                    <div style="width:40px;">#</div>
                    <div style="width:40px;"></div> <!-- toggle space -->
                    <div style="max-width:150px; width:150px;">Shift</div>
                    <div style="max-width:50px; width:50px;">Code</div>
                    <div style="width:170px;">Start Time</div>
                    <div style="width:20px;"></div>
                    <div style="width:170px;">End Time</div>
                </div>
            `;
            container.append(header);

            res.data.forEach(slot => {
                let html = `
                    <div class="time-slot border p-2 mb-3 rounded">
                        <input type="hidden" class="slot-id" value="${slot.Id}" />

                        <div class="d-flex align-items-center gap-1 flex-wrap">

                            <!-- Toggle -->
                            <div class="form-check form-switch m-0">
                                <input class="form-check-input slot-active" type="checkbox" ${slot.IsActive ? "checked" : ""}>
                            </div>

                            <!-- Name -->
                            <input type="text" class="form-control slot-name p-1" value="${slot.Name}" style="max-width:190px;" />

                            <!-- Code -->
                            <input type="text" class="form-control slot-code p-1" value="${slot.Code}" style="max-width:50px;" readonly />

                            <!-- Start Time -->
                            <div class="d-flex align-items-center gap-1">
                                <input type="number" class="form-control start-hh p-1" placeholder="HH" min="1" max="12" style="width:55px;" value="${slot.StartTimeHH}">
    
                                <input type="number" class="form-control start-mm p-1" placeholder="MM" min="0" max="59" style="width:55px;" value="${slot.StartTimeMM}">
    
                                <select class="form-control start-ampm p-1" style="width:50px;">
                                    <option value="AM" ${slot.StartTimeAMPM === 'AM' ? 'selected' : ''}>AM</option>
                                    <option value="PM" ${slot.StartTimeAMPM === 'PM' ? 'selected' : ''}>PM</option>
                                </select>
                            </div>

                            <span class="fw-bold mx-2">-</span>

                            <!-- End Time -->
                            <div class="d-flex align-items-center gap-1">
                                <input type="number" class="form-control end-hh p-1" placeholder="HH" min="1" max="12" style="width:55px;" value="${slot.EndTimeHH}">
    
                                <input type="number" class="form-control end-mm p-1" placeholder="MM" min="0" max="59" style="width:55px;" value="${slot.EndTimeMM}">
    
                                <select class="form-control end-ampm p-1" style="width:50px;">
                                    <option value="AM" ${slot.EndTimeAMPM === 'AM' ? 'selected' : ''}>AM</option>
                                    <option value="PM" ${slot.EndTimeAMPM === 'PM' ? 'selected' : ''}>PM</option>
                                </select>
                            </div>

                        </div>
                    </div>
                `;
                container.append(html);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "fetch Slots Failed");
        }
    });
}
function getTimeFromFields(container, type) {
    let hh = container.querySelector(`.${type}-hh`).value;
    let mm = container.querySelector(`.${type}-mm`).value;
    let ampm = container.querySelector(`.${type}-ampm`).value;

    if (!hh || !mm) return '';

    return `${hh.toString().padStart(2, '0')}:${mm.toString().padStart(2, '0')} ${ampm}`;
}

function generateTimeOptions(selectedValue = "") {
    let options = "";
    for (let hour = 0; hour < 24; hour++) {
        for (let min = 0; min < 60; min += 15) {

            let period = hour >= 12 ? "PM" : "AM";
            let displayHour = hour % 12;
            if (displayHour === 0) displayHour = 12;

            let minute = min.toString().padStart(2, "0");

            let value = `${displayHour.toString().padStart(2, "0")}:${minute}${period}`;
            let selected = value === selectedValue ? "selected" : "";

            options += `<option value="${value}" ${selected}>${value}</option>`;
        }
    }
    return options;
}

function convertTo12HourFormat(time24) {
    if (!time24) return "";

    let [hour, minute] = time24.split(":");

    hour = parseInt(hour);

    let period = hour >= 12 ? "PM" : "AM";
    let displayHour = hour % 12;
    if (displayHour === 0) displayHour = 12;

    return `${displayHour.toString().padStart(2, "0")}:${minute}${period}`;
}
//<!-- TAB 1-2: Company Profile & Property Details -- END -->

//<!-- TAB 3: Hall Rent & Details -- START -->

// function openModal(img) {
//     document.getElementById("modalImage").src = img.src;
//     new bootstrap.Modal(document.getElementById("imageModal")).show();
// }

function toggleOption(item) {
    const checkbox = item.querySelector('.option-checkbox');
    checkbox.checked = !checkbox.checked;
    //item.classList.toggle('active', checkbox.checked);

    // Also toggle the icon color
    const icon = item.querySelector('.facility-icon');
    if (checkbox.checked) {
        icon.style.color = '#ff0055';
    } else {
        icon.style.color = 'gray';
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const partitionToggle = document.getElementById('PartitionToggle');
    const partitionLabel = document.getElementById('partitionLabel');

    partitionToggle.addEventListener('change', () => {
        if (partitionToggle.checked) {
            partitionLabel.textContent = 'Yes';
            partitionLabel.classList.remove('text-muted');
            partitionLabel.classList.add('text-danger', 'fw-semibold');
        } else {
            partitionLabel.textContent = 'No';
            partitionLabel.classList.remove('text-danger', 'fw-semibold');
            partitionLabel.classList.add('text-muted');
        }
    });

    document.querySelectorAll('.option-item').forEach(item => {
        item.addEventListener('click', () => {
            item.classList.toggle('active');
        });
    });

    // Fetch and populate hall data
    fetchHallDetails();

    // ✅ Toggle label for Partition Yes/No
    // document.getElementById("PartitionToggle").addEventListener("change", function () {
    //     document.getElementById("partitionLabel").innerText = this.checked ? "Yes" : "No";
    // });

    // ✅ Handle form submit
    document.getElementById("hallRentForm").addEventListener("submit", async function (e) {
        e.preventDefault();

        // Collect images (max 5 as per your form)
        const imageInputs = document.querySelectorAll("#hallRentForm input[type='file']");
        const imagesBase64 = [];
        for (let i = 0; i < imageInputs.length; i++) {
            const file = imageInputs[i].files[0];
            const base64 = await getBase64(file);
            imagesBase64.push(base64);
        }

        // SELECTED MERGE HALLS
        let mergeHallList =
            $(".merge-hall-checkbox:checked")
                .map(function () {
                    return { Hall_Id: parseInt($(this).val()) };
                }).get();

        // VALIDATION
        if ($("#HallMergeToggle").is(":checked")) {
            if (mergeHallList.length < 2) {
                Swal.fire({
                    icon: "warning",
                    title: "Validation",
                    text: "Please select at least 2 halls for merge."
                });
                return;
            }
        }

        // Prepare request object
        const requestData = {
            Id: document.getElementById("HallId").value, // if editing, set proper ID
            HallName: document.getElementById("HallName").value,
            CapacityRange: document.getElementById("HallCapacity").value,
            RentPerDay: parseFloat(document.getElementById("HallRent").value || 0),
            IsPartitioned: document.getElementById("PartitionToggle").checked,

            Video360Link: document.getElementById("Video360Link").value || null,
            PromoVideo1: document.getElementById("PromoVideo1").value || null,
            PromoVideo2: document.getElementById("PromoVideo2").value || null,

            Image1: imagesBase64[0],
            Image2: imagesBase64[1],
            Image3: imagesBase64[2],
            Image4: imagesBase64[3],
            Image5: imagesBase64[4],

            // Good For checkboxes
            GoodFor_GrandWedding: getCheckboxValue("GoodFor_GrandWedding"),
            GoodFor_Weddings: getCheckboxValue("GoodFor_Weddings"),
            GoodFor_BirthdayParty: getCheckboxValue("GoodFor_BirthdayParty"),
            GoodFor_Engagement: getCheckboxValue("GoodFor_Engagement"),
            GoodFor_FamilyFunctions: getCheckboxValue("GoodFor_FamilyFunctions"),
            GoodFor_Communion: getCheckboxValue("GoodFor_Communion"),
            GoodFor_BoardMeetings: getCheckboxValue("GoodFor_BoardMeetings"),
            GoodFor_ProductLaunch: getCheckboxValue("GoodFor_ProductLaunch"),
            GoodFor_OfficialEvent: getCheckboxValue("GoodFor_OfficialEvent"),

            // Seating arrangements (mapping Yes/No/N/A -> true/false/null)
            Seating_Cluster: document.getElementById("Cluster").value === "Yes",
            Seating_Theatre: document.getElementById("Theatre").value === "Yes",
            Seating_UShape: document.getElementById("UShape").value === "Yes",
            Seating_HollowSquare: document.getElementById("HollowSquare").value === "Yes",
            Seating_Boardroom: document.getElementById("Boardroom").value === "Yes",
            Seating_DanceFloor: document.getElementById("SeatingDanceFloor").value === "Yes",

            // Amenities (checkboxes)
            Amenity_ChangingRoom: getCheckboxValue("Amenities_ChangingRoom"),
            Amenity_AirCondition: getCheckboxValue("Amenities_AirCondition"),
            Amenity_CoupleChairs: getCheckboxValue("Amenities_CoupleChairs"),
            Amenity_Sofa: getCheckboxValue("Amenities_Sofa"),
            Amenity_MusicSystem: getCheckboxValue("Amenities_MusicSystem"),
            Amenity_Mic: getCheckboxValue("Amenities_Mic"),
            Amenity_Podium: getCheckboxValue("Amenities_Podium"),
            Amenity_Lights: getCheckboxValue("Amenities_Lights"),
            Amenity_FalseCeiling: getCheckboxValue("Amenities_FalseCeiling"),
            Amenity_Fans: getCheckboxValue("Amenities_Fans"),
            Amenity_Cloakroom: getCheckboxValue("Amenities_CloakRoom"),
            Amenity_Lockers: getCheckboxValue("Amenities_Lockers"),
            Amenity_Projectors: getCheckboxValue("Amenities_Projectors"),
            Amenity_WiFi: getCheckboxValue("Amenities_WiFi"),
            Amenity_Stage: getCheckboxValue("Amenities_Stage"),
            Amenity_WashRoom: getCheckboxValue("Amenities_Washroom"),
            Amenity_PlasmaScreen: getCheckboxValue("Amenities_PlasmaScreen"),
            Amenity_PowerBackup: getCheckboxValue("Amenities_PowerBackup"),
            Amenity_BoardroomSetup: getCheckboxValue("Amenities_BoardRoomSetup"),
            Amenity_WallClock: getCheckboxValue("Amenities_WallClock"),
            Amenity_ShoeRack: getCheckboxValue("Amenities_ShoeRack"),
            Amenity_RedCarpet: getCheckboxValue("Amenities_RedCarpet"),
            Is_Blocked: false,
            CapacityWithPartition_Id: document.getElementById("HallCapacity").value,
            RentWithPartition: parseFloat(document.getElementById("HallRent").value || 0),

            IsHallMerged: $("#HallMergeToggle").is(":checked"),
            MergeHall_JSON: mergeHallList,

            Action_By: userId // Example user id
        };
        console.log("hall requestData:", requestData);
        //showLoader();

        // ✅ AJAX Call
        $.ajax({
            url: "/api/Company_HallDetails/SaveCompanyHallDetails",
            type: "POST",
            data: JSON.stringify(requestData),
            contentType: "application/json",
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess("Saved!");
                    resetHallRentForm();
                    fetchHallDetails();
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message || 'Failed to save terms and conditions'
                    });
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while saving CompanyHallDetails');
            }
        });
    });
});

function resetHallRentForm() {
    // Reset text inputs, hidden fields, numbers, and URLs
    document.querySelectorAll('#hallRentForm input[type="text"], #hallRentForm input[type="hidden"], #hallRentForm input[type="number"], #hallRentForm input[type="url"]').forEach(input => {
        input.value = input.type === "hidden" ? "0" : "";
    });

    // Reset selects
    document.querySelectorAll('#hallRentForm select').forEach(select => {
        select.selectedIndex = 0; // Reset to first option
    });

    // Reset partition toggle
    const partitionToggle = document.getElementById("PartitionToggle");
    const partitionLabel = document.getElementById("partitionLabel");
    if (partitionToggle) {
        partitionToggle.checked = false;
        partitionLabel.textContent = "No";
    }

    resetHallMergeSection();

    // Reset checkboxes, icons, and active class
    document.querySelectorAll('#hallRentForm .option-item').forEach(item => {
        const checkbox = item.querySelector('.option-checkbox');
        const icon = item.querySelector('.facility-icon');

        // reset checkbox
        if (checkbox) checkbox.checked = false;

        // reset icon color
        if (icon) icon.style.color = "gray";

        // remove active class
        item.classList.remove("active");
    });

    // Reset file inputs
    document.querySelectorAll('#hallRentForm input[type="file"]').forEach(fileInput => {
        fileInput.value = "";
    });
    resetHallPreviews();
}

// RESET HALL MERGE SECTION
function resetHallMergeSection() {

    // SWITCH OFF
    $("#HallMergeToggle")
        .prop("checked", false);

    // LABEL
    $("#hallMergeLabel")
        .text("No");

    // HIDE SECTION
    $(".hall-merge-fields")
        .addClass("d-none");

    // CLEAR CHECKBOX LIST
    $("#mergeHallContainer")
        .html("");
}

// Function to fetch hall details
var hallList = null;
function fetchHallDetails() {
    //showLoader();
    $.ajax({
        url: '/api/Company_HallDetails/GetCompanyHallDetailsList',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0 }), // Get all halls
        success: function (response) {
            //console.log("GetCompanyHallDetailsList res:", response);
            if (response.status === "Success" && response.data) {
                populateHallTable(response.data);

                hallList = response.data; //.filter(x => !x.isHallMerged);
                //console.log("Filtered Hall List:", hallList);
            } else {
                //console.error('Failed to fetch hall details:', response.message);
                // Swal.fire({
                //     icon: 'error',
                //     title: 'Error',
                //     text: response?.message || 'Failed to fetch hall details.'
                // });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to fetch CompanyHallDetailsList');
        }
    });
}

// Function to populate the table
function populateHallTable(halls) {
    const tableBody = document.getElementById('hallDetailsTableBody');
    tableBody.innerHTML = ''; // Clear existing rows

    if (!halls || halls.length === 0) {
        const row = document.createElement('tr');
        row.innerHTML = `
                <td colspan="9" class="text-center text-muted py-3">
                    No hall data found
                </td>
            `;
        tableBody.appendChild(row);
        return;
    }

    halls.forEach((hall, index) => {
        const row = document.createElement('tr');

        // Count media items
        const promoVideosCount = [hall.PromoVideo1, hall.PromoVideo2].filter(v => v).length;
        const has360Video = hall.Video360Link ? 1 : 0;
        const imagesCount = [hall.Image1, hall.Image2, hall.Image3, hall.Image4, hall.Image5].filter(i => i).length;

        row.innerHTML = `
                <th scope="row">${index + 1}</th>
                <td>${hall.HallName}</td>
                <td>${hall.CapacityRangeText}</td>
                <td>${hall.RentPerDay}</td>
                <td><small class="me-1">🎥</small>${promoVideosCount}</td>
                <td><small class="me-1">🌙</small>${has360Video}</td>
                <td><small class="me-1">🖼️</small>${imagesCount}</td>
                <td>
                    <button class="btn btn-xs btn-outline-warning edit-hall-btn" data-hall='${JSON.stringify(hall)}'>
                        <i class="bi bi-pen"></i> Edit
                    </button>
                    <button class="btn btn-xs btn-outline-danger delete-hall-btn" data-hall='${JSON.stringify(hall)}'>
                        <i class="bi bi-trash"></i> Delete
                    </button>
                </td>
                <td>
                    <div class="form-check form-switch">
                        <input class="form-check-input block-switch" type="checkbox"
                                id="block${hall.Id}" ${hall.Is_Blocked ? 'checked' : ''}
                                data-hall-id="${hall.Id}"/>
                        <label class="form-check-label" for="block${hall.Id}"></label>
                    </div>
                </td>
            `;

        tableBody.appendChild(row);
    });

    // EDIT button click
    $(document).on("click", ".edit-hall-btn", function () {
        let hall = $(this).data("hall"); // parse hall JSON object
        console.log("edit hall:", hall);

        loadHallImages(hall);

        // Fill form fields
        $("#HallId").val(hall.Id);
        $("#HallName").val(hall.HallName);
        $("#HallCapacity").val(hall.CapacityRange);
        $("#HallRent").val(hall.RentPerDay);
        $("#PartitionToggle").prop("checked", hall.IsPartitioned == true);

        $("#HallCapacity_WithPartition").val(hall.CapacityWithPartition_Id);
        $("#HallRent_WithPartition").val(hall.RentWithPartition);

        $("#Video360Link").val(hall.Video360Link);
        $("#PromoVideo1").val(hall.PromoVideo1);
        $("#PromoVideo2").val(hall.PromoVideo2);

        // HALL MERGE
        $("#HallMergeToggle").prop("checked", hall.IsHallMerged == true).trigger("change");

        // KEEP SELECTED AFTER CHECKBOXES RENDER
        if (hall.IsHallMerged == true &&
            hall.MergeHall_List &&
            hall.MergeHall_List.length > 0) {

            setTimeout(() => {
                hall.MergeHall_List.forEach(item => {
                    $(`#mergeHall_${item.Hall_Id}`).prop("checked", true);
                });
            }, 300);
        }

        // ✅ Good For checkboxes
        //setGoodForOptions(hall);
        // ✅ Example calls
        setOptionGroup(hall, "GoodFor", "GoodFor", [
            "GrandWedding", "Weddings", "BirthdayParty", "Engagement", "FamilyFunctions",
            "Communion", "BoardMeetings", "ProductLaunch", "OfficialEvent"
        ]);
        // $("#GoodFor_GrandWedding").prop("checked", hall.GoodFor_GrandWedding == true);
        // $("#GoodFor_Weddings").prop("checked", hall.GoodFor_Weddings == true);
        // $("#GoodFor_BirthdayParty").prop("checked", hall.GoodFor_BirthdayParty == true);
        // $("#GoodFor_Engagement").prop("checked", hall.GoodFor_Engagement == true);
        // $("#GoodFor_FamilyFunctions").prop("checked", hall.GoodFor_FamilyFunctions == true);
        // $("#GoodFor_Communion").prop("checked", hall.GoodFor_Communion == true);
        // $("#GoodFor_BoardMeetings").prop("checked", hall.GoodFor_BoardMeetings == true);
        // $("#GoodFor_ProductLaunch").prop("checked", hall.GoodFor_ProductLaunch == true);
        // $("#GoodFor_OfficialEvent").prop("checked", hall.GoodFor_OfficialEvent == true);

        // ✅ Seating dropdowns (set Yes/No/N/A)
        $("#Cluster").val(hall.Seating_Cluster ? "Yes" : "No");
        $("#Theatre").val(hall.Seating_Theatre ? "Yes" : "No");
        $("#UShape").val(hall.Seating_UShape ? "Yes" : "No");
        $("#HollowSquare").val(hall.Seating_HollowSquare ? "Yes" : "No");
        $("#Boardroom").val(hall.Seating_Boardroom ? "Yes" : "No");
        $("#SeatingDanceFloor").val(hall.Seating_DanceFloor ? "Yes" : "No");

        // ✅ Amenities checkboxes
        setOptionGroup(hall, "Amenities", "Amenity", [
            "ChangingRoom", "AirCondition", "CoupleChairs", "Sofa", "MusicSystem", "Mic", "Podium",
            "Lights", "FalseCeiling", "Fans", "Cloakroom", "Lockers", "Projectors", "WiFi",
            "Stage", "WashRoom", "PlasmaScreen", "PowerBackup", "BoardroomSetup", "WallClock",
            "ShoeRack", "RedCarpet"
        ]);

        // $("#Amenities_ChangingRoom").prop("checked", hall.Amenity_ChangingRoom == true);
        // $("#Amenities_AirCondition").prop("checked", hall.Amenity_AirCondition == true);
        // $("#Amenities_CoupleChairs").prop("checked", hall.Amenity_CoupleChairs == true);
        // $("#Amenities_Sofa").prop("checked", hall.Amenity_Sofa == true);
        // $("#Amenities_MusicSystem").prop("checked", hall.Amenity_MusicSystem == true);
        // $("#Amenities_Mic").prop("checked", hall.Amenity_Mic == true);
        // $("#Amenities_Podium").prop("checked", hall.Amenity_Podium == true);
        // $("#Amenities_Lights").prop("checked", hall.Amenity_Lights == true);
        // $("#Amenities_FalseCeiling").prop("checked", hall.Amenity_FalseCeiling == true);
        // $("#Amenities_Fans").prop("checked", hall.Amenity_Fans == true);
        // $("#Amenities_Cloakroom").prop("checked", hall.Amenity_Cloakroom == true);
        // $("#Amenities_Lockers").prop("checked", hall.Amenity_Lockers == true);
        // $("#Amenities_Projectors").prop("checked", hall.Amenity_Projectors == true);
        // $("#Amenities_WiFi").prop("checked", hall.Amenity_WiFi == true);
        // $("#Amenities_Stage").prop("checked", hall.Amenity_Stage == true);
        // $("#Amenities_WashRoom").prop("checked", hall.Amenity_WashRoom == true);
        // $("#Amenities_PlasmaScreen").prop("checked", hall.Amenity_PlasmaScreen == true);
        // $("#Amenities_PowerBackup").prop("checked", hall.Amenity_PowerBackup == true);
        // $("#Amenities_BoardroomSetup").prop("checked", hall.Amenity_BoardroomSetup == true);
        // $("#Amenities_WallClock").prop("checked", hall.Amenity_WallClock == true);
        // $("#Amenities_ShoeRack").prop("checked", hall.Amenity_ShoeRack == true);
        // $("#Amenities_RedCarpet").prop("checked", hall.Amenity_RedCarpet == true);

        // ✅ Save hall id for update mode
        $("#hallRentForm").data("edit-id", hall.Id);

        // Scroll to form for editing (optional UX)
        $("html, body").animate({
            scrollTop: $("#hallRentForm").offset().top
        }, 600);
    });

    $(document).on("click", ".delete-hall-btn", function () {
        let hall = $(this).data("hall"); // parse hall JSON object
        let hallId = hall.Id;
        console.log("edit hall:", hall);

        Swal.fire({
            title: "Are you sure?",
            text: `You want to delete "${hall.HallName}"?`,
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#6c757d",
            confirmButtonText: "Yes, delete it!",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                //showLoader();
                var payload = {
                    Id: hallId,
                    ActionBy: Number(sessionStorage.getItem('userId') || 0)
                };

                $.ajax({
                    url: `/api/Company_HallDetails/DeleteCompanyHallDetails`,
                    type: "post",
                    contentType: "application/json",
                    data: JSON.stringify(payload),  // passing ID in body
                    success: function (response) {
                        handleApiSuccess("Deleted");
                        $(`#block${hallId}`).closest("tr").remove();
                    },
                    error: function (xhr, status, error) {
                        handleApiError(xhr, status, error, 'Error while deleting Company Hall');
                    }
                });
            }
        });
    });

    $(document).on("change", ".block-switch", function () {
        const checkbox = $(this);

        const requestData = {
            Id: checkbox.data("hall-id"),
            Is_Blocked: checkbox.is(":checked"),
            ActionBy: Number(sessionStorage.getItem('userId') || 0)
        };

        // Optional: disable switch during API call
        checkbox.prop("disabled", true);

        $.ajax({
            url: "/api/Company_HallDetails/SaveCompanyHallDetails_IsBlocked",
            type: "POST",
            data: JSON.stringify(requestData),
            contentType: "application/json",
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess("Hall status updated");
                }
                else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: response.message || 'Failed to update hall status'
                    });

                    // ❗ revert toggle if failed
                    checkbox.prop("checked", !checkbox.is(":checked"));
                }
            },

            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, 'Error while updating hall status');

                // ❗ revert toggle on error
                checkbox.prop("checked", !checkbox.is(":checked"));
            },
            complete: function () {
                checkbox.prop("disabled", false);
            }
        });
    });
}

// HALL MERGE TOGGLE
$("#HallMergeToggle").change(function () {
    let isChecked = $(this).is(":checked");
    $("#hallMergeLabel").text(isChecked ? "Yes" : "No");
    $(".hall-merge-fields").toggleClass("d-none", !isChecked);

    // LOAD HALLS
    if (isChecked) {
        renderMergeHallCheckboxes();
    }
    else {
        $("#mergeHallContainer").html("");
    }
});
// RENDER NON-MERGED HALLS
function renderMergeHallCheckboxes() {
    let html = "";
    // current editing hall id
    let currentHallId = $("#HallId").val() || 0;

    console.log("hallList render1:", hallList);
    // only non-merged halls
    let filteredHalls = hallList.filter(x =>
        !x.IsHallMerged &&
        x.Id != currentHallId
    );
    console.log("hallList render2:", filteredHalls);

    if (filteredHalls.length === 0) {
        html = `<div class="text-muted">No halls available</div>`;
    }
    else {
        filteredHalls.forEach(hall => {
            html += `
                <div class="form-check mb-2">

                    <input class="form-check-input merge-hall-checkbox"
                           type="checkbox"
                           value="${hall.Id}"
                           id="mergeHall_${hall.Id}" />

                    <label class="form-check-label" for="mergeHall_${hall.Id}">
                        ${hall.HallName}
                    </label>
                </div>
            `;
        });
    }

    $("#mergeHallContainer").html(html);
}


// ✅ Convert file input to Base64
function getBase64(file) {
    return new Promise((resolve, reject) => {
        if (!file) {
            resolve(null);
            return;
        }
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result.split(',')[1]); // only base64 string
        reader.onerror = error => reject(error);
        reader.readAsDataURL(file);
    });
}

function getCheckboxValue(id) {
    return document.getElementById(id)?.checked || false;
}

//for edit
function setOptionGroup(hall, prefix, fieldPrefix, options) {
    options.forEach(opt => {
        let checkbox = $(`#${prefix}_${opt}`);
        let item = checkbox.closest(".option-item");
        let icon = item.find(".facility-icon");

        let isChecked = hall[`${fieldPrefix}_${opt}`] === true;
        checkbox.prop("checked", isChecked);

        if (isChecked) {
            item.addClass("active");
            icon.css("color", "#ff0055");
        } else {
            item.removeClass("active");
            icon.css("color", "gray");
        }
    });
}

// Show preview on file select
function previewHallImage(input) {
    if (input.files && input.files[0]) {
        let reader = new FileReader();
        reader.onload = function (e) {
            input.nextElementSibling.src = e.target.result; // set preview image
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Open modal with full image
function openModal(img) {
    document.getElementById('modalImage').src = img.src;
    new bootstrap.Modal(document.getElementById('imageModal')).show();
}

// Populate previews when editing hall
function loadHallImages(hall) {
    let images = [hall.Image1, hall.Image2, hall.Image3, hall.Image4, hall.Image5];
    let previews = document.querySelectorAll('#hallImageContainer .preview-img');

    previews.forEach((img, i) => {
        if (images[i]) {
            img.src = images[i]; // assign existing hall image
        } else {
            img.src = "/assets2/img/image_icon.png"; // fallback default
        }
    });
}

function resetHallPreviews() {
    document.querySelectorAll(".preview-img").forEach(img => {
        img.src = "/assets2/img/image_icon.png"; // reset to default icon
    });

    document.querySelectorAll(".image-upload input[type='file']").forEach(input => {
        input.value = ""; // clear file input
    });
}


//<!-- TAB 3: Hall Rent & Details -- END -->

//<!-- TAB 4: Invoice Setup -- START -->

document.addEventListener('DOMContentLoaded', function () {
    // Show/hide custom receipt number field based on checkbox
    document.getElementById('UseCustomReceiptNo').addEventListener('change', function () {
        document.getElementById('customReceiptNoContainer').style.display =
            this.checked ? 'block' : 'none';
    });

    // Show/hide bank details based on checkbox
    document.getElementById("PrintBankDetails").addEventListener("change", function () {
        const container = document.getElementById("bankDetailsContainer");
        container.style.display = this.checked ? "flex" : "none";
    });

    // Preview logo when file is selected
    document.getElementById('CompanyLogoUpload').addEventListener('change', function (event) {
        const preview = document.getElementById('logoPreview');
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                preview.src = e.target.result;
            };
            reader.readAsDataURL(file);
        }
    });

    loadInvoiceSetupData();
});
var invoiceSetupId = 0;

function loadInvoiceSetupData() {
    //showLoader();

    $.ajax({
        url: `/api/Company_InvoiceSetup/GetCompanyInvoiceSetup`,
        type: "GET",
        success: function (res) {
            //console.log("res invoiceSetup:", res);
            if (res.status === "Success" && res.data && res.data.length > 0) {
                const setup = res.data[0]; // assuming only one setup record
                //console.log("invoice setup fetched:", setup);

                invoiceSetupId = setup.Id || 0;

                // GST Invoice
                $("#GSTInvoiceStartNo").val(setup.GstInvoiceStartNo || "");
                $("#GSTInvoicePrefix").val(setup.GstInvoicePrefix || "");

                // Proforma Invoice
                $("#ProformaInvoiceStartNo").val(setup.ProformaInvoiceStartNo || "");
                $("#ProformaInvoicePrefix").val(setup.ProformaInvoicePrefix || "");

                // Receipt
                $("#ReceiptPrefix").val(setup.ReceiptPrefix || "");

                // Checkboxes
                $("#PrintLogoProspectus").prop("checked", setup.PrintLogoProspectus);
                $("#PrintLogoQuotation").prop("checked", setup.PrintLogoQuotation);
                $("#PrintLogoReceipt").prop("checked", setup.PrintLogoReceipt);
                $("#IncludeFYInvoiceNo").prop("checked", setup.IncludeFyInvoiceNo);
                $("#UseCustomReceiptNo").prop("checked", setup.UseCustomReceiptNo);
                $("#PrintFPPaymentTrail").prop("checked", setup.PrintFpPaymentTrail);
                $("#PrintBankDetails").prop("checked", setup.PrintBankDetails);

                // Custom Receipt Start No. container toggle
                if (setup.UseCustomReceiptNo) {
                    $("#customReceiptNoContainer").show();
                    $("#CustomReceiptStartNo").val(setup.CustomReceiptStartNo || "");
                } else {
                    $("#customReceiptNoContainer").hide();
                }

                // Signatory Label
                $("#signatoryLabelSelect").val(setup.SignatoryLabel || "1");

                // Bank Details container toggle
                if (setup.PrintBankDetails) {
                    $("#bankDetailsContainer").show();
                } else {
                    $("#bankDetailsContainer").hide();
                }
                $("#BankName").val(setup.BankName || "");
                $("#AccountNo").val(setup.AccountNo || "");
                $("#BranchDetails").val(setup.BranchDetails || "");
                $("#IFSCCode").val(setup.IFSCCode || "");
                $("#UPIId").val(setup.UpiId || "");

                // Company Logo
                if (setup.CompanyLogo) {
                    $("#logoPreview").attr("src", setup.CompanyLogo);
                } else {
                    $("#logoPreview").attr("src", "/assets2/img/image_icon.png");
                }

            } else {
                //Swal.fire("Info", "No invoice setup found.", "info");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load invoice setup');
        }
    });
}

async function saveInvoiceSetupData() {
    let fileInput = document.getElementById("CompanyLogoUpload");
    let file = fileInput.files.length > 0 ? fileInput.files[0] : null;

    let logoBase64 = "";
    if (file) {
        logoBase64 = await getBase64(file);  // same helper as in your company profile code
    }
    saveInvoiceSetupToApi(logoBase64);
}

function saveInvoiceSetupToApi(companyLogo) {
    const payload = {
        Id: parseInt(invoiceSetupId) || 0,

        GstInvoiceStartNo: parseInt($("#GSTInvoiceStartNo").val()) || 0,
        GstInvoicePrefix: $("#GSTInvoicePrefix").val(),
        ProformaInvoiceStartNo: parseInt($("#ProformaInvoiceStartNo").val()) || 0,
        ProformaInvoicePrefix: $("#ProformaInvoicePrefix").val(),

        ReceiptPrefix: $("#ReceiptPrefix").val(),
        PrintLogoProspectus: $("#PrintLogoProspectus").is(":checked"),
        PrintLogoQuotation: $("#PrintLogoQuotation").is(":checked"),
        PrintLogoReceipt: $("#PrintLogoReceipt").is(":checked"),
        IncludeFyInvoiceNo: $("#IncludeFYInvoiceNo").is(":checked"),

        UseCustomReceiptNo: $("#UseCustomReceiptNo").is(":checked"),
        CustomReceiptStartNo: parseInt($("#CustomReceiptStartNo").val()) || 0,
        PrintFpPaymentTrail: $("#PrintFPPaymentTrail").is(":checked"),
        SignatoryLabel: parseInt($("#signatoryLabelSelect").val()) || 0,
        PrintBankDetails: $("#PrintBankDetails").is(":checked"),

        BankName: $("#BankName").val(),
        AccountNo: $("#AccountNo").val(),
        BranchDetails: $("#BranchDetails").val(),
        IFSCCode: $("#IFSCCode").val(),
        UpiId: $("#UPIId").val(),

        CompanyLogo: companyLogo,  // make sure backend model has this property
        ActionBy: Number(sessionStorage.getItem('userId') || 0)
    };

    console.log("invoice setup payload:", payload);
    //showLoader();
    $.ajax({
        url: `/api/Company_InvoiceSetup/SaveCompanyInvoiceSetup`,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                handleApiSuccess("Saved");
                loadInvoiceSetupData();

                if (res.data && res.data.companylogo) {
                    sessionStorage.setItem('companylogo', res.data.companylogo);
                    window.location.reload();
                }
            } else {
                Swal.fire("Error", res.message || "Failed to save data", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to save CompanyInvoiceSetup');
        }
    });
}


//<!-- TAB 4: Invoice Setup -- END -->

//<!-- TAB 5: Food Rate & Services > SubTab 5.1: Ad/Edit Food Items -- START -->

// =============================
// TAB: Add/Edit Food Items
// =============================
let headerCount_items = 0;
let inclusionOptions = [];

function toggleHeaderItems(headerId, btn) {
    const container = document.getElementById(headerId);
    if (!container) return;

    // Determine toggle action based on icon
    const isExpanded = btn.textContent.trim() === "▼";

    if (isExpanded) {
        // 🔹 Collapse
        container.style.maxHeight = container.scrollHeight + "px";
        requestAnimationFrame(() => {
            container.style.maxHeight = "0px";
        });
        btn.textContent = "◀";
    } else {
        // 🔹 Expand
        container.style.display = "block";
        container.style.maxHeight = container.scrollHeight + "px";
        btn.textContent = "▼";
    }
}
function addNewHeader_items(headerDbId = 0) {
    headerCount_items++;

    const headerId = `header-item-${headerCount_items}`;

    const container = document.createElement('div');
    container.className = 'border rounded mb-3 bg-white shadow-sm';

    container.innerHTML = `
        <input type="hidden" class="food-header-id" value="${headerDbId}" />

        <div class="d-flex justify-content-between align-items-center px-3 py-2 rounded-top" style="background-color: #e9f5ff; margin: 2 -0.5rem; border-bottom: 1px solid #cce5ff;">
            <div class="d-flex flex-grow-1 align-items-center">
                <input type="text" class="form-control form-control-sm me-2 food-item-header fw-semibold border-0 bg-transparent" placeholder="Category Name (e.g. Welcome Drink)" />
            </div>

            <div>
                <button type="button" class="btn btn-sm btn-outline-primary me-2 toggle-arrow" onclick="toggleHeaderItems('${headerId}', this)">▼</button>
                <button type="button" class="btn btn-sm btn-outline-danger border fw-bold" onclick="deleteFoodItem(${headerDbId}, this)">X</button>
            </div>
        </div>

        <div class="mt-2 mb-2 px-2" id="${headerId}" style="max-height:1200px; overflow-y:auto;">
            ${generateFoodItemRow_items()}

            <div class="add-food-item-wrapper mt-2">
                <button type="button" class="btn btn-outline-success btn-sm" onclick="addFoodItem_items('${headerId}')">+ Add Food SubCategory</button>
            </div>
        </div>
    `;

    document.getElementById('foodHeaderContainer_items').appendChild(container);
}
function generateFoodItemRow_items(foodItemDbId = 0) {
    const foodItemUniqueId = `food-item-${Date.now()}-${Math.floor(Math.random() * 100000)}`;

    return `
        <div class="food-item-wrapper border rounded p-2 mb-2 bg-light">
            <div class="input-group food-item-row">
                <input type="hidden" class="food-item-id" value="${foodItemDbId}" />

                <input type="text" class="form-control food-item-name" placeholder="Food Subcategory Name" />

                <input type="text" class="form-control food-item-description ms-2" placeholder="Description (optional)" />

                <button type="button" class="btn btn-outline-primary ms-2" onclick="addFoodChildItem_items('${foodItemUniqueId}')">+ Add Food Item</button>

                <button type="button" class="btn btn-outline-danger ms-2" onclick="deleteFoodItem_Items(this)">X</button>
            </div>

            <div id="${foodItemUniqueId}" class="food-child-container mt-2 ms-4"></div>
        </div>
    `;
}
function addFoodItem_items(headerId) {
    const container = document.getElementById(headerId);
    if (!container) return;

    const addFoodItemWrapper = container.querySelector('.add-food-item-wrapper');

    if (!addFoodItemWrapper) {
        console.error('Add Food Item wrapper not found:', headerId);
        return;
    }

    addFoodItemWrapper.insertAdjacentHTML('beforebegin', generateFoodItemRow_items());
}
function generateFoodChildItemRow_items(childDbId = 0) {
    return `
        <div class="input-group mb-2 food-child-item-row">
            <input type="hidden" class="food-child-item-id" value="${childDbId}" />

            <span class="input-group-text">↳</span>

            <input type="text" class="form-control food-child-item-name" placeholder="Food Item Name" />

            <input type="text" class="form-control food-child-item-description ms-2" placeholder="Description (optional)" />

            <button type="button" class="btn btn-outline-danger ms-2" onclick="deleteFoodItem_Items(this)">X</button>
        </div>
    `;
}
function addFoodChildItem_items(parentFoodItemId) {
    const container = document.getElementById(parentFoodItemId);
    if (!container) return;

    container.insertAdjacentHTML('beforeend', generateFoodChildItemRow_items());
}

function saveFoodItems() {
    //console.log("saveFoodItems clicked..");

    // Build cuisine dictionary
    let cuisines = {};
    $("#cuisineCheckboxContainer input[type=checkbox]").each(function () {
        cuisines[$(this).val()] = $(this).is(":checked");
    });

    userId = Number(sessionStorage.getItem('userId') || 0);

    // 🔧 Collect Food Items for Save
    let foodItems = [];

    $("#foodHeaderContainer_items > .border").each(function () {
        const header = $(this);

        const headerId = parseInt(header.find("> .food-header-id").val()) || 0;
        const headerName = (header.find("> div:first-of-type .food-item-header").val() || "").trim();

        const items = [];

        // 🔹 Get only parent food items directly under the header item container
        header.find('[id^="header-item-"] > .food-item-wrapper').each(function () {
            const itemWrapper = $(this);

            const itemRow = itemWrapper.find("> .food-item-row");

            const itemId = parseInt(itemRow.find(".food-item-id").val()) || 0;
            const itemName = (itemRow.find(".food-item-name").val() || "").trim();
            const description = (itemRow.find(".food-item-description").val() || "").trim();

            const children = [];

            // 🔸 Get only children belonging to this parent item
            itemWrapper.find("> .food-child-container > .food-child-item-row").each(function () {
                const childRow = $(this);

                const childId = parseInt(childRow.find(".food-child-item-id").val()) || 0;
                const childName = (childRow.find(".food-child-item-name").val() || "").trim();
                const childDescription = (childRow.find(".food-child-item-description").val() || "").trim();

                // Skip empty child item
                if (!childName && !childDescription) return;

                children.push({
                    ItemId: childId,
                    ItemName: childName,
                    Description: childDescription
                });
            });

            // Skip completely empty parent item
            if (!itemName && !description && children.length === 0) return;

            items.push({
                ItemId: itemId,
                ItemName: itemName,
                Description: description,
                Children: children
            });
        });

        // Skip completely empty header
        if (!headerName && items.length === 0) return;

        foodItems.push({
            Id: headerId,
            HeaderName: headerName,
            Items: items
        });
    });

    //console.log(foodItems);

    let requestData = {
        FoodItems: foodItems,
        ActionBy: Number(sessionStorage.getItem('userId') || 0),
        Cuisines: cuisines
    };

    console.log("SaveCompanyFoodItems payload:", requestData);

    $.ajax({
        url: "/api/Company_FoodRate_Item/SaveCompanyFoodItems",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (res) {
            console.log("save res:", res);
            if (res.status === "Success") {
                handleApiSuccess("Saved!");
                loadFoodItems();
                //loadAddonCharges(); //here2
            } else {
                Swal.fire({ icon: 'error', title: 'Oops...', text: res.message });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Error Saving CompanyFoodItems');
        }
    });
}

function loadFoodItems(itemId = 0) {
    headerCount_items = 0;
    inclusionOptions = [];

    $.ajax({
        url: "/api/Company_FoodRate_Item/GetCompanyFoodItem",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: itemId }),
        success: function (res) {
            //console.log("Company_FoodRate_Item res:", res);
            if (res.status === "Success" && res.data) {
                // ---------- Render cuisines ----------
                if (res.data.Cuisines) {
                    const container = document.getElementById("cuisineCheckboxContainer");
                    container.innerHTML = ""; // clear old ones

                    Object.entries(res.data.Cuisines).forEach(([cuisine, isActive]) => {
                        const col = document.createElement("div");
                        col.className = "form-check col";

                        const checkbox = document.createElement("input");
                        checkbox.type = "checkbox";
                        checkbox.className = "form-check-input";
                        checkbox.id = cuisine.toLowerCase().replace(/\s+/g, '');
                        checkbox.name = "cuisine";
                        checkbox.value = cuisine;
                        checkbox.checked = isActive;

                        const label = document.createElement("label");
                        label.className = "form-check-label";
                        label.setAttribute("for", checkbox.id);
                        label.innerText = cuisine;

                        col.appendChild(checkbox);
                        col.appendChild(label);
                        container.appendChild(col);
                    });
                }

                // ---------- Render food items ----------
                if (res.data.FoodItems && res.data.FoodItems.length > 0) {
                    const headerContainer = document.getElementById('foodHeaderContainer_items');

                    headerContainer.innerHTML = '';

                    res.data.FoodItems.forEach((header, index) => {
                        addNewHeader_items(header.Id || 0);

                        const lastHeaderContainer = headerContainer.lastElementChild;

                        lastHeaderContainer.querySelector('.food-item-header').value = header.HeaderName || '';

                        const itemsDiv = lastHeaderContainer.querySelector('[id^="header-item-"]');

                        itemsDiv.innerHTML = '';

                        if (header.Items && header.Items.length > 0) {
                            header.Items.forEach(foodItem => {
                                const foodItemUniqueId = `food-item-${Date.now()}-${Math.floor(Math.random() * 100000)}`;

                                itemsDiv.insertAdjacentHTML('beforeend', `
                                    <div class="food-item-wrapper border rounded p-2 mb-2 bg-light">
                                        <div class="input-group food-item-row">
                                            <input type="hidden" class="food-item-id" value="${foodItem.ItemId || 0}" />

                                            <input type="text" class="form-control food-item-name" placeholder="Food Subcategory Name" value="${foodItem.ItemName || ''}" />

                                            <input type="text" class="form-control food-item-description ms-2" placeholder="Description (optional)" value="${foodItem.Description || ''}" />

                                            <button type="button" class="btn btn-outline-primary ms-2" onclick="addFoodChildItem_items('${foodItemUniqueId}')">+ Add Food Item</button>

                                            <button type="button" class="btn btn-outline-danger ms-2" onclick="deleteFoodItem_Items(this)">X</button>
                                        </div>

                                        <div id="${foodItemUniqueId}" class="food-child-container mt-2 ms-4"></div>
                                    </div>
                                `);

                                const childContainer = document.getElementById(foodItemUniqueId);

                                if (foodItem.Children && foodItem.Children.length > 0) {
                                    foodItem.Children.forEach(child => {
                                        childContainer.insertAdjacentHTML('beforeend', `
                                            <div class="input-group mb-2 food-child-item-row">
                                                <input type="hidden" class="food-child-item-id" value="${child.ItemId || 0}" />

                                                <span class="input-group-text">↳</span>

                                                <input type="text" class="form-control food-child-item-name" placeholder="Food Item Name" value="${child.ItemName || ''}" />

                                                <input type="text" class="form-control food-child-item-description ms-2" placeholder="Description (optional)" value="${child.Description || ''}" />

                                                <button type="button" class="btn btn-outline-danger ms-2" onclick="deleteFoodItem_Items(this)">X</button>
                                            </div>
                                        `);
                                    });
                                }
                            });
                        }

                        itemsDiv.insertAdjacentHTML('beforeend', `
                            <div class="add-food-item-wrapper mt-2">
                                <button type="button" class="btn btn-outline-success btn-sm" onclick="addFoodItem_items('${itemsDiv.id}')">+ Add Food Subcategory</button>
                            </div>
                        `);

                        setTimeout(() => {
                            const headerId = itemsDiv.id;
                            const toggleBtn = lastHeaderContainer.querySelector('.toggle-arrow');

                            if (index === 0) {
                                if (toggleBtn) toggleBtn.textContent = '▼';
                            } else {
                                if (toggleBtn) {
                                    toggleHeaderItems(headerId, toggleBtn);
                                    toggleBtn.textContent = '◀';
                                }
                            }
                        }, 50);
                    });
                }
                inclusionOptions = res.data.FoodItems;
                loadFoodPackageData();
            } else {
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to fetch food items.');
        }
    });
}

function deleteFoodItem_Items(button) {
    const $wrapper = $(button).closest('.food-item-wrapper');
    const $parentRow = $(button).closest('.food-item-row');
    const $childRow = $(button).closest('.food-child-item-row');

    let id = 0;
    let $rowToRemove = null;

    if ($childRow.length > 0) {
        id = parseInt($childRow.find('.food-child-item-id').val()) || 0;
        $rowToRemove = $childRow;
    } else if ($parentRow.length > 0) {
        id = parseInt($parentRow.find('.food-item-id').val()) || 0;
        $rowToRemove = $wrapper;
    }

    if (id === 0) {
        $rowToRemove.remove();
        return;
    }

    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    const payload = {
        Id: id,
        DeletedBy: actionBy
    };

    $.ajax({
        url: '/api/Company_FoodRate_Item/DeleteCompanyFoodItem_Items',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (data) {
            if (data.status === "Success") {
                $rowToRemove.remove();
                handleApiSuccess(data.message || "Deleted!");
            } else {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: data.message || "Failed to delete food item.",
                    confirmButtonColor: "#d33"
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to delete food item.");
        }
    });
}

function deleteFoodItem(headerId, btn) {

    const id = headerId;
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    const payload = {
        Id: id,
        DeletedBy: actionBy
    };

    $.ajax({
        url: '/api/Company_FoodRate_Item/DeleteCompanyFoodItem',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (data) {
            if (data.status === "Success") {
                handleApiSuccess(data.message || "Deleted!");    
                loadFoodItems();
            }
            else {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: data.message,
                    confirmButtonColor: "#d33"
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to delete food item.");
        }
    });
}

//TAB: Food Rate & Services > SubTab: Add/Edit Food Items -- START
document.addEventListener("DOMContentLoaded", function () {
    loadFoodItems();
});

//<!-- TAB 5: Food Rate & Services > SubTab 5.1: Add/Edit Food Items -- END -->

//<!-- TAB 5: Food Rate & Services > SubTab 5.2: Setup/Update Food Packages -- START -->

// 🔁 Track package rows and inclusions (updated format)
let staticPackages = [];

function loadFoodPackageData() {
    //console.log("inclusionOptions in pkg:", inclusionOptions);
    var payload = {
        Id: 0  // send packageId (or null if all)
    };
    //showLoader();

    $.ajax({
        url: `/api/Company_FoodItem_Package/GetCompanyFoodPackage`,
        type: "POST", // must match backend
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("GetCompanyFoodPackage res:", res);
            if (res.status === "Success" && res.data && res.data.length > 0) {
                staticPackages = res.data; // assuming only one setup record
                const body = document.getElementById("packageTableBody");
                body.innerHTML = "";   // ✅ clears all rows
                rowCount = 0;
                staticPackages.forEach(pkg => {
                    body.insertAdjacentHTML("beforeend", renderPackageRow(pkg));
                });
            } else {
                //Swal.fire("Info", "No Company FoodPackage found.", "info");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load FoodPackage');
        }
    });
}

let rowCount = 0;
let currentEditIndex = null;

function renderPackageRow(pkg = { Id: 0, PackageName: "", RatePerPerson: "", InclusionList: [], PackageType: "Veg", MenuImagePath: "" }) {
    const id = `pkg-${rowCount}`;
    const index = rowCount++;

    //console.log("pkg.InclusionList:", pkg.InclusionList);
    //const inclusionCount = Object.keys(pkg.InclusionList && pkg.InclusionList.IsIncluded || {}).length;
    const inclusionCount = Array.isArray(pkg.InclusionList)
        ? pkg.InclusionList.filter(x => x.IsIncluded).length
        : 0;

    return `
            <tr>
                <td>
                    <div class="form-check form-check-inline">
                        <input class="form-check-input" type="radio" name="type-${id}" value="Veg" ${pkg.PackageType === 'Veg' ? 'checked' : ''}>
                        <label class="form-check-label">Veg</label>
                    </div>
                    <div class="form-check form-check-inline">
                        <input class="form-check-input" type="radio" name="type-${id}" value="NonVeg" ${pkg.PackageType === 'NonVeg' ? 'checked' : ''}>
                        <label class="form-check-label">Non Veg</label>
                    </div>
                </td>
                <td>
                    <input type="text" class="form-control" value="${pkg.PackageName}" placeholder="Package Name">
                </td>
                <td>
                    <input type="number" step="0.01" class="form-control" value="${pkg.RatePerPerson}" placeholder="Rate/PP">
                </td>
                <td>
                    <input type="hidden" class="inclusion-data" value='${JSON.stringify(Array.isArray(pkg.InclusionList) ? pkg.InclusionList : [])}'>
                    <button class="btn btn-outline-secondary btn-sm" onclick="openInclusionModal(${index})">
                        <span class="badge bg-success me-1" id="inclusion-count-${index}">${inclusionCount}</span>Update
                    </button>
                </td>
                <td>
                  <div class="d-flex align-items-center gap-2">
                    <input type="file" class="form-control" accept="image/*"
                           onchange="previewFoodPackageImage(event, '${id}', ${index})">
                    <img id="preview-${id}" src="${pkg.MenuImagePath || ''}"
                         alt="menu" class="img-thumbnail"
                         style="max-width: 60px; max-height: 60px; ${pkg.MenuImagePath ? '' : 'display:none;'} cursor:pointer;"
                         onclick="openModal(this)" />
                  </div>
                </td>
                <td class="text-center">
                    <button class="btn btn-outline-danger btn-sm" onclick="deleteCompanyFoodPackage(${pkg.Id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `;
}

// Render all rows initially
//document.addEventListener("DOMContentLoaded", function () {
//loadFoodPackageData();
//});

function addNewPackageRow() {
    const newPackage = {
        Id: 0,
        PackageName: "",
        RatePerPerson: "",
        PackageType: "Veg",
        InclusionList: [],
        MenuImagePath: ""
    };
    staticPackages.push(newPackage);
    const rowHTML = renderPackageRow(newPackage);
    document.getElementById("packageTableBody").insertAdjacentHTML("beforeend", rowHTML);
}

// Preview selected image + update internal data + show preview
function previewFoodPackageImage(event, id, index) {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = function (e) {
        const img = document.getElementById(`preview-${id}`);
        img.src = e.target.result;
        img.style.display = "inline-block";
    };
    reader.readAsDataURL(file);
}

// Inclusions Modal Logic
function openInclusionModal(index) {
    currentEditIndex = index;
    const container = document.getElementById("inclusionListContainer");
    container.innerHTML = "";

    // Ensure it's parsed correctly
    console.log("staticPackages:", staticPackages, ", index:", index);
    //console.log("index:", index);
    //console.log("staticPackages[index].InclusionList:", staticPackages[index].InclusionList);
    let currentInclusions = staticPackages[index].InclusionList || [];
    if (typeof currentInclusions === "string") {
        try {
            currentInclusions = JSON.parse(currentInclusions);
        } catch {
            currentInclusions = [];
        }
    }
    //console.log("1 index:",index,", currentInclusions:", currentInclusions);
    // 🔑 Normalize to array of {Name, IsIncluded}
    if (!Array.isArray(currentInclusions) && typeof currentInclusions === "object") {
        currentInclusions = Object.entries(currentInclusions).map(([fooditem_id, isIncluded, count_size]) => ({
            FoodItem_Id: fooditem_id,
            IsIncluded: isIncluded,
            Count_Size: count_size
        }));
    }
    //console.log("2 index:",index,", currentInclusions:", currentInclusions);
    // console.log("OPEN inclusionOptions:", inclusionOptions)
    // console.log("OPEN currentInclusions:", currentInclusions)
    inclusionOptions.forEach((option, i) => {
        //console.log("OPEN option:", option)
        const match = currentInclusions.find(
            inc => inc.FoodItem_Id === option.Id
        );
        const isChecked = match ? match.IsIncluded === true : false;
        const countSize = match ? (match.Count_Size || 0) : 0;

        // Build dropdown 1-10
        let dropdown = `<select class="form-select form-select-sm ms-auto w-auto" id="countsize-${i}">
                ${Array.from({ length: 10 }, (_, n) => {
            const val = n + 1;
            return `<option value="${val}" ${countSize === val ? "selected" : ""}>${val}</option>`;
        }).join("")}
            </select>`;

        container.insertAdjacentHTML("beforeend", `
                <div class="col-md-4 mb-2">
                    <div class="inclusion-item d-flex align-items-center justify-content-between border p-2 rounded">
                        <div class="form-check mb-0">
                            <input type="checkbox" class="form-check-input me-2" id="inclusion-${i}" value="${option.Id}" ${isChecked ? "checked" : ""}>
                            <label class="form-check-label" for="inclusion-${i}">${option.HeaderName}</label>
                        </div>
                        ${dropdown}
                    </div>
                </div>
            `);
    });

    // inclusionOptions.forEach((option, i) => {
    //     // Find if option exists in currentInclusions
    //     const match = currentInclusions.find(
    //         inc => inc.Name.toLowerCase() === option.toLowerCase()
    //     );
    //     const isChecked = match ? match.IsIncluded === true : false;

    //     container.insertAdjacentHTML("beforeend", `
    //         <div class="form-check col">
    //             <input type="checkbox" class="form-check-input" id="inclusion-${i}" value="${option}" ${isChecked ? "checked" : ""}>
    //             <label class="form-check-label" for="inclusion-${i}">${option}</label>
    //         </div>
    //     `);
    // });

    new bootstrap.Modal(document.getElementById("inclusionModal")).show();
}

function saveInclusions() {
    const checkboxes = document.querySelectorAll("#inclusionListContainer input[type='checkbox']");
    const selected = [];

    checkboxes.forEach((cb, idx) => {
        // find corresponding dropdown
        const dropdown = document.getElementById(`countsize-${idx}`);
        const countSize = dropdown ? parseInt(dropdown.value) : 0;

        selected.push({
            FoodItem_Id: parseInt(cb.value),
            IsIncluded: cb.checked,
            Count_Size: countSize
        });
    });

    console.log("inclusion selected:", selected);

    if (currentEditIndex !== null) {
        staticPackages[currentEditIndex].InclusionList = selected;

        // ✅ update hidden input so saveAllPackages reads latest inclusions
        const hiddenInput = document.querySelector(
            `#packageTableBody tr:nth-child(${currentEditIndex + 1}) .inclusion-data`
        );
        if (hiddenInput) {
            hiddenInput.value = JSON.stringify(selected);
        }

        // update badge count (# of checked inclusions)
        const includedCount = selected.filter(x => x.IsIncluded).length;
        document.getElementById(`inclusion-count-${currentEditIndex}`).innerText = includedCount;
    }

    bootstrap.Modal.getInstance(document.getElementById("inclusionModal")).hide();
}

async function saveAllPackages() {
    const tableBody = document.getElementById("packageTableBody");
    const rows = tableBody.querySelectorAll("tr");
    const payload = []; // ✅ Array, not object with "packages"

    for (let i = 0; i < rows.length; i++) {
        const row = rows[i];
        const typeInput = row.querySelector(`input[type="radio"][name^="type-"]:checked`);
        const nameInput = row.querySelector(`input[type="text"]`);
        const rateInput = row.querySelector(`input[type="number"]`);
        const fileInput = row.querySelector(`input[type="file"]`);
        const imagePreview = row.querySelector("img");

        const pkg = {
            id: staticPackages[i]?.Id || 0,
            packageType: typeInput?.value || "",   // will be "Veg" or "NonVeg"
            packageName: nameInput?.value?.trim() || "",
            ratePerPerson: parseFloat(rateInput?.value || 0),
            menuImagePath: null,
            ActionBy: Number(sessionStorage.getItem('userId') || 0),

            inclusionList: (() => {
                const hiddenInputs = document.querySelectorAll(".inclusion-data");
                const hiddenInput = hiddenInputs[i]; // match with row index
                if (!hiddenInput) return [];

                try {
                    const arr = JSON.parse(hiddenInput.value);
                    if (Array.isArray(arr)) {
                        return arr.map(item => ({
                            FoodItem_Id: item.FoodItem_Id,
                            IsIncluded: Boolean(item.IsIncluded),
                            Count_Size: item.Count_Size || 0
                        }));
                    }
                    return [];
                } catch (e) {
                    console.error("Inclusion parse error:", e, hiddenInput.value);
                    return [];
                }
            })()
        };

        if (fileInput?.files?.length > 0) {
            pkg.menuImagePath = await getBase64(fileInput.files[0]);
        } else {
            //pkg.menuImagePath = imagePreview?.src || null;
        }

        payload.push(pkg);
    }

    console.log("SaveCompanyFoodPackage Payload:", payload);

    //showLoader();
    $.ajax({
        url: `/api/Company_FoodItem_Package/SaveCompanyFoodPackage`,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload), // ✅ send array
        success: function (response) {
            console.log("SaveCompanyFoodPackage response:", response);
            handleApiSuccess("Saved");
            staticPackages = [];
            loadFoodPackageData();
            rowCount = 0;
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Error saving packages!');
        }
    });
}

function deleteCompanyFoodPackage(id) {
    Swal.fire({
        title: "Are you sure?",
        text: "This action will permanently delete the food package.",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Yes, delete it!",
        cancelButtonText: "Cancel",
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6"
    }).then((result) => {
        if (result.isConfirmed) {
            const requestData = {
                Id: id,
                ActionBy: Number(sessionStorage.getItem('userId') || 0)
            };
            console.log("requestData delete package:", requestData);

            //showLoader();  
            $.ajax({
                url: '/api/Company_FoodItem_Package/DeleteCompanyFoodPackage', // ✅ adjust route if needed
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(requestData),
                success: function (response) {
                    if (response.status === "Success") {
                        handleApiSuccess(response.message || "Deleted!");    
                        //$(`#row-${id}`).remove(); // remove row visually
                        loadFoodPackageData();
                    } else {
                        Swal.fire({
                            icon: "error",
                            title: "Failed!",
                            text: response.message
                        });
                    }
                },
                error: function (xhr, status, error) {
                    handleApiError(xhr, status, error, "Delete package failed");
                }
            });
        }
    });
}

//<!-- TAB 5: Food Rate & Services > SubTab 5.2: Setup/Update Food Packages -- END -->

//<!-- TAB 5: Food Rate & Services > SubTab 5.3: Add-on Food Charges -- START -->

// =============================
// TAB: Addon Food Charges
// =============================
let headerCount_charges = 0;

function toggleHeaderCharges(bodyId, btn) {
    const body = document.getElementById(bodyId);
    if (!body) return;

    if ($(body).is(':visible')) {
        $(body).stop(true, true).slideUp(250); // smooth collapse
        btn.textContent = '◀';
    } else {
        $(body).stop(true, true).slideDown(250); // smooth expand
        btn.textContent = '▼';
    }
}

function addNewHeader_charges(headerDbId) {
    headerCount_charges++;
    const headerId = `header-charge-${headerCount_charges}`;
    const container = document.createElement('div');
    //container.className = 'border rounded mb-3 p-2 bg-light';
    container.className = 'border rounded mb-3 bg-white shadow-sm addon-charge-row-header';

    container.innerHTML = `

            <div class="d-flex justify-content-between align-items-center px-3 py-2 rounded-top" 
                style="background-color: #e9f5ff; margin: 2 -0.5rem; border-bottom: 1px solid #cce5ff;">

                <div class="d-flex flex-grow-1 align-items-center">
                    <input type="text" class="form-control form-control-sm me-2 addon-charge-header fw-semibold border-0 bg-transparent" placeholder="Category Name (e.g. Service Charge)" />
                </div>
                <div>
                    <button class="btn btn-sm btn-outline-primary me-2 toggle-arrow-charges"
                            onclick="toggleHeaderCharges('${headerId}', this)">▼</button>
                    <button class="btn btn-sm btn-outline-danger border fw-bold" onclick="deleteAddonCharge_Header('${headerDbId}', this)">X</button>
                </div>
            </div>

            <div class="mt-2 mb-2 px-2" id="${headerId}">
                ${generateFoodItemRow_charges()}
                <div class="mt-2">
                    <button class="btn btn-success btn-sm mt-2" onclick="addFoodItem_charges('${headerId}')">+ Add more</button>
                </div>
            </div>
        `;
    document.getElementById('foodHeaderContainer_charges').appendChild(container);
}

function generateFoodItemRow_charges() {
    return `
            <div class="input-group mb-2 addon-charge-row">
                <input type="hidden" class="addon-charge-id" />
                <input type="text" class="form-control addon-charge-name" placeholder="Food Item Name" />
                <input type="number" step="0.01" class="form-control addon-charge-amount ms-2" placeholder="Amount" />
                <input type="text" class="form-control addon-charge-description ms-2" placeholder="Description (optional)" />
                <button class="btn btn-outline-danger ms-2" type="button" onclick="deleteAddonCharge_Items(this)">X</button>
            </div>
        `;
}

function addFoodItem_charges(headerId) {
    // const container = document.getElementById(headerId);
    // container.insertAdjacentHTML('beforeend', generateFoodItemRow_charges());

    const container = document.getElementById(headerId);
    const addMoreDiv = container.querySelector('.mt-2:last-child'); // button wrapper
    addMoreDiv.insertAdjacentHTML('beforebegin', generateFoodItemRow_charges());
}

function saveAddonCharges() {
    //console.log("saveAddonCharges called ");
    const addonCharges = [];
    //const userId = sessionStorage.getItem("userId");

    $("#foodHeaderContainer_charges .addon-charge-row").each(function () {
        const id = $(this).find(".addon-charge-id").val() || 0;
        const headerName = $(this).closest(".border").find(".addon-charge-header").val();
        const itemName = $(this).find(".addon-charge-name").val();
        const amount = parseFloat($(this).find(".addon-charge-amount").val()) || 0;
        const description = $(this).find(".addon-charge-description").val();

        // Skip empty rows
        if (!itemName && !description && (!amount || amount==0)) return;

        addonCharges.push({
            Id: id,
            HeaderName: headerName,
            ItemName: itemName,
            Amount: amount,
            Description: description
        });
    });

    if (addonCharges.length === 0) {
        addonCharges.push({
            Id: 0,
            HeaderName: null,
            ItemName: null,
            Amount: 0,
            Description: null
        });
    }

    const requestData = {
        AddonCharges: addonCharges,
        ActionBy: Number(sessionStorage.getItem('userId') || 0)
    };

    console.log("SaveCompanyFoodRateAddonCharge payload:", requestData);
    //showLoader();

    $.ajax({
        url: "/api/Company_FoodRate_AddonCharge/SaveCompanyFoodRateAddonCharge",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (res) {
            console.log("SaveCompanyFoodRateAddonCharge res:", res);
            if (res.status === "Success") {
                handleApiSuccess("Saved");

                loadAddonCharges();
            } else {
                Swal.fire({ icon: 'error', title: 'Oops...', text: res.message });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to save addon charges.');
        }
    });
}

function loadAddonCharges() {
    //showLoader();
    $.ajax({
        url: "/api/Company_FoodRate_AddonCharge/GetCompanyFoodRateAddonCharge",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            //console.log("GetCompanyFoodRateAddonCharge() res:", res);
            if (res.status === "Success" && res.data && res.data.length > 0) {
                //document.getElementById('foodHeaderContainer_charges').innerHTML = '';
                const container = document.getElementById('foodHeaderContainer_charges');
                container.innerHTML = ''; // clear old data

                const grouped = {};
                res.data.forEach(item => {
                    if (!grouped[item.HeaderName]) grouped[item.HeaderName] = [];
                    grouped[item.HeaderName].push(item);
                });
                //console.log("addon grouped:", grouped);
                let headerIndex = 0;
                // 🟡 Iterate through each header group
                Object.keys(grouped).forEach(headerName => {
                    const headerDbId = grouped[headerName][0].HeaderId;
                    addNewHeader_charges(headerDbId);

                    const lastHeader = container.lastElementChild;
                    const headerInput = lastHeader.querySelector('.addon-charge-header');
                    const headerBody = lastHeader.querySelector('.mt-2');
                    const toggleBtn = lastHeader.querySelector('.toggle-arrow-charges');

                    headerInput.value = headerName || '';
                    headerBody.innerHTML = '';

                    // 🔹 Render charges inside this header
                    grouped[headerName].forEach(charge => {
                        headerBody.insertAdjacentHTML('beforeend', `
                                <div class="input-group mb-2 addon-charge-row">
                                    <input type="hidden" class="addon-charge-id" value="${charge.Id || 0}" />
                                    <input type="text" class="form-control addon-charge-name" value="${charge.ItemName || ''}" placeholder="Charge Name" />
                                    <input type="number" step="0.01" class="form-control addon-charge-amount ms-2" value="${charge.Amount || 0}" placeholder="Amount" />
                                    <input type="text" class="form-control addon-charge-description ms-2" value="${charge.Description || ''}" placeholder="Description" />
                                    <button class="btn btn-outline-danger ms-2" type="button" onclick="deleteAddonCharge_Items(this)">X</button>
                                </div>
                            `);
                    });

                    // ➕ Add “Add More” button at the end
                    headerBody.insertAdjacentHTML('beforeend', `
                            <div class="mt-2">
                                <button class="btn btn-outline-success btn-sm" onclick="addFoodItem_charges('${headerBody.id}')">+ Add more</button>
                            </div>
                        `);

                    // 🎯 Expand 1st header only, collapse others
                    setTimeout(() => {
                        if (headerIndex === 0) {
                            // ✅ Keep 1st header open
                            $(headerBody).slideDown(0);
                            if (toggleBtn) toggleBtn.textContent = '▼';
                        } else {
                            // 🔒 Hide others
                            $(headerBody).hide();
                            if (toggleBtn) toggleBtn.textContent = '◀';
                        }
                    }, 200); // delay ensures DOM is ready

                    headerIndex++;
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load addon charges.');
        }
    });
}

function deleteAddonCharge_Header(id, button) {
    const row = button.closest('.addon-charge-row-header');
    //const id = row.querySelector('.addon-charge-headerid')?.value || 0;

    if (!id || id == "0") {
        row.remove();
        return;
    }

    const payload = {
        Id: Number(id || 0),
        ActionBy: Number(sessionStorage.getItem("userId") || 0)
    };
    console.log("addOn Header delete payload:", payload);
    //showLoader();

    fetch('/api/Company_FoodRate_AddonCharge/DeleteCompanyFoodRateAddonCharge_Header', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(res => res.json())
        .then(data => {
            //hideLoader();  // Hide after request completes (success or error)
            console.log("Delete CompanyFoodRateAddonCharge title res:", data);
            if (data.status === "Success") {
                row.remove();
                handleApiSuccess("Deleted!");
            } else {
                //alert("Error: " + data.message);
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: data.message || 'Something went wrong!'
                });
            }
        })
        .catch(err => {
            //hideLoader();  // Hide after request completes (success or error)
            console.error("Delete AddonCharge title error:", err);
            //alert("Failed to delete addon charge.");
            Swal.fire({
                icon: 'error',
                title: 'failed',
                text: 'Failed to delete addon charge title!'
            });
        });
}
function deleteAddonCharge_Items(button) {
    const row = button.closest('.addon-charge-row');
    const id = row.querySelector('.addon-charge-id')?.value || 0;

    if (!id || id == "0") {
        row.remove();
        return;
    }

    const payload = {
        Id: parseInt(id),
        ActionBy: sessionStorage.getItem("userId") || 0
    };
    //showLoader();

    fetch('/api/Company_FoodRate_AddonCharge/DeleteCompanyFoodRateAddonCharge_Items', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    })
        .then(res => res.json())
        .then(data => {
            //hideLoader();  // Hide after request completes (success or error)
            console.log("DeleteCompanyFoodRateAddonCharge res:", data);
            if (data.status === "Success") {
                row.remove();
                handleApiSuccess("Deleted!");
            } else {
                //alert("Error: " + data.message);
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: data.message || 'Something went wrong!'
                });
            }
        })
        .catch(err => {
            //hideLoader();  // Hide after request completes (success or error)
            console.error("DeleteCompanyFoodRateAddonCharge error:", err);
            //alert("Failed to delete addon charge.");
            Swal.fire({
                icon: 'error',
                title: 'failed',
                text: 'Failed to delete addon charge!'
            });
        });
}

// Auto-load when page is ready
document.addEventListener("DOMContentLoaded", function () {
    loadAddonCharges();
});

//<!-- TAB 5: Food Rate & Services > SubTab 5.3: Add-on Food Charges -- END -->

//<!-- TAB 5: Food Rate & Services > SubTab 5.4: Other Charges -- START -->

//new code
let services = [
    // ✅ Fixed Services
    { Id: 1, ServiceName: "Hall Rent", ChargeType: "lumpsum", Amount: 0, TaxType: "GST", GSTRate: 18, HSNSAC: "", IsFixedService: true, TallyLedgerName: "" },
    { Id: 2, ServiceName: "Food", ChargeType: "lumpsum", Amount: 0, TaxType: "GST", GSTRate: 5, HSNSAC: "", IsFixedService: true, TallyLedgerName: "" },

    // ✅ Other Services
    { Id: 3, ServiceName: "AC Charges", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 4, ServiceName: "Additional Decor Charges", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 5, ServiceName: "Addon Item Cost", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 6, ServiceName: "Anchor", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 7, ServiceName: "Band", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 8, ServiceName: "Bar Handling Charges", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 9, ServiceName: "Basic Lighting Decoration", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 10, ServiceName: "Bouncers", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 11, ServiceName: "Breakfast", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 12, ServiceName: "Bride & Groom welcome entry with sweat group", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 13, ServiceName: "Chair Covers", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 14, ServiceName: "Changing Rooms", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 15, ServiceName: "Cleaning Boys", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 16, ServiceName: "Cleaning Charges", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },

    // ✅ VAT Items
    { Id: 17, ServiceName: "Corkage-Beer", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 18, ServiceName: "Corkage-Breezer", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 19, ServiceName: "Corkage-Champagne", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 20, ServiceName: "Corkage Charges", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 21, ServiceName: "Corkage-Diet Beverage", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 22, ServiceName: "Corkage-Hard Drink", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 23, ServiceName: "Corkage-Redbull", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 24, ServiceName: "Corkage-Soda", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 25, ServiceName: "Corkage-Softdrinks", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 26, ServiceName: "Corkage-Water Bottle", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 27, ServiceName: "Corkage-Wine", ChargeType: "perHeadQty", Amount: 0, TaxType: "VAT", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },

    // ✅ More GST Services
    { Id: 28, ServiceName: "Couple sofa", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 29, ServiceName: "Current/Electricity", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 30, ServiceName: "Decoration", ChargeType: "perHeadQty", Amount: 3000, TaxType: "GST", GSTRate: 18, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 31, ServiceName: "Decoration Royalty", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 32, ServiceName: "Deluxe Room", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 33, ServiceName: "Security Deposit", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 34, ServiceName: "Security Deposit Catering", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 35, ServiceName: "Security Deposit Host", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 36, ServiceName: "Service boys", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 37, ServiceName: "Service Charge", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 38, ServiceName: "Soft Music Charges", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 39, ServiceName: "Sound System", ChargeType: "perHeadQty", Amount: 1500, TaxType: "GST", GSTRate: 18, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 40, ServiceName: "Stage carpet", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 41, ServiceName: "Stage extension", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 42, ServiceName: "Standard Room", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 43, ServiceName: "Super Deluxe Room", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 44, ServiceName: "Transportation Charges", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 45, ServiceName: "Vallet Parking", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 46, ServiceName: "Varmala & other accessories", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" },
    { Id: 47, ServiceName: "Walkway Carpet", ChargeType: "perHeadQty", Amount: 0, TaxType: "GST", GSTRate: 0, HSNSAC: "", IsFixedService: false, TallyLedgerName: "" }
];

let dynamicCharges = [];

document.addEventListener('DOMContentLoaded', () => {
    loadCharges(); // Auto-load on page ready
});

function renderCharges(dataList) {
    const fixedBody = document.getElementById("fixedChargesTableBody");
    const optionalBody = document.getElementById("chargesTableBody");
    fixedBody.innerHTML = "";
    optionalBody.innerHTML = "";

    dataList.forEach((svc, idx) => {
        const badgeClass = svc.TaxType === "VAT" ? "bg-warning text-white fw-bold" : "bg-success text-white fw-bold";
        const taxLabel = svc.TaxType === "VAT" ? "VAT" : "GST";

        //console.log("svc:", svc);
        // row for fixed charges
        if (svc.IsFixedService) {
            const fixedRow = document.createElement("tr");
            fixedRow.innerHTML = `
                    <td>
                        ${svc.ServiceName}
                        <input type="hidden" class="service-id" value="${svc.Id}" />
                        <input type="hidden" class="tax-type" value="${svc.TaxType}" />
                        <input type="hidden" class="is-fixed" value="1" />
                    </td>
                    <td>
                        <input type="number" class="form-control form-control-sm gst-rate"
                               value="${svc.GSTRate || 0}" placeholder="GST %" />
                    </td>
                    <td><input type="text" class="form-control form-control-sm"
                               value="${svc.HSNSAC || ''}" placeholder="HSN/SAC"/></td>
                    <td><input type="text" class="form-control form-control-sm"
                               value="${svc.TallyLedgerName || ''}" placeholder="Your Tally Ledger Name"/></td>
                `;
            fixedBody.appendChild(fixedRow);
        }
        // row for optional charges
        else {
            const optRow = document.createElement("tr");
            optRow.innerHTML = `
                    <td>
                        ${svc.ServiceName}
                        <input type="hidden" class="service-id" value="${svc.Id}" />
                        <input type="hidden" class="tax-type" value="${svc.TaxType}" />
                        <input type="hidden" class="is-fixed" value="0" />
                    </td>
                    <td>
                        <div class="form-check form-check-inline">
                            <input class="form-check-input" type="radio" name="chargeType${idx}"
                                   value="lumpsum" ${svc.ChargeType === "lumpsum" ? 'checked' : ''}>
                            <label class="form-check-label">Lump-sum</label>
                        </div>
                        <div class="form-check form-check-inline">
                            <input class="form-check-input" type="radio" name="chargeType${idx}"
                                   value="perHeadQty" ${svc.ChargeType === "perHeadQty" ? 'checked' : ''}>
                            <label class="form-check-label">Per-head / Qty</label>
                        </div>
                    </td>
                    <td><input type="number" class="form-control form-control-sm" value="${svc.Amount || 0}"/></td>
                    <td>
                        <div class="d-flex align-items-center gap-2">
                            <input type="number" class="form-control form-control-sm gst-rate"
                                   style="width: 80px;" value="${svc.GSTRate || 0}" />
                            <span class="badge ${badgeClass}">${taxLabel}</span>
                        </div>
                    </td>
                    <td><input type="text" class="form-control form-control-sm" value="${svc.HSNSAC || ''}" placeholder="HSN/SAC"/></td>
                    <td><input type="text" class="form-control form-control-sm" value="${svc.TallyLedgerName || ''}" placeholder="Your Tally Ledger Name"/></td>
                `;
            optionalBody.appendChild(optRow);
        }
    });
}

function loadCharges() {
    var inputData = {
        Id: 0 // pass id or null
    };
    //showLoader();

    $.ajax({
        url: "/api/Company_FoodRate_OtherCharge/GetCompanyFoodRateOtherCharge",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(inputData),
        success: function (res) {
            //console.log("GetCompanyFoodRateOtherCharge res:", res);
            let data = [];
            if (res.status === "Success" && res.data.length > 0) {
                //console.log("dynamic data called");
                // ✅ Normalize API data
                data = res.data.map(item => ({
                    Id: item.Id || 0,
                    ServiceName: item.ServiceName || "",
                    ChargeType: item.ChargeType || "lumpsum",
                    Amount: item.Amount || 0,
                    TaxType: item.TaxType || "GST",
                    GSTRate: item.GSTRate || 0,
                    HSNSAC: item.HSNSAC || "",
                    TallyLedgerName: item.TallyLedgerName || "",
                    IsFixedService: item.IsFixedService === true // ensure boolean
                }));
            } else {
                //console.log("static data called");
                // ✅ Fallback to local services with consistent structure
                data = services.map(s => ({
                    Id: s.Id || 0,
                    ServiceName: s.ServiceName || "",
                    ChargeType: s.ChargeType || "lumpsum",
                    Amount: s.Amount || 0,
                    TaxType: s.TaxType || "GST",
                    GSTRate: s.GSTRate || 0,
                    HSNSAC: s.HSNSAC || "",
                    TallyLedgerName: s.TallyLedgerName || "",
                    IsFixedService: s.IsFixedService === true
                }));
            }


            dynamicCharges = data;
            renderCharges(data);
            //console.log("GetCompanyFoodRateOtherCharge res data:", data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'GetCompanyFoodRateOtherCharge');
            const fallback = services.map(s => ({
                ...s,
                isLumpsum: true,
                hsn: s.hsn || "",
                ledgerName: s.ledgerName || ""
            }));
            renderCharges(fallback);
        }
    });
}

function saveCharges() {
    const allRows = [
        ...document.querySelectorAll("#fixedChargesTableBody tr"),
        ...document.querySelectorAll("#chargesTableBody tr")
    ];

    const payload = [];

    allRows.forEach((row, idx) => {
        const id = row.querySelector(".service-id")?.value || 0;
        const name = row.children[0]?.innerText.trim();
        const isFixed = row.querySelector(".is-fixed")?.value === "1";
        const taxType = row.querySelector(".tax-type")?.value || "GST";

        // chargeType
        const chargeTypeRadios = row.querySelectorAll(`input[type='radio'][name="chargeType${idx}"]`);
        const chargesType = chargeTypeRadios[0]?.checked ? "lumpsum" : "perHeadQty";

        // amount
        const chargesAmount = parseFloat(row.querySelector("input[type='number']")?.value || "0");

        // GST Rate (from input field)
        const gstRate = parseFloat(row.querySelector(".gst-rate")?.value || "0");

        // hsn + ledger
        const hsn = row.querySelector("input[placeholder='HSN/SAC']")?.value.trim() || "";
        const ledgerName = row.querySelector("input[placeholder='Your Tally Ledger Name']")?.value.trim() || "";

        payload.push({
            Id: parseInt(id),
            ServiceName: name,
            ChargeType: chargesType,
            Amount: chargesAmount,
            GSTRate: gstRate,
            HSNSAC: hsn,
            TallyLedgerName: ledgerName,
            TaxType: taxType,
            IsFixedService: isFixed
        });
    });

    var inputData = {
        charges: payload,
        ActionBy: sessionStorage.getItem("userId") || 0
    };
    console.log("payload full", inputData);
    //showLoader();
    $.ajax({
        url: "/api/Company_FoodRate_OtherCharge/SaveCompanyFoodRateOtherCharge",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(inputData),
        success: (res) => {
            console.log("SaveCompanyFoodRateOtherCharge res:", res);
            if (res.status === "Success") {
                handleApiSuccess("Saved");
                loadCharges(); // Auto-load on save
            } else {
                Swal.fire({ icon: 'error', title: 'Failed', text: res.message || "Something went wrong." });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Saving OtherCharge failed. Please try again.');
        }
    });
}

//<!-- TAB 5: Food Rate & Services > SubTab 5.4: Other Charges -- END -->

//<!-- TAB 5: Food Rate & Services > SubTab 5.5: Label Printing -- START -->

function fillFontSizeDropdown() {
    const fontSizeSelect = document.getElementById("fontSize");

    // clear old options
    fontSizeSelect.innerHTML = "";

    // Example range 1–50 (you can change as needed)
    for (let i = 1; i <= 50; i++) {
        const option = document.createElement("option");
        option.value = i;
        option.textContent = i;
        fontSizeSelect.appendChild(option);
    }
    loadLabelSetup();
}

function saveLabelSetup() {
    const selectedFont = document.querySelector('input[name="fontOption"]:checked')?.value || '';
    const selectedFontSize = document.getElementById('fontSize').value || '0';
    const userId = sessionStorage.getItem('userId') || 0;
    const id = document.getElementById('label_id').value;

    const payload = {
        Id: id,
        FontName: selectedFont,
        FontSize: selectedFontSize,
        PreviewImagePath: '/assets2/img/image_icon.png',
        PlaqueSizeInches: '22*12',
        PlaqueSizeCm: '9*6',
        PlaqueSizeMm: '90*60',
        ActionBy: userId
    };
    console.log("label Save payload:", payload);
    //showLoader();
    $.ajax({
        url: '/api/Company_FoodRate_LabelPrinting/SaveCompanyFoodRateLabelPrinting',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("SaveCompanyFoodRateLabelPrinting res:", res);
            if (res.status === "Success") {
                handleApiSuccess("Saved");
                loadLabelSetup();
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Failed',
                    text: res.message
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to save label setup');
        }
    });
}

function loadLabelSetup() {
    var inputData = {
        Id: 0
    };
    //showLoader();
    $.ajax({
        url: '/api/Company_FoodRate_LabelPrinting/GetCompanyFoodRateLabelPrinting',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(inputData),
        success: function (res) {
            //console.log("GetCompanyFoodRateLabelPrinting res:", res);
            if (res.status === "Success" && res.data) {
                document.getElementById('label_id').value = res.data.Id;

                // Set font size
                const size = res.data.FontSize || '0';
                document.getElementById('fontSize').value = size;

                // Select font radio
                const font = res.data.FontName || '';
                document.querySelectorAll('input[name="fontOption"]').forEach(radio => {
                    radio.checked = (radio.value === font);
                });

                // Static values with numbers only
                // ================= SIZE DATA =================
                let sizeInInches = res.data.PlaqueSizeInches || ''; // "22*12"
                let sizeInCm = res.data.PlaqueSizeCm || '';
                let sizeInMm = res.data.PlaqueSizeMm || '';

                // Fill UI text
                document.getElementById("sizeInches").innerHTML = sizeInInches;
                document.getElementById("sizeCm").innerHTML = sizeInCm;
                document.getElementById("sizeMm").innerHTML = sizeInMm;

                // ================= PREVIEW SIZE LOGIC =================
                const previewBox = document.getElementById("labelPreviewBox");
                const previewText = document.getElementById("labelPreview");

                // Safety checks
                if (previewBox && sizeInInches) {

                    // Accept formats: 22*12 | 22 x 12
                    const [wIn, hIn] = sizeInInches
                        .toLowerCase()
                        .replace('x', '*')
                        .split('*')
                        .map(v => parseFloat(v.trim()));

                    if (!isNaN(wIn) && !isNaN(hIn)) {

                        const DPI = 96;                 // screen DPI
                        const MAX_PREVIEW_WIDTH = 300;  // UI constraint

                        // Actual size in px (real-world)
                        const actualWidthPx = wIn * DPI;
                        const actualHeightPx = hIn * DPI;

                        // Scale down to fit UI
                        const scale = Math.min(
                            MAX_PREVIEW_WIDTH / actualWidthPx,
                            1
                        );

                        // Apply scaled size
                        previewBox.style.width = `${actualWidthPx * scale}px`;
                        previewBox.style.height = `${actualHeightPx * scale}px`;
                    }
                }

                // ================= FONT SETTINGS =================
                if (previewText) {
                    previewText.style.fontFamily = font || "'Dancing Script', cursive";
                    previewText.style.fontSize = size ? `${size}px` : "16px";
                }

            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load label setup.');
        }
    });
}

// 🔹 Update preview when font or size changes
function updateLabelPreview() {
    const selectedFont = document.querySelector('input[name="fontOption"]:checked')?.value || 'Arial';
    const selectedSize = document.getElementById("fontSize").value || 16;

    const preview = document.getElementById("labelPreview");
    preview.style.fontFamily = selectedFont;
    preview.style.fontSize = selectedSize + "px";
}

// Load label setup when page is ready
// Attach event listeners
document.addEventListener("DOMContentLoaded", function () {
    fillFontSizeDropdown();

    // radio font change
    document.querySelectorAll('input[name="fontOption"]').forEach(radio => {
        radio.addEventListener("change", updateLabelPreview);
    });

    // font size dropdown change
    document.getElementById("fontSize").addEventListener("change", updateLabelPreview);

    // Initial call
    updateLabelPreview();

    $('#btnAmazon').on('click', function () {
        url = "https://www.amazon.in/Shuban-Acrylic-L-Shape-Restaurants-Supermarket/dp/B0DDJJ9RPR/ref=sr_1_2_sspa?crid=1X925VM1OO9U7&dib=eyJ2IjoiMSJ9.YCawWf30GZsyfDI7eI_jtOt2mBkPja0R0SdyxBfkzE5n5bZHg4zpfL5L5pN_HV7xhd0lUC3aueC_VEfQzX1-dvsSKbzuM6Yt7GGZdn2eh8qAKD9cg9G-N626iUTMCTuXmuyHKNHJEcKoyC03o8UKyBZocawfXcJdJT419O8GAxDEGH8aPCF2_Jc4Rdh9eXrAc_s_5skGCd4HrU4uOMVt_5RHthfWXvteb9onPVmZkbbfohV2aV-MEQwnlHukrVwEJmawo5erIv1Z9AtufzJEA8s1iYiFd3pLLDwnd6Jf1j4.8p_a7XQtORrzWLSlMrNoV9Y-xIUNw77KaAaNOlS8XKU&dib_tag=se&keywords=menu%2Bholder%2Bfor%2Brestaurant&qid=1733903594&sp_csd=d2lkZ2V0TmFtZT1zcF9hdGY&sprefix=menu%2Bholder%2Caps%2C264&sr=8-2-spons&th=1";
        window.open(url, "_blank");
    });
});

//<!-- TAB 5: Food Rate & Services > SubTab 5.5: Label Printing -- END -->

//<!-- TAB 6: Terms & Conditions -- START -->

initEditor();
//getTermsAndConditions();

function initEditor() {
    const editor = document.getElementById('termsEditor');
    if (!editor) return;

    // Initialize placeholder if empty
    if (editor.innerText.trim() === '') {
        editor.innerText = 'Type Terms & Conditions';
        editor.style.color = '#888'; // Placeholder color
    }

    editor.addEventListener('focus', () => {
        if (editor.innerText.trim() === 'Type Terms & Conditions') {
            editor.innerText = '';
            editor.style.color = '#000'; // Normal text color
        }
    });

    editor.addEventListener('blur', () => {
        if (editor.innerText.trim() === '') {
            editor.innerText = 'Type Terms & Conditions';
            editor.style.color = '#888'; // Placeholder color
        }
    });
}
function formatText(command) {
    document.execCommand(command, false, null);
}

//userId = sessionStorage.getItem('userId') || 0;

function saveTermsAndConditions() {
    // Get form data
    const data = {
        Id: $('#tncId').val(), // Assuming you have companyId defined elsewhere
        PrintInProspectus: $('#printProspectus').is(':checked'),
        PrintInInvoice: $('#printInvoice').is(':checked'),
        TermsContent: $('#termsEditor').html(),
        ActionBy: Number(sessionStorage.getItem('userId') || 0)
    };
    //console.log("tnc data:", data);
    // Show loading indicator
    // const saveBtn = $('button[onclick="saveData(\'terms\')"]');
    // saveBtn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status"></span> Saving...');
    //showLoader();

    $.ajax({
        url: '/api/Company_TermsAndConditions/SaveCompanyTermsAndConditions',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            //console.log("SaveCompanyTermsAndConditions res:", response);
            if (response.status === "Success") {
                handleApiSuccess("Saved");
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message || 'Failed to save terms and conditions'
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to save terms and conditions');
        },
        complete: function () {
            //hideLoader();  // Hide after request completes (success or error)
            saveBtn.prop('disabled', false).text('Save');
        }
    });
}

//called in Script(Tab1-2) - GetCompanyProfile()
function getTermsAndConditions() {
    //showLoader();
    $.ajax({
        url: '/api/Company_TermsAndConditions/GetCompanyTermsAndConditions',
        type: 'GET',
        contentType: 'application/json',
        success: function (response) {
            //console.log("GetCompanyTermsAndConditions res:", response);
            if (response.status === "Success" && response.data && response.data.length > 0) {
                const termsData = response.data[0];
                $('#tncId').val(termsData.Id);
                // Set checkbox values
                $('#printProspectus').prop('checked', termsData.PrintInProspectus);
                $('#printInvoice').prop('checked', termsData.PrintInInvoice);

                // Set editor content
                const termsEditor = $('#termsEditor');
                if (termsData.TermsContent && termsData.TermsContent !== '') {
                    termsEditor.html(termsData.TermsContent);
                } else {
                    termsEditor.html('Type Terms & Conditions');
                }
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Save Failed - Terms & Conditions');
            // Initialize with default content if there's an error
            $('#termsEditor').html('Type Terms & Conditions');
        }
    });
}

//<!-- TAB 6: Terms & Conditions -- END -->

//<!--Common for ALL - START -->

//Save Company & Property Data -- END
// function saveForm(formId) {
//     let formData = {};
//     $(`#${formId}`).serializeArray().forEach(f => formData[f.name] = f.value);
//     console.log("Saved Data for", formId, formData);
// }

//<!--Common for ALL - END -->
