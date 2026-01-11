$(document).ready(function () {
   
    let today = new Date().toISOString().split('T')[0];
    $("#invoice-date").val(today);

});


function DownloadGSTReport() {

    fetch('/api/Report/DownloadGSTReport', {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        }
    })
        .then(res => res.json())
        .then(res => {

            if (res.status === "Success" && res.data.FileUrl) {

                const link = document.createElement('a');
                link.href = res.data.FileUrl;
                link.download = '';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

            } else {
                alert("PDF Not generate");
            }
        })
        .catch(err => {
            console.error(err);
            alert("Error while downloading PDF");
        });
}
