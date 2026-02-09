let selectedMonthDuration = "thismonth";
const HEADER_KEY = "printWithHeader";

$(document).ready(function () {
    $("#filterPanel").removeClass("d-none");

    applyQuickRange();
    ViewMonthlySalesReport("viewPDF");

    $(".sort-option").on("click", function () {

        $(".sort-option").removeClass("active");
        $(this).addClass("active");

        selectedMonthDuration = $(this).data("value");
        $("#sortByDropdown").text($(this).text());

        applyQuickRange();
        ViewMonthlySalesReport("viewPDF");
    });

    $("#btnApplyFilter").on("click", function () {
        ViewMonthlySalesReport("viewPDF");
    });

    $("#btnResetFilter").on("click", function () {
        selectedMonthDuration = "thismonth";

        $(".sort-option").removeClass("active");
        $(".sort-option[data-value='thismonth']").addClass("active");

        $("#sortByDropdown").text("This Month");

        applyQuickRange();
        ViewMonthlySalesReport("viewPDF");
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
        ViewMonthlySalesReport();
    });
    // WITH HEADER -- END ------------

});

function ViewMonthlySalesReport(actionName = "viewPDF") {
    const withHeader = sessionStorage.getItem(HEADER_KEY) === "true";
    console.log("With Header:", withHeader);

    const userId = parseInt(sessionStorage.getItem('userId') || 0);

    const url = (actionName === "downloadExcel")
        ? "/api/Report/Report_MonthlySalesDetails_Excel"
        : "/api/Report/Report_MonthlySalesDetails";

    var payload = {
        FromDate: $("#from-date").val(),
        ToDate: $("#to-date").val(),
        FilterDuration: selectedMonthDuration,
        WithHeader: withHeader,
        ActionBy: userId
    };

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        data: JSON.stringify(payload),
        success: function (result) {

            if (result.status !== "Success" || !result.data || !result.data.FileUrl) {
                Swal.fire("Fail", result.message || "No data found", "error");
                return;
            }

            if (actionName === "viewPDF") {
                $("#pdfPreviewContainer").html(
                    `<iframe src="${result.data.FileUrl}" style="width:100%;height:100%;border:none;"></iframe>`
                );
            }
            else {
                const link = document.createElement("a");
                link.href = result.data.FileUrl;
                link.download = actionName === "downloadExcel"
                    ? "MonthlySales_Report.xlsx"
                    : "MonthlySales_Report.pdf";

                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to generate monthly sales report");
            console.error("AJAX Error:", status, error);
            //Swal.fire("Error", "Something went wrong while processing the request", "error");
        }
    });

}

