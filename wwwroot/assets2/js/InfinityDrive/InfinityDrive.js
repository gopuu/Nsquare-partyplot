let infinityDriveFiles = [];
let fileModal = null;

const MAX_FILE_SIZE = 1 * 1024 * 1024; // 1 MB
const ALLOWED_TYPES = ["application/pdf", "image/jpeg", "image/png"];
const ALLOWED_EXTENSIONS = ["pdf", "jpg", "jpeg", "png"];


$(document).ready(function () {

    fileModal = new bootstrap.Modal(document.getElementById("fileModal"));

    loadFolderDropdowns();
    loadFiles();

    $("#ddlFilterCategory").on("change", function () {
        loadFiles();
    });

    $("#txtSearchDocument").on("input", function () {
        renderFiles();
    });

});


/* =========================================================
   LOAD FOLDER DROPDOWNS
========================================================= */
function loadFolderDropdowns() {

    $.ajax({
        url: "/api/InfinityDrive_Master/GetInfinityDrive_Master",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: 0
        }),

        success: function (res) {

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            const folders = res.data || [];

            const filterDdl = $("#ddlFilterCategory");
            const modalDdl = $("#ddlFileCategory");

            filterDdl.empty();
            modalDdl.empty();

            filterDdl.append(`
                <option value="0">All Folders</option>
            `);

            modalDdl.append(`
                <option value="">-- Select Folder --</option>
            `);

            folders.forEach(function (folder) {

                filterDdl.append(`
                    <option value="${folder.Id}">
                        ${escapeHtml(folder.FolderName)}
                    </option>
                `);

                modalDdl.append(`
                    <option value="${folder.Id}">
                        ${escapeHtml(folder.FolderName)}
                    </option>
                `);

            });
        },

        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load folders");
        }
    });
}


/* =========================================================
   GET FILES
========================================================= */
function loadFiles() {

    const categoryId = parseInt($("#ddlFilterCategory").val()) || 0;

    $("#documentTableBody").html(`
        <tr>
            <td colspan="6" class="text-center text-muted py-4">
                <div class="spinner-border spinner-border-sm me-2"></div>
                Loading documents...
            </td>
        </tr>
    `);

    const payload = {
        Id: 0,
        CategoryId: categoryId
    };

    $.ajax({
        url: "/api/InfinityDrive/GetInfinityDrive_Files",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),

        success: function (res) {

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            infinityDriveFiles = res.data || [];

            renderFiles();
        },

        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load documents");
        }
    });
}


/* =========================================================
   RENDER FILES
========================================================= */
function renderFiles() {
    const tbody = $("#documentTableBody");
    tbody.empty();

    const search = ($("#txtSearchDocument").val() || "")
        .trim()
        .toLowerCase();

    let files = infinityDriveFiles;

    if (search) {

        files = files.filter(function (x) {

            const documentName = (x.DocumentName || "").toLowerCase();
            const categoryName = (x.CategoryName || "").toLowerCase();

            return documentName.includes(search) ||
                   categoryName.includes(search);

        });
    }

    if (!files.length) {
        tbody.html(`
            <tr>
                <td colspan="6" class="py-4">
                    <div class="d-flex align-items-center gap-2 text-muted ps-4">
                        <i class="fas fa-folder-open fa-lg"></i>
                        <span>No documents found</span>
                    </div>
                </td>
            </tr>
        `);
        return;
    }

    files.forEach(function (file, index) {
        const hasFile = !!file.DocumentFileName;

        const fileButton = hasFile
            ? `<a href="${file.DocumentFileName}"
                  target="_blank"
                  class="btn btn-outline-primary infinity-file-btn"
                  title="View File">
                   <i class="fas fa-eye"></i>
               </a>`
            : `<span class="badge bg-light text-muted border infinity-no-file">No File</span>`;

        tbody.append(`
            <tr>
                <td class="text-center">
                    ${index + 1}
                </td>

                <td>
                    <div class="fw-semibold">
                        ${escapeHtml(file.DocumentName || "-")}
                    </div>
                </td>

                <td>
                    <i class="fas fa-folder text-warning me-1"></i>
                    ${escapeHtml(file.CategoryName || "-")}
                </td>

                <td class="text-center">
                    ${file.SortOrder || 0}
                </td>

                <td class="text-center">
                    ${fileButton}
                </td>

                <td>
                    <div class="infinity-actions">

                        <button type="button"
                                class="btn btn-outline-warning infinity-file-btn"
                                onclick="editDocument(${file.Id})"
                                title="Edit">
                            <i class="fas fa-edit"></i>
                        </button>

                        <button type="button"
                                class="btn btn-outline-danger infinity-file-btn"
                                onclick="deleteDocument(${file.Id})"
                                title="Delete">
                            <i class="fas fa-trash"></i>
                        </button>

                    </div>
                </td>

            </tr>
        `);

    });
}


/* =========================================================
   OPEN ADD MODAL
========================================================= */
function openFileModal() {
    if (!checkPermission("CanAdd")) return;

    clearFileForm();

    $("#fileModalTitle").text("Add Document");

    $("#fileRequiredStar").removeClass("d-none");

    fileModal.show();
}


/* =========================================================
   EDIT
========================================================= */
function editDocument(id) {
    if (!checkPermission("CanEdit")) return;

    const file = infinityDriveFiles.find(function (x) {
        return Number(x.Id) === Number(id);
    });

    if (!file) {
        Swal.fire("Error", "Document not found.", "error");
        return;
    }

    clearFileForm();

    $("#fileModalTitle").text("Edit Document");

    $("#txtFileId").val(file.Id);
    $("#ddlFileCategory").val(file.CategoryId);
    $("#txtDocumentName").val(file.DocumentName || "");
    $("#txtSortOrder").val(file.SortOrder || 0);

    // File is optional during edit
    $("#fileRequiredStar").addClass("d-none");

    if (file.DocumentFileName) {

        $("#existingFileName").text(
            getFileNameFromUrl(file.DocumentFileName)
        );

        $("#existingFileLink")
            .attr("href", file.DocumentFileName);

        $("#existingFileBox").removeClass("d-none");

    }

    fileModal.show();
}


/* =========================================================
   SAVE
========================================================= */
async function saveDocument() {

    const id = parseInt($("#txtFileId").val()) || 0;
    const categoryId = parseInt($("#ddlFileCategory").val()) || 0;
    const documentName = ($("#txtDocumentName").val() || "").trim();
    const sortOrder = parseInt($("#txtSortOrder").val()) || 0;

    const input = document.getElementById("documentFile");
    const selectedFile = input.files.length ? input.files[0] : null;

    /* VALIDATION */
    if (!categoryId) {
        Swal.fire(
            "Validation",
            "Please select folder.",
            "warning"
        );
        return;
    }

    if (!documentName) {
        Swal.fire(
            "Validation",
            "Please enter document name.",
            "warning"
        );
        $("#txtDocumentName").focus();
        return;
    }

    // New document requires file
    if (id === 0 && !selectedFile) {
        Swal.fire("Validation", "Please select a file.", "warning");
        return;
    }

    // Validate selected file
    if (selectedFile) {
        const extension = selectedFile.name.split(".").pop().toLowerCase();

        if (!ALLOWED_TYPES.includes(selectedFile.type) || !ALLOWED_EXTENSIONS.includes(extension)) {
            Swal.fire("Validation", "Only PDF, JPG and PNG files are allowed.", "warning");
            input.value = "";
            return;
        }

        if (selectedFile.size > MAX_FILE_SIZE) {
            Swal.fire("Validation", "Maximum allowed file size is 1 MB.", "warning");
            input.value = "";
            return;
        }
    }

    try {
        let fileBase64 = null;
        if (selectedFile) {
            fileBase64 = await fileToBase64(selectedFile);
        }

        const payload = {
            Id: id,
            CategoryId: categoryId,
            DocumentName: documentName,
            SortOrder: sortOrder,
            FileBase64: fileBase64,
            ActionBy: parseInt(sessionStorage.getItem("userId")) || 0

        };

        console.log("Save Infinity Drive:", payload);

        $("#btnSaveDocument")
            .prop("disabled", true)
            .html(`
                <span class="spinner-border spinner-border-sm me-1"></span>
                Saving...
            `);

        $.ajax({
            url: "/api/InfinityDrive/SaveInfinityDrive_Files",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status !== "Success") {
                    Swal.fire(
                        "Error",
                        res.message,
                        "error"
                    );
                    return;
                }
                fileModal.hide();
                clearFileForm();
                loadFiles();
                handleApiSuccess(id > 0 ? "Document updated successfully." : "Document uploaded successfully.");
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Failed to save document");
            },
            complete: function () {
                $("#btnSaveDocument").prop("disabled", false).html(`<i class="fas fa-save me-1"></i> Save`);
            }
        });
    }
    catch (error) {
        console.error(error);
        Swal.fire(
            "Error",
            "Unable to read selected file.",
            "error"
        );
    }
}


/* =========================================================
   DELETE
========================================================= */
function deleteDocument(id) {
    if (!checkPermission("CanDelete")) return;

    const file = infinityDriveFiles.find(function (x) {
        return Number(x.Id) === Number(id);
    });

    if (!file)
        return;


    Swal.fire({

        title: "Delete Document?",

        html: `
            Are you sure you want to delete
            <strong>${escapeHtml(file.DocumentName)}</strong>?
        `,

        icon: "warning",

        showCancelButton: true,

        confirmButtonText: "Yes, Delete",

        cancelButtonText: "Cancel",

        confirmButtonColor: "#dc3545"

    }).then(function (result) {

        if (!result.isConfirmed)
            return;


        const payload = {

            Id: id,

            ActionBy:
                parseInt(
                    sessionStorage.getItem("userId")
                ) || 0

        };


        $.ajax({

            url: "/api/InfinityDrive/DeleteInfinityDrive_Files",

            type: "POST",

            contentType: "application/json",

            data: JSON.stringify(payload),

            success: function (res) {

                if (res.status !== "Success") {

                    Swal.fire(
                        "Error",
                        res.message,
                        "error"
                    );

                    return;
                }


                loadFiles();


                handleApiSuccess(
                    "Document deleted successfully."
                );

            },

            error: function (xhr, status, error) {

                handleApiError(
                    xhr,
                    status,
                    error,
                    "Failed to delete document"
                );

            }

        });

    });
}

/* =========================================================
   CLEAR FORM
========================================================= */
function clearFileForm() {

    $("#txtFileId").val(0);

    $("#ddlFileCategory").val("");

    $("#txtDocumentName").val("");

    $("#txtSortOrder").val(0);

    $("#documentFile").val("");

    $("#existingFileName").text("");

    $("#existingFileLink").attr("href", "#");

    $("#existingFileBox").addClass("d-none");
}


/* =========================================================
   GET FILE NAME FROM URL
========================================================= */
function getFileNameFromUrl(url) {

    if (!url)
        return "";

    try {

        const cleanUrl = url.split("?")[0];

        return decodeURIComponent(
            cleanUrl.substring(
                cleanUrl.lastIndexOf("/") + 1
            )
        );

    }
    catch {

        return "Uploaded File";

    }
}


/* =========================================================
   ESCAPE HTML
========================================================= */
function escapeHtml(value) {

    if (value === null || value === undefined)
        return "";

    return $("<div>")
        .text(value)
        .html();
}
