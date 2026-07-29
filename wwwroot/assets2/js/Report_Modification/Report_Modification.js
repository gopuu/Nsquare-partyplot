/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No" },
    { key: "ModuleName", label: "Module" },
    { key: "RowId", label: "Row Id" },
    { key: "ReferenceNo", label: "Reference No" },
    { key: "OldAmount", label: "Old Amount" },
    { key: "NewAmount", label: "New Amount" },
    { key: "ImpactAmount", label: "Impact Amount" },
    { key: "ModifiedDateTime", label: "Modified Datetime" },
    { key: "ModifiedBy", label: "Modified By" }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "modificationPrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadModificationGrid();
    loadColumnCheckboxes();
    loadModules();

    /* ── Module filter live reload ── */
    $("#ddlModule").on("change", function () {
        loadModificationGrid();
    });

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
            case "thisyear":
                fromDate = new Date(today.getFullYear(), 0, 1);
                toDate = new Date(today.getFullYear(), 11, 31);
                break;
            case "lastyear":
                fromDate = new Date(today.getFullYear() - 1, 0, 1);
                toDate = new Date(today.getFullYear() - 1, 11, 31);
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
        loadModificationGrid();
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

    $("#btnToggleFilter").on("click", function () { $("#filterPanel").toggleClass("active"); });
    $("#btnCloseFilter").on("click", function () { $("#filterPanel").removeClass("active"); });

    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
        loadModificationGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        $("#ddlModule").val("0");
        setCurrentMonthDates();
        loadModificationGrid();
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
        loadModificationGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadModificationGrid() {

    const payload = {
        ModuleId: parseInt($("#ddlModule").val()) || 0,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
    };
    console.log("grid payload:", payload);

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading Modification data...
        </div>
    `);
    $("#modSummarySection").hide();

    $.ajax({
        url: "/api/Report_Modification/GetModificationGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            $("#tableMain").html(`
                <table id="tblModification" class="table table-bordered w-100">
                    <tfoot>
                        <tr>
                            <th></th><th></th><th></th><th></th><th></th>
                        </tr>
                    </tfoot>
                </table>
            `);

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            renderModificationTable(res.data.LineItems);
            renderModificationSummary(res.data.Summary);
        },
        error: function (xhr, status, error) {
            //handleApiError(xhr, status, error, "Download Modification Report Failed");
            $("#tableMain").html(`<div class="alert alert-danger">Failed to load data. Please try again.</div>`);
        }
    });
}

/* ═══════════════════════════════════════════════════
   RENDER TABLE
═══════════════════════════════════════════════════ */
function renderModificationTable(data) {

    if ($.fn.DataTable.isDataTable("#tblModification")) {
        $("#tblModification").DataTable().clear().destroy();
    }

    $("#tblModification").DataTable({
        data: data,
        responsive: false,
        pageLength: 25,
        scrollX: true,
        order: [[1, "asc"]],

        paging: false,
        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows
        scrollX: true,            // horizontal scroll if needed

        columns: [
            {
                data: "SrNo",
                title: "#",
                width: "60px",
                className: "text-center"
            },
            {
                data: "ModuleName",
                title: "Module",
                width: "150px"
            },
            {
                data: "RowId",
                title: "Row Id",
                width: "100px",
                className: "text-center"
            },
            {
                data: "ReferenceNo",
                title: "Reference No (InvoiceNo / VoucherNo / Ref.No)"
            },
            {
                data: "OldAmount",
                title: "Old Amount",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "NewAmount",
                title: "New Amount",
                className: "text-end",
                render: d => formatINR2(d)
            },
            {
                data: "ImpactAmount",
                title: "Impact Amount",
                className: "text-end fw-bold",
                render: function (data) {
                    const amount = parseFloat(data || 0);

                    return `
                        <span class="${amount >= 0 ? 'text-success' : 'text-danger'}">
                            ${formatINR2(amount)}
                        </span>
                    `;
                }
            },
            {
                data: "ModifiedDateTime_Format",
                title: "Modified Datetime",
                width: "180px"
            },
            {
                data: "ModifiedBy",
                title: "Modified By",
                width: "150px"
            }
        ],

        language: {
            emptyTable: "No modification records found for the selected period."
        },

        initComplete: function () {
            const api = this.api();

            $(api.table().header())
                .find("th")
                .addClass("bg-warning-subtle text-dark");

            api.columns.adjust();
        },

    });
}

/* ═══════════════════════════════════════════════════
   RENDER SUMMARY CARDS
═══════════════════════════════════════════════════ */
function renderModificationSummary(summary) {

    if (!summary) return;

    $("#modSummaryCards").html(`
        <div class="col-md-3">
            <div class="card border-primary">
                <div class="card-body">
                    <div class="small text-muted">Total Modifications</div>
                    <div class="fs-4 fw-bold">
                        ${summary.TotalModifications || 0}
                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="card border-secondary">
                <div class="card-body">
                    <div class="small text-muted">Total Old Amount</div>
                    <div class="fs-5 fw-bold">
                        ${formatINR2(summary.TotalOldAmount)}
                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="card border-info">
                <div class="card-body">
                    <div class="small text-muted">Total New Amount</div>
                    <div class="fs-5 fw-bold">
                        ${formatINR2(summary.TotalNewAmount)}
                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-3">
            <div class="card border-success">
                <div class="card-body">
                    <div class="small text-muted">Total Impact Amount</div>
                    <div class="fs-5 fw-bold">
                        ${formatINR2(summary.TotalImpactAmount)}
                    </div>
                </div>
            </div>
        </div>
    `);

    $("#modSummarySection").show();
}


/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewModificationReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        ModuleId: Number($("#ddlModule").val()) || 0,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration,
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };
    console.log("report payload:", payload);

    const url = actionName === "downloadExcel"
        ? "/api/Report_Modification/DownloadModificationReport_Excel"
        : "/api/Report_Modification/DownloadModificationReport";

    downloadReport(url, payload, 
        loaderMsg = actionName === "downloadExcel" ? "Generating Modification Excel Report..." : "Generating Modification PDF Report...", 
        failedMsg = "Failed to generate Modification Report");
}

/* ═══════════════════════════════════════════════════
   HELPERS
═══════════════════════════════════════════════════ */
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

function loadModules() {
    $.ajax({
        url: "/api/Report_Modification/GetModuleMasterForModificationReport",
        type: "GET",
        dataType: "json",
        success: function (res) {
            let options = '<option value="0">-- All Modules --</option>';
            if (res.status === "Success" && res.data?.length > 0) {
                $.each(res.data, function (index, item) {
                    options += `<option value="${item.Id}">${item.Name}</option>`;
                });
            }
            $("#ddlModule").html(options);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load modules");
            $("#ddlModule").html('<option value="">-- All Modules --</option>');
        }
    });
}


