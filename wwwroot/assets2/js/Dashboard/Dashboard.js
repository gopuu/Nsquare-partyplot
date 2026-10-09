
$(document).ready(function () {
    setCurrentMonthDates();
    loadDashboardWidgetCounts();
    loadUpcomingEventsWidget();

    //loadComparativeInsightsChart("last3months"); //hide - not needed
    loadYearDropdown(); //Overall Bookings Statistics - loadOverallBookingsStatistics();

    let today = new Date();
    let month = today.getFullYear() + "-" + String(today.getMonth() + 1).padStart(2, '0');

    // Enquiry to Booking Conversion -- START
    $("#txtEnquiryToBookingConversionMonth").val(month);
    loadEnquiryConversion();
    $("#txtEnquiryToBookingConversionMonth").change(function(){
        loadEnquiryConversion();
    });
    // Enquiry to Booking Conversion -- END

    // Expense Analysis -- START
    $("#txtExpenseAnalysisMonth").val(month);
    loadExpenseAnalysis();
    $("#txtExpenseAnalysisMonth").change(function () {
        loadExpenseAnalysis();
    });
    // Expense Analysis -- END

    //8 Top Revenue Events -------------- START ------------------------
    let selectedRevenueDuration = "thismonth";
    loadTopRevenueEvents(selectedRevenueDuration);

    $(".chart-filter-Revenue .revenuchart-option").on("click", function (e) {
        e.preventDefault();

        const newFilter = $(this).data("value");
        if (selectedRevenueDuration === newFilter) return;

        selectedRevenueDuration = newFilter;

        $(".revenuchart-option").removeClass("active");
        $(this).addClass("active");

        $("#RevenueChartFilter").text($(this).text());
        loadTopRevenueEvents(selectedRevenueDuration);

    });
    //8 Top Revenue Events -------------- END ------------------------

    AppSignalR.subscribeSettings((data) => {
        //console.log("Settings Page Update", data);
        //console.log("page refresh");
        loadDashboardWidgetCounts();
        //loadComparativeInsightsChart("last3months");

        loadYearDropdown(); //Overall Bookings Statistics - loadOverallBookingsStatistics();
        $("#yearDropdown").text(currentYear);

        loadEnquiryConversion();
        loadExpenseAnalysis();
    });

});

//1. Dashboard Widget Counts --- START ------------------------
function setCurrentMonthDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#fromDate").val(formatDate2(firstDay));
    $("#toDate").val(formatDate2(lastDay));
}

let selectedMonthDuration = "thismonth"; // ✅ default
$(".sort-option").on("click", function (e) {
    e.preventDefault();
    let newFilter = $(this).data("value");

    // agar same filter, kuch mat karo
    if (selectedMonthDuration === newFilter) return;
    selectedMonthDuration = newFilter;

    // dropdown active class manage
    $(".sort-option").removeClass("active");
    $(this).addClass("active");

    $("#sortByDropdown").text("Filter : " + $(this).text());
    $(".sub_count").text("In " + $(this).text());

    // --------------------------
    // MonthDuration ke hisab se From/To date set karna
    // --------------------------
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
        // custom range me datepicker pe user select karega
        return loadDashboardWidgetCounts();
    }

    // Store selected filter in session
    sessionStorage.setItem("SelectedFilter_DashboardWidget", selectedMonthDuration);    // Set the datepickers

    $("#fromDate").val(formatDate2(fromDate));
    $("#toDate").val(formatDate2(toDate));

    // 🔥 Load data
    loadDashboardWidgetCounts();
});

$("#fromDate, #toDate").on("change", function () {

    let fromDate = $("#fromDate").val();
    let toDate = $("#toDate").val();

    if (fromDate && toDate) {
        selectedMonthDuration = "customrange";

        $("#sortByDropdown").text("Filter : Custom Range");
        $(".sub").text("In Custom Range");

        $(".sort-option").removeClass("active");

        loadDashboardWidgetCounts(); // 🔥
    }
});

$("#btnReset").on("click", function () {
    selectedMonthDuration = "thismonth";

    $("#fromDate").val("");
    $("#toDate").val("");

    $("#sortByDropdown").text("Filter : This Month");
    $(".sub").text("In This Month");

    $(".sort-option").removeClass("active");
    $('.sort-option[data-value="thismonth"]').addClass("active");

    loadDashboardWidgetCounts();
});
function loadDashboardWidgetCounts() {
    let requestObj = {
        MonthDuration: selectedMonthDuration, // eg: thismonth, lastmonth
        FromDate: $("#fromDate").val(),        // yyyy-mm-dd
        ToDate: $("#toDate").val()             // yyyy-mm-dd
    };
    //console.log("requestObj DashboardWidgets:", requestObj);

    $.ajax({
        url: "/api/Dashboard/GetDashboardWidgetCounts",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestObj),
        success: function (res) {
            if (res.status === "Success") {
                // Parse the JSON string returned by the API
                //let statsJson = JSON.parse(res.data.DashboardStats);
                //console.log("Dashboard Stats:", res.data);

                // 🔥 FORCE NUMBER CONVERSION
                const totalSales = Number(res.data.TotalSales) || 0;
                const totalEvents = Number(res.data.TotalEvents) || 0;
                const totalCustomers = Number(res.data.TotalCustomer) || 0;
                const eventLostCount = Number(res.data.EventLostCount) || 0;
                const totalEventsLost = Number(res.data.TotalEventsLost) || 0;

                $("#totalSales").text(
                    "₹ " + totalSales.toLocaleString("en-IN", {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    })
                );

                $("#totalEvents").text(totalEvents.toLocaleString("en-IN"));
                $("#totalCustomers").text(totalCustomers.toLocaleString("en-IN"));

                setAmountWithAutoFont("#totalEventsLost", totalEventsLost);
                $("#eventLostCount").text("(" + eventLostCount + ")");

                //console.log("Dashboard Stats Fetched:", res.data);

                // Re-initialize Bootstrap tooltips
                var tooltipTriggerList = [].slice.call(
                    document.querySelectorAll('[data-bs-toggle="tooltip"]')
                );
                tooltipTriggerList.map((el) => new bootstrap.Tooltip(el));

            } else {
                console.warn("API returned error:", res.message);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading dashboard widgets");
            //console.error("AJAX Error:", status, error, xhr.responseText);
        }
    });
}
function setAmountWithAutoFont(elSelector, amount) {
    const el = $(elSelector);

    // Format amount (Indian format)
    const formatted = amount.toLocaleString("en-IN", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    el.text("₹ " + formatted);

    // Reset font classes
    el.removeClass("amount-normal amount-large amount-xlarge");

    // Auto font sizing (Indian thresholds)
    if (amount >= 100000000) {          // 1 Cr+
        el.addClass("amount-xlarge");
    } else if (amount >= 10000000) {    // 10 Lakh+
        el.addClass("amount-large");
    } else {
        el.addClass("amount-normal");
    }
}
function formatValue(key, value) {
    if (key.toLowerCase().includes("sales") || key.toLowerCase().includes("revenue") || key.toLowerCase().includes("amount")) {
        return "₹" + Number(value).toLocaleString("en-IN");
    }
    return Number(value).toLocaleString("en-IN");
}

var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    }
)
//1. Dashboard Widget Counts --- END --------------------------

//1.4 Dashboard Upcoming Event & Amount--- START --------------------------
let selectedUpcomingDuration = "thismonth_upcoming";
$(".upcoming-filter .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".upcoming-filter .dropdown-item").removeClass("active");
    $(this).addClass("active");

    selectedUpcomingDuration = $(this).data("value");
    var txt = $(this).text();
    $("#upcomingFilterBtn").text(txt);
    $(".sub_upcoming").text("In " + txt);

    loadUpcomingEventsWidget();
});

/* === LOAD UPCOMING EVENTS WIDGET === */
function loadUpcomingEventsWidget() {

    let requestObj = {
        MonthDuration: selectedUpcomingDuration,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null
    };

    //console.log("Upcoming Events Request:", requestObj);

    $.ajax({
        url: "/api/Dashboard/GetDashboardUpcomingEvents",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestObj),
        success: function (res) {
            if (res.status === "Success") {
                //console.log("upcoming event res:", res);
                const upcomingCount = Number(res.data.UpcomingEventsCount) || 0;
                const upcomingAmount = Number(res.data.UpcomingEventsAmount) || 0;

                $("#upcomingEventsCount")
                    .text("(" + upcomingCount.toLocaleString("en-IN") + ")");

                setAmountWithAutoFont("#upcomingEventsAmount", upcomingAmount);

                //console.log("Upcoming Events Loaded:", res.data);
            }
            else {
                console.warn("Upcoming API Error:", res.message);
            }
        },

        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading upcoming events");
        }
    });
}

//1.4 Dashboard Upcoming Event & Amount--- END --------------------------

//2. Comparative Insights Chart --- START --------------------------------
function loadComparativeInsightsChart(filter) {

    const monthCount = filter.replace("last", "").replace("months", "");
    $("#pnlFilterBtn").text(`Last ${monthCount} Months`);
    // console.log("pnl inputdata:", filter);
    $.ajax({
        url: "/api/Dashboard/Dashboard_GetComparativeInsightsChart",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Filter: filter }),
        success: function (res) {
            //console.log("pnl res:", res);

            if (res.status !== "Success" || !res.data || res.data.length === 0) {
                alert("No data found");
                return;
            }
            // THIS WAS MISSING
            buildPnLCategoryMonthChart(res.data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "PnL API Error");
        }
    });
}

const PNL_CATEGORIES = [
    { key: "Revenue", percent: null, color: "#4caf50" },
    { key: "Purchase", percent: "PurchasePercent", color: "#f44336" },
    { key: "Salary", percent: "SalaryPercent", color: "#9c27b0" },
    { key: "Rent", percent: "RentPercent", color: "#000000" },
    { key: "Electricity", percent: "ElectricityPercent", color: "#03a9f4" },
    { key: "Gas", percent: "GasPercent", color: "#795548" },
    { key: "OtherExpense", percent: "OtherExpensePercent", color: "#607d8b" },
    { key: "P&L", percent: "ProfitPercent", color: "#4caf50" }
];
function buildPnLCategoryMonthChart2(apiData) {

    const months = apiData.map(x => x.MonthKey ? x.MonthKey.split('-')[0] : '');

    // X-axis labels
    const labels = [];
    PNL_CATEGORIES.forEach(cat => {
        months.forEach(m => labels.push(`${cat.key} ${m}`));
    });

    const datasets = PNL_CATEGORIES.map((cat, catIndex) => {

        const data = [];
        const percentData = [];

        PNL_CATEGORIES.forEach(c2 => {
            apiData.forEach(row => {
                if (c2.key === cat.key) {
                    data.push(row[cat.key]);
                    percentData.push(cat.percent ? row[cat.percent] : null);
                } else {
                    data.push(0);
                    percentData.push(null);
                }
            });
        });

        return {
            label: cat.key,
            data: data,
            backgroundColor: cat.color,
            _percentData: percentData   // custom store for tooltip
        };
    });

    renderPnLCategoryChart(labels, datasets);
}
function buildPnLCategoryMonthChart(apiData) {

    const months = apiData.map(x =>
        x.MonthKey ? x.MonthKey.split('-')[0] : ''
    );

    const labels = [];
    const categoryIndexMap = [];

    // Build label structure
    PNL_CATEGORIES.forEach(cat => {
        months.forEach(m => {
            labels.push(m);
            categoryIndexMap.push(cat.key);
        });
    });

    const datasets2 = PNL_CATEGORIES.map(cat => {

        const data = [];
        const percentData = [];

        PNL_CATEGORIES.forEach(groupCat => {
            apiData.forEach(row => {

                if (groupCat.key === cat.key) {
                    data.push(row[cat.key] ?? null);
                    percentData.push(cat.percent ? row[cat.percent] : null);
                } else {
                    data.push(null);   // 🔥 IMPORTANT: use null not 0
                    percentData.push(null);
                }

            });
        });

        return {
            label: cat.key,
            data,
            backgroundColor: cat.color,
            _percentData: percentData
        };
    });

    const datasets = PNL_CATEGORIES.map(cat => {

        const data = [];
        const percentData = [];

        PNL_CATEGORIES.forEach(groupCat => {
            apiData.forEach(row => {

                if (groupCat.key === cat.key) {
                    const value = row[cat.key] ?? null;
                    data.push(value);
                    percentData.push(cat.percent ? row[cat.percent] : null);
                } else {
                    data.push(null);
                    percentData.push(null);
                }

            });
        });

        return {
            label: cat.key,
            data,
            _percentData: percentData,

            // 🔥 Dynamic coloring only for ProfitAndLoss
            backgroundColor: cat.key === "ProfitAndLoss"
                ? function (ctx) {
                    const value = ctx.raw ?? 0;

                    if (value > 0) return "#4caf50";   // green
                    if (value < 0) return "#f44336";   // red
                    return "#9e9e9e";                  // gray
                }
                : cat.color
        };
    });


    renderPnLCategoryChart(labels, datasets, categoryIndexMap, apiData);
}

let pnlCategoryChart;
function renderPnLCategoryChart2(labels, datasets) {
    console.log("labels:", labels, ", datasets:", datasets);
    if (pnlCategoryChart) pnlCategoryChart.destroy();

    pnlCategoryChart = new Chart(
        document.getElementById("pnlCategoryChart"),
        {
            type: "bar",
            data: { labels, datasets },
            options: {
                responsive: true,
                datasets: {
                    bar: {
                        grouped: true,
                        categoryPercentage: 0.8,  // width of category slot (default 0.8)
                        barPercentage: 1.9,       // width of bar inside slot (default 0.9)
                        maxBarThickness: 40       // hard limit for thickness
                    }
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                const ds = ctx.dataset;
                                const val = ctx.raw || 0;
                                const pct = ds._percentData?.[ctx.dataIndex];

                                if (pct !== null && pct !== undefined) {
                                    return `${ds.label}: ₹${val.toLocaleString()} (${pct}%)`;
                                }
                                return `${ds.label}: ₹${val.toLocaleString()}`;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        type: 'category',
                        offset: true,   // 🔥 important for proper centering
                        ticks: {
                            callback: function (value, index) {
                                const label = this.getLabelForValue(index);
                                return label.split(" ")[1]; // Jan Feb Mar
                            }
                        }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: v => "₹ " + v.toLocaleString()
                        }
                    }
                },
            }
        }
    );
}
function renderPnLCategoryChart(labels, datasets, categoryIndexMap, apiData) {

    if (pnlCategoryChart) pnlCategoryChart.destroy();

    pnlCategoryChart = new Chart(
        document.getElementById("pnlCategoryChart"),
        {
            type: "bar",
            data: { labels, datasets },
            options: {
                responsive: true,

                datasets: {
                    bar: {
                        categoryPercentage: 0.9,
                        barPercentage: 1.0,   // never exceed 1
                        maxBarThickness: 35
                    }
                },

                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                const ds = ctx.dataset;
                                const val = ctx.raw ?? 0;
                                const pct = ds._percentData?.[ctx.dataIndex];

                                if (pct != null) {
                                    return `${ds.label}: ₹${val.toLocaleString()} (${pct}%)`;
                                }
                                return `${ds.label}: ₹${val.toLocaleString()}`;
                            }
                        }
                    },

                    legend: {
                        labels: {
                            generateLabels: function (chart) {

                                const defaultLabels =
                                    Chart.defaults.plugins.legend.labels.generateLabels(chart);

                                return defaultLabels.map(label => {

                                    if (label.text === "ProfitAndLoss") {
                                        return {
                                            ...label,
                                            fillStyle: "#9e9e9e",
                                            text: "ProfitAndLoss (🟢/🔴)"
                                        };
                                    }

                                    return label;
                                });
                            }
                        }
                    }
                },

                scales: {
                    x: {
                        offset: true,
                        ticks: {
                            callback: function (value, index) {
                                return labels[index];
                            }
                        }
                    },

                    x2: {
                        position: 'bottom',
                        offset: true,
                        grid: { display: false },
                        ticks: {
                            callback: function (value, index) {

                                const monthsPerGroup = apiData.length;

                                if (index % monthsPerGroup === Math.floor(monthsPerGroup / 2)) {
                                    return categoryIndexMap[index];
                                }

                                return "";
                            }
                        }
                    },

                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: v => "₹ " + v.toLocaleString()
                        }
                    }
                }
            }

        }
    );
}
//2. Comparative Insights Chart --- END --------------------------------

//6. Overall Bookings Statistics  --- START ------------------------
function loadYearDropdown() {
    $.ajax({
        url: "/api/YearMaster/GetYearList",
        type: "GET",
        success: function (response) {

            if (response.status === "Success" && response.data.length > 0) {

                let html = "";
                let html2 = "";
                let html3 = "";

                let currentYear = new Date().getFullYear();
                let selectedYearId = null;
                let selectedYearName = null;

                response.data.forEach(function (item) {

                    const isCurrentYear = parseInt(item.YearName) === currentYear;

                    if (isCurrentYear) {
                        selectedYearId = item.Id;
                        selectedYearName = item.YearName;
                    }

                    html += `
                        <li>
                            <a class="dropdown-item year-option ${isCurrentYear ? 'active' : ''}"
                               href="#"
                               data-id="${item.Id}"
                               data-year="${item.YearName}">
                               ${item.YearName}
                            </a>
                        </li>`;

                    html2 += `
                        <li>
                            <a class="dropdown-item year-option2 ${isCurrentYear ? 'active' : ''}"
                               href="#"
                               data-id="${item.Id}"
                               data-year="${item.YearName}">
                               ${item.YearName}
                            </a>
                        </li>`;

                    html3 += `
                        <li>
                            <a class="dropdown-item year-option3 ${isCurrentYear ? 'active' : ''}"
                               href="#"
                               data-id="${item.Id}"
                               data-year="${item.YearName}">
                               ${item.YearName}
                            </a>
                        </li>`;
                });

                $("#yearDropdownMenu").html(html);
                $("#ddlInsightYear").html(html2);
                $("#ddlRevenueExpenseYear").html(html3);

                // 🔥 Set default dropdown text
                if (selectedYearId) {
                    $("#yearDropdown, #insightYearBtn, #revenueExpenseYearBtn").text(currentYear);
                    loadOverallBookingsStatistics(selectedYearId, selectedYearName);
                    loadSmartInsights();
                    //loadRevenueExpensePNL();
                }
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading year list");
        }
    });
}

$(document).on("click", ".year-option", function (e) {
    e.preventDefault();

    $(".year-option").removeClass("active");
    $(this).addClass("active");

    const selectedYearId = $(this).data("id");
    const yearText = $(this).data("year");

    $("#yearDropdown").text(yearText);

    console.log("Selected YearId:", selectedYearId, " Year:", yearText);
    loadOverallBookingsStatistics(selectedYearId, yearText);
});

$(document).on("click", ".year-option2", function (e) {
    e.preventDefault();

    $(".year-option2").removeClass("active");
    $(this).addClass("active");

    const selectedYearId = $(this).data("id");
    const yearText = $(this).data("year");

    $("#insightYearBtn").text(yearText);

    console.log("Selected YearId:", selectedYearId, " Year:", yearText);
    loadSmartInsights();
});

$(document).on("click", ".year-option3", function (e) {
    e.preventDefault();

    $(".year-option3").removeClass("active");
    $(this).addClass("active");

    const selectedYearId = $(this).data("id");
    const yearText = $(this).data("year");

    $("#revenueExpenseYearBtn").text(yearText);

    console.log("Selected YearId:", selectedYearId, " Year:", yearText);
    loadRevenueExpensePNL();
});

let bookingBarChart;
function loadOverallBookingsStatistics(yearId, yearName) {
    var payload = {
        YearId: yearId
    };
    //console.log("payload stats:", payload);

    $.ajax({
        url: '/api/Dashboard/Dashboard_GetOverallBookingsStatistics',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {

            if (res.status !== 'Success') return;

            const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

            const dataMap = {};
            res.data.forEach(d => {
                dataMap[d.MonthNo] = d;
            });

            // Extra full data store for tooltip
            const fullMonthData = [];

            months.forEach((m, i) => {

                const d = dataMap[i + 1] || {};

                const row = {
                    month: m,
                    BookingQty: d.BookingQty || 0,
                    TotalAmount: d.TotalAmount || 0,
                    TotalEvents: d.TotalEvents || 0,
                    FamilyEvents: d.TotalFamilyFunctionCount || 0,
                    CorporateEvents: d.TotalCorporateCount || 0
                };

                fullMonthData.push(row);
            });

            if (bookingBarChart) {
                bookingBarChart.destroy();
            }

            bookingBarChart = new Chart(document.getElementById("bookingBarChart"),
                {
                    type: "bar",
                    data: {
                        labels: months,
                        datasets: [
                            {
                                label: "Booking Qty",
                                data: fullMonthData.map(x => x.BookingQty),
                                backgroundColor: "#f39c12",
                                borderRadius: 6
                            }
                        ]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,

                        datasets: {
                            bar: {
                                categoryPercentage: 0.6,
                                barPercentage: 0.8,
                                maxBarThickness: 40
                            }
                        },

                        plugins: {
                            legend: {
                                position: "top",
                                labels: {
                                    usePointStyle: true,
                                    pointStyle: "rectRounded"
                                }
                            },

                            tooltip: {
                                enabled: false,
                                external: function (context) {

                                    let tooltipEl = document.getElementById('chartjs-tooltip');

                                    // Create tooltip element if not exists
                                    if (!tooltipEl) {
                                        tooltipEl = document.createElement('div');
                                        tooltipEl.id = 'chartjs-tooltip';
                                        tooltipEl.style.position = 'absolute';
                                        tooltipEl.style.pointerEvents = 'none';
                                        tooltipEl.style.transition = 'all .15s ease';
                                        document.body.appendChild(tooltipEl);
                                    }

                                    const tooltip = context.tooltip;

                                    if (tooltip.opacity === 0) {
                                        tooltipEl.style.opacity = 0;
                                        return;
                                    }

                                    const index = tooltip.dataPoints[0].dataIndex;
                                    const row = fullMonthData[index];

                                    tooltipEl.innerHTML = `
                                        <div style="
                                            background:#ffffff;
                                            border:1px solid #e5e7eb;
                                            border-radius:10px;
                                            padding:14px 16px;
                                            box-shadow:0 8px 20px rgba(0,0,0,0.08);
                                            font-family:system-ui;
                                            min-width:220px;
                                        ">
                                            <div style="
                                                font-weight:600;
                                                font-size:15px;
                                                margin-bottom:8px;
                                                color:#111827;
                                            ">
                                                ${row.month} (${yearName})
                                            </div>

                                            <div style="font-size:13px;">
                                                Booking Qty:
                                                <span style="float:right; font-weight:600;">
                                                    ${row.BookingQty}
                                                </span>
                                            </div>

                                            <div style="font-size:13px; color:#374151; margin-top:4px;">
                                                Total Amount:
                                                <span style="float:right; font-weight:600;">
                                                    ₹ ${formatINR(row.TotalAmount)}
                                                </span>
                                            </div>

                                            <hr style="margin:10px 0; border:none; border-top:1px solid #e5e7eb;" />

                                            <div style="font-size:13px; margin-top:2px;" class="fw-bold">
                                                Total Events:
                                                <span style="float:right; color:#374151;">
                                                    ${row.TotalEvents}
                                                </span>
                                            </div>

                                            <div style="font-size:13px; margin-top:4px;">
                                                Family Functions:
                                                <span style="float:right; font-weight:600; color:#8e44ad;">
                                                    ${row.FamilyEvents}
                                                </span>
                                            </div>

                                            <div style="font-size:13px; margin-top:4px;">
                                                Corporate Events:
                                                <span style="float:right; font-weight:600; color:#e74c3c;">
                                                    ${row.CorporateEvents}
                                                </span>
                                            </div>

                                            <div style="clear:both;"></div>
                                        </div>
                                    `;

                                    const position = context.chart.canvas.getBoundingClientRect();

                                    tooltipEl.style.opacity = 1;
                                    tooltipEl.style.left = position.left + window.pageXOffset + tooltip.caretX + 'px';
                                    tooltipEl.style.top = position.top + window.pageYOffset + tooltip.caretY + 'px';
                                }
                            }
                        },

                        scales: {
                            x: {
                                grid: { display: false }
                            },
                            y: {
                                beginAtZero: true,
                                ticks: {
                                    precision: 0
                                }
                            }
                        }
                    }
                }
            );
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading Overall-Bookings-Statistics");
        }
    });
}
//6. Overall Bookings Statistics  --- END ------------------------

//8 Revenue Chart - START -------------------------------------------
function loadTopRevenueEvents(monthDuration, fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };

    $.ajax({
        url: "/api/FinanceDashboard/GetDashboardTopRevenueEvents",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("Top Revenue Events:", res);

            let html = "";
            if (res.status === "Success" && res.data && res.data.length > 0) {
                res.data.forEach(item => {
                    html += `
                        <div class="revenue-item d-flex justify-content-between align-items-start p-3 mb-2 rounded">

                            <!-- LEFT -->
                            <div class="flex-grow-1">

                                <!-- Line 1 -->
                                <div class="d-flex justify-content-between">
                                    <div class="fw-semibold text-dark">
                                        ${item.EventTypeName}
                                        <span class="text-muted">(${item.EventSubTypeName})</span>
                                    </div>
                                </div>

                                <!-- Line 2 -->
                                <div class="small text-secondary mt-1">
                                    <span class="fw-semibold">${item.InvoiceNo}</span>
                                    <span class="mx-1">•</span>
                                    <span>${item.ClientName}</span>
                                </div>

                            </div>

                            <!-- RIGHT (Amount) -->
                            <div class="text-end ms-3">
                                <div class="fw-bold text-success fs-7">
                                    ₹ ${formatINR(item.Amount)}
                                </div>
                            </div>

                        </div>`;
                });
            }
            else {
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

//8 Revenue Chart - END -------------------------------------------


//COUNT WIDGETS - tooltip --- START --------------
document.addEventListener('DOMContentLoaded', function () {
    const tooltipContent = {
        tooltipTotalSales: `Sum of total invoice amounts generated during the selected period.`,
        tooltipTotalOrders: `Sum of total invoices generated during the selected period.`,
        tooltipTotalCustomers: `Sum of total Booking PAX of total invoice generated during the selected period.`,
        tooltipEventsLost: `Sum of total lead (enquiries) lost during the selected period.`,
        tooltipUpcomingEvents: `Total number of confirmed events scheduled for upcoming dates.`
    };

    // Loop through all tooltip elements and initialize
    Object.keys(tooltipContent).forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            // Destroy previous tooltip if any (for re-init)
            if (el._tooltipInstance) {
                el._tooltipInstance.dispose();
            }

            // Create new tooltip instance
            const tooltip = new bootstrap.Tooltip(el, {
                title: tooltipContent[id],   // assign dynamic content
                html: true,                  // allow HTML inside tooltip
                placement: 'bottom',         // force bottom
                trigger: 'hover',            // only hover
                fallbackPlacements: []       // prevent auto flip
            });

            // Save instance for later use (optional)
            el._tooltipInstance = tooltip;
        }
    });

    //Amount Click & Redirect page
    $('#totalSales, #totalEvents, #totalCustomers').on('click', function () {
        sessionStorage.setItem("IsShowTotalSalesList", "true");
        window.location.href = '/CustomerInvoice';
    });

    //Eent Lost Click & Redirect page
    $('#totalEventsLost, #tooltipEventsLost').on('click', function () {
        window.location.href = '/Enquiry';
    });
});
//COUNT WIDGETS - tooltip --- END --------------

//UnUsed ----------- START --------------------
$(document).on("click", "#btnSummary", function () {
    console.log("Summary icon clicked ✔");
});
//UnUsed ----------- END --------------------

//SMART INSIGHTS ----------- START --------------------
function loadSmartInsights() {
    var payload = {
        Year: $("#insightYearBtn").text()
    };
    //console.log("smart payload:", payload);

    $.ajax({
        url: "/api/Dashboard/GetDashboardSmartInsights",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("smart res:", res);

            renderInsight("#peakBookingList", res.data.PeakBookingMonths, false);
            renderInsight("#lowBookingList", res.data.LowBookingMonths, false);

            renderInsight("#highRevenueList", res.data.HighestRevenueMonths, true);
            renderInsight("#lowRevenueList", res.data.LowestRevenueMonths, true);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Unable to load Smart Insights.");
        }
    });
}

function renderInsight(container, list, isCurrency) {

    // No data
    if (!list || list.length === 0) {
        $(container).html(`
            <div class="text-muted py-4 ms-4">
                <i class="fa-regular fa-folder-open fs-3 d-block mb-2"></i>
                No data available.
            </div>
        `);
        return;
    }

    let html = "";

    // Maximum value for proportional width
    const maxValue = Math.max(...list.map(x => Number(x.Value)), 1);

    $.each(list, function (index, item) {

        const percentage = ((Number(item.Value) / maxValue) * 100).toFixed(1);
        const rowClass = index % 2 === 0 ? "insight-row-odd" : "insight-row-even";

        html += `
            <div class="${rowClass}" style="padding:8px 10px; border-radius:6px; style="border:1px solid red"">

                <div class="insight-row">

                    <div class="insight-month">
                        ${item.MonthName}
                    </div>

                    <div class="insight-bar-wrapper">

                        <div class="insight-bar">
                            <div class="insight-bar-fill" style="width:${percentage}%"></div>
                        </div>

                        <span class="insight-value me-4 ${isCurrency ? 'ps-2' : ''}"
                              data-bs-toggle="tooltip"
                              data-bs-placement="top"
                              title="${item.Summary}">
                            ${isCurrency ? formatINR2(item.Value) : Number(item.Value)}
                        </span>

                    </div>
                </div>

            </div>
        `;
    });

    $(container).html(html);

    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        new bootstrap.Tooltip(el);
    });
}
//SMART INSIGHTS ----------- END --------------------

//Enquiry to Booking Conversion ----------- START ------------------
let leadSourceData = [];
let leadSourceColor = "#198754";

let leadSortColumn = "";
let leadSortOrder = "ASC";

function loadEnquiryConversion() {
    var payload = {
        YearMonth: $("#txtEnquiryToBookingConversionMonth").val(),
    };
    //console.log("EnquiryConversion payload:", payload);

    $.ajax({
        url: "/api/Dashboard/GetDashboardEnquiryToBookingConversion",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            //console.log("EnquiryConversion res:", res);
            leadSourceData = res.data.LeadSources;
            leadSourceColor = res.data.ColorCode;

            renderLeadSource();

            bindConversionSummary(res.data);
            //bindLeadSource(res.data.LeadSources, res.data.ColorCode);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Unable to load conversion data.");
        }
    });
}
function bindConversionSummary(data) {
    $("#lblConversionRate").text(data.ConversionRate + "%");
    $("#lblTotalEnquiries").text(data.TotalEnquiries);
    $("#lblTotalBookings").text(data.TotalBookings);
    $("#lblMeaning").text(data.Meaning);
    
    const badge = $("#conversionStatusBadge");
    const icon = $("#conversionStatusIcon");

    badge.removeClass("status-excellent status-good status-average status-poor");

    //$("#lblConversionStatus").text(data.Status);
    const statusText = data.Status;
    $("#lblConversionStatus").text(statusText); //.css("font-size", statusText.length > 10 ? "14px" : "18px");
    console.log("Status:", data.Status);

    switch (data.Status) {

        case "Excellent":
            badge.addClass("status-excellent");
            $("#conversionStatusIcon").replaceWith('<i id="conversionStatusIcon" class="fa-solid fa-fw fa-trophy"></i>');
            break;

        case "Good":
            badge.addClass("status-good");
            $("#conversionStatusIcon").replaceWith('<i id="conversionStatusIcon" class="fa-solid fa-fw fa-thumbs-up"></i>');
            break;

        case "Average":
            badge.addClass("status-average");
            $("#conversionStatusIcon").replaceWith('<i id="conversionStatusIcon" class="fa-solid fa-fw fa-chart-column"></i>');
            break;

        default:
            badge.addClass("status-poor");
            $("#conversionStatusIcon").replaceWith('<i id="conversionStatusIcon" class="fa-solid fa-fw fa-triangle-exclamation"></i>');
            break;
    }

    $("#conversionProgress")
        .css({
            width: data.ConversionRate + "%",
            background: data.ColorCode
        });

}

function renderLeadSource() {

    let list = [...leadSourceData];

    if (leadSortColumn !== "") {

        list.sort(function (a, b) {

            let result = 0;

            switch (leadSortColumn) {

                case "LeadSourceName":
                    result = a.LeadSourceName.localeCompare(b.LeadSourceName);
                    break;

                case "TotalEnquiries":
                    result = a.TotalEnquiries - b.TotalEnquiries;
                    break;

                case "TotalBookings":
                    result = a.TotalBookings - b.TotalBookings;
                    break;
            }

            return leadSortOrder === "ASC"
                ? result
                : -result;

        });
    }
    bindLeadSource(list, leadSourceColor);
    updateLeadSortIcons();
}

function bindLeadSource(list, colorCode) {
    if (!list || list.length == 0) {
        $("#tblLeadSourceBody").html(`
            <tr>
                <td colspan="4" class="text-center text-muted py-4">
                    No Data Available
                </td>
            </tr>
        `);
        return;
    }

    let html = "";
    $.each(list, function(i,item){
        html += `

        <tr>
            <td>${item.LeadSourceName}</td>
            <td class="text-center">${item.TotalEnquiries}</td>
            <td class="text-center">${item.TotalBookings}</td>

            <td>
                <div class="d-flex align-items-center">
                    <div class="progress flex-grow-1" style="height:8px;">
                        <div class="progress-bar"
                            style="width:${item.ConversionRate}%;
                            background:${colorCode};">
                        </div>
                    </div>
                    <small class="ms-2 fw-bold">
                        ${item.ConversionRate}%
                    </small>
                </div>
            </td>
        </tr>
        `;
    });
    $("#tblLeadSourceBody").html(html);

    $(".lead-source-table").toggleClass("overflow-auto", list.length > 5);
}
function updateLeadSortIcons() {

    $(".sortable i")
        .removeClass("fa-sort-up fa-sort-down")
        .addClass("fa-sort");

    if (leadSortColumn === "")
        return;

    const icon = $(`.sortable[data-sort='${leadSortColumn}'] i`);

    icon.removeClass("fa-sort");

    if (leadSortOrder === "ASC")
        icon.addClass("fa-sort-up");
    else
        icon.addClass("fa-sort-down");
}
$(document).on("click", ".sortable", function () {

    const column = $(this).data("sort");

    if (leadSortColumn === column) {
        leadSortOrder = leadSortOrder === "ASC"
            ? "DESC"
            : "ASC";
    }
    else {
        leadSortColumn = column;
        leadSortOrder = "ASC";
    }

    renderLeadSource();

});
//Enquiry to Booking Conversion ----------- END --------------------

//EXPENSE ANAYSIS ----------- START ------------------
let expenseAnalysisChart = null;
let expenseAnalysisData = [];
let expenseSortOrder = ""; // DESC = Z→A, ASC = A→Z

let expenseAmountSort = "";
let expenseGrandTotal = 0;

function loadExpenseAnalysis() {
    var payload = {
        YearMonth: $("#txtExpenseAnalysisMonth").val()
    };
    //console.log("ExpenseAnalysis payload:", payload);

    $.ajax({
        url: "/api/Dashboard/GetDashboardExpenseAnalysis",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            expenseAnalysisData = res.data.ExpenseList || [];
            expenseGrandTotal = res.data.GrandTotal || 0;

            bindExpenseAnalysis(expenseAnalysisData, expenseGrandTotal);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fail to load Expense Analysis data");
        }
    });
}

//CATEGORY SORTING
$("#btnExpenseSort").click(function () {
    expenseAmountSort = "";
    $("#btnExpenseAmountSort")
        .attr("title", "Sort Amount : High → Low")
        .html('<i class="fa-solid fa-arrow-down-9-1"></i>');

    expenseSortOrder = expenseSortOrder === "ASC" ? "DESC" : "ASC";

    $(this)
        .attr("title", expenseSortOrder === "ASC" ? "Sort: A → Z" : "Sort: Z → A")
        .html(
            expenseSortOrder === "ASC"
                ? '<i class="fa-solid fa-arrow-down-a-z"></i>'
                : '<i class="fa-solid fa-arrow-down-z-a"></i>'
        );

    renderExpenseAnalysis();
});

$("#btnExpenseAmountSort").click(function () {

    expenseSortOrder = "";

    $("#btnExpenseSort")
        .attr("title", "Sort Category : A → Z")
        .html('<i class="fa-solid fa-arrow-down-a-z"></i>');

    expenseAmountSort =
        expenseAmountSort === "DESC" ? "ASC" : "DESC";

    $(this)
        .attr("title",
            expenseAmountSort === "DESC"
                ? "Sort Amount : High → Low"
                : "Sort Amount : Low → High")
        .html(
            expenseAmountSort === "DESC"
                ? '<i class="fa-solid fa-arrow-down-9-1"></i>'
                : '<i class="fa-solid fa-arrow-up-1-9"></i>'
        );

    renderExpenseAnalysis();
});

function renderExpenseAnalysis() {

    let list = [...expenseAnalysisData];

    if (expenseSortOrder === "ASC") {
        list.sort((a, b) => a.CategoryName.localeCompare(b.CategoryName));
    }
    else if (expenseSortOrder === "DESC") {
        list.sort((a, b) => b.CategoryName.localeCompare(a.CategoryName));
    }

    if (expenseAmountSort === "DESC") {
        list.sort((a, b) => b.TotalAmount - a.TotalAmount);
    }
    else if (expenseAmountSort === "ASC") {
        list.sort((a, b) => a.TotalAmount - b.TotalAmount);
    }

    bindExpenseAnalysis(list, expenseGrandTotal);
}

function bindExpenseAnalysis(list, grandTotal) {

    if (!list || list.length === 0) {
        $("#tblExpenseAnalysis").html(`
            <div class="text-center text-muted py-5">
                No Expense Data Available
            </div>
        `);
        if (expenseAnalysisChart)
            expenseAnalysisChart.destroy();
        return;
    }

    const labels = [];
    const amounts = [];
    const colors = [];

    const palette = [
        "#0d6efd",
        "#198754",
        "#ffc107",
        "#dc3545",
        "#6610f2",
        "#fd7e14",
        "#20c997",
        "#6c757d",
        "#0dcaf0",
        "#e83e8c"
    ];

    let html = "";
    $.each(list, function (i, item) {
        labels.push(item.CategoryName);
        amounts.push(item.TotalAmount);
        colors.push(palette[i % palette.length]);

        html += `
        <div class="mb-3">
            <div class="d-flex justify-content-between align-items-center mb-1">
                <div class="fw-semibold pe-2"
                     style="white-space: normal; word-break: break-word;">
                    ${item.CategoryName}
                </div>

                <div class="text-end">
                    <div class="fw-bold text-danger">
                        ${formatINR2(item.TotalAmount)}
                    </div>
                    <small class="text-muted">
                        ${item.Rate}%
                    </small>
                </div>
            </div>

            <div class="progress" style="height:12px;border-radius:20px;background:#e9edf5;">
                <div class="progress-bar" role="progressbar"
                    style="width:${item.Rate}%;
                        background:${colors[i]};
                        border-radius:20px;"
                    aria-valuenow="${item.Rate}"
                    aria-valuemin="0"
                    aria-valuemax="100">
                </div>
            </div>

        </div>`;
    });

    $("#tblExpenseAnalysis").html(html);
    drawExpenseAnalysisChart(labels, amounts, colors, grandTotal);
}

function drawExpenseAnalysisChart(labels, data, colors, grandTotal) {

    if (expenseAnalysisChart)
        expenseAnalysisChart.destroy();

    const centerTextPlugin = {
        id: 'centerText',
        afterDraw(chart) {

            const { ctx } = chart;
            const meta = chart.getDatasetMeta(0);

            if (!meta.data.length) return;

            const x = Math.round(meta.data[0].x);
            const y = Math.round(meta.data[0].y);

            ctx.save();

            ctx.textAlign = "center";
            ctx.textBaseline = "middle";

            // Title
            ctx.fillStyle = "#6c757d";
            ctx.font = "600 13px Segoe UI";
            ctx.fillText("Total Expenses", x, y - 12);

            // Amount
            const amount = formatINR2(grandTotal);
            let fontSize = 22;

            if (grandTotal >= 10000000) {          // >= 1 Crore
                fontSize = 16;
            }
            else if (grandTotal >= 1000000) {      // >= 10 Lakhs
                fontSize = 18;
            }
            else if (grandTotal >= 100000) {       // >= 1 Lakh
                fontSize = 20;
            }

            ctx.fillStyle = "#212529";
            ctx.font = `700 ${fontSize}px Arial`;
            ctx.fillText(amount, x, y + 12);

            ctx.restore();
        }
    };

    const ctx = document.getElementById("expenseAnalysisChart").getContext("2d");

    expenseAnalysisChart = new Chart(ctx, {
        type: "doughnut",
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderWidth: 1
            }]
        },
        options: {
            maintainAspectRatio: false,
            cutout: "65%",
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return context.label + " : " + formatINR2(context.raw);
                        }
                    }
                }
            }
        },
        plugins: [centerTextPlugin]
    });
}


//EXPENSE ANAYSIS ----------- END ------------------

//REVENUE-EXPENSE-P&L ----------- START ----------------
let revenueExpensePNLChart = null;
function loadRevenueExpensePNL() {
    var payload = {
        Year: $("#revenueExpenseYearBtn").text()
    };
    console.log("RevenueExpensePNL payload:", payload);

    $.ajax({
        url: "/api/Dashboard/GetDashboardRevenueExpensePNL",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {
            console.log("RevenueExpensePNL res:", res);
            bindRevenueExpensePNL(res.data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load Revenue vs Expense chart.");
        }
    });
}

function bindRevenueExpensePNL(list) {

    const months = [];
    const revenue = [];
    const expense = [];
    const pnl = [];

    $.each(list, function (i, item) {
        months.push(item.MonthName);
        revenue.push(item.Revenue);
        expense.push(item.Expense);
        pnl.push(item.ProfitLoss);
    });

    drawRevenueExpensePNLChart(months, revenue, expense, pnl);
}

function drawRevenueExpensePNLChart_17sept26(labels, revenue, expense, pnl) {
    console.log("Revenue:", revenue);
    console.log("Expense:", expense);
    console.log("P&L:", pnl);

    if (revenueExpensePNLChart)
        revenueExpensePNLChart.destroy();

    const ctx = document.getElementById("revenueExpensePNLChart").getContext("2d");

    revenueExpensePNLChart = new Chart(ctx, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [
                {
                    label: "Revenue",
                    data: revenue,
                    backgroundColor: "#ff7f2a"
                },

                {
                    label: "Expense",
                    data: expense,
                    backgroundColor: "#ffc107"
                },

                {
                    label: "P&L",
                    data: pnl,
                    backgroundColor: "#4caf50"
                }
            ]
        },

        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: "index",
                intersect: false
            },

            plugins: {
                legend: {
                    position: "bottom"
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return context.dataset.label + " : " + formatINR2(context.raw);
                        }
                    }
                }
            },

            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return formatINR2(value);
                        }
                    }
                }
            }
        }
    });
}

//ADDED RED BAR - if amt < 0
function drawRevenueExpensePNLChart(labels, revenue, expense, pnl) {
    console.log("Revenue:", revenue);
    console.log("Expense:", expense);
    console.log("P&L:", pnl);

    if (revenueExpensePNLChart)
        revenueExpensePNLChart.destroy();

    const ctx = document.getElementById("revenueExpensePNLChart").getContext("2d");

    revenueExpensePNLChart = new Chart(ctx, {
        type: "bar",

        data: {
            labels: labels,

            datasets: [
                {
                    label: "Revenue",
                    data: revenue,
                    backgroundColor: revenue.map(value =>
                        Number(value) < 0 ? "#dc3545" : "#ff7f2a"
                    )
                },

                {
                    label: "Expense",
                    data: expense,
                    backgroundColor: expense.map(value =>
                        Number(value) < 0 ? "#dc3545" : "#ffc107"
                    )
                },

                {
                    label: "P&L",
                    data: pnl,
                    backgroundColor: pnl.map(value =>
                        Number(value) < 0 ? "#dc3545" : "#4caf50"
                    )
                }
            ]
        },

        options: {
            responsive: true,
            maintainAspectRatio: false,

            interaction: {
                mode: "index",
                intersect: false
            },

            plugins: {
                legend: {
                    position: "bottom"
                },

                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return context.dataset.label + " : " + formatINR2(context.raw);
                        }
                    }
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        callback: function (value) {
                            return formatINR2(value);
                        }
                    }
                }
            }
        }
    });
}

//REVENUE-EXPENSE-P&L ----------- END ------------------




