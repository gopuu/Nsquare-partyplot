let selectedMonthDuration = "thismonth";
const HEADER_KEY = "printWithHeader";

$(document).ready(function () {
    $("#filterPanel").removeClass("d-none");

    selectedMonthDuration = "thismonth";
    applyQuickRange();
    ViewExpenseReport("viewPDF");

    $(".sort-option").on("click", function () {

        $(".sort-option").removeClass("active");
        $(this).addClass("active");

        selectedMonthDuration = $(this).data("value");
        $("#sortByDropdown").text($(this).text());

        applyQuickRange();
        ViewExpenseReport("viewPDF");
    });

    $("#btnApplyFilter").on("click", function () {
        ViewExpenseReport("viewPDF");
    });

    $("#btnResetFilter").on("click", function () {
        selectedMonthDuration = "thismonth";

        $(".sort-option").removeClass("active");
        $(".sort-option[data-value='thismonth']").addClass("active");

        $("#sortByDropdown").text("This Month");

        applyQuickRange();
        ViewExpenseReport("viewPDF");
    });

    $("#from-date, #to-date").on("change", function () {
        selectedMonthDuration = "customrange";
        $("#sortByDropdown").text("Custom Range");
        $(".sort-option").removeClass("active");
        ViewExpenseReport("viewPDF");
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
        ViewExpenseReport();
    });
    // WITH HEADER -- END ------------

});

function ViewExpenseReport(actionName = "viewPDF") {
    $('#btn-loader').show();

    const withHeader = sessionStorage.getItem(HEADER_KEY) === "true";
    console.log("With Header:", withHeader);

    const userId = parseInt(sessionStorage.getItem('userId') || 0);
    const url = (actionName === "downloadExcel")
        ? "/api/Report/Report_Expense_Excel"
        : "/api/Report/Report_Expense";

    var payload = {
        FromDate: $("#from-date").val(),
        ToDate: $("#to-date").val(),
        FilterDuration: selectedMonthDuration,
        WithHeader: withHeader,
        ActionBy: userId
    }

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
                    ? "Expense_Report.xlsx"
                    : "Expense_Report.pdf";

                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Download Failed");
            console.error("AJAX Error:", status, error);
            //Swal.fire("Error", "Something went wrong while processing the request", "error");
        }
    });

}
