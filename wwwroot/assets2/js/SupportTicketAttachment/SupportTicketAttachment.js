$(document).ready(function () {
    loadTicketDropdown();
    loadAttachmentData();

});

$("#btn-add").click(function () {
    resetAttachmentForm();
    $("#AttachmentModalTitle").text("Add Support Ticket Attachment");
    $("#AttachmentModal").modal("show");
});


$(document).on("click", "#btnSaveAttachment", function () {

    const ticketId = $("#ddlTicket").val();
    let fileInput = $("#AttachmentFile").get(0);
    let file = fileInput.files.length > 0 ? fileInput.files[0] : null;
    let existingPath = $("#existingAttachmentPath").val();

    if (!ticketId) {
        Swal.fire("Validation", "Ticket is required", "warning");
        return;
    }

    // 🔹 ADD → File mandatory
    if ($("#AttachmentId").val() == 0 && !file) {
        Swal.fire("Validation", "File is required", "warning");
        return;
    }

    // ---- SAVE WITHOUT FILE (EDIT CASE) ----
    if (!file) {
        saveAttachment(null);
        return;
    }

    // ---- Convert to base64 ----
    let reader = new FileReader();
    reader.onload = function (e) {
        let base64 = e.target.result.split(',')[1];
        saveAttachment(base64);
    };
    reader.readAsDataURL(file);
});

function saveAttachment(base64File) {

    const payload = {
        Id: Number($("#AttachmentId").val() || 0),
        TicketId: Number($("#ddlTicket").val()),
        FileName: $("#AttachmentFile")[0]?.files[0]?.name || "",
        FilePath: base64File,  
        ActionBy: sessionStorage.getItem("userId") ?? 0
    };

    $.ajax({
        url: "/api/SupportTicketAttachment/SaveSupportTicketAttachment",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                handleApiSuccess("Attachment saved successfully.");
                $("#AttachmentModal").modal("hide");
                loadAttachmentData();
            } else {
                Swal.fire("Error", res.message, "error");
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, status, error, "Failed to Save ticket attachements");
        }
    });
}


$(document).on("click", ".btn-edit-attachment", function () {

    // ---- Permission Check ----
    const canEdit = (sessionStorage.getItem("CanEdit") || localStorage.getItem("CanEdit")) === "true";
    if (!canEdit) {
        Swal.fire("Access Denied", "You do not have permission to Edit.", "error");
        return;
    }

    // ---- Get Row Data ----
    const table = $("#tableAttachments table").DataTable();
    const rowData = table.row($(this).closest("tr")).data();
    resetAttachmentForm();

    console.log("Attachment Edit Row:", rowData);

    // ---- Fill Modal Fields ----
    $("#AttachmentId").val(rowData.Id);
    $("#ddlTicket").val(rowData.TicketId).trigger("change");

    // File input cannot be prefilled (browser security)
    $("#AttachmentFile").val("");

    // ---- Existing File Preview / Download ----
    $("#existingAttachmentPath").val(rowData.FilePath || "");

    if (rowData.FilePath && rowData.FilePath !== "") {
        $("#btnViewAttachment")
            .removeClass("d-none")
            .attr("href", rowData.FilePath)
            .attr("target", "_blank");
    } else {
        $("#btnViewAttachment").addClass("d-none");
    }

    // ---- Modal UI ----
    $("#AttachmentModalTitle").text("Edit Support Ticket Attachment");
    $("#AttachmentModal").modal("show");
});


function loadAttachmentData() {
    $.ajax({
        url: "/api/SupportTicketAttachment/GetSupportTicketAttachmentList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ TicketId: 0 }),
        success: function (res) {
            renderAttachmentTable(res.data || []);
        }
    });
}

function renderAttachmentTable(data) {

    if ($.fn.dataTable.isDataTable("#tableAttachments table")) {
        $("#tableAttachments table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#tableAttachments table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            //{ data: "FileName", title: "File Name" },
            { data: "TicketDisplay", title: "Ticket" },
            {
                data: "FilePath",
                title: "Preview",
                orderable: false,
                render: function (d) {
                    return renderPreview(d);
                }
            },
            {
                data: null,
                title: "Action",
                orderable: false,
                render: function () {
                    return `
                        <button class="btn btn-outline-warning btn-sm btn-edit-attachment me-1">
                            <i class="fa fa-edit"></i>
                        </button>
                        <button class="btn btn-outline-danger btn-sm btn-delete">
                            <i class="fa fa-trash"></i>
                        </button>`;
                }
            }
        ]
    });
}



$(document).on("click", ".btn-delete", function () {
    let table = $("#tableAttachments table").DataTable();
    let data = table.row($(this).closest("tr")).data();

    $("#deleteRecordModal").data("id", data.Id).modal("show");
});

$("#delete-record").click(function () {

    let id = $("#deleteRecordModal").data("id");

    $.ajax({
        url: "/api/SupportTicketAttachment/DeleteSupportTicketAttachment",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        }),
        success: function (res) {
            if (res.status === "Success") {
                Swal.fire("Deleted", res.message, "success");
                $("#deleteRecordModal").modal("hide");
                loadAttachmentData();
            }
        }
    });
});

function loadTicketDropdown() {
    $.ajax({
        url: "/api/SupportTicket/GetSupportTicketList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            let ddl = $("#ddlTicket");
            ddl.empty().append(`<option value="">-- Select Ticket --</option>`);

            res.data.forEach(t => {
                ddl.append(`<option value="${t.Id}">
                    #${t.TicketNumber} - ${t.Title}
                </option>`);
            });
        }
    });
}

function resetAttachmentForm() {
    $("#AttachmentId").val(0);
    $("#ddlTicket").val("");
    $("#AttachmentFile").val("");
}

function renderPreview(filePath) {

    if (!filePath) return "-";

    let type = getFileType(filePath);

    switch (type) {

        case "image":
            return `
                <a href="${filePath}" target="_blank">
                    <img src="${filePath}"
                         style="width:60px;height:60px;object-fit:cover;border-radius:4px"/>
                </a>`;

        case "pdf":
            return `
                <a href="${filePath}" target="_blank" class="btn btn-sm btn-outline-primary">
                    <i class="fa fa-file-pdf"></i> View PDF
                </a>`;

        case "doc":
            return `
                <a href="${filePath}" target="_blank" class="btn btn-sm btn-outline-success">
                    <i class="fa fa-download"></i> Download
                </a>`;

        default:
            return `
                <a href="${filePath}" target="_blank" class="btn btn-sm btn-outline-secondary">
                    View File
                </a>`;
    }
}

function getFileType(url) {
    if (!url) return "unknown";

    let ext = url.split('.').pop().toLowerCase();

    if (["jpg", "jpeg", "png", "gif", "webp"].includes(ext)) return "image";
    if (ext === "pdf") return "pdf";
    if (["doc", "docx", "xls", "xlsx", "csv"].includes(ext)) return "doc";

    return "unknown";
}
