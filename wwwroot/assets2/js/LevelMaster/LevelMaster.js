$(document).ready(function () {
    loadLevelMasterData();
    // ----------------- ADD BUTTON CLICK -----------------
    $(document).on('click', '#btn-add', function () {

        const canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';
        if (!canAdd) {
            Swal.fire("Access Denied", "You do not have permission to Add.", "error");
            return;
        }

        resetLevelForm();
        $('#LevelModalTitle').text("Add Level");
        $('#LevelModal').modal('show');
    });

    // ----------------- SAVE LEVEL -----------------
    $("#btnSaveLevel").click(function () {
        let name = ($("#LevelName").val());
        console.log("levelname :", name);
        if (!name) {
            Swal.fire("Validation", "Level Name is required!", "warning");
            return;
        }

        const payload = {
            Id: Number($("#LevelId").val() || 0),
            LevelName: name,
            PriorityNo: Number($("#priorityNo").val() || 0),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $("#btnSaveLevel").prop("disabled", true);

        $.ajax({
            url: "/api/LevelMaster/SaveLevelMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response?.status === "Success") {
                    handleApiSuccess("Saved Successfully!");
                    $('#LevelModal').modal('hide');
                    loadLevelMasterData();
                } else {
                    Swal.fire("Error", response?.message || "Save Failed!", "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save Level Error");
            },
            complete: function () {
                $("#btnSaveLevel").prop("disabled", false);
            }
        });
    });
    // ----------------- EDIT BUTTON -----------------
    $(document).on("click", ".btn-edit", function () {
        const canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';
        if (!canEdit) {
            Swal.fire("Access Denied", "You do not have permission to Edit.", "error");
            return;
        }
        let table = $("#tableMain table").DataTable();
        let row = $(this).closest("tr");
        let rowData = table.row(row).data();
        resetLevelForm();
        $("#LevelId").val(rowData.Id);
        $("#LevelName").val(rowData.LevelName);
        $("#priorityNo").val(rowData.PriorityNo);

        $('#LevelModalTitle').text("Edit Level");
        $("#LevelModal").modal("show");
    });

    // ----------------- DELETE BUTTON -----------------
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
    $("#delete-record").click(function () {
        let id = $("#deleteRecordModal").data("id");
        const payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };
        $.ajax({
            url: "/api/LevelMaster/DeleteLevelMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res?.status === "Success") {
                    handleApiSuccess("Deleted Successfully!");
                } else {
                    Swal.fire("Delete Failed", res?.message || "Unable to delete", "error");
                }

                $("#deleteRecordModal").modal("hide");
                loadLevelMasterData();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete Level Error");
            }
        });
    });

});

// ----------------- RESET FORM -----------------
function resetLevelForm() {
    $("#LevelId").val(0);
    $("#LevelName").val('');
    $("#priorityNo").val('');
}

// ----------------- FETCH DATA -----------------
function loadLevelMasterData() {
    $.ajax({
        url: "/api/LevelMaster/GetLevelMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            renderLevelTable(res?.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Level Error");
        }
    });

}

// ----------------- RENDER TABLE -----------------
function renderLevelTable(data) {

    if ($.fn.dataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }
    data.forEach((item, i) => item.srno = i + 1);
    $("#tableMain table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr. No." },
            { data: "LevelName", title: "Level Name" },
            { data: "PriorityNo", title: "Priority No." },
            {
                data: null,
                title: "Actions",
                render: function () {
                    return `
                        <button class="btn btn-outline-warning btn-action-xss btn-sm btn-edit"><i class="fa fa-edit"></i></button>
                            <button class="btn btn-outline-danger btn-action-xss btn-sm btn-delete"><i class="fa fa-trash"></i></button>
                `;
                }
            }
        ],
        pageLength: 10,
        ordering: true,
        searching: true
    });
}