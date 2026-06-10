
$(document).ready(function () {
    loadFeedbackCounts("thismonth", "", "");
    loadCategoryWiseRatings("thismonth");
    loadRatingBreakdown("thismonth");
    loadRatingsOverTime("thismonth");
    loadFeedbackDashboard("thismonth");
    loadDashboardEventInsights();

    //Widget count hover
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(el => {
        new bootstrap.Tooltip(el);
    });
});

function loadFeedbackCounts(monthDuration = "thismonth", fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };

    $.ajax({
        url: '/api/FeedbackDashboard/GetDashboardFeedbackCounts',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (response) {
            if (response.status === "Success" && response.data) {
                const data = response.data;
                console.log("feedback data:", data);

                /* =======================
                   TOP METRICS
                ======================= */
                $("#valTotalFeedback").text(data.TotalFeedback || 0);
                $("#valAvgRating").text(data.AvgRatingOutOf5 || "0%");
                $("#valAI").text(data.TotalAssistedReviews || 0);

                /* =======================
                   POS / NEG / NEU
                ======================= */
                if (data.JSON_FeedbackReport) {
                    $("#valPositive").text(data.JSON_FeedbackReport.Positive);
                    $("#valNegative").text(data.JSON_FeedbackReport.Negative);
                    $("#valNeutral").text(data.JSON_FeedbackReport.Neutral);
                } else {
                    $("#valPositive, #valNegative, #valNeutral").text("0%");
                }
            }
                
        },
        error: function (xhr, status, err) {
            handleApiError(xhr, status, err, "Error loading feedback counts");
            resetFeedbackUI();
        }
    });
}

function loadCategoryWiseRatings(monthDuration = "thismonth", fromDate = null, toDate = null) {
    var payload = {
        MonthDuration: monthDuration,
        FromDate: fromDate,
        ToDate: toDate
    };

    $.ajax({
        url: "/api/FeedbackDashboard/GetDashboardFeedbackCounts",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            if (res.status !== "Success" || !res.data || !res.data.JSON_CategoryWiseRating) {
                $("#rating-container").html("<div class='text-muted text-center'>No data found</div>");
                return;
            }
            renderCategoryRatings(res.data.JSON_CategoryWiseRating);
        },
        error: function (xhr, status, err) {
            //handleApiError(xhr, status, err, "Error loading feedback counts");
            $("#rating-container").html("<div class='text-danger text-center'>Error loading data</div>");
        }
    });
}


// 🔹 Dropdown click event (same as event pie)
$(".chart-filter-feedback .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".chart-filter-feedback .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterText = $(this).text().toLowerCase().replaceAll(" ", "");
    $("#jobChartFilter").text($(this).text());

    loadFeedbackCounts(filterText);
    loadFeedbackDashboard(filterText);
});

function formatPercent(val) {
    if (!val) return "0%";

    // "12.98456231%" or "12.98456231" → number
    let num = parseFloat(val.toString().replace('%', ''));

    if (isNaN(num)) return "0%";

    return num.toFixed(2) + "%";
}

function renderCategoryRatings(list) {

    let html = "";

    list.forEach(item => {
        html += `
        <div class="category-rating-row">
            
            <!-- Left : Category Name -->
            <div class="category-name fs-9">
                ${item.CategoryName}
            </div>

            <!-- Center : Avg Rating -->
            <div class="avg-rating-box">
                <div class="avg-text">Avg Rate</div>
                <div class="avg-value fs-8">${item.AvgRating}</div>
                <div class="avg-sub">Out of 5</div>
            </div>

            <!-- Right : Feedback counts -->
            <div class="feedback-box">
                
                <div class="feedback-item positive">
                    <div class="icon fs-6">😊</div>
                    <div class="count text-success fs-9">${item.PositiveCount}</div>
                    <div class="label text-success">Positive</div>
                </div>

                <div class="feedback-item neutral">
                    <div class="icon fs-6">😐</div>
                    <div class="count text-secondary fs-9">${item.NeutralCount}</div>
                    <div class="label text-secondary">Neutral</div>
                </div>

                <div class="feedback-item negative">
                    <div class="icon fs-6">🙁</div>
                    <div class="count text-danger fs-9">${item.NegativeCount}</div>
                    <div class="label text-danger">Negative</div>
                </div>

            </div>
        </div>`;
    });

    $("#rating-container").html(html);
}


function resetFeedbackUI() {
    $("#valTotalFeedback").text(0);
    $("#valAvgRating").text("0%");
    $("#valPositive, #valNegative, #valNeutral").text("0%");
    $("#rating-container").html(
        "<div class='text-muted text-center'>No data</div>"
    );
}

$(".chart-filter-rating .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".chart-filter-rating .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const filterText = $(this).text();
    $("#jobChartFilter").text(filterText);

    const monthDuration = filterText.toLowerCase().replaceAll(" ", "");

    loadCategoryWiseRatings(monthDuration);
});


function loadRatingBreakdown(monthDuration = "thismonth", fromDate = null, toDate = null) {

    $.ajax({
        url: "/api/FeedbackDashboard/GetDashboardRatingBreakdown",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (res) {
            console.log("Rating Breakdown API:", res);

            if (res.status !== "Success" || !res.data || !Array.isArray(res.data.RatingBreakdown)) {
                $("#ratingBreakdownContainer").html("<div class='text-muted text-center w-100'>No data found</div>");
                return;
            }
            renderRatingBreakdown(res.data.RatingBreakdown);
        },
        error: function () {
            $("#ratingBreakdownContainer").html("<div class='text-danger text-center w-100'>Error loading data</div>");
        }
    });
}

function renderRatingBreakdown(list) {
    let html = "";

    list.forEach(item => {
        const percentValue = parseFloat(item.Percentage) || 0;

        html += `
        <div class="rating-col text-center">

            <div class="bar ${item.RatingValue === 5 ? 'highlight' : ''}"
                 style="height:${percentValue.toFixed(2)}%">
            </div>

            <div class="emoji fs-6">${item.IconUrl}</div>

            <div class="percent">${formatPercent(item.Percentage)}</div>
            <div class="count">${item.Count} Ratings</div>
            <div class="star">${item.RatingValue}⭐</div>
        </div>`;
    });

    $("#ratingBreakdownContainer").html(html);
}

$(".chart-filter-RatingBreakdown .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".chart-filter-RatingBreakdown .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const text = $(this).text();
    $("#ratingBreakdownFilter").text(text);

    const duration = text.toLowerCase().replaceAll(" ", "");
    loadRatingBreakdown(duration);
});

function loadRatingsOverTime(monthDuration = "thismonth") {

    $.ajax({
        url: "/api/FeedbackDashboard/GetDashboardRatingsOverTime",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: null,
            ToDate: null
        }),
        success: function (res) {

            console.log("Ratings Over Time:", res);

            if (
                res.status !== "Success" ||
                !res.data ||
                !Array.isArray(res.data.RatingsOverTime)
            ) {
                $("#ratingSpikeArea").html("");
                $("#ratingDateRow").html("");
                return;
            }

            renderRatingSpikes(res.data.RatingsOverTime);
        }
    });
}

function renderRatingSpikes(list) {
    console.log("list:", list);
    const monthMap = {};

    // 🔹 Merge all ratings → month-wise total
    list.forEach(rating => {
        rating.Data.forEach(d => {
            const dateObj = new Date(d.Date);
            const monthKey = dateObj.getFullYear() + "-" + String(dateObj.getMonth() + 1).padStart(2, "0");

            monthMap[monthKey] = (monthMap[monthKey] || 0) + d.Count;
        });
    });

    const months = Object.keys(monthMap).sort();
    const values = Object.values(monthMap);

    if (months.length === 0) {
        $("#ratingSpikeArea").html("");
        $("#ratingDateRow").html("");
        return;
    }

    const maxValue = Math.max(...values);
    const maxHeight = 150;

    let spikeHtml = "";
    let dateHtml = "";

    console.log("maxValue:", maxValue);
    months.forEach(key => {

        const count = monthMap[key];
        console.log("count:", count);

        const height = maxValue === 0 ? 4 : (count / maxValue) * maxHeight;
        spikeHtml += `
            <div class="spike"
                 data-bs-toggle="tooltip"
                 data-bs-placement="top"
                 title="${formatMonth(key)}<br>${count} Ratings"
                 style="height:${height}px">
            </div>`;
        dateHtml += `<span>${formatMonth(key)}</span>`;
    });

    $("#ratingSpikeArea").html(spikeHtml);
    $("#ratingDateRow").html(dateHtml);

    $('[data-bs-toggle="tooltip"]').tooltip({
        html: true
    });
}

$(".chart-filter-ratingtime .dropdown-item").on("click", function (e) {
    e.preventDefault();

    $(".chart-filter-ratingtime .dropdown-item").removeClass("active");
    $(this).addClass("active");

    const text = $(this).text();
    const duration = $(this).data("duration");

    $("#ratingTimeFilter").text(text);

    loadRatingsOverTime(duration);
});
function formatMonthLabel(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleDateString("en-IN", { month: "short" });
}
function formatMonth(monthKey) {
    const [year, month] = monthKey.split("-");
    const date = new Date(year, month - 1, 1);

    return date.toLocaleDateString("en-IN", { month: "short" });
}

function loadFeedbackDashboard(monthDuration = "thismonth") {
    $.ajax({
        url: '/api/FeedbackDashboard/GetFeedbackDashboardHost_Visitors',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: null,
            ToDate: null
        }),

        success: function (response) {
            console.log("Host_Visitors:", response);
            if (response.status === "Success" && response.data.length > 0) {
                response.data.forEach(function (item) {
                    if (item.FeedbackType === "Hosts") {
                        $("#hostTitle").text(item.Title);
                        $("#hostTotal").text(item.TotalFeedbacks);
                        $("#hostAvg").text(item.AverageRating + ' / 5');
                        $("#hostNegative").text(item.NegativeFeedbackPercent + "%");
                    }

                    if (item.FeedbackType === "Visitors") {
                        $("#visitorTitle").text(item.Title);
                        $("#visitorTotal").text(item.TotalFeedbacks);
                        $("#visitorAvg").text(item.AverageRating + ' / 5');
                        $("#visitorNegative").text(item.NegativeFeedbackPercent + "%");
                    }
                });
            }
        },
        error: function (xhr, status, err) {
            handleApiError(xhr, status, err, "Failed to Load Host-Visitors")
        }
    });
}

function loadDashboardEventInsights() {
    $.ajax({
        url: '/api/FeedbackDashboard/GetDashboardEventInsights',
        type: 'POST',
        success: function (response) {

            console.log("API response:", response);
            if (response.status === "Success" && response.data) {

                let d = response.data;

                /* ---------- Card 1 ---------- */
                $('#insightTitle1').text(d.AvgbillingText);
                $('#totalRevenue').text('₹' + d.AvgbillingValue);
                $('#totalVisits').text(d.AvgbillingValue_PAX); // example static

                /* ---------- Card 2 ---------- */
                $('#RevenueText').text(d.RevenueDistributionText);
                $('#customerPercentValue').text(d.RevenueDistribution_CustomerValue + '%');
                $('#revenuePercent').text(d.RevenueDistribution_TotalValue + '%');

                /* ---------- Card 3 ---------- */
                $('#avgordertext').text(d.CorporateEventText);
                $('#avgOrderValueAmount').text('₹' + d.CorporateEventValue);
                $('#avgItems').text(d.CorporateEventValue_Count); // static example

                /* ---------- Card 4 ---------- */
                $('#customervisittext').text(d.FamilyeventbillingText);
                $('#visitFrequencyValue').text('₹' + d.Familyeventbillingvalue);
            }
        },
        error: function (err) {
            console.error("Error loading dashboard insights", err);
        }
    });
}



   


