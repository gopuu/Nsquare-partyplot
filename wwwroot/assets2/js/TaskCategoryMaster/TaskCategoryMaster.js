$(document).ready(function () {

    loadTaskCategoryData();

    // ➤ ADD BUTTON CLICK
    $("#btn-add-task-cat").click(function () {

        const canAdd = (sessionStorage.getItem("CanAdd") === "true");
        if (!canAdd) return Swal.fire("Access Denied", "You cannot Add.", "error");

        resetTaskCategoryForm();
        $("#TaskCategoryModalTitle").text("Add Task Category");
        $("#TaskCategoryModal").modal("show");
    });

    // ➤ SAVE TASK CATEGORY
    $("#btnSaveTaskCategory").click(function () {

        let name = $("#TaskCategoryName").val().trim();
        if (!name) return Swal.fire("Validation", "Category Name is required!", "warning");

        const payload = {
            Id: Number($("#TaskCategoryId").val() || 0),
            CategoryName: name,
            PriorityNo: Number($("#TaskPriorityNo").val() || 0),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $("#btnSaveTaskCategory").prop("disabled", true);

        $.ajax({
            url: "/api/TaskCategoryMaster/SaveTaskCategoryMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess("Task Category saved successfully.");
                    $("#TaskCategoryModal").modal("hide");
                    loadTaskCategoryData();
                } else {
                    Swal.fire("Error", res.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save TaskCategory Error");
            },
            complete: () => $("#btnSaveTaskCategory").prop("disabled", false)
        });
    });

    // ➤ EDIT BUTTON
    $(document).on("click", ".btn-task-edit", function () {

        const canEdit = (sessionStorage.getItem("CanEdit") === "true");
        if (!canEdit) return Swal.fire("Access Denied", "You cannot Edit.", "error");

        let row = $("#tableTaskCategory table").DataTable().row($(this).closest("tr")).data();

        resetTaskCategoryForm();
        $("#TaskCategoryId").val(row.Id);
        $("#TaskCategoryName").val(row.CategoryName);
        $("#TaskPriorityNo").val(row.PriorityNo);

        $("#TaskCategoryModalTitle").text("Edit Task Category");
        $("#TaskCategoryModal").modal("show");
    });

    // ➤ DELETE BUTTON
    $(document).on("click", ".btn-task-delete", function () {

        const canDelete = (sessionStorage.getItem("CanDelete") === "true");
        if (!canDelete) return Swal.fire("Access Denied", "You cannot Delete.", "error");

        let row = $("#tableTaskCategory table").DataTable().row($(this).closest("tr")).data();

        $("#deleteTaskCategoryModal").data("id", row.Id).modal("show");
    });

    // ➤ CONFIRM DELETE
    $("#delete-task-cat").click(function () {

        let id = $("#deleteTaskCategoryModal").data("id");

        const payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/TaskCategoryMaster/DeleteTaskCategoryMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {

                if (res.status === "Success") {
                    handleApiSuccess("Task Category deleted successfully.");
                } else {
                    Swal.fire("Error", res.message, "error");
                }

                $("#deleteTaskCategoryModal").modal("hide");
                loadTaskCategoryData();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete TaskCategory Error");
            }
        });
    });

});

// RESET FORM
function resetTaskCategoryForm() {
    $("#TaskCategoryId").val(0);
    $("#TaskCategoryName").val("");
    $("#TaskPriorityNo").val("");
}

// LOAD DATA
function loadTaskCategoryData() {

    $.ajax({
        url: "/api/TaskCategoryMaster/GetTaskCategoryMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            renderTaskCategoryTable(res.data);
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, status, error, "Failed to Load Task Category");
        }
    });
}

// RENDER TABLE
function renderTaskCategoryTable(data) {

    if ($.fn.dataTable.isDataTable("#tableTaskCategory table")) {
        $("#tableTaskCategory table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#tableTaskCategory table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "CategoryName", title: "Category Name" },
            { data: "PriorityNo", title: "Priority No" },
            {
                data: null,
                title: "Actions",
                render: () => `
                    <button class="btn btn-outline-warning btn-action-xss btn-task-edit">
                        <i class="fa fa-edit"></i>
                    </button>
                    <button class="btn btn-outline-danger btn-action-xss btn-task-delete">
                        <i class="fa fa-trash"></i>
                    </button>
                `
            }
        ]
    });
}
