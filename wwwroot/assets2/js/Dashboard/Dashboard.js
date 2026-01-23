
$(document).ready(function () {
    setCurrentMonthDates();
    loadDashboardWidgets();
    loadJobPieChart();
    loadEventPieChart();
    loadSalesPieChart();

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

    // --------------------------
    // MonthDuration ke hisab se From/To date set karna
    // --------------------------
    let today = new Date();
    let fromDate, toDate;

    if (selectedMonthDuration === "thismonth") {
        fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
        toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    } else if (selectedMonthDuration === "lastmonth") {
        fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        toDate = new Date(today.getFullYear(), today.getMonth(), 0);
    } else if (selectedMonthDuration === "days") { // Last 7 Days
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 6);
        toDate = today;
    } else if (selectedMonthDuration === "lastdays") { // Last 30 Days
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 29);
        toDate = today;
    } else if (selectedMonthDuration === "yesterday") {
        fromDate = new Date(today);
        fromDate.setDate(today.getDate() - 1);
        toDate = fromDate;
    } else if (selectedMonthDuration === "12months") {
        fromDate = new Date(today);
        fromDate.setMonth(today.getMonth() - 12);
        toDate = today;
    } else if (selectedMonthDuration === "customrange") {
        // custom range me datepicker pe user select karega
        return loadDashboardWidgets();
    }

    // Date format yyyy-mm-dd
    function formatDate(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, '0');
        const d = String(date.getDate()).padStart(2, '0');
        return `${y}-${m}-${d}`;
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
        $(".sort-option").removeClass("active");

        loadDashboardWidgets(); // 🔥
    }
});


$("#btnReset").on("click", function () {
    selectedMonthDuration = "thismonth";

    $("#fromDate").val("");
    $("#toDate").val("");

    $("#sortByDropdown").text("Filter : This Month");

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

                //console.log("totalSales", totalSales);
                //console.log("totalEvents", totalEvents);
                //console.log("totalCustomers", totalCustomers);

                $("#totalSales").text(
                    "₹ " + totalSales.toLocaleString("en-IN", {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    })
                );
                $("#totalEvents").text(totalEvents.toLocaleString("en-IN"));
                $("#totalCustomers").text(totalCustomers.toLocaleString("en-IN"));
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
            debugger
            renderJobPieChart(labels, values);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "pie api Error");
        },
    });
}
function renderJobPieChart(labels, values) {
    debugger
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

    $(".chart-filter-eventPie .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterText = $(this).text().toLowerCase().replaceAll(" ", "");
    $("#jobChartFilter").text($(this).text());

    loadEventPieChart(filterText); 
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
    $("#salesChartFilter").text($(this).text());

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

$(document).ready(function () {
    barChart();
     $(window).resize(function () {
        window.barChart.redraw();
        
    });
});
function barChart() {
    window.barChart = Morris.Bar({
        element: 'bar-chart',
        data: [
            { y: 'Jan', a: 100, b: 90 },
            { y: 'Feb', a: 75, b: 65 },
            { y: 'Mar', a: 50, b: 40 },
            { y: 'Apr', a: 60, b: 55 },
            { y: 'May', a: 70, b: 60 },
            { y: 'Jun', a: 75, b: 65 },
            { y: 'Jul', a: 50, b: 40 },
            { y: 'Aug', a: 75, b: 65 },
            { y: 'Sep', a: 85, b: 75 },
            { y: 'Oct', a: 90, b: 80 },
            { y: 'Nov', a: 95, b: 85 },
            { y: 'Dec', a: 100, b: 90 }
        ],

        xkey: 'y',
        ykeys: ['a', 'b'],
        labels: ['Series A', 'Series B'],

        /* 🎨 Multi color (series-wise) */
        barColors: ['#1e88e5', '#ff3321'],

        /* 👇 IMPORTANT for showing all months */
        xLabelAngle: 45,        // rotate labels
        xLabelMargin: 10,
        gridTextSize: 11,

        barGap: 4,
        barSizeRatio: 0.55,

        resize: true,
        redraw: true,
        hideHover: 'auto'
    });
}


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
google.charts.setOnLoadCallback(() => {
    drawPieChart();
});

function drawPieChart() {
    var data = google.visualization.arrayToDataTable([
        ["Type", "Count"],
        ["Kids", 6],
        ["Under Legal Age", 8],
        ["Over Legal Age", 20]
    ]);

    let options = {
        title: "Source of Lead",
        chartArea: { width: "90%", height: "80%" },
        legend: { position: "bottom" }
    };

    let chart = new google.visualization.PieChart(
        document.querySelector(".pie-chart")
    );

    chart.draw(data, options);
}

    
$(document).on("click", "#btnSummary", function () {
    console.log("Summary icon clicked ✔");
});




