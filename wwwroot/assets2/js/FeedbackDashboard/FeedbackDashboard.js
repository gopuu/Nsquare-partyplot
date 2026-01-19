$(document).ready(function () {
    loadFeedbackInsightSummary();
    loadRatingDistribution();
    loadCategoryRatings();
});

function loadFeedbackInsightSummary() {

    $.ajax({
        url: '/api/FeedbackDashboard/GetFeedbackInsightSummaryCount',
        type: 'POST',
        contentType: 'application/json',
        success: function (res) {

            if (res.status !== "Success" || !res.data) {
                console.warn(res.message);
                return;
            }

            let data = res.data;

            /* ===== METRIC CARDS ===== */
            if (data.MetricCards && data.MetricCards.length > 0) {

                data.MetricCards.forEach(item => {

                    switch (item.MetricKey) {
                        case "feedback":
                            $('#lblTotalFeedback').text(item.MetricTitle);
                            $('#valTotalFeedback').text(item.MetricValue);
                            break;

                        case "rating":
                            $('#lblAvgRating').text(item.MetricTitle);
                            $('#valAvgRating').text(item.MetricValue);
                            break;

                        case "ai":
                            $('#lblAI').text(item.MetricTitle);
                            $('#valAI').text(item.MetricValue);
                            break;
                    }
                });
            }

            /* ===== SENTIMENT ===== */
            if (data.Sentiment) {
                $('#valPositive').text(data.Sentiment.Positive + '%');
                $('#valNegative').text(data.Sentiment.Negative + '%');
                $('#valNeutral').text(data.Sentiment.Neutral + '%');
            }

        },
        error: function (err) {
            console.error("AJAX Error", err);
        }
    });
}

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

function loadCategoryRatings() {
    $.ajax({
        url: '/api/FeedbackDashboard/GetCategoryRatingSummary', // your API endpoint
        type: 'POST',
        success: function (res) {
            if (res.status !== "Success" || !Array.isArray(res.data)) {
                console.warn(res.message);
                return;
            }

            let container = $('#rating-container');
            container.empty(); // clear existing rows

            res.data.forEach(function (item) {
                let rowHtml = `
                    <div class="rating-row">
                        <div class="category-name">${item.CategoryName}</div>
                        <div class="avg-box bg-success">
                            <div class="avg-title">Avg Rating</div>
                            <div class="avg-value">${item.AvgRating.toFixed(2)}</div>
                            <div class="avg-sub">Out of 5</div>
                        </div>
                        <div class="rating-stat positive">😊 <span>${item.PositiveCount}</span> <small>Positive</small></div>
                        <div class="rating-stat neutral">😐 <span>${item.NeutralCount}</span> <small>Neutral</small></div>
                        <div class="rating-stat negative">☹️ <span>${item.NegativeCount}</span> <small>Negative</small></div>
                    </div>
                `;
                container.append(rowHtml);
            });
        },
        error: function (err) {
            console.error("AJAX Error", err);
        }
    });
}
