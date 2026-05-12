/* ================= GLOBALS ================= */
let steps = [];
let currentStepIndex = 0;
let feedbackResponses = {};

let bookingId = 0;
let customerName = "";
let whatsappNo = "";

/* ================= INIT ================= */
$(document).ready(function () {
    // Decode URL params
    const params = new URLSearchParams(window.location.search);
    if (params.has("bid")) bookingId = atobFix(params.get("bid"));
    if (params.has("n")) customerName = atobFix(params.get("n"));
    if (params.has("w")) whatsappNo = atobFix(params.get("w"));

    loadFeedbackForm();
});

/* ================= LOAD FORM ================= */
function loadFeedbackForm() {
    $.get("/api/Feedback/GetFeedbackFormData", function (res) {
        console.log("res:", res);
        steps = res.data.sort((a, b) => a.StepOrder - b.StepOrder);
        console.log("res steps:", steps);
        currentStepIndex = 0;
        feedbackResponses = {};
        renderStep();
    });
}

/* ================= RENDER STEP ================= */
function renderStep() {
    const step = steps[currentStepIndex];

    $("#stepTitle").text(step.StepTitle);
    $("#stepsContainer").empty();

    step.HasTextBox ? renderTextBox(step) : renderOptions(step);
    //updateNavButtons();
}
function moveToNextStep() {
    if (currentStepIndex === steps.length - 1) {
        submitFeedback();
    }
    else {
        currentStepIndex++;
        renderStep();
    }
}

/* ================= OPTIONS ================= */
function renderOptions(step) {
    //const container = $('<div class="option-grid"></div>');
    const container = $('<div class="d-flex justify-content-center gap-4 flex-nowrap option-row"></div>');

    step.Options.forEach(opt => {
        const card = $(`
                    <div class="option-card" data-id="${opt.OptionId}">
                        <img src="${encodeURI(opt.IconUrl)}">
                        <div class="option-text">${opt.OptionName}</div>
                    </div>
                `);

        card.on("click", function () {

            if (!step.IsMultiSelect) {
                container.find(".option-card").removeClass("active");
                feedbackResponses[step.StepId] = [];
            }

            $(this).toggleClass("active");

            const selected = container.find(".option-card.active")
                .map((_, el) => $(el).data("id"))
                .get();

            feedbackResponses[step.StepId] = selected;

            const isSingleSelect = !step.IsMultiSelect;

            // ================= SINGLE SELECT =================
            if (isSingleSelect) {
                setTimeout(() => {
                    moveToNextStep();
                }, 300);
            }
            // ================= MULTI SELECT =================
            else {
                // Remove old timer if exists
                clearInterval(window.multiSelectTimer);

                // Remove old countdown UI
                $("#multiSelectCountdown").remove();

                let seconds = 3;

                // Countdown UI
                $("#stepsContainer").append(`
                    <div id="multiSelectCountdown"
                         class="text-center mt-4 text-primary fw-bold fs-5">
                        Moving next in ${seconds}...
                    </div>
                `);

                window.multiSelectTimer = setInterval(() => {
                    seconds--;
                    $("#multiSelectCountdown").text(`Moving next in ${seconds}...`);
                    if (seconds <= 0) {
                        clearInterval(window.multiSelectTimer);
                        moveToNextStep();
                    }
                }, 1000);
            }

            $("#hiddenMessage").remove();
            if (opt.HiddenMessage) {

                let hintClass = "neutral";

                if (opt.DisplayOrder <= 2) {
                    hintClass = "positive";
                }
                else if (opt.DisplayOrder >= 4) {
                    hintClass = "negative";
                }
                let icon = "fa-heart";
                if (hintClass === "negative") icon = "fa-face-sad-tear";
                if (hintClass === "neutral") icon = "fa-face-meh";

                $("#stepsContainer").append(`
                            <div id="hiddenMessage" class="feedback-hint ${hintClass} mt-4">
                                <i class="fa-solid ${icon}"></i>
                                <span>${opt.HiddenMessage}</span>
                            </div>
                        `);
            }

            console.log("feedbackResponses:", feedbackResponses);
        });

        container.append(card);
    });

    $("#stepsContainer").append(container);
}

/* ================= TEXTBOX ================= */
function renderTextBox2(step) {
    const textarea = $(`<textarea class="form-control" rows="4"
                placeholder="Enter your feedback"></textarea>`);

    textarea.on("input", function () {
        feedbackResponses[step.StepId] = $(this).val();
    });

    $("#stepsContainer").append(textarea);
}
function renderTextBox(step) {

    const wrapper = $(`
        <div class="text-center">
            <textarea class="form-control mb-3"
                      rows="4"
                      placeholder="Enter your feedback"></textarea>

            <button class="btn btn-primary px-4" id="btnSubmitFeedback">
                Submit Feedback
            </button>
        </div>
    `);

    wrapper.find("textarea").on("input", function () {
        feedbackResponses[step.StepId] = $(this).val();
    });

    wrapper.find("#btnSubmitFeedback").on("click", function () {

        submitFeedback();

    });

    $("#stepsContainer").append(wrapper);
}

/* ================= SUBMIT ================= */
function submitFeedback() {

    let responses = [];

    steps.forEach(step => {
        const val = feedbackResponses[step.StepId];

        if (Array.isArray(val)) {
            val.forEach(id => responses.push({
                StepId: step.StepId,
                OptionId: id,
                TextResponse: null
            }));
        }

        if (typeof val === "string" && val.trim()) {
            responses.push({
                StepId: step.StepId,
                OptionId: null,
                TextResponse: val
            });
        }
    });

    const payload = {
        BookingId: Number(bookingId),
        CustomerName: customerName,
        WhatsappNo: whatsappNo,
        FeedbackResponseJSON: responses
    };
    console.log("payload:", payload);

    $.ajax({
        url: "/api/Feedback/SaveFeedback",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        beforeSend: function () {
            showSaveLoader();   // 👈 loader ON + buttons disabled
        },
        success: function (res) {
            console.log("save res:", res);

            $(".feedback-card").fadeOut(300, function () {
                $("#thankYouScreen").removeClass("d-none");
                if (bookingId == 0) {
                    $("#reloadCountdown").removeClass("d-none");
                    let seconds = 3;
                    // Initialize UI
                    $("#countdownSeconds").text(seconds);
                    // Start countdown
                    const interval = setInterval(function () {
                        seconds--;
                        $("#countdownSeconds").text(seconds);

                        if (seconds <= 0) {

                            clearInterval(interval);

                            steps = [];
                            currentStepIndex = 0;
                            feedbackResponses = {};

                            $("#stepTitle").text("");

                            // Show loader before loading form
                            $("#stepsContainer").html(`
                                <div class="loading-state text-center py-5">
                                    <div class="spinner-border text-primary" role="status"></div>
                                    <div class="mt-2">
                                        Please wait, loading feedback form…
                                    </div>
                                </div>
                            `);

                            $("#thankYouScreen").fadeOut(200, function () {
                                $(this).addClass("d-none");

                                $(".feedback-card").fadeIn(200);

                                loadFeedbackForm();
                            });
                        }
                    }, 300);
                }
                else {
                    $("#reloadCountdown").addClass("d-none");
                }
            });
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Unable to save feedback");
        },
        complete: function () {
            hideSaveLoader();   // 👈 loader OFF + buttons enabled
        }
    });
}

/* ================= UTIL ================= */
function atobFix(v) {
    v = v.replace(/-/g, '+').replace(/_/g, '/');
    while (v.length % 4) v += '=';
    return atob(v);
}

function showSaveLoader() {
    $("#saveLoader").removeClass("d-none");
    disableNavButtons(true);
}

function hideSaveLoader() {
    $("#saveLoader").addClass("d-none");
    disableNavButtons(false);
}

function disableNavButtons(disable) {
    $("#btnNext, #btnPrev").prop("disabled", disable);
}
