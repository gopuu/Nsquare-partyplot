$(document).ready(function () {

    loadDutyRoleData();

    // ➤ ADD BUTTON CLICK
    $("#btn-add").click(function () {

        const canAdd = (sessionStorage.getItem("CanAdd") === "true");
        if (!canAdd) return Swal.fire("Access Denied", "You cannot Add.", "error");

        resetForm();
        $("#DutyRoleModalTitle").text("Add Duty Role");
        $("#DutyRoleModal").modal("show");
    });

    // ➤ SAVE DUTY ROLE
    $("#btnSaveDutyRole").click(function () {

        let roleName = $("#RoleName").val();
        if (!roleName) {
            Swal.fire("Validation", "Role Name is required!", "warning");
            return;
        }

        const payload = {
            Id: Number($("#DutyRoleId").val() || 0),
            RoleName: roleName,
            PriorityNo: Number($("#PriorityNo").val() || 0),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $("#btnSaveDutyRole").prop("disabled", true);

        $.ajax({
            url: "/api/DutyRoleMaster/SaveDutyRoleMaster",
            type: "POST",
            data: JSON.stringify(payload),
            contentType: "application/json",
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess("Duty Role saved successfully.");
                    $("#DutyRoleModal").modal("hide");
                    loadDutyRoleData();
                } else {
                    Swal.fire("Error", res.message, "error");
                }
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, status, error, "Failed to Save Duty Role");
            },
            complete: () => $("#btnSaveDutyRole").prop("disabled", false)
        });
    });

    // ➤ EDIT BUTTON
    $(document).on("click", ".btn-edit", function () {

        const canEdit = (sessionStorage.getItem("CanEdit") === "true");
        if (!canEdit) return Swal.fire("Access Denied", "You cannot Edit.", "error");

        let row = $("#tableDutyRole table").DataTable().row($(this).closest("tr")).data();

        resetForm();
        $("#DutyRoleId").val(row.Id);
        $("#RoleName").val(row.RoleName);
        $("#PriorityNo").val(row.PriorityNo);

        $("#DutyRoleModalTitle").text("Edit Duty Role");
        $("#DutyRoleModal").modal("show");
    });

    // ➤ DELETE BUTTON
    $(document).on("click", ".btn-delete", function () {

        const canDelete = (sessionStorage.getItem("CanDelete") === "true");
        if (!canDelete) return Swal.fire("Access Denied", "You cannot Delete.", "error");

        let row = $("#tableDutyRole table").DataTable().row($(this).closest("tr")).data();

        $("#deleteDutyRoleModal").data("id", row.Id).modal("show");
    });

    // ➤ CONFIRM DELETE
    $("#delete-dutyrole").click(function () {

        let id = $("#deleteDutyRoleModal").data("id");

        const payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/DutyRoleMaster/DeleteDutyRoleMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {

                if (res.status === "Success") {
                    handleApiSuccess("Duty Role deleted successfully.");
                } else {
                    Swal.fire("Error", res.message, "error");
                }

                $("#deleteDutyRoleModal").modal("hide");
                loadDutyRoleData();
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, status, error, "Failed to Delete Duty Role");
            }
        });
    });

});

// RESET FORM
function resetForm() {
    $("#DutyRoleId").val(0);
    $("#RoleName").val("");
    $("#PriorityNo").val("");
}

// LOAD DATA
function loadDutyRoleData() {

    $.ajax({
        url: "/api/DutyRoleMaster/GetDutyRoleMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            renderDutyRoleTable(res.data);
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, status, error, "Failed to Load Duty Role");
        }
    });
}

// RENDER TABLE
function renderDutyRoleTable(data) {

    if ($.fn.dataTable.isDataTable("#tableDutyRole table")) {
        $("#tableDutyRole table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#tableDutyRole table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "RoleName", title: "Role Name" },
            { data: "PriorityNo", title: "Priority No" },
            {
                data: null,
                title: "Actions",
                render: () => `
                        <button class="btn btn-outline-warning btn-action-xs  btn-edit"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-outline-danger btn-action-xs btn-delete"><i class="fa fa-trash"></i></button>
                `
            }
        ]
    });
}
