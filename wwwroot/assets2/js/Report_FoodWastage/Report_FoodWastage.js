
$(document).ready(function () {
    let today = new Date().toISOString().split('T')[0];
    $("#to-date").val(today);
    $("#from-date").val(today);
    ViewFoodWastageReport("viewPDF");
});


function ViewFoodWastageReport(actionName = "viewPDF") {

    $('#btn-loader').show();

    let url = "";

    if (actionName === "downloadExcel") {
        url = "/api/Report/Report_FoodWastage_Excel";
    } else {
        url = "/api/Report/Report_FoodWastage";
    }

    fetch(url, {
        method: "POST",
        headers: {
            "Accept": "application/json",
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            FromDate: $("#from-date").val(),
            ToDate: $("#to-date").val(),
            FilterDuration: selectedMonthDuration
        })
    })
    .then(async response => {

        if (!response.ok)
            throw new Error("Server error : " + response.status);

        const text = await response.text();
        if (!text)
            throw new Error("Empty response from server");

        return JSON.parse(text);
    })
    .then(result => {

        $('#btn-loader').hide();

        if (result.status !== "Success" || !result.data?.FileUrl) {
            Swal.fire("Download Fail", result.message || "No file found", "error");
            return;
        }

        const fileUrl = result.data.FileUrl;

        // 🔹 VIEW PDF
        if (actionName === "viewPDF") {

            document.getElementById("pdfPreviewContainer").innerHTML = `
                <iframe src="${fileUrl}"
                        style="width:100%; height:100%; border:none;">
                </iframe>
            `;

        }
        // 🔹 DOWNLOAD PDF / EXCEL
        else {

            const link = document.createElement("a");
            link.href = fileUrl;

            link.download =
                actionName === "downloadExcel"
                    ? "FoodWastage_Report.xlsx"
                    : "FoodWastage_Report.pdf";

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    })
    .catch(error => {

        $('#btn-loader').hide();

        Swal.fire("Fail", error.message, "error");
        console.error(error);
    });
}

let selectedMonthDuration = "thismonth";

$(".sort-option").on("click", function (e) {
    e.preventDefault();

    $(".sort-option").removeClass("active");
    $(this).addClass("active");

    selectedMonthDuration = $(this).data("value");

    $("#sortByDropdown").text("Filter : " + $(this).text());
});