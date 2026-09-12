//CUSTOMER INVOICE REPORT MODAL --- START ------------------------
function generateInvoicePdf(payload) {
    var url = "/api/Report/DownloadCustomerInvoiceReport";
    downloadReport(url, payload, loaderMsg = "Generating Invoice PDF..", failedMsg = "Failed to Generating Invoice PDF..");
}
//CUSTOMER INVOICE REPORT MODAL --- END ------------------------

//PURCHASE INVOICE REPORT MODAL --- START ------------------------
function downloadPurchaseInvoice(purchaseId = 0, invoiceNo = null, type="pdf") {

    const payload = {
        PurchaseId: purchaseId,
        InvoiceNo: invoiceNo.toString(),
        WithHeader: true,
        ActionBy: Number(sessionStorage.getItem("userId") || 0)
    };
    console.log("payload:", payload);

    var url = "/api/Report_Purchase/DownloadPurchaseInvoiceReport_Pdf";
    if (type == "excel") {
        url = "/api/Report_Purchase/DownloadPurchaseInvoiceReport_Excel";
    }

    downloadReport(url, payload, loaderMsg = "Generating Purchase Invoice..");
}

function downloadPurchaseScannedInvoice(purchaseId = 0, invoiceNo = null) {
    if (!purchaseId && !invoiceNo) {
        Swal.fire({
            icon: "warning",
            title: "Invalid Request",
            text: "PurchaseId or InvoiceNo is required"
        });
        return;
    }

    const payload = {
        Id: purchaseId,
        InvoiceNo: invoiceNo ? invoiceNo.toString() : ""
    };
    let url= "/api/Purchase/GetPurchaseList";

    showTempLoader("Generating Report...");
    downloadReport(url, payload, loaderMsg = "Fetching Scanned Invoice..", failedMsg = "Failed to fetch Scanned Invoice");

}
//PURCHASE INVOICE REPORT MODAL --- END ------------------------

//Download Receipt -- START
function downloadReport_old_18aug26(url, payload, loaderMsg="Generating Report..", failedMsg = "Failed to generate report") {
    console.log("receipt payload:", payload);
    const loaderId = showTempLoader(loaderMsg);

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function (res) {
            ///Swal.close();
            console.log("downloadReport res:", res);
            if (res.status === 'Success' && res.data?.FileUrl) {
                const link = document.createElement('a');
                link.href = res.data.FileUrl;
                link.download = '';   // force download
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            }
            else {
                Swal.fire({
                    icon: 'info',
                    title: 'Failed',
                    text: res.message || failedMsg
                });
            }
        },
        error: function (xhr, status, error) {
            //Swal.close();
            handleApiError(xhr, status, error, "report generation failed");
        },
        complete: function () {
            hideTempLoader(loaderId);
        }
        
    });
}

function downloadReport(url, payload,
    loaderMsg = "Generating Report...",
    failedMsg = "Failed to generate report"
) {

    console.log("report payload:", payload);

    const loaderId = showTempLoader(loaderMsg);

    let progress = 0;

    // Simulated progress
    const progressTimer = setInterval(function () {

        if (progress < 90) {
            progress += 10;
        }
        else if (progress < 99) {
            progress += 1;
        }

        updateTempLoader(loaderId, progress);

    }, 500);


    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),

        success: function (res) {

            console.log("downloadReport res:", res);

            clearInterval(progressTimer);

            if (res.status === "Success" && res.data?.FileUrl) {

                // Final completion
                updateTempLoader(loaderId, 100);

                const link = document.createElement("a");

                link.href = res.data.FileUrl;
                link.download = "";

                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

            }
            else {

                Swal.fire({
                    icon: "info",
                    title: "Failed",
                    text: res.message || failedMsg
                });
            }
        },

        error: function (xhr, status, error) {
            clearInterval(progressTimer);
            handleApiError(
                xhr,
                status,
                error,
                "Report generation failed"
            );
        },

        complete: function () {

            clearInterval(progressTimer);

            // Keep 100% visible briefly before removing
            setTimeout(function () {
                hideTempLoader(loaderId);
            }, 500);
        }
    });
}
//Download Receipt -- END


