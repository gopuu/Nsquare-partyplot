$(document).ready(function () {
    loadTicketDropdown();
    loadAttachmentData();

});

$("#btn-add").click(function () {
    resetAttachmentForm();
    $("#AttachmentModalTitle").text("Add Attachment");
    $("#AttachmentModal").modal("show");
});

$(document).on("click", "#btnSaveAttachment", function () {

    // ---- Validation ----
    const ticketId = $("#ddlTicket").val();
    let fileInput = $("#AttachmentFile").get(0);
    let file = fileInput.files.length > 0 ? fileInput.files[0] : null;

    if (!ticketId) {
        Swal.fire("Validation", "Ticket is required", "warning");
        return;
    }
    // ---- Convert to base64 WITHOUT metadata ----
    function getBase64WithoutMeta(file, callback) {
        let reader = new FileReader();
        reader.onload = function (e) {
            let base64 = e.target.result.split(',')[1]; // 🔥 remove data:image/...;base64,
            callback(base64);
        };
        reader.readAsDataURL(file);
    }

    // ---- Save Attachment API Call ----
    function saveAttachment(base64File) {

        const payload = {
            Id: Number($("#AttachmentId").val() || 0),
            TicketId: Number($("#ddlTicket").val()),
            FileName: $("#AttachmentFile")[0]?.files[0]?.name || "",
            FilePath: base64File,   // null allowed for edit
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/SupportTicketAttachment/SaveSupportTicketAttachment",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    Swal.fire({
                        icon: "success",
                        title: "Saved!",
                        timer: 1200,
                        showConfirmButton: false,
                        position: "top-end"
                    });
                    $("#AttachmentModal").modal("hide");
                    loadAttachmentData();
                } else {
                    Swal.fire("Error", res?.message || "Save failed", "error");
                }
            },
            error: function (xhr, status, error) {
                console.log("Error:", xhr);
                Swal.fire("API Error", "Something went wrong.", "error");
            },
        
        });
    }


// ---- File exists → convert → save ----
getBase64WithoutMeta(file, function (base64File) {
    saveAttachment(base64File);
});
});

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
    $("#AttachmentModalTitle").text("Edit Attachment");
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
            { data: "FileName", title: "File Name" },
            {
                data: "FilePath",
                title: "Preview",
                render: d => `<a href="${d}" target="_blank">View</a>`
            },
            {
                data: null,
                title: "Action",
                render: function () {
                    return `
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

    $("#deleteAttachmentModal").data("id", data.Id).modal("show");
});

$("#btnDeleteAttachment").click(function () {

    let id = $("#deleteAttachmentModal").data("id");

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
                $("#deleteAttachmentModal").modal("hide");
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


