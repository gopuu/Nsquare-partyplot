//PROCESS MASTER -- START --------------------------------------------------------------------

document.addEventListener("DOMContentLoaded", function () {
    loadProcessList();
   
});

let openedFromDeal = false; 

// Open from Deal Modal (+ button)
$(document).on("click", "#openProcessModal, #btn-add", function () {
    openedFromDeal = true; // mark flag
    $("#dealModal").modal("hide"); // hide deal modal
    $('#processModalLabel').text("Add Process");

    $("#processModal").modal("show");
    resetProcessForm();
});

// Add Process
$("#btnprocess").click(function (e) {
    e.preventDefault();

    if (!validateProcessForm()) {
        console.log("Validation failed");
        return;
    }

    let inputData = {
        Id: $("#processId").val(),
        Name: $("#processName").val(),
        IndexNo: $("#processIndexNo").val()
        
    };

    $.ajax({
        url: "/api/Process/SaveProcess",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(inputData),
        success: function (response) {
            if (response.status === "Success") {
                Swal.fire({
                    position: "top-end",
                    icon: "success",
                    title: "Saved!",
                    showConfirmButton: false,
                    timer: 900
                });

                resetProcessForm();
                loadProcessList();
                $("#processModal").modal("hide");
            } else {
                Swal.fire("Error", response.message, "error");
            }
        },
        error: function () {
            Swal.fire("Error", "Error saving process.", "error");
        }
    });
});


// Edit Process
$(document).on("click", ".edit-btn", function () {

    let table = $("#tableMain table").DataTable();
    let rowData = table.row($(this).closest("tr")).data();

    if (!rowData) return;

    resetProcessForm();

    $("#processId").val(rowData.Id);
    $("#processName").val(rowData.Name);
    $("#processIndexNo")
        .prop("disabled", false)
        .val(rowData.IndexNo);

    $('#processModalLabel').text("Edit Process");
    $("#processModal").modal("show");
});


$(document).on("click", ".btn-delete", function () {
    const canDelete = (sessionStorage.getItem('CanDelete') || localStorage.getItem('CanDelete')) === 'true';
    if (!canDelete) {
        Swal.fire("Access Denied", "You do not have permission to Delete.", "error");
        return;
    }
    let table = $("#tableMain table").DataTable();
    let rowData = table.row($(this).closest("tr")).data();
    $("#deleteRecordModal")
        .data("id", rowData.Id)
        .modal("show");
});

// ----------------- CONFIRM DELETE -----------------
$("#delete-record").on("click", function () {
    let id = $("#deleteRecordModal").data("id");

    $.ajax({
        url: "/api/Process/DeleteProcess",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        }),
        success: function (res) {
            if (res?.status === "Success") {
                Swal.fire({
                    position: "top-end",
                    icon: "success",
                    title: "Deleted Successfully!",
                    showConfirmButton: false,
                    timer: 1000
                });

                $("#deleteRecordModal").modal("hide");
                loadProcessList(); // ✅ correct reload
            } else {
                Swal.fire("Delete Failed", res?.message || "Unable to delete", "error");
            }
        }
    });
});


// Load process list from API
var last_processIndexNo = 0;
function loadProcessList11() {
    ////showLoader()();
    $.ajax({
        url: "/api/Process/GetProcessList",   // adjust path if needed
        type: "GET",
        success: function (response) {
            console.log("process response:", response);
            if (response.status === "Success") {
                let rows = "";
                let srNo = 1;
                let options = `<option value="0">-- Select --</option>`; // default option

                $.each(response.data, function (index, item) {
                    rows += `
                            <tr data-id="${item.Id}">
                                <td class="text-center">${srNo++}</td>
                                <td>${item.Name}</td>
                                <td class="text-center">${item.IndexNo}</td>
                                <td class="text-center">
                                    <button class="btn btn-outline-warning btn-action-xs me-1 edit-btn">
                                        <i class="fa fa-pencil"></i>
                                    </button>
                                    <button class="btn btn-outline-danger btn-action-xs delete-btn">
                                        <i class="fa fa-trash"></i>
                                    </button>
                                </td>
                            </tr>
                        `;
                    // dropdown options
                    options += `<option value="${item.Id}">${item.Name}</option>`;
                    last_processIndexNo = item.IndexNo;
                    //console.log("loop - last_processIndexNo:", last_processIndexNo);
                });
                //console.log("process options:", options);
                $("#processTable tbody").html(rows);
                $("#dealProcess").html(options);
            } else {
                $("#processTable tbody").html(
                    `<tr><td colspan="3" class="text-center text-danger">No records found</td></tr>`
                );
                $("#dealProcess").html(`<option value="0">No Process Found</option>`); // reset dropdown
            }
        },
        error: function () {
            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Error fetching process list.",
                confirmButtonText: "OK",
            });
        },
        complete: function () {
            //hideLoader();
        }
    });
}

function loadProcessList() {
    $.ajax({
        url: "/api/Process/GetProcessList",
        type: "GET",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            renderProcessTable(res?.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Level Error");
        }
    });

}

function renderProcessTable(data) {

    if ($.fn.dataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }

    last_processIndexNo = 0;
    if (data.length > 0) {
        last_processIndexNo = Math.max(...data.map(x => parseInt(x.IndexNo)));
    }


    data.forEach((item, i) => item.srno = i + 1);
    $("#tableMain table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr. No." },
            { data: "Name", title: "Name" },
            { data: "IndexNo", title: "Index No." },
            {
                data: null,
                title: "Actions",
                render: function (data, type, row) {
                    return `
                            <button class="btn btn-outline-warning btn-sm edit-btn"
                                    data-id="${row.Id}">
                                <i class="fa fa-edit"></i>
                            </button>
                            <button class="btn btn-outline-danger btn-sm btn-delete"
                                    data-id="${row.Id}">
                                <i class="fa fa-trash"></i>
                            </button>`; 
                }

            }
        ],
        pageLength: 10,
        ordering: true,
        searching: true
    });
}


// Reset Process Form
function resetProcessForm() {
    $("#processForm")[0].reset();   // clear input fields
    $("#processId").val(0);         // reset hidden ID to 0
    $("#processIndexNo").prop("disabled", true).val(parseInt(last_processIndexNo) + 1);
    // Reset error message
    $("#processName").siblings(".error-msg").addClass("d-none");
}

// ✅ Validation Function for Event Type Form
function validateProcessForm() {
    let isValid = true;

    let name = $("#processName").val().trim();

    // Reset error message
    $("#processName").siblings(".error-msg").addClass("d-none");

    // Validate Event Type Name
    if (name === "" || name === null) {
        $("#processName").siblings(".error-msg").removeClass("d-none").text("Please enter Process Name");
        isValid = false;
    }

    return isValid;
}

//PROCESS MASTER -- END -----------------------------------------------------------------------
