function DownloadMonthlySalesReport() {

    const btn = event?.target;
    if (btn) btn.disabled = true;

    fetch('/api/Report/DownloadMonthlySalesReport', {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        }
    })
        .then(res => res.json())
        .then(res => {

            if (res.status === "Success" && res.data?.FileUrl) {

                const link = document.createElement('a');
                link.href = res.data.FileUrl;
                link.target = "_blank";
                link.download = '';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

            } else {
                alert("PDF not generated");
            }
        })
        .catch(err => {
            console.error(err);
            alert("Error while downloading PDF");
        })
        .finally(() => {
            if (btn) btn.disabled = false;
        });
}



