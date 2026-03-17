//CUSTOMER INVOICE REPORT MODAL --- START ------------------------
function generateInvoicePdf(request) {
    console.log("pdf payload:", request);
    $.ajax({
        url: '/api/Report/DownloadCustomerInvoiceReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        beforeSend: function () {
            showTempLoader("Generating Invoice..");
        },
        success: function (res) {

            if (res.status === 'Success' && res.data?.FileUrl) {
                const link = document.createElement('a');
                link.href = res.data.FileUrl;
                link.download = '';           // 🔑 forces download
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Failed',
                    text: res.message || 'Failed to generate invoice PDF'
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Invoice PDF generation failed");
        },
        complete: function () {
            Swal.close();
        }
    });
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

    $.ajax({
        url: url,
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        beforeSend: function () {
            showTempLoader("Generating Invoice..");
        },
        success: function (res) {
            if (res.status === 'Success' && res.data?.FileUrl) {
                const link = document.createElement('a');
                link.href = res.data.FileUrl;
                link.download = '';           // 🔑 forces download
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Failed',
                    text: res.message || 'Failed to generate invoice PDF'
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to generate invoice");
        },
        complete: function () {
            Swal.close();
        }
    });
}
//PURCHASE INVOICE REPORT MODAL --- END ------------------------

