$(document).ready(function () {
    loadFinanceSummary("ThisMonth");
    loadPaymentCollectionStatus("ThisMonth");
    loadUpcomingPayments("ThisMonth");
    loadVendorPayables("thismonth");
    loadTopRevenueEvents("thismonth");

});

function loadFinanceSummary(monthDuration, fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardFinanceSummary",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {
            console.log("res count summaery finance:",res);
            if (res.status === "Success" && res.data) {

                $("#txtTotalRevenue").text(formatINR(res.data.TotalRevenue));
                $("#txtOutstanding").text(formatINR(res.data.OutstandingReceivables));
                $("#txtExpenses").text(formatINR(res.data.TotalExpenses));
                $("#txtNetProfit").text(formatINR(res.data.NetProfit));

                // duration label
              //  $("#lblRevenueDuration").text(getDurationLabel(monthDuration));
            }
            else {
                resetFinanceCards();
            }
        },
        error: function () {
            resetFinanceCards();
        }
    });
}
// 🔹 Dropdown click event (same as event pie)
$(".chart-filter-finance .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".chart-filter-finance .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterText = $(this).text().toLowerCase().replaceAll(" ", "");
    $("#jobChartFilter").text($(this).text());

    loadFinanceSummary(filterText);
});
function formatINR(amount) {
    if (!amount) return "0";
    return amount.toLocaleString("en-IN");
}

function loadUpcomingPayments(monthDuration = "thismonth", fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardUpcomingPayments",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {

            console.log("UpcomingPayments API:", res);

            let html = "";

            if (res.status === "Success" && Array.isArray(res.data) && res.data.length > 0) {

                res.data.forEach(item => {
                    html += `
                        <li class="list-group-item d-flex justify-content-between">
                            <span>${item.EventName}</span>
                            <span class="text-danger">
                                ₹ ${formatINR(item.Amount)}
                            </span>
                        </li>`;
                });

            } else {
                html = `
                    <li class="list-group-item text-muted text-center">
                        No upcoming payments
                    </li>`;
            }

            $("#upcomingPaymentsList").html(html);
        },
        error: function () {
            $("#upcomingPaymentsList").html(`
                <li class="list-group-item text-danger text-center">
                    Error loading data
                </li>
            `);
        }
    });
}

//function getDurationLabel(duration) {
//    switch (duration) {
//        case "ThisMonth": return "Current Month";
//        case "LastMonth": return "Last Month";
//        case "Last30Days": return "Last 30 Days";
//        case "Last12Months": return "Last 12 Months";
//        default: return "Custom Range";
//    }
//}

function resetFinanceCards() {
    $("#txtTotalRevenue").text("0");
    $("#txtOutstanding").text("0");
    $("#txtExpenses").text("0");
    $("#txtNetProfit").text("0");
}

function loadPaymentCollectionStatus(monthDuration, fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardPaymentCollectionStatus",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {
            console.log("res count PaymentCollection finance:", res);

            if (res.status === "Success" && res.data) {

                const paid = res.data.PaidAmount;
               // const partial = res.data.PartialAmount;
                const pending = res.data.PendingAmount;
                const total = res.data.TotalAmount || 1; // avoid divide by zero

                $("#txtPaidAmount").text(formatINR(paid));
               // $("#txtPartialAmount").text(formatINR(partial));
                $("#txtPendingAmount").text(formatINR(pending));

                $("#barPaid").css("width", calcPercent(paid, total) + "%");
                //$("#barPartial").css("width", calcPercent(partial, total) + "%");
                $("#barPending").css("width", calcPercent(pending, total) + "%");
            }
        }
    });
}

// 🔹 Dropdown click event (same as event pie)
$(".chart-filter-Payment .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".chart-filter-Payment .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterText = $(this).text().toLowerCase().replaceAll(" ", "");
    $("#PaymentChartFilter").text($(this).text());

    loadPaymentCollectionStatus(filterText);
});

function calcPercent(value, total) {
    return Math.round((value / total) * 100);
}
function loadVendorPayables(monthDuration, fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardVendorPayables",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {

            console.log("Vendor Payables API:", res);

            let html = "";

            if (res.status === "Success" && Array.isArray(res.data) && res.data.length > 0) {

                res.data.forEach(item => {

                    let badgeClass =
                        item.Status === "Overdue" ? "bg-danger" :
                            item.Status === "Pending" ? "bg-warning" :
                                "bg-secondary";

                    html += `
                        <tr>
                            <td>${item.VendorName}</td>
                            <td>₹ ${formatINR(item.PendingAmount)}</td>
                            <td>
                                <span class="badge ${badgeClass}">
                                    ${item.Status}
                                </span>
                            </td>
                        </tr>`;
                });

            } else {
                html = `
                    <tr>
                        <td colspan="3" class="text-muted text-center">
                            No vendor payables
                        </td>
                    </tr>`;
            }

            $("#vendorPayablesBody").html(html);
        },
        error: function () {
            $("#vendorPayablesBody").html(`
                <tr>
                    <td colspan="3" class="text-danger text-center">
                        Error loading data
                    </td>
                </tr>
            `);
        }
    });
}
function loadTopRevenueEvents(monthDuration, fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardTopRevenueEvents",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {
            console.log("Top Revenue Events:", res);

            let html = "";

            if (res.status === "Success" && res.data && res.data.length > 0) {
                res.data.forEach(item => {
                    html += `
                        <li class="list-group-item d-flex justify-content-between">
                            <span>${item.EventName}</span>
                            <strong>₹ ${formatINR(item.Amount)}</strong>
                        </li>`;
                });
            } else {
                html = `
                    <li class="list-group-item text-muted text-center">
                        No revenue events found
                    </li>`;
            }

            $("#topRevenueEventsList").html(html);
        },
        error: function () {
            $("#topRevenueEventsList").html(`
                <li class="list-group-item text-danger text-center">
                    Error loading data
                </li>
            `);
        }
    });
}
