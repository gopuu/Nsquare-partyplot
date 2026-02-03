
$(document).ready(function () {

    loadCategoryDropdown();
    loadEmergencyContactData();

    // ---------------- ADD BUTTON ----------------
    $(document).on('click', '#btn-add-contact', function () {

        const canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';
    if (!canAdd) {
        Swal.fire("Access Denied", "You do not have permission to Add.", "error");
    return;
        }

    resetContactForm();
    $('#ContactModalTitle').text("Add Emergency Contact");
    $('#ContactModal').modal('show');
    });

    // ---------------- CATEGORY CHANGE → LOAD SUBCATEGORY ----------------
    $("#CategoryId").change(function () {
        loadSubCategoryDropdown($(this).val());
    });

    // ---------------- SAVE ----------------
    $("#btnSaveContact").click(function () {

        if (!$("#SubCategoryId").val()) {
        Swal.fire("Validation", "Sub-Category is required!", "warning");
    return;
        }
    if (!$("#ContactName").val()) {
        Swal.fire("Validation", "Contact Name is required!", "warning");
    return;
        }
    if (!$("#MobileNo").val()) {
        Swal.fire("Validation", "Mobile No is required!", "warning");
    return;
        }

    const payload = {
        Id: Number($("#ContactId").val() || 0),
    SubCategoryId: Number($("#SubCategoryId").val()),
    ContactType: $("#ContactType").val(),
    ContactName: $("#ContactName").val(),
    MobileNo: $("#MobileNo").val(),
    MobileNo2: $("#MobileNo2").val(),
    Location: $("#Location").val(),
    Remarks: $("#Remarks").val(),
    IsActive: $("#IsActive").is(":checked"),
    ActionBy: sessionStorage.getItem("userId") ?? 0
        };

    $("#btnSaveContact").prop("disabled", true);

    $.ajax({
        url: "/api/EmergencyContactMaster/SaveEmergencyContact_Master",
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
    $('#ContactModal').modal('hide');
    loadEmergencyContactData();
                } else {
        Swal.fire("Error", res?.message || "Save failed", "error");
                }
            },
    error: function (xhr, status, error) {
        handleApiError(xhr, status, error, "Save Contact Error");
            },
    complete: function () {
        $("#btnSaveContact").prop("disabled", false);
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

    resetContactForm();

    $("#ContactId").val(rowData.Id);
    $("#ContactType").val(rowData.ContactType);
    $("#ContactName").val(rowData.ContactName);
    $("#MobileNo").val(rowData.MobileNo);
    $("#MobileNo2").val(rowData.MobileNo2);
    $("#Location").val(rowData.Location);
    $("#Remarks").val(rowData.Remarks);
    $("#IsActive").prop("checked", rowData.IsActive);

    loadCategoryDropdown(function () {
        $("#CategoryId").val(rowData.CategoryId);
    loadSubCategoryDropdown(rowData.CategoryId, function () {
        $("#SubCategoryId").val(rowData.SubCategoryId);
            });
        });

    $('#ContactModalTitle').text("Edit Emergency Contact");
    $('#ContactModal').modal('show');
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

    $.ajax({
        url: "/api/EmergencyContactMaster/DeleteEmergencyContact_Master",
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify({
        Id: id,
    ActionBy: sessionStorage.getItem("userId") ?? 0
            }),
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
    loadEmergencyContactData();
            }
        });
    });
});

    // ---------------- RESET FORM ----------------
    function resetContactForm() {
        $("#ContactId").val(0);
    $("#CategoryId").val('');
    $("#SubCategoryId").val('');
    $("#ContactType").val('');
    $("#ContactName").val('');
    $("#MobileNo").val('');
    $("#MobileNo2").val('');
    $("#Location").val('');
    $("#Remarks").val('');
    $("#IsActive").prop("checked", true);
}

    // ---------------- LOAD CATEGORY ----------------
    function loadCategoryDropdown(callback) {
        $.ajax({
            url: "/api/EmergencyContactCategoryMaster/GetEmergencyContact_CategoryMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: 0 }),
            success: function (res) {
                let ddl = $("#CategoryId");
                ddl.empty().append(`<option value="">-- Select Category --</option>`);
                (res?.data || []).forEach(x => {
                    ddl.append(`<option value="${x.Id}">${x.Name}</option>`);
                });
                if (callback) callback();
            }
        });
}

    // ---------------- LOAD SUBCATEGORY ----------------
    function loadSubCategoryDropdown(categoryId, callback) {

    if (!categoryId) {
        $("#SubCategoryId").empty().append(`<option value="">-- Select Sub-Category --</option>`);
    return;
    }

    $.ajax({
        url: "/api/EmergencyContactSubCategoryMaster/GetEmergencyContact_SubCategoryMaster",
    type: "POST",
    contentType: "application/json",
    data: JSON.stringify({CategoryId: categoryId }),
    success: function (res) {
        let ddl = $("#SubCategoryId");
    ddl.empty().append(`<option value="">-- Select Sub-Category --</option>`);
            (res?.data || []).forEach(x => {
        ddl.append(`<option value="${x.Id}">${x.SubCategoryName}</option>`);
            });
    if (callback) callback();
        }
    });
}

    // ---------------- FETCH CONTACT ----------------
    function loadEmergencyContactData() {

        $.ajax({
            url: "/api/EmergencyContactMaster/GetEmergencyContact_Master",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: 0 }),
            success: function (res) {
                renderContactTable(res?.data || []);
            }
        });
}

    // ---------------- RENDER TABLE ----------------
    function renderContactTable(data) {

    if ($.fn.dataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#tableMain table").DataTable({
        data: data,
    columns: [
    {data: "srno", title: "Sr. No" },
    {data: "SubCategoryName", title: "Sub-Category" },
    {data: "ContactType", title: "Type" },
    {data: "ContactName", title: "Name" },
    {data: "MobileNo", title: "Mobile" },
    {
        data: "IsActive",
    title: "Status",
                render: d => d
    ? `<span class="badge bg-success">Active</span>`
    : `<span class="badge bg-danger">Inactive</span>`
            },
    {
        data: null,
    title: "Actions",
                render: () => `
    <button class="btn btn-outline-warning btn-sm btn-action-xss btn-edit">
        <i class="fa fa-edit"></i>
    </button>
    <button class="btn btn-outline-danger btn-sm btn-action-xss btn-delete">
        <i class="fa fa-trash"></i>
    </button>`
            }
    ]
    });
}


