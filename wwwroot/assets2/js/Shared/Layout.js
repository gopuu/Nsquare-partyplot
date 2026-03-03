function showTempLoader(title = "Checking Availability") {
    // Show loader before AJAX
    Swal.fire({
        title: title,
        text: "Please wait...",
        allowOutsideClick: false,
        allowEscapeKey: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}
function handleApiSuccess(msg = "Saved successfully!") {
    Swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: msg,
        showConfirmButton: false,
        timer: 1200,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

}
function formatINR(amount) {
    if (!amount) return "0";
    return amount.toLocaleString("en-IN");
}
function calcPercent(value, total) {
    return Math.round((value / total) * 100);
}

function showLoader() {
    document.getElementById("loaderOverlay").style.display = "flex";
}

function hideLoader() {
    document.getElementById("loaderOverlay").style.display = "none";
}

function encodeValue(value) {
    return btoa(value.toString())
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=+$/, '');
}

function decodeValue(encoded) {
    encoded = encoded.replace(/-/g, '+').replace(/_/g, '/');
    while (encoded.length % 4) encoded += '=';
    return atob(encoded);
}

function updateLiveDateTime() {
    const now = new Date();

    const day = String(now.getDate()).padStart(2, '0');

    const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    const month = months[now.getMonth()];
    const year = now.getFullYear();

    let hours = now.getHours();
    const minutes = String(now.getMinutes()).padStart(2, '0');

    const ampm = hours >= 12 ? "PM" : "AM";
    hours = hours % 12;
    hours = hours ? hours : 12;
    hours = String(hours).padStart(2, '0');

    const formatted = `${day}-${month}-${year}<br>${hours}:${minutes} ${ampm}`;

    const el = document.getElementById("liveDateTime");
    if (el) el.innerHTML = formatted;   // ⚠ Use innerHTML for <br>
    $("#liveDateTime").addClass("d-none");
}

document.addEventListener("DOMContentLoaded", function () {
    const userName = sessionStorage.getItem("userName");
    const userImage = sessionStorage.getItem("userImage") || "/assets2/img/user.png";
    // Set user name
    if (userName) {
        $("#lblUserName").text(userName);
    }

    //LIVE DATE-TIME ------
    // Run immediately
    //updateLiveDateTime();
    // Update every second
    //setInterval(updateLiveDateTime, 60000);
    //LIVE DATE-TIME ------

    // Set user image
    // if (userImage) {
    //     $(".imgUserProfile").attr("src", userImage);
    // }

    let defaultImage = `https://ui-avatars.com/api/?name=${encodeURIComponent(userName || "User")}&background=random`;

    // Select ALL avatars
    let avatarImgs = document.querySelectorAll(".userAvatar");
    avatarImgs.forEach(function (img) {
        // Primary image
        img.src = userImage && userImage.trim() !== ""
            ? userImage
            : defaultImage;
        // Fallback if image fails
        img.onerror = function () {
            this.onerror = null; // prevent loop
            this.src = defaultImage;
        };
    });

    var userid = Number(sessionStorage.getItem("userId", userid));
    const staffIds = [userid];

    //sessionStorage.setItem("userId", userid);
    fetchUserAccessData(userid);

    //loadStaffAccountsRights(staffIds);

    document.querySelectorAll('.nav-item.dropdown').forEach(dropdown => {
        const subItems = dropdown.querySelectorAll('.dropdown-menu .dropdown-item');

        if (subItems.length === 1) {
            const mainLink = dropdown.querySelector('.nav-link');
            const onlySubLink = subItems[0].getAttribute('href');

            // Disable dropdown behavior
            mainLink.removeAttribute('data-bs-toggle');
            mainLink.removeAttribute('role');
            mainLink.removeAttribute('aria-expanded');
            mainLink.classList.remove('dropdown-toggle');

            // Make parent clickable
            mainLink.setAttribute('href', onlySubLink);
        }
    });
    
    //All In One Link -- START
    feather.replace();

    const fab = document.getElementById("allInOneFab");
    const modal = document.getElementById("allInOneModal");

    fab.addEventListener("click", function () {
        modal.classList.toggle("d-none");
    });

    document.getElementById("btnCloseAio").addEventListener("click", function () {
        modal.classList.add("d-none");
    });

    function validateAioMobile() {
        const mobile = $("#aioMobile").val().trim();

        if (!mobile) {
            showError("Please enter WhatsApp number");
            return false;
        }

        if (mobile.length !== 10) {
            showError("Enter a valid 10-digit number");
            return false;
        }

        hideError();
        return true;
    }

    function showError(msg) {
        $("#aioMobileError").text(msg).removeClass("d-none");
    }

    function hideError() {
        $("#aioMobileError").addClass("d-none");
    }
    
    //Hide Error While Typing (Auto)
    $("#aioMobile").on("input", function () {
        const val = $(this).val().trim();
        if (val.length === 10) {
            hideError();
        }
    });

    document.getElementById("btnSendAio").addEventListener("click", function () {
        if (!validateAioMobile()) {
            return;
        }

        const name = document.getElementById("aioName").value.trim();
        const countryCode = document.getElementById("countryCode").innerText.replace("+", "");
        const mobile = document.getElementById("aioMobile").value.trim();
        const fullNumber = countryCode + mobile;

        // 🔗 Your All-In-One link
        const link = `${window.location.origin}/AllInOneLink?name=${encodeURIComponent(name)}`;
        const message = `Hi ${name || ""},\nPlease check this link:\n${link}`;

        const whatsappUrl = `https://wa.me/${fullNumber}?text=${encodeURIComponent(message)}`;
        window.open(whatsappUrl, "_blank");

        modal.classList.add("d-none");
    });
    //All In One Link -- END
});

let ajaxActiveCount = 0;

$(document).ajaxStart(function () {
    ajaxActiveCount++;
    //console.log("ajaxActiveCount START:", ajaxActiveCount);
    showLoader();
});

$(document).ajaxStop(function () {
    ajaxActiveCount--;
    if (ajaxActiveCount <= 0) {
        ajaxActiveCount = 0;
        hideLoader();
    }
    //console.log("ajaxActiveCount STOP:", ajaxActiveCount);
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

function printAllErrors(xhr, status, error, srcApi = "") {
    console.log("---- API ERROR ----");
    console.log("srcAPi:", srcApi, ", Status Code:", xhr.status);            // e.g., 404, 500
    console.log("srcAPi:", srcApi, ", Status Text:", xhr.statusText);        // e.g., Not Found, Internal Server Error
    console.log("srcAPi:", srcApi, ", Response Text:", xhr.responseText);    // server error message
    console.log("srcAPi:", srcApi, ", Ready State:", xhr.readyState);
    console.log("srcAPi:", srcApi, ", jQuery Status:", status);              // "timeout", "error", etc.
    console.log("srcAPi:", srcApi, ", Error Thrown:", error);                // exception object/message
    console.log("srcAPi:", srcApi, ", Full XHR Object:", xhr);               // dump everything
    console.log("-------------------");
}

// Helper: Convert abbreviation to full name
let shiftMap = {};      // Code -> Full Name
let shiftTimes = {};    // Code -> Time Object
function fullShiftName(abbr) {
    return shiftMap[abbr] || abbr;
}


//get User Rights
var userRightsData = null;
//Fetch Data
function fetchUserAccessData(userid) {
    const inputData = {
        StaffId: userid
    };
    $.ajax({
        type: "POST",
        url: "/api/UserRights/GetUserRightsList_ForLayout",
        data: JSON.stringify(inputData),
        contentType: "application/json",
        dataType: "json",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("accept", "application/json");
        },
        success: function (response) {
            //console.log("Full Response:", response);
            // ✅ response is already OBJECT
            let userRightsData = response.data || [];
            //console.log("userRightsData:", userRightsData);
            validateUserAccessOfCurrentPage(userRightsData);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetching User Rights Failed");
        }
    });
}

//validate user access rights on load - if user access page directly via URL ------------ START ------------------------------------------------------
function validateUserAccessOfCurrentPage(userRightsData) {
    // Get the current URL and remove the domain name
    const currentUrl = window.location.pathname;
    // Validate user rights using the Link property
    const module = userRightsData.find(item => item.Link === currentUrl);
    //console.log("module:", module);
    // If module is not found or the user does not have view rights, prevent access
    if (!module || !module.CanView) {
        var isSuperUser = sessionStorage.getItem('isSuperUser');
        Swal.fire({
            icon: "error",
            title: "Access Denied",
            text: "You don't have permission access to this page. Please contact admin.",
            confirmButtonText: "OK"
        }).then(() => {
            if (isSuperUser === "true") {
                if (window.location.pathname === "/Home/Dashboard") {
                    logoutUser();
                } else {
                    window.location.href = "/Home/Dashboard"; // Redirect to Dashboard
                }
            } else {
                if (window.location.pathname === "/Home/Dashboard") {
                    logoutUser();
                } else {
                    window.location.href = "/Home/Dashboard"; // Redirect to Dashboard
                }
            }

        });
    }
    else {
        sessionStorage.setItem('Link', module.Link);
        sessionStorage.setItem('CanAdd', module.CanAdd);
        sessionStorage.setItem('CanEdit', module.CanEdit);
        sessionStorage.setItem('CanDelete', module.CanDelete);
    }
}
//validate user access rights on load - if user access page directly via URL ------------ END ------------------------------------------------------


// Handle the logout action
document.getElementById('btnLogout').addEventListener('click', function (e) {

    e.preventDefault();  // Prevent default link behavior

    const isautologinemployee = false; //sessionStorage.getItem('isautologinemployee') === 'true';
    //console.log("logout isautologinemployee:", isautologinemployee);
    if (isautologinemployee) {
        //PunchOut();
    }
    else {
        logoutUser();
    }
});

// Function to handle logout
function logoutUser() {
    //var isSuperUser = (sessionStorage.getItem('isSuperUser')) === 'true' ? true : false; // || localStorage.getItem('isSuperUser')) === 'true' ? true : false;
    // Clear session storage or local storage if needed
    sessionStorage.clear();
    localStorage.clear();

    window.location.href = '/';

    // if (isSuperUser) {
    //     window.location.href = '/';
    // }
    // else {
    //     window.location.href = '/Home/EmployeeLogin';
    // }
}

//USER PROFLE
(function () {
    const toggle = document.getElementById("navbarDropdownUser");
    const menu = toggle?.nextElementSibling;

    if (!toggle || !menu) return;

    toggle.addEventListener("click", function (e) {
        e.stopPropagation();
        menu.classList.toggle("show");
    });

    document.addEventListener("click", function () {
        menu.classList.remove("show");
    });

    menu.addEventListener("click", function (e) {
        e.stopPropagation(); // keep open when clicking inside
    });
})();

function formatDate2(date = new Date()) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`; // local date
}
function formatDate(date) {

    if (!date) return "";

    // If string → convert to Date
    if (typeof date === "string") {
        date = new Date(date);
    }

    // If still invalid date
    if (!(date instanceof Date) || isNaN(date)) {
        return "";
    }

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");

    return `${day}/${month}/${year}`;
}


//FOR REPORTS - DURATION filter
function applyQuickRange() {
    const today = new Date();
    let from = null;
    let to = null;

    switch (selectedMonthDuration) {

        case "thismonth":
            from = new Date(today.getFullYear(), today.getMonth(), 1);
            to = new Date(today.getFullYear(), today.getMonth() + 1, 0);
            break;

        case "lastmonth":
            from = new Date(today.getFullYear(), today.getMonth() - 1, 1);
            to = new Date(today.getFullYear(), today.getMonth(), 0);
            break;

        case "days": // Last 7 Days
            to = new Date();
            from = new Date();
            from.setDate(to.getDate() - 6);
            break;

        case "lastdays": // Last 30 Days
            to = new Date();
            from = new Date();
            from.setDate(to.getDate() - 29);
            break;

        case "yesterday":
            from = new Date();
            from.setDate(from.getDate() - 1);
            to = new Date(from);
            break;

        case "12months": // ✅ Last 12 Months
            to = new Date();
            from = new Date();
            from.setMonth(to.getMonth() - 11);
            from.setDate(1);
            break;

        default:
            return;
    }

    $("#from-date").val(formatDate(from));
    $("#to-date").val(formatDate(to));
}

