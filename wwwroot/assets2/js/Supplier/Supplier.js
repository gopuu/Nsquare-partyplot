$(document).ready(function () {
    loadStateDropdown();
    loadCityDropdown();
    fetchData();

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
            //$('#addEventModal').modal('hide');
            const modal = new bootstrap.Modal(document.getElementById('addEventModal'));
            modal.hide();
        } else {
            resetFormFields(); // Reset form fields when modal is shown
            $("#addEventModalTitle").text("Add Supplier");
            //$("#addEventModal").modal("show");
            const modal = new bootstrap.Modal(document.getElementById('addEventModal'));
            modal.show();        }
    });

    // Edit button click
    $(document).on('click', '.btn-edit', function (event) {
        var canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';
        if (!canEdit) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Edit. Please contact admin.",
                confirmButtonText: "OK"
            });
            event.preventDefault(); // Stop navigation
            $('#addEventModal').modal('hide');
        }
        else {
            resetFormFields();

            // Get the DataTable instance
            var table = $('#tableMain table').DataTable();

            // Get the row object for the clicked Edit button
            var row = $(this).closest('tr');
            var rowData = table.row(row).data(); // Get the data object for this row

            console.log("edit rowData:", rowData);
            // Extract values from the row data object
            $("#supplierId").val(rowData.Id);
            $("#supName").val(rowData.Name);
            $("#supCompany").val(rowData.CompanyName);
            $("#supEmail").val(rowData.Email);
            $("#supPhone").val(rowData.Phone);
            $("#supGST").val(rowData.GSTNo);
            $("#supRegAddress").val(rowData.RegisterAddress);
            $("#supRegStateId").val(rowData.RegisterStateId);
            $("#supRegCity").val(rowData.RegisterCity);
            $("#supRegPincode").val(rowData.RegisterPincode);
            $("#supShipAddress").val(rowData.ShippingAddress);
            $("#supFSSAI").val(rowData.FSSAI_LicNo);
            $("#supPAN").val(rowData.PAN_No);
            $("#supMSME").val(rowData.MSME_No);
            $("#supTAN").val(rowData.TAN_No);
            $("#supCIN").val(rowData.CIN_No);
            $("#supTCS").val(rowData.TCS_Percent);
            $("#supType").val(rowData.SupplierType);
            $("#supIsActive").prop("checked", rowData.IsActive);

            // SET DOCUMENT URL
            $("#supDocumentLink").val(rowData.Documents || "");

            // SHOW / HIDE DOWNLOAD BUTTON
            if (rowData.Documents && rowData.Documents !== "") {
                $("#btnDownloadSupDoc").removeClass("d-none");
            } else {
                $("#btnDownloadSupDoc").addClass("d-none");
            }


            // Show the modal with the pre-filled data
            $("#addEventModalTitle").text("Edit Supplier");
            $('#addEventModal').modal('show');
        }
    });

    // SAVE - Handle form submission
    $("#btnSave").click(function (event) {
        event.preventDefault(); // Prevent the default form submission
        saveSupplier();
    });

    async function saveSupplier() {

        let file = $("#supFile")[0].files[0];

        // Convert only if file exists
        let fileBase64 = await getFileAsBase64(file);

        // Build payload NOW
        let payload = {
            Id: parseInt($("#supplierId").val()),
            Name: $("#supName").val(),
            CompanyName: $("#supCompany").val(),
            Email: $("#supEmail").val(),
            Phone: $("#supPhone").val(),
            GSTNo: $("#supGST").val(),
            RegisterAddress: $("#supRegAddress").val(),
            RegisterStateId: parseInt($("#supRegStateId").val() || 0),
            RegisterCity: parseInt($("#supRegCity").val() || 0),
            RegisterPincode: $("#supRegPincode").val(),
            ShippingAddress: $("#supShipAddress").val(),
            FSSAI_LicNo: $("#supFSSAI").val(),
            PAN_No: $("#supPAN").val(),
            MSME_No: $("#supMSME").val(),
            TAN_No: $("#supTAN").val(),
            CIN_No: $("#supCIN").val(),
            TCS_Percent: parseFloat($("#supTCS").val() || 0),

            SupplierType: $("input[name='supType']:checked").val(),

            Documents: fileBase64?.FileBase64 || null,    // <-- base64 file or null

            IsActive: $("#supIsActive").is(":checked"),
            ActionBy: parseInt(sessionStorage.getItem("userId") ?? 0)
        };

        console.log("save payload:", payload);

        $.ajax({
            url: "/api/Supplier/SaveSupplier",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            success: function (response) {
                if (response.status.toLowerCase() == "success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 900
                    });
                    fetchData();
                    resetFormFields();
                    $('#addEventModal').modal('hide');
                } else {
                    $('#message').text(response.message).css('color', 'red')
                        .fadeIn(300).delay(5000).fadeOut(300);
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Supplier Save Error");
            },
            complete: function () {
                $('#btn-loader').hide();
            }
        });
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
            url: "/api/Supplier/DeleteSupplier",
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
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Deleted !!",
                        showConfirmButton: false,
                        timer: 900
                    });
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
            error: function (xhr) {
                handleApiError(xhr, status, error, "Supplier Delete Error");
            },
            complete: function () {
                $('#btn-loader').hide(); // Ensure loader is hidden in any case
            }
        });
        $('#deleteRecordModal').modal('hide');
    });

    // When user selects a state, fetch its cities
    $("#supRegStateId").on("change", function () {
        let stateId = $(this).val();

        if (stateId && stateId !== "0") {
            loadCityDropdown(stateId);
        } else {
            // Reset city dropdown if no valid state selected
            $("#supRegCity").html(`<option value="0">-- Select City --</option>`);
        }
    });

    //file download
    $("#btnDownloadSupDoc").on("click", function () {
        let fileUrl = $("#supDocumentLink").val();

        if (!fileUrl || fileUrl.trim() === "") {
            Swal.fire("Document not found", "No file available to download.", "warning");
            return;
        }

        // Create a hidden <a> for safe browser download
        const link = document.createElement("a");
        link.href = fileUrl;
        link.target = "_blank";   // open in new tab
        link.download = "";       // allow browser to download if possible

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    });
});

function renderTable(data) {
    console.log("Supplier data:", data);

    // Destroy existing DataTable if present
    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    // If data is empty
    if (!data || data.length === 0) {
        console.log("No Data Found");

        $("#tableMain tbody").html(`
                <tr>
                    <td colspan="7" class="text-center fw-bold text-danger">No Data Found !!</td>
                </tr>
            `);

        // Reinitialize empty DataTable
        $('#tableMain table').DataTable({
            data: [],
            columns: [
                { data: 'srno', title: 'Sr. No.' },
                { data: 'Name', title: 'Name' },
                { data: 'CompanyName', title: 'Company' },
                { data: 'Phone', title: 'Phone' },
                { data: 'GSTNo', title: 'GST No.' },
                {
                    data: 'IsActiveText',
                    title: 'Status',
                    render: (val) => `<span class="badge ${val === 'Active' ? 'bg-success' : 'bg-secondary'}">${val}</span>`
                },
                {
                    data: null,
                    title: 'Actions',
                    render: function () {
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
            stateSave: false,
            pageLength: 10,
            ordering: true,
            searching: true,
            lengthChange: false,
            pagingType: "full_numbers",
            columnDefs: [
                {
                    targets: 0,
                    className: "text-center"
                }
            ]
        });

        return;
    }

    // Add Sr No
    let processData = data;
    processData.forEach((item, index) => {
        item.srno = index + 1;
        item.IsActiveText = item.isActive ? "Active" : "Inactive";
    });

    console.log("Processed supplier data:", processData);

    // Destroy old DataTable again to avoid residual config
    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    // Reinitialize DataTable
    $('#tableMain table').DataTable({
        data: processData,
        columns: [
            { data: 'srno', title: '#' },
            { data: 'Name', title: 'Name' },
            { data: 'CompanyName', title: 'Company' },
            { data: 'Phone', title: 'Phone' },
            { data: 'GSTNo', title: 'GST No.' },
            {
                data: 'IsActive',
                title: 'Status',
                render: (val) => `<span class="badge ${val ? 'bg-success' : 'bg-secondary'}">${val ? 'Active' : 'Inactive'}</span>`
            },
            {
                data: null,
                title: 'Actions',
                render: function (data, type, row) {
                    return `
                            <div class="d-flex flex-column flex-sm-row gap-2">
                                <button class="btn btn-outline-warning btn-sm btn-action-xss btn-edit" data-id="${row.id}">
                                    <i class="fa fa-edit"></i>
                                </button>

                                <button class="btn btn-outline-danger btn-sm btn-action-xss btn-delete" data-id="${row.id}">
                                    <i class="fa fa-trash"></i>
                                </button>
                            </div>
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
        pageLength: 10,
        ordering: true,
        searching: true,
        lengthChange: false,
        pagingType: "full_numbers"
    });

}

function initializeSorting() {
    const options = {
        valueNames: ["#", "Name", "Company", "Phone", "GST No.", "Status"],
        page: 10,
        pagination: true
    };
    new List('#tableMain table', options);
}

//get data from API
function fetchData() {
    $('.search').val('');

    var payload = { Id: 0 };
    $.ajax({
        url: "/api/Supplier/GetSupplierList",
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
            handleApiError(xhr, status, error, "Supplier Fetch Error");
        }
    });
}

// Reset form fields
function resetFormFields() {
    //console.log("reset called");
    // Switch to FIRST TAB
    var firstTab = new bootstrap.Tab(document.querySelector('#supTabs .nav-link:first-child'));
    firstTab.show();

    // Hidden ID
    $("#supplierId").val(0);
    // Basic info
    $("#supName").val("");
    $("#supCompany").val("");
    $("#supEmail").val("");
    $("#supPhone").val("");

    $("#supGST").val("");
    // Registered Address
    $("#supRegAddress").val("");
    $("#supRegStateId").val("0");   // dropdown
    $("#supRegCity").val("0");      // dropdown
    $("#supRegPincode").val("");

    // Shipping Address
    $("#supShipAddress").val("");
    // Legal Identifiers
    $("#supFSSAI").val("");
    $("#supPAN").val("");
    $("#supMSME").val("");
    $("#supTAN").val("");

    $("#supCIN").val("");
    // Tax / Supplier Type
    $("#supTCS").val("");
    $('input[name="supType"][value="1"]').prop('checked', true);
    //File upload
    $("#supFile").val("");
    $("#supDocumentLink").val("");
    $("#btnDownloadSupDoc").addClass("d-none");

    // Is Active
    $("#supIsActive").prop("checked", true);
}

// Validate form fields
function validateAddEventForm() {

    let name = $("#supName").val().trim();
    let phone = $("#supPhone").val().trim();
    let gst = $("#supGST").val().trim();
    let regState = $("#supRegStateId").val();
    let regCity = $("#supRegCity").val();
    let regPincode = $("#supRegPincode").val().trim();

    // Required fields
    if (name === "") {
        Swal.fire("Validation", "Supplier Name is required", "warning");
        return false;
    }
    // Optional: Basic phone validation
    if (!/^[0-9]{10}$/.test(phone)) {
        Swal.fire("Validation", "Phone number must be 10 digits.", "warning");
        return false;
    }

    if (gst !== "" && gst.length !== 15) {
        Swal.fire("Validation", "Invalid GST Number", "warning");
        return false;
    }

    if (!regState) { // || regState === "0") {
        Swal.fire("Validation", "Please select Register State", "warning");
        return false;
    }

    if (!regCity) { //} === "" || regCity === null) {
        Swal.fire("Validation", "Please select Register City", "warning");
        return false;
    }

    if (regPincode === "") {
        Swal.fire("Validation", "Register Pincode is required", "warning");
        return false;
    }

    if (regPincode.length < 6) {
        Swal.fire("Validation", "Invalid Pincode", "warning");
        return false;
    }

    let file = $("#supFile")[0].files[0];
    if (file && file.size > 2 * 1024 * 1024) {
        Swal.fire("Validation", "File size must be less than 2MB", "warning");
        return;
    }

    return true; // valid
}

function loadStateDropdown() {
    $.ajax({
        url: "/api/State/GetStateList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            console.log("response state:", response);
            var data = response.data;
            $("#supRegStateId").html(`<option value="0">-- Select State --</option>`);
            data.forEach(s => {
                $("#supRegStateId").append(`<option value="${s.Id}">${s.Name}</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "State Fetch Error");
        }
    });
}

function loadCityDropdown(stateId = 0) {
    var payload = { Id: 0, StateId: stateId };
    $.ajax({
        url: "/api/City/GetCityList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            console.log("response city:", response);
            var data = response.data;

            $("#supRegCity").html(`<option value="0">-- Select City --</option>`);
            data.forEach(s => {
                $("#supRegCity").append(`<option value="${s.Id}">${s.Name}</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "City Fetch Error");
        }
    });
}

function getFileAsBase64(file) {
    return new Promise((resolve, reject) => {
        if (!file) {
            resolve(null);
            return;
        }

        const reader = new FileReader();

        reader.onload = function (e) {
            let base64 = e.target.result;

            // Remove metadata (e.g., "data:application/pdf;base64,")
            base64 = base64.split(",")[1];

            resolve({
                FileName: file.name,
                FileExtension: file.name.split('.').pop(),
                FileBase64: base64
            });
        };

        reader.onerror = function () {
            reject("File reading error");
        };

        reader.readAsDataURL(file);
    });
}


//TAB - to show specific tab on modal open -- START ---------------
let currentTab = 0;
const totalTabs = 3;

function showTab(index) {
    currentTab = index;

    let tabs = document.querySelectorAll("#supTabs .nav-link");
    let tabContents = document.querySelectorAll(".tab-pane");

    tabs.forEach(t => t.classList.remove("active"));
    tabContents.forEach(tc => tc.classList.remove("show", "active"));

    tabs[index].classList.add("active");
    tabContents[index].classList.add("show", "active");

    updateTabButtons();
}

function updateTabButtons() {
    $("#btnPrev").toggle(currentTab !== 0);
    $("#btnNext").toggle(currentTab !== totalTabs - 1);
    $("#btnSave").toggle(currentTab === totalTabs - 1);
}

$("#btnPrev").click(() => showTab(currentTab - 1));
$("#btnNext").click(() => showTab(currentTab + 1));

$("#supTabs .nav-link").on("shown.bs.tab", function () {
    let all = $("#supTabs .nav-link");
    currentTab = all.index(this);
    updateTabButtons();
});

$(document).ready(function () {
    updateTabButtons();
});

//TAB - to show specific tab on modal open -- END ---------------
