let staffDocuments = [];
let docIndex = 0;
let isProcessingFiles = false;

$(document).ready(function () {

    fetchStaffData();
    loadDesignationDropdown();

    // ================= ADD STAFF =================
    $(document).on("click", "#btn-add", function () {
        resetStaffForm();
        const today = new Date().toISOString().split("T")[0];
        //$("#txtJoiningDate,#txtBirthDate,#txtAnniversaryDate").val(today);
        $("#staffModalTitle").text("Add Staff");
        $("#staffModal").modal("show");
    });

    // ================= ADD DOCUMENT =================
    $(document).on("click", "#btnAddDocument", function () {

        const templateHtml = $("#docRowTemplate").html();

        if (!templateHtml) {
            console.error("❌ docRowTemplate not found in DOM");
            return;
        }

        const template = templateHtml.replace("{{index}}", docIndex);

        $("#documentsContainer").append(template);

        staffDocuments.push({
            index: docIndex,
            Id: 0,
            DocumentName: "",
            FileBase64: null,
            IsDeleted: false
        });

        docIndex++;
    });

    // ================= REMOVE DOCUMENT =================
    $(document).on("click", ".btnRemoveDoc", function () {

        const row = $(this).closest(".document-row");
        const index = row.data("index");

        const doc = staffDocuments.find(d => d.index === index);
        if (!doc) return;

        // 🔥 If document already saved in DB
        if (doc.Id && doc.Id > 0) {

            Swal.fire({
                title: "Are you sure?",
                text: "This document will be permanently deleted.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: "#d33",
                cancelButtonColor: "#6c757d",
                confirmButtonText: "Yes, delete it!"
            }).then((result) => {

                if (!result.isConfirmed) return;

                $.ajax({
                    url: "/api/StaffDocuments/DeleteStaffDocuments",
                    type: "POST",
                    contentType: "application/json",
                    data: JSON.stringify({
                        Id: doc.Id,
                        ActionBy: Number(sessionStorage.getItem("userId") ?? 0)
                    }),
                    success: function (res) {
                        if (res.status === "Success") {

                            row.remove();
                            staffDocuments = staffDocuments.filter(d => d.index !== index);
                            handleApiSuccess("Document deleted successfully.");
                        }
                        else {
                            Swal.fire({
                                icon: "error",
                                title: "Error",
                                text: res.message || "Failed to delete document."
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        handleApiError(xhr, status, error, "Document Delete Failed");
                    }
                });

            });

        }
        else {
            // 🔥 New unsaved document
            doc.IsDeleted = true;
            row.remove();
        }
    });
    

    // ================= DOCUMENT NAME =================
    $(document).on("input", ".doc-name", function () {
        const row = $(this).closest(".document-row");
        const index = row.data("index");

        const doc = staffDocuments.find(d => d.index === index);
        if (doc) doc.DocumentName = $(this).val();
    });

    // ================= DOCUMENT FILE =================
    $(document).on("change", ".doc-file", function () {
        const row = $(this).closest(".document-row");
        const index = row.data("index");
        const file = this.files[0];
        if (!file) return;

        isProcessingFiles = true;

        fileToBase64Document(file, function (base64) {
            const doc = staffDocuments.find(d => d.index === index);
            if (doc) doc.FileBase64 = base64;

            row.find(".uploaded-label").removeClass("d-none");
            isProcessingFiles = false;
        });
    });

    // ================= SAVE STAFF =================
    $(document).on("click", "#btnSaveStaff", function () {

        if (isProcessingFiles) {
            Swal.fire("Please wait", "Files are still processing", "info");
            return;
        }

        if (!$("#txtName").val().trim()) {
            Swal.fire("Validation", "Name is required", "warning");
            return;
        }

        if (!$("#txtPhoneNo").val().trim()) {
            Swal.fire("Validation", "Phone number is required", "warning");
            return;
        }

        const file = $("#txtPhoto").get(0).files[0] || null;

        const proceedSave = (photoBase64) => {

            $("#btnSaveStaff").prop("disabled", true);

            const payload = {
                Id: Number($("#staffId").val() || 0),
                Name: $("#txtName").val().trim(),
                Emp_Code: $("#txtEmpCode").val().trim(),
                Address: $("#txtAddress").val().trim(),
                PhoneNo: $("#txtPhoneNo").val().trim(),
                Emergency_PhoneNo: $("#txtEmergencyPhone").val().trim(),
                Email: $("#txtEmail").val().trim(),
                DesignationId: Number($("#ddlDesignation").val() || 0),
                AadharCardNo: $("#txtAadhar").val().trim(),
                JoiningDate: $("#txtJoiningDate").val(),
                BirthDate: $("#txtBirthDate").val(),
                AnniversaryDate: $("#txtAnniversaryDate").val(),
                Salary: Number($("#txtSalary").val() || 0),

                Photo: photoBase64,

                User_Name: $("#txtUserName").val().trim(),
                Password: $("#txtPassword").val().trim(),
                IsSuperUser: $("#switchIsSuperUser").is(":checked"),
                IsAutoLogin: $("#switchIsAutoLogin").is(":checked"),
                IsActive: $("#switchIsActive").is(":checked"),

                Bank_Name: $("#txtBankName").val().trim(),
                Bank_BranchName: $("#txtBankBranch").val().trim(),
                Bank_IFSC: $("#txtIFSC").val().trim(),
                Bank_AccountNo: $("#txtAccountNo").val().trim(),
                Bank_AccountHolderName: $("#txtBankAccountHolderName").val().trim(),
                Bank_UPI_Id: $("#txtBankUPIId").val().trim(),

                Documents: staffDocuments.map(d => ({
                    Id: d.Id,
                    DocumentName: d.DocumentName,
                    FileBase64: d.FileBase64,
                    IsDeleted: d.IsDeleted
                })),

                ActionBy: sessionStorage.getItem("userId") ?? 0
            };

            $.ajax({
                url: "/api/StaffMaster/SaveStaffMaster",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (res) {
                    if (res?.status === "Success") {
                        handleApiSuccess("Saved successfully.");
                        $("#staffModal").modal("hide");
                        fetchStaffData();
                    } else {
                        Swal.fire("Error", res?.message || "Save failed", "error");
                    }
                },
                error: function (xhr, error, status) {
                    handleApiError(xhr, status, error, "Failed to Save Staff");
                },
                complete: () => $("#btnSaveStaff").prop("disabled", false)
            });
        };

        if (file) {
            fileToBase64Photo(file, proceedSave);
        } else {
            proceedSave(null);
        }
    });

    // ================= MODAL RESET =================
    $('#staffModal').on('hidden.bs.modal', resetStaffForm);

    // ================= EDIT STAFF =================
    $(document).on("click", ".btn-edit", function () {

        const canEdit = (sessionStorage.getItem("CanEdit") || localStorage.getItem("CanEdit")) === "true";
        if (!canEdit) {
            Swal.fire("Access Denied", "You do not have permission to Edit.", "error");
            return;
        }

        const table = $("#tableStaff table").DataTable();
        const rowData = table.row($(this).closest("tr")).data();
        console.log("edit rowData:", rowData);
        resetStaffForm();

        // ===== BASIC DETAILS =====
        $("#staffId").val(rowData.Id);
        $("#txtName").val(rowData.Name);
        $("#txtEmpCode").val(rowData.Emp_Code);
        $("#txtAddress").val(rowData.Address);
        $("#txtPhoneNo").val(rowData.PhoneNo);
        $("#txtEmergencyPhone").val(rowData.Emergency_PhoneNo);
        $("#txtEmail").val(rowData.Email);
        $("#ddlDesignation").val(rowData.DesignationId).trigger("change");
        $("#txtAadhar").val(rowData.AadharCardNo);
        $("#txtSalary").val(rowData.Salary);

        $("#txtJoiningDate").val(rowData.JoiningDate);
        $("#txtBirthDate").val(rowData.BirthDate);
        $("#txtAnniversaryDate").val(rowData.AnniversaryDate);

        // ===== PHOTO =====
        $("#invoiceFileLink").val(rowData.Photo || "");
        if (rowData.Photo) {
            $("#btnDownloadInvoice").removeClass("d-none");
        } else {
            $("#btnDownloadInvoice").addClass("d-none");
        }

        // ===== LOGIN & BANK =====
        $("#txtUserName").val(rowData.User_Name);
        $("#txtPassword").val(rowData.Password);
        $("#txtBankName").val(rowData.Bank_Name);
        $("#txtBankBranch").val(rowData.Bank_BranchName);
        $("#txtIFSC").val(rowData.Bank_IFSC);
        $("#txtAccountNo").val(rowData.Bank_AccountNo);
        $("#txtBankAccountHolderName").val(rowData.Bank_AccountHolderName);
        $("#txtBankUPIId").val(rowData.Bank_UPI_Id);

        $("#switchIsSuperUser").prop("checked", rowData.IsSuperUser === true);
        $("#switchIsAutoLogin").prop("checked", rowData.IsAutoLogin === true);
        $("#switchIsActive").prop("checked", rowData.IsActive === true);

        // ===== DOCUMENTS (EDIT MODE) =====
        $("#documentsContainer").empty();
        staffDocuments = [];
        docIndex = 0;

        if (rowData.Documents && rowData.Documents.length > 0) {
            rowData.Documents.forEach(d => {
                const template = $("#docRowTemplate").html().replace("{{index}}", docIndex);
                $("#documentsContainer").append(template);

                const row = $(`.document-row[data-index='${docIndex}']`);
                row.find(".doc-name").val(d.DocumentName);
                row.find(".uploaded-label").removeClass("d-none");

                staffDocuments.push({
                    index: docIndex,
                    Id: d.Id,
                    DocumentName: d.DocumentName,
                    FileBase64: null,   // unchanged unless user uploads
                    IsDeleted: false,   // IMPORTANT
                    ExistingPath: d.FilePath
                });

                docIndex++;
            });
        }

        $("#staffModalTitle").text("Edit Staff");
        $("#staffModal").modal("show");
    });


    // ================= DELETE STAFF =================
    $(document).on("click", ".btn-delete", function () {

        const canDelete = (sessionStorage.getItem("CanDelete") || localStorage.getItem("CanDelete")) === "true";
        if (!canDelete) {
            Swal.fire("Access Denied", "You do not have permission to Delete.", "error");
            return;
        }

        const table = $("#tableStaff table").DataTable();
        const rowData = table.row($(this).closest("tr")).data();

        $("#deleteRecordModal")
            .data("id", rowData.Id)
            .modal("show");
    });

    $("#delete-record").click(function () {

        const id = $("#deleteRecordModal").data("id");

        $.ajax({
            url: "/api/StaffMaster/DeleteStaffMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                Id: id,
                ActionBy: sessionStorage.getItem("userId") ?? 0
            }),
            success: function (res) {
                if (res?.status === "Success") {
                    handleApiSuccess("Record deleted successfully.");
                    fetchStaffData();
                } else {
                    Swal.fire("Delete Failed", res?.message || "Error", "error");
                }
                $("#deleteRecordModal").modal("hide");
            },
            error: function (xhr, error, status) {
                handleApiError(xhr, status, error, "Failed to Delete Staff");
            }
        });
    });


    // ================= DOWNLOAD PHOTO =================
    $("#btnDownloadInvoice").on("click", function () {

        const fileUrl = $("#invoiceFileLink").val();
        if (!fileUrl) {
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


    // ================= MODAL RESET =================
    $('#staffModal').on('hidden.bs.modal', function () {
        resetStaffForm();
    });

});

// ================= HELPERS =================

function resetStaffForm() {
    $("#staffId").val(0);
    $("#staffModal input, #staffModal textarea").val("");
    $("#ddlDesignation").val("0");
    $("#switchIsSuperUser,#switchIsAutoLogin,#switchIsActive").prop("checked", false);

    $("#documentsContainer").empty();
    staffDocuments = [];
    docIndex = 0;
}

function fileToBase64Document(file, callback) {
    if (!file.type.startsWith("image/") && file.type !== "application/pdf") {
        Swal.fire("Invalid File", "Only image or PDF allowed", "warning");
        return;
    }
    if (file.size > 5 * 1024 * 1024) {
        Swal.fire("File Too Large", "Max 5MB allowed", "warning");
        return;
    }
    const reader = new FileReader();
    reader.onload = e => callback(e.target.result.split(',')[1]);
    reader.readAsDataURL(file);
}

function fileToBase64Photo(file, callback) {
    if (!file.type.startsWith("image/")) {
        Swal.fire("Invalid File", "Only image allowed", "warning");
        return;
    }
    if (file.size > 2 * 1024 * 1024) {
        Swal.fire("File Too Large", "Max 2MB allowed", "warning");
        return;
    }
    const reader = new FileReader();
    reader.onload = e => callback(e.target.result.split(',')[1]);
    reader.readAsDataURL(file);
}

function fetchStaffData() {
    $.ajax({
        url: '/api/StaffMaster/GetStaffMaster',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            const data = (res && res.data) ? res.data : [];
            renderStaffTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Staff Error");
        }
    });
}

function renderStaffTable(data) {

    if ($.fn.dataTable.isDataTable('#tableStaff table')) {
        $('#tableStaff table').DataTable().clear().destroy();
    }

    if (!data || data.length === 0) {
        $('#tableStaff table').DataTable({
            data: [],
            columns: [
                { data: 'srno', title: '#' },
                { data: 'Name', title: 'Name' },
                { data: 'PhoneNo', title: 'Phone' },
                { data: 'Email', title: 'Email' },
                { data: 'DesignationId', title: 'Designation' },
                {
                    data: null,
                    title: 'Actions',
                    render: () => getActionsHtml()
                }
            ],
            pageLength: 10
        });
        return;
    }

    // Add auto numbering
    data.forEach((x, i) => x.srno = i + 1);

    $('#tableStaff table').DataTable({
        data: data,
        columns: [
            { data: 'srno', title: '#' },
            { data: 'Name', title: 'Name' },
            { data: 'PhoneNo', title: 'Phone' },
            {
                data: "JoiningDate",
                title: "Joining Date",
                render: function (d) {
                    if (!d) return '';
                    let date = new Date(d);
                    return date.toLocaleDateString(); // shows only date
                }
            },
            { data: 'Salary', title: 'Salary' },
            { data: 'User_Name', title: 'Username' },
            {
                data: 'Photo',
                title: 'Photo',
                className: 'text-center align-middle',
                render: function (d) {
                    return d
                        ? `<div class="d-flex justify-content-center align-items-center">
                           <img src="${d}" class="rounded" width="40" height="40" />
                       </div>`
                                : `<div class="d-flex justify-content-center align-items-center text-muted">
                           No Image
                       </div>`;
                }
            },

            {
                data: 'IsSuperUser',
                title: 'Super User',
                className: 'text-center align-middle',
                render: d =>
                    `<span class="badge ${d ? 'bg-success' : 'bg-secondary'}">Yes</span>`
            },

            {
                data: 'IsAutoLogin',
                title: 'Auto Login',
                className: 'text-center align-middle',
                render: d =>
                    `<span class="badge ${d ? 'bg-success' : 'bg-secondary'}">Yes</span>`
            },

            {
                data: 'IsActive',
                title: 'Is Active',
                className: 'text-center align-middle',
                render: d =>
                    `<span class="badge ${d ? 'bg-success' : 'bg-secondary'}">Yes</span>`
            },
            {
                data: null,
                title: 'Actions',
                orderable: false,
                className: 'text-center',
                render: () => getActionsHtml()
            }
        ],
        pageLength: 10,
        ordering: true,
        searching: true,
        lengthChange: false
    });
}

function getActionsHtml() {
    return `
    <div class="dropdown">
        <button class="btn btn-sm btn-light dropdown-toggle"
                type="button"
                data-bs-toggle="dropdown"
                aria-expanded="false">
            <i class="fa-solid fa-ellipsis-vertical"></i>
        </button>

        <ul class="dropdown-menu dropdown-menu-end">
            <li>
                <a class="dropdown-item btn-edit" href="#">
                    <i class="fa-solid fa-pen me-2 text-warning"></i> Edit
                </a>
            </li>
            <li>
                <a class="dropdown-item btn-delete text-danger" href="#">
                    <i class="fa-solid fa-trash me-2"></i> Delete
                </a>
            </li>
        </ul>
    </div>
    `;
}

function loadDesignationDropdown() {
    $.ajax({
        url: "/api/DesignationMaster/GetDesignationMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            const dropdown = $("#ddlDesignation");
            dropdown.empty().append('<option value="0">-- Select --</option>');

            if (res?.status === "Success") {
                res.data.forEach(item => {
                    dropdown.append(`<option value="${item.Id}">${item.DesignationName}</option>`);
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}

