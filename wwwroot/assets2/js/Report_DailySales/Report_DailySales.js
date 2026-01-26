let selectedMonthDuration = "thismonth";

$(document).ready(function () {

    setTodayDates();
    ViewDailySalesReport("viewPDF");

    $(".sort-option").on("click", function () {
        $(".sort-option").removeClass("active");
        $(this).addClass("active");

        selectedMonthDuration = $(this).data("value");
        $("#sortByDropdown").text($(this).text());

        applyQuickRange();
        ViewDailySalesReport("viewPDF");
    });

    $("#btnApplyFilter").on("click", function () {
        ViewDailySalesReport("viewPDF");
    });

    $("#btnResetFilter").on("click", function () {
        setTodayDates();
        selectedMonthDuration = "thismonth";
        ViewDailySalesReport("viewPDF");
    });
});

function ViewDailySalesReport(actionName = "viewPDF") {

    const url = (actionName === "downloadExcel")
        ? "/api/Report/Report_DailySalesDetails_Excel"
        : "/api/Report/Report_DailySalesDetails";

    fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            FromDate: $("#from-date").val(),
            ToDate: $("#to-date").val(),
            FilterDuration: selectedMonthDuration
        })
    })
        .then(r => r.json())
        .then(result => {

            if (result.status !== "Success" || !result.data?.FileUrl) {
                Swal.fire("Fail", result.message || "No data", "error");
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
                    ? "DailySales_Report.xlsx"
                    : "DailySales_Report.pdf";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        });
}

function setTodayDates() {
    const today = new Date().toISOString().split("T")[0];
    $("#from-date").val(today);
    $("#to-date").val(today);
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
