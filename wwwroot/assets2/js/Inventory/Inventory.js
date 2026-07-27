/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr. No."},
    { key: "CategoryName", label: "Category"},
    { key: "RawMaterialName", label: "Raw Material"},
    { key: "UnitName", label: "Unit"},
    { key: "CurrentQty", label: "Stock Available"}
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "expensePrintWithHeader";

var unitMaster = [];

$(document).ready(function () {
    fetchData();
    GetCategoryMasterData();
    GetRawMaterialMasterData();  // Load ALL initially
    GetUnitMasterData();
    loadColumnCheckboxes();

    AppSignalR.subscribePurchase((data) => {
        //console.log("SignalR: Purchase Updated");
        fetchData();
        GetRawMaterialMasterData();  // Load ALL initially
    });

    var rawMaterialId = 0;

    // When Category changes → Fetch Raw Materials of that category
    $("#filterItemCategory").on("change", function () {
        let categoryId = $(this).val();
        GetRawMaterialMasterData(categoryId);
        fetchData();
    });

    // On Raw Material Change → Load allowed units only
    $(document).on("change", "#filterRawMaterial", function () {
        // 1. GET SELECTED RAW MATERIAL ID
        rawMaterialId = $(this).val();
        fetchData();

        let selectedOption = $(this).find("option:selected");
        let jsonUnits = selectedOption.attr("data-unitIdList");

        // Reset Unit dropdown
        $("#filterUnit").empty().append(`<option value="0">All</option>`);

        if (!jsonUnits || jsonUnits === "[]") {
            return; // no units found → leave only "All"
        }

        // Convert encoded quotes
        jsonUnits = jsonUnits.replace(/&quot;/g, '"');

        let allowedUnits = [];
        try {
            allowedUnits = JSON.parse(jsonUnits); // array of { UnitId }
        }
        catch (e) {
            console.error("Invalid unit JSON:", jsonUnits);
            return;
        }

        // Filter GLOBAL unitMaster (you already have it loaded)
        allowedUnits.forEach(u => {
            let unitObj = unitMaster.find(x => x.Id == u.UnitId);
            if (unitObj) {
                $("#filterUnit").append(`
                        <option value="${unitObj.Id}">
                            ${unitObj.UnitName}
                        </option>
                    `);
            }
        });
    });

    // When Unit changes → Fetch Inventory
    $("#filterUnit").on("change", function () {
        let unitId = $(this).val();
        fetchData();
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
        fetchData();
    });

    $("#btnResetFilter").on("click", function () {
        // Reset Category → "All"
        $("#filterItemCategory").val("0");

        // Reload Raw Materials (All)
        GetRawMaterialMasterData(0);

        // Reset Unit Dropdown
        $("#filterUnit").empty().append(`<option value="0">All</option>`);
        GetUnitMasterData();

        // Optionally reload inventory list
        rawMaterialId = 0;
        fetchData();
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
        fetchData();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });
});

//history btn
$(document).on("click", ".btn-history", function () {
    let id = $(this).data("id");
    let name = $(this).data("name");

    $("#historyItemName").text(name);
    loadHistory(id);
});
function loadHistory(id) {
    $("#historyTimeline").html("<p>Loading...</p>");

    var payload = {
        RawMaterialId: id
    };

    console.log("payload:", payload);

    $.ajax({
        url: "/api/Inventory/GetStockOutHistory",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("History response:", res);
            if (res.status === "Success" && res.data.length == 0) {
                $("#historyTimeline").html("<p>No history found</p>");
            }
            else {
                let html = `<div class="timeline">`;

                res.data.forEach(x => {

                    html += `
                    <div class="timeline-item">

                        <div class="timeline-dot"></div>

                        <div class="timeline-content">

                            <div class="fw-bold">
                                ${x.Quantity_In} ${x.UnitName} (In) / ${x.Quantity_Out} ${x.UnitName} (Out)
                            </div>

                            <div class="text-muted small">
                                ${x.ModifiedOn2}
                            </div>

                            <div class="small">
                                ${x.Remarks || ""}
                            </div>

                            <div class="small text-primary">
                                By ${x.ModifiedBy}
                            </div>

                        </div>

                    </div>
                    `;
                });

                html += `</div>`;

                $("#historyTimeline").html(html);
            }
            $("#historyModal").modal("show");
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading history");
            $("#historyTimeline").html("<p>Error loading history</p>");
        }
    });
}

/* ====== RENDER TABLE ====== */
function renderTable(data) {

    // Destroy existing DataTable safely
    if ($.fn.dataTable.isDataTable('#inventoryTable')) {
        $('#inventoryTable').DataTable().clear().destroy();
    }

    const tbody = $('#inventoryTable tbody');
    tbody.empty();

    // Only build rows if data exists
    if (data && data.length > 0) {

        data.forEach((r, idx) => {

            const closingText = `${(r.CurrentQty || 0)} ${r.UnitName || ''}`;
            const inputId = `input_qty_${r.RawMaterialId}_${idx}`;
            const remarksId = `input_rem_${r.RawMaterialId}_${idx}`;

            const row = `
                <tr data-rawid="${r.RawMaterialId}" data-unitid="${r.UnitId}">
                    <td>${escapeHtml(r.ItemCategoryName || '')}</td>
                    <td>${escapeHtml(r.RawMaterialName || '')}</td>
                    <td class="align-middle">${escapeHtml(closingText)}</td>

                    <td class="bg-success-light">
                        <div class="input-group input-group-sm" style="width: 200px;">
                            <input type="number" min="0" step="0.01"
                                   class="form-control stock-inward-qty"
                                   id="${inputId}" placeholder="Stock Inward">
                            <span class="input-group-text px-2">/ ${escapeHtml(r.UnitName || '')}</span>
                        </div>
                    </td>

                    <td class="bg-danger-light">
                        <div class="input-group input-group-sm" style="width: 200px;">
                            <input type="number" min="0" step="0.01"
                                   class="form-control stock-outward-qty"
                                   id="${inputId}" placeholder="Stock Outward">
                            <span class="input-group-text px-2">/ ${escapeHtml(r.UnitName || '')}</span>
                        </div>
                    </td>

                    <td>
                        <input type="text"
                               class="form-control form-control-sm"
                               id="${remarksId}"
                               placeholder="Remarks">
                    </td>

                    <td>
                        <button class="btn btn-sm btn-outline-warning btn-history"
                                data-id="${r.RawMaterialId}"
                                data-name="${r.RawMaterialName}"
                                title="View History">
                            <i class="fa fa-history"></i>
                        </button>
                    </td>
                </tr>
            `;

            tbody.append(row);
        });
    }

    // Initialize DataTable (let it handle empty state)
    stockTable = $('#inventoryTable').DataTable({
        paging: false,
        pageLength: 10,
        lengthChange: false,
        ordering: false,
        info: true,
        searching: true,
        autoWidth: false,

        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows

        language: {
            emptyTable: "No records found"
        },
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .not(':eq(3), :eq(4)') // exclude 4th & 5th column (0-based index)
                .addClass('bg-warning-subtle text-dark');
        }
    });


}


/* ====== FETCH DATA ====== */
function fetchData() {
    $('#btnLoadStock').prop('disabled', true);

    var itemCategoryId = $("#filterItemCategory").val();
    var rawMaterialId = $("#filterRawMaterial").val();
    var unitId = $("#filterUnit").val();

    const payload = {
        ItemCategoryId: itemCategoryId,  //Number($('#filterCategory').val() || 0),
        RawMaterialId: rawMaterialId,
        UnitId: unitId
    };

    $.ajax({
        url: "/api/Inventory/GetStockList", // adjust if controller name differs
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            if (response && response.status === "Success") {
                rawDataCache = response.data || [];
                renderTable(rawDataCache);
            } else {
                rawDataCache = [];
                renderTable([]);
                Swal.fire("Error", response?.message || "Failed to fetch stock", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to fetch stock.");
        }
    });
}

//SAVE 
$(document).on("click", "#btnSaveStockOut", function () {
    saveStockInOut();
});

function saveStockInOut() {

    const stockInOutList = [];

    $('#inventoryTable tbody tr').each(function () {
        const row = $(this);

        const rawId = Number(row.data('rawid') || 0);
        const unitId = Number(row.data('unitid') || 0);

        const qty_in = parseFloat(row.find(".stock-inward-qty").val() || 0);
        const qty_out = parseFloat(row.find(".stock-outward-qty").val() || 0);
        const remarks = row.find("input[type='text']").val() || "";

        if (qty_in > 0 || qty_out > 0) {
            stockInOutList.push({
                RawMaterialId: rawId,
                Qty_In: qty_in,
                Qty_Out: qty_out,
                UnitId: unitId,
                Remarks: remarks
            });
        }
    });

    if (stockInOutList.length === 0) {
        Swal.fire("Nothing to Save", "Enter stock inward or outward for at least one item.", "info");
        return;
    }

    const payload = {
        StockInOutList: stockInOutList,
        ActionBy: Number(sessionStorage.getItem("userId") || 0)
    };

    $("#btnSaveStockOut").prop("disabled", true).text("Saving...");

    $.ajax({
        url: "/api/Inventory/SaveStockInOut",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res && res.status === "Success") {
                handleApiSuccess("Stock out saved successfully.");
                fetchData();
            } else {
                Swal.fire("Error", res?.message || "Unable to save stock out", "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Unable to save stock out.");
        },
        complete: function () {
            $("#btnSaveStockOut").prop("disabled", false).text("Save");
        }
    });

}

// ITEM CATEGORY MASTER - DROPDOWN
function GetCategoryMasterData() {
    $.ajax({
        url: "/api/ItemCategoryMaster/GetItemCategoryList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            $("#filterItemCategory").empty().append(`<option value="0">All</option>`);
            response.data.forEach(c => {
                $("#filterItemCategory").append(`
                        <option value="${c.Id}">${c.CategoryName}</option>
                    `);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Unable to Fetch Category Data!");
        }
    });
}

// ITEM MASTER - DROPDOWN 
function GetRawMaterialMasterData(categoryId = 0) {
    var payload = {
        Id: 0,
        ItemCategoryId: Number(categoryId) || 0
    };
    $.ajax({
        url: "/api/RawMaterialsMaster/GetRawMaterialList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            $("#filterRawMaterial").empty().append(`<option value="0">All</option>`);
            response.data.forEach(c => {
                $("#filterRawMaterial").append(`
                        <option value="${c.Id}" data-unitIdList="${JSON.stringify(c.JSON_PurchaseUnits || []).replace(/"/g, "&quot;")}">${c.Name}</option>
                    `);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Unable to Fetch Raw Materials Data!");
        }
    });
}

// UNIT MASTER - DROPDOWN
function GetUnitMasterData() {
    $.ajax({
        url: "/api/UnitMaster/GetUnitMasterList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            $("#filterUnit").empty().append(`<option value="0">All</option>`);
            response.data.forEach(c => {
                $("#filterUnit").append(`
                        <option value="${c.Id}">${c.UnitName}</option>
                    `);
            });
            unitMaster = response.data;
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Unable to Fetch Raw Materials Data!");
        }
    });
}

/* ====== small helpers ====== */
function escapeHtml(text) {
    if (!text && text !== 0) return '';
    return String(text)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

//HELPER TO LOAD COLUMN CHECKBOXES IN MODAL
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

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewStockReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    var itemCategoryId = $("#filterItemCategory").val();
    var rawMaterialId = $("#filterRawMaterial").val();
    var unitId = $("#filterUnit").val();

    const payload = {
        ItemCategoryId: itemCategoryId,  //Number($('#filterCategory').val() || 0),
        RawMaterialId: rawMaterialId,
        UnitId: unitId,
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };

    const url = actionName === "downloadExcel"
        ? "/api/Inventory/DownloadStockReport_Excel"
        : "/api/Inventory/DownloadStockReport_Pdf";

    showTempLoader("Generating Report...");

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("res:", res);
            //Swal.close();

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            const ext = actionName === "downloadExcel" ? "xlsx" : "pdf";
            const ts = new Date().toISOString().replace(/[:.-]/g, "");
            const link = document.createElement("a");
            link.href = res.data.FileUrl;
            link.download = `Inventory_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            //Swal.close();
            handleApiError(xhr, status, error, "Inventory Report Download Failed");
            Swal.fire("Error", "Failed to generate report. Please try again.", "error");
        },
        complete: function(){
            hideTempLoader();
        }
    });
}
