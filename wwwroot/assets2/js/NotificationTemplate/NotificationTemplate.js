$(document).ready(function () {
    fetchData();
    fillTimeDropdown("#Reminder_TriggerTime");

    // ============================================
    // TOGGLE SEND MSG
    // ============================================
    $(document).on('change', '.toggle-sendmsg', function () {
        let eventId = $(this).data('eventid');
        let isSendMsg = $(this).prop('checked');
        updateNotificationStatus(eventId, isSendMsg);
    });
});
// ============================================
// FETCH DATA
// ============================================
function fetchData() {
    let payload = {
        EventId: 0
    };
    console.log("payload:", payload);
    $.ajax({
        url: "/api/NotificationTemplate/GetNotification_TemplateParameterList",
        type: "POST",
        data: JSON.stringify(payload),
        contentType: "application/json",
        success: function (response) {
            console.log("response:", response);
            let data = response.data || [];
            renderTable(data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Notification Template Fetch Error");
        }
    });
}
// ============================================
// RENDER TABLE
// ============================================
function renderTable(data) {
    console.log("data:", data);
    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    if (!data || data.length === 0) {
        $('#tableMain tbody').html(`
            <tr>
                <td colspan="6" class="text-center fw-bold text-danger">
                    No Data Found !!
                </td>
            </tr>
        `);
        return;
    }

    data.forEach((item, index) => {
        item.srno = index + 1;
    });

    $('#tableMain table').DataTable({

        data: data,
        columns: [
            {
                data: 'srno',
                title: 'Sr. No.'
            },

            {
                data: 'TempName_Title',
                title: 'Template Title'
            },

            {
                data: 'PTFlag',
                title: 'Type',
                render: function (data) {
                    if (data === 'T') {
                        return `<span class="badge bg-success">Transactional</span>`;
                    }
                    if (data === 'P') {
                        return `<span class="badge bg-warning text-dark">Promotional</span>`;
                    }
                    return `<span class="badge bg-secondary">${data || '-'}</span>`;
                }
            },

            {
                data: 'EventId',
                title: 'Event Id'
            },

            {
                data: 'IsSendMsg',
                title: 'Send Message',
                className: 'text-center',
                render: function (data, type, row) {
                    return `
                        <div class="form-check form-switch d-flex justify-content-center">
                            <input class="form-check-input toggle-sendmsg"
                                   type="checkbox"
                                   role="switch"
                                   data-eventid="${row.EventId}"
                                   ${data ? 'checked' : ''}>
                        </div>
                    `;
                }
            },
            // ===========================================
            // MANAGE REMINDERS
            // ===========================================
            {
                data: null,
                title: 'Reminders',
                orderable: false,
                className: 'text-center',
                render: function (data, type, row) {
                    if (!row.HasReminders)
                        return '-';
                    return `
                        <button class="btn btn-sm btn-outline-warning btn-manage-reminder"
                                data-eventid="${row.EventId}"
                                data-title="${row.TempName_Title}">
                            <i class="fa fa-bell me-1"></i>
                            Manage
                        </button>
                    `;
                }
            }

        ],

        stateSave: false,
        pageLength: 100,
        ordering: true,
        searching: true,
        lengthChange: false,
        pagingType: "full_numbers"
    });

}

// ============================================
// UPDATE STATUS
// ============================================

function updateNotificationStatus(eventId, isSendMsg) {
    let payload = {
        EventId: eventId,
        IsSendMsg: isSendMsg
    };
    console.log("payload:", payload);

    $.ajax({
        url: "/api/NotificationTemplate/SaveNotification_TemplateParameterList",
        type: "POST",
        data: JSON.stringify(payload),
        contentType: "application/json",
        success: function (response) {
            console.log("save response:", response);
            if (response.status.toLowerCase() === "success") {
                handleApiSuccess("Notification status updated");
            }
            else {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: response.message
                });
                fetchData();
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Update Notification Status Failed");
            fetchData();
        }
    });

}

// =======================================================
// GLOBALS
// =======================================================
let currentReminderEventId = 0;
let currentReminderEventTitle = "";

// =======================================================
// OPEN REMINDER MODAL
// =======================================================
$(document).on('click', '.btn-manage-reminder', function () {
    currentReminderEventId = Number($(this).data('eventid'));
    currentReminderEventTitle = $(this).data('title');
    $('#reminderModalTitle').html(`Manage Reminders<span class="text-primary">(${currentReminderEventTitle})</span>`);
    resetReminderForm();
    fetchReminderSettings(currentReminderEventId);
    $('#reminderModal').modal('show');
});

// =======================================================
// FETCH REMINDER SETTINGS
// =======================================================
function fetchReminderSettings(eventId) {
    let payload = {
        EventId: eventId
    };

    $.ajax({
        url: "/api/NotificationTemplate/GetNotification_EventReminderSettingsList",
        type: "POST",
        data: JSON.stringify(payload),
        contentType: "application/json",
        success: function (response) {
            renderReminderTable(response.data || []);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Reminder Fetch Error");
        }
    });
}

// =======================================================
// RENDER REMINDER GRID
// =======================================================
function renderReminderTable(data) {
    if ($.fn.dataTable.isDataTable('#tableReminder')) {
        $('#tableReminder').DataTable().clear().destroy();
    }
    data.forEach((item, index) => {
        item.srno = index + 1;
    });

    $('#tableReminder').DataTable({
        data: data,
        columns: [
            {
                data: 'srno',
                title: '#',
                className: "text-center",
            },
            {
                data: 'Title',
                title: 'Title'
            },
            {
                data: 'TriggerDays',
                title: 'Days',
                className: "text-center",
            },
            {
                data: 'TriggerTime_Format',
                title: 'Time',
                className: "text-center",
            },
            {
                data: 'IsEnabled',
                title: 'Enabled',
                className: "text-center",
                render: function (data) {
                    return data
                        ? `<span class="badge bg-success">Yes</span>`
                        : `<span class="badge bg-danger">No</span>`;
                }
            },
            {
                data: 'LastModifiedOn',
                title: 'Modified',
                className: "text-center",
                width: '100px'
            },
            {
                data: null,
                title: 'Action',
                orderable: false,
                className: "text-center",
                render: function (data, type, row) {
                    return `
                        <button class="btn btn-xs btn-outline-warning btn-edit-reminder"
                                data-item='${JSON.stringify(row)}'>
                            <i class="fa fa-edit"></i>
                        </button>
                    `;
                }
            }

        ],

        pageLength: 50,
        ordering: false,
        searching: false,
        lengthChange: false
    });
}

// =======================================================
// RESET FORM
// =======================================================
function resetReminderForm() {
    $('#Reminder_Id').val(0);
    $('#Reminder_Title').val('');
    $('#Reminder_TriggerDays').val('');
    $('#Reminder_IsEnabled').prop('checked', true);
    $('#reminderModalTitle').text('Add Reminder');

    // Reset Time (Previous 5-Minute Slot)
    //------------------------------------------
    const now = new Date();
    // Round DOWN to nearest 5 minutes
    const roundedMinutes = Math.floor(now.getMinutes() / 5) * 5;

    now.setMinutes(roundedMinutes);
    now.setSeconds(0);
    now.setMilliseconds(0);

    // 24-Hour Format Value
    const hh = now.getHours().toString().padStart(2, '0');
    const mm = now.getMinutes().toString().padStart(2, '0');
    const time24 = `${hh}:${mm}`;
    $('#Reminder_TriggerTime').val(time24).removeClass('is-invalid');
}

// =======================================================
// EDIT REMINDER
// =======================================================
$(document).on('click', '.btn-edit-reminder', function () {
    let item = $(this).data('item');
    $('#Reminder_Id').val(item.Id);
    $('#Reminder_Title').val(item.Title);
    $('#Reminder_TriggerDays').val(item.TriggerDays);
    $('#Reminder_TriggerTime').val(item.TriggerTime);
    $('#Reminder_IsEnabled').prop('checked', item.IsEnabled);
    $('#reminderModalTitle').text('Edit Reminder');
});

// =======================================================
// SAVE REMINDER
// =======================================================
$('#btnSaveReminder').click(function () {

    // ==========================================
    // VALIDATE TITLE
    // ==========================================
    let title = $('#Reminder_Title').val().trim();
    let alphaNumericRegex = /^[a-zA-Z0-9\s]+$/;
    if (title === '') {
        Swal.fire({
            icon: 'warning',
            title: 'Validation',
            text: 'Reminder title is required'
        });
        $('#Reminder_Title').focus();
        return;
    }

    if (!alphaNumericRegex.test(title)) {
        Swal.fire({
            icon: 'warning',
            title: 'Validation',
            text: 'Only alphabets & numbers characters allowed'
        });
        $('#Reminder_Title').focus();
        return;
    }

    let payload = {
        Id: Number($('#Reminder_Id').val()),
        Title: title, //$('#Reminder_Title').val(),
        TriggerDays: Number($('#Reminder_TriggerDays').val()),
        TriggerTime: $('#Reminder_TriggerTime').val(),
        IsEnabled: $('#Reminder_IsEnabled').is(':checked'),
        EventId: currentReminderEventId,
        ActionBy: Number(sessionStorage.getItem('userId'))
    };

    $.ajax({
        url: "/api/NotificationTemplate/SaveNotification_EventReminderSettings",
        type: "POST",
        data: JSON.stringify(payload),
        contentType: "application/json",
        success: function (response) {
            if (response.status.toLowerCase() === "success") {
                handleApiSuccess("Reminder saved successfully");
                resetReminderForm();
                fetchReminderSettings(currentReminderEventId);
            }
            else {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: response.message
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Save Reminder Failed");
        }
    });
});


