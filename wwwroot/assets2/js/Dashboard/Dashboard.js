
$(document).ready(function () {
    setCurrentMonthDates();
    loadDashboardWidgets();
    loadJobPieChart();
    loadEventPieChart();
    loadSalesPieChart();
    loadYearDropdown();
    loadPnLChart("last3months");

    const currentYear = new Date().getFullYear();
    renderBarChart(currentYear);
    $("#yearDropdown").text(currentYear);

});
function setCurrentMonthDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#fromDate").val(formatDate(firstDay));
    $("#toDate").val(formatDate(lastDay));
}
function formatDate(date) {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;   // ✅ IMPORTANT
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
    $(".sub").text("In " + $(this).text());

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
    else if (selectedMonthDuration === "days") { // Last 7 Days
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 6);
        toDate = today;
    }
    else if (selectedMonthDuration === "lastdays") { // Last 30 Days
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 29);
        toDate = today;
    }
    else if (selectedMonthDuration === "yesterday") {
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 1);
        toDate = fromDate;
    }
    else if (selectedMonthDuration === "12months") {
        fromDate = new Date(today);
        fromDate.setMonth(today.getMonth() - 12);
        toDate = today;
    }
    else if (selectedMonthDuration === "customrange") {
        // custom range me datepicker pe user select karega
        return loadDashboardWidgets();
    }

    // Set the datepickers
    $("#fromDate").val(formatDate(fromDate));
    $("#toDate").val(formatDate(toDate));

    // 🔥 Load data
    loadDashboardWidgets();
});


$("#fromDate, #toDate").on("change", function () {

    let fromDate = $("#fromDate").val();
    let toDate = $("#toDate").val();

    if (fromDate && toDate) {
        selectedMonthDuration = "customrange";

        $("#sortByDropdown").text("Filter : Custom Range");
        $(".sub").text("In Custom Range");

        $(".sort-option").removeClass("active");

        loadDashboardWidgets(); // 🔥
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

    loadDashboardWidgets();
});
function loadDashboardWidgets() {
    let requestObj = {
        MonthDuration: selectedMonthDuration, // eg: thismonth, lastmonth
        FromDate: $("#fromDate").val(),        // yyyy-mm-dd
        ToDate: $("#toDate").val()             // yyyy-mm-dd
    };
    console.log("requestObj DashboardWidgets:", requestObj);

    $.ajax({
        url: "/api/Dashboard/GetDashboardWidgetCounts2",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestObj),
        success: function (res) {
            if (res.status === "Success") {
                // Parse the JSON string returned by the API
                //let statsJson = JSON.parse(res.data.DashboardStats);
                console.log("Dashboard Stats:", res.data);

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

                $("#totalEventsLost").text(
                    "₹ " + totalEventsLost.toLocaleString("en-IN", {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    })
                );
                $("#eventLostCount").text("(" + eventLostCount + ")");

                console.log("Dashboard Stats Fetched:", res.data);

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
            console.error("AJAX Error:", status, error, xhr.responseText);
            alert("Error loading dashboard widgets! " + error);
        }
    });
}
function renderDashboardWidgets(stats) {

    const container = $("#dashboard-widgets");
    container.empty();

    // Map KPI → Icons + Colors + Labels
    const widgetConfig = {
        TotalSales: { label: "Total Sales", icon: "fa-coins", bg: "#ffdddd" },
        TotalOrders: { label: "Total Orders", icon: "fa-cart-shopping", bg: "#ddffdd" },
        TotalCustomer: { label: "Total Customers", icon: "fa-users", bg: "#e0e7ff" },
        RewardsRedeemed: { label: "Rewards Redeemed", icon: "fa-gift", bg: "#e0f7ff" },
        TotalEvents: { label: "Total Events", icon: "fa-calendar-days", bg: "#f3e8ff" },

        // TotalCustomer_New: { label: "New Customers", icon: "fa-user-plus", bg: "#ede9fe" },
        // TotalCustomer_Repeat: { label: "Repeat Customers", icon: "fa-user-check", bg: "#fff7cc" },

        TotalLoss_NoOfEvents: { label: "Loss Events", icon: "fa-triangle-exclamation", bg: "#fff0e6" },
        TotalLoss_RevenueLoss: { label: "Loss Amount", icon: "fa-indian-rupee-sign", bg: "#ffe6e6" },

        UpcomingBookings_NoOfBookings: { label: "Upcoming Bookings", icon: "fa-clock", bg: "#e6f7ff" },
        UpcomingBookings_ExpectedRevenue: { label: "Upcoming Revenue", icon: "fa-wallet", bg: "#e8ffe8" }
    };

    Object.keys(stats).forEach(key => {
        if (key === "TotalCustomer_New" || key === "TotalCustomer_Repeat") return;

        // ==============================
        // SPECIAL WIDGET: TOTAL CUSTOMERS
        // ==============================
        if (key === "TotalCustomer") {

            const total = stats.TotalCustomer;
            const newCust = stats.TotalCustomer_New || 0;
            const repeatCust = stats.TotalCustomer_Repeat || 0;

            const totalValue = Number(total) || 1;
            const pctNew = (newCust / totalValue) * 100;
            const pctRepeat = (repeatCust / totalValue) * 100;

            const widgetHtml = `
                    <div class="col-12 col-md-6 col-xl-3">
                        <div class="dashboard-widget" style="width:100%;">

                            <div>
                                <p class="widget-value">${Number(total).toLocaleString("en-IN")}</p>
                                <p class="widget-title">${widgetConfig[key].label}</p>

                                <!-- Top-right labels -->
                                <div class="d-none" style="display:flex; justify-content:flex-start; gap:16px; margin-bottom:4px;">

                                    <div style="display:flex; align-items:center; gap:4px;">
                                        <span style="
                                            width:10px; height:10px; border-radius:50%;
                                            background:#7c3aed; display:inline-block;">
                                        </span>
                                        <span style="font-size:0.75rem; color:#444;">New</span>
                                    </div>

                                    <div style="display:flex; align-items:center; gap:4px;">
                                        <span style="
                                            width:10px; height:10px; border-radius:50%;
                                            background:#fbbf24; display:inline-block;">
                                        </span>
                                        <span style="font-size:0.75rem; color:#444;">Repeat</span>
                                    </div>

                                </div>

                                <!-- Progress Bar -->
                                <div style="width:200px; height:8px; background:#eee; border-radius:6px; overflow:hidden; margin:4px 0 6px 0;">
                                    <div style="width:${pctNew}%; height:100%; background:#7c3aed; float:left;"></div>
                                    <div style="width:${pctRepeat}%; height:100%; background:#fbbf24; float:left;"></div>
                                </div>

                                <!-- Numbers under bar -->
                                <div style="display:flex; justify-content:space-between; width:200px; font-size:0.8rem; font-weight:600; color:#555;">
                                    <span>${newCust} New</span>
                                    <span>${repeatCust} Repeat</span>
                                </div>

                            </div>

                        </div>
                    </div>
                `;

            container.append(widgetHtml);
            return; // skip normal widget rendering
        }

        // ==============================
        // NORMAL WIDGETS
        //==============================
        if (!widgetConfig[key]) return;

        const cfg = widgetConfig[key];
        const value = stats[key];

        const widgetHtml = `
                <div class="col-12 col-md-6 col-xl-3">
                    <div class="dashboard-widget">
                        <div class="widget-icon" style="background:${cfg.bg}">
                            <i class="fa-solid ${cfg.icon}"></i>
                        </div>
                        <div>
                            <p class="widget-value">${formatValue(key, value)}</p>
                            <p class="widget-title">${cfg.label}</p>
                        </div>
                    </div>
                </div>
            `;

        container.append(widgetHtml);
    });

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
})


let jobPieChart = null;
function loadJobPieChart() {

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
            console.log("pie res:",res);
            if (res.status !== "Success") {
                console.warn(res.message);
                return;
            }

            let labels = [];
            let values = [];

            res.data.forEach(item => {
                labels.push(item.EventLabel);
                values.push(item.EventCount);
            });
           
            renderJobPieChart(labels, values);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "pie api Error");
        },
    });
}
function renderJobPieChart(labels, values) {
    
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
                                text: `${label} (${data.datasets[0].data[i]})`,
                                fillStyle: data.datasets[0].backgroundColor[i]
                            }));
                        }
                    }
                },
                datalabels: {
                    color: '#fff',
                    font: { weight: 'bold', size: 12 },
                    formatter: (value) => value
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

    loadJobPieChart(); // 🔥 reload chart
});


let eventPieChart; // 🔹 store chart instance globally

// 🔹 Function to render chart
function renderEventPieChart(labels, values) {
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
                                text: `${label} - ${data.datasets[0].data[i]}`,
                                fillStyle: data.datasets[0].backgroundColor[i]
                            }));
                        }
                    }
                },
                datalabels: {
                    color: '#fff',
                    font: { weight: 'bold', size: 12 },
                    formatter: (value) => value
                }
            }
        },
        plugins: [ChartDataLabels] // make sure you include plugin
    });
}

// 🔹 Function to load data via AJAX
function loadEventPieChart(monthDuration = "thismonth", fromDate = null, toDate = null) {
    $.ajax({
        url: '/api/Dashboard/GetBookingSubEventTypeCorporate',
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

                renderEventPieChart(labels, values);
            } else {
                // empty chart
                renderEventPieChart([], []);
            }
        },
        error: function (err) {
            console.error("Error loading pie chart data:", err);
        }
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

    // Call your chart update
    loadEventPieChart(selectedValue);
});


let salesPieChart; // global chart instance

// 🔹 Function to render chart
function renderSalesPieChart(labels, values) {
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
                                text: `${label} (${data.datasets[0].data[i]}%)`,
                                fillStyle: data.datasets[0].backgroundColor[i]
                            }));
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

// 🔹 Function to load data via AJAX
function loadSalesPieChart(monthDuration = "thismonth", fromDate = null, toDate = null) {
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

                renderSalesPieChart(labels, values);
            } else {
                // empty chart
                renderSalesPieChart([], []);
            }
        },
        error: function (err) {
            console.error("Error loading sales pie chart data:", err);
        }
    });
}


// 🔹 Dropdown click event
$(".chart-filter-salepie .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".chart-filter-salepie .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterText = $(this).text().toLowerCase().replaceAll(" ", "");
    $("#salepieChartFilter").text($(this).text());

    // map dropdown to MonthDuration (or custom date logic)
    loadSalesPieChart(filterText);
});


// chart 2
//const ctx2 = document.getElementById('eventPie');
//new Chart(ctx2, {
//    type: 'pie',
//    data: {
//        labels: [
//            'Corporate Events',
//            'Family Events',
//            'Birthday',
//            'Wedding',
//            'Conference',
//            'Other'
//        ],
//        datasets: [{
//            data: [32, 24, 15, 18, 7, 4],
//            backgroundColor: [
//                '#1abc9c',   // teal
//                '#3498db',   // blue
//                '#9b59b6',   // purple
//                '#e67e22',   // orange
//                '#e74c3c',   // red
//                '#95a5a6'    // gray
//            ],
//            borderWidth: 1
//        }]
//    },
//    options: {
//        plugins: {
//            legend: {
//                position: 'right',
//                labels: {
//                    generateLabels(chart) {
//                        const data = chart.data;
//                        return data.labels.map((label, i) => ({
//                            text: `${label} - ${data.datasets[0].data[i]}%`,
//                            fillStyle: data.datasets[0].backgroundColor[i]
//                        }));
//                    }
//                }
//            },
//            datalabels: {
//                display: false
//            }
//        }
//    }
//});

 // chart 3
//const ctx3 = document.getElementById('salesPie');
//new Chart(ctx3, {
//    type: 'doughnut',
//    data: {
//        labels: [
//            'Online',
//            'Direct',
//            'Partner',
//            'Referral',
//            'Other'
//        ],
//        datasets: [{
//            data: [38, 22, 18, 12, 10],
//            backgroundColor: [
//                '#0d6efd',   // Bootstrap Blue
//                '#20c997',   // Teal
//                '#ffc107',   // Yellow
//                '#fd7e14',   // Orange
//                '#6c757d'    // Gray
//            ],
//            borderWidth: 2
//        }]
//    },
//    options: {
//        cutout: '65%',
//        plugins: {
//            legend: {
//                position: 'right',
//                labels: {
//                    generateLabels(chart) {
//                        const data = chart.data;
//                        return data.labels.map((label, i) => ({
//                            text: `${label} (${data.datasets[0].data[i]}%)`,
//                            fillStyle: data.datasets[0].backgroundColor[i]
//                        }));
//                    }
//                }
//            },
//            datalabels: {
//                display: false
//            }
//        }
//    }
//});

//$(document).ready(function () {
//    barChart();
//     $(window).resize(function () {
//        window.barChart.redraw();
        
//    });
//});
//function barChart() {
//    window.barChart = Morris.Bar({
//        element: 'bar-chart',
//        data: [
//            { y: 'Jan', a: 100, b: 90 },
//            { y: 'Feb', a: 75, b: 65 },
//            { y: 'Mar', a: 50, b: 40 },
//            { y: 'Apr', a: 60, b: 55 },
//            { y: 'May', a: 70, b: 60 },
//            { y: 'Jun', a: 75, b: 65 },
//            { y: 'Jul', a: 50, b: 40 },
//            { y: 'Aug', a: 75, b: 65 },
//            { y: 'Sep', a: 85, b: 75 },
//            { y: 'Oct', a: 90, b: 80 },
//            { y: 'Nov', a: 95, b: 85 },
//            { y: 'Dec', a: 100, b: 90 }
//        ],

//        xkey: 'y',
//        ykeys: ['a', 'b'],
//        labels: ['Series A', 'Series B'],

//        /* 🎨 Multi color (series-wise) */
//        barColors: ['#1e88e5', '#ff3321'],

//        /* 👇 IMPORTANT for showing all months */
//        xLabelAngle: 45,        // rotate labels
//        xLabelMargin: 10,
//        gridTextSize: 11,

//        barGap: 4,
//        barSizeRatio: 0.55,

//        resize: true,
//        redraw: true,
//        hideHover: 'auto'
//    });
//}

//$(document).ready(function () {
//   // loadYearDropdown(); // populate year dropdown
//    let selectedYear = new Date().getFullYear();
//    renderBarChart(selectedYear);

//    // Handle year selection
//    $(document).on('click', '#yearDropdownMenu li a', function () {
//        selectedYear = $(this).data('year');
//        $('#yearDropdown').text(selectedYear);
//        renderBarChart(selectedYear);
//    });

//    $(window).resize(function () {
//        if (window.barChart) window.barChart.redraw();
//    });
//});



// lead chart

// Load the Visualization API and the corechart package.
google.charts.load('current', { 'packages': ['corechart'] });

// Set a callback to run when the Google Visualization API is loaded.
google.charts.setOnLoadCallback(drawChart);

// Callback that creates and populates a data table,
// instantiates the pie chart, passes in the data and
// draws it.
function drawChart() {

    // Create the data table.
    var data = new google.visualization.DataTable();
    data.addColumn('string', 'Job Listings');
    data.addColumn('number', 'Jobs');
    data.addRows([
        ['GOjobs', 33],
        ['Vault', 29],
        ['Freshjobs', 28],
        ['TweetMyJob', 27],
        ['US Jobs', 21],
        ['Monster', 20],
        ['CareerBuilder', 43],
        ['JobCircle', 18],
        ['Careercast', 10],
        ['Indeed', 12],
        ['Dice', 12]


    ]);

    // Set chart options
    var options = {
        'title': 'Job listings that closely relate to your resume in Indiana',
        'width': 1000,
        'height': 800
    };

    // Instantiate and draw our chart, passing in some options.
    var chart = new google.visualization.PieChart(document.getElementById('chart_div'));
    chart.draw(data, options);
}

//here1

// init the google charts
google.charts.load("current", { packages: ["corechart"] });

// create a callback function that render the chart
//google.charts.setOnLoadCallback(() => {
//    drawPieChart();
//});
google.charts.setOnLoadCallback(loadLeadSourcePieChart);
function loadLeadSourcePieChart(monthDuration = "thismonth", fromDate = null, toDate = null) {
    $.ajax({
        url: "/api/Dashboard/GetDashboardLeadSourceCount",
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (response) {
            console.log("piechart lead data:", response);
            if (response.status === "Success" && response.data.length > 0) {
                drawLeadSourcePieChart(response.data);
            } else {
                drawLeadSourcePieChart([]);
            }
        },
        error: function (err) {
            console.error("Error loading sales pie chart data:", err);
        }
    });
}

function drawLeadSourcePieChart(apiData) {

    let chartArray = [["Lead Source", "Count"]];

    apiData.forEach(item => {
        chartArray.push([
            item.LeadSourceName,   // 👈 EXACT property name
            item.LeadSourceCount
        ]);
    });

    // Handle empty data
    if (chartArray.length === 1) {
        chartArray.push(["No Data", 1]);
    }

    var data = google.visualization.arrayToDataTable(chartArray);

    let options = {
        title: "Lead Source",
        chartArea: { width: "90%", height: "80%" },
        legend: { position: "bottom" },
        pieHole: 0.4
    };

    let chart = new google.visualization.PieChart(
        document.querySelector(".pie-chart")
    );

    chart.draw(data, options);
}

$(".pie-chart-filter .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".pie-chart-filter .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterText = $(this).text().toLowerCase().replaceAll(" ", "");
    $("#filterChartFilter").text($(this).text());

    loadLeadSourcePieChart(filterText);
});

//function drawPieChart() {
//    var data = google.visualization.arrayToDataTable([
//        ["Type", "Count"],
//        ["Kids", 6],
//        ["Under Legal Age", 8],
//        ["Over Legal Age", 20]
//    ]);

//    let options = {
//        title: "Source of Lead",
//        chartArea: { width: "90%", height: "80%" },
//        legend: { position: "bottom" }
//    };

//    let chart = new google.visualization.PieChart(
//        document.querySelector(".pie-chart")
//    );

//    chart.draw(data, options);
//}
    
$(document).on("click", "#btnSummary", function () {
    console.log("Summary icon clicked ✔");
});
function loadYearDropdown() {
    $.ajax({
        url: "/api/YearMaster/GetYearList",
        type: "GET",
        success: function (response) {
            if (response.status === "Success" && response.data.length > 0) {

                let html = "";

                // 🔹 FIRST OPTION → This Year
                //html += `
                //    <li>
                //        <a class="dropdown-item year-option active"
                //           href="#"
                //           data-id="0"
                //           data-year="-">
                //           This Year
                //        </a>
                //    </li>`;

                // 🔹 API YEARS
                response.data.forEach(function (item) {
                    html += `
                        <li>
                            <a class="dropdown-item year-option"
                               href="#"
                               data-id="${item.Id}"
                               data-year="${item.YearName}">
                               ${item.YearName}
                            </a>
                        </li>`;
                });

                $("#yearDropdownMenu").html(html);

                // 🔹 Default button text
               // $("#yearDropdown").text("This Year");
            }
        },
        error: function (err) {
            console.error("Error loading year list", err);
        }
    });
}

// Render Bar Chart
function renderBarChart(year) {
    
    $.ajax({
        url: '/api/Dashboard/GetDashboardEventSummaryYearWise',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            SelectedYear: year,
            IsThisYear: year === new Date().getFullYear()
        }),
        success: function (res) {
            if (res.status === 'Success') {
                const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                const currentMonth = new Date().getMonth(); // 0–11

                const dataMap = {};
                res.data.forEach(d => {
                    dataMap[d.MonthNo] = d;
                });

                const chartData = months.map((m, i) => {
                    const d = dataMap[i + 1];
                    const TotalAmount = d && d.TotalAmount ? d.TotalAmount : 0; // instead of 0
                    const BookingQty = d && d.BookingQty ? d.BookingQty : 0; // instead of 0
                    const TotalEvents = d && d.TotalEvents ? d.TotalEvents : 0; // instead of 0
                    const FamilyEvents = d && d.TotalFamilyFunctionCount ? d.TotalFamilyFunctionCount : 0;
                    const CorporateEvents = d && d.TotalCorporateCount ? d.TotalCorporateCount : 0;

                    return { y: m, TotalAmount ,BookingQty, TotalEvents, FamilyEvents, CorporateEvents };
                });


                $('#bar-chart').empty();

                Morris.Bar({
                    element: 'bar-chart',
                    data: chartData,
                    xkey: 'y',
                    ykeys: ['TotalEvents', 'FamilyEvents', 'CorporateEvents'], // all 3 bars
                    labels: ['Total Events', 'Family Functions', 'Corporate Events'],
                    barColors: ['#f39c12', '#8e44ad', '#e74c3c'],
                    hideHover: 'auto',
                    resize: true,
                    xLabelAngle: 45,
                    barGap: 4,
                    barSizeRatio: 0.55,
                    hoverCallback: function (index, options, content, row) {
                        return `
                            <strong>${row.y} (${year})</strong><br/>
                            Booking Qty: ${row.BookingQty}<br/>
                            Total Amount: ${row.TotalAmount}<br/>
                            <span style="color:#f39c12;">Total Events: ${row.TotalEvents}</span><br/>
                            <span style="color:#8e44ad;">Family Functions: ${row.FamilyEvents}</span><br/>
                            <span style="color:#e74c3c;">Corporate Events: ${row.CorporateEvents}</span>
                        `;
                    }
                });
            }
        }
    });

}


$(document).on("click", ".year-option", function (e) {
    e.preventDefault();

    $(".year-option").removeClass("active");
    $(this).addClass("active");

    let yearText = $(this).data("year");
    let selectedYear;

    if (yearText === "thisyear") {
        selectedYear = new Date().getFullYear();
        $("#yearDropdown").text("This Year");
    } else {
        selectedYear = parseInt(yearText);
        $("#yearDropdown").text(yearText);
    }

    console.log("Selected Year:", selectedYear);

    renderBarChart(selectedYear);
});





function loadPnLChart(filter) {

    const monthCount = filter.replace("last", "").replace("months", "");
    $("#pnlFilterBtn").text(`Last ${monthCount} Months`);
   // console.log("pnl inputdata:", filter);
    $.ajax({
        url: "/api/Dashboard/GetMonthlyPnLChart",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Filter: filter }),
        success: function (res) {
            console.log("pnl res:", res);

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
    { key: "Revenue", percent: null, color: "#ff9800" },
    { key: "Purchase", percent: "PurchasePercent", color: "#f44336" },
    { key: "Salary", percent: "SalaryPercent", color: "#9c27b0" },
    { key: "Rent", percent: "RentPercent", color: "#4caf50" },
    { key: "Electricity", percent: "ElectricityPercent", color: "#03a9f4" },
    { key: "Gas", percent: "GasPercent", color: "#795548" },
    { key: "OtherExpense", percent: "OtherExpensePercent", color: "#607d8b" },
    { key: "ProfitAndLoss", percent: "ProfitPercent", color: "#000000" }
];

function buildPnLCategoryMonthChart(apiData) {

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


let pnlCategoryChart;

function renderPnLCategoryChart(labels, datasets) {

    if (pnlCategoryChart) pnlCategoryChart.destroy();

    pnlCategoryChart = new Chart(
        document.getElementById("pnlCategoryChart"),
        {
            type: "bar",
            data: { labels, datasets },
            options: {
                responsive: true,
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
                }
            }
        }
    );
}



document.addEventListener('DOMContentLoaded', function () {
    const tooltipContent = {
        tooltipTotalSales: `
            <div class='tooltip-title'>
                Total value of sales generated during the selected time period.
                (This data comes from the POS, cashier portal or any transactions you have imported)
            </div>
            <ul class='tooltip-list'>
                <li>All orders – customers with valid, invalid, blocked, and no mobile number orders are all included.</li>
                <li>Pending and voided bills are excluded.</li>
                <li>The data is valid until 11.59 PM of the selected time.</li>
            </ul>
        `,
        tooltipTotalOrders: `
            <div class='tooltip-title'>
                Total number of orders placed during the selected time period.
                (This data comes from your POS or imported transactions)
            </div>
            <ul class='tooltip-list'>
                <li>Includes all valid, invalid, blocked, and no mobile number orders.</li>
                <li>Pending and voided orders are excluded.</li>
                <li>Data is valid until 11:59 PM of the selected day.</li>
            </ul>
        `,
        tooltipTotalCustomers: `
            <div class='tooltip-title'>
                Total number of customer visits recorded during the selected time period.
                (This includes all visits tracked in the system)
            </div>
            <ul class='tooltip-list'>
                <li>Includes first-time and repeat customer visits.</li>
                <li>Only valid check-ins are counted; canceled or invalid visits are excluded.</li>
                <li>Data is accurate as of 11:59 PM of the selected day.</li>
            </ul>
        `,
        tooltipEventsLost: `
            <div class='tooltip-title'>
                Total number of events lost or not completed during the selected time period.
            </div>
            <ul class='tooltip-list'>
                <li>Includes cancelled, rejected, and unconfirmed events.</li>
                <li>Only valid tracked events are counted; duplicates or test events are excluded.</li>
                <li>Data is accurate as of 11:59 PM of the selected day.</li>
            </ul>
        `
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
});




