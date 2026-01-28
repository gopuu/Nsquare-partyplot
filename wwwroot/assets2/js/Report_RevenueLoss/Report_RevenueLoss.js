let selectedMonthDuration = "thismonth";
const HEADER_KEY = "printWithHeader";

$(document).ready(function () {

    loadEventTypeDropdown();
    setCurrentMonthDates();
    ViewRevenueLossReport("viewPDF");

    $(".sort-option").on("click", function () {

        $(".sort-option").removeClass("active");
        $(this).addClass("active");

        selectedMonthDuration = $(this).data("value");
        $("#sortByDropdown").text($(this).text());

        applyQuickRange();
        ViewRevenueLossReport("viewPDF");
    });

    $("#ddlEventType").on("change", function () {
        loadEventSubTypeDropdown($(this).val());
        ViewRevenueLossReport("viewPDF");
    });

    $("#ddlEventSubType").on("change", function () {
        ViewRevenueLossReport("viewPDF");
    });

    $("#btnApplyFilter").on("click", function () {
        ViewRevenueLossReport("viewPDF");
    });

    $("#btnResetFilter").on("click", function () {
        setCurrentMonthDates();
        $("#ddlEventType").val("");
        $("#ddlEventSubType").val("").prop("disabled", true);
        selectedMonthDuration = "thismonth";
        ViewRevenueLossReport("viewPDF");
    });

    $("#fromDate, #toDate").on("change", function () {
        selectedMonthDuration = "customrange";
        $("#sortByDropdown").text("Custom Range");
        $(".sort-option").removeClass("active");
        ViewRevenueLossReport("viewPDF");
    });

    // WITH HEADER -- START ------------
    // Restore value on page load
    const saved = sessionStorage.getItem(HEADER_KEY);
    if (saved !== null) {
        document.getElementById("toggleWithHeader").checked = (saved === "true");
    }

    // On toggle change
    document.getElementById("toggleWithHeader").addEventListener("change", function () {
        const isChecked = this.checked;
        sessionStorage.setItem(HEADER_KEY, isChecked);

        console.log("With Header:", isChecked); // 🔹 print value
        ViewRevenueLossReport();
    });
    // WITH HEADER -- END ------------

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

function ViewRevenueLossReport(actionName = "viewPDF") {
    $('#btn-loader').show();

    const withHeader = sessionStorage.getItem(HEADER_KEY) === "true";
    console.log("With Header:", withHeader);

    const url = (actionName === "downloadExcel")
        ? "/api/Report/Report_RevenueLoss_Excel"
        : "/api/Report/Report_RevenueLoss";

    fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            FromDate: $("#fromDate").val(),
            ToDate: $("#toDate").val(),
            FilterDuration: selectedMonthDuration,
            WithHeader: withHeader
        })
    })
        .then(r => r.json())
        .then(result => {

            if (result.status !== "Success" || !result.data?.FileUrl) {
                Swal.fire("Fail", result.message || "No data found", "error");
                return;
            }

            if (actionName === "viewPDF") {
                $("#pdfPreviewContainer").html(
                    `<iframe src="${result.data.FileUrl}" style="width:100%;height:100%;border:none;"></iframe>`
                );
            } else {
                const link = document.createElement("a");
                link.href = result.data.FileUrl;
                link.download = actionName === "downloadExcel"
                    ? "RevenueLoss_Report.xlsx"
                    : "RevenueLoss_Report.pdf";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        });
}

function setCurrentMonthDates() {
    const today = new Date();
    const first = new Date(today.getFullYear(), today.getMonth(), 1);
    const last = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#fromDate").val(formatDate(first));
    $("#toDate").val(formatDate(last));
}

function applyQuickRange() {
    const today = new Date();
    let from, to = today;

    switch (selectedMonthDuration) {
        case "thismonth":
            from = new Date(today.getFullYear(), today.getMonth(), 1);
            break;
        case "lastmonth":
            from = new Date(today.getFullYear(), today.getMonth() - 1, 1);
            to = new Date(today.getFullYear(), today.getMonth(), 0);
            break;
        case "days":
            from = new Date(today.setDate(today.getDate() - 6));
            break;
        case "lastdays":
            from = new Date(today.setDate(today.getDate() - 29));
            break;
        case "yesterday":
            from = to = new Date(today.setDate(today.getDate() - 1));
            break;
        default:
            return;
    }

    $("#fromDate").val(formatDate(from));
    $("#toDate").val(formatDate(to));
}

function formatDate(d) {
    return d.toISOString().split("T")[0];
}
