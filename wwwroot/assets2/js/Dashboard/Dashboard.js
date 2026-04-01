
$(document).ready(function () {
    setCurrentMonthDates();
    loadDashboardWidgetCounts();
    loadUpcomingEventsWidget();

    loadComparativeInsightsChart("last3months");

    loadBookingEventTypeCount();
    loadBookingSubEventTypeCorporate();
    loadBookingSubEventTypeFamilyFunction();

    loadLeadSourceChart("thismonth");
    loadLeadLostChart("thismonth");
    loadYearDropdown(); //loadOverallBookingsStatistics();

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
        loadComparativeInsightsChart("last3months");

        loadBookingEventTypeCount();
        loadBookingSubEventTypeCorporate();
        loadBookingSubEventTypeFamilyFunction();

        loadYearDropdown(); //loadOverallBookingsStatistics(currentYear);
        $("#yearDropdown").text(currentYear);
    });


    //Lead SOurce
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
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


//3. Corporate Event vs Family Events -- START ------------------------
let jobPieChart = null;
function loadBookingEventTypeCount() {

    let requestObj = {
        MonthDuration: selectedMonthDuration,
        FromDate: $("#fromDate").val() || null,
        ToDate: $("#toDate").val() || null
    };

    $.ajax({
        url: "/api/Dashboard/GetBookingEventTypeCount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestObj),
        success: function (res) {
            //console.log("pie res:",res);
            if (res.status !== "Success") {
                console.warn(res.message);
                return;
            }
            let labels = [];
            let values = [];
            let percents = [];

            res.data.forEach(item => {
                labels.push(item.EventLabel);
                values.push(item.EventCount);
                percents.push(item.EventPercent); // 🔥 NEW
            });

            renderCorporateFamilyChart(labels, values, percents);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fail to get Corporate-Family Data");
        },
    });
}
function renderCorporateFamilyChart(labels, values, percents) {     
    const ctx = document.getElementById('jobPie');

    // 🔁 destroy old chart
    if (jobPieChart) {
        jobPieChart.destroy();
    }

    jobPieChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: [
                    '#3366cc', '#dc3912', '#ff9900',
                    '#109618', '#990099', '#0099c6',
                    '#dd4477', '#66aa00', '#b82e2e'
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'right',
                    labels: {
                        generateLabels(chart) {
                            const data = chart.data;
                            return data.labels.map((label, i) => ({
                                text: wrapText(`${label} (${percents[i]})`, 20), // wrap at 20 chars
                                fillStyle: data.datasets[0].backgroundColor[i]
                            }));
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const i = context.dataIndex;
                            return `${labels[i]}: ${values[i]} (${percents[i]})`;
                        }
                    }
                },
                datalabels: {
                    color: '#fff',
                    font: { weight: 'bold', size: 12 },
                    formatter: (value, context) => {
                        return percents[context.dataIndex];
                    }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}
$(".chart-filter-pie .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".chart-filter-pie .dropdown-item").removeClass("active");
    $(this).addClass("active");

    selectedMonthDuration = $(this).text().toLowerCase().replaceAll(" ", "");

    $("#jobChartFilter").text($(this).text());

    loadBookingEventTypeCount(); // 🔥 reload chart
});
//3. Corporate Event vs Family Events -- END ------------------------

//4. Corporate & Sub type of Event -- START ------------------------
// 🔹 Function to load data via AJAX
function loadBookingSubEventTypeCorporate(monthDuration = "thismonth", fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    }
    //console.log("BookingSubEventTypeCorporate payload:", payload);
    $.ajax({
        url: '/api/Dashboard/GetBookingSubEventTypeCorporate',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (response) {
            //console.log("BookingSubEventTypeCorporate response:", response);
            if (response.status === "Success" && response.data.length > 0) {
                const labels = response.data.map(x => x.EventLabel);
                const values = response.data.map(x => x.EventCount);
                const percents = response.data.map(x => x.EventPercent);

                renderCorporateSubTypeChart(labels, values, percents);
            } else {
                // empty chart
                renderCorporateSubTypeChart([], [], []);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading pie chart data");
        }
    });
}
let eventPieChart; // 🔹 store chart instance globally
function renderCorporateSubTypeChart(labels, values, percents) {
    const ctx = document.getElementById('eventPie');

    // Destroy old chart if exists
    if (eventPieChart) eventPieChart.destroy();

    eventPieChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: [
                    '#1abc9c',   // teal
                    '#3498db',   // blue
                    '#9b59b6',   // purple
                    '#e67e22',   // orange
                    '#e74c3c',   // red
                    '#95a5a6'    // gray
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'right',
                    labels: {
                        generateLabels(chart) {
                            const data = chart.data;
                            return data.labels.map((label, i) => ({
                                text: wrapText(`${label} (${percents[i]})`, 20), // wrap at 20 chars
                                fillStyle: data.datasets[0].backgroundColor[i]
                            }));
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const i = context.dataIndex;
                            return `${labels[i]}: ${values[i]} (${percents[i]})`;
                        }
                    }
                },
                datalabels: {
                    color: '#fff',
                    font: { weight: 'bold', size: 12 },
                    //formatter: (value) => value
                    formatter: (value, context) => {
                        return percents[context.dataIndex];
                    }
                }
            }
        },
        plugins: [ChartDataLabels] // make sure you include plugin
    });
}

// 🔹 Dropdown click event
$(".chart-filter-eventPie .dropdown-item").on("click", function (e) {
    e.preventDefault();

    // Remove active class from all
    $(".chart-filter-eventPie .dropdown-item").removeClass("active");

    // Add active to clicked item
    $(this).addClass("active");

    // Update the button text
    const selectedText = $(this).text();
    $("#eventPieChartFilter").text(selectedText);

    // Optional: store selected value for chart
    const selectedValue = $(this).data("value"); // e.g., "yesterday", "thismonth"
    selectedMonthDuration = selectedValue;
    console.log("subEvent-Corporate selectedMonthDuration:", selectedValue);
    // Call your chart update
    loadBookingSubEventTypeCorporate(selectedValue);
});
//4. Corporate & Sub type of Event -- END ------------------------

function wrapText(text, maxLength) {
    const words = text.split(' ');
    let lines = [];
    let currentLine = '';

    words.forEach(word => {
        if ((currentLine + word).length > maxLength) {
            lines.push(currentLine.trim());
            currentLine = '';
        }
        currentLine += word + ' ';
    });

    lines.push(currentLine.trim());
    return lines;
}

//5. Family & Sub type of Event -- START ------------------------
function loadBookingSubEventTypeFamilyFunction(monthDuration = "thismonth", fromDate = null, toDate = null) {
    $.ajax({
        url: '/api/Dashboard/GetBookingSubEventTypeFamilyFunction', // your API route
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (response) {
            if (response.status === "Success" && response.data.length > 0) {
                const labels = response.data.map(x => x.EventLabel);
                const values = response.data.map(x => x.EventCount);
                const percents = response.data.map(x => x.EventPercent);

                renderFamilySubTypeChart(labels, values, percents);
            } else {
                // empty chart
                renderFamilySubTypeChart([], [], []);
            }
        },
        error: function (xhr, status, err) {
            handleApiError(xhr, status, err, "Error loading family subtype data");
        }
    });
}
let salesPieChart; // global chart instance
function renderFamilySubTypeChart(labels, values, percents) {
    const ctx = document.getElementById('salesPie');

    if (salesPieChart) salesPieChart.destroy(); // destroy old chart

    salesPieChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: [
                    '#0d6efd',   // Bootstrap Blue
                    '#20c997',   // Teal
                    '#ffc107',   // Yellow
                    '#fd7e14',   // Orange
                    '#6c757d'    // Gray
                ],
                borderWidth: 2
            }]
        },
        options: {
            cutout: '65%',
            responsive: true,
            plugins: {
                legend: {
                    position: 'right',
                    labels: {
                        generateLabels(chart) {
                            const data = chart.data;
                            return data.labels.map((label, i) => ({
                                text: wrapText(`${label} (${percents[i]})`, 20), // wrap at 20 chars
                                fillStyle: data.datasets[0].backgroundColor[i]
                            }));
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const i = context.dataIndex;
                            return `${labels[i]}: ${values[i]} (${percents[i]})`;
                        }
                    }
                },
                datalabels: {
                    display: false
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}
//5. Family & Sub type of Event -- END ------------------------

//6. Overall Bookings Statistics  --- START ------------------------
function loadYearDropdown() {
    $.ajax({
        url: "/api/YearMaster/GetYearList",
        type: "GET",
        success: function (response) {

            if (response.status === "Success" && response.data.length > 0) {

                let html = "";
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
                });

                $("#yearDropdownMenu").html(html);

                // 🔥 Set default dropdown text
                if (selectedYearId) {
                    $("#yearDropdown").text(currentYear);
                    loadOverallBookingsStatistics(selectedYearId, selectedYearName);
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

    console.log("Selected YearId:", selectedYearId);

    loadOverallBookingsStatistics(selectedYearId, yearText);
});

let bookingBarChart;
function loadOverallBookingsStatistics(yearId, yearName) {
    $.ajax({
        url: '/api/Dashboard/Dashboard_GetOverallBookingsStatistics',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            YearId: yearId
        }),
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

                                            <div style="font-size:13px; color:#f39c12;">
                                                Booking Qty:
                                                <span style="float:right; font-weight:600;">
                                                    ${row.BookingQty}
                                                </span>
                                            </div>

                                            <div style="font-size:13px; color:#374151; margin-top:4px;">
                                                Total Amount:
                                                <span style="float:right; font-weight:600;">
                                                    ₹${row.TotalAmount.toLocaleString()}
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

//7. Lead Source -START-------------------------------------------
let leadSourceChart = null;
// Dropdown filter click
$(".pie-chart-filter .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".pie-chart-filter .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterValue = $(this).data("value");
    $("#leadSourceFilterBtn").text($(this).text());

    loadLeadSourceChart(filterValue);
});

// API Loader
function loadLeadSourceChart(monthDuration, fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/Dashboard/GetDashboardLeadSourceCount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (response) {

            if (response.status === "Success" && response.data.length > 0) {
                renderLeadSourceChart(response.data);
            } else {
                renderLeadSourceChart([]);
            }
        },
        error: function (xhr, status, err) {
            handleApiError(xhr, status, err, "Error loading Lead Source data");
        }
    });
}

function renderLeadSourceChart(apiData) {

    const labels = [];
    const values = [];
    const percents = [];

    if (apiData.length === 0) {
        labels.push("No Data");
        values.push(1);
        percents.push("0%");
    }
    else {
        apiData.forEach(item => {
            labels.push(item.LeadSourceName);
            values.push(item.LeadSourceCount);
            percents.push(item.LeadSourcePercent);
        });
    }

    const ctx = document.getElementById("leadSourceChart").getContext("2d");

    if (leadSourceChart !== null) {
        leadSourceChart.destroy();
    }

    let chartColors = []; // ✅ store colors

    leadSourceChart = new Chart(ctx, {
        type: "doughnut",
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: function (context) {
                    const chart = context.chart;
                    const { ctx, chartArea } = chart;

                    if (!chartArea) return null;

                    // ✅ generate once and store
                    if (chartColors.length === 0) {
                        chartColors = generateGradientColors(ctx, chartArea, labels.length);
                    }

                    return chartColors;
                },
                borderWidth: 2,
                borderColor: "#ffffff"
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: "65%",
            plugins: {
                legend: {
                    position: "bottom",
                    labels: {
                        generateLabels: function () {
                            return labels.map((label, i) => ({
                                //text: `${label} (${values[i]} | ${percents[i]})`,
                                text: `${label} (${percents[i]})`,
                                fillStyle: chartColors[i], // ✅ correct color
                                strokeStyle: "#fff",
                                index: i
                            }));
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const i = context.dataIndex;
                            return `${labels[i]}: ${values[i]} (${percents[i]})`;
                        }
                    }
                }
            }
        }
    });
}

function generateGradientColors(ctx, chartArea, count) {

    const gradientPairs = [
        ["#7F7FD5", "#6C63FF"],  // Purple
        ["#f7971e", "#ff6a00"],  // Orange
        ["#43cea2", "#185a9d"],  // Teal-Blue
        ["#ff758c", "#ff7eb3"],  // Pink
        ["#00c6ff", "#0072ff"],  // Sky Blue
        ["#11998e", "#38ef7d"],  // Green
        ["#ee0979", "#ff6a00"],  // Magenta-Orange
        ["#8e2de2", "#4a00e0"]   // Deep Purple
    ];

    return Array.from({ length: count }, (_, i) => {

        const pair = gradientPairs[i % gradientPairs.length];

        const gradient = ctx.createLinearGradient(
            0,
            chartArea.top,
            0,
            chartArea.bottom
        );

        gradient.addColorStop(0, pair[0]);
        gradient.addColorStop(1, pair[1]);

        return gradient;
    });
}

//7. Lead Source -END-------------------------------------------

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

//8 Revenue Chart - END -------------------------------------------

//9 Lead Lost Chart - START -------------------------------------------
let leadLostChart = null;

/* ─────────────────────────────────────────────
   Dropdown filter click (Lead Lost)
───────────────────────────────────────────── */
$("#leadLostFilterBtn").closest(".pie-chart-filter").find(".dropdown-item").on("click", function (e) {
    e.preventDefault();

    const container = $(this).closest(".pie-chart-filter");

    container.find(".dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterValue = $(this).data("value");

    $("#leadLostFilterBtn").text($(this).text());

    loadLeadLostChart(filterValue);
});

/* ─────────────────────────────────────────────
   API Loader
───────────────────────────────────────────── */
function loadLeadLostChart(monthDuration, fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };

    $.ajax({
        url: "/api/Dashboard/GetDashboardLeadLostCount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {

            if (response.status === "Success" && response.data.length > 0) {
                renderLeadLostChart(response.data);
            } else {
                renderLeadLostChart([]);
            }
        },
        error: function (xhr, status, err) {
            handleApiError(xhr, status, err, "Error loading Lead Lost data");
        }
    });
}


/* ─────────────────────────────────────────────
   Chart Renderer
───────────────────────────────────────────── */
function renderLeadLostChart(apiData) {

    const labels = [];
    const values = [];
    const percents = [];

    if (apiData.length === 0) {
        labels.push("No Data");
        values.push(0);
        percents.push("0%");
    } else {
        apiData.forEach(item => {
            labels.push(item.LeadLostCategoryName);
            values.push(item.LeadLostCount);
            percents.push(item.LeadLostPercent);
        });
    }

    const ctx = document.getElementById("leadLostChart").getContext("2d");

    if (leadLostChart !== null) {
        leadLostChart.destroy();
    }

    let chartColors = [];

    leadLostChart = new Chart(ctx, {
        type: "doughnut",
        data: {
            labels: labels,
            datasets: [{
                data: values,
                backgroundColor: function (context) {

                    const chart = context.chart;
                    const { ctx, chartArea } = chart;

                    if (!chartArea) return null;

                    if (chartColors.length === 0) {
                        chartColors = generateGradientColors(ctx, chartArea, labels.length);
                    }

                    return chartColors;
                },
                borderWidth: 2,
                borderColor: "#ffffff"
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: "65%",
            plugins: {

                legend: {
                    position: "bottom",
                    labels: {
                        generateLabels: function () {
                            return labels.map((label, i) => ({
                                //text: `${label} (${values[i]} | ${percents[i]})`,
                                text: `${label} (${percents[i]})`,
                                fillStyle: chartColors[i],
                                strokeStyle: "#fff",
                                index: i
                            }));
                        }
                    }
                },

                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const i = context.dataIndex;
                            return `${labels[i]}: ${values[i]} (${percents[i]})`;
                        }
                    }
                }

            }
        }
    });
}
//9 Lead Lost Chart - END -------------------------------------------




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



