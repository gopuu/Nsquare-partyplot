$(document).ready(function () {
    loadEventTypeDropdown();
    loadCompanyHallDropdown();
    let today = new Date().toISOString().split('T')[0];
    $("#event-date").val(today);

});

function DownloadBookingReport() {

    const btn = event?.target;
    if (btn) btn.disabled = true;

    fetch('/api/Report/DownloadBookingReport', {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        }
    })
        .then(res => res.json())
        .then(res => {

            if (res.status === "Success" && res.data?.FileUrl) {

                const link = document.createElement('a');
                link.href = res.data.FileUrl;
                link.target = "_blank";
                link.download = '';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

            } else {
                alert("PDF not generated");
            }
        })
        .catch(err => {
            console.error(err);
            alert("Error while downloading PDF");
        })
        .finally(() => {
            if (btn) btn.disabled = false;
        });
}


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



