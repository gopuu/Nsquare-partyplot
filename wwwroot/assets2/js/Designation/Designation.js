$(document).ready(function () {
    loadDesignationData();
    loadLevelDropdown();

    // -------- ADD BUTTON --------
    $("#btn-add").click(function () {
        const canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';
        if (!canAdd) return Swal.fire("Access Denied", "You do not have permission to Add.", "error");

        resetDesignationForm();
        $('#DesignationModalTitle').text("Add Designation");
        $('#DesignationModal').modal('show');
    });

    // -------- SAVE DESIGNATION --------
    $("#btnSaveDesignation").click(function () {
        let name = ($("#DesignationName").val() || "").trim();
        let levelId = $("#LevelId").val();

        if (!name) return Swal.fire("Validation", "Designation Name is required!", "warning");
        if (!levelId) return Swal.fire("Validation", "Please select a Level!", "warning");

        const payload = {
            Id: Number($("#DesignationId").val() || 0),
            DesignationName: name,
            LevelId: Number(levelId),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $("#btnSaveDesignation").prop("disabled", true);

        $.ajax({
            url: "/api/DesignationMaster/SaveDesignationMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response?.status === "Success") {

                    Swal.fire({
                        icon: "success",
                        title: "Saved!",
                        timer: 1200,
                        showConfirmButton: false,
                        position: "top-end"
                    });

                    $('#DesignationModal').modal('hide');
                    loadDesignationData();
                }
                else {
                    Swal.fire("Error", response?.message || "Save Failed!", "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save Designation Error");
            },
            complete: function () {
                $("#btnSaveDesignation").prop("disabled", false);
            }
        });
    });

    // -------- EDIT BUTTON --------
    $(document).on("click", ".btn-edit-designation", function () {
        let table = $("#DesignationTable table").DataTable();
        let rowData = table.row($(this).closest("tr")).data();

        const canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';
        if (!canEdit) return Swal.fire("Access Denied", "You do not have permission to Edit.", "error");

        resetDesignationForm();

        $("#DesignationId").val(rowData.Id);
        $("#DesignationName").val(rowData.DesignationName);
        $("#LevelId").val(rowData.LevelId);

        $('#DesignationModalTitle').text("Edit Designation");
        $('#DesignationModal').modal('show');
    });

    // ----------------- DELETE BUTTON -----------------
    $(document).on("click", ".btn-delete", function () {
        const canDelete = (sessionStorage.getItem('CanDelete') || localStorage.getItem('CanDelete')) === 'true';
        if (!canDelete) {
            Swal.fire("Access Denied", "You do not have permission to Delete.", "error");
            return;
        }
        let table = $("#DesignationTable table").DataTable();
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
            url: "/api/DesignationMaster/DeleteDesignationMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res?.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Deleted Successfully!",
                        showConfirmButton: false,
                        timer: 1000
                    });
                } else {
                    Swal.fire("Delete Failed", res?.message || "Unable to delete", "error");
                }

                $("#deleteRecordModal").modal("hide");
                loadDesignationData();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete Level Error");
            }
        });
    });
});

// -------- RESET FORM --------
function resetDesignationForm() {
    $("#DesignationId").val(0);
    $("#DesignationName").val('');
    $("#LevelId").val('');
}

// -------- FETCH TABLE --------
function loadDesignationData() {
    $.ajax({
        url: "/api/DesignationMaster/GetDesignationMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            renderDesignationTable(res?.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Designation Error");
        }
    });
}

// -------- RENDER DATATABLE --------
function renderDesignationTable(data) {
    if ($.fn.dataTable.isDataTable("#DesignationTable table"))
        $("#DesignationTable table").DataTable().destroy();

    data.forEach((item, i) => item.srno = i + 1);

    $("#DesignationTable table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr.No" },
            { data: "DesignationName", title: "Designation Name" },
            { data: "LevelName", title: "Level" },
            {
                data: null, title: "Actions",
                render: () => `
                        <button class="btn btn-outline-warning btn-sm btn-action-xss btn-edit-designation"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-outline-danger btn-sm btn-action-xss btn-delete"><i class="fa fa-trash"></i></button>
                          `
            }
        ],
        pageLength: 10
    });
}

// -------- LOAD LEVEL DROPDOWN --------
function loadLevelDropdown() {
    $.ajax({
        url: "/api/LevelMaster/GetLevelMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {

            let dropdown = $("#LevelId");
            dropdown.empty().append(`<option value="">-- Select Level --</option>`);

            res?.data?.forEach(item => {
                dropdown.append(`<option value="${item.Id}">${item.LevelName}</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Dropdown Load Error");
        }
    });
}
