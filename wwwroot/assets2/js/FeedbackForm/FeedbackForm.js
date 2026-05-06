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

    $("#btnNext").on("click", handleNext);
    $("#btnPrev").on("click", handlePrev);
});

/* ================= LOAD FORM ================= */
function loadFeedbackForm() {
    $.get("/api/Feedback/GetFeedbackFormData", function (res) {
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
    $(".btn-actions").removeClass("d-none");

    step.HasTextBox ? renderTextBox(step) : renderOptions(step);
    updateNavButtons();
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
function renderTextBox(step) {
    const textarea = $(`<textarea class="form-control" rows="4"
                placeholder="Enter your feedback"></textarea>`);

    textarea.on("input", function () {
        feedbackResponses[step.StepId] = $(this).val();
    });

    $("#stepsContainer").append(textarea);
}

/* ================= NAVIGATION ================= */
function handleNext() {
    const stepId = steps[currentStepIndex].StepId;

    if (!feedbackResponses[stepId] && stepId < 4) {
        $("#feedbackError").removeClass("d-none");
        return;
    }
    else {
        $("#feedbackError").addClass("d-none");
    }

    if (currentStepIndex === steps.length - 1) {
        submitFeedback();
        return;
    }

    currentStepIndex++;
    renderStep();
}

function handlePrev() {
    currentStepIndex--;
    renderStep();
}

function updateNavButtons() {
    const isFirst = currentStepIndex === 0;
    const isLast = currentStepIndex === steps.length - 1;

    $("#btnPrev").toggleClass("hidden", isFirst);

    const icon = $("#btnNext i");
    if (isLast) {
        icon.removeClass("fa-chevron-right").addClass("fa-check");
        $("#btnNext").addClass("save");
    } else {
        icon.removeClass("fa-check").addClass("fa-chevron-right");
        $("#btnNext").removeClass("save");
    }
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
        success: function () {
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

                            if (bookingId == 0) {
                                // Reset UI (SPA-style reload)
                                $("#thankYouScreen").addClass("d-none");
                                $(".feedback-card").fadeIn(300);

                                steps = [];
                                currentStepIndex = 0;
                                feedbackResponses = {};

                                loadFeedbackForm();
                            } else {
                                // Optional full reload
                                // location.reload();
                            }
                        }
                    }, 1000);
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
