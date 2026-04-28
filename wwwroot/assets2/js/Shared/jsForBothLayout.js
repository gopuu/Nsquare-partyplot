//FEEDBACK LINK
$("#feedbackFab").click(function () {
    window.open('/feedbackForm', '_blank');
});

/*
$("#feedbackFab").click(function () {

    const feedbackUrl = window.location.origin + "/feedbackForm";

    // Copy to clipboard
    navigator.clipboard.writeText(feedbackUrl)
        .then(() => {
            console.log("Copied:", feedbackUrl);

            // Optional: show toast (SweetAlert)
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: 'success',
                title: 'Feedback link copied!',
                showConfirmButton: false,
                timer: 1500
            });
        })
        .catch(err => {
            console.error("Copy failed", err);
        });

    // Optional: Open WhatsApp with prefilled message
    const message = encodeURIComponent(
        `Hi, \nThankyou for Booking. \nPlease share your feedback: \n${feedbackUrl}`
    );

    window.open(`https://wa.me/?text=${message}`, "_blank");
});
*/