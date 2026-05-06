$(document).ready(function () {
    var feedbackId = 0;
    fetchFeedbackData(feedbackId);
});

function fetchFeedbackData(feedbackId) {
    $.ajax({
        url: '/api/Feedback/GetFeedback',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ FeedbackId: feedbackId }),
        success: function (res) {
            console.log("res Data:", res);
            if (res && res.status === "Success") {

                let tableData = res.data;   // 👈 FULL DATA OBJECT

                console.log("Table Data:", tableData);
                renderFeedbackTable(tableData);

            } else {
                Swal.fire("Error", res.message || "No data", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Feedback Error");
        }
    });
}
function renderFeedbackTable(data) {

    // ✅ ensure array
    // if (!Array.isArray(data)) {
    //     if (data && data.data && Array.isArray(data.data)) {
    //         data = data.data;
    //     } else {
    //         data = [];
    //     }
    // }

    if ($.fn.dataTable.isDataTable('#tableMain table')) {
        $('#tableMain table').DataTable().clear().destroy();
    }

    if (data.length === 0) {
        $('#tableMain table').DataTable({
            data: [],
            columns: [
                { title: '#' },
                { title: 'Customer Name' },
                { title: 'Whatsapp No' },
                { title: 'Booking SubEvent Name' },
                { title: 'Text Response' },
                { title: 'Created At' }
            ],
            initComplete: function () {
                $(this.api().table().header())
                    .find('th')
                    .addClass('bg-warning-subtle text-dark');
            },

        });
        return;
    }

    // ✅ safe now
    data.forEach((x, i) => x.srno = i + 1);

    $('#tableMain table').DataTable({
        data: data,
        paging: false,          // remove pagination
        searching: false,       // optional
        info: false,            // optional
        ordering: false,        // optional
        scrollY: '75vh',       // fixed height
        scrollCollapse: false, // keep fixed height even with 0/1 rows
        autoWidth: false,
        columns: [
            { data: 'srno', title: '#', className: 'text-center' },
            { data: 'CustomerName', title: 'Customer' },
            { data: 'WhatsappNo', title: 'Whatsapp No' },
            { data: 'Booking_SubEventName', title: 'Booking Event' },
            //{
            //    data: 'Responses',
            //    title: 'Text Response',
            //    render: function (d) {
            //        if (!d || d.length === 0) return '-';

            //        return d.map(r => r.TextResponse || '')
            //            .filter(x => x)
            //            .join('<br>');
            //    }
            //},
            {
                data: 'Created_At_Format',
                title: 'Created At'
            }
        ],
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },

    });
}
