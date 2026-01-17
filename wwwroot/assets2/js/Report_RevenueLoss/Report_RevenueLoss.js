
$(document).ready(function () {
    loadEventTypeDropdown();
    ViewRevenueLossReport("viewPDF");

    let today = new Date().toISOString().split('T')[0];
    $("#to-date").val(today);
    $("#from-date").val(today);
});


function ViewRevenueLossReport(actionName = "viewPDF") {
    $('#btn-loader').show();
    const url = '/api/Report/Report_RevenueLoss';
    fetch(url, {
        method: 'POST',
        headers: {
            'Accept': 'application/json'
        }
    })
        .then(async response => {
            // 🔐 SAFETY CHECK
            if (!response.ok) {
                throw new Error("Server error : " + response.status);
            }
            const text = await response.text();

            if (!text) {
                throw new Error("Empty response from server");
            }
            return JSON.parse(text);
        })
        .then(result => {
            $('#btn-loader').hide();
            if (result.status !== 'Success' || !result.data || !result.data.FileUrl) {
                Swal.fire({
                    title: "Download Fail",
                    text: result.message || "No report URL returned",
                    icon: "error"
                });
                return;
            }
            const fileUrl = result.data.FileUrl;
            if (actionName === 'downloadPDF') {
                const link = document.createElement('a');
                link.href = fileUrl;
                link.download = 'RevenueLoss_Report.pdf';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            } else {
                document.getElementById('pdfPreviewContainer').innerHTML = `
        <iframe 
            src="${fileUrl}" 
            style="width:100%;height:100%;border:none;">
        </iframe>
    `;
            }
        })
        .catch(error => {
            $('#btn-loader').hide();
            console.error("Error fetching RevenueLoss report:", error);
            Swal.fire({
                title: "Download Fail",
                text: error.message,
                icon: "error"
            });
        });
}



//function DownloadRevenueLossReport() {

//    fetch('/api/Report/Report_RevenueLoss', {
//        method: 'GET',
//        headers: {
//            'Accept': 'application/json'
//        }
//    })
//        .then(res => res.json())
//        .then(res => {

//            if (res.status === "Success" && res.data.FileUrl) {

//                const link = document.createElement('a');
//                link.href = res.data.FileUrl;
//                link.download = '';
//                document.body.appendChild(link);
//                link.click();
//                document.body.removeChild(link);

//            } else {
//                alert("PDF Not generate");
//            }
//        })
//        .catch(err => {
//            console.error(err);
//            alert("Error while downloading PDF");
//        });
//}
function loadEventTypeDropdown(selectedId = "") {
    $.ajax({
        url: "/api/EventType/GetEventTypeListForDropdown",
        type: "GET",
        success: function (response) {

            let dropdown = $("#ddlEventType");
            dropdown.empty();

            dropdown.append(`<option value="">-- Select Event Type --</option>`);

            if (response.status === "Success" && response.data.length > 0) {
                response.data.forEach(item => {
                    dropdown.append(
                        `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                            ${item.Name}
                        </option>`
                    );
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load Event Type");
        }
    });
}
function loadEventSubTypeDropdown(eventTypeId, selectedId = "") {

    let dropdown = $("#ddlEventSubType");

    dropdown.prop("disabled", true);
    dropdown.empty();
    dropdown.append(`<option value="">-- Select Event Sub Type --</option>`);

    if (!eventTypeId) return;

    $.ajax({
        url: "/api/EventSubType/GetEventSubTypeListForDropdown",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            EventTypeId: eventTypeId
        }),
        success: function (response) {

            if (response.status === "Success" && response.data.length > 0) {

                response.data.forEach(item => {
                    dropdown.append(
                        `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                            ${item.Name}
                        </option>`
                    );
                });

                dropdown.prop("disabled", false);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load Event Sub Type");
        }
    });
}

$("#ddlEventType").on("change", function () {
    let eventTypeId = $(this).val();
    loadEventSubTypeDropdown(eventTypeId);
});
