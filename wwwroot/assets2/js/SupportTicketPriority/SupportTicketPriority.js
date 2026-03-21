$(document).ready(function () {
    loadPriorityData();

    // ADD
    $("#btn-add-priority").click(function () {
        resetPriorityForm();
        $("#PriorityModalTitle").text("Add Support Ticket Priority");
        $("#PriorityModal").modal("show");
    });

    // SAVE
    $("#btnSavePriority").click(function () {
        let code = $("#PriorityCode").val().trim();
        let name = $("#PriorityName").val().trim();

        if (!code || !name) {
            Swal.fire("Validation", "Code & Name are required", "warning");
            return;
        }

        const payload = {
            Id: Number($("#PriorityId").val()),
            Code: code,
            Name: name,
            SortOrder: Number($("#SortOrder").val() || 0),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/SupportTicketPriority/SaveSupportTicketPriority",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess("Saved Successfully");
                    $("#PriorityModal").modal("hide");
                    loadPriorityData();
                } else {
                    Swal.fire("Info", res.message, "info");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save SupportTicket Status Error");
            }
        });
    });

    // EDIT
    $(document).on("click", ".btn-edit", function () {
        let table = $("#PriorityTable table").DataTable();
        let data = table.row($(this).closest("tr")).data();

        $("#PriorityId").val(data.Id);
        $("#PriorityCode").val(data.Code);
        $("#PriorityName").val(data.Name);
        $("#SortOrder").val(data.SortOrder);

        $("#PriorityModalTitle").text("Edit Support Ticket Priority");
        $("#PriorityModal").modal("show");
    });

    // DELETE CLICK
    $(document).on("click", ".btn-delete", function () {
        let table = $("#PriorityTable table").DataTable();
        let data = table.row($(this).closest("tr")).data();
        $("#deletePriorityModal").data("id", data.Id).modal("show");
    });

    // CONFIRM DELETE
    $("#confirmDeletePriority").click(function () {
        let id = $("#deletePriorityModal").data("id");

        $.ajax({
            url: "/api/SupportTicketPriority/DeleteSupportTicketPriority",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                Id: id,
                ActionBy: sessionStorage.getItem("userId") ?? 0
            }),
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess("Deleted Successfully");
                }
                else {
                    Swal.fire({
                        title: "Delete Fail..!!",
                        text: res.message,
                        icon: "error"
                    });
                }
                $("#deletePriorityModal").modal("hide");
                loadPriorityData();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete SupportTicket Priority Error");
            }
        });
    });
});

// LOAD DATA
function loadPriorityData() {
    $.ajax({
        url: "/api/SupportTicketPriority/GetSupportTicketPriorityList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            renderPriorityTable(res.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Support Ticket Priority Error");
        }
    });
}

// TABLE
function renderPriorityTable(data) {
    if ($.fn.dataTable.isDataTable("#PriorityTable table")) {
        $("#PriorityTable table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#PriorityTable table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "Code", title: "Code" },
            { data: "Name", title: "Priority Name" },
            { data: "SortOrder", title: "Sort Order" },
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
        ],
        pageLength: 10,
        searching: true,
        ordering: true
    });
}

// RESET FORM
function resetPriorityForm() {
    $("#PriorityId").val(0);
    $("#PriorityCode").val("");
    $("#PriorityName").val("");
    $("#SortOrder").val("");
}
