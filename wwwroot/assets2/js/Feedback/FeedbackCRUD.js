
let steps = [];
let currentStep = 0;
let answers = {};
const imgMapStep1 = {
    "Very Good": "/assets2/img/Feedback/Heart_Eyes_Emoji.png",
    "Good": "/assets2/img/Feedback/Smiling_Emoj.png",
    "Poor": "/assets2/img/Feedback/Slightly_Smiling_Emoji.png",
    "Very Poor": "/assets2/img/Feedback/Very_Sad_Emoji.png"
};
const imgMapStep2 = {
    "Food": "/assets2/img/Feedback/Food.png",
    "Service": "/assets2/img/Feedback/Service.png",
    "Ambience": "/assets2/img/Feedback/Ambience.png",
    "Cleanliness": "/assets2/img/Feedback/Cleanliness.png",
    "Value For Money": "/assets2/img/Feedback/ValueForMoney.png"
};


$(document).ready(function () {
    loadFeedbackForm();

    $("#btnPrev").click(function () {
        changeStep(-1);
    });

    $("#btnNext").click(function () {
        changeStep(1);
    });
});


function loadFeedbackForm() {
    $.ajax({
        url: "/api/Feedback/GetFeedbackFormData", // 🔁 change API
        type: "GET",
        success: function (res) {
            steps = res.data.sort((a, b) => a.StepOrder - b.StepOrder);
            renderSteps();
            showStep(0);
        }
    });
}

function renderSteps() {
    let html = "";
    steps.forEach((step, index) => {

        html += `<div class="step" data-step="${index}">
        <p style="font-size:15px;font-weight:700;">
            ${step.StepTitle}
        </p>`;
            // OPTIONS
        if (step.Options && step.Options.length> 0) {
            // 🔥 STEP 1 – Emoji images (single select)
            if (step.StepOrder === 1) {
                html += `<div class="row justify-content-center">`;
                step.Options.forEach(opt => {
                    html += `
                    <div class="col-6 col-md-3 d-flex justify-content-center">
                        <label class="emoji-option mb-3">
                            <input type="radio"
                                    name="step_${step.StepId}"
                                    value="${opt.OptionId}" hidden>

                            <img src="${imgMapStep1[opt.OptionName]}"
                                    alt="${opt.OptionName}"
                                    title="${opt.OptionName}">
                        </label>
                    </div>`;
                });
                html += `</div>`;
            }
            // 🔥 STEP 2 – Image based MULTI select
            else if (step.StepOrder === 2) {

                html += `<div class="row justify-content-center">`;

                step.Options.forEach(opt => {
                    html += `
    <div class="col-6 col-md-4 d-flex justify-content-center">
    <label class="emoji-option mb-3">
        <input type="checkbox"
            name="step_${step.StepId}"
            value="${opt.OptionId}" hidden>

            <img src="${imgMapStep2[opt.OptionName]}"
                alt="${opt.OptionName}"
                title="${opt.OptionName}">
            </label>
        </div>`;
                });

        html += `</div>`;
            }

            // 🔹 STEP 3 & 4 – Normal
            else {
                step.Options.forEach(opt => {
                    let type = step.IsMultiSelect ? "checkbox" : "radio";
                    html += `
            <label class="option-btn d-block mb-2">
                <input type="${type}"
                    name="step_${step.StepId}"
                    value="${opt.OptionId}">
                    ${opt.OptionName}
                </label>`;
                });
            }
        }
        // TEXTBOX
        if (step.HasTextBox) {
            html += `<textarea class="form-control mt-3"
                        rows = "3"
                        placeholder = "Type here..."
                        data - textbox="${step.StepId}"></textarea>`;
                        }

        html += `</div>`;
    });
    $("#stepsContainer").html(html);
    // ✅ CLICK HANDLING (emoji + normal both)
    $(document).off("click", ".emoji-option, .option-btn");
    $(document).on("click", ".emoji-option, .option-btn", function () {
        let input = $(this).find("input");
        if (input.attr("type") === "radio") {
            $(this).closest(".step").find("label").removeClass("active");
            $(this).addClass("active");
            input.prop("checked", true);
        }
        else {
            // checkbox (multi select)
            $(this).toggleClass("active");
            input.prop("checked", !input.prop("checked"));
        }
    });
}
function showStep(index) {
    $(".step").removeClass("active");
    $(`.step[data-step="${index}"]`).addClass("active");

    $("#stepTitle").text(`Step ${index + 1} of ${steps.length}`);

    $("#btnPrev").toggle(index > 0);
    $("#btnNext").text(index === steps.length - 1 ? "Submit" : "Next");
}

function changeStep(dir) {
    if (dir === 1 && currentStep === steps.length - 1) {
        submitFeedback();
        return;
    }
    currentStep += dir;
    showStep(currentStep);
}

function submitFeedback() {
    let payload = [];

    steps.forEach(step => {
        let selected = [];

        $(`input[name="step_${step.StepId}"]:checked`).each(function () {
            selected.push($(this).val());
        });

        let text = $(`textarea[data-textbox="${step.StepId}"]`).val() || "";

        payload.push({
            StepId: step.StepId,
            OptionIds: selected,
            Text: text
        });
    });

    console.log("Final Payload", payload);
    alert("Thank you for your feedback ❤️");
}

    

