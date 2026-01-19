
$(document).ready(function () {
    loadEventTypeDropdown();
    ViewRevenueLossReport("viewPDF");

    let today = new Date().toISOString().split('T')[0];
    $("#to-date").val(today);
    $("#from-date").val(today);
});


function ViewRevenueLossReport(actionName = "viewPDF") {
    $('#btn-loader').show();

    const url = (actionName === "downloadExcel")
        ? "/api/Report/Report_RevenueLoss_Excel"
        : "/api/Report/Report_RevenueLoss";

    fetch(url, {
        method: "POST",
        headers: { "Accept": "application/json" }
    })
        .then(async response => {
            if (!response.ok)
                throw new Error("Server error : " + response.status);

            const text = await response.text();
            if (!text) throw new Error("Empty response");

            return JSON.parse(text);
        })
        .then(result => {
            $('#btn-loader').hide();

            if (result.status !== "Success" || !result.data?.FileUrl) {
                Swal.fire("Download Fail", result.message, "error");
                return;
            }

            const fileUrl = result.data.FileUrl;

            if (actionName === "viewPDF") {
                document.getElementById("pdfPreviewContainer").innerHTML = `
                    <iframe src="${fileUrl}"
                            style="width:100%;height:100%;border:none;">
                    </iframe>`;
            } else {
                const link = document.createElement("a");
                link.href = fileUrl;
                link.download =
                    actionName === "downloadExcel"
                        ? "RevenueLoss_Report.xlsx"
                        : "RevenueLoss_Report.pdf";

                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        })
        .catch(error => {
            $('#btn-loader').hide();
            Swal.fire("Fail", error.message, "error");
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
