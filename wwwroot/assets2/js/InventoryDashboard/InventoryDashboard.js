
$(document).ready(function () {
    loadKeyStatistics("thismonth");
    AppSignalR.subscribe(() => {
        //console.log("page refresh");
        //loadKeyStatistics("thismonth");
        const $active = $(".chart-filter-Statistics .dropdown-item.active");
        if ($active.length) {
            $active.trigger("click");
        }
        else {
            $(".chart-filter-Statistics .dropdown-item:first").trigger("click");
        }
    });

    loadCOGSCount("thismonth");
    //loadPending_Purchases_ApprovalsCount("thismonth");

    // initial load
    GetCategoryMasterData();
    loadStockLevel();
    // On category change → reload stock level
    $("#categoryFilter").on("change", function () {
        const categoryId = $(this).val();
        loadStockLevel(categoryId);
    });
    
    // Initial load
    setStockOutDatesByDuration();
    loadStockOutSummary();

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
        const filterMonth = $(this).text().toLowerCase().replaceAll(" ", "");

        // Set dropdown button text
        $("#COGSChartFilter").text($(this).text());

        console.log("filterMonth:", filterMonth);
        // Load COGS count
        loadCOGSCount(filterMonth);
    });

});

function loadKeyStatistics(monthDuration = "thismonth", fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };
    console.log("key stats payload:", payload);
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
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Key Statistics Data Failed")
        }
    });
}

function loadCOGSCount(monthDuration = "thismonth", fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };
    //console.log("COGs payload:", payload);

    $.ajax({
        url: "/api/InventoryDashboard/GetInventoryDashboardGetCOGSCount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("res Worth of Stock:", res);
            if (res.status === "Success") {
                $("#WorthofStockConsumed").text(res.data.WorthofStockConsumed);
              
            } else {
                $("#WorthofStockConsumed").text("0");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "COGs Data Failed")
        }
    });
}

function loadPending_Purchases_ApprovalsCount(monthDuration = "thismonth", fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };
    console.log("pending purchase payload:", payload);

    $.ajax({
        url: "/api/InventoryDashboard/GetInventoryDashboardGetPending_Purchases_ApprovalsCount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("res Pending_Purchases_Approvals:", res);
            if (res.status === "Success") {
                $("#pendingpurchases").text(res.data.PendingPurchases);
                $("#pendingapprovals").text(res.data.PendingApprovals);
            }
            else {
                $("#pendingpurchases","#pendingapprovals").text("0");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Pending Purchase Data Failed")
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
            const dropdown = $("#categoryFilter");

            dropdown.empty().append(`<option value="0">Filter By Category</option>`);

            response.data.forEach(c => {
                dropdown.append(`<option value="${c.Id}">${c.CategoryName}</option>`);
            });

            dropdown.select2({
                placeholder: "Filter By Category",
                allowClear: true,
                width: '50%',   // 🔥 increase by 30%
                //width: 'resolve'
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
            //console.log("Stock level response:", res);

            if (res.status === "Success" && Array.isArray(res.data)) {
                renderStockList(res.data);
            }
            else {
                $("#stockList").html("<div class='text-muted text-center'>No stock data found</div>");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Stock Level failed");
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

//Stock-Out Summary (Raw Material Wise) ----------------------- START ---------------------------------------
function loadStockOutSummary() {

    const payload = {
        MonthDuration: $("#stockOutDuration").val(),
        FromDate: $("#stockOutFromDate").val(),
        ToDate: $("#stockOutToDate").val(),
        RawMaterialId: 0
    };

    $.ajax({
        url: "/api/InventoryDashboard/GetInventoryDashboardStockOutSummary",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            if (res.status === "Success" && res.data?.length) {
                renderStockOutSummary(res.data);
            } else {
                $(".stockout-list").html(
                    "<div class='text-muted text-center'>No stock-out data found</div>"
                );
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "StockOut Data Failed")
            $(".stockout-list").html(
                "<div class='text-danger text-center'>Failed to load stock-out data</div>"
            );
        }
    });
}

function renderStockOutSummary(data) {

    const container = $(".stockout-list");
    container.empty();

    // Header row
    container.append(`
        <div class="d-flex fw-bold text-muted border-bottom pb-2 mb-2 small stockout-header">
            <div class="flex-fill">Raw Material</div>
            <div class="text-center d-none" style="width:120px;">Count</div>
            <div class="text-end" style="width:150px;">Total Qty</div>
        </div>
    `);

    data.forEach(item => {
        container.append(`
            <div class="d-flex align-items-center py-2 border-bottom small stockout-row">
                <div class="flex-fill fw-semibold">
                    ${item.RawMaterialName}
                </div>

                <div class="text-center d-none" style="width:120px;">
                    <strong class="text-danger">${item.StockOutCount}</strong>
                </div>

                <div class="text-end" style="width:150px;">
                    <strong>${item.TotalStockOutQty}</strong>
                </div>
            </div>
        `);
    });
}

$("#stockOutDuration").on("change", function () {
    setStockOutDatesByDuration();
    loadStockOutSummary();
});

$("#stockOutFromDate, #stockOutToDate").on("change", function () {
    if ($("#stockOutDuration").val() === "customrange") {
        loadStockOutSummary();
    }
});
function setStockOutDatesByDuration() {

    const duration = $("#stockOutDuration").val();
    const today = new Date();

    let fromDate = null;
    let toDate = formatDate2(today);

    switch (duration) {

        case "yesterday":
            fromDate = formatDate2(new Date(today.setDate(today.getDate() - 1)));
            toDate = fromDate;
            break;

        case "last7days": // Last 7 days
            fromDate = formatDate2(new Date(today.setDate(today.getDate() - 6)));
            break;

        case "last30days": // Last 30 days
            fromDate = formatDate2(new Date(today.setDate(today.getDate() - 29)));
            break;

        case "thismonth":
            fromDate = formatDate2(new Date(today.getFullYear(), today.getMonth(), 1));
            toDate = formatDate2(new Date(today.getFullYear(), today.getMonth() + 1, 0));
            break;

        case "lastmonth":
            fromDate = formatDate2(new Date(today.getFullYear(), today.getMonth() - 1, 1));
            toDate = formatDate2(new Date(today.getFullYear(), today.getMonth(), 0));
            break;

        case "last12months":
            fromDate = formatDate2(new Date(today.getFullYear() - 1, today.getMonth(), 1));
            break;

        case "customrange":
            $("#stockOutFromDate, #stockOutToDate").prop("disabled", false);
            return; // 👈 wait for manual input
    }

    // 🔒 Lock date inputs for auto ranges
    $("#stockOutFromDate, #stockOutToDate")
        .prop("disabled", true)
        .val("");

    $("#stockOutFromDate").val(fromDate);
    $("#stockOutToDate").val(toDate);
}

//Stock-Out Summary (Raw Material Wise)----------------------- END - ---------------------------------------


