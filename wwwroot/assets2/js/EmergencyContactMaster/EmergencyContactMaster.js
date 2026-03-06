const reportColumns = [
    { field: "CategoryName", label: "Category" },
    { field: "SubCategoryName", label: "Sub Category" },
    { field: "ContactType", label: "Contact Type" },
    { field: "ContactName", label: "Contact Name" },
    { field: "MobileNo", label: "Mobile No" },
    { field: "MobileNo2", label: "Alternate Mobile" },
    { field: "Location", label: "Location" },
    { field: "Remarks", label: "Remarks" },
    { field: "IsActive", label: "Active Status" }
];
const COLUMN_STORAGE_KEY = "EmergencyContactReportColumns";
const HEADER_KEY_EMERGENCY = "EmergencyReportWithHeader";

$(document).ready(function () {
    loadCategoryDropdown();
    loadSubCategoryDropdown();
    fetchData();
    loadColumnCheckboxes();

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
    $("#CategoryId, #ddlFilter_Category").change(function () {
        loadSubCategoryDropdown($(this).val());
    });

    $("#ddlFilter_Category, #ddlFilter_SubCategory, #ddlFilter_Status").change(function () {
        fetchData();
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
                    fetchData();
                }
                else {
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
                fetchData();
            }
        });
    });
    
    //REPORT DOWNLOAD----------------------------------
    $(document).on("change", "#chk-select-all", function () {
        $(".column-checkbox").prop("checked", $(this).prop("checked"));
    });
    $(document).on("change", ".column-checkbox", function () {
        updateSelectAllState();
    });

    $("#btn-select-columns").click(function () {
        loadColumnCheckboxes();
        $("#modalColumns").modal("show");
    });

    $("#btn-download-pdf").click(function () {
        ViewEmergencyContactReport("downloadPDF");
    });

    $("#btn-download-excel").click(function () {
        ViewEmergencyContactReport("downloadExcel");
    });

    $("#btn-reset-columns").click(function () {
        $(".column-checkbox").prop("checked", true);
        $("#chk-select-all").prop("checked", true);
    });

    $("#btn-save-columns").click(function () {
        let columns = [];
        $(".column-checkbox:checked").each(function () {
            columns.push($(this).val());
        });
        localStorage.setItem(COLUMN_STORAGE_KEY, columns.join(","));
        $("#modalColumns").modal("hide");
    });

    //With Header
    // Restore saved value
    const savedHeader = sessionStorage.getItem(HEADER_KEY_EMERGENCY);
    if (savedHeader !== null) {
        document.getElementById("toggleEmergencyHeader").checked = (savedHeader === "true");
    }

    // Toggle change event
    document.getElementById("toggleEmergencyHeader").addEventListener("change", function () {
        const isChecked = this.checked;
        sessionStorage.setItem(HEADER_KEY_EMERGENCY, isChecked);
        console.log("Emergency Report With Header:", isChecked);
    });
});

// ---------------- RESET FORM ----------------
function resetContactForm() {
    $("#ContactId").val(0);
    $("#CategoryId").val('0');
    $("#SubCategoryId").val('0');
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
            let ddl = $("#CategoryId, #ddlFilter_Category");
            ddl.empty().append(`<option value="0">-Select Category-</option>`);
            (res?.data || []).forEach(x => {
                ddl.append(`<option value="${x.Id}">${x.Name}</option>`);
            });
            if (callback) callback();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching Category data");
        }
    });
}

// ---------------- LOAD SUBCATEGORY ----------------
function loadSubCategoryDropdown(categoryId = 0, callback) {

    //if (!categoryId) {
    //    $("#SubCategoryId").empty().append(`<option value="">-- Select Sub-Category --</option>`);
    //    return;
    //}

    $.ajax({
        url: "/api/EmergencyContactSubCategoryMaster/GetEmergencyContact_SubCategoryMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ CategoryId: categoryId }),
        success: function (res) {
            let ddl = $("#SubCategoryId,#ddlFilter_SubCategory");
            ddl.empty().append(`<option value="0">-Select SubCategory-</option>`);
            (res?.data || []).forEach(x => {
                ddl.append(`<option value="${x.Id}">${x.SubCategoryName}</option>`);
            });
            if (callback) callback();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching SubCategory data");
        }
    });
}

// ---------------- FETCH CONTACT ----------------
function fetchData() {
    const categoryId = parseInt($("#ddlFilter_Category").val());
    const subCategoryId = parseInt($("#ddlFilter_SubCategory").val());
    const isActive = $("#ddlFilter_Status").val() === "" ? null : $("#ddlFilter_Status").val() === "1"
    var payload = {
        Id: 0,
        CategoryId: categoryId,
        SubCategoryId: subCategoryId,
        IsActive: isActive
    };
    $.ajax({
        url: "/api/EmergencyContactMaster/GetEmergencyContact_Master",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            renderContactTable(res?.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching Emergency Contact data");
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
            { data: "srno", title: "Sr. No" },
            { data: "CategoryName", title: "Category" },
            { data: "SubCategoryName", title: "Sub-Category" },
            { data: "ContactType", title: "Type" },
            { data: "ContactName", title: "Name" },
            { data: "MobileNo", title: "Mobile" },
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

// REPORT COLUMNS ------------------------------
function loadColumnCheckboxes() {

    let savedColumns = localStorage.getItem(COLUMN_STORAGE_KEY);
    let selectedColumns = [];

    if (savedColumns)
        selectedColumns = savedColumns.split(",");

    let html = '';

    reportColumns.forEach(col => {

        let isChecked = true;

        if (selectedColumns.length > 0)
            isChecked = selectedColumns.includes(col.field);

        html += `
        <div class="col-md-6 mb-2">

            <div class="form-check">

                <input class="form-check-input column-checkbox"
                       type="checkbox"
                       value="${col.field}"
                       ${isChecked ? "checked" : ""}>

                <label class="form-check-label">
                    ${col.label}
                </label>

            </div>

        </div>
        `;
    });

    $("#column-list").html(html);

    updateSelectAllState();
}

function getSelectedColumns() {
    let saved = localStorage.getItem(COLUMN_STORAGE_KEY);
    if (saved) {
        return JSON.stringify(saved.split(","));
    }
    // default → all columns
    return JSON.stringify(reportColumns.map(x => x.field));
}
function updateSelectAllState() {
    let total = $(".column-checkbox").length;
    let checked = $(".column-checkbox:checked").length;
    $("#chk-select-all").prop("checked", total === checked);
}

function ViewEmergencyContactReport(actionName = "downloadPDF") {

    const withHeader = sessionStorage.getItem("EmergencyReportWithHeader") === "true";
    const userId = parseInt(sessionStorage.getItem('userId') || 0);

    const categoryId = parseInt($("#ddlFilter_Category").val());
    const subCategoryId = parseInt($("#ddlFilter_SubCategory").val());
    const isActive = $("#ddlFilter_Status").val() === "" ? null : $("#ddlFilter_Status").val() === "1"

    const payload = {
        Id: 0,
        CategoryId: categoryId,
        SubCategoryId: subCategoryId,
        IsActive: isActive,
        Columns: getSelectedColumns(),   // JSON columns
        WithHeader: withHeader,
        ActionBy: userId
    };

    console.log("Emergency Report payload:", payload);

    const url = (actionName === "downloadExcel")
        ? "/api/Report/Report_EmergencyContact_Excel"
        : "/api/Report/Report_EmergencyContact";

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        data: JSON.stringify(payload),

        success: function (result) {

            console.log("result:", result);

            if (result.status !== "Success" || !result.data || !result.data.FileUrl) {
                Swal.fire({
                    title: "Fail",
                    text: result.message || "No report URL returned",
                    icon: "error"
                });
                return;
            }

            var fileUrl = result.data.FileUrl;

            var link = document.createElement("a");
            link.href = fileUrl;

            if (actionName === "downloadPDF") {
                link.download = `EmergencyContact_Report_${new Date().toISOString().replace(/[:.-]/g, "")}.pdf`;
            }
            else if (actionName === "downloadExcel") {
                link.download = `EmergencyContact_Report_${new Date().toISOString().replace(/[:.-]/g, "")}.xlsx`;
            }

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },

        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error generating Emergency Contact report");
        }
    });
}
