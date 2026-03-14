const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No", visible: true },
    { key: "MonthName", label: "Month", visible: true },
    { key: "InvoiceCount", label: "Invoice Generated", visible: true },
    { key: "GrossSales", label: "Gross Sales", visible: true },
    { key: "Taxes", label: "Taxes", visible: true },
    { key: "NetSales", label: "Net Sales", visible: true },
    { key: "PaidAmount", label: "Paid Amount", visible: true },
    { key: "PaymentMode", label: "Payment Mode", visible: true }
];
const DETAILREPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No", visible: true },
    { key: "InvoiceNo", label: "Invoice No", visible: true },
    { key: "InvoiceDate", label: "Invoice Date", visible: true },
    { key: "BookingId", label: "Booking ID", visible: true },

    { key: "ClientName", label: "Client Name", visible: true },
    { key: "GrossAmount", label: "Gross Amount", visible: true },
    { key: "Taxes", label: "Taxes", visible: true },
    { key: "NetAmount", label: "Net Amount", visible: true },

    { key: "PaidAmount", label: "Paid Amount", visible: true },
    { key: "DueAmount", label: "Due Amount", visible: true },
    { key: "PaymentModes", label: "Payment Mode", visible: true }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedColumns_Details = DETAILREPORT_COLUMNS.map(c => c.key);

const HEADER_KEY = "monthlySalesPrintWithHeader";

let selectedYearId = null;
let selectedYear = new Date().getFullYear();
let selectedMonthFrom = null;
let selectedMonthTo = null;

let selectedMonthId = null;

document.addEventListener("DOMContentLoaded", function () {

    populateYearDropdown();
    //loadMonthlySalesGrid();
    loadColumnCheckboxes();

    /* ── Quick Year Range ── */
    $("#quickRange").on("change", function () {
        selectedYearId = $(this).val();
        selectedYear = $("#quickRange option:selected").data("year");

        fetchMonthData(selectedYearId);
    });

    $("#quickRange_MonthFrom, #quickRange_MonthTo").on("change", function () {
        loadMonthlySalesGrid();
    });


    /* ── Filter panel ── */
    $("#filterPanel").on("click", function (e) { e.stopPropagation(); });

    $(document).on("click", function (e) {
        if ($(e.target).closest(".dropdown, .dropdown-menu, #filterPanel, #btnToggleFilter").length > 0) return;
        $("#filterPanel").removeClass("active");
    });

    $("#btnCloseFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
    });

    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").removeClass("active");

        loadMonthlySalesGrid();
    });

    $("#btnResetFilter").on("click", function () {

        $("#quickRange").val("");
        $("#quickRange_MonthFrom").val("0");
        $("#quickRange_MonthTo").val("0");

        selectedMonthFrom = null;
        selectedMonthTo = null;

        loadMonthlySalesGrid();
    });

    $("#btnToggleFilter").on("click", function () {
        $("#filterPanel").toggleClass("active");
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
    $("#btn-select-columns-detail").on("click", function () {
        loadColumnCheckboxes_Detail();
        $("#columns-section-detail").toggle();
    });

    $("#btn-reset-columns").on("click", function () {
        $(".column-checkbox").prop("checked", true);
        $("#chk-select-all").prop("checked", true);
    });
    $("#btn-reset-columns-detail").on("click", function () {
        $(".column-checkbox-detail").prop("checked", true);
        $("#chk-select-all-detail").prop("checked", true);
        updateSelectedColumnsDetails();
    });

    $("#btn-save-columns").on("click", function () {
        selectedColumns = [];
        $(".column-checkbox:checked").each(function () {
            selectedColumns.push($(this).val());
        });
        $("#modalColumns").modal("hide");
        //loadMonthlySalesGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
    $("#chk-select-all-detail").on("change", function () {
        $(".column-checkbox-detail").prop("checked", this.checked);
        updateSelectedColumnsDetails();
    });
    $(document).on("change", ".column-checkbox-detail", function () {
        const total = $(".column-checkbox-detail").length;
        const checked = $(".column-checkbox-detail:checked").length;

        $("#chk-select-all-detail").prop("checked", total === checked);

        updateSelectedColumnsDetails();
    });

    $(document).on("click", ".monthly-sales-month", function () {
        selectedMonthId = $(this).data("month");
        console.log("invoiceMonth:", selectedMonthId);
        loadMonthlySalesMonthDetails(selectedMonthId);
        $("#monthlySalesDetailsModal").modal("show");
    });

    //Recalculate Columns When Modal Opens
    $('#monthlySalesDetailsModal').on('shown.bs.modal', function () {
        $('#tblMonthlySalesDetails').DataTable().columns.adjust();
    });

    $(document).on("keydown", function (e) {
        if (e.key === "Escape") {
            $('.modal').modal('hide');
        }
    });
});

/* =========================
   LOAD YEAR DROPDOWN
========================= */
function populateYearDropdown() {
    $.ajax({
        url: "/api/YearMaster/GetYearList",
        type: "GET",
        contentType: "application/json",
        success: function (res) {
            if (res.status === "Success") {
                const years = res.data || [];
                let dropdown = $("#quickRange");

                dropdown.empty();
                console.log("years:", years);

                years.forEach(y => {
                    let option = $("<option>");

                    option.val(y.Id);
                    option.attr("data-year", y.YearName);

                    if (parseInt(y.YearName) === selectedYear) {
                        option.prop("selected", true);
                        selectedYearId = y.Id;
                        option.text(y.YearName + " (This Year)");
                    }
                    else {
                        option.text(y.YearName);
                    }
                    dropdown.append(option);
                });
                // Fetch months for current year
                fetchMonthData(selectedYearId);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching years");
        }
    });
}

/* =========================
   LOAD MONTHS
========================= */
function fetchMonthData(yearId) {
    var payload = { YearId: parseInt(yearId), MonthId: 0 };
    console.log("month list payload:", payload);

    $.ajax({
        url: "/api/MonthMaster/GetMonthList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status === "Success") {
                const months = res.data || [];
                console.log("months:", months);
                let fromDropdown = $("#quickRange_MonthFrom");
                let toDropdown = $("#quickRange_MonthTo");

                fromDropdown.empty();
                toDropdown.empty();

                fromDropdown.append(`<option value="0">Select</option>`);
                toDropdown.append(`<option value="0">Select</option>`);

                months.forEach(m => {

                    let option1 = $("<option>");
                    option1.val(m.MonthId);
                    option1.text(m.MonthName);

                    let option2 = $("<option>");
                    option2.val(m.MonthId);
                    option2.text(m.MonthName);

                    fromDropdown.append(option1);
                    toDropdown.append(option2);

                });

                // Load grid with current year
                loadMonthlySalesGrid();
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching months");
        }
    });
}

function loadMonthlySalesGrid() {
    selectedYearId = Number($("#quickRange").val() || 0);
    selectedMonthFrom = Number($("#quickRange_MonthFrom").val() || 0);
    selectedMonthTo = Number($("#quickRange_MonthTo").val() || 0);

    const payload = {
        YearId: selectedYearId,
        Year: selectedYear,
        MonthId_From: selectedMonthFrom,
        MonthId_To: selectedMonthTo
    };
    console.log("grid payload:", payload);
    $.ajax({
        url: "/api/Report_MonthlySales/GetMonthlySalesGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }
            renderMonthlySalesTable(res.data.rows, res.data.totals);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load monthly sales");
        }
    });
}

function renderMonthlySalesTable(data, totals) {

    if ($.fn.DataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().clear().destroy();
    }

    $("#tableMain table").DataTable({

        data: data,
        responsive: true,
        pageLength: 25,
        scrollX: true,
        order: [[1, "asc"]],

        paging: false,
        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows
        scrollX: true,            // horizontal scroll if needed

        columns: [

            { data: "SrNo", title: "#" },

            {
                data: "MonthName",
                title: "Month",
                render: function (data, type, row) {

                    return `<a href="javascript:void(0)"
                            class="monthly-sales-month"
                            data-month="${row.MonthId}">
                            ${data}
                        </a>`;
                }
            },

            {
                data: "InvoiceGeneratedCount",
                title: "Total Invoices",
                className: "text-center"
            },

            {
                data: "GrossSales",
                title: "Gross Sales",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "Taxes",
                title: "Taxes",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "NetSales",
                title: "Net Sales",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "PaidAmount",
                title: "Paid Amount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "PaymentMode",
                title: "Payment Mode",
                className: "text-center",
                render: function (data, type, row) {
                    if (!data) return "";

                    if (data.length > 10) {
                        return `<span data-bs-toggle="tooltip" title="${data}">
                        ${data.substring(0, 10)}...
                    </span>`;
                    }
                    return data;
                }
            }

        ],

        footerCallback: function () {

            if (!totals) return;

            const api = this.api();

            $(api.column(0).footer()).html("<strong>TOTAL</strong>");

            $(api.column(2).footer()).html(`<strong>${totals.TotalInvoices}</strong>`);

            $(api.column(3).footer()).html(`<strong>${formatINR2(totals.GrossSales)}</strong>`);

            $(api.column(4).footer()).html(`<strong>${formatINR2(totals.Taxes)}</strong>`);

            $(api.column(5).footer()).html(`<strong>${formatINR2(totals.NetSales)}</strong>`);

            $(api.column(6).footer()).html(`<strong>${formatINR2(totals.TotalPayments)}</strong>`);

        }

    });
}

function loadMonthlySalesMonthDetails(monthId) {
    $("#columns-section-detail").hide();
    const payload = {
        MonthId: monthId
    };

    $.ajax({
        url: "/api/Report_MonthlySales/GetMonthlySalesMonthDetails",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            //renderMonthlySalesDetailsTable(res.data.Rows, res.data.Totals);

            showLoader();
            setTimeout(() => {
                renderMonthlySalesDetailsTable(res.data.Rows, res.data.Totals);
                hideLoader();
            }, 100);

        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load month details");
        }
    });
}

function renderMonthlySalesDetailsTable(rows, totals) {

    const table = $("#tblMonthlySalesDetails");

    if ($.fn.DataTable.isDataTable(table)) {
        table.DataTable().clear().destroy();
    }

    table.DataTable({

        data: rows,
        paging: false,
        searching: false,
        info: false,
        scrollX: true,
        scrollY: "350px",
        scrollCollapse: true,
        autoWidth: false,
        destroy: true,

        columns: [

            {
                data: "SrNo",
                width: "50px"
            },

            {
                data: "BookingId",
                width: "120px",
                render: function (data, type, row) {
                    if (!data) return "";
                    return `#${data}`;
                }
            },

            {
                data: "InvoiceNo",
                width: "120px",
                render: function (data, type, row) {
                    if (!data) return "";
                    return `#${data}`;
                }
            },

            {
                data: "InvoiceDate",
                width: "120px"
            },

            {
                data: "ClientName",
                width: "220px"
            },

            {
                data: "GrossAmount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "Taxes",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "NetAmount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "PaidAmount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "DueAmount",
                className: "text-end",
                render: d => formatINR2(d)
            },

            {
                data: "PaymentModes",
                title: "Payment Mode",
                className: "text-center",
                render: function (data, type, row) {
                    if (!data) return "";

                    if (data.length > 10) {
                        return `<span data-bs-toggle="tooltip" title="${data}">
                        ${data.substring(0, 10)}...
                    </span>`;
                    }
                    return data;
                }
            }

        ],

        /* ───────── Footer Totals ───────── */

        footerCallback: function () {

            if (!totals) return;

            const api = this.api();

            $(api.column(0).footer()).html("TOTAL");

            $(api.column(4).footer()).html(formatINR2(totals.GrossAmount));
            $(api.column(5).footer()).html(formatINR2(totals.Taxes));
            $(api.column(6).footer()).html(formatINR2(totals.NetAmount));
            $(api.column(7).footer()).html(formatINR2(totals.PaidAmount));
            $(api.column(8).footer()).html(formatINR2(totals.DueAmount));
        }

    });

}

function ViewMonthlySalesReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        YearId: selectedYearId,
        Year: selectedYear,
        MonthId_From: selectedMonthFrom,
        MonthId_To: selectedMonthTo,
        Columns: selectedColumns,
        WithHeader: withHeader,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_MonthlySales/DownloadMonthlySalesReport_Excel"
        : "/api/Report_MonthlySales/DownloadMonthlySalesReport_Pdf";

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

            const fileUrl = res.data.FileUrl;
            const ext = actionName === "downloadExcel" ? "xlsx" : "pdf";

            const ts = new Date().toISOString().replace(/[:.-]/g, "");

            const link = document.createElement("a");
            link.href = fileUrl;
            link.download = `MonthlySales_Report_${ts}.${ext}`;

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching monthly report");
            Swal.close();
        }
    });
}
function ViewMonthlySalesDetailsReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        MonthId: selectedMonthId,
        Columns: selectedColumns_Details,
        WithHeader: withHeader,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_MonthlySales/DownloadMonthlySalesMonthDetailsReport_Excel"
        : "/api/Report_MonthlySales/DownloadMonthlySalesMonthDetailsReport_Pdf";

    showTempLoader("Generating Report...");

    console.log("details payload:", payload);
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

            const fileUrl = res.data.FileUrl;
            const ext = actionName === "downloadExcel" ? "xlsx" : "pdf";

            const ts = new Date().toISOString().replace(/[:.-]/g, "");

            const link = document.createElement("a");
            link.href = fileUrl;
            link.download = `MonthlySalesDetails_Report_${ts}.${ext}`;

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error fetching monthly details report");
            Swal.close();
        }
    });
}

/* ─────────────────────────────────────────────
   HELPERS
───────────────────────────────────────────── */
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
function loadColumnCheckboxes_Detail() {
    let html = "";

    DETAILREPORT_COLUMNS.forEach(col => {
        const isChecked = selectedColumns_Details.includes(col.key);
        html += `
        <div class="col-md-2 mb-2">
            <div class="form-check">
                <input class="form-check-input column-checkbox-detail"
                       type="checkbox"
                       value="${col.key}"
                       ${isChecked ? "checked" : ""}>
                <label class="form-check-label">${col.label}</label>
            </div>
        </div>`;
    });

    $("#column-list-details").html(html);
    updateSelectAllState_Detail();
}

function updateSelectAllState() {
    const total = $(".column-checkbox").length;
    const checked = $(".column-checkbox:checked").length;
    $("#chk-select-all").prop("checked", total === checked);
}
function updateSelectAllState_Detail() {
    const total = $(".column-checkbox-detail").length;
    const checked = $(".column-checkbox-detail:checked").length;
    $("#chk-select-all-detail").prop("checked", total === checked);
}
function updateSelectedColumnsDetails() {
    selectedColumns_Details = [];
    $(".column-checkbox-detail:checked").each(function () {
        selectedColumns_Details.push($(this).val());
    });
    console.log("selectedColumns_Details:", selectedColumns_Details);
}
