let selectedMonthDuration = "thismonth";

$(document).ready(function () {
    loadEventTypeDropdown();
    loadCompanyHallDropdown();
    ViewBookingReport();
    setCurrentMonthDates();

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

        // Set the datepickers
        $("#fromDate").val(formatDate(fromDate));
        $("#toDate").val(formatDate(toDate));

        // 🔥 Load data
        ViewBookingReport("viewPDF");

    });

    // Open / Toggle filter panel
    $("#btnOpenFilter").on("click", function (e) {
        e.stopPropagation();
        $("#filterPanel").toggleClass("d-none");
    });

    // Close button
    $("#btnCloseFilter").on("click", function () {
        $("#filterPanel").addClass("d-none");
    });

    // Prevent inside click from bubbling
    $("#filterPanel").on("click", function (e) {
        e.stopPropagation();
    });

    // Close on outside click
    $(document).on("click", function () {
        $("#filterPanel").addClass("d-none");
    });

    // Apply Filter
    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").addClass("d-none");
        ViewBookingReport();
    });

    // Reset Filter
    $("#btnResetFilter").on("click", function () {

        $("#ddlEventType").val("");
        $("#ddlEventSubType").val("").prop("disabled", true);
        $("#ddlCompanyHall").val("");
        $("#fromDate").val("");
        $("#toDate").val("");
        $("#quickRange").val("");

        ViewBookingReport();
    });
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

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        data: JSON.stringify(inputData),
        success: function (result) {

            if (result.status !== "Success" || !result.data || !result.data.FileUrl) {
                Swal.fire({
                    title: "Fail",
                    text: result.message || "No report URL returned",
                    icon: "error"
                });
                return;
            }

            if (result.data.HasData === false) {
                $("#pdfPreviewContainer").html(`
                    <div class="text-center text-muted py-5">
                        <i class="bi bi-info-circle fs-1"></i><br/>
                        <b>No data found</b>
                    </div>
                `);
                return;
            }

            var fileUrl = result.data.FileUrl;

            if (actionName === "viewPDF") {
                $("#pdfPreviewContainer").html(`<iframe src="${fileUrl}" style="width:100%;height:100%;border:none;"></iframe>`);
            }
            else {

                var link = document.createElement("a");
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
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching Booking report")
        }
    });


}

// Date format yyyy-mm-dd
function formatDate(date) {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
}

function setCurrentMonthDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#fromDate").val(formatDate(firstDay));
    $("#toDate").val(formatDate(lastDay));
}

