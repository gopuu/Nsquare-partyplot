$(document).ready(function () {
    setCurrentTime("#DutyStartTime");
    setCurrentTime("#DutyEndTime");
    loadStaffDropdown();
    resetDutyForm();
    loadDutyRoleDropdown();
    loadStatusDropdown();
    loadBookingDropdown();
    fetchEventDutyAssignments();

    $(document).on("click", "#btn-add", function () {
        const canAdd = (sessionStorage.getItem("CanAdd") || localStorage.getItem("CanAdd")) === "true";
        if (!canAdd) {
            Swal.fire("Access Denied", "You do not have permission to Add.", "error");
            return;
        }
        resetDutyForm();
        // loadDutyDropdowns();
        $("#DutyModalTitle").text("Assign Duty");
        $("#DutyModal").modal("show");
    });

    $("#btnSaveDuty").on("click", function () {
        let dutyId = parseInt($("#DutyId").val()) || 0;
        // --- VALIDATION ---
        if (!$("#ddlBookingId").val()) {
            toastr.error("Please select event");
            return;
        }
        if (!$("#ddlStaff").val()) {
            toastr.error("Please select staff");
            return;
        }
        if (!$("#ddlDutyRole").val()) {
            toastr.error("Please select duty role");
            return;
        }
        let payload = {
            Id: dutyId,
            BookingId: $("#ddlBookingId").val(),
            StaffId: $("#ddlStaff").val(),
            DutyRoleId: $("#ddlDutyRole").val(),
            StatusId: $("#ddlStatus").val() || null,
            DutyStartTime: $("#DutyStartTime").val() || null,
            DutyEndTime: $("#DutyEndTime").val() || null,
            Remarks: $("#Remarks").val() || null,
            ActionBy: localStorage.getItem("UserId") || 0
        };
        $("#btnSaveDuty").prop("disabled", true).text("Saving...");
        $.ajax({
            url: "/api/EventDutyAssignment/SaveEventDutyAssignment",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (response) {
                console.log("save response:", response);
                if (response.status === "Success") {
                    Swal.fire({
                        position: "top-end",
                        icon: "success",
                        title: "Saved!",
                        showConfirmButton: false,
                        timer: 900
                    });
                    $("#DutyModal").modal("hide");
                    fetchEventDutyAssignments();
                }
                else {
                    Swal.fire({
                        title: "EventDutyAssignment Save Failed",
                        text: response.message,
                        icon: "error"
                    });
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save Duty failed");
            },
            complete: function () {
                $("#btnSaveDuty").prop("disabled", false).text("Save");
            }
        });
    });

    $(document).on("click", ".btn-edit", function () {
        const table = $("#tableMain table").DataTable();
        const row = table.row($(this).closest("tr")).data();

        resetDutyForm(); // optional function to clear/reset form

        // Hidden Id
        $("#DutyId").val(row.Id);

        // Set dropdowns
        loadDutyDropdowns({
            BookingId: row.BookingId,
            StaffId: row.StaffId,
            DutyRoleId: row.DutyRoleId,
            StatusId: row.StatusId
        });

        // Duty Start Time
        if (row.DutyStartTime) {
            $("#DutyStartTime").val(row.DutyStartTime.substring(0, 5)); // HH:mm
        }

        // Duty End Time
        if (row.DutyEndTime) {
            $("#DutyEndTime").val(row.DutyEndTime.substring(0, 5)); // HH:mm
        }

        // Remarks
        $("#Remarks").val(row.Remarks);

        // Modal title
        $("#DutyModalTitle").text("Edit Duty Assignment");

        // Show modal
        $("#DutyModal").modal("show");
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
            url: "/api/EventDutyAssignment/DeleteEventDutyAssignment",
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
                fetchEventDutyAssignments(); // reload updated list
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete  fail");
            },
            complete: function () {
                $('#btn-loader').hide();
            }
        });
    });

});

function loadDutyDropdowns(data) {
    if (data.BookingId) $("#ddlBookingId").val(data.BookingId).trigger("change");
    if (data.StaffId) $("#ddlStaff").val(data.StaffId).trigger("change");
    if (data.DutyRoleId) $("#ddlDutyRole").val(data.DutyRoleId).trigger("change");
    if (data.StatusId) $("#ddlStatus").val(data.StatusId).trigger("change");
}

function loadBookingDropdown() {

    let bookingId = 0; //$("#SomeBookingIdField").val() || 0;
    let banquetId = 0; //$("#SomeBanquetIdField").val() || 0;
    let eventDate = null; //$("#SomeEventDateField").val() || null;

    var requestData = {
        BookingId: bookingId,
        BanquetId: banquetId,
        EventDate: eventDate
    };

    $.ajax({
        url: '/api/BookingCalendar/GetBookingCalendar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),

        success: function (response) {

            if (response.status === "Success") {

                var list = response.data || [];

                let html = `<option value="">-- Select Booking --</option>`;

                list.forEach(x => {
                    let text = `${x.Client_Name} - ${x.Booking_SubEventName} (${x.BanquetName})`;
                    html += `<option value="${x.BookingId}">${text}</option>`;
                });

                $("#ddlBookingId").html(html);
            }
            else {
                Swal.fire({
                    icon: 'warning',
                    title: 'No Data Found',
                    text: response.message || 'No bookings available.'
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "getBooking fetch fail");
        }
    });
}

function resetDutyForm() {
    $("#DutyId").val(0);

    $("#ddlStaff").val("");
    $("#ddlDutyRole").val("");
    $("#ddlStatus").val("");

    $("#DutyStartTime").val("");
    $("#DutyEndTime").val("");
    $("#Remarks").val("");
}

function loadStaffDropdown() {
    $.ajax({
        url: '/api/StaffMaster/GetStaffMaster',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0, StatusType: "Attendance" }),
        success: function (res) {
            const staff = res?.data || [];

            let html = `<option value="">-- Select Staff --</option>`;
            staff.forEach(x => {
                html += `<option value="${x.Id}">${x.Name}</option>`;
            });

            $('#ddlStaff').html(html);
        }
    });
}

function loadDutyRoleDropdown() {
    $.ajax({
        url: '/api/DutyRoleMaster/GetDutyRoleMaster',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0, StatusType: "Attendance" }),
        success: function (res) {
            const roles = res?.data || [];

            let html = `<option value="">-- Select Duty Role --</option>`;
            roles.forEach(x => {
                html += `<option value="${x.Id}">${x.RoleName}</option>`;
            });

            $('#ddlDutyRole').html(html);
        }
    });
}

function loadStatusDropdown() {
    $.ajax({
        url: '/api/StatusMaster/GetStatusMaster',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0, StatusType: "Attendance" }),
        success: function (res) {
            const status = res?.data || [];

            let html = `<option value="">-- Select Status --</option>`;
            status.forEach(x => {
                html += `<option value="${x.Id}">${x.StatusName}</option>`;
            });

            $('#ddlStatus').html(html);
        }
    });
}

function setCurrentTime(selector) {
    const now = new Date();
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');

    $(selector).val(`${hours}:${minutes}`);
}

function fetchEventDutyAssignments() {
    let payload = {
        Id: 0,

        StaffId: 0
    };
    $.ajax({
        url: "/api/EventDutyAssignment/GetEventDutyAssignment",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            renderDutyTable(res.data ?? []);
        },
        error: function (err) {
            handleApiError(err);
        }
    });
}

function renderDutyTable(data) {
    if ($.fn.DataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }
    data.forEach((x, i) => x.srno = i + 1);
    $("#tableMain table").DataTable({
        data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "EventName", title: "Event" },
            { data: "StaffName", title: "Staff" },
            { data: "RoleName", title: "Duty Role" },
            {
                data: "DutyStartTime",
                title: "Start Time",
                render: d => formatTime12Hour(d)
            },
            {
                data: "DutyEndTime",
                title: "End Time",
                render: d => formatTime12Hour(d)
            },
            { data: "Remarks", title: "Remarks" },
            { data: "StatusName", title: "Status" },
            {
                data: "Id",
                title: "Actions",
                render: function (id, type, row) {
                    return `
                            <button class="btn btn-outline-warning btn-sm btn-edit btn-action-xss" data-id="${id}">
                                <i class="fa fa-edit"></i>
                            </button>

                            <button class="btn btn-outline-danger btn-sm btn-delete btn-action-xss" data-id="${id}">
                                <i class="fa fa-trash"></i>
                            </button>`;
                }
            }
        ]
    });
}

function formatTime12Hour(timeString) {
    if (!timeString) return "";

    // Split hours, minutes, seconds
    let [hour, minute] = timeString.split(":");

    hour = parseInt(hour, 10);
    let ampm = hour >= 12 ? "PM" : "AM";

    hour = hour % 12;
    if (hour === 0) hour = 12; // 0 => 12 AM/PM

    return `${hour}:${minute} ${ampm}`;
}

