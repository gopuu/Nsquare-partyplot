$(document).ready(function () {
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
            $('#addEventModal').modal('hide');
        } else {
            resetFormFields(); // Reset form fields when modal is shown
            $("#addEventModalTitle").text("Add Reason for Asset-Remove");
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
            let row = $(this).closest("tr");

            let rowData = table.row(row).data();

            if (!rowData) return;

            // Extract values
            let id = rowData.Id;
            let title = rowData.ReasonTitle;
            let description = rowData.ReasonDescription;

            // Populate modal
            $("#reasonId").val(id);
            $("#reasonTitle").val(title);
            $("#reasonDescription").val(description);

            // Show the modal with the pre-filled data
            $("#addEventModalTitle").text("Edit Reason for Asset-Remove");
            $('#addEventModal').modal('show');
        }
    });

    // SAVE - Handle form submission
    $("#btnSaveAddEventModal").click(function (event) {
        event.preventDefault(); // Prevent the default form submission
        let id = $("#reasonId").val();
        let title = $("#reasonTitle").val().trim();
        let description = $("#reasonDescription").val().trim();

        if (title === "") {
            Swal.fire("Validation", "Reason Title is required", "warning");
            $("#reasonTitle").focus();
            return;
        }

        let payload = {
            Id: id,
            ReasonTitle: title,
            ReasonDescription: description || null,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/AssetRemove_ReasonMaster/SaveAssetRemoveReason",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            success: function (response) {
                let obj = response;
                if (obj.status.toLowerCase() === "success") {
                    handleApiSuccess('Saved!');
                    fetchData();              // reload table
                    resetFormFields();        // reset modal
                    $("#addEventModal").modal("hide");
                }
                else {
                    $("#message")
                        .removeClass("d-none")
                        .text(obj.message)
                        .css("color", "red")
                        .fadeIn(300)
                        .delay(5000)
                        .fadeOut(300);
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save Reason Error")
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

        $.ajax({
            url: "/api/AssetRemove_ReasonMaster/DeleteAssetRemoveReason",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
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
                $('#deleteRecordModal').modal('hide');
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete failed");
            }
        });
        $('#deleteRecordModal').modal('hide');
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
                            <td colspan="4" class="text-center fw-bold text-danger">No Data Found !!</td>
                        </tr>
                    `);

        // Initialize an empty DataTable with headers intact
        $('#tableMain table').DataTable({
            data: [],
            columns: [
                { data: 'srno', title: '#' },
                { data: 'ReasonTitle', title: 'Title' },
                { data: 'ReasonDescription', title: 'Description' },
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
            { data: 'srno', title: '#' },
            { data: 'ReasonTitle', title: 'Title' },
            { data: 'ReasonDescription', title: 'Description' },
            {
                data: null,
                title: 'Actions',
                render: function (data, type, row) {
                    return `
                        <div class="d-flex flex-column flex-sm-row gap-2">
                            <button class="btn btn-outline-warning btn-sm me-1 mb-0 btn-action-xss btn-edit">
                                <i class="fa fa-edit"></i>
                            </button>
                            <button class="btn btn-outline-danger btn-sm me-1 mb-0 btn-action-xss btn-delete">
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
        valueNames: ["srno", "title", "description"],
        page: 10,
        pagination: true
    };
    const list = new List('tableMain', options); // Initialize List.js or your sorting library
}

//get data from API
function fetchData() {
    $.ajax({
        url: "/api/AssetRemove_ReasonMaster/GetAssetRemoveReason",
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
            handleApiError(xhr, status, error, "Fetch AssetRemove-Reason failed");
        }
    });
}

// Reset form fields
function resetFormFields() {
    $("#reasonId").val(0);
    $("#reasonTitle").val("");
    $("#reasonDescription").val("");
}
