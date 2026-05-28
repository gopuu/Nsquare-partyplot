const greetings = [
    "Every event begins with great planning.",
    "Today’s booking is tomorrow’s celebration.",
    "Create memorable moments for every guest.",
    "Details make events unforgettable.",
    "Excellence in service creates lasting impressions.",
    "A well-managed banquet builds lasting relationships.",
    "Behind every successful event is flawless coordination.",
    "Great hospitality turns clients into family.",
    "Every reservation carries someone’s special moment.",
    "Plan perfectly. Execute seamlessly.",
    "Your dedication makes celebrations extraordinary.",
    "Precision today ensures applause tomorrow.",
    "From booking to celebration — excellence matters.",
    "Each event is an opportunity to shine.",
    "Make every gathering a grand success.",
    "Every celebration deserves perfect execution.",
    "Hospitality is where professionalism meets warmth.",
    "Success in events lies in attention to detail.",
    "Your effort shapes unforgettable experiences.",
    "Every booking reflects trust — honor it.",
    "Consistency builds reputation in hospitality.",
    "A seamless event starts with disciplined preparation.",
    "Celebrate milestones with perfection.",
    "Service excellence is the heart of every banquet.",
    "Make logistics invisible and memories visible.",
    "Teamwork turns planning into perfection.",
    "A great venue creates timeless memories.",
    "Every guest experience defines your brand.",
    "Deliver beyond expectations — every single time.",
    "Today’s excellence becomes tomorrow’s referrals."
];

$(document).ready(function () {
    setCurrentDate();
    setDailyGreeting();

    $('#Password').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#loginForm').submit();
        }
    });

    // Handle form submission
    $('#loginForm').on('submit', function (e) {
        e.preventDefault(); // Prevent the form from submitting normally
        var userName = $('#UserName').val();
        var password = $('#Password').val();
        // Prepare the data for the API request
        var data = {
            username: userName,
            password: password,
            fcm: ''
        };
        // Make AJAX request to login API
        $.ajax({
            url: '/api/User/Login',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                console.log("response login:", response);
                if (response.status === 'Success' && response.data !== null) {
                    sessionStorage.setItem('isSuperUser', response.data.issuperuser);
                    sessionStorage.setItem('userId', response.data.userid);
                    sessionStorage.setItem('userName', response.data.username);
                    sessionStorage.setItem('userImage', response.data.userimage);
                    sessionStorage.setItem('token', response.data.token);
                    sessionStorage.setItem('companyid', response.data.companyid);
                    sessionStorage.setItem('companyname', response.data.companyname);
                    sessionStorage.setItem('companylogo', response.data.companylogo);
                    sessionStorage.setItem('companycontact', response.data.phoneno);

                    window.location.href = '/Home/Dashboard';
                }
                else {
                    $('#errorMessage').text(response.message);
                    ajaxActiveCount--;
                }
            },
            error: function (xhr, status, error) {
                ajaxActiveCount--;
                handleApiError(xhr, status, error, 'Login failed');
                // Handle any errors
                $('#errorMessage').text('Login failed. Please try again.');
            },
            complete: function () {
                ajaxActiveCount++;
            }
        });
    });

    setWallpaper();
});

function setWallpaper() {
    //Image Loop
    (function () {
        const images = [
            "LoginImg1.png",
            "LoginImg2.png",
            "LoginImg3.png",
            "LoginImg4.png",
            "LoginImg5.png",
            "LoginImg6.png",
            "LoginImg7.png",
            "LoginImg8.png",
            "LoginImg9.png",
            "LoginImg10.png"
        ];

        const today = new Date();
        const dayIndex = today.getDate(); // 1–31

        // Rotate based on day
        //const imageIndex = (dayIndex % totalImages) + 1;
        const imageIndex = dayIndex % images.length;

        //const imagePath = `/assets2/img/loginImages/LoginImg1.png`;
        const imagePath = `/assets2/img/loginImages/${images[imageIndex]}`;

        document.querySelector(".light-bg-image")
            .style.backgroundImage = `url('${imagePath}')`;
    })();
}

function setDailyGreeting() {
    const today = new Date();
    // Cycle from 0 to 14
    const index = (today.getDate() - 1) % 15;
    document.getElementById("dailyGreeting").innerText = '"' + greetings[index] + '"';
}

function setCurrentDate() {
    const now = new Date();

    const day = String(now.getDate()).padStart(2, '0');
    const month = now.toLocaleString('en-US', { month: 'short' });
    const year = now.getFullYear();

    const formatted = `${day}-${month}-${year}`;

    document.getElementById("currentDate").innerText = formatted;
}
