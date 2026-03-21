$(document).ready(function () {
    loadTaskDropdown();
    resetTaskCommentForm();
    loadComments();

    $("#btn-add").click(function () {
        const canAdd = (sessionStorage.getItem("CanAdd") === "true");
        if (!canAdd) return Swal.fire("Access Denied", "You cannot Add.", "error");

        resetTaskCommentForm();
        $("#taskCommentModalTitle").text("Add Task Comment");
        $("#TaskCommentModal").modal("show");
    });

    // ➤ Save Comment
    $("#btnSaveComment").click(function () {

        let text = $("#TaskCommenttext").val().trim();
        let taskId = Number($("#ddlTaskId").val());

        const payload = {
            Id: Number($("#TaskCommentId").val() || 0),
            TaskId: taskId,
            CommentText: text,
            CommentedBy: sessionStorage.getItem("userId") ?? 0,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/TaskComments/SaveTaskComments",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    handleApiSuccess("Saved!");
                    loadComments();
                    $("#TaskCommentModal").modal("hide");
                } else {
                    Swal.fire("Error", res.message, "error");
                }
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, status, error, "Failed to Save Comments");
            }

        });
    });

    // ➤ Edit
    $(document).on("click", ".btn-edit-comment", function () {

        // Get selected row data
        let row = $("#tableTaskCommets table")
            .DataTable()
            .row($(this).closest("tr"))
            .data();

        // Reset form first (optional but recommended)
        resetTaskCommentForm();

        // Fill modal fields
        $("#TaskCommentId").val(row.Id);
        $("#TaskCommenttext").val(row.CommentText);

        // Select task from dropdown
        $("#ddlTaskId").val(row.TaskId).trigger("change");

        // Update modal title
        $("#taskCommentModalTitle").text("Edit Task Comment");

        // Show modal
        $("#TaskCommentModal").modal("show");
    });

    // ----------------- DELETE BUTTON -----------------
    $(document).on("click", ".btn-delete", function () {

        const canDelete =
            (sessionStorage.getItem('CanDelete') || localStorage.getItem('CanDelete')) === 'true';

        if (!canDelete) {
            Swal.fire("Access Denied", "You do not have permission to Delete.", "error");
            return;
        }

        let table = $("#tableTaskCommets table").DataTable();
        let rowData = table.row($(this).closest("tr")).data();

        // Store ID inside modal
        $("#deleteRecordModal")
            .data("id", rowData.Id)
            .modal("show");
    });

    // ----------------- CONFIRM DELETE -----------------
    $("#delete-record").click(function () {

        let id = $("#deleteRecordModal").data("id");

        const payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/TaskComments/DeleteTaskComments",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {

                if (res?.status === "Success") {
                    handleApiSuccess("Deleted!");
                } else {
                    Swal.fire("Delete Failed", res?.message || "Unable to delete", "error");
                }

                $("#deleteRecordModal").modal("hide");

                // Reload Comments
                loadComments();
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, status, error, "Failed to Delete");
            }
        });
    });


});

function loadTaskDropdown() {
    $.ajax({
        url: "/api/TaskMaster/GetTaskMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            let options = `<option value="">-- Select Task --</option>`;
            res.data.forEach(task => {
                options += `<option value="${task.Id}">${task.TaskTitle}</option>`;
            });
            $("#ddlTaskId").html(options);
        }
    });
}


function resetTaskCommentForm() {
    $("#TaskCommentId").val(0);             // Reset hidden ID
    $("#ddlTaskId").val("").change();       // Reset dropdown
    $("#TaskCommenttext").val("");          // Clear comment textbox

    $("#taskCommentModalTitle").text("Add Task Comment");  // Reset modal title
}

function renderCommentTable(data) {

    // Destroy if exists
    if ($.fn.dataTable.isDataTable("#tableTaskCommets table")) {
        $("#tableTaskCommets table").DataTable().destroy();
    }

    // Add Sr No
    data.forEach((x, i) => x.srno = i + 1);

    // Initialize DataTable
    $("#tableTaskCommets table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "CommentText", title: "Comment" },
            { data: "TaskTitle", title: "Task" },
            {
                data: null,
                title: "Actions",
                render: () => `
                    <button class="btn btn-outline-warning btn-action-xss btn-edit-comment">
                        <i class="fa fa-edit"></i>
                    </button>
                        <button class="btn btn-outline-danger btn-action-xss btn-delete">
                        <i class="fa fa-trash"></i>
                    </button>
                `
            }
        ]
    });
}

function loadComments(taskId = 0) {
    $.ajax({
        url: "/api/TaskComments/GetTaskComments",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ TaskId: taskId }),
        success: function (res) {

            if (res.status === "Success") {
                renderCommentTable(res.data);
            } else {
                Swal.fire("Error", res.message, "error");
            }
        }
    });
}


