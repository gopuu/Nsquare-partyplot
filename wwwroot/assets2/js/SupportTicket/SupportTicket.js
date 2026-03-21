$(document).ready(function () {   
    fetchData();
    loadTicketTypeDropdown();
    loadPriorityDropdown();
    loadStatusDropdown();
    addDocumentRow();
});

// ADD
$("#btn-add").click(function () {
    resetSupportTicketForm();
    $("#ticketModalTitle").text("Add Support Ticket");
    $("#SupportTicketModal").modal("show");
});

// 🔹 Reset Sr No
function resetSrNo() {
    $("#tblDocuments tbody tr").each(function (index) {
        $(this).find(".srno").text(index + 1);
    });
}

$("#btnSave").click(async function () {

    if ($("#TicketNumber").val().trim() === "") {
        Swal.fire("Info", "Ticket Number is required", "info");
        return;
    }

    /* =====================
       COLLECT ATTACHMENTS
       (NO BASE64 – just file names)
    ======================*/
    let attachmentArr = [];
    let rows = $("#tblDocuments tbody tr");

    for (let i = 0; i < rows.length; i++) {
        let row = rows[i];
        let title = $(row).find(".txtTitle").val()?.trim();
        let fileInput = $(row).find(".docFile")[0];
        let fileName = fileInput?.files[0]?.name || $(row).find(".fileName").val(); // existing file name if already uploaded

        if (fileName) {
            attachmentArr.push({
                FileName: title || fileName,
                FilePath: fileName  // <-- just save file name or server path
            });
        }
    }

    console.log("Attachments:", attachmentArr);

    /* =====================
       PAYLOAD
    ======================*/
    const payload = {
        Id: parseInt($("#TicketId").val()) || 0,
        TicketNumber: $("#TicketNumber").val().trim(),
        TicketTypeId: parseInt($("#TicketTypeId").val()) || 0,
        PriorityId: parseInt($("#PriorityId").val()) || 0,
        StatusId: parseInt($("#StatusId").val()) || 0,
        Title: $("#Title").val().trim(),
        Description: $("#Description").val().trim(),
        IsClosed: $("#IsClosed").is(":checked"),
        ActionBy: parseInt(sessionStorage.getItem("userId")) || 0,
        AttachmentDetails: attachmentArr
    };

    $.ajax({
        url: "/api/SupportTicket/SaveSupportTicket",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                handleApiSuccess("Saved !!");
                $("#SupportTicketModal").modal("hide");
                fetchData();
            } else {
                Swal.fire("Info", res.message, "info");
            }
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, status, error, "Failed to Save Suport Ticket");
        }
    });
});



// DELETE
$(document).on("click", ".btn-delete", function () {
    let data = $("#tableMain table").DataTable().row($(this).closest("tr")).data();
    $("#deleteRecordModal").data("id", data.Id).modal("show");
});

$("#delete-record").click(function () {
    let id = $("#deleteRecordModal").data("id");

    $.ajax({
        url: "/api/SupportTicket/DeleteSupportTicket",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        }),
        success: function (res) {
            if (res.status === "Success") {
                handleApiSuccess("Deleted !!");
            }
            else {
                Swal.fire({
                    title: "Delete Fail..!!",
                    text: res.message,
                    icon: "error"
                });
            }
            $("#deleteRecordModal").modal("hide");
            fetchData();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Delete SupportTicket Type Error");
        }
    });
});

function resetSupportTicketForm() {
    $("#TicketId").val(0);
    $("#TicketNumber").val("").prop("disabled", false);
    $("#TicketTypeId").val("");
    $("#PriorityId").val("");
    $("#StatusId").val("");
    $("#Title").val("");
    $("#Description").val("");
    $("#IsClosed").prop("checked", false);
    $("#tblDocuments tbody").empty();
    docIndex = 0;
    addDocumentRow();
}

function fetchData() {
    $.ajax({
        url: "/api/SupportTicket/GetSupportTicketList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            console.log("list res:",res);
            renderSupportTicketTable(res.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Support Ticket  Error");
        }
    });
}
function renderSupportTicketTable(data) {

    if ($.fn.dataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#tableMain table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "Title", title: "Title" },
            { data: "TicketNumber", title: "Ticket No" },
            { data: "TicketTypeName", title: "TicketType Name" },
            { data: "PriorityName", title: "Priority" },
            { data: "StatusName", title: "Status" },
            {
                data: "IsClosed",
                title: "Closed",
                render: function (data) {
                    return data
                        ? "<span class='badge bg-success'>Yes</span>"
                        : "<span class='badge bg-danger'>No</span>";
                }
            },
            //{
            //    data: null,
            //    title: "Action",
            //    render: function () {
            //        return `


            //            <button class="btn btn-outline-danger btn-sm btn-delete">
            //                <i class="fa fa-trash"></i>
            //            </button>
            //        `;
            //    }
            //}
        ]
    });
}

function loadTicketTypeDropdown(selectedId = "") {
    $.ajax({
        url: "/api/SupportTicketType/GetSupportTicketTypeList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            let dropdown = $("#TicketTypeId");
            dropdown.empty();

            dropdown.append(`<option value="">-- Select Ticket Type --</option>`);

            response.data.forEach(item => {
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                        ${item.Name}
                    </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load Ticket Type");
        }
    });
}

function loadPriorityDropdown(selectedId = "") {
    $.ajax({
        url: "/api/SupportTicketPriority/GetSupportTicketPriorityList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            let dropdown = $("#PriorityId");
            dropdown.empty();

            dropdown.append(`<option value="">-- Select Priority --</option>`);

            response.data.forEach(item => {
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                        ${item.Name}
                    </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load Priority");
        }
    });
}

function loadStatusDropdown(selectedId = "") {
    $.ajax({
        url: "/api/SupportTicketStatus/GetSupportTicketStatusList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {

            let dropdown = $("#StatusId");
            dropdown.empty();

            dropdown.append(`<option value="">-- Select Status --</option>`);

            response.data.forEach(item => {
                dropdown.append(
                    `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                        ${item.Name}
                    </option>`
                );
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load Status");
        }
    });
}

let docIndex = 0;

$("#btnAddRow").click(function () {
    addDocumentRow();
});

function addDocumentRow() {
    docIndex++;

    let row = `
        <tr>
            <td class="srno ps-3">${docIndex}</td>

            <td>
                <input type="text" class="form-control txtTitle" placeholder="Enter Title">
            </td>

            <td>
                <div class="input-group">
                    <button class="btn btn-dark file-input">
                        Choose File
                        <input type="file" class="docFile" accept="image/*,.pdf">
                    </button>
                    <input type="text" class="form-control fileName" placeholder="No file chosen" readonly>
                </div>
            </td>

            <td class="action-btns">
                <button type="button" class="btn btn-sm btn-action-xss btn-outline-primary btnView">
                    <i class="fa fa-eye"></i>
                </button>
                              

                <button type="button" class="btn btn-sm  btn-action-xss btn-outline-danger btnDelete">
                    <i class="fa fa-times"></i>
                </button>
            </td>
        </tr>
        `;

    $("#tblDocuments tbody").append(row);
    resetSrNo();
}

// File select
$(document).on("change", ".docFile", function () {
    let fileName = this.files[0]?.name || "";
    $(this).closest("td").find(".fileName").val(fileName);
});

// Delete row with SweetAlert
$(document).on("click", ".btnDelete", function () {
    let row = $(this).closest("tr");

    Swal.fire({
        title: 'Are you sure?',
        text: 'Do you want to delete this document?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        confirmButtonText: 'Yes, Delete'
    }).then((result) => {
        if (result.isConfirmed) {
            row.remove();
            resetSrNo();
        }
    });
});

// View file (image preview only)
$(document).on("click", ".btnView", function () {
    let fileInput = $(this).closest("tr").find(".docFile")[0];

    if (!fileInput.files.length) {
        Swal.fire("No File", "Please select a file first", "info");
        return;
    }

    let file = fileInput.files[0];
    let url = URL.createObjectURL(file);

    window.open(url, "_blank");
});

// Download file
//$(document).on("click", ".btnDownload", function () {
//    let fileInput = $(this).closest("tr").find(".docFile")[0];
//    if (!fileInput.files.length) {
//        Swal.fire("No File", "Please select a file first", "info");
//        return;
//    }
//    let file = fileInput.files[0];
//    let url = URL.createObjectURL(file);
//    let a = document.createElement("a");
//    a.href = url;
//    a.download = file.name;
//    a.click();
//});



function toBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => resolve(reader.result);
        reader.onerror = error => reject(error);
    });
}