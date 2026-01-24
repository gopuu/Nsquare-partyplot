$(document).ready(function () {
    //loadFeedbackInsightSummary();
    loadFeedbackCounts("thismonth", "", "");
    loadRatingDistribution();
    //loadCategoryRatings();
});

//function loadFeedbackInsightSummary() {

//    $.ajax({
//        url: '/api/FeedbackDashboard/GetFeedbackInsightSummaryCount',
//        type: 'POST',
//        contentType: 'application/json',
//        success: function (res) {

//            if (res.status !== "Success" || !res.data) {
//                console.warn(res.message);
//                return;
//            }

//            let data = res.data;

//            /* ===== METRIC CARDS ===== */
//            if (data.MetricCards && data.MetricCards.length > 0) {

//                data.MetricCards.forEach(item => {

//                    switch (item.MetricKey) {
//                        case "feedback":
//                            $('#lblTotalFeedback').text(item.MetricTitle);
//                            $('#valTotalFeedback').text(item.MetricValue);
//                            break;

//                        case "rating":
//                            $('#lblAvgRating').text(item.MetricTitle);
//                            $('#valAvgRating').text(item.MetricValue);
//                            break;

//                        case "ai":
//                            $('#lblAI').text(item.MetricTitle);
//                            $('#valAI').text(item.MetricValue);
//                            break;
//                    }
//                });
//            }

//            /* ===== SENTIMENT ===== */
//            if (data.Sentiment) {
//                $('#valPositive').text(data.Sentiment.Positive + '%');
//                $('#valNegative').text(data.Sentiment.Negative + '%');
//                $('#valNeutral').text(data.Sentiment.Neutral + '%');
//            }

//        },
//        error: function (err) {
//            console.error("AJAX Error", err);
//        }
//    });
//}

function loadRatingDistribution() {

    $.ajax({
        url: '/api/FeedbackDashboard/GetRatingDistributionSummary',
        type: 'POST',
        success: function (res) {
            console.log("res here1:",res);

            if (res.status !== "Success" || !Array.isArray(res.data)) {
                console.warn(res.message);
                return;
            }

            $('.rating-chart .rating-col').each(function (index) {

                let item = res.data[index];
                if (!item) return;
                console.log("item here1:", item);

                // BAR HEIGHT (max 100%)
                let barHeight = Math.round(item.RatingPercent * 1.3);

                $(this).find('.bar').css('height', barHeight + 'px');
                $(this).find('.percent').text(item.RatingPercent + '%');
                $(this).find('.count').text(item.RatingCount + ' Ratings');
            });
        },
        error: function (err) {
            console.error("AJAX Error", err);
        }
    });
}

//function loadCategoryRatings() {
//    $.ajax({
//        url: '/api/FeedbackDashboard/GetCategoryRatingSummary', // your API endpoint
//        type: 'POST',
//        success: function (res) {
//            if (res.status !== "Success" || !Array.isArray(res.data)) {
//                console.warn(res.message);
//                return;
//            }

//            let container = $('#rating-container');
//            container.empty(); // clear existing rows

//            res.data.forEach(function (item) {
//                let rowHtml = `
//                    <div class="rating-row">
//                        <div class="category-name">${item.CategoryName}</div>
//                        <div class="avg-box bg-success">
//                            <div class="avg-title">Avg Rating</div>
//                            <div class="avg-value">${item.AvgRating.toFixed(2)}</div>
//                            <div class="avg-sub">Out of 5</div>
//                        </div>
//                        <div class="rating-stat positive">😊 <span>${item.PositiveCount}</span> <small>Positive</small></div>
//                        <div class="rating-stat neutral">😐 <span>${item.NeutralCount}</span> <small>Neutral</small></div>
//                        <div class="rating-stat negative">☹️ <span>${item.NegativeCount}</span> <small>Negative</small></div>
//                    </div>
//                `;
//                container.append(rowHtml);
//            });
//        },
//        error: function (err) {
//            console.error("AJAX Error", err);
//        }
//    });
//}


// 🔹 Function to load feedback counts via AJAX
function loadFeedbackCounts(monthDuration = "thismonth", fromDate = null, toDate = null) {

    $.ajax({
        url: '/api/FeedbackDashboard/GetDashboardFeedbackCounts',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            MonthDuration: monthDuration,
            FromDate: fromDate,
            ToDate: toDate
        }),
        success: function (response) {

            if (response.status === "Success" && response.data) {

                const data = response.data;
                console.log("feedback data:", data);

                /* =======================
                   TOP METRICS
                ======================= */
                $("#valTotalFeedback").text(data.TotalFeedback || 0);
                $("#valAvgRating").text(data.AvgRatingOutOf5 || "0%");

                /* =======================
                   POS / NEG / NEU
                ======================= */
                if (data.JSON_FeedbackReport) {
                    $("#valPositive").text(formatPercent(data.JSON_FeedbackReport.Positive));
                    $("#valNegative").text(formatPercent(data.JSON_FeedbackReport.Negative));
                    $("#valNeutral").text(formatPercent(data.JSON_FeedbackReport.Neutral));
                } else {
                    $("#valPositive, #valNegative, #valNeutral").text("0%");
                }

                /* =======================
                   ⭐ CATEGORY WISE RATING ⭐
                ======================= */
                if (data.JSON_CategoryWiseRating && data.JSON_CategoryWiseRating.length > 0) {
                    renderCategoryRatings(data.JSON_CategoryWiseRating);
                } else {
                    $("#rating-container").html(
                        "<div class='text-muted text-center'>No category data</div>"
                    );
                }

            } else {
                resetFeedbackUI();
            }
        },
        error: function (err) {
            console.error("Error loading feedback counts:", err);
            resetFeedbackUI();
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
            <div class="category-name">
                ${item.CategoryName}
            </div>

            <!-- Center : Avg Rating -->
            <div class="avg-rating-box">
                <div class="avg-text">Avg Rating</div>
                <div class="avg-value">${formatPercent(item.AvgRating)}</div>
                <div class="avg-sub">Out of 5</div>
            </div>

            <!-- Right : Feedback counts -->
            <div class="feedback-box">
                
                <div class="feedback-item positive">
                    <img src="assets2/img/Smiling_Emoji.png">
                    <div class="count">${item.PositiveCount}</div>
                    <div class="label">Positive</div>
                </div>

                <div class="feedback-item neutral">
                    <img src="assets2/img/Neutral_Face.png">
                    <div class="count">${item.NeutralCount}</div>
                    <div class="label">Neutral</div>
                </div>

                <div class="feedback-item negative">
                    <img src="assets2/img/Very_Sad.png">
                    <div class="count">${item.NegativeCount}</div>
                    <div class="label">Negative</div>
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
