const REPORT_COLUMNS = [

    { key: "SrNo", label: "SrNo.", visible: true },
    { key: "Client_Name", label: "Client Name", visible: true },
    { key: "Client_Contact", label: "Contact", visible: true },
    { key: "Client_Email", label: "Email", visible: true },

    { key: "EventDate", label: "Event Date", visible: true },
    { key: "EventTypeName", label: "Event", visible: true },
    { key: "EventSubTypeName", label: "Sub-Event", visible: true },
    { key: "HallName", label: "Hall Name", visible: true },

    { key: "Pax", label: "Pax", visible: true },
    { key: "Package", label: "Package", visible: true },
    { key: "DealAmount", label: "Deal Amount", visible: true },

    { key: "TotalAmount", label: "Total Amount", visible: true },
    { key: "Discount", label: "Discount", visible: true },
    { key: "Tax", label: "Tax", visible: true },
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

    $("#ddlEventSubType, #ddlCompanyHall, #txtSearch").on("change", function () {
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

    if ($.fn.DataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().clear().destroy();
    }

    $("#tableMain table").DataTable({
        data: data,
        responsive: true,
        pageLength: 25,
        scrollX: true,
        order: [[1, "desc"]],

        paging: false,
        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows

        columns: [
            { data: "BookingId", name: "BookingId", title: "#BRN" },
            { data: "Client_Name", name: "Client_Name", title: "Client Name" },
            { data: "Client_Contact", name: "Client_Contact", title: "Contact", className: "text-center" },

            {
                data: "InvoiceNo",
                name: "InvoiceNo",
                title: "Invoice No",
                className: "text-center",
                render: function (data) {
                    return (!data || data === 0) ? "-" : data;
                }
            },

            { data: "EventDate", name: "EventDate", title: "Event Date" },
            { data: "EventDisplay", name: "EventDisplay", title: "Event" },
            { data: "HallName", name: "HallName", title: "Hall Name" },

            { data: "Pax", name: "Pax", title: "Pax", className: "text-center" },

            { data: "Package", name: "Package", title: "Package" },

            { data: "InvoiceAmount", name: "InvoiceAmount", title: "Invoice Amount", className: "text-end", render: d => formatINR2(d) },

            { data: "DealAmount", name: "DealAmount", title: "Deal Amount", className: "text-end", render: d => formatINR2(d) },

            { data: "Discount", name: "Discount", title: "Discount", className: "text-end", render: d => formatINR2(d) },

            { data: "Tax", name: "Tax", title: "Tax", className: "text-end", render: d => formatINR2(d) },

            { data: "TotalAmount", name: "TotalAmount", title: "Total", className: "text-end", render: d => formatINR2(d) },
        ],

        footerCallback: function (row, data, start, end, display) {
            const api = this.api();
            function sumByField(field) {
                return api
                    .column(`${field}:name`)
                    .data()
                    .reduce((a, b) => (parseFloat(a) || 0) + (parseFloat(b) || 0), 0);
            }

            const pax = sumByField('Pax');
            const invoice = sumByField('InvoiceAmount');
            const deal = sumByField('DealAmount');
            const discount = sumByField('Discount');
            const tax = sumByField('Tax');
            const total = sumByField('TotalAmount');

            $(api.column('Pax:name').footer()).html(pax);
            $(api.column('InvoiceAmount:name').footer()).html(formatINR2(invoice));
            $(api.column('DealAmount:name').footer()).html(formatINR2(deal));
            $(api.column('Discount:name').footer()).html(formatINR2(discount));
            $(api.column('Tax:name').footer()).html(formatINR2(tax));
            $(api.column('TotalAmount:name').footer()).html(formatINR2(total));
        },
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },

    });
}

function loadEventTypeDropdown(selectedId = "") {
    $.ajax({
        url: "/api/EventType/GetEventTypeListForDropdown",
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
        url: "/api/Company_HallDetails/GetCompanyHallDetailsList_Dropdown",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Id: 0   // 0 or null = get all halls
        }),
        success: function (response) {

            let dropdown = $("#ddlCompanyHall");
            dropdown.empty();

            dropdown.append(`<option value="0">-- Select Hall --</option>`);

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

    showTempLoader("Generating Report...");

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            Swal.close();

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            var fileUrl = res.data.FileUrl;

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
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to download report.");
        }
    });
}
function setCurrentMonthDates() {
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
