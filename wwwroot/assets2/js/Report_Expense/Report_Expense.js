
$(document).ready(function () {
    ViewExpenseReport("viewPDF");
});

function ViewExpenseReport(actionName = "viewPDF") {
    $('#btn-loader').show();

    const url =
        actionName === "downloadExcel"
            ? "/api/Report/Report_Expense_Excel"
            : "/api/Report/Report_Expense";

    fetch(url, {
        method: "POST",
        headers: { "Accept": "application/json" }
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
                    <iframe src="${fileUrl}"
                            style="width:100%;height:100%;border:none;">
                    </iframe>`;
            } else {
                const link = document.createElement("a");
                link.href = fileUrl;
                link.download =
                    actionName === "downloadExcel"
                        ? "Expense_Report.xlsx"
                        : "Expense_Report.pdf";

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
