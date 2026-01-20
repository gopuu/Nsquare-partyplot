$(document).ready(function () {
    loadAssignmentData();
    loadSupportTicketDropdown();
    loadStaffDropdown();


    // ADD
    $("#btn-add").click(function () {
        resetAssignmentForm();
        $("#AssignmentModalTitle").text("Add Support Ticket Assignment");
        $("#AssignmentModal").modal("show");
    });

    // SAVE
    $("#btnSaveAssignment").click(function () {
        const payload = {
            Id: Number($("#AssignmentId").val()),
            TicketId: Number($("#TicketId").val()),
            AssignedTo: Number($("#ddlStaff").val()),
            AssignedBy: sessionStorage.getItem("userId") ?? 0,
            IsActive: $("#IsActive").prop("checked")
        };

        if (!payload.TicketId || !payload.AssignedTo || !payload.AssignedBy) {
            Swal.fire("Validation", "Ticket ID, Assigned To, and Assigned By are required", "warning");
            return;
        }

        $.ajax({
            url: "/api/SupportTicketAssignment/SaveSupportTicketAssignment",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved",
                        showConfirmButton: false,
                        timer: 1000
                    });
                    $("#AssignmentModal").modal("hide");
                    loadAssignmentData();
                } else {
                    Swal.fire("Info", res.message, "info");
                }
            },
            error: function (xhr, status, error) {
                console.error("Save Assignment Error:", error);
            }
        });
    });

    // EDIT
    $(document).on("click", ".btn-edit-assignment", function () {
        const table = $("#tableAssignments table").DataTable();
        const row = $(this).closest("tr");
        const data = table.row(row).data();

        $("#AssignmentId").val(data.Id);
        $("#TicketId").val(data.TicketId);
        $("#AssignedTo").val(data.AssignedTo);
        $("#AssignedBy").val(data.AssignedBy);
        $("#IsActive").prop("checked", data.IsActive);

        $("#AssignmentModalTitle").text("Edit Support Ticket Assignment");
        $("#AssignmentModal").modal("show");
    });

    // DELETE
    $(document).on("click", ".btn-delete-assignment", function () {
        const table = $("#tableAssignments table").DataTable();
        const data = table.row($(this).closest("tr")).data();
        $("#deleteAssignmentModal").data("id", data.Id).modal("show");
    });

    $("#delete-record").click(function () {
        const id = $("#deleteAssignmentModal").data("id");

        $.ajax({
            url: "/api/SupportTicketAssignment/DeleteSupportTicketAssignment",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: id, ActionBy: 1 }),
            success: function (res) {
                if (res.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Deleted",
                        showConfirmButton: false,
                        timer: 900
                    });
                    $("#deleteAssignmentModal").modal("hide");
                    loadAssignmentData();
                } else {
                    Swal.fire("Error", res.message, "error");
                }
            },
            error: function (xhr, status, error) {
                console.error("Delete Assignment Error:", error);
            }
        });
    });
});

// RESET FORM
function resetAssignmentForm() {
    $("#AssignmentId").val(0);
    $("#TicketId").val("");
    $("#AssignedTo").val("");
    $("#AssignedBy").val("");
    $("#IsActive").prop("checked", true);
}

// FETCH & RENDER TABLE
function loadAssignmentData() {
    $.ajax({
        url: "/api/SupportTicketAssignment/GetSupportTicketAssignmentList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, TicketId: 0 }),
        success: function (res) {
            renderAssignmentTable(res.data || []);
        },
        error: function (xhr, status, error) {
            console.error("Fetch Assignment Error:", error);
        }
    });
}

function renderAssignmentTable(data) {
    if ($.fn.dataTable.isDataTable("#tableAssignments table")) {
        $("#tableAssignments table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#tableAssignments table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "TicketDisplay", title: "Ticket" },
            { data: "AssignedToName", title: "Assigned To" },
            { data: "AssignedByName", title: "Assigned By" },
            
            {
                data: "IsActive",
                title: "Active",
                render: function (data) {
                    return data
                        ? "<span class='badge bg-success'>Yes</span>"
                        : "<span class='badge bg-danger'>No</span>";
                }
            },
            {
                data: null,
                title: "Action",
                render: function () {
                    return `
                        <button class="btn btn-outline-warning btn-sm btn-edit-assignment"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-outline-danger btn-sm btn-delete-assignment"><i class="fa fa-trash"></i></button>
                    `;
                }
            }
        ]
    });
}


function loadSupportTicketDropdown(selectedId = "") {
    $.ajax({
        url: "/api/SupportTicket/GetSupportTicketList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }), // fetch all tickets
        success: function (response) {
            let dropdown = $("#TicketId"); // ID of your <select>
            dropdown.empty();

            dropdown.append(`<option value="">-- Select Support Ticket --</option>`);

            response.data.forEach(item => {
                // Display as #TicketNumber - Title
                const displayText = `#${item.TicketNumber} - ${item.Title}`;
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                        ${displayText}
                    </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            console.error("Failed to load Support Ticket dropdown:", error);
        }
    });
}

function loadStaffDropdown(selectedId = null) {
    $.ajax({
        url: "/api/StaffMaster/GetStaffMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            let ddl = $("#ddlStaff");
            ddl.empty().append(`<option value="">-- Select Staff --</option>`);

            response.data.forEach(staff => {
                ddl.append(`<option value="${staff.Id}">${staff.Name}</option>`);
            });

            if (selectedId) ddl.val(selectedId);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}