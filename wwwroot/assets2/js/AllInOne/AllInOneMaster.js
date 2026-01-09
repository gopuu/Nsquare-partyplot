$(document).ready(function () {
    fetchAllInOne();
    resetAllInOneFields();

    // ➕ Add
    $(document).on("click", "#btn-add", function () {

        var canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';
        if (!canAdd) {
            Swal.fire("Access Denied", "You don't have permission to Add!", "error");
            return;
        }
        resetAllInOneFields();
        $("#allInOneModalTitle").text("Add All In One");
        $("#allInOneModal").modal("show");
    });

    // 💾 Save
    $("#btnSaveAllInOne").click(function (e) {
        e.preventDefault();

        if ($("#allInOneTitle").val().trim() === "") {
            Swal.fire("Validation", "Title is required", "warning");
            return;
        }

        let file = $("#allInOneImage")[0].files[0];

        if (file) {
            // ✅ New image selected
            convertToBase64(file, function (base64) {
                saveAllInOne(base64);
            });
        } else {
            // ✅ Image not changed → send NULL
            saveAllInOne(null);
        }
    });

    // Convert image to base64 
    function convertToBase64(file, callback) {
        let reader = new FileReader();
        reader.onload = e => {
            // send only base64 string without prefix
            callback(e.target.result.split(",")[1]);
        };
        reader.readAsDataURL(file);
    }

    // Final Save Function
    function saveAllInOne(base64Image) {

        let payload = {
            Id: $("#allInOneId").val(),
            Title: $("#allInOneTitle").val(),
            Link: $("#allInOneLink").val(),
            Image: base64Image, // null OR base64
            Action_By: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/AllInOneMaster/SaveAllInOneMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 1000
                    });
                    $("#allInOneModal").modal("hide");
                    fetchAllInOne();
                } else {
                    Swal.fire("Error", res.message, "error");
                }
            }
        });
    }

    // ✏️ Edit
    $(document).on("click", ".btn-edit", function () {

        let table = $("#tableAllInOne table").DataTable();
        let data = table.row($(this).closest("tr")).data();

        $("#allInOneId").val(data.Id);
        $("#allInOneTitle").val(data.Title);
        $("#allInOneLink").val(data.Link);
        console.log("edit data",data);

        // 🟢 Store OLD IMAGE (base64 or filename)
        if (data.Image) {
            $("#allInOneImagePreview")
                .attr("src", data.Image)
                .show();
        } else {
            $("#allInOneImagePreview").hide();
        }

        $("#allInOneImage").val(""); // clear file input
        $("#allInOneModalTitle").text("Edit All In One");
        $("#allInOneModal").modal("show");
    });

    $("#allInOneImage").on("change", function () {

        var file = this.files[0];
        var errorMsg = $("#imgError");

        errorMsg.addClass("d-none");

        if (!file) return;

        var allowedTypes = ["image/png", "image/jpeg"];

        if (!allowedTypes.includes(file.type)) {
            errorMsg.removeClass("d-none");
            this.value = ""; // clear file
            return;
        }
    });



    // 🗑 Delete (confirm)
    $(document).on("click", ".btn-delete", function () {
        let table = $("#tableAllInOne table").DataTable();
        let row = table.row($(this).closest("tr")).data();

        $("#deleteRecordModal").data("id", row.Id).modal("show");
    });

    // ❌ Delete confirmed
    $("#delete-record").click(function () {

        let payload = {
            Id: $("#deleteRecordModal").data("id"),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/AllInOneMaster/DeleteAllInOneMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function () {
                Swal.fire("Deleted!", "Record deleted successfully!", "success");
                $("#deleteRecordModal").modal("hide");
                fetchAllInOne();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete AllInOne Error");
            }
        });
    });
});

function fetchAllInOne() {
    $.ajax({
        url: "/api/AllInOneMaster/GetAllInOneMaster",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            renderAllInOneTable(response.data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch AllInOne Error");
        }
    });
}

function renderAllInOneTable(data) {
    if ($.fn.dataTable.isDataTable("#tableAllInOne table")) {
        $("#tableAllInOne table").DataTable().destroy();
    }
    data.forEach((x, i) => x.srno = i + 1);
    $("#tableAllInOne table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr. No." },
            { data: "Title", title: "Title" },
            {
                data: "Link",
                title: "Link",
                render: function (data, type, row, meta) {
                    if (data) {
                        return `<a href="${data}" target="_blank">${data}</a>`;
                    }
                    return "-";
                }
            },
            {
                data: "Image", title: "Image",
                render: function (d) {
                    return d
                        ? `<img src="${d}" class="rounded" width="40" height="40" />`
                        : `<span class="text-muted">No Image</span>`;
                }
            },
            {
                data: null,
                title: "Actions",
                render: () => `
                    <button class="btn btn-outline-warning btn-action-xs btn-edit">
                        <i class="fa fa-edit"></i>
                    </button>
                    <button class="btn btn-outline-danger btn-action-xs btn-delete">
                        <i class="fa fa-trash"></i>
                    </button>
                `
            }
        ]
    });
}

function resetAllInOneFields() {
    $("#allInOneId").val(0);
    $("#allInOneTitle").val("");
    $("#allInOneImage").val("");
    $("#allInOneImagePreview").attr("src", "").hide();
}
