/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr. No." },
    { key: "MobileNo", label: "Mobile No" },
    { key: "TemplateName", label: "Template" },
    { key: "MsgSample", label: "Message" },
    { key: "Status", label: "Status" },
    { key: "ErrorMessage", label: "Error" },
    { key: "DeductedAmount", label: "Amount" },
    { key: "RefundAmount", label: "Refund" },
    { key: "SentAt", label: "Sent At" },
    { key: "FailedAt", label: "Failed At" },
    { key: "ActionBy", label: "Action By" },
    { key: "ReferenceId", label: "Reference ID" }
];
let selectedColumns = REPORT_COLUMNS.map(c => c.key);

/* ═══════════════════════════════════════════════════
   OTHER SETTINGS
═══════════════════════════════════════════════════ */
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "whatsappsPrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {
    loadWhatsappTemplateDropdown();

    resetGrid(); //loadWhatsappGrid();
    loadColumnCheckboxes();

    $("#quickRange").on("change", function () {
        console.log("quickRange triggered");

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

        loadWhatsappGrid();
    });    

    /* ── dropdown change → reload sub category ── */
    $("#fromDate, #toDate, #whatsappStatus, #ddlWhatsappTemplate").on("change", function () {
        // If user manually edits date → switch dropdown to custom
        if ($("#quickRange").val() !== "customrange") {
            $("#quickRange").val("customrange");
            // Optional: update variable also
            selectedMonthDuration = "customrange";
        }
        loadWhatsappGrid();
    });

    $(document).on("keydown", "#whatsappMobileNo, #whatsappSearch", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            loadWhatsappGrid();
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
        loadWhatsappGrid();
    });

    // ─────────────────────────────────────────────
    // RESET FILTER
    // ─────────────────────────────────────────────
    $("#btnResetFilter").on("click", function () {
        resetGrid();
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
        loadWhatsappGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

function resetGrid() {
    const now = new Date();

    // Reset duration
    $("#quickRange").val("thismonth");

    // Reset date range
    const fromDate = new Date(now.getFullYear(), now.getMonth(), 1);
    const toDate = new Date(now.getFullYear(), now.getMonth() + 1, 0);
    $("#fromDate").val(formatDate2(fromDate));
    $("#toDate").val(formatDate2(toDate));

    // Reset status
    $("#whatsappStatus").val("").trigger("change");

    // Reset mobile
    $("#whatsappMobileNo").val("");

    // Reset template
    $("#ddlWhatsappTemplate").val("").trigger("change");

    // Reset search
    $("#whatsappSearch").val("");

    // Update title
    updateWhatsappReportTitle();

    // Reload grid
    loadWhatsappGrid();
}
// ─────────────────────────────────────────────
// UPDATE REPORT TITLE
// ─────────────────────────────────────────────
function updateWhatsappReportTitle() {
    const selectedMonth = $("#whatsappMonth").val(); // yyyy-MM
    if (!selectedMonth) {
        $("#whatsappReportTitle").text("Whatsapp Report");
        return;
    }

    const [year, month] = selectedMonth.split("-");
    const date = new Date(
        parseInt(year),
        parseInt(month) - 1,
        1
    );

    const monthYear = date.toLocaleString("en-US", {
        month: "long",
        year: "numeric"
    });

    $("#whatsappReportTitle").text(`Whatsapp Report - ${monthYear}`);
}


/* ═══════════════════════════════════════════════════
   DROPDOWNS
═══════════════════════════════════════════════════ */
function loadWhatsappTemplateDropdown(selectedId = 0) {
    $.ajax({
        url: "/api/Report_WhatsappMsg/GetWhatsappTemplateList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ EventId: 0 }),
        success: function (res) {
            const dd = $("#ddlWhatsappTemplate");
            dd.empty().append(`<option value="0">-- All --</option>`);
            if (res.status === "Success" && res.data.length > 0) {
                res.data.forEach(item => {
                    dd.append(`<option value="${item.EventId}" ${selectedId == item.EventId ? "selected" : ""}>${item.TempName_Title}</option>`);
                });
            }
        },
        error: (xhr, s, e) => console.error("Template dropdown failed", e)
    });
}

/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadWhatsappGrid() {

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: $("#quickRange").val() || null,
        Status: $("#whatsappStatus").val() || null,
        CompanyId: 0,
        MobileNo: $("#whatsappMobileNo").val() || null,
        EventId: $("#ddlWhatsappTemplate").val() || null,
        Search: $("#whatsappSearch").val() || null
    };

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading WhatsApp message logs...
        </div>
    `);

    console.log("WhatsApp Message Log Payload:", payload);

    $.ajax({
        url: "/api/Report_WhatsappMsg/Report_WhatsApp_MessageLog_GRID",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),

        success: function (res) {

            console.log("WhatsApp Message Log res:", res);

            if (res.status !== "Success") {
                $("#tableMain").html("");
                Swal.fire("Error", res.message, "error");
                return;
            }

            const rows = res.data?.rows || [];
            const summary = res.data?.summary || {};

            console.log("WhatsApp Message Log Rows:", rows);
            console.log("WhatsApp Message Log Summary:", summary);

            $("#tableMain").html(`
                <table id="tblWhatsappMessageLog"
                       class="table table-striped table-bordered w-100">

                    <tfoot class="table-secondary fw-bold">
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
                            <th></th>
                            <th></th>
                        </tr>
                    </tfoot>

                </table>
            `);

            renderWhatsappMessageLogTable(rows, summary);
        },

        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch WhatsApp Message Log Failed");
            $("#tableMain").html(`
                <div class="alert alert-danger">
                    Failed to load WhatsApp message logs. Please try again.
                </div>
            `);
        }
    });
}

function renderWhatsappMessageLogTable(rows, summary) {

    if ($.fn.DataTable.isDataTable("#tblWhatsappMessageLog")) {
        $("#tblWhatsappMessageLog").DataTable().clear().destroy();
    }

    rows = rows || [];
    summary = summary || {};

    const totalRows = parseInt(summary.TotalRows) || 0;
    const totalDeductedAmount = parseFloat(summary.TotalDeductedAmount) || 0;
    const totalRefundAmount = parseFloat(summary.TotalRefundAmount) || 0;

    $("#tblWhatsappMessageLog").DataTable({

        data: rows,

        autoWidth: false,
        responsive: false,

        paging: true,
        pageLength: 25,

        lengthMenu: [
            [10, 25, 50, 100, -1],
            [10, 25, 50, 100, "All"]
        ],

        scrollY: "60vh",
        scrollX: true,
        scrollCollapse: true,

        order: [
            [0, "desc"]
        ],

        columns: [

            // 0 - ID
            {
                data: "Id",
                title: "#",
                width: "50px",
                className: "text-center",

                render: function (data) {
                    return data || "-";
                }
            },

            // 1 - Mobile No.
            {
                data: "MobileNo",
                title: "Mobile No.",
                width: "140px",
                className: "text-start",

                render: function (data) {
                    return data || "-";
                }
            },

            // 2 - Template
            {
                data: "TempName_Title",
                title: "Template",
                width: "130px",
                className: "text-start",
                render: function (data, type, row) {
                    const title = data || "-";
                    return `
                        <div class="fw-semibold text-dark">
                            ${$("<div>").text(title).html()}
                        </div>
                    `;
                }
            },

            // 3 - Message
            {
                data: null,
                title: "Message",
                width: "240px",
                className: "text-start align-middle message-column",

                createdCell: function (td) {
                    $(td).css({
                        "padding": "6px 10px",
                        "vertical-align": "middle",
                        "width": "240px",
                        "min-width": "240px",
                        "max-width": "240px"
                    });
                },

                render: function (data, type, row) {

                    const template = row.MsgTemplate || "-";
                    const sample = row.MsgSample || "-";

                    const safeTemplate = $("<div>").text(template).html();
                    const safeSample = $("<div>").text(sample).html();

                    return `
                        <div class="message-preview"
                             data-bs-toggle="popover"
                             data-bs-placement="top"
                             data-bs-html="true"
                             data-bs-trigger="hover focus"
                             data-bs-custom-class="message-popover"
                             data-bs-content="
                                <div class='message-popover-section'>
                                    <div class='message-popover-label'>
                                        <i class='fa fa-comment-dots me-1'></i> Whatsapp Message
                                    </div>

                                    <div class='message-popover-text'>
                                        ${safeSample}
                                    </div>
                                </div>
                             ">

                            <div class="message-row message-sample">
                                <span class="message-label">
                                    <i class="fa fa-comment-dots"></i>
                                </span>

                                <span class="message-text" title="${safeSample}">
                                    ${safeSample}
                                </span>
                            </div>

                        </div>
                    `;
                }
            },

            // 4 - Status
            {
                data: "Status",
                title: "Status",
                width: "100px",
                className: "text-center",

                render: function (data) {

                    const status = (data || "").toLowerCase();

                    if (status === "sent") {
                        return `
                            <span class="badge bg-success-subtle text-success border border-success-subtle px-2 py-1">
                                <i class="fa fa-check-circle me-1"></i>Sent
                            </span>
                        `;
                    }

                    if (status === "failed") {
                        return `
                            <span class="badge bg-danger-subtle text-danger border border-danger-subtle px-2 py-1">
                                <i class="fa fa-times-circle me-1"></i>Failed
                            </span>
                        `;
                    }

                    if (status === "pending") {
                        return `
                            <span class="badge bg-warning-subtle text-warning-emphasis border border-warning-subtle px-2 py-1">
                                <i class="fa fa-clock me-1"></i>Pending
                            </span>
                        `;
                    }

                    if (status === "refunded") {
                        return `
                            <span class="badge bg-light text-dark border px-2 py-1">
                                Refunded
                            </span>
                        `;
                    }

                    return `
                        <span class="badge bg-secondary-subtle text-secondary border px-2 py-1">
                            ${$("<div>").text(data || "-").html()}
                        </span>
                    `;
                }
            },

            // 5 - ErrorMessage
            {
                data: "ErrorMessage",
                title: "Error Message",
                width: "160px",
                className: "text-start",

                render: function (data) {
                    return data || "-";
                }
            },

            // 5 - Deducted
            {
                data: "DeductedAmount",
                title: "Deducted",
                width: "110px",
                className: "text-end",

                render: function (data, type) {

                    const amount = parseFloat(data) || 0;

                    if (type === "sort" || type === "type") {
                        return amount;
                    }

                    if (amount <= 0) {
                        return `<span class="text-muted">-</span>`;
                    }

                    return `
                        <span class="fw-semibold text-danger">
                            ${formatINR2(amount)}
                        </span>
                    `;
                }
            },

            // 6 - Refund
            {
                data: "RefundAmount",
                title: "Refund",
                width: "110px",
                className: "text-end",

                render: function (data, type) {

                    const amount = parseFloat(data) || 0;

                    if (type === "sort" || type === "type") {
                        return amount;
                    }

                    if (amount <= 0) {
                        return `<span class="text-muted">-</span>`;
                    }

                    return `
                        <span class="fw-semibold text-success">
                            ${formatINR2(amount)}
                        </span>
                    `;
                }
            },

            // 7 - Sent At
            {
                data: "SentAt",
                title: "Sent At",
                width: "150px",
                className: "text-center",

                render: function (data) {
                    return data && data !== "-" ? data : "-";
                }
            },

            // 8 - Failed At
            {
                data: "FailedAt",
                title: "Failed At",
                width: "150px",
                className: "text-center",

                render: function (data) {
                    return data && data !== "-" ? data : "-";
                }
            },

            // 10 - Action By
            {
                data: "ActionByName",
                title: "Action By",
                width: "140px",
                className: "text-start",

                render: function (data) {
                    return data || "-";
                }
            },

            // 11 - Reference ID
            {
                data: "ReferenceId",
                title: "ReferenceID",
                width: "200px",
                className: "text-start",

                render: function (data) {

                    if (!data || data === "-") {
                        return "-";
                    }

                    const safe = $("<div>").text(data).html();

                    return `
                        <span class="small text-muted"
                              title="${safe}">
                            ${safe}
                        </span>
                    `;
                }
            }
        ],

        language: {
            emptyTable: "No WhatsApp message logs found for the selected filters.",
            zeroRecords: "No matching WhatsApp message logs found."
        },

        footerCallback: function (tfoot) {
            const $cells = $(tfoot).find("th");
            /* Clear all footer cells */
            $cells.html("");

            /* 0 - Total Rows */
            $cells.eq(0).html(`<strong>${totalRows}</strong>`);

            /* 3 - Total Label */
            $cells.eq(3).html(`<strong>Total</strong>`);

            /* 5 - Total Deducted */
            $cells.eq(6).html(`<strong class="text-danger">${formatINR2(totalDeductedAmount)}</strong>`);

            /* 6 - Total Refund */
            $cells.eq(7).html(`<strong class="text-success">${formatINR2(totalRefundAmount)}</strong>`);

            /* Alignment */
            $cells.addClass("align-middle");
        },

        initComplete: function () {

            const api = this.api();

            /* Header styling */
            $(api.table().header())
                .find("th")
                .addClass("bg-warning-subtle text-dark");

            /* Initialize Bootstrap popovers */
            $(api.table().body())
                .find('[data-bs-toggle="popover"]')
                .each(function () {
                    new bootstrap.Popover(this);
                });
        },

        drawCallback: function () {

            /*
             * Reinitialize popovers after
             * pagination / redraw
             */
            $(this.api().table().body())
                .find('[data-bs-toggle="popover"]')
                .each(function () {

                    if (!bootstrap.Popover.getInstance(this)) {
                        new bootstrap.Popover(this);
                    }

                });
        }
    });
}

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewWhatsappReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: $("#quickRange").val() || null,
        Status: $("#whatsappStatus").val() || null,
        CompanyId: 0,
        MobileNo: $("#whatsappMobileNo").val() || null,
        TemplateName: $("#ddlWhatsappTemplate").val() || null,
        Search: $("#whatsappSearch").val() || null,

        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_WhatsappMsg/Report_WhatsApp_MessageLog_Excel"
        : "/api/Report_WhatsappMsg/Report_WhatsApp_MessageLog_Pdf";

    downloadReport(url, payload, 
        loaderMsg = actionName === "downloadExcel" ? "Generating Whatsapp Msg Excel Report..." : "Generating Whatsapp Msg PDF Report...", 
        failedMsg = "Failed to generate Whatsapp Msg Report");
}

/* ═══════════════════════════════════════════════════
   HELPERS
═══════════════════════════════════════════════════ */
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

