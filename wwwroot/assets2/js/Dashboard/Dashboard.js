
$(document).ready(function () {
    loadDashboardWidgets();
});

function loadDashboardWidgets() {
    $.ajax({
        url: "/api/Dashboard/GetDashboardWidgetCounts",
        type: "POST",
        contentType: "application/json",
        success: function (res) {
            if (res.status === "Success") {
                let statsJson = JSON.parse(res.data.DashboardStats);
                //renderDashboardWidgets(statsJson);
                console.log("statsJson fetchdata:", statsJson);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, title = "Error loading widgets!");
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



// chart 1
const ctx = document.getElementById('jobPie');
new Chart(ctx, {
    type: 'pie',
    data: {
        labels: [
            'GoJobs',
            'Vault',
            'Freshjobs',
            'TweetMyJob',
            'US Jobs',
            'Monster',
            'CareerBuilder',
            'JobCircle',
            'Careercast',
            'Indeed'
        ],
        datasets: [{
            data: [13, 11.5, 11.1, 10.7, 8.3, 7.9, 17, 7.1, 4, 4.7],
            backgroundColor: [
                '#3366cc',
                '#dc3912',
                '#ff9900',
                '#109618',
                '#990099',
                '#0099c6',
                '#dd4477',
                '#66aa00',
                '#b82e2e',
                '#316395'
            ],
            borderWidth: 1
        }]
    },
    options: {
        plugins: {
            legend: {
                position: 'right',
                labels: {
                    generateLabels(chart) {
                        const data = chart.data;
                        return data.labels.map((label, i) => {
                            return {
                                text: `${label} (${data.datasets[0].data[i]}%)`,
                                fillStyle: data.datasets[0].backgroundColor[i]
                            };
                        });
                    }
                }
            },
            datalabels: {
                color: '#fff',
                font: {
                    weight: 'bold',
                    size: 12
                },
                formatter: (value) => value + '%'
            }
        }
    },
    plugins: [ChartDataLabels]
});


// chart 2
const ctx2 = document.getElementById('eventPie');
new Chart(ctx2, {
    type: 'pie',
    data: {
        labels: [
            'Corporate Events',
            'Family Events',
            'Birthday',
            'Wedding',
            'Conference',
            'Other'
        ],
        datasets: [{
            data: [32, 24, 15, 18, 7, 4],
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
        plugins: {
            legend: {
                position: 'right',
                labels: {
                    generateLabels(chart) {
                        const data = chart.data;
                        return data.labels.map((label, i) => ({
                            text: `${label} - ${data.datasets[0].data[i]}%`,
                            fillStyle: data.datasets[0].backgroundColor[i]
                        }));
                    }
                }
            },
            datalabels: {
                display: false
            }
        }
    }
});

 // chart 3
const ctx3 = document.getElementById('salesPie');
new Chart(ctx3, {
    type: 'doughnut',
    data: {
        labels: [
            'Online',
            'Direct',
            'Partner',
            'Referral',
            'Other'
        ],
        datasets: [{
            data: [38, 22, 18, 12, 10],
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
    }
});

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
    var data = new google.visualization.DataTable();
    // indicates the placeholder's column
    data.addColumn("string", "Topping");

    // indicates that the percentage should be inserted in each piece of the graph
    data.addColumn("number", "slices");

    // set the title and the charts options (width / height)
    let options = {
        title: "Source of Lead",
        width: 500,
        height: 500
    };

    // insert the data
    data.addRows([
        ["Kids", 6],
        ["Under Legal Age", 8],
        ["Over Legal Age", 20]
    ]);

    // select the pie_div
    let pieDiv = document.querySelector(".pie-chart");
    let chart = new google.visualization.PieChart(pieDiv);
    chart.draw(data, options);
}

    


