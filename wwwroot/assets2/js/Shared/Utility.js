//CUSTOMER INVOICE REPORT MODAL --- START ------------------------
function generateInvoicePdf(payload) {
    var url = "/api/Report/DownloadCustomerInvoiceReport";
    downloadReport(url, payload, loaderMsg = "Generating Invoice..");
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

    $.ajax({
        url: "/api/Purchase/GetPurchaseList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        beforeSend: function () {
            showTempLoader('Fetching invoice...');
        },
        success: function (res) {
            //Swal.close();
            if (res.status === "Success" && res.data && res.data.length > 0) {
                let fileUrl = res.data[0].UploadInvoiceFile;
                if (fileUrl) {
                    // ✅ Open file in new tab
                    //window.open(fileUrl, "_blank");
                    const link = document.createElement('a');
                    link.href = fileUrl;
                    link.download = '';   // force download
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);

                }
                else {
                    Swal.fire({
                        icon: "warning",
                        title: "Not Available",
                        text: "Scanned invoice not uploaded"
                    });
                }

            }
            else {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: res.message || "No data found"
                });
            }
        },
        error: function (xhr, status, err) {
            //Swal.close();
            handleApiError(xhr, status, err, "Scan Invoice Download Failed");
       },
        complete: function () {
            hideTempLoader();
        }
     });
}
//PURCHASE INVOICE REPORT MODAL --- END ------------------------

//Download Receipt -- START
function downloadReport(url, payload, loaderMsg="Generating Report..") {
    console.log("receipt payload:", payload);

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        beforeSend: function () {
            showTempLoader(loaderMsg);
        },
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
                    text: res.message || 'Failed to generate report'
                });
            }
        },
        error: function (xhr, status, error) {
            //Swal.close();
            handleApiError(xhr, status, error, "report generation failed");
        },
        complete: function () {
            hideTempLoader();
        }
        
    });
}
//Download Receipt -- END


