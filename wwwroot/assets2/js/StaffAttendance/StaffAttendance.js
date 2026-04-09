$(document).ready(function () {
    // 1️⃣ Set default dates (today)
    let today = new Date().toISOString().split('T')[0];
    $("#txtfromdate, #txttodate").val(today);

    // 2️⃣ Load dropdowns
    loadStaffDropdown();
    loadStatusDropdown().then(() => {
        fetchAttendanceData();
    });

    // 3️⃣ Change events to refresh table
    $("#ddlStaff, #ddlStatus, #txtfromdate, #txttodate").on("change", function () {
        fetchAttendanceData();
    });

    //keep status selecte when time changes & when status=0
    $(document).on("change", ".checkin-time, .checkout-time", function () {
        let row = $(this).closest("tr");
        let statusDropdown = row.find(".duty-status");
        let currentStatus = Number(statusDropdown.val() || 0);

        // If status is empty or 0, auto-select Present
        if (currentStatus === 0) {
            let presentId = getPresentStatusId();
            if (presentId > 0) {
                statusDropdown.val(presentId);
            }
        }
    });

    $("#btnSaveAttendance").on("click", function () {
        let table = $('#tableAttendance table').DataTable();
        let attendanceList = [];

        table.rows().every(function () {
            let rowData = this.data();          // original row object
            let rowNode = this.node();          // actual <tr> element

            // let checkInTime  = $(rowNode).find('.checkin-time').val();
            // let checkOutTime = $(rowNode).find('.checkout-time').val();
            let checkInTime = $(rowNode).find('.checkin-time-hidden').val();
            let checkOutTime = $(rowNode).find('.checkout-time-hidden').val();
            let statusId = Number($(rowNode).find('.duty-status').val());

            // ✅ pass only rows that have some data
            //if (checkInTime || checkOutTime || statusId) {
            attendanceList.push({
                Id: rowData.AttendanceId,
                StaffId: rowData.StaffId,
                AttendanceDate: rowData.AttendanceDate,
                CheckInTime: checkInTime || null,
                CheckOutTime: checkOutTime || null,
                StatusId: statusId || null
            });
            //}
        });
        // If no rows have valid data, show a message to the user
        if (attendanceList.length === 0) {
            Swal.fire({
                icon: "info",
                title: "No Changes Detected",
                text: "No attendance data has been updated."
            });
            return;
        }

        let payload = {
            AttendanceList: attendanceList,
            ActionBy: Number(sessionStorage.getItem("userId")) ?? 0
        };

        console.log("Payload Sending:", payload);

        $.ajax({
            url: "/api/StaffAttendance/SaveStaffAttendance",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                handleApiSuccess("Attendance saved successfully.");
                fetchAttendanceData();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Attendance not saved");
                // console.error(err);
                // Swal.fire("Error","Attendance not saved","error");
            }
        });
    });

    // $(document).on('click', '.btn-edit', function () {
    //     const canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';
    //     if (!canEdit) {
    //         Swal.fire("Access Denied", "You do not have permission to Edit.", "error");
    //         return;
    //     }

    //     const table = $('#tableAttendance table').DataTable();
    //     const rowData = table.row($(this).closest('tr')).data();
    //     resetAttendanceForm();

    //     $('#attendanceId').val(rowData.Id);
    //     $('#ddlStaff').val(rowData.StaffId);

    //     // Format date for <input type="date">
    //     if (rowData.AttendanceDate) {
    //         const date = new Date(rowData.AttendanceDate);
    //         const yyyy = date.getFullYear();
    //         const mm = String(date.getMonth() + 1).padStart(2, '0');
    //         const dd = String(date.getDate()).padStart(2, '0');
    //         $('#txtAttendanceDate').val(`${yyyy}-${mm}-${dd}`);
    //     }

    //     // Format time for <input type="time">
    //     if (rowData.CheckInTime) {
    //         const timeIn = new Date(rowData.CheckInTime);
    //         const hh = String(timeIn.getHours()).padStart(2, '0');
    //         const mi = String(timeIn.getMinutes()).padStart(2, '0');
    //         $('#txtCheckInTime').val(`${hh}:${mi}`);
    //     }

    //     if (rowData.CheckOutTime) {
    //         const timeOut = new Date(rowData.CheckOutTime);
    //         const hh = String(timeOut.getHours()).padStart(2, '0');
    //         const mi = String(timeOut.getMinutes()).padStart(2, '0');
    //         $('#txtCheckOutTime').val(`${hh}:${mi}`);
    //     }

    //     $('#ddlStatus').val(rowData.StatusId);

    //     $('#attendanceModalTitle').text("Edit Attendance");
    //     $('#attendanceModal').modal('show');
    // });

    // Delete button click
    $(document).on('click', '.btn-delete', function () {
        const canDelete = (sessionStorage.getItem('CanDelete') || localStorage.getItem('CanDelete')) === 'true';
        if (!canDelete) {
            Swal.fire("Access Denied", "You do not have permission to Delete.", "error");
            return;
        }
        let table = $('#tableAttendance  table').DataTable();
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
            url: "/api/StaffAttendance/DeleteStaffAttendance",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                if (typeof response === "string") {
                    response = JSON.parse(response);
                }

                if (response.status === "Success") {
                    handleApiSuccess("Leave Type Deleted!");
                } else {
                    Swal.fire("Delete Failed", response.message, "error");
                }
                $("#deleteRecordModal").modal("hide");
                fetchAttendanceData(); // reload updated list
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

// RESET FORM
function resetAttendanceForm() {
    $('#attendanceId').val(0);
    $('#ddlStaff').val('');
    $('#txtAttendanceDate').val('');
    $('#txtCheckInTime').val('');
    $('#txtCheckOutTime').val('');
    $('#ddlStatus').val('');
}

let statusList = [];

// ----------------------
// Load Staff Dropdown
// ----------------------
function loadStaffDropdown() {
    $.ajax({
        url: '/api/StaffMaster/GetStaffMaster',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0, StatusType: "Attendance" }),
        success: function (res) {
            const staff = res?.data || [];
            $('#ddlStaff').append(staff.map(x => `<option value="${x.Id}">${x.Name}</option>`));
        }
    });
}

// ----------------------
// Load Status Dropdown
// ----------------------
function loadStatusDropdown() {
    return new Promise((resolve) => {
        $.ajax({
            url: '/api/StatusMaster/GetStatusMaster',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Id: 0, StatusType: "Attendance" }),
            success: function (res) {
                statusList = res?.data || [];
                $('#ddlStatus').append(statusList.map(s => `<option value="${s.Id}">${s.StatusName}</option>`));
                resolve();
            },
            error: function () {
                statusList = [];
                resolve();
            }
        });
    });
}

function getPresentStatusId() {
    let present = statusList.find(s => s.StatusName.toLowerCase() === "present");
    return present ? present.Id : 0;
}

// ----------------------
// Fetch Attendance Data
// ----------------------
function fetchAttendanceData() {
    let staffId = Number($("#ddlStaff").val()) || 0;
    let statusId = Number($("#ddlStatus").val()) || 0;
    let fromDate = $("#txtfromdate").val();
    let toDate = $("#txttodate").val();

    $.ajax({
        url: "/api/StaffAttendance/GetStaffAttendance",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            StaffId: staffId,
            StatusId: statusId,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {
            console.log("Attendance Data:", res);
            renderAttendanceTable(res?.data || []);
        }
    });
}

// ----------------------
// Render DataTable
// ----------------------
function renderAttendanceTable(data) {
    if ($.fn.dataTable.isDataTable('#tableAttendance table')) {
        $('#tableAttendance table').DataTable().clear().destroy();
    }

    data.forEach((x, i) => x.srno = i + 1);

    $('#tableAttendance table').DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "StaffName", title: "Staff" },
            {
                title: "Date",
                data: null,
                render: function (data, type, row) {
                    let date = row.AttendanceDateFormat || "";
                    let day = row.DayName ? ` (${row.DayName})` : "";
                    return date + day;
                }
            },
            {
                data: "CheckInTime",
                title: "Check In",
                render: function (d, type, row) {
                    return `
                            <input type="text"
                                   class="form-control checkin-time timepicker"
                                   data-id="${row.AttendanceId}"
                                   value="${d || ''}">
                            <input type="hidden"
                                   class="checkin-time-hidden"
                                   value="${d || ''}">
                        `;
                }
            },
            {
                data: "CheckOutTime",
                title: "Check Out",
                render: function (d, type, row) {
                    return `
                            <input type="text"
                                   class="form-control checkout-time timepicker"
                                   data-id="${row.AttendanceId}"
                                   value="${d || ''}">
                            <input type="hidden"
                                   class="checkout-time-hidden"
                                   value="${d || ''}">
                        `;
                }
            },
            {
                data: "StatusId",
                title: "Status",
                render: function (d, type, row) {

                    let options = `<option value="0">-- Select Status --</option>`;
                    options += statusList
                        .map(s => `<option value="${s.Id}" ${s.Id == d ? "selected" : ""}>${s.StatusName}</option>`)
                        .join('');

                    return `
                            <div class="d-flex gap-1 align-items-center">
                                <select class="form-select duty-status"
                                        data-id="${row.AttendanceId}">
                                    ${options}
                                </select>

                                <button 
                                    type="button" 
                                    class="btn btn-outline-warning btn-action-xss clear-time" 
                                    title="Clear Check In, Check Out & Status">
                                    <i class="fa-solid fa-rotate-left"></i>
                               </button>
                            </div>
                        `;
                }
            }
        ],
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },
    });
}

function initTimePickers() {
    flatpickr(".timepicker", {
        enableTime: true,
        noCalendar: true,
        dateFormat: "h:i K",   // UI → 03:14 PM
        time_24hr: false,
        minuteIncrement: 1,
        onChange: function (selectedDates, dateStr, instance) {
            let row = $(instance.input).closest("tr");

            // Convert to 24-hour for hidden field
            let time24 = instance.formatDate(selectedDates[0], "H:i");

            if ($(instance.input).hasClass("checkin-time")) {
                row.find(".checkin-time-hidden").val(time24);
            } else {
                row.find(".checkout-time-hidden").val(time24);
            }
        }
    });
}
$('#tableAttendance table').on('draw.dt', function () {
    initTimePickers();
});

$(document).on("click", ".clear-time", function () {

    let row = $(this).closest("tr");

    // Clear visible inputs
    row.find(".checkin-time").val("");
    row.find(".checkout-time").val("");

    // Clear hidden (24-hr) values
    row.find(".checkin-time-hidden").val("");
    row.find(".checkout-time-hidden").val("");

    row.find(".duty-status").val("0");
});
