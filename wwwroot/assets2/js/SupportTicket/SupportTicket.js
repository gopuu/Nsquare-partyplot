$(document).ready(function () {
   
    loadSupportTicketData();
    loadTicketTypeDropdown();
    loadPriorityDropdown();
    loadStatusDropdown();
});

// ADD
$("#btn-add").click(function () {
    resetSupportTicketForm();
    $("#ticketModalTitle").text("Add Support Ticket");
    $("#SupportTicketModal").modal("show");
});

// EDIT
$(document).on("click", ".btn-edit", function () {

    let table = $("#tableMain table").DataTable();
    let data = table.row($(this).closest("tr")).data();

    $("#TicketId").val(data.Id);
    $("#TicketNumber").val(data.TicketNumber).prop("disabled", true);
    $("#Title").val(data.Title);
    $("#Description").val(data.Description);
    $("#IsClosed").prop("checked", data.IsClosed);

    // 🔥 SAME pattern as IncomeCategory
    loadTicketTypeDropdown(data.TicketTypeId);
    loadPriorityDropdown(data.PriorityId);
    loadStatusDropdown(data.StatusId);

    $("#ticketModalTitle").text("Edit Support Ticket");
    $("#SupportTicketModal").modal("show");
});



$("#btnSave").click(function () {

    // ===== BASIC VALIDATION =====
    if ($("#TicketNumber").val().trim() === "") {
        Swal.fire("Info", "Ticket Number is required", "info");
        return;
    }
    
    const payload = {
        Id: parseInt($("#TicketId").val()) || 0,
        TicketNumber: $("#TicketNumber").val().trim(),
        TicketTypeId: parseInt($("#TicketTypeId").val()),
        PriorityId: parseInt($("#PriorityId").val()),
        StatusId: parseInt($("#StatusId").val()),
        Title: $("#Title").val().trim(),
        Description: $("#Description").val().trim(),
        IsClosed: $("#IsClosed").is(":checked"),
        ActionBy: parseInt(sessionStorage.getItem("userId")) || 0
    };

    $.ajax({
        url: "/api/SupportTicket/SaveSupportTicket",
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
                $("#SupportTicketModal").modal("hide");
                loadSupportTicketData();
            } else {
                Swal.fire("Info", res.message, "info");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Save SupportTicket Error");
        }
    });
});

function resetSupportTicketForm() {
    $("#TicketId").val(0);
    $("#TicketNumber").val("").prop("disabled", false);
    $("#TicketTypeId").val("");
    $("#PriorityId").val("");
    $("#StatusId").val("");
    $("#Title").val("");
    $("#Description").val("");
    $("#IsClosed").prop("checked", false);
}

function loadSupportTicketData() {
    $.ajax({
        url: "/api/SupportTicket/GetSupportTicketList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            renderSupportTicketTable(res.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Support Ticket  Error");
        }
    });
}
function renderSupportTicketTable(data) {

    if ($.fn.dataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#tableMain table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "Title", title: "Title" },
            { data: "TicketNumber", title: "Ticket No" },
            { data: "TicketTypeName", title: "TicketType Name" },
            { data: "PriorityName", title: "Priority" },
            { data: "StatusName", title: "Status" },
            {
                data: "IsClosed",
                title: "Closed",
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
                        <button class="btn btn-outline-warning btn-sm btn-edit">
                            <i class="fa fa-edit"></i>
                        </button>
                        <button class="btn btn-outline-danger btn-sm btn-delete">
                            <i class="fa fa-trash"></i>
                        </button>
                    `;
                }
            }
        ]
    });
}


function loadTicketTypeDropdown(selectedId = "") {
    $.ajax({
        url: "/api/SupportTicketType/GetSupportTicketTypeList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            let dropdown = $("#TicketTypeId");
            dropdown.empty();

            dropdown.append(`<option value="">-- Select Ticket Type --</option>`);

            response.data.forEach(item => {
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                        ${item.Name}
                    </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load Ticket Type");
        }
    });
}



function loadPriorityDropdown(selectedId = "") {
    $.ajax({
        url: "/api/SupportTicketPriority/GetSupportTicketPriorityList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            let dropdown = $("#PriorityId");
            dropdown.empty();

            dropdown.append(`<option value="">-- Select Priority --</option>`);

            response.data.forEach(item => {
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                        ${item.Name}
                    </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load Priority");
        }
    });
}


function loadStatusDropdown(selectedId = "") {
    $.ajax({
        url: "/api/SupportTicketStatus/GetSupportTicketStatusList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            let dropdown = $("#StatusId");
            dropdown.empty();

            dropdown.append(`<option value="">-- Select Status --</option>`);

            response.data.forEach(item => {
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                        ${item.Name}
                    </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load Status");
        }
    });
}

