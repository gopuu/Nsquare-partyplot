$(document).ready(function () {
    fetchData();
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
            }

        ],

        stateSave: false,
        pageLength: 10,
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