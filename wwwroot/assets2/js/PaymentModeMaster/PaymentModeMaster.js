$(document).ready(function () {
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
            $('#addEventModal').modal('hide');
        } else {
            resetFormFields(); // Reset form fields when modal is shown
            $("#addEventModalTitle").text("Add Payment Mode");
            $("#addEventModal").modal("show");
        }
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

            // Fill modal fields
            $("#paymentModeId").val(rowData.Id);
            $("#paymentModeName").val(rowData.PaymentModeName);
            $("#paymentModeDescription").val(rowData.Description);
            $("#isPaymentModeActive").prop("checked", rowData.IsActive);

            $("#addEventModalTitle").text("Edit Payment Mode");
            $('#addEventModal').modal('show');
        }
    });

    // SAVE - Handle form submission
    $("#btnSaveAddEventModal").click(function (event) {
        event.preventDefault(); // Prevent the default form submission
        // Validate first
        if (!validateForm()) return;

        let id = $("#paymentModeId").val();
        let name = $("#paymentModeName").val().trim();
        let desc = $("#paymentModeDescription").val().trim();
        let isActive = $("#isPaymentModeActive").is(":checked");

        let payload = {
            Id: id,
            PaymentModeName: name,
            Description: desc,
            IsActive: isActive,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        console.log("inputData:", payload)
        // Send AJAX request
        $.ajax({
            url: "/api/PaymentMode/SavePaymentMode",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            success: function (response) {
                //var obj = JSON.parse(response);
                var obj = response;
                //console.log("submit obj:", obj);
                if (obj.status.toLowerCase() == "success") {
                    handleApiSuccess("Payment Mode Saved");
                    fetchData();
                    resetFormFields();  // Reset the form fields
                    $('#addEventModal').modal('hide');  // Close the modal
                } else {
                    $('#message').removeAttr('hidden');
                    $('#message').text(obj.message).css('color', 'red').fadeIn(300).delay(5000).fadeOut(300);
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save PaymentMode Error")
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
    $('#delete-record').click(function () {
        var id = $('#deleteRecordModal').data('id');

        let payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $('#btn-loader').show();
        $.ajax({
            url: "/api/PaymentMode/DeletePaymentMode",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                //console.log("Delete Data:", response);
                // Parse the response if it's a string
                if (typeof response === 'string') {
                    response = JSON.parse(response);
                }

                if (response.status === "Success") {
                    handleApiSuccess("Payment Mode Deleted");
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
                handleApiError(xhr, status, error, "Delete PaymentMode Error")
            }
        });
        $('#deleteRecordModal').modal('hide');
    });

});

function renderTable(data) {
    //console.log("PaymentMode data:", data);
    // Destroy existing DataTable if present
    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    // Handle empty data
    if (!data || data.length === 0) {

        $('#tableMain tbody').html(`
        <tr>
            <td colspan="4" class="text-center fw-bold text-danger">No Data Found !!</td>
        </tr>
        `);

        $('#tableMain table').DataTable({
            data: [],
            columns: [
                { data: 'srno', title: 'Sr. No.' },
                { data: 'PaymentModeName', title: 'Payment Mode' },
                { data: 'Description', title: 'Description' },
                { data: 'IsActive', title: 'Active' },
            ],
            pageLength: 10,
            ordering: true,
            searching: true,
            lengthChange: false,
            pagingType: "full_numbers"
        });

        return;
    }
    // Add Sr. No
    let processData = data;
    processData.forEach((item, index) => {
        item.srno = index + 1;
    });
    //console.log("Processed PaymentMode data:", processData);
    /// Build DataTable
    $('#tableMain table').DataTable({
        data: processData,
        columns: [
            { data: 'srno', title: 'Sr. No.' },

            { data: 'PaymentModeName', title: 'Payment Mode' },

            { data: 'Description', title: 'Description' },

            {
                data: 'IsActive',
                title: 'Status',
                render: function (data) {
                    return data
                        ? `<span class="badge bg-success">Active</span>`
                        : `<span class="badge bg-danger">Inactive</span>`;
                }
            },
            /*
            {
                data: null,
                title: 'Actions',
                render: function (data, type, row) {
                    return `
                        <div class="d-flex flex-column flex-sm-row gap-2">
        
                            <button class="btn btn-outline-warning btn-action-xss btn-edit">
                                <i class="fa fa-edit"></i>
                            </button>
        
                            <button class="btn btn-outline-danger btn-action-xss btn-delete">
                                <i class="fa fa-trash"></i>
                            </button>
        
                        </div>
                    `;
                }
            }
            */
        ],
        columnDefs: [
            {
                targets: 0
            },
            {
                targets: 3
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

// Initialize sorting
function initializeSorting() {
    const options = {
        valueNames: ["srno", "PaymentModeName", "Description", "IsActive"],
        page: 10,
        pagination: true
    };

    const list = new List('tableMain', options);
}

//get data from API
function fetchData() {
    $('#btn-loader').show();
    $('.search').val('');

    $.ajax({
        url: "/api/PaymentMode/GetPaymentMode",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            //data = response; // Store the fetched data
            console.log("response fetchData:", response);
            var data = response.data;
            renderTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Get PaymentMode Error")
        }
    });
}

//RESET
function resetFormFields() {

    $("#paymentModeId").val(0);
    $("#paymentModeName").val("");
    $("#paymentModeDescription").val("");
    $("#isPaymentModeActive").prop("checked", true);

    // Set modal title
    $("#paymentModeModalTitle").text("Add Payment Mode");
}

//VALIDATION
function validateForm() {
    let modeName = $("#paymentModeName").val().trim();

    if (modeName === "") {
        Swal.fire("Validation", "Payment Mode name is required", "warning");
        return false;
    }

    return true;
}
