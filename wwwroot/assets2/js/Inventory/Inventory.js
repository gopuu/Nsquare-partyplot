var unitMaster = [];

$(document).ready(function () {
    fetchData(0, 0, 0);
    GetCategoryMasterData();
    GetRawMaterialMasterData();  // Load ALL initially
    GetUnitMasterData();

    var rawMaterialId = 0;

    // When Category changes → Fetch Raw Materials of that category
    $("#filterItemCategory").on("change", function () {
        let categoryId = $(this).val();
        GetRawMaterialMasterData(categoryId);
        fetchData(categoryId, 0, 0);
    });

    // On Raw Material Change → Load allowed units only
    $(document).on("change", "#filterRawMaterial", function () {
        // 1. GET SELECTED RAW MATERIAL ID
        rawMaterialId = $(this).val();
        fetchData(0, rawMaterialId, 0);

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
        fetchData(0, rawMaterialId, unitId);
    });

    // CLEAR FILTERS
    $(document).on("click", "#btnClearFilters", function () {
        // Reset Category → "All"
        $("#filterItemCategory").val("0");

        // Reload Raw Materials (All)
        GetRawMaterialMasterData(0);

        // Reset Unit Dropdown
        $("#filterUnit").empty().append(`<option value="0">All</option>`);
        GetUnitMasterData();

        // Optionally reload inventory list
        rawMaterialId = 0;
        fetchData(0, 0, 0);
    });
});

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

                    <td>
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
                </tr>
            `;

            tbody.append(row);
        });
    }

    // Initialize DataTable (let it handle empty state)
    stockTable = $('#inventoryTable').DataTable({
        paging: true,
        pageLength: 10,
        lengthChange: false,
        ordering: false,
        info: true,
        searching: true,
        autoWidth: false,
        language: {
            emptyTable: "No records found"
        }
    });
}


/* ====== FETCH DATA ====== */
function fetchData(itemCategoryId = 0, rawMaterialId = 0, unitId = 0) {

    $('#btnLoadStock').prop('disabled', true);
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
    saveStockOut();
});

function saveStockOut() {

    const stockOutList = [];

    $('#inventoryTable tbody tr').each(function () {
        const row = $(this);

        const rawId = Number(row.data('rawid') || 0);
        const unitId = Number(row.data('unitid') || 0);

        const qty = parseFloat(row.find(".stock-outward-qty").val() || 0);
        const remarks = row.find("input[type='text']").val() || "";

        if (qty > 0) {
            stockOutList.push({
                RawMaterialId: rawId,
                Qty: qty,
                UnitId: unitId,
                Remarks: remarks
            });
        }
    });

    if (stockOutList.length === 0) {
        Swal.fire("Nothing to Save", "Enter stock outward for at least one item.", "info");
        return;
    }

    const payload = {
        StockOutList: stockOutList,
        ActionBy: Number(sessionStorage.getItem("userId") || 0)
    };

    $("#btnSaveStockOut").prop("disabled", true).text("Saving...");

    $.ajax({
        url: "/api/Inventory/SaveStockOut",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            if (res && res.status === "Success") {
                Swal.fire({
                    position: "top-end",
                    icon: "success",
                    title: "Saved!",
                    showConfirmButton: false,
                    timer: 1200
                });
                // Refresh table
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

