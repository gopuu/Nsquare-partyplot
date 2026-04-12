/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No" },
    { key: "InvoiceDate", label: "Date" },
    { key: "InvoiceNo", label: "Invoice No" },
    { key: "SupplierName", label: "Supplier" },
    { key: "GrandTotal", label: "Grand Total" },
    { key: "PaidAmount", label: "Paid Amount" },
    { key: "Outstanding", label: "Outstanding" }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "purchasePrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadSupplierDropdown();
    loadPurchaseGrid();
    loadColumnCheckboxes();

    /* ── Supplier filter live reload ── */
    $("#ddlSupplier").on("change", function () {
        loadPurchaseGrid();
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
        loadPurchaseGrid();
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
        loadPurchaseGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        $("#ddlSupplier").val("0");
        setCurrentMonthDates();
        loadPurchaseGrid();
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
        loadPurchaseGrid();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });

    $(document).on("click", ".btn-view-invoice", function () {
        const purchaseId = $(this).data("id");
        const invoiceNo = $(this).data("invoice");
        loadPurchaseInvoice(purchaseId, invoiceNo);
    });

    $("#btnDownloadInvoice").on("click", function () {
        const purchaseId = $(this).data("purchaseid");
        const invoiceNo = $(this).data("invoiceno");

        console.log("PurchaseId:", purchaseId);
        console.log("InvoiceNo:", invoiceNo);
        downloadPurchaseInvoice(purchaseId, invoiceNo);
    });

    $("#btnDownloadInvoice_Excel").on("click", function () {
        const purchaseId = $(this).data("purchaseid");
        const invoiceNo = $(this).data("invoiceno");

        console.log("PurchaseId:", purchaseId);
        console.log("InvoiceNo:", invoiceNo);
        downloadPurchaseInvoice(purchaseId, invoiceNo, "excel");
    });
});

function loadPurchaseInvoice(purchaseId, invoiceNo) {

    const payload = {
        PurchaseId: purchaseId,
        InvoiceNo: invoiceNo.toString()
    };

    $.ajax({
        url: "/api/Report_Purchase/Report_GetPurchaseInvoice_GRID",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            const inv = res.data;

            // HEADER
            $("#inv_no").text(inv.InvoiceNo || "-");
            $("#inv_date").text(inv.InvoiceDate_Format || "-");
            $("#inv_supplier").text(inv.SupplierName || "-");
            $("#inv_company").text(inv.SupplierCompany || "-");
            $("#inv_phone").text(inv.SupplierPhone || "-");
            $("#inv_po").text(inv.PONo || "-");

            // store values for download button
            $("#btnDownloadInvoice")
                .data("purchaseid", inv.Id)
                .data("invoiceno", inv.InvoiceNo);

            $("#btnDownloadInvoice_Excel")
                .data("purchaseid", inv.Id)
                .data("invoiceno", inv.InvoiceNo);

            renderInvoiceItems(inv.JSON_PurchaseItems || []);

            // totals
            $("#t_subtotal").text(formatINR2(inv.SubTotal_TotalDiscount_Amount));
            $("#t_charges").text(formatINR2(inv.SubTotal_OtherCharges_Amount));
            $("#t_taxes").text(formatINR2(inv.SubTotal_OtherTaxes_Amount));
            $("#t_discount").text(formatINR2(inv.TotalDiscountInInvoice));
            $("#t_total").text(formatINR2(inv.GrandTotal_Amount));

            $("#purchaseInvoiceModal").modal("show");
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load invoice.");
        }
    });
}
function renderInvoiceItems(items) {
    let html = "";
    items.forEach((x, i) => {
        html += `
        <tr>
            <td class="text-center">${i + 1}</td>
            <td>${x.RawMaterial}</td>
            <td class="text-center">${x.Qty}</td>
            <td class="text-center">${x.UnitName}</td>
            <td class="text-end">${formatINR2(x.Price)}</td>
            <td class="text-end">${formatINR2(x.BaseAmount)}</td>
            <td class="text-end">${x.CGST_Amount} (${x.CGST}%)</td>
            <td class="text-end">${x.SGST_Amount} (${x.SGST}%)</td>
            <td class="text-end">${x.IGST_Amount} (${x.IGST}%)</td>
            <td class="text-end">${formatINR2(x.TotalGST)}</td>
            <td class="text-end fw-semibold">${formatINR2(x.TotalAmount)}</td>
        </tr>
        `;
    });

    $("#invoiceItemsTable tbody").html(html);
}


/* ═══════════════════════════════════════════════════
   LOAD SUPPLIER DROPDOWN
═══════════════════════════════════════════════════ */
function loadSupplierDropdown(selectedId = 0) {
    var payload = { Id: 0 };
    $.ajax({
        url: "/api/Supplier/GetSupplierList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("res Supplier:", res);
            const dropdown = $("#ddlSupplier");
            dropdown.empty();
            dropdown.append(`<option value="0">-- All Suppliers --</option>`);

            if (res.status === "Success" && res.data.length > 0) {
                res.data.forEach(item => {
                    dropdown.append(
                        `<option value="${item.Id}">${item.CompanyName && item.CompanyName.trim() !== "" ? item.CompanyName : item.Name}</option>`
                    );
                });
            }
        },
        error: function (xhr, status, error) {
            console.error("Failed to load Supplier dropdown", xhr, status, error);
        }
    });
}

/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadPurchaseGrid() {

    const payload = {
        SupplierId: parseInt($("#ddlSupplier").val()) || 0,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
    };

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2" role="status"></div>
            Loading Purchase data...
        </div>
    `);

    $.ajax({
        url: "/api/Report_Purchase/GetPurchaseReportGrid",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            $("#tableMain").html(`
                <table id="tblPurchase" class="table table-striped table-bordered w-100">
                    <tfoot>
                        <tr>
                            <th></th><th></th><th></th>
                            <th></th><th></th><th></th><th></th>
                        </tr>
                    </tfoot>
                </table>
            `);

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            renderPurchaseTable(res.data);
        },
        error: function (xhr, status, error) {
            console.error("Fetch Purchase Data Failed", xhr, status, error);
            $("#tableMain").html(`
                <div class="alert alert-danger">
                    Failed to load data. Please try again.
                </div>
            `);
        }
    });
}

/* ═══════════════════════════════════════════════════
   RENDER TABLE
═══════════════════════════════════════════════════ */
function renderPurchaseTable(data) {

    if ($.fn.DataTable.isDataTable("#tblPurchase")) {
        $("#tblPurchase").DataTable().clear().destroy();
    }

    $("#tblPurchase").DataTable({
        data: data,
        responsive: true,
        pageLength: 25,
        scrollX: true,
        order: [[1, "asc"]],

        paging: false,
        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows

        columns: [
            {
                data: "InvoiceNo",
                title: "Invoice No",
                className: "text-center",
                render: function (data, type, row) {

                    if (!data) return "-";

                    return `
                        <a href="javascript:void(0)" 
                           class="link-primary fw-semibold btn-view-invoice"
                           data-id="${row.PurchaseId}"
                           data-invoice="${data}">
                           #${data}
                        </a>
                    `;
                }
            },
            {
                data: "InvoiceDate",
                title: "Date",
                width: "110px"
            },
            {
                data: "SupplierName",
                title: "Supplier"
            },
            {
                data: "GrandTotal",
                title: "Grand Total",
                className: "text-end fw-semibold",
                render: d => formatINR2(d)
            },
            {
                data: "PaidAmount",
                title: "Paid Amount",
                className: "text-end text-success",
                render: d => formatINR2(d)
            },
            {
                data: "Outstanding",
                title: "Outstanding",
                className: "text-end",
                render: function (data) {
                    const val = parseFloat(data) || 0;
                    const cls = val > 0 ? "text-danger fw-semibold" : "text-muted";
                    return `<span class="${cls}">${formatINR2(val)}</span>`;
                }
            },
            // hidden
            { data: "PurchaseId", visible: false },
            { data: "SupplierId", visible: false }
        ],

        /* ── Footer totals ── */
        footerCallback: function (tfoot, data, start, end, display) {

            const api = this.api();
            const sumCol = idx =>
                api.column(idx, { page: "all" })
                    .data()
                    .reduce((acc, val) => acc + (parseFloat(val) || 0), 0);

            const $cells = $(tfoot).find("th");

            $cells.eq(0).html("<strong>TOTAL</strong>");
            $cells.eq(1).html("");
            $cells.eq(2).html("");
            //$cells.eq(3).html("");
            $cells.eq(3).html(`<strong>${formatINR2(sumCol(4))}</strong>`);
            $cells.eq(4).html(`<strong class="text-success">${formatINR2(sumCol(5))}</strong>`);
            $cells.eq(5).html(`<strong class="text-danger">${formatINR2(sumCol(6))}</strong>`);
        },

        /* ── Highlight outstanding rows ── */
        createdRow: function (row, rowData) {
            if (parseFloat(rowData.outstanding) > 0) {
                $(row).find("td").eq(6).addClass("table-danger");
            }
        },

        language: {
            emptyTable: "No purchase records found for the selected period."
        },
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },

    });
}

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewPurchaseReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        SupplierId: parseInt($("#ddlSupplier").val()) || 0,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration,
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Report_Purchase/DownloadPurchaseReport_Excel"
        : "/api/Report_Purchase/DownloadPurchaseReport";

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
            link.download = `Purchase_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            Swal.close();
            console.error("Download Purchase Report Failed", xhr, status, error);
            Swal.fire("Error", "Failed to generate report. Please try again.", "error");
        }
    });
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