/* ═══ COLUMN DEFINITIONS ══════ */
const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No" },
    { key: "EventDate", label: "Event Date" },
    { key: "GuestName", label: "Guest Name" },
    { key: "EventType", label: "Event Type" },
    { key: "EventSubType", label: "Event Sub Type" },
    { key: "LeadSource", label: "Lead Source" },
    { key: "EnquiryDateTime", label: "Enquiry DateTime" },
    { key: "LeadLostCategory", label: "Lost Reason Category" },
    { key: "LeadLostReason", label: "Lost Reason" },
    { key: "LossAmount", label: "Loss Amount" }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "revenueLossPrintWithHeader";

/* ══ INIT ═════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadRevenueLossGrid();
    loadColumnCheckboxes();

    loadEventTypeDropdown();
    loadEventSubTypeDropdown();
    loadLeadSourceList();

    /* ── Quick range ── */
    $("#quickRange").on("change", function () {

        const newFilter = $(this).val();
        if (selectedMonthDuration === newFilter) return;
        selectedMonthDuration = newFilter;

        if (selectedMonthDuration === "customrange") return;

        const today = new Date();
        let fromDate, toDate;

        switch (selectedMonthDuration) {
            case "thismonth":
                fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
                toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
                break;
            case "lastmonth":
                fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
                toDate = new Date(today.getFullYear(), today.getMonth(), 0);
                break;
            case "last7days":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 6);
                toDate = today;
                break;
            case "last30days":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 29);
                toDate = today;
                break;
            case "yesterday":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 1);
                toDate = new Date(fromDate);
                break;
            case "last12months":
                fromDate = new Date(today);
                fromDate.setMonth(today.getMonth() - 12);
                toDate = today;
                break;
        }

        $("#fromDate").val(formatDate2(fromDate));
        $("#toDate").val(formatDate2(toDate));
        loadRevenueLossGrid();
    });

    $("#fromDate, #toDate").on("change", function () {
        // If user manually edits date → switch dropdown to custom
        if ($("#quickRange").val() !== "customrange") {
            $("#quickRange").val("customrange");
            // Optional: update variable also
            selectedMonthDuration = "customrange";
        }
    });


    /* ── Filter panel ── */
    $("#filterPanel").on("click", function (e) { e.stopPropagation(); });

    $(document).on("click", function (e) {
        if ($(e.target).closest(".dropdown, .dropdown-menu, #filterPanel, #btnToggleFilter").length > 0) return;
        $("#filterPanel").removeClass("active");
    });

    $("#ddlEventType").on("change", function () {
        const id = $(this).val();
        loadEventSubTypeDropdown(id);
        loadRevenueLossGrid();
    });

    $("#ddlEventSubType, #ddlLeadSource").on("change", function () {
        loadRevenueLossGrid();
    });

    $("#btnToggleFilter").on("click", function () { $("#filterPanel").toggleClass("active"); });
    $("#btnCloseFilter").on("click", function () { $("#filterPanel").removeClass("active"); });

    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
        loadRevenueLossGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        $("#ddlEventType, #ddlEventSubType, #ddlLeadSource").val("0");

        setCurrentMonthDates();
        loadRevenueLossGrid();
    });

    /* ── With Header ── */
    const saved = sessionStorage.getItem(HEADER_KEY);
    if (saved !== null)
        document.getElementById("toggleWithHeader").checked = (saved === "true");

    document.getElementById("toggleWithHeader").addEventListener("change", function () {
        sessionStorage.setItem(HEADER_KEY, this.checked);
    });

    /* ── Column selector ── */
    $("#btn-select-columns").on("click", function () {
        loadColumnCheckboxes();
        $("#modalColumns").modal("show");
    });

    $("#btn-reset-columns").on("click", function () {
        $(".column-checkbox").prop("checked", true);
        $("#chk-select-all").prop("checked", true);
    });

    $("#btn-save-columns").on("click", function () {
        selectedColumns = [];
        $(".column-checkbox:checked").each(function () {
            selectedColumns.push($(this).val());
        });
        $("#modalColumns").modal("hide");
        loadRevenueLossGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

function loadEventTypeDropdown() {
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
                        `<option value="${item.Id}">
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
function loadEventSubTypeDropdown(eventTypeId=0) {

    let dropdown = $("#ddlEventSubType");

    dropdown.prop("disabled", true);
    dropdown.empty();
    dropdown.append(`<option value="0">--Select Sub-Event Type--</option>`);

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
                        `<option value="${item.Id}">${item.Name}</option>`
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

function loadLeadSourceList() {
    $.ajax({
        url: "/api/LeadSource/GetLeadSourceList",
        type: "GET",
        success: function (response) {
            //console.log("leadSource response:", response);
            if (response.status && response.status.toLowerCase() === "success") {
                let options = `<option value = "0"> --Select Lead Source--</option> `; // default option
                $.each(response.data, function (index, item) {
                    options += `<option value = "${item.Id}"> ${item.Name}</option> `;
                });

                $("#ddlLeadSource").html(options);
            }
            else {
                $("#ddlLeadSource").html(`<option value = "0">No Lead Source Found</option> `);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching lead source list.");
        }
    });
}


/* ═══ LOAD GRID═════ */
function loadRevenueLossGrid() {

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration,
        EventTypeId: $("#ddlEventType").val() || 0,
        EventSubTypeId: $("#ddlEventSubType").val() || 0,
        LeadSourceId: $("#ddlLeadSource").val() || 0
    };

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading Revenue Loss data...
        </div>
    `);

    $.ajax({
        url: "/api/Report_RevenueLoss/GetRevenueLossGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            $("#tableMain").html(`
                <table id="tblRevenueLoss" class="table table-striped table-bordered w-100">
                    <tfoot>
                        <tr>
                            <th></th>
                            <th></th>
                            <th></th>
                            <th></th>

                            <th></th>
                            <th></th>

                            <th></th>
                            <th></th>
                            <th></th>
                            <th></th>
                        </tr>
                    </tfoot>
                </table>
            `);

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }
            renderRevenueLossTable(res.data.Rows, res.data.Totals);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Revenue Loss Failed");
            $("#tableMain").html(`
                <div class="alert alert-danger">
                    Failed to load data. Please try again.
                </div>
            `);
        }
    });
}

/* ══ RENDER TABLE ═════*/
function renderRevenueLossTable(rows, totals) {

    if ($.fn.DataTable.isDataTable("#tblRevenueLoss")) {
        $("#tblRevenueLoss").DataTable().clear().destroy();
    }

    $("#tblRevenueLoss").DataTable({
        data: rows,
        responsive: true,
        pageLength: 25,
        scrollX: true,
        order: [[1, "asc"]],

        paging: false,
        scrollY: "60vh",
        scrollCollapse: true,

        columns: [
            {
                data: "SrNo",
                title: "#",
                width: "50px",
                className: "text-center"
            },

            {
                data: 'EventDate', // actual date (ISO format from backend: yyyy-MM-dd)
                title: 'Event Date',
                name: 'EventDate',
                width: "110px",
                render: function (data, type, row) {

                    if (type === 'display') {
                        return row.EventDate_Format; // show formatted
                    }

                    return data; // use raw date for sorting
                }
            },

            {
                data: "GuestName",
                title: "Guest Name"
            },
            {
                data: "EventType",
                title: "Event Category"
            },
            {
                data: "EventSubType",
                title: "Event SubCategory"
            },
            {
                data: "LeadSource",
                title: "Lead Source"
            },
            {
                data: "EnquiryDateTime",
                title: "Enquiry DateTime"
            },
            {
                data: "LeadLostReason_CategoryName",
                title: "Lead Lost Reason - Category"
            },
            {
                data: "LeadLostReason_Name",
                title: "Lead Lost Reason"
            },
            {
                data: "LossAmount",
                title: "Loss Amount",
                className: "text-end fw-semibold text-danger",
                render: d => formatINR2(d)
            }
        ],

        /* ── Footer totals from API ── */
        footerCallback: function (tfoot) {

            const $cells = $(tfoot).find("th");

            $cells.eq(0).html("");
            $cells.eq(1).html("");
            $cells.eq(2).html("");
            $cells.eq(3).html("");

            $cells.eq(4).html("");
            $cells.eq(5).html("");

            $cells.eq(6).html("");
            $cells.eq(7).html("");
            $cells.eq(8).html("<strong>TOTAL</strong>");

            $cells.eq(9).html(`
                <strong class="text-danger fs-8">
                    ${formatINR2(totals.TotalLossAmount)}
                </strong>
            `);
        },

        language: {
            emptyTable: "No revenue loss records found for the selected period."
        },
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },

    });    
}

/* ═══ DOWNLOAD PDF / EXCEL ═══════ */
function ViewRevenueLossReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration,
        EventTypeId: $("#ddlEventType").val() || 0,
        EventSubTypeId: $("#ddlEventSubType").val() || 0,
        LeadSourceId: $("#ddlLeadSource").val() || 0,
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_RevenueLoss/DownloadRevenueLossReport_Excel"
        : "/api/Report_RevenueLoss/DownloadRevenueLossReport";

    downloadReport(url, payload, 
        loaderMsg = actionName === "downloadExcel" ? "Generating RevenueLoss Excel Report..." : "Generating RevenueLoss PDF Report...", 
        failedMsg = "Failed to generate RevenueLoss Report");
}

/* ══ HELPERS ══════ */
function setCurrentMonthDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    $("#fromDate").val(formatDate2(firstDay));
    $("#toDate").val(formatDate2(lastDay));
}

function loadColumnCheckboxes() {
    let html = "";
    REPORT_COLUMNS.forEach(col => {
        const isChecked = selectedColumns.includes(col.key);
        html += `
        <div class="col-md-4 mb-2">
            <div class="form-check">
                <input class="form-check-input column-checkbox"
                       type="checkbox"
                       value="${col.key}"
                       ${isChecked ? "checked" : ""}>
                <label class="form-check-label">${col.label}</label>
            </div>
        </div>`;
    });
    $("#column-list").html(html);
    updateSelectAllState();
}

function updateSelectAllState() {
    const total = $(".column-checkbox").length;
    const checked = $(".column-checkbox:checked").length;
    $("#chk-select-all").prop("checked", total === checked);
}