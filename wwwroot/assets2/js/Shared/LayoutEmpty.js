function showLoader() {
    document.getElementById("loaderOverlay").style.display = "flex";
}

function hideLoader() {
    document.getElementById("loaderOverlay").style.display = "none";
}

let ajaxActiveCount = 0;

$(document).ajaxStart(function () {
    ajaxActiveCount++;
    console.log("ajaxActiveCount START:", ajaxActiveCount);
    showLoader();
});

$(document).ajaxStop(function () {
    ajaxActiveCount--;
    if (ajaxActiveCount <= 0) {
        ajaxActiveCount = 0;
        hideLoader();
    }
    console.log("ajaxActiveCount STOP:", ajaxActiveCount);
});

function handleApiError(xhr, status, error, title = "Info!") {

    console.log("API Error:", xhr.responseText);

    let msg = "Something went wrong!";

    try {
        // Parse JSON response safely
        let resp = JSON.parse(xhr.responseText);

        // If message exists, show that only
        if (resp && resp.message) {
            msg = resp.message;
        }
    } catch (e) {
        // If JSON parse fails, fallback to raw text
        msg = xhr.responseText;
    }

    // Show SweetAlert Error
    Swal.fire({
        icon: "info",
        title: title,
        text: msg
    });
}
