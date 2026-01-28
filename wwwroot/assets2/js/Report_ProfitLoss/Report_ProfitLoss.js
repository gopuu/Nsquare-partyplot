let selectedMonthDuration = "thismonth";
const HEADER_KEY = "printWithHeader";

$(document).ready(function () {

    setCurrentMonthDates();
    ViewProfitLossReport("viewPDF");

    $(".sort-option").on("click", function () {

        $(".sort-option").removeClass("active");
        $(this).addClass("active");

        selectedMonthDuration = $(this).data("value");
        $("#sortByDropdown").text($(this).text());

        applyQuickRange();
        ViewProfitLossReport("viewPDF");
    });

    $("#btnApplyFilter").on("click", function () {
        ViewProfitLossReport("viewPDF");
    });

    $("#btnResetFilter").on("click", function () {
        setCurrentMonthDates();
        selectedMonthDuration = "thismonth";
        ViewProfitLossReport("viewPDF");
    });

    $("#from-date, #to-date").on("change", function () {
        selectedMonthDuration = "customrange";
        $("#sortByDropdown").text("Custom Range");
        $(".sort-option").removeClass("active");
        ViewProfitLossReport("viewPDF");
    });
    // WITH HEADER -- START ------------
    // Restore value on page load
    const saved = sessionStorage.getItem(HEADER_KEY);
    if (saved !== null) {
        document.getElementById("toggleWithHeader").checked = (saved === "true");
    }

    // On toggle change
    document.getElementById("toggleWithHeader").addEventListener("change", function () {
        const isChecked = this.checked;
        sessionStorage.setItem(HEADER_KEY, isChecked);

        console.log("With Header:", isChecked); // 🔹 print value
        ViewProfitLossReport();
    });
    // WITH HEADER -- END ------------
});

function ViewProfitLossReport(actionName = "viewPDF") {
    $('#btn-loader').show();

    const withHeader = sessionStorage.getItem(HEADER_KEY) === "true";
    console.log("With Header:", withHeader);

    const url = (actionName === "downloadExcel")
        ? "/api/Report/Report_ProfitLoss_Excel"
        : "/api/Report/Report_ProfitLoss";

    fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            FromDate: $("#from-date").val(),
            ToDate: $("#to-date").val(),
            FilterDuration: selectedMonthDuration,
            WithHeader: withHeader
        })
    })
        .then(r => r.json())
        .then(result => {

            if (result.status !== "Success" || !result.data?.FileUrl) {
                Swal.fire("Fail", result.message || "No data found", "error");
                return;
            }

            if (actionName === "viewPDF") {
                $("#pdfPreviewContainer").html(
                    `<iframe src="${result.data.FileUrl}" style="width:100%;height:100%;border:none;"></iframe>`
                );
            } else {
                const link = document.createElement("a");
                link.href = result.data.FileUrl;
                link.download = actionName === "downloadExcel"
                    ? "ProfitLoss_Report.xlsx"
                    : "ProfitLoss_Report.pdf";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        });
}

function setCurrentMonthDates() {
    const today = new Date();
    const first = new Date(today.getFullYear(), today.getMonth(), 1);
    const last = new Date(today.getFullYear(), today.getMonth() + 1, 0);

    $("#from-date").val(formatDate(first));
    $("#to-date").val(formatDate(last));
}

function applyQuickRange() {
    const today = new Date();
    let from, to = today;

    switch (selectedMonthDuration) {
        case "thismonth":
            from = new Date(today.getFullYear(), today.getMonth(), 1);
            break;
        case "lastmonth":
            from = new Date(today.getFullYear(), today.getMonth() - 1, 1);
            to = new Date(today.getFullYear(), today.getMonth(), 0);
            break;
        case "days":
            from = new Date(today.setDate(today.getDate() - 6));
            break;
        case "lastdays":
            from = new Date(today.setDate(today.getDate() - 29));
            break;
        case "yesterday":
            from = to = new Date(today.setDate(today.getDate() - 1));
            break;
        default:
            return;
    }

    $("#from-date").val(formatDate(from));
    $("#to-date").val(formatDate(to));
}

function formatDate(d) {
    return d.toISOString().split("T")[0];
}
