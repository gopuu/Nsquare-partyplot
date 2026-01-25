$(document).ready(function () {
    loadEventTypeDropdown();
    loadCompanyHallDropdown();
    ViewBookingReport("viewPDF");
    setCurrentMonthDates();
    //let today = new Date().toISOString().split('T')[0];
    //$("#to-date").val(today);
    //$("#from-date").val(today);

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
    ViewBookingReport("viewPDF");

});
$("#ddlEventSubType").on("change", function () {
    ViewBookingReport("viewPDF");
});
$("#ddlCompanyHall").on("change", function () {
    ViewBookingReport("viewPDF");
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
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
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
        if (!result.data || result.data.HasData === false) {
            document.getElementById("pdfPreviewContainer").innerHTML = `
            <div class="text-center text-muted py-5">
                <i class="bi bi-info-circle fs-1"></i><br/>
                <b>No data found</b>
            </div>`;
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

    let newFilter = $(this).data("value");

    // agar same filter, kuch mat karo
    if (selectedMonthDuration === newFilter) return;

    selectedMonthDuration = newFilter;

    // dropdown active class manage
    $(".sort-option").removeClass("active");
    $(this).addClass("active");

    $("#sortByDropdown").text("Filter : " + $(this).text());

    // --------------------------
    // MonthDuration ke hisab se From/To date set karna
    // --------------------------
    let today = new Date();
    let fromDate, toDate;

    if (selectedMonthDuration === "thismonth") {
        fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
        toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    } else if (selectedMonthDuration === "lastmonth") {
        fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        toDate = new Date(today.getFullYear(), today.getMonth(), 0);
    } else if (selectedMonthDuration === "days") { // Last 7 Days
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 6);
        toDate = today;
    } else if (selectedMonthDuration === "lastdays") { // Last 30 Days
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 29);
        toDate = today;
    } else if (selectedMonthDuration === "yesterday") {
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 1);
        toDate = fromDate;
    } else if (selectedMonthDuration === "12months") {
        fromDate = new Date(today);
        fromDate.setMonth(today.getMonth() - 12);
        toDate = today;
    } else if (selectedMonthDuration === "customrange") {
        // custom range me datepicker pe user select karega
        return ViewBookingReport("viewPDF");

    }

    // Date format yyyy-mm-dd
    function formatDate(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
    }

    // Set the datepickers
    $("#fromDate").val(formatDate(fromDate));
    $("#toDate").val(formatDate(toDate));

    // 🔥 Load data
    ViewBookingReport("viewPDF");

});


$("#fromDate, #toDate").on("change", function () {

    let fromDate = $("#fromDate").val();
    let toDate = $("#toDate").val();

    if (fromDate && toDate) {
        selectedMonthDuration = "customrange";

        $("#sortByDropdown").text("Filter : Custom Range");
        $(".sort-option").removeClass("active");

        ViewBookingReport("viewPDF");
    }
});
function setCurrentMonthDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#fromDate").val(formatDate(firstDay));
    $("#toDate").val(formatDate(lastDay));
}


