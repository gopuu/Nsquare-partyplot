//INVOICE REPORT MODAL --- START ------------------------
function generateInvoicePdf(request) {
    console.log("pdf payload:", request);
    $.ajax({
        url: '/api/Report/DownloadCustomerInvoiceReport',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        beforeSend: function () {
            Swal.fire({
                title: 'Generating PDF...',
                text: 'Please wait',
                allowOutsideClick: false,
                didOpen: () => Swal.showLoading()
            });
        },
        success: function (res) {
            Swal.close();

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
            Swal.close();
            handleApiError(xhr, status, error, "Invoice PDF generation failed");
        }
    });
}
//INVOICE REPORT MODAL --- END ------------------------

