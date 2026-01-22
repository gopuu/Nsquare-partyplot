//function DownloadDailySalesReport() {

//    const btn = event?.target;
//    if (btn) btn.disabled = true;

//    fetch('/api/Report/DownloadDailySalesReport', {
//        method: 'GET',
//        headers: {
//            'Accept': 'application/json'
//        }
//    })
//    .then(res => res.json())
//    .then(res => {

//        if (res.status === "Success" && res.data?.FileUrl) {

//            const link = document.createElement('a');
//            link.href = res.data.FileUrl;
//            link.target = "_blank";
//            link.download = '';
//            document.body.appendChild(link);
//            link.click();
//            document.body.removeChild(link);

//        } else {
//            alert("PDF not generated");
//        }
//    })
//    .catch(err => {
//        console.error(err);
//        alert("Error while downloading PDF");
//    })
//    .finally(() => {
//        if (btn) btn.disabled = false;
//    });
//}

$(document).ready(function () {
    let today = new Date().toISOString().split('T')[0];
    $("#to-date").val(today);
    $("#from-date").val(today);

    ViewDailySalesReport("viewPDF");
});

function ViewDailySalesReport(actionName = "viewPDF") {
    $('#btn-loader').show();

    const url = (actionName === "downloadExcel")
        ? "/api/Report/Report_DailySalesDetails_Excel"
        : "/api/Report/Report_DailySalesDetails";

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
        if (!text) throw new Error("Empty response");

        return JSON.parse(text);
    })
    .then(result => {
        $('#btn-loader').hide();

        if (result.status !== "Success" || !result.data?.FileUrl) {
            Swal.fire("Download Fail", result.message, "error");
            return;
        }

        const fileUrl = result.data.FileUrl;

        if (actionName === "viewPDF") {
            document.getElementById("pdfPreviewContainer").innerHTML = `
            <iframe src="${fileUrl}" style="width:100%;height:100%;border:none;"></iframe>
        `;
        } else {
            const link = document.createElement("a");
            link.href = fileUrl;

            link.download = (actionName === "downloadExcel")
                ? "DailySales_Report.xlsx"
                : "DailySales_Report.pdf";

            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    })
    .catch(error => {
        $('#btn-loader').hide();
        Swal.fire("Fail", error.message, "error");
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