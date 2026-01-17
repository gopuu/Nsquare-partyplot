
$(document).ready(function () {
    ViewPurchaseReport("viewPDF");
});

function ViewPurchaseReport(actionName = "viewPDF") {
    $('#btn-loader').show();
    const url = '/api/Report/Report_Purchase';
    fetch(url, {
        method: 'POST',
        headers: {
            'Accept': 'application/json'
        }
    })
        .then(async response => {
            // 🔐 SAFETY CHECK
            if (!response.ok) {
                throw new Error("Server error : " + response.status);
            }
            const text = await response.text();

            if (!text) {
                throw new Error("Empty response from server");
            }
            return JSON.parse(text);
        })
        .then(result => {
            $('#btn-loader').hide();
            if (result.status !== 'Success' || !result.data || !result.data.FileUrl) {
                Swal.fire({
                    title: "Download Fail",
                    text: result.message || "No report URL returned",
                    icon: "error"
                });
                return;
            }
            const fileUrl = result.data.FileUrl;
            if (actionName === 'downloadPDF') {
                const link = document.createElement('a');
                link.href = fileUrl;
                link.download = 'Purchase_Report.pdf';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            } else {
                document.getElementById('pdfPreviewContainer').innerHTML = `
    <iframe 
        src="${fileUrl}" 
        style="width:100%;height:100%;border:none;">
    </iframe>
`;
            }
        })
        .catch(error => {
            $('#btn-loader').hide();
            console.error("Error fetching GST report:", error);
            Swal.fire({
                title: "Download Fail",
                text: error.message,
                icon: "error"
            });
        });
}
