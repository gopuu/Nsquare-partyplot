$(document).ready(function () {

    loadTaskData();
    loadCategoryDropdown();
    loadStatusDropdown();
    loadStaffDropdown();
    loadPriorityLevelDropdown();
    setDefaultDates();

    // ➤ ADD TASK BUTTON
    $("#btn-add-task").click(function () {
        if (sessionStorage.getItem("CanAdd") !== "true")
            return Swal.fire("Access Denied", "You cannot Add.", "error");

        resetTaskForm();
        $("#TaskModalTitle").text("Add Task");
        $("#TaskModal").modal("show");
    });

    // ➤ SAVE TASK
    $("#btnSaveTask").click(function () {

        let title = $("#TaskTitle").val().trim();
        if (!title) return Swal.fire("Validation", "Task Title required!", "warning");

        const payload = {
            Id: Number($("#TaskId").val() || 0),
            TaskTitle: title,
            TaskDescription: $("#TaskDescription").val().trim(),
            CategoryId: $("#TaskCategoryId").val(),
            AssignedTo: $("#TaskAssignedTo").val(),
            AssignedBy: $("#TaskAssignedBy").val(), // current user
            PriorityLevel: $("#PriorityLevel").val(),
            StartDate: $("#StartDate").val(),
            DueDate: $("#DueDate").val(),
            CompletedDate: $("#CompletedDate").val(),
            StatusId: $("#TaskStatusId").val(),
            ActionBy: sessionStorage.getItem("userId")
        };

        $.ajax({
            url: "/api/TaskMaster/SaveTaskMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 1200
                    });
                    // Modal hide & reload after small delay
                    setTimeout(() => {
                        $("#TaskModal").modal("hide");
                        loadTaskData();
                    }, 1200);
                } else {
                    Swal.fire("Error", res.message, "error");
                }
            },
            error: handleApiError
        });
    });

    // ➤ EDIT BUTTON
    $(document).on("click", ".btn-task-edit", function () {

        if (sessionStorage.getItem("CanEdit") !== "true")
            return Swal.fire("Access Denied", "You cannot Edit.", "error");

        let row = $("#tableTask table").DataTable().row($(this).closest("tr")).data();
        resetTaskForm();

        $("#TaskId").val(row.Id);
        $("#TaskTitle").val(row.TaskTitle);
        $("#TaskDescription").val(row.TaskDescription);
        $("#TaskCategoryId").val(row.CategoryId);
        $("#TaskAssignedTo").val(row.AssignedTo);
        $("#TaskAssignedBy").val(row.AssignedBy);
        $("#TaskStatusId").val(row.StatusId);
        $("#PriorityLevel").val(row.PriorityLevel);

        if (row.StartDate) {
            const date = new Date(row.StartDate);
            const yyyy = date.getFullYear();
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const dd = String(date.getDate()).padStart(2, '0');
            $('#StartDate').val(`${yyyy}-${mm}-${dd}`);
        }
        if (row.DueDate) {
            const date = new Date(row.DueDate);
            const yyyy = date.getFullYear();
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const dd = String(date.getDate()).padStart(2, '0');
            $('#DueDate').val(`${yyyy}-${mm}-${dd}`);
        }
        if (row.CompletedDate) {
            const date = new Date(row.CompletedDate);
            const yyyy = date.getFullYear();
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const dd = String(date.getDate()).padStart(2, '0');
            $('#CompletedDate').val(`${yyyy}-${mm}-${dd}`);
        }
        $("#TaskModalTitle").text("Edit Task");
        $("#TaskModal").modal("show");
    });

    // ➤ DELETE OPEN MODAL
    $(document).on("click", ".btn-task-delete", function () {
        if (sessionStorage.getItem("CanDelete") !== "true")
            return Swal.fire("Access Denied", "You cannot Delete.", "error");

        let row = $("#tableTask table").DataTable().row($(this).closest("tr")).data();
        $("#deleteRecordModal").data("id", row.Id).modal("show");
    });

    // ➤ CONFIRM DELETE
    $("#delete-record").click(function () {
        let id = $("#deleteRecordModal").data("id");
        const payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId")
        };
        $.ajax({
            url: "/api/TaskMaster/DeleteTaskMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                Swal.fire("Success", "Task deleted!", "success");
                $("#deleteRecordModal").modal("hide");
                loadTaskData();
            },
            error: handleApiError
        });
    });

});

// RESET FORM
function resetTaskForm() {
    $("#TaskId").val(0);
    $("#TaskTitle").val("");
    $("#TaskDescription").val("");
    $("#TaskCategoryId").val("");
    $("#TaskAssignedTo").val("");
    $("#TaskStatusId").val("");
    $("#PriorityLevel").val("");

}

// LOAD TASK DATA
function loadTaskData() {
    let filter = {
        Id: 0,
        AssignedTo: 0,
        CategoryId: 0,
        StatusId: 0
    };

    $.ajax({
        url: "/api/TaskMaster/GetTaskMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(filter),
        success: function (res) {
            renderTaskTable(res.data);
        },
        error: handleApiError
    });
}

// RENDER TASK TABLE
function renderTaskTable(data) {

    if ($.fn.dataTable.isDataTable("#tableTask table")) {
        $("#tableTask table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#tableTask table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "TaskTitle", title: "Title" },
            { data: "TaskDescription", title: "Description" },
            { data: "CategoryName", title: "Category" },
            { data: "StatusName", title: "Status" },

            {
                data: "StartDate",
                title: "Start Date",
                render: function (d) {
                    if (!d) return '';
                    let date = new Date(d);
                    return date.toLocaleDateString(); // shows only date
                }
            },
            {
                data: "DueDate",
                title: "Due Date",
                render: function (d) {
                    if (!d) return '';
                    let date = new Date(d);
                    return date.toLocaleDateString(); // shows only date
                }
            },
            {
                data: "CompletedDate",
                title: "Completed Date",
                render: function (d) {
                    if (!d) return '';
                    let date = new Date(d);
                    return date.toLocaleDateString(); // shows only date
                }
            },
            { data: "PriorityLevel", title: "Priority" },
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

function loadCategoryDropdown() {
    $.ajax({
        url: "/api/TaskCategoryMaster/GetTaskCategoryMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            let ddl = $("#TaskCategoryId");
            ddl.empty().append(`<option value="">--Select Category--</option>`);
            res.data.forEach(x => {
                ddl.append(`<option value="${x.Id}">${x.CategoryName}</option>`);
            });
        }
    });
}

function loadStatusDropdown() {
    $.ajax({
        url: "/api/StatusMaster/GetStatusMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, StatusType: "TASK" }),
        success: function (res) {
            let ddl = $("#TaskStatusId");
            ddl.empty().append(`<option value="">--Select Status--</option>`);
            res.data.forEach(x => {
                ddl.append(`<option value="${x.Id}">${x.StatusName}</option>`);
            });
        }
    });
}

function loadStaffDropdown() {
    $.ajax({
        url: '/api/StaffMaster/GetStaffMaster',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0, StatusType: "Attendance" }),
        success: function (res) {

            const staff = res?.data || [];

            // Default + Staff list
            const options =
                `<option value="">-- Select Staff --</option>` +
                staff.map(x => `<option value="${x.Id}">${x.Name}</option>`).join('');

            // Same options in both dropdowns
            $('#TaskAssignedTo').html(options);
            $('#TaskAssignedBy').html(options);
        }
    });
}

function loadPriorityLevelDropdown() {
    $.ajax({
        url: "/api/TaskMaster/GetPriorityLevels",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, StatusType: "TASK" }),
        success: function (res) {
            let ddl = $("#PriorityLevel");
            ddl.empty().append(`<option value="">Select</option>`);
            res.data.forEach(x => {
                ddl.append(`<option value="${x.Id}">${x.PriorityName}</option>`);
            });
        }
    });
}

function setDefaultDates() {
    const today = new Date().toISOString().split("T")[0];  // yyyy-mm-dd format

    $("#StartDate").val(today);
    $("#DueDate").val(today);
    $("#CompletedDate").val(today);
}


