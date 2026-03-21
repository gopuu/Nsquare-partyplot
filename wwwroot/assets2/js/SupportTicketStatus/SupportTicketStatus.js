$(document).ready(function () {
    loadTicketStatusData();

    // ADD
    $("#btn-add").click(function () {
        resetForm();
        $("#TicketStatusModalTitle").text("Add Ticket Status");
        $("#TicketStatusModal").modal("show");
    });

    // SAVE
    $("#btnSave").click(function () {

        if (!$("#StatusCode").val() || !$("#StatusName").val()) {
            Swal.fire("Validation", "Code & Name required", "warning");
            return;
        }

        const payload = {
            Id: Number($("#StatusId").val()),
            Code: $("#StatusCode").val(),
            Name: $("#StatusName").val(),
            IsFinal: $("#IsFinal").prop("checked"),
            IsActive: $("#isActive").prop("checked"),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/SupportTicketStatus/SaveSupportTicketStatus",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess("Saved successfully.");
                    $("#TicketStatusModal").modal("hide");
                    loadTicketStatusData();
                } else {
                    Swal.fire("Info", res.message, "info");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save SupportTicket Status Error");
            },
        });
    });

    // EDIT
    $(document).on("click", ".btn-edit", function () {
        // Get the DataTable instance
        var table = $('#tableMain table').DataTable();

        // Get the row object for the clicked Edit button
        var row = $(this).closest('tr');
        var data = table.row(row).data(); 

        $("#StatusId").val(data.Id);
        $("#StatusCode").val(data.Code);
        $("#StatusName").val(data.Name);
        $("#IsFinal").prop("checked", data.IsFinal === true);
        $("#isActive").prop("checked", data.IsActive === true);

        $("#TicketStatusModalTitle").text("Edit Ticket Status");
        $("#TicketStatusModal").modal("show");
    });

    // DELETE
    $(document).on("click", ".btn-delete", function () {
        let data = $("#tableMain table").DataTable()
            .row($(this).closest("tr")).data();

        $("#deleteRecordModal")
            .data("id", data.Id)
            .modal("show");
    });

    $("#delete-record").click(function () {
        let id = $("#deleteRecordModal").data("id");

        $.ajax({
            url: "/api/SupportTicketStatus/DeleteSupportTicketStatus",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                Id: id,
                ActionBy: sessionStorage.getItem("userId") ?? 0
            }),
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess("Deleted successfully.");
                }
                else {
                    Swal.fire({
                        title: "Delete Fail..!!",
                        text: res.message,
                        icon: "error"
                    });
                }

                $("#deleteRecordModal").modal("hide");
                loadTicketStatusData();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete SupportTicket Type Error");
            }
        });
    });
});

// RESET FORM
function resetForm() {
    $("#StatusId").val(0);
    $("#StatusCode").val("");
    $("#StatusName").val("");
    $("#IsFinal").prop("checked", false);
    $("#IsActive").prop("checked", true);
}

// FETCH DATA
function loadTicketStatusData() {
    $.ajax({
        url: "/api/SupportTicketStatus/GetSupportTicketStatusList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            renderTable(res.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Support Ticket status Error");
        }
    });
}

// RENDER TABLE
function renderTable(data) {

    if ($.fn.dataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#tableMain table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "Code", title: "Code" },
            { data: "Name", title: "Name" },
            {
                data: "IsFinal",
                title: "Final",
                render: function (data) {
                    return data
                        ? "<span class='badge bg-success'>Yes</span>"
                        : "<span class='badge bg-danger'>No</span>";
                }
            },
            {
                data: 'IsActive',
                title: 'Active',
                render: function (data) {
                    return data
                        ? "<span class='badge bg-success'>Yes</span>"
                        : "<span class='badge bg-danger'>No</span>";
                }
            },
            {
                data: null,
                title: "Action",
                render: () => `
                    <button class="btn btn-outline-warning btn-sm btn-edit">
                        <i class="fa fa-edit"></i>
                    </button>
                    <button class="btn btn-outline-danger btn-sm btn-delete">
                        <i class="fa fa-trash"></i>
                    </button>`
            }
        ]
    });
}
