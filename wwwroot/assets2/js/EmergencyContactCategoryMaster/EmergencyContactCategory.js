
    $(document).ready(function () {
        loadEmergencyCategoryData();

    // ----------------- ADD BUTTON -----------------
    $(document).on('click', '#btn-add-category', function () {

        const canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';
    if (!canAdd) {
        Swal.fire("Access Denied", "You do not have permission to Add.", "error");
    return;
        }

    resetCategoryForm();
    $('#CategoryModalTitle').text("Add Emergency Contact Category");
    $('#CategoryModal').modal('show');
    });

    // ----------------- SAVE CATEGORY -----------------
    $("#btnSaveCategory").click(function () {

        let name = $("#CategoryName").val();
    if (!name) {
        Swal.fire("Validation", "Category Name is required!", "warning");
    return;
        }

    const payload = {
        Id: Number($("#CategoryId").val() || 0),
    Name: name,
    Description: $("#CategoryDescription").val(),
    ActionBy: sessionStorage.getItem("userId") ?? 0
        };

    $("#btnSaveCategory").prop("disabled", true);

    $.ajax({
        url: "/api/EmergencyContactCategoryMaster/SaveEmergencyContact_CategoryMaster",
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify(payload),
    success: function (res) {
                if (res?.status === "Success") {
        Swal.fire({
            position: "top-end",
            icon: "success",
            title: "Saved",
            showConfirmButton: false,
            timer: 1000
        });
    $('#CategoryModal').modal('hide');
    loadEmergencyCategoryData();
                } else {
        Swal.fire("Error", res?.message || "Save failed", "error");
                }
            },
    error: function (xhr, status, error) {
        handleApiError(xhr, status, error, "Save Category Error");
            },
    complete: function () {
        $("#btnSaveCategory").prop("disabled", false);
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
    let rowData = table.row($(this).closest("tr")).data();

    resetCategoryForm();
    $("#CategoryId").val(rowData.Id);
    $("#CategoryName").val(rowData.Name);
    $("#CategoryDescription").val(rowData.Description);

    $('#CategoryModalTitle').text("Edit Emergency Contact Category");
    $('#CategoryModal').modal('show');
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
        url: "/api/EmergencyContactCategoryMaster/DeleteEmergencyContact_CategoryMaster",
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify(payload),
    success: function (res) {
                if (res?.status === "Success") {
        Swal.fire({
            position: "top-end",
            icon: "success",
            title: "Deleted",
            showConfirmButton: false,
            timer: 1000
        });
                } else {
        Swal.fire("Delete Failed", res?.message || "Unable to delete", "error");
                }

    $("#deleteRecordModal").modal("hide");
    loadEmergencyCategoryData();
            },
    error: function (xhr, status, error) {
        handleApiError(xhr, status, error, "Delete Category Error");
            }
        });
    });
});

    // ----------------- RESET FORM -----------------
    function resetCategoryForm() {
        $("#CategoryId").val(0);
    $("#CategoryName").val('');
    $("#CategoryDescription").val('');
}

    // ----------------- FETCH DATA -----------------
    function loadEmergencyCategoryData() {

        $.ajax({
            url: "/api/EmergencyContactCategoryMaster/GetEmergencyContact_CategoryMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: 0 }),
            success: function (res) {
                renderCategoryTable(res?.data || []);
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Fetch Category Error");
            }
        });
}

    // ----------------- RENDER TABLE -----------------
    function renderCategoryTable(data) {

    if ($.fn.dataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }

    data.forEach((item, i) => item.srno = i + 1);

    $("#tableMain table").DataTable({
        data: data,
    columns: [
    {data: "srno", title: "Sr. No." },
    {data: "Name", title: "Category Name" },
    {data: "Description", title: "Description" },
    {
        data: null,
    title: "Actions",
    render: function () {
                    return `
    <button class="btn btn-outline-warning btn-action-xss btn-sm btn-edit">
        <i class="fa fa-edit"></i>
    </button>
    <button class="btn btn-outline-danger btn-action-xss btn-sm btn-delete">
        <i class="fa fa-trash"></i>
    </button>`;
                }
            }
    ],
    pageLength: 10,
    ordering: true,
    searching: true
    });
}

