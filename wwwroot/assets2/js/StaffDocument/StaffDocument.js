function getBase64File(file, callback) {
    const reader = new FileReader();
    reader.onload = function (e) {
        const base64 = e.target.result.split(',')[1];
        callback(base64);
    };
    reader.readAsDataURL(file);
}

function loadStaffDropdown(selectedId = null) {
    $.ajax({
        url: "/api/StaffMaster/GetStaffMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            let ddl = $("#ddlStaff");
            ddl.empty().append(`<option value="">-- Select Staff --</option>`);

            response.data.forEach(staff => {
                ddl.append(`<option value="${staff.Id}">${staff.Name}</option>`);
            });

            if (selectedId) ddl.val(selectedId);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}

$(document).ready(function () {
    loadStaffDropdown();
    resetDocuments();
    fetchStaffDocuments();

    $(document).on("click", "#btn-add", function () {
        resetDocuments();
        $("#staffDocModalTitle").text("Add Staff Document");
        $("#staffDocModal").modal("show");
    });

    $("#btnSaveDoc").on("click", function () {
        const staffId = $("#ddlStaff").val();
        if (!staffId) {
            Swal.fire("Required", "Please select staff", "warning");
            return;
        }
        let fileInput = $("#txtDocFile").get(0);
        let file = fileInput.files.length > 0 ? fileInput.files[0] : null;
        function saveDocument(fileBase64) {
            const payload = {
                Id: Number($("#docId").val() || 0),
                StaffId: Number(staffId),
                DocumentType: $("#txtDocType").val() || null,
                DocumentName: $("#txtDocName").val() || null,
                FilePath: fileBase64,
                ActionBy: sessionStorage.getItem("userId") || 0
            };
            console.log("payload:", payload);
            $.ajax({
                url: "/api/StaffDocuments/SaveStaffDocuments",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (res) {
                    Swal.fire("Success", res.message, "success");
                    $("#staffDocModal").modal("hide");
                    fetchStaffDocuments();
                },
                error: function (xhr, status, error) {
                    handleApiError(xhr, status, error, "Save StaffDocuments  Error")
                }
            });
        }
        if (file)
            getBase64File(file, saveDocument);
        else
            saveDocument(null);
    });

    $(document).on("click", ".btn-edit", function () {

        // Permission Check (same logic as staff edit)
        const canEdit = (sessionStorage.getItem("CanEdit") || localStorage.getItem("CanEdit")) === "true";
        if (!canEdit) {
            Swal.fire("Access Denied", "You do not have permission to Edit.", "error");
            return;
        }
        // Get row data from table
        const table = $("#tableStaffDocuments table").DataTable();
        const rowData = table.row($(this).closest("tr")).data();
        // console.log("Edit Document Row:", rowData);
        resetDocuments(); // Clear previous modal values

        // ------- Fill Modal fields --------
        $("#docId").val(rowData.Id);// Hidden ID
        $("#ddlStaff").val(rowData.StaffId).trigger("change");
        $("#txtDocType").val(rowData.DocumentType);
        $("#txtDocName").val(rowData.DocumentName);

        // If file exists, show "Download"
        if (rowData.FilePath && rowData.FilePath !== "") {
            $("#btnDownloadDocFile")
                .removeClass("d-none")
                .data("file", rowData.FilePath);  // <-- THIS
        } else {
            $("#btnDownloadDocFile").addClass("d-none").data("file", "");
        }
        // Update Modal Title & Button Text
        $("#staffDocModalTitle").text("Edit Staff Document");
        $("#btnSaveDoc").text("Update");
        // Show Modal
        $("#staffDocModal").modal("show");
    });

    // Delete button click in table
    $(document).on("click", ".btn-delete", function () {
        // Get DataTable instance
        var table = $("#tableStaffDocuments table").DataTable(); // make sure this matches your table id
        var rowData = table.row($(this).closest("tr")).data();
        if (!rowData || !rowData.Id) {
            Swal.fire("Error", "Unable to get selected row data.", "error");
            return;
        }
        // Store Id in modal using jQuery data
        $("#deleteRecordModal").data("id", rowData.Id).modal("show");

    });
    // Confirm delete
    $("#delete-record").click(function () {
        var id = $("#deleteRecordModal").data("id");
        if (!id) {
            Swal.fire("Error", "Record Id not found.", "error");
            return;
        }
        let payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/StaffDocuments/DeleteStaffDocuments",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (response.status === "Success") {
                    Swal.fire("Deleted!", response.message, "success");
                    $("#deleteRecordModal").modal("hide");
                    // Reload table
                    fetchStaffDocuments();
                } else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Something went wrong!");
            }
        });
    });
});

$("#btnDownloadDocFile").on("click", function () {
    let fileUrl = $(this).data("file") || $("#fileBase64").val();

    if (!fileUrl || fileUrl.trim() === "") {
        Swal.fire("Error", "File not found!", "error");
        return;
    }

    const link = document.createElement("a");
    link.href = fileUrl;
    link.download = fileUrl.split("/").pop();
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
});

function resetDocuments() {
    $("#docId").val(0);
    $("#ddlStaff").val("").trigger("change");
    $("#txtDocType").val("");
    $("#txtDocName").val("");
    $("#txtDocFile").val("");
    $("#fileBase64").val("");

    $("#btnDownloadDocFile").addClass("d-none");  // hide download
    $("#btnSaveDoc").text("Save");
    $("#staffDocModalTitle").text("Add Staff Document");
}

// Fetch Staff Documents
function fetchStaffDocuments(staffId = 0) {

    $('#btn-loader').show();
    const payload = {
        Id: 0

    };

    $.ajax({
        url: "/api/StaffDocuments/GetStaffDocuments",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            const data = response.data || [];
            renderStaffDocumentsTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching staff documents");
        },
        complete: function () {
            $('#btn-loader').hide();
        }
    });
}

function renderStaffDocumentsTable(data) {

    if ($.fn.dataTable.isDataTable("#tableStaffDocuments table")) {
        $("#tableStaffDocuments table").DataTable().destroy();
    }

    // Add Serial Number
    data.forEach((item, index) => item.srno = index + 1);

    $("#tableStaffDocuments table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "StaffName", title: "Staff" },
            { data: "DocumentType", title: "Type" },
            { data: "DocumentName", title: "Name" },
            {
                data: null,
                title: "Actions",
                render: function (row) {
                    let actionBtns = `
                        <button class="btn btn-outline-warning btn-action-xss btn-edit">
                        <i class="fa fa-edit"></i>
                        </button>

                        <button class="btn btn-outline-danger btn-action-xss btn-delete">
                        <i class="fa fa-trash"></i>
                        </button>
                        `;

                    // File Exists → Show Download Button
                    if (row.FilePath && row.FilePath.trim() !== "") {
                        actionBtns += `
                            <a href="${row.FilePath}" target="_blank" class="btn btn-outline-info btn-action-xss">
                            <i class="fa fa-download"></i>
                            </a>
                            `;
                    }
                    else {
                        actionBtns += `
                            <span class="text-muted ms-2">No File</span>
                            `;
                    }

                    return actionBtns;
                }
            }

        ],
        pageLength: 10,
        ordering: true,
        searching: true,
        lengthChange: false,
        pagingType: "full_numbers"
    });
}
