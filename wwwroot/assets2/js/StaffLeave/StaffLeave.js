$(document).ready(function () {
    fetchStaffLeave();

    $(document).on("click", ".btn-edit", function () {
        const table = $("#tableMain table").DataTable();
        const row = table.row($(this).closest("tr")).data();
        resetForm();
        $('#staffLeaveId').val(row.Id);
        loadDropdowns({
            StaffId: row.StaffId,
            LeaveTypeId: row.LeaveTypeId,
            StatusId: row.StatusId
        });

        if (row.FromDate) {
            const date = new Date(row.FromDate);
            const yyyy = date.getFullYear();
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const dd = String(date.getDate()).padStart(2, '0');
            $('#fromDate').val(`${yyyy}-${mm}-${dd}`);
        }

        if (row.ToDate) {
            const date = new Date(row.ToDate);
            const yyyy = date.getFullYear();
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const dd = String(date.getDate()).padStart(2, '0');
            $('#toDate').val(`${yyyy}-${mm}-${dd}`);
        }

        $("#reason").val(row.Reason);
        $('#staffLeaveTitle').text("Edit Staff Leave");
        $('#staffLeaveModal').modal('show');
    });

    $(document).on('click', '#btn-add', function () {
        resetForm();
        loadDropdowns();
        $('#staffLeaveTitle').text("Add Staff Leave");
        $('#staffLeaveModal').modal('show');
    });

    function loadDropdowns(selected = {}) {
        loadStaffDropdown(selected.StaffId);
        loadLeaveTypeDropdown(selected.LeaveTypeId);
        loadStatusDropdown(selected.StatusId);
    }

    $("#btnSaveStaffLeave").click(function () {

        let payload = {
            Id: Number($("#staffLeaveId").val() || 0),
            StaffId: Number($("#ddlStaff").val()),
            LeaveTypeId: Number($("#ddlLeaveType").val()),
            FromDate: $("#fromDate").val(),
            ToDate: $("#toDate").val(),
            Reason: $("#reason").val(),
            StatusId: Number($("#ddlStatus").val()),
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/StaffLeave/SaveStaffLeave",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: (res) => {
                if (res.status === "Success") {
                    $('#staffLeaveModal').modal('hide');
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 1200
                    });
                    fetchStaffLeave();
                } else {
                    Swal.fire("Error", res.message, "error");
                }
            }
        });
    });

    // Delete button click
    $(document).on('click', '.btn-delete', function () {
        const canDelete = (sessionStorage.getItem('CanDelete') || localStorage.getItem('CanDelete')) === 'true';
        if (!canDelete) {
            Swal.fire("Access Denied", "You do not have permission to Delete.", "error");
            return;
        }
        let table = $('#tableMain table').DataTable();
        let rowData = table.row($(this).closest('tr')).data();
        $("#deleteRecordModal")
            .data("id", rowData.Id)
            .modal("show");
    });

    // Confirm Delete
    $("#delete-record").click(function () {
        let id = $("#deleteRecordModal").data("id");
        let payload = {
            Id: id,
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };
        $('#btn-loader').show();
        $.ajax({
            url: "/api/StaffLeave/DeleteStaffLeave",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),

            success: function (response) {
                if (typeof response === "string") {
                    response = JSON.parse(response);
                }
                if (response.status === "Success") {

                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Leave Type Deleted!",
                        showConfirmButton: false,
                        timer: 1000
                    });

                } else {
                    Swal.fire("Delete Failed", response.message, "error");
                }

                $("#deleteRecordModal").modal("hide");
                fetchStaffLeave(); // reload updated list
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete Leave Type Error");
            },
            complete: function () {
                $('#btn-loader').hide();
            }
        });
    });
});

function loadStaffDropdown(selectedId = 0) {
    $.ajax({
        url: "/api/StaffMaster/GetStaffMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            let ddl = $("#ddlStaff");
            ddl.empty();
            ddl.append(`<option value="">-- Select Staff --</option>`);

            if (res.data && res.data.length > 0) {
                res.data.forEach(row => {
                    ddl.append(`<option value="${row.Id}">${row.Name}</option>`);
                });

                if (selectedId) ddl.val(selectedId);
            }
        }
    });
}

function loadLeaveTypeDropdown(selectedId = 0) {
    $.ajax({
        url: "/api/LeaveTypeMaster/GetLeaveTypeMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            let ddl = $("#ddlLeaveType");
            ddl.empty();
            ddl.append(`<option value="">-- Select Leave Type --</option>`);

            res.data?.forEach(row => {
                ddl.append(`<option value="${row.Id}">${row.LeaveTypeName}</option>`);
            });

            if (selectedId) ddl.val(selectedId);
        }
    });
}

function loadStatusDropdown(selectedId = 0) {
    $.ajax({
        url: "/api/StatusMaster/GetStatusMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, StatusType: "Leave" }),
        success: function (res) {
            let ddl = $("#ddlStatus");
            ddl.empty();
            ddl.append(`<option value="">-- Select Status --</option>`);

            res.data?.forEach(row => {
                ddl.append(`<option value="${row.Id}">${row.StatusName}</option>`);
            });

            if (selectedId) ddl.val(selectedId);
        }
    });
}

// FETCH DATA
function fetchStaffLeave() {
    $.ajax({
        url: "/api/StaffLeave/GetStaffLeave",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: (res) => renderTable(res.data ?? [])
    });
}

function renderTable(data) {

    if ($.fn.DataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $("#tableMain table").DataTable({
        data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "StaffName", title: "Staff" },
            { data: "LeaveTypeName", title: "Leave Type" },
            {
                data: "FromDate",
                title: "FromDate",
                render: function (d) {
                    if (!d) return '';
                    let date = new Date(d);
                    return date.toLocaleDateString(); // shows only date
                }
            },
            {
                data: "ToDate",
                title: "ToDate",
                render: function (d) {
                    if (!d) return '';
                    let date = new Date(d);
                    return date.toLocaleDateString(); // shows only date
                }
            },
            { data: "Reason", title: "Reason" },
            {
                data: "StatusName", title: "Status",
            },
            {
                data: "Id",
                title: "Actions",
                render: id => `
                        <button class="btn btn-outline-warning btn-sm btn-edit btn-action-xss"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-outline-danger btn-sm btn-delete btn-action-xss" data-id="${id}"><i class="fa fa-trash"></i></button>`
            }
        ]
    });
}

function resetForm() {
    $("#staffLeaveId").val(0);
    $("#ddlStaff").val("");
    $("#ddlLeaveType").val("");
    $("#fromDate").val("");
    $("#toDate").val("");
    $("#reason").val("");
    $("#ddlStatus").val(1);
}
