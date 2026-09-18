const REPORT_COLUMNS = [

    { key: "BRN", label: "BRN", visible: true },

    { key: "Client_Name", label: "Client Name", visible: true },
    { key: "Client_Contact", label: "Contact", visible: true },

    { key: "EventDate", label: "Event Date", visible: true },
    { key: "EventType", label: "Event Type", visible: true },
    { key: "HallName", label: "Hall Name", visible: true },

    { key: "Pax", label: "Pax", visible: true },
    { key: "Package", label: "Package", visible: true },

    { key: "DealAmount", label: "Deal Amount", visible: true },

    { key: "SubTotal", label: "Sub-Total", visible: true },

    { key: "Discount", label: "Discount", visible: true },

    { key: "NetAmount", label: "Net Total", visible: true },

    { key: "TaxableAmount", label: "Taxable Amount", visible: true },

    { key: "Tax", label: "Tax Amount", visible: true },

    { key: "InvoiceAmount", label: "Invoice Amount", visible: true },

    { key: "InvoiceNo", label: "Invoice No", visible: true }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "printWithHeader";

document.addEventListener("DOMContentLoaded", function () {
    loadEventTypeDropdown();
    loadEventSubTypeDropdown(0);
    loadCompanyHallDropdown();
    setCurrentMonthDates();
    loadBookingGrid();
    loadColumnCheckboxes();


    $("#ddlEventType").on("change", function () {
        let eventTypeId = $(this).val();
        loadEventSubTypeDropdown(eventTypeId);
        loadBookingGrid();
    });

    $("#ddlEventSubType, #ddlCompanyHall, #txtSearch, #fltIsInvoiceGenerated").on("change", function () {
        loadBookingGrid();
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

        $("#fromDate").val(formatDate2(fromDate));
        $("#toDate").val(formatDate2(toDate));

        loadBookingGrid();
    });    

    $("#fromDate, #toDate").on("change", function () {
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
        loadBookingGrid();
    });

    $("#btnResetFilter").on("click", function () {

        $("#ddlEventType").val("0");
        $("#ddlEventSubType").val("0");
        $("#ddlCompanyHall").val("0");
        $("#fromDate").val("");
        $("#toDate").val("");
        $("#fltIsInvoiceGenerated").val("");
        setCurrentMonthDates();
        loadBookingGrid();
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
        loadBookingGrid();
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
function loadBookingGrid() {

    const userId = parseInt(sessionStorage.getItem('userId') || 0);

    const payload = {
        NameEmailContactNo: $("#txtSearch").val(),
        EventTypeId: parseInt($("#ddlEventType").val()) || 0,
        EventSubTypeId: parseInt($("#ddlEventSubType").val()) || 0,
        HallId: parseInt($("#ddlCompanyHall").val()) || 0,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        IsInvoiceGenerated: $("#fltIsInvoiceGenerated").val() === "" ? null : $("#fltIsInvoiceGenerated").val() === "true" ? true : false,
        FilterDuration: selectedMonthDuration,
        ActionBy: userId
    };
    console.log("payload:", payload);

    $.ajax({
        url: "/api/Report_Booking/GetBookingReportGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }
            renderBookingTable(res.data);
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to generate booking report.");
        }
    });
}

function renderBookingTable(data) {
    console.log("data:", data);
    if ($.fn.DataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().clear().destroy();
    }

    const table = $("#tableMain table").DataTable({
        data: data,
        responsive: false,
        pageLength: 25,
        scrollX: true,
        order: [[1, "desc"]],
        autoWidth: false,
        paging: false,
        fixedHeader: true,
        deferRender: true,
        //scrollY: "60vh",          // table body scroll height
        scrollCollapse: false,     // shrink if less rows

        columns: [
            { data: "BRN", name: "BRN", title: "#BRN" },

            { data: "Client_Name", name: "Client_Name", title: "Client Name" },

            {
                data: "Client_Contact",
                name: "Client_Contact",
                title: "Contact",
                className: "text-center"
            },

            {
                data: null,
                name: "InvoiceNo",
                title: "Invoice No",
                className: "text-center",
                render: function (data, type, row) {

                    if (row.InvoiceNo && row.InvoiceNo > 0)
                        return row.InvoiceNo;

                    if (row.GSTInvoiceNo)
                        return row.GSTInvoiceNo;

                    return "-";
                }
            },

            {
                data: "EventDate",
                name: "EventDate",
                title: "Event Date",
                render: function (data, type, row) {

                    if (type === "display")
                        return row.EventDate_Format;

                    return data;
                }
            },

            { data: "EventDisplay", name: "EventDisplay", title: "Event", width: "100px" },

            { data: "HallName", name: "HallName", title: "Hall Name" },

            {
                data: "Pax",
                name: "Pax",
                title: "Pax",
                className: "text-center"
            },

            { data: "Package", name: "Package", title: "Package" },

            {
                data: "DealAmount",
                name: "DealAmount",
                title: "Deal Amount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "SubTotal",
                name: "SubTotal",
                title: "SubTotal",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "Discount",
                name: "Discount",
                title: "Discount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "NetAmount",
                name: "NetAmount",
                title: "Net Amount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "TaxableAmount",
                name: "TaxableAmount",
                title: "Taxable Amount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "TaxAmount",
                name: "TaxAmount",
                title: "Tax Amount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "InvoiceAmount",
                name: "InvoiceAmount",
                title: "Invoice Amount",
                className: "text-end fw-bold",
                render: d => formatINR2(d)
            }
        ],

        footerCallback: function () {
            const api = this.api();
            const sumByField = (field) =>
                api.column(`${field}:name`).data().reduce(
                    (a, b) => (parseFloat(a) || 0) + (parseFloat(b) || 0),
                    0
                );

            $(api.column('Pax:name').footer()).html(sumByField('Pax'));
            $(api.column('DealAmount:name').footer()).html(formatINR2(sumByField('DealAmount')));

            $(api.column('SubTotal:name').footer()).html(formatINR2(sumByField('SubTotal')));
            $(api.column('Discount:name').footer()).html(formatINR2(sumByField('Discount')));

            $(api.column('NetAmount:name').footer()).html(formatINR2(sumByField('NetAmount')));
            $(api.column('TaxableAmount:name').footer()).html(formatINR2(sumByField('TaxableAmount')));

            $(api.column('TaxAmount:name').footer()).html(formatINR2(sumByField('TaxAmount')));
            $(api.column('InvoiceAmount:name').footer()).html(formatINR2(sumByField('InvoiceAmount')));
        },

        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },
    });

    table.columns.adjust();

    $(window).on('resize', function () {
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
function ViewBookingReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem('userId') || 0);

    console.log("selectedColumns:", selectedColumns);
    const payload = {
        NameEmailContactNo: $("#txtSearch").val(),
        EventTypeId: parseInt($("#ddlEventType").val()) || 0,
        EventSubTypeId: parseInt($("#ddlEventSubType").val()) || 0,
        HallId: parseInt($("#ddlCompanyHall").val()) || 0,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration,
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = (actionName === "downloadExcel")
        ? "/api/Report_Booking/DownloadBookingReport_Excel"
        : "/api/Report_Booking/DownloadBookingReport";
    console.log("download payload:", payload);

    downloadReport(url, payload, 
        loaderMsg = actionName === "downloadExcel" ? "Generating Booking Excel Report..." : "Generating Booking PDF Report...", 
        failedMsg = "Failed to generate Booking Report");
}
function setCurrentMonthDates() {
    $("#quickRange").val(selectedMonthDuration);

    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#fromDate").val(formatDate2(firstDay));
    $("#toDate").val(formatDate2(lastDay));
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
