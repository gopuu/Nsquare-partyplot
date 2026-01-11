$(document).ready(function () {
    fetchLeaveTypeData();

    $(document).on('click', '#btn-add', function () {
        const canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';
        if (!canAdd) {
            Swal.fire("Access Denied", "You do not have permission to Add.", "error");
            return;
        }

        resetLeaveTypeForm();
        $('#leaveTypeModalTitle').text('Add Leave Type');
        $('#leaveTypeModal').modal('show');
    });

    $('#btnSaveLeaveType').on('click', function () {
        const name = $('#leaveTypeName').val().trim();
        if (!name) {
            Swal.fire("Validation", "Leave Type Name is required", "warning");
            return;
        }

        const payload = {
            Id: Number($('#leaveTypeId').val() || 0),
            LeaveTypeName: name,
            PaidFlag: $('#paidFlagSwitch').is(":checked"),
            PriorityNo: Number($('#priorityNo').val() || 0),
            ActionBy: sessionStorage.getItem('userId') ?? 0
        };

        $('#btnSaveLeaveType').prop('disabled', true);

        $.ajax({
            url: '/api/LeaveTypeMaster/SaveLeaveTypeMaster',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                if (res && res.status === "Success") {

                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 1200
                    });

                    $('#leaveTypeModal').modal('hide');
                    fetchLeaveTypeData();
                } else {
                    Swal.fire("Error", res?.message || "Save failed", "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save Leave Type Error");
            },
            complete: function () {
                $('#btnSaveLeaveType').prop('disabled', false);
            }
        });
    });

    $(document).on('click', '.btn-edit', function () {
        const canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';
        if (!canEdit) {
            Swal.fire("Access Denied", "You do not have permission to Edit.", "error");
            return;
        }

        const table = $('#tableMain table').DataTable();
        const row = $(this).closest('tr');
        const rowData = table.row(row).data();

        resetLeaveTypeForm();

        $('#leaveTypeId').val(rowData.Id);
        $('#leaveTypeName').val(rowData.LeaveTypeName);
        $('#priorityNo').val(rowData.PriorityNo);
        $('#paidFlagSwitch').prop("checked", rowData.PaidFlag);

        $('#leaveTypeModalTitle').text('Edit Leave Type');
        $('#leaveTypeModal').modal('show');

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
            url: "/api/LeaveTypeMaster/DeleteLeaveTypeMaster",
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
                fetchLeaveTypeData(); // reload updated list
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

function resetLeaveTypeForm() {
    $('#leaveTypeId').val(0);
    $('#leaveTypeName').val('');
    $('#priorityNo').val('');
    $('#paidFlagSwitch').prop('checked', false);
}

function fetchLeaveTypeData() {
    $.ajax({
        url: '/api/LeaveTypeMaster/GetLeaveTypeMaster',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0 }),

        success: function (res) {

            const data = (res && res.data) ? res.data : [];
            renderLeaveTypeTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Leave Type Error");
        }
    });
}

function renderLeaveTypeTable(data) {

    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    if (!data || data.length === 0) {

        $('#tableMain table').DataTable({
            data: [],
            columns: [
                { data: 'srno', title: 'Sr. No.' },
                { data: 'LeaveTypeName', title: 'Leave Type' },
                { data: 'PaidFlag', title: 'Paid?', render: d => d ? 'Yes' : 'No' },
                { data: 'PriorityNo', title: 'Priority' },
                {
                    data: null,
                    title: 'Actions',
                    render: function () {
                        return `
                        <button class="btn btn-outline-warning btn-action-xs btn-sm btn-edit"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-outline-danger btn-action-xs btn-sm btn-delete"><i class="fa fa-trash"></i></button>
                `;
                    }
                }
            ],
            pageLength: 10,
            ordering: true,
            searching: true,
            lengthChange: false
        });

        return;
    }

    data.forEach((x, i) => x.srno = i + 1);

    $('#tableMain table').DataTable({
        data: data,
        columns: [
            { data: 'srno', title: 'Sr. No.' },
            { data: 'LeaveTypeName', title: 'Leave Type' },
            {
                data: 'PaidFlag',
                title: 'Paid?',
                render: d => d
                    ? `<span class="badge bg-success">Paid</span>`
                    : `<span class="badge bg-secondary">Unpaid</span>`
            },
            { data: 'PriorityNo', title: 'Priority' },

            {
                data: null,
                title: 'Actions',
                render: function () {
                    return `
                    <div class="d-flex gap-2">
                        <button class="btn btn-outline-warning btn-action-xss btn-edit"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-outline-danger btn-action-xss btn-delete"><i class="fa fa-trash"></i></button>
                    </div>
                `;
                }
            }
        ],
        columnDefs: [{ targets: 0 }],
        pageLength: 10,
        ordering: true,
        searching: true,
        lengthChange: false
    });
}
