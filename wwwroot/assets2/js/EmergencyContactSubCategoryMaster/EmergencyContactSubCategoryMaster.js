
$(document).ready(function () {

    loadCategoryDropdown();
    loadSubCategoryData();

    // ---------------- ADD BUTTON ----------------
    $(document).on('click', '#btn-add-subcategory', function () {

        const canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';
    if (!canAdd) {
        Swal.fire("Access Denied", "You do not have permission to Add.", "error");
    return;
        }

    resetSubCategoryForm();
    $('#SubCategoryModalTitle').text("Add Emergency Contact Sub-Category");
    $('#SubCategoryModal').modal('show');
    });

    // ---------------- SAVE ----------------
    $("#btnSaveSubCategory").click(function () {

        let categoryId = $("#CategoryId").val();
    let name = $("#SubCategoryName").val();

    if (!categoryId) {
        Swal.fire("Validation", "Category is required!", "warning");
    return;
        }
    if (!name) {
        Swal.fire("Validation", "Sub-Category Name is required!", "warning");
    return;
        }

    const payload = {
        Id: Number($("#SubCategoryId").val() || 0),
    CategoryId: Number(categoryId),
    SubCategoryName: name,
    Description: $("#SubCategoryDescription").val(),
    ActionBy: sessionStorage.getItem("userId") ?? 0
        };

    $("#btnSaveSubCategory").prop("disabled", true);

    $.ajax({
        url: "/api/EmergencyContactSubCategoryMaster/SaveEmergencyContact_SubCategoryMaster",
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
    $('#SubCategoryModal').modal('hide');
    loadSubCategoryData();
                } else {
        Swal.fire("Error", res?.message || "Save failed", "error");
                }
            },
    error: function (xhr, status, error) {
        handleApiError(xhr, status, error, "Save SubCategory Error");
            },
    complete: function () {
        $("#btnSaveSubCategory").prop("disabled", false);
            }
        });
    });

    // ---------------- EDIT ----------------
    $(document).on("click", ".btn-edit", function () {

        const canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';
    if (!canEdit) {
        Swal.fire("Access Denied", "You do not have permission to Edit.", "error");
    return;
        }

    let table = $("#tableMain table").DataTable();
    let rowData = table.row($(this).closest("tr")).data();

    resetSubCategoryForm();
    $("#SubCategoryId").val(rowData.Id);
    $("#CategoryId").val(rowData.CategoryId);
    $("#SubCategoryName").val(rowData.SubCategoryName);
    $("#SubCategoryDescription").val(rowData.Description);

    $('#SubCategoryModalTitle').text("Edit Emergency Contact Sub-Category");
    $('#SubCategoryModal').modal('show');
    });

    // ---------------- DELETE ----------------
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

    // ---------------- CONFIRM DELETE ----------------
    $("#delete-record").click(function () {

        let id = $("#deleteRecordModal").data("id");

    const payload = {
        Id: id,
    ActionBy: sessionStorage.getItem("userId") ?? 0
        };

    $.ajax({
        url: "/api/EmergencyContactSubCategoryMaster/DeleteEmergencyContact_SubCategoryMaster",
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify(payload),
    success: function (res) {

                if (res?.status === "Success") {
        Swal.fire({
            position: "top-end",
            icon: "success",
            title: res.message || "Deleted",
            showConfirmButton: false,
            timer: 1000
        });
                } else {
        Swal.fire("Delete Failed", res?.message || "Unable to delete", "error");
                }

    $("#deleteRecordModal").modal("hide");
    loadSubCategoryData();
            },
    error: function (xhr, status, error) {
        handleApiError(xhr, status, error, "Delete SubCategory Error");
            }
        });
    });
});

    // ---------------- RESET FORM ----------------
    function resetSubCategoryForm() {
        $("#SubCategoryId").val(0);
    $("#CategoryId").val('');
    $("#SubCategoryName").val('');
    $("#SubCategoryDescription").val('');
}

    // ---------------- LOAD CATEGORY DROPDOWN ----------------
    function loadCategoryDropdown() {

        $.ajax({
            url: "/api/EmergencyContactCategoryMaster/GetEmergencyContact_CategoryMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: 0 }),
            success: function (res) {

                let ddl = $("#CategoryId");
                ddl.empty().append(`<option value="">-- Select Category --</option>`);

                (res?.data || []).forEach(item => {
                    ddl.append(`<option value="${item.Id}">${item.Name}</option>`);
                });
            }
        });
}

    // ---------------- FETCH SUBCATEGORY ----------------
    function loadSubCategoryData() {

        $.ajax({
            url: "/api/EmergencyContactSubCategoryMaster/GetEmergencyContact_SubCategoryMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: 0, CategoryId: 0 }),
            success: function (res) {
                renderSubCategoryTable(res?.data || []);
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Fetch SubCategory Error");
            }
        });
}

    // ---------------- RENDER TABLE ----------------
    function renderSubCategoryTable(data) {

    if ($.fn.dataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }

    data.forEach((item, i) => item.srno = i + 1);

    $("#tableMain table").DataTable({
        data: data,
    columns: [
    {data: "srno", title: "Sr. No." },
    {data: "CategoryName", title: "Category" },
    {data: "SubCategoryName", title: "Sub-Category" },
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
    searching: true,
    ordering: true
    });
}
