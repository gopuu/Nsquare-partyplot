$(document).ready(function () {
    loadEventTypeDropdown();
    loadCompanyHallDropdown();
    ViewBookingReport("viewPDF");

    let today = new Date().toISOString().split('T')[0];
    $("#to-date").val(today);
    $("#from-date").val(today);

});

function loadEventTypeDropdown(selectedId = "") {
    $.ajax({
        url: "/api/EventType/GetEventTypeListForDropdown",
        type: "GET",
        success: function (response) {

            let dropdown = $("#ddlEventType");
            dropdown.empty();

            dropdown.append(`<option value="">-- Event Type --</option>`);

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

function loadCompanyHallDropdown(selectedId = "") {

    $.ajax({
        url: "/api/Company_HallDetails/GetCompanyHallDetailsList_Dropdown",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: 0   // 0 or null = get all halls
        }),
        success: function (response) {

            let dropdown = $("#ddlCompanyHall");
            dropdown.empty();

            dropdown.append(`<option value="">-- Select Hall --</option>`);

            if (response.status === "Success" && response.data.length > 0) {

                response.data.forEach(item => {
                    dropdown.append(
                        `<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>
                            ${item.HallName} (${item.CapacityRangeText})
                        </option>`
                    );
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load hall list");
        }
    });
}


//function ViewBookingReport(actionName = "viewPDF") {
//    $('#btn-loader').show();
//    const url = '/api/Report/DownloadBookingReport';

//    fetch(url, {
//        method: 'POST',
//        headers: {
//            'Accept': 'application/json'
//        }
//    })
//    .then(async response => {

//        // 🔐 SAFETY CHECK
//        if (!response.ok) {
//            throw new Error("Server error : " + response.status);
//        }

//        const text = await response.text();

//        if (!text) {
//            throw new Error("Empty response from server");
//        }

//        return JSON.parse(text);
//    })
//    .then(result => {

//        $('#btn-loader').hide();

//        if (result.status !== 'Success' || !result.data || !result.data.FileUrl) {
//            Swal.fire({
//                title: "Download Fail",
//                text: result.message || "No report URL returned",
//                icon: "error"
//            });
//            return;
//        }

//        const fileUrl = result.data.FileUrl;

//        if (actionName === 'downloadPDF') {
//            const link = document.createElement('a');
//            link.href = fileUrl;
//            link.download = 'Booking_Report.pdf';
//            document.body.appendChild(link);
//            link.click();
//            document.body.removeChild(link);
//        } else {
//            document.getElementById('pdfPreviewContainer').innerHTML = `
//            <iframe
//                src="${fileUrl}"
//                style="width:100%;height:100%;border:none;">
//            </iframe>
//        `;
//        }
//    })
//    .catch(error => {

//        $('#btn-loader').hide();
//        console.error("Error fetching GST report:", error);

//        Swal.fire({
//            title: "Download Fail",
//            text: error.message,
//            icon: "error"
//        });
//    });
//}

// Common function to fetch report

function ViewBookingReport(actionName = "viewPDF") {
    $('#btn-loader').show();

    const inputData = {
        EventTypeId: parseInt($("#ddlEventType").val()) || 0,
        EventSubTypeId: parseInt($("#ddlEventSubType").val()) || 0,
        HallId: parseInt($("#ddlCompanyHall").val()) || 0,
        FromDate: $("#from-date").val(),
        ToDate: $("#to-date").val(),
        FilterDuration: selectedMonthDuration // thismonth / lastmonth / customrange
    };

    const url = (actionName === "downloadExcel")
        ? "/api/Report/DownloadBookingReport_Excel"
        : "/api/Report/DownloadBookingReport";

    fetch(url, {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: JSON.stringify(inputData)
    })
    .then(async response => {
        if (!response.ok) throw new Error("Server error: " + response.status);
        const text = await response.text();
        if (!text) throw new Error("Empty response from server");
        return JSON.parse(text);
    })
    .then(result => {
        $('#btn-loader').hide();

        if (result.status !== "Success" || !result.data?.FileUrl) {
            Swal.fire({
                title: "Fail",
                text: result.message || "No report URL returned",
                icon: "error"
            });
            return;
        }

        const fileUrl = result.data.FileUrl;

        if (actionName === "viewPDF") {
            document.getElementById("pdfPreviewContainer").innerHTML = `
            <iframe src="${fileUrl}" style="width:100%;height:100%;border:none;"></iframe>
        `;
        } else {
            const link = document.createElement("a");
            link.href = fileUrl;

            if (actionName === "downloadPDF") {
                link.download = `Booking_Report_${new Date().toISOString().replace(/[:.-]/g, "")}.pdf`;
            } else if (actionName === "downloadExcel") {
                link.download = `Booking_Report_${new Date().toISOString().replace(/[:.-]/g, "")}.xlsx`;
            }

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    })
    .catch(error => {
        $('#btn-loader').hide();
        console.error("Error fetching Booking report:", error);
        Swal.fire({
            title: "Fail",
            text: error.message,
            icon: "error"
        });
    });
}

let selectedMonthDuration = "thismonth";

$(".sort-option").on("click", function (e) {
    e.preventDefault();

    $(".sort-option").removeClass("active");
    $(this).addClass("active");
    selectedMonthDuration = $(this).data("value");

    $("#sortByDropdown").text("Filter : " + $(this).text());
});


