
$(document).ready(function () {
    loadKeyStatistics("thismonth");
    loadCOGSCount("thismonth");
    loadPending_Purchases_ApprovalsCount("thismonth");

    // initial load
    GetCategoryMasterData();
    loadStockLevel();
    // On category change → reload stock level
    $("#categoryFilter").on("change", function () {
        const categoryId = $(this).val();
        loadStockLevel(categoryId);
    });
    
    loadSuppliersWithItems();

    //FOR KEY STATISTICS 🔹 Dropdown click event (same as event pie)
    $(".chart-filter-Statistics .dropdown-item").on("click", function (e) {
        e.preventDefault();

        $(".chart-filter-Statistics .dropdown-item").removeClass("active");
        $(this).addClass("active");

        const filterText = $(this).text().toLowerCase().replaceAll(" ", "");
        $("#jobChartFilter").text($(this).text());

        loadKeyStatistics(filterText);
    });

    // 🔹 COGS Dropdown click event
    $(".chart-filter-COGS .dropdown-item").on("click", function (e) {
        e.preventDefault();

        // Toggle active state
        $(".chart-filter-COGS .dropdown-item").removeClass("active");
        $(this).addClass("active");

        // Get filter text → normalized value
        const filterText = $(this).text().toLowerCase().replaceAll(" ", "");

        // Set dropdown button text
        $("#COGSChartFilter").text($(this).text());

        // Load COGS count
        loadCOGSCount(filterText);
    });

});

function loadKeyStatistics(monthDuration = "thismonth", fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };
    //console.log("key stats payload:", payload);
    $.ajax({
        url: "/api/InventoryDashboard/GetInventoryDashboardKeyStatisticsCount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("res key statics:",res);
            if (res.status === "Success") {
                $("#totalSales").text(res.data.TotalSales);
                $("#totalPurchase").text(res.data.TotalPurchase);
                $("#totalWastage").text(res.data.TotalWastage);
            } else {
                $("#totalSales,#totalPurchase,#totalWastage").text("0");
            }
        },
        error: function () {
            console.error("Key Statistics API failed");
        }
    });
}

function loadCOGSCount(monthDuration = "thismonth", fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };
    console.log("COGs payload:", payload);

    $.ajax({
        url: "/api/InventoryDashboard/GetInventoryDashboardGetCOGSCount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("res Worth of Stock:", res);
            if (res.status === "Success") {
                $("#WorthofStockConsumed").text(res.data.WorthofStockConsumed);
              
            } else {
                $("#WorthofStockConsumed").text("0");
            }
        },
        error: function () {
            console.error("Key Statistics API failed");
        }
    });
}

function loadPending_Purchases_ApprovalsCount(monthDuration = "thismonth", fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };
    console.log("COGs payload:", payload);

    $.ajax({
        url: "/api/InventoryDashboard/GetInventoryDashboardGetPending_Purchases_ApprovalsCount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("res Pending_Purchases_Approvals:", res);
            if (res.status === "Success") {
                $("#pendingpurchases").text(res.data.PendingPurchases);
                $("#pendingapprovals").text(res.data.PendingApprovals);
            }
            else {
                $("#pendingpurchases","#pendingapprovals").text("0");
            }
        },
        error: function () {
            console.error("purchase API failed");
        }
    });
}

//Stock Level ----------------------- START -------------------------------------
// ITEM CATEGORY DROPDOWN
function GetCategoryMasterData() {
    $.ajax({
        url: "/api/ItemCategoryMaster/GetItemCategoryList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            $("#categoryFilter").empty().append(`<option value="0">Filter By Category</option>`);
            response.data.forEach(c => {
                $("#categoryFilter").append(`<option value="${c.Id}">${c.CategoryName}</option>`);
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Unable to Fetch Category Data!");
        }
    });
}

function loadStockLevel(itemCategoryId = 0) {
    var payload = {
        ItemCategoryId: itemCategoryId
    };

    $.ajax({
        url: "/api/InventoryDashboard/GetInventoryDashboardStockLevel",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("Stock level response:", res);

            if (res.status === "Success" && Array.isArray(res.data)) {
                renderStockList(res.data);
            }
            else {
                $("#stockList").html("<div class='text-muted text-center'>No stock data found</div>");
            }
        },
        error: function (xhr) {
            console.error("Stock API failed", xhr);
            $("#stockList").html("<div class='text-danger text-center'>Failed to load stock data</div>");
        }
    });
}

function renderStockList(list) {

    const container = document.getElementById("stockList");
    container.innerHTML = "";

    const limit = 20; // max stock limit (100%)

    list
        .sort((a, b) => b.Quantity - a.Quantity) // high → low
        .forEach(item => {

            const ratio = Math.min(item.Quantity / limit, 1);
            const percent = ratio * 100;

            // color logic
            let barColor = "#ffc107"; // warning
            if (item.Quantity <= 5) barColor = "#dc3545"; // danger
            if (item.Quantity >= limit) barColor = "#198754"; // success

            const stockItem = `
                <div class="mb-3">
                    <div class="d-flex justify-content-between">
                        <span class="fw-semibold small">${item.ProductName}</span>
                        <span class="small text-muted">${item.Quantity} Qty</span>
                    </div>

                    <div class="progress" style="height: 8px; border-radius: 5px;">
                        <div class="progress-bar"
                             role="progressbar"
                             style="width: ${percent}%; background-color: ${barColor};"
                             aria-valuenow="${percent}"
                             aria-valuemin="0"
                             aria-valuemax="100">
                        </div>
                    </div>
                </div>
            `;

            container.insertAdjacentHTML("beforeend", stockItem);
        });
}
//Stock Level ----------------------- END ---------------------------------------

//Supplier & Items ----------------------- START ---------------------------------------
function loadSuppliersWithItems() {
    var payload = {
        SupplierId: 0
    };

    $.ajax({
        url: "/api/InventoryDashboard/GetInventoryDashboardSuppliers",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("Suppliers response:", res);

            if (res.status === "Success" && res.data?.length) {
                renderSuppliers(res.data);
            } else {
                $("#supplierList").html(
                    "<div class='text-muted text-center'>No data found</div>"
                );
            }
        },
        error: function () {
            $("#supplierList").html(
                "<div class='text-danger text-center'>Failed to load suppliers</div>"
            );
        }
    });
}

function renderSuppliers(data) {

    const container = $(".supplier-list");
    container.empty();

    const grouped = {};

    data.forEach(item => {
        const supplier = item.SupplierName;
        if (!grouped[supplier]) grouped[supplier] = [];
        grouped[supplier].push(item);
    });

    let idx = 1;

    for (const supplier in grouped) {

        const id = "supplier_" + idx++;
        let items = "";

        grouped[supplier].forEach(i => {
            items += `<p class="mb-1 small">${i.ItemName} - ${i.Quantity}</p>`;
        });

        container.append(`
            <div class="supplier mb-3">
                <div class="d-flex justify-content-between align-items-center 
                            border-bottom py-2 bg-light px-2 rounded">
                    <div class="d-flex align-items-center">
                        <button class="btn btn-sm btn-outline-secondary toggle-btn"
                                data-target="#${id}">-</button>
                        <span class="ms-2 fw-semibold supplier-name">${supplier}</span>
                    </div>
                    <button class="btn btn-sm btn-outline-primary raise-po-btn">
                        Raise PO - Coming Soon
                    </button>
                </div>

                <div id="${id}" class="mt-2 ms-4">
                    ${items}
                </div>
            </div>
        `);
    }
}


$(document).on("click", ".toggle-btn", function () {
    const target = $(this).data("target");
    const isVisible = $(target).is(":visible");

    $(target).toggle();
    $(this).text(isVisible ? "+" : "-");
});
//Supplier & Items ----------------------- END - ---------------------------------------


