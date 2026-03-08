/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No" },
    { key: "Month", label: "Month" },
    { key: "GSTType", label: "GST Type" },
    { key: "TaxableAmount", label: "Taxable Amount" },
    { key: "CGST", label: "CGST" },
    { key: "SGST", label: "SGST" },
    { key: "IGST", label: "IGST" },
    { key: "TotalGST", label: "Total GST" },
    { key: "GrandTotal", label: "Grand Total" }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "gstSalesPrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentYearDates();
    loadGSTSalesGrid();
    loadColumnCheckboxes();

    /* ── Quick range ── */
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
        else if (selectedMonthDuration === "days") { // Last 7 Days
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 6);
            toDate = today;
        }
        else if (selectedMonthDuration === "lastdays") { // Last 30 Days
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 29);
            toDate = today;
        }
        else if (selectedMonthDuration === "yesterday") {
            fromDate = new Date(today);
            fromDate.setDate(today.getDate() - 1);
            toDate = fromDate;
        }
        else if (selectedMonthDuration === "12months") {
            fromDate = new Date(today);
            fromDate.setMonth(today.getMonth() - 12);
            toDate = today;
        }
        else if (selectedMonthDuration === "customrange") {
            return;
        }

        $("#fromDate").val(formatDate2(fromDate));
        $("#toDate").val(formatDate2(toDate));

        loadGSTSalesGrid();
    });    

    /* ── Filter panel ── */
    $("#filterPanel").on("click", function (e) { e.stopPropagation(); });

    $(document).on("click", function (e) {
        if ($(e.target).closest(".dropdown, .dropdown-menu, #filterPanel, #btnToggleFilter").length > 0) return;
        $("#filterPanel").removeClass("active");
    });

    $("#btnToggleFilter").on("click", function () {
        $("#filterPanel").toggleClass("active");
    });

    $("#btnCloseFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
    });

    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
        loadGSTSalesGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thisyear");
        selectedMonthDuration = "thisyear";
        setCurrentYearDates();
        loadGSTSalesGrid();
    });

    /* ── With Header toggle ── */
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
        loadGSTSalesGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadGSTSalesGrid() {

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
    };

    // Show loading state
    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading GST data...
        </div>
    `);

    $.ajax({
        url: "/api/Report_GST/GetGSTSalesGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            // Restore table container
            $("#tableMain").html(`
                <table id="tblGSTSales" class="table table-striped table-bordered w-100">
                    <tfoot>
                        <tr>
                            <th></th><th></th><th></th>
                            <th></th><th></th><th></th>
                            <th></th><th></th><th></th>
                        </tr>
                    </tfoot>
                </table>
            `);

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            renderGSTSalesTable(res.data);
        },
        error: function (xhr, status, error) {
            console.error("Fetch GST Sales Failed", xhr, status, error);
            $("#tableMain").html(`
                <div class="alert alert-danger">
                    Failed to load GST data. Please try again.
                </div>
            `);
        }
    });
}

/* ═══════════════════════════════════════════════════
   RENDER TABLE
═══════════════════════════════════════════════════ */
function renderGSTSalesTable(data) {

    if ($.fn.DataTable.isDataTable("#tblGSTSales")) {
        $("#tblGSTSales").DataTable().clear().destroy();
    }

    $("#tblGSTSales").DataTable({
        data: data,
        responsive: true,
        pageLength: 25,
        scrollX: true,
        order: [[0, "asc"]],

        columns: [
            {
                data: "SrNo",
                title: "#",
                width: "50px",
                className: "text-center"
            },
            {
                data: "Month",
                title: "Month",
                width: "120px"
            },
            {
                data: "GSTType",
                title: "GST Type",
                className: "text-center",
                render: function (data) {
                    if (!data || data === "-") return "<span class='text-muted'>-</span>";

                    let color = "secondary";
                    if (data === "Sales (Invoice)") color = "success";
                    if (data === "Sales (Booking)") color = "primary";
                    if (data === "Purchase") color = "danger";

                    return `<span class="badge bg-${color}">${data}</span>`;
                }
            },
            {
                data: "TaxableAmount",
                title: "Taxable Amt",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "CGST",
                title: "CGST",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "SGST",
                title: "SGST",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "IGST",
                title: "IGST",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "TotalGST",
                title: "Total GST",
                className: "text-end fw-semibold text-danger",
                render: d => formatINR2(d)
            },
            {
                data: "GrandTotal",
                title: "Grand Total",
                className: "text-end fw-bold",
                render: d => formatINR2(d)
            },
            // ── hidden ──
            //{ data: "Yr", visible: false },
            //{ data: "Mn", visible: false }
        ],

        /* ── Footer totals row ── */
        footerCallback: function (tfoot, data, start, end, display) {

            const api = this.api();
            const sumCol = idx =>
                api.column(idx, { page: "all" })
                    .data()
                    .reduce((acc, val) => acc + (parseFloat(val) || 0), 0);

            const fmt = v => `<strong class="text-dark">${formatINR2(v)}</strong>`;

            const $cells = $(tfoot).find("th");

            $cells.eq(0).html("<strong>TOTAL</strong>");
            $cells.eq(1).html("");
            $cells.eq(2).html("");
            $cells.eq(3).html(fmt(sumCol(3)));
            $cells.eq(4).html(fmt(sumCol(4)));
            $cells.eq(5).html(fmt(sumCol(5)));
            $cells.eq(6).html(fmt(sumCol(6)));
            $cells.eq(7).html(fmt(sumCol(7)));
            $cells.eq(8).html(fmt(sumCol(8)));
        },

        /* ── Row grouping visual: shade alternating months ── */
        createdRow: function (row, rowData) {
            if (rowData.gstType === "Purchase") {
                $(row).addClass("table-warning");
            }
        },

        language: {
            emptyTable: "No GST data found for the selected period."
        }
    });
}

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewGSTSalesReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration,
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_GST/DownloadGSTSalesReport_Excel"
        : "/api/Report_GST/DownloadGSTSalesReport";

    // Show loading on button
    const $btn = $(".dropdown-item:contains('PDF'), .dropdown-item:contains('Excel')").filter(function () {
        return $(this).attr("onclick")?.includes(actionName);
    });

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

            const ext = actionName === "downloadExcel" ? "xlsx" : "pdf";
            const ts = new Date().toISOString().replace(/[:.-]/g, "");
            const link = document.createElement("a");
            link.href = res.data.FileUrl;
            link.download = `GSTSales_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            Swal.close();
            console.error("Download GST Sales Failed", xhr, status, error);
            Swal.fire("Error", "Failed to generate report. Please try again.", "error");
        }
    });
}

/* ═══════════════════════════════════════════════════
   HELPERS
═══════════════════════════════════════════════════ */
function setCurrentYearDates() {
    const today = new Date();
    $("#fromDate").val(formatDate2(new Date(today.getFullYear(), 0, 1)));
    $("#toDate").val(formatDate2(new Date(today.getFullYear(), 11, 31)));
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