const REPORT_COLUMNS = [
    { key: "EnquiryNo", label: "Enq. No.", visible: true },

    { key: "EnquiryDate", label: "Enq. Date", visible: true },

    { key: "EventDate", label: "Event Date", visible: true },

    { key: "ClientName", label: "Client Name", visible: true },

    { key: "EventType", label: "Event Type (Sub-Type)", visible: true },

    { key: "Hall", label: "Hall", visible: true },

    { key: "Pax", label: "Pax", visible: true },

    { key: "LeadSource", label: "Lead Source", visible: true },

    { key: "Amount", label: "Amount", visible: true },

    { key: "FollowUps", label: "Follow-ups", visible: true },

    { key: "Status", label: "Status", visible: true }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "printWithHeader";

document.addEventListener("DOMContentLoaded", function () {
    loadEventTypeDropdown();
    loadEventSubTypeDropdown(0);
    loadCompanyHallDropdown();
    loadLeadSourceList();
    loadProcessList();

    setCurrentMonthDates();
    loadEnquiryGrid();
    loadColumnCheckboxes();

    $("#ddlEventType").on("change", function () {
        let eventTypeId = $(this).val();
        loadEventSubTypeDropdown(eventTypeId);
        loadEnquiryGrid();
    });

    $("#ddlEventSubType, #ddlCompanyHall, #ddlLeadSource, #ddlProcess, #txtSearch, #AmountFrom, #AmountTo, #PAXfrom, #PAXto, #eventFromDate, #eventToDate")
        .on("change", function () {
        loadEnquiryGrid();
    });

    $("#quickRange").on("change", function () {

        let newFilter = $(this).val();

        if (selectedMonthDuration === newFilter) return;

        selectedMonthDuration = newFilter;

        let today = new Date();
        let fromDate, toDate;

        if (selectedMonthDuration === "thismonth") {
            fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
            toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
        }
        else if (selectedMonthDuration === "lastmonth") {
            fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
            toDate = new Date(today.getFullYear(), today.getMonth(), 0);
        }
        else if (selectedMonthDuration === "last7days") { // Last 7 days
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 6);
            toDate = today;
        }
        else if (selectedMonthDuration === "last30days") { // Last 30 days
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 29);
            toDate = today;
        }
        else if (selectedMonthDuration === "yesterday") {
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 1);
            toDate = fromDate;
        }
        else if (selectedMonthDuration === "last12months") {
            fromDate = new Date(today);
            fromDate.setMonth(today.getMonth() - 12);
            toDate = today;
        }
        else if (selectedMonthDuration === "customrange") {
            return;
        }

        $("#enquiryFromDate").val(formatDate2(fromDate));
        $("#enquiryToDate").val(formatDate2(toDate));

        loadEnquiryGrid();
    });    

    $("#enquiryFromDate, #enquiryToDate").on("change", function () {
        // If user manually edits date → switch dropdown to custom
        if ($("#quickRange").val() !== "customrange") {
            $("#quickRange").val("customrange");
            // Optional: update variable also
            selectedMonthDuration = "customrange";
        }
    });

    // Prevent inside click from bubbling
    $("#filterPanel").on("click", function (e) {
        e.stopPropagation();
    });

    // Close on outside click 
    //$(document).on("click", function (e) {
    //    // ❌ Do NOT close filter if click is inside dropdown
    //    if ($(e.target).closest(".dropdown, .dropdown-menu").length > 0) {
    //        return;
    //    }

    //    // ❌ Do NOT close filter if click is inside filter panel
    //    if ($(e.target).closest("#filterPanel").length > 0) {
    //        return;
    //    }
    //    //$("#filterPanel").addClass("d-none");
    //});

    $(document).on("click", function (e) {
        if ($(e.target).closest(".dropdown, .dropdown-menu, #filterPanel, #btnToggleFilter").length > 0) return;
        $("#filterPanel").removeClass("active");
    });

    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
        loadEnquiryGrid();
    });

    $("#btnResetFilter").on("click", function () {

        $("#ddlEventType").val("0");
        $("#ddlEventSubType").val("0");
        $("#ddlCompanyHall").val("0");
        $("#ddlLeadSource").val("0");
        $("#ddlProcess").val("0");

        $("#enquiryFromDate").val("");
        $("#enquiryToDate").val("");

        $("#eventFromDate").val("");
        $("#eventToDate").val("");

        $("#PAXfrom").val("");
        $("#PAXto").val("");

        $("#AmountFrom").val("");
        $("#AmountTo").val("");

        $("#txtSearch").val("");

        setCurrentMonthDates();
        loadEnquiryGrid();
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
    });
    // WITH HEADER -- END ------------

    $("#btn-select-columns").click(function () {
        loadColumnCheckboxes();
        $("#modalColumns").modal("show");
    });

    $("#btn-reset-columns").click(function () {
        $(".column-checkbox").prop("checked", true);
        $("#chk-select-all").prop("checked", true);
    });

    $("#btn-save-columns").click(function () {
        selectedColumns = [];
        $(".column-checkbox:checked").each(function () {
            selectedColumns.push($(this).val());
        });
        $("#modalColumns").modal("hide");
        // reload grid immediately if needed
        loadEnquiryGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });

    $("#btnToggleFilter").on("click", function () {
        $("#filterPanel").toggleClass("active");
    });

    $("#btnCloseFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
    });
});


function loadEnquiryGrid() {
    //const userId = parseInt(sessionStorage.getItem('userId') || 0);
    const payload = {
        // Enquiry Date
        EnquiryFromDate: $("#enquiryFromDate").val() || null,
        EnquiryToDate: $("#enquiryToDate").val() || null,
        EnquiryMonthDuration: selectedMonthDuration || null,

        // Event Date
        EventFromDate: $("#eventFromDate").val() || null,
        EventToDate: $("#eventToDate").val() || null,

        // Search
        NameEmailContactNo: $("#txtSearch").val().trim(),

        // Dropdowns
        EventTypeId: parseInt($("#ddlEventType").val()) || 0,
        EventSubTypeId: parseInt($("#ddlEventSubType").val()) || 0,
        HallId: parseInt($("#ddlCompanyHall").val()) || 0,

        // PAX
        FromPax: parseInt($("#PAXfrom").val()) || null,
        ToPax: parseInt($("#PAXto").val()) || null,

        // Lead Source & Status
        LeadSourceId: parseInt($("#ddlLeadSource").val()) || 0,
        ProcessId: parseInt($("#ddlProcess").val()) || 0,

        // Amount
        FromAmount: parseFloat($("#AmountFrom").val()) || null,
        ToAmount: parseFloat($("#AmountTo").val()) || null
    };
    //console.log("fetchdata payload:", payload);

    $.ajax({
        url: "/api/Report_EnquiryDeals/GetEnquiryDealsReportGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("fetchdata res:", res);
            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }
            renderEnquiryTable(res.data);
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to load enquiry report.");
        }
    });
}

function renderEnquiryTable(data) {

    console.log("data:", data);

    if ($.fn.DataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().clear().destroy();
    }

    const table = $("#tableMain table").DataTable({
        data: data,
        responsive: false,
        pageLength: 25,
        paging: false,
        scrollX: true,
        autoWidth: false,
        fixedHeader: true,
        deferRender: true,
        order: [[0, "desc"]],

        columns: [

            {
                data: "EnquiryId",
                name: "EnquiryId",
                title: "Enq. No.",
                className: "text-center"
            },

            {
                data: "EnquiryDate",
                name: "EnquiryDate",
                title: "Enq. Date",
                className: "text-center"
            },

            {
                data: "EventDate",
                name: "EventDate",
                title: "Event Date",
                className: "text-center"
            },

            {
                data: "ClientName",
                name: "ClientName",
                title: "Client Name"
            },

            {
                data: "EventType_SubType",
                name: "EventType_SubType",
                title: "Event Type"
            },

            {
                data: "Hall",
                name: "Hall",
                title: "Hall"
            },

            {
                data: "Pax",
                name: "Pax",
                title: "Pax",
                className: "text-center"
            },

            {
                data: "LeadSource",
                name: "LeadSource",
                title: "Lead Source"
            },

            {
                data: "Amount",
                name: "Amount",
                title: "Amount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "FollowUps",
                name: "FollowUps",
                title: "Follow-ups",
                className: "text-center"
            },

            {
                data: "Status",
                name: "Status",
                title: "Status"
            }

        ],

        footerCallback: function () {

            const api = this.api();

            const sumByField = (field) =>
                api.column(`${field}:name`).data().reduce(
                    (a, b) => (parseFloat(a) || 0) + (parseFloat(b) || 0),
                    0
                );

            $(api.column("Pax:name").footer())
                .html(sumByField("Pax"));

            $(api.column("Amount:name").footer())
                .html(formatINR2(sumByField("Amount")));

            $(api.column("FollowUps:name").footer())
                .html(sumByField("FollowUps"));
        },

        initComplete: function () {
            $(this.api().table().header())
                .find("th")
                .addClass("bg-warning-subtle text-dark");
        }
    });

    table.columns.adjust();

    $(window).off("resize.enquiryTable").on("resize.enquiryTable", function () {
        table.columns.adjust();
    });
}

function loadEventTypeDropdown(selectedId = "") {
    $.ajax({
        url: "/api/EventType/GetEventTypeList",
        type: "GET",
        success: function (response) {

            let dropdown = $("#ddlEventType");
            dropdown.empty();

            dropdown.append(`<option value="0">-- Event Type --</option>`);

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
    dropdown.append(`<option value="0">-- Select Event Sub Type --</option>`);

    //if (!eventTypeId) return;

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
        url: "/api/Company_HallDetails/GetCompanyHallDetailsList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: 0
        }),
        success: function (response) {

            let dropdown = $("#ddlCompanyHall");
            dropdown.empty();
            dropdown.append(`<option value="0">-- Select Hall --</option>`);
            if (response.status === "Success" && response.data.length > 0) {
                response.data.forEach(item => {
                    dropdown.append(`<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>${item.HallName_Display}</option>`);
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load hall list");
        }
    });
}

// Load lead source list from API
function loadLeadSourceList(selectedId=0) {
    $.ajax({
        url: "/api/LeadSource/GetLeadSourceList",
        type: "GET",
        success: function (response) {
            //console.log("leadSource response:", response);
            let dropdown = $("#ddlLeadSource");
            dropdown.empty();
            dropdown.append(`<option value="0">-- Select Lead Source --</option>`);
            if (response.status === "Success" && response.data.length > 0) {
                response.data.forEach(item => {
                    dropdown.append(`<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>${item.Name}</option>`);
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching lead source list.");
        }
    });
}

// Load process list from API
function loadProcessList(selectedId=0) {
    $.ajax({
        url: "/api/Process/GetProcessList",
        type: "GET",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            //console.log("leadSource response:", response);
            let dropdown = $("#ddlProcess");
            dropdown.empty();
            dropdown.append(`<option value="0">-- Select Lead Source --</option>`);
            if (response.status === "Success" && response.data.length > 0) {
                response.data.forEach(item => {
                    dropdown.append(`<option value="${item.Id}" ${selectedId == item.Id ? "selected" : ""}>${item.Name}</option>`);
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching process list.");
        }
    });
}

function ViewEnquiryReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem('userId') || 0);

    console.log("selectedColumns:", selectedColumns);
    const payload = {
        // Enquiry Date
        EnquiryFromDate: $("#enquiryFromDate").val() || null,
        EnquiryToDate: $("#enquiryToDate").val() || null,
        EnquiryMonthDuration: selectedMonthDuration || null,

        // Event Date
        EventFromDate: $("#eventFromDate").val() || null,
        EventToDate: $("#eventToDate").val() || null,

        // Search
        NameEmailContactNo: $("#txtSearch").val().trim(),

        // Dropdowns
        EventTypeId: parseInt($("#ddlEventType").val()) || 0,
        EventSubTypeId: parseInt($("#ddlEventSubType").val()) || 0,
        HallId: parseInt($("#ddlCompanyHall").val()) || 0,

        // PAX
        FromPax: parseInt($("#PAXfrom").val()) || null,
        ToPax: parseInt($("#PAXto").val()) || null,

        // Lead Source & Status
        LeadSourceId: parseInt($("#ddlLeadSource").val()) || 0,
        ProcessId: parseInt($("#ddlProcess").val()) || 0,

        // Amount
        FromAmount: parseFloat($("#AmountFrom").val()) || null,
        ToAmount: parseFloat($("#AmountTo").val()) || null,

        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };
    console.log("report payload:", payload);

    const url = (actionName === "downloadExcel")
        ? "/api/Report_EnquiryDeals/DownloadEnquiryDealsReport_Excel"
        : "/api/Report_EnquiryDeals/DownloadEnquiryDealsReport";
    console.log("download payload:", payload);

    showTempLoader("Generating Report...");

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("report res:", res);
            //Swal.close();

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            var fileUrl = res.data.FileUrl;

            var link = document.createElement("a");
            link.href = fileUrl;

            if (actionName === "downloadPDF") {
                link.download = `Enquiry_Report_${new Date().toISOString().replace(/[:.-]/g, "")}.pdf`;
            } else if (actionName === "downloadExcel") {
                link.download = `Enquiry_Report_${new Date().toISOString().replace(/[:.-]/g, "")}.xlsx`;
            }

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to download report.");
        },
        complete: function(){
            hideTempLoader();
            //Swal.close();
        }
    });
}

function setCurrentMonthDates() {
    $("#quickRange").val(selectedMonthDuration);

    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#enquiryFromDate").val(formatDate2(firstDay));
    $("#enquiryToDate").val(formatDate2(lastDay));
}

function loadColumnCheckboxes() {
    let html = '';
    REPORT_COLUMNS.forEach(col => {
        let isChecked = selectedColumns.includes(col.key);

        html += `
        <div class="col-md-3 mb-2">
            <div class="form-check">
                <input class="form-check-input column-checkbox"
                       type="checkbox"
                       value="${col.key}"
                       ${isChecked ? "checked" : ""}>
                <label class="form-check-label">
                    ${col.label}
                </label>
            </div>
        </div>`;
    });

    $("#column-list").html(html);
    updateSelectAllState();
}

function getSelectedColumns() {
    let saved = localStorage.getItem(COLUMN_STORAGE_KEY);

    if (saved) {
        return JSON.stringify(saved.split(","));
    }

    return JSON.stringify(REPORT_COLUMNS.map(x => x.key));
}

function updateSelectAllState() {
    let total = $(".column-checkbox").length;
    let checked = $(".column-checkbox:checked").length;
    $("#chk-select-all").prop("checked", total === checked);
}
