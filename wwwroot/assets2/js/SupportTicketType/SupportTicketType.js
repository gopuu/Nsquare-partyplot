$(document).ready(function () {
    loadTicketTypeData();

    // ADD
    $("#btn-add").click(function () {
        resetForm();
        $("#TicketTypeModalTitle").text("Add Ticket Type");
        $("#TicketTypeModal").modal("show");
    });

    // SAVE
    $("#btnSave").click(function () {
        if (!$("#TypeCode").val() || !$("#TypeName").val()) {
            Swal.fire("Validation", "Code & Name required", "warning");
            return;
        }

        const payload = {
            Id: Number($("#TypeId").val()),
            Code: $("#TypeCode").val(),
            Name: $("#TypeName").val(),
            IsActive: $("#isActive").prop("checked"),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/SupportTicketType/SaveSupportTicketType",
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
                    $("#TicketTypeModal").modal("hide");
                    loadTicketTypeData();
                } else {
                    Swal.fire("Info", res.message, "info");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save Level Error");
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

        $("#TypeId").val(data.Id);
        $("#TypeCode").val(data.Code);
        $("#TypeName").val(data.Name);
        $("#isActive").prop("checked", data.IsActive === true);

        $("#TicketTypeModalTitle").text("Edit Ticket Type");
        $("#TicketTypeModal").modal("show");
    });

    // DELETE
    $(document).on("click", ".btn-delete", function () {
        let data = $("#tableMain table").DataTable().row($(this).closest("tr")).data();
        $("#deleteRecordModal").data("id", data.Id).modal("show");
    });

    $("#delete-record").click(function () {
        let id = $("#deleteRecordModal").data("id");

        $.ajax({
            url: "/api/SupportTicketType/DeleteSupportTicketType",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                Id: id,
                ActionBy: sessionStorage.getItem("userId") ?? 0
            }),
            success: function (res) {
                if (res.status === "Success") {
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
                        text: res.message,
                        icon: "error"
                    });
                }
                $("#deleteRecordModal").modal("hide");
                loadTicketTypeData();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete SupportTicket Type Error");
            }
        });
    });

});

// RESET
function resetForm() {
    $("#TypeId").val(0);
    $("#TypeCode").val("");
    $("#TypeName").val("");
    $("#IsActive").prop("checked", true);
}

// FETCH
function loadTicketTypeData() {
    $.ajax({
        url: "/api/SupportTicketType/GetSupportTicketTypeList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            renderTable(res.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Support Ticket Type Error");
        }
    });
}

// TABLE
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
                    <button class="btn btn-outline-warning btn-sm btn-edit"><i class="fa fa-edit"></i></button>
                    <button class="btn btn-outline-danger btn-sm btn-delete"><i class="fa fa-trash"></i></button>
                `
            }
        ]
    });
}





