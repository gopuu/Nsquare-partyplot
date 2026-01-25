
$(document).ready(function () {
    loadEventTypeDropdown();
    ViewRevenueLossReport("viewPDF");
    setCurrentMonthDates();
    
});


function ViewRevenueLossReport(actionName = "viewPDF") {
    $('#btn-loader').show();

    const url = (actionName === "downloadExcel")
        ? "/api/Report/Report_RevenueLoss_Excel"
        : "/api/Report/Report_RevenueLoss";

    fetch(url, {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            FromDate: $("#fromDate").val(),
            ToDate: $("#toDate").val(),
            FilterDuration: selectedMonthDuration
        })
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
        return ViewRevenueLossReport("viewPDF");

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
    ViewRevenueLossReport("viewPDF");

});


$("#fromDate, #toDate").on("change", function () {

    let fromDate = $("#fromDate").val();
    let toDate = $("#toDate").val();

    if (fromDate && toDate) {
        selectedMonthDuration = "customrange";

        $("#sortByDropdown").text("Filter : Custom Range");
        $(".sort-option").removeClass("active");

        ViewRevenueLossReport("viewPDF");
    }
});
function setCurrentMonthDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#fromDate").val(formatDate(firstDay));
    $("#toDate").val(formatDate(lastDay));
}

function formatDate(date) {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
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
    ViewRevenueLossReport("viewPDF");

});
$("#ddlEventSubType").on("change", function () {
    ViewRevenueLossReport("viewPDF");
});
