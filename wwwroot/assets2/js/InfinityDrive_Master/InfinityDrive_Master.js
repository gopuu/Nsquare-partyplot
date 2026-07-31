let folderList = [];
let folderModal = null;

$(document).ready(function () {
    folderModal = new bootstrap.Modal(document.getElementById("folderModal"));

    getFolders();

    $("#txtFolderSearch").on("input", function () {
        filterFolders();
    });

    $("#txtFolderName").on("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            saveFolder();
        }
    });

    $("#folderModal").on("hidden.bs.modal", function () {
        resetFolderForm();
    });
});


/* ═══════════════════════════════════════════════════
   GET FOLDERS
═══════════════════════════════════════════════════ */
function getFolders() {
    $("#folderLoader").removeClass("d-none");
    $("#folderTableContainer").addClass("d-none");
    $("#folderEmptyState").addClass("d-none");

    const payload = {
        Id: 0
    };

    $.ajax({
        url: "/api/InfinityDrive_Master/GetInfinityDrive_Master",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status !== "Success") {
                Swal.fire("Error", res.message || "Failed to load folders.", "error");
                return;
            }

            folderList = res.data || [];

            renderFolders(folderList);
            bindParentFolderDropdown();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load folders");
        },
        complete: function () {
            $("#folderLoader").addClass("d-none");
        }
    });
}


/* ═══════════════════════════════════════════════════
   RENDER FOLDERS
═══════════════════════════════════════════════════ */
function renderFolders(data) {
    const $tbody = $("#folderTableBody");

    $tbody.empty();

    if (!data || data.length === 0) {
        $("#folderTableContainer").addClass("d-none");
        $("#folderEmptyState").removeClass("d-none");
        return;
    }

    $("#folderEmptyState").addClass("d-none");
    $("#folderTableContainer").removeClass("d-none");

    data.forEach((folder, index) => {
        const parentName = folder.ParentFolderName || "-";

        $tbody.append(`
            <tr data-id="${folder.Id}">
                <td class="text-center">${index + 1}</td>

                <td>
                    <div class="d-flex align-items-center">
                        <i class="fas fa-folder text-warning me-2"></i>
                        <span class="fw-semibold">${escapeHtml(folder.FolderName)}</span>
                    </div>
                </td>

                <td>
                    ${folder.ParentFolderId > 0
                        ? `<span>${escapeHtml(parentName)}</span>`
                        : `<span class="text-muted">Root</span>`
                    }
                </td>

                <td class="text-center">
                    ${folder.SortOrder || 0}
                </td>

                <td class="text-center text-nowrap">
                    <button type="button"
                            class="btn btn-outline-warning btn-sm me-1"
                            title="Edit"
                            onclick="editFolder(${folder.Id})">
                        <i class="fas fa-edit"></i>
                    </button>

                    <button type="button"
                            class="btn btn-outline-danger btn-sm"
                            title="Delete"
                            onclick="deleteFolder(${folder.Id})">
                        <i class="fas fa-trash-alt"></i>
                    </button>
                </td>
            </tr>
        `);
    });
}


/* ═══════════════════════════════════════════════════
   SEARCH
═══════════════════════════════════════════════════ */
function filterFolders() {
    const search = ($("#txtFolderSearch").val() || "").trim().toLowerCase();

    if (!search) {
        renderFolders(folderList);
        return;
    }

    const filtered = folderList.filter(x =>
        (x.FolderName || "").toLowerCase().includes(search) ||
        (x.ParentFolderName || "").toLowerCase().includes(search)
    );

    renderFolders(filtered);
}


/* ═══════════════════════════════════════════════════
   OPEN ADD FOLDER
═══════════════════════════════════════════════════ */
function openFolderModal() {
    if (!checkPermission("CanAdd")) return;

    resetFolderForm();

    $("#folderModalTitle").html(`
        <i class="fas fa-folder-plus me-2"></i>Add Folder
    `);

    bindParentFolderDropdown();

    folderModal.show();

    setTimeout(function () {
        $("#txtFolderName").trigger("focus");
    }, 300);
}


/* ═══════════════════════════════════════════════════
   EDIT FOLDER
═══════════════════════════════════════════════════ */
function editFolder(id) {
    if (!checkPermission("CanEdit")) return;

    const folder = folderList.find(x => Number(x.Id) === Number(id));

    if (!folder) {
        Swal.fire("Error", "Folder not found.", "error");
        return;
    }

    resetFolderForm();

    $("#folderModalTitle").html(`
        <i class="fas fa-folder-open me-2"></i>Edit Folder
    `);

    $("#hdnFolderId").val(folder.Id);
    $("#txtFolderName").val(folder.FolderName);
    $("#txtSortOrder").val(folder.SortOrder || 0);

    bindParentFolderDropdown(folder.ParentFolderId || 0, folder.Id);

    folderModal.show();
}


/* ═══════════════════════════════════════════════════
   PARENT FOLDER DROPDOWN
═══════════════════════════════════════════════════ */
function bindParentFolderDropdown(selectedId = 0, excludeId = 0) {
    const $ddl = $("#ddlParentFolder");

    $ddl.empty();
    $ddl.append(`<option value="0">-- Root Folder --</option>`);

    folderList.forEach(folder => {
        if (Number(folder.Id) === Number(excludeId))
            return;

        $ddl.append(`
            <option value="${folder.Id}">
                ${escapeHtml(folder.FolderName)}
            </option>
        `);
    });

    $ddl.val(String(selectedId || 0));
}


/* ═══════════════════════════════════════════════════
   SAVE FOLDER
═══════════════════════════════════════════════════ */
function saveFolder() {
    const id = parseInt($("#hdnFolderId").val()) || 0;
    const folderName = ($("#txtFolderName").val() || "").trim();
    const parentFolderId = parseInt($("#ddlParentFolder").val()) || 0;
    const sortOrder = parseInt($("#txtSortOrder").val()) || 0;
    const actionBy = parseInt(sessionStorage.getItem("userId")) || 0;

    if (!folderName) {
        Swal.fire("Warning", "Please enter folder name.", "warning");
        $("#txtFolderName").trigger("focus");
        return;
    }

    if (!actionBy) {
        Swal.fire("Error", "Invalid logged-in user.", "error");
        return;
    }

    if (id > 0 && id === parentFolderId) {
        Swal.fire("Warning", "Folder cannot be its own parent.", "warning");
        return;
    }

    const payload = {
        Id: id,
        FolderName: folderName,
        ParentFolderId: parentFolderId,
        SortOrder: sortOrder,
        ActionBy: actionBy
    };

    const $btn = $("#btnSaveFolder");
    const originalHtml = $btn.html();

    $btn.prop("disabled", true);
    $btn.html(`<span class="spinner-border spinner-border-sm me-1"></span>Saving...`);

    $.ajax({
        url: "/api/InfinityDrive_Master/SaveInfinityDrive_Master",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status !== "Success") {
                Swal.fire("Error", res.message || "Failed to save folder.", "error");
                return;
            }

            folderModal.hide();

            handleApiSuccess(id === 0 ? "Folder created successfully." : "Folder updated successfully.");

            getFolders();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to save folder");
        },
        complete: function () {
            $btn.prop("disabled", false);
            $btn.html(originalHtml);
        }
    });
}


/* ═══════════════════════════════════════════════════
   DELETE FOLDER
═══════════════════════════════════════════════════ */
function deleteFolder(id) {
    if (!checkPermission("CanDelete")) return;

    const folder = folderList.find(x => Number(x.Id) === Number(id));

    if (!folder) {
        Swal.fire("Error", "Folder not found.", "error");
        return;
    }

    const actionBy = parseInt(sessionStorage.getItem("userId")) || 0;

    if (!actionBy) {
        Swal.fire("Error", "Invalid logged-in user.", "error");
        return;
    }

    Swal.fire({
        title: "Delete Folder?",
        html: `Are you sure you want to delete <strong>${escapeHtml(folder.FolderName)}</strong>?`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Yes, Delete",
        cancelButtonText: "Cancel",
        confirmButtonColor: "#dc3545",
        reverseButtons: true
    }).then(function (result) {
        if (!result.isConfirmed)
            return;

        const payload = {
            Id: id,
            ActionBy: actionBy
        };

        $.ajax({
            url: "/api/InfinityDrive_Master/DeleteInfinityDrive_Master",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status !== "Success") {
                    Swal.fire("Error", res.message || "Failed to delete folder.", "error");
                    return;
                }

                handleApiSuccess("Folder deleted successfully.");

                getFolders();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Failed to delete folder");
            }
        });
    });
}


/* ═══════════════════════════════════════════════════
   RESET FORM
═══════════════════════════════════════════════════ */
function resetFolderForm() {
    $("#hdnFolderId").val(0);
    $("#txtFolderName").val("");
    $("#ddlParentFolder").val("0");
    $("#txtSortOrder").val(0);
}


/* ═══════════════════════════════════════════════════
   HTML ENCODE
═══════════════════════════════════════════════════ */
function escapeHtml(value) {
    return $("<div>").text(value || "").html();
}