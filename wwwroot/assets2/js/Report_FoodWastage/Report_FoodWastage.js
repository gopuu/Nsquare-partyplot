let selectedMonthDuration = "thismonth";
const HEADER_KEY = "foodWastagePrintWithHeader";

/* ═══════════════════════════════════════════════════
   INIT
═══════════════════════════════════════════════════ */
document.addEventListener("DOMContentLoaded", function () {

    setCurrentMonthDates();
    loadFoodWastageGrid();

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
            case "days":
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
        loadFoodWastageGrid();
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
        loadFoodWastageGrid();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        setCurrentMonthDates();
        loadFoodWastageGrid();
    });

    /* ── With Header ── */
    const saved = sessionStorage.getItem(HEADER_KEY);
    if (saved !== null)
        document.getElementById("toggleWithHeader").checked = (saved === "true");

    document.getElementById("toggleWithHeader").addEventListener("change", function () {
        sessionStorage.setItem(HEADER_KEY, this.checked);
    });

    $(document).on('click', '.wastage-click', function (e) {
        e.stopPropagation();

        const bookingId = $(this).data("booking");
        if (!bookingId) return; // safety check

        console.log("Wastage clicked:", bookingId);
        openFoodWastageDetailModal(bookingId);
    });


    $("#btnDownloadFoodWastageDetailPDF").click(function () {
        downloadFoodWastageDetailReport("downloadPDF");
    });

    $("#btnDownloadFoodWastageDetailExcel").click(function () {
        downloadFoodWastageDetailReport("downloadExcel");
    });

    $('#modalFoodWastageDetail').on('hidden.bs.modal', function () {
        loadFoodWastageGrid();
    });
});

let selectedBookingId = 0;
function openFoodWastageDetailModal(bookingId) {
    selectedBookingId = bookingId;
    const payload = {
        BookingId: bookingId
    };

    showTempLoader("Fetching data..");

    $.ajax({
        url: "/api/Report_FoodWastage/Report_GetFoodWastageDetail_GRID",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            Swal.close();

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }
            const data = res.data;

            renderFoodWastageDetailHeader(data);
            renderFoodWastageDetailGrid(data.Items);

            $("#modalFoodWastageDetail").modal("show");
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load details");
        }
    });
}
function renderFoodWastageDetailHeader(data) {

    $("#foodWastageDetailInfo").html(`

        <div class="row">

            <div class="col-md-3">
                <strong>Booking ID</strong><br>
                #${data.BookingId}
            </div>

            <div class="col-md-3">
                <strong>Date</strong><br>
                ${data.EventDate || "-"}
            </div>

            <div class="col-md-3">
                <strong>Shift</strong><br>
                ${data.ShiftCode || "-"}
            </div>

            <div class="col-md-3">
                <strong>Client</strong><br>
                ${data.ClientName || "-"}
            </div>

        </div>

    `);
}
function renderFoodWastageDetailGrid(items) {

    if ($.fn.DataTable.isDataTable("#tblFoodWastageDetail")) {
        $("#tblFoodWastageDetail").DataTable().destroy();
    }

    $("#tblFoodWastageDetail").DataTable({

        data: items,
        paging: false,
        searching: false,
        info: false,

        columns: [

            {
                data: "SrNo",
                className: "text-center",
                width: "60px"
            },

            {
                data: "MenuItemName"
            },

            {
                data: "WastageKg",
                className: "text-end",
                render: function (data) {

                    const val = parseFloat(data) || 0;

                    const cls = val > 0
                        ? "text-danger fw-semibold"
                        : "text-muted";

                    return `<span class="${cls}">${val.toFixed(2)} kg</span>`;
                }
            }

        ],

        footerCallback: function () {

            const api = this.api();

            const total = api
                .column(2)
                .data()
                .reduce((a, b) => a + (parseFloat(b) || 0), 0);

            $(api.column(2).footer()).html(
                `<strong class="text-danger">${total.toFixed(2)} kg</strong>`
            );
        },
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },

    });
}
function downloadFoodWastageDetailReport(actionName="pdf") {
    const payload = {
        BookingId: selectedBookingId,
        ActionBy: parseInt(sessionStorage.getItem('userId') || 0),
        WithHeader: $("#toggleWithHeader").prop("checked")
    };

    let apiUrl = "";
    if (actionName === "downloadPDF")
        apiUrl = "/api/Report_FoodWastage/DownloadFoodWastageDetailReport_Pdf";

    if (actionName === "downloadExcel")
        apiUrl = "/api/Report_FoodWastage/DownloadFoodWastageDetailReport_Excel";

    showTempLoader("Generating Report..");

    $.ajax({
        url: apiUrl,
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
                link.download = `FoodWastageDetail_Report_${new Date().toISOString().replace(/[:.-]/g, "")}.pdf`;
            }
            else if (actionName === "downloadExcel") {
                link.download = `FoodWastageDetail_Report_${new Date().toISOString().replace(/[:.-]/g, "")}.xlsx`;
            }

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error , "Failed to generate detail report");
        }
    });
}


/* ═══════════════════════════════════════════════════
   LOAD GRID
═══════════════════════════════════════════════════ */
function loadFoodWastageGrid() {

    const payload = {
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
    };

    $("#tableMain").html(`
        <div class="text-center py-5 text-muted">
            <div class="spinner-border spinner-border-sm me-2"></div>
            Loading Food Wastage data...
        </div>
    `);

    $.ajax({
        url: "/api/Report_FoodWastage/Report_GetFoodWastage_GRID",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            $("#tableMain").html(`
                <table id="tblFoodWastage" class="table table-striped table-bordered w-100">
                    <tfoot>
                        <tr>
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

            renderFoodWastageTable(res.data);
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to load food wastage data")
            $("#tableMain").html(`
                <div class="alert alert-danger">
                    Failed to load food wastage data.
                </div>
            `);
        }
    });
}

/* ═══════════════════════════════════════════════════
   RENDER TABLE
═══════════════════════════════════════════════════ */
function renderFoodWastageTable(data) {

    if ($.fn.DataTable.isDataTable("#tblFoodWastage")) {
        $("#tblFoodWastage").DataTable().clear().destroy();
    }

    $("#tblFoodWastage").DataTable({

        data: data,
        responsive: true,
        pageLength: 25,
        order: [[1, "asc"]],
        paging: false,

        scrollY: "60vh",
        scrollCollapse: true,
        scrollX: true,

        columns: [

            {
                data: "SrNo",
                title: "#",
                width: "50px",
                className: "text-center"
            },

            {
                data: "EventDate",
                title: "Date",
                width: "110px"
            },

            {
                data: "ShiftCode",
                title: "Shift"
            },

            {
                data: "ClientName",
                title: "Booking Client"
            },

            {
                data: "WastageKg",
                title: "Wastage (Kg)",
                className: "text-end",
                render: function (data, type, row) {
                    const val = parseFloat(data) || 0;

                    // ❌ disable click if 0
                    if (val <= 0) {
                        return `<span class="text-muted">${val.toFixed(2)} kg</span>`;
                    }

                    return `
                        <span class="text-danger fw-semibold wastage-click"
                              style="cursor:pointer"
                              data-booking="${row.BookingId}">
                            ${val.toFixed(2)} kg
                        </span>
                    `;
                }
            },

            { data: "BookingId", visible: false }

        ],

        footerCallback: function (tfoot) {

            const api = this.api();

            const total = api
                .column(4, { page: "all" })
                .data()
                .reduce((a, b) => a + (parseFloat(b) || 0), 0);

            const $cells = $(tfoot).find("th");

            $cells.eq(0).html("<strong>TOTAL</strong>");
            $cells.eq(1).html("");
            $cells.eq(2).html("");
            $cells.eq(3).html("");
            $cells.eq(4).html(
                `<strong class="text-danger">${total.toFixed(2)} kg</strong>`
            );
        },

        language: {
            emptyTable: "No food wastage records found."
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
function ViewFoodWastageReport(actionName) {

    const payload = {
        ActionBy: parseInt(sessionStorage.getItem('userId') || 0),
        WithHeader: $("#toggleWithHeader").prop("checked"),
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null,
        FilterDuration: selectedMonthDuration
    };

    let apiUrl = "";

    if (actionName === "downloadPDF")
        apiUrl = "/api/Report_FoodWastage/DownloadFoodWastageReport_Pdf";

    if (actionName === "downloadExcel")
        apiUrl = "/api/Report_FoodWastage/DownloadFoodWastageReport_Excel";

    showTempLoader("Generating Report..");

    $.ajax({
        url: apiUrl,
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
                link.download = `FoodWastage_Report_${new Date().toISOString().replace(/[:.-]/g, "")}.pdf`;
            } else if (actionName === "downloadExcel") {
                link.download = `FoodWastage_Report_${new Date().toISOString().replace(/[:.-]/g, "")}.xlsx`;
            }

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, error, status) {
            handleApiError(xhr, error, status, "Failed to generate summary report.");
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


