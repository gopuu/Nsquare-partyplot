$(document).ready(function () {
    fetchData();
    AppSignalR.subscribeSettings((data) => {
        //console.log("Settings Page Update", data);
        fetchData();
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

            // Extract values from the row data object
            var id = rowData.Id;
            var name = rowData.Name;
            var value = rowData.Value;

            // Populate the modal with the values
            $('#Id').val(id); // Populate hidden input or ID field in the modal
            $('#Name').text(name);
            $('#Value').val(value);

            // Show the modal with the pre-filled data
            $("#addEventModalTitle").text("Edit Settings");
            $('#addEventModal').modal('show');
        }
    });

    // SAVE - Handle form submission
    $("#btnSaveAddEventModal").click(function (event) {
        event.preventDefault(); // Prevent the default form submission
        if (!validateAddEventForm()) return;

        let id = $("#Id").val();
        let value = $("#Value").val().trim();

        let payload = {
            Id: id,
            Name: '',
            Value: value,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/Settings/SaveSettings",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            success: function (response) {
                //var obj = JSON.parse(response);
                var obj = response;
                ////console.log("submit obj:", obj);
                if (obj.status.toLowerCase() == "success") {
                    handleApiSuccess();
                    fetchData();
                    resetFormFields();  // Reset the form fields
                    $('#addEventModal').modal('hide');  // Close the modal
                } else {
                    $('#message').removeAttr('hidden');
                    $('#message').text(obj.message).css('color', 'red').fadeIn(300).delay(5000).fadeOut(300);
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save Failed")
            }
        });
    });

});

function renderTable(data) {
    //console.log("data:", data);

    // Destroy existing DataTable if present
    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    // Check if data is null or empty
    if (!data || data.length === 0) {
        //console.log("No Data Found");

        // Ensure table headers remain visible and add "No Data Found" row in the tbody
        $('#tableMain tbody').html(`
                        <tr>
                            <td colspan="4" class="text-center fw-bold text-danger">No Data Found !!</td>
                        </tr>
                    `);

        // Initialize an empty DataTable with headers intact
        $('#tableMain table').DataTable({
            data: [],
            columns: [
                { data: 'srno', title: 'Sr. No.' },
                { data: 'Name', title: 'Name' },
                { data: 'Value', title: 'Value' },
                {
                    data: null,
                    title: 'Actions',
                    render: function (data, type, row) {
                        return `
                                <button class="btn btn-outline-warning btn-sm me-1 mb-0 btn-edit">
                                    <i class="fa fa-edit"></i>
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

    processData = data.filter(item =>
        item.Name?.trim().toLowerCase() !== "show limit"
    );

    // Adding Sr. No.
    processData.forEach((item, index) => {
        item.srno = index + 1;
    });

    //console.log("processData:", processData);

    // Destroy the previous DataTable instance before reinitializing
    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy(); // Destroy the existing DataTable instance
    }

    $('#tableMain table').DataTable({
        data: processData,
        columns: [
            { data: 'srno', title: 'Sr. No.' },
            { data: 'Name', title: 'Name' },
            { data: 'Value', title: 'Value' },
            {
                data: null,
                title: 'Actions',
                render: function (data, type, row) {
                    return `
                        <div class="d-flex flex-column flex-sm-row gap-2">
                            <button class="btn btn-outline-warning btn-sm me-1 mb-0 btn-action-xss btn-edit">
                                <i class="fa fa-edit"></i>
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
        pageLength: 10,           // Items per page
        ordering: true,          // Enable sorting
        searching: true,         // Enable searching
        lengthChange: false,     // Hide page length change
        pagingType: "full_numbers" // Show Prev, Next, and Page Numbers
    });
}

// Initialize sorting
function initializeSorting() {
    const options = {
        valueNames: ["srno", "name", "value"],
        page: 10,
        pagination: true
    };
    const list = new List('tableMain', options); // Initialize List.js or your sorting library
}

//get data from API
function fetchData() {
        $.ajax({
        url: "/api/Settings/GetSettings",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            //data = response; // Store the fetched data
            //console.log("response fetchData:", response);
            var data = response.data;
            renderTable(data);
        },
        error: function (xhr, statue, error) {
            handleApiError(xhr, statue, error, "Fetch Settings Failed");
        }
    });
}

// Reset form fields
function resetFormFields() {
    $("#iId").val(0);
    $("#Name").text("");
    $("#Value").val("");
}

// Validate form fields
function validateAddEventForm() {

    let name = $("#Name").text().trim();
    let value = $("#Value").val().trim();

    // Required check
    if (value === "") {
        Swal.fire("Validation", "Value is required", "warning");
        return false;
    }

    // Special rule for Grace Time
    if (name === "Booking Grace Time (in Mins)") {

        // Must be positive whole number
        if (!/^\d+$/.test(value) || parseInt(value) <= 0) {
            Swal.fire(
                "Validation",
                "Grace time must be a positive number (minutes only)",
                "warning"
            );
            return false;
        }

    }

    return true;
}