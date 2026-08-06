// PERMISSION CHECK
function checkPermission(type) {
    let allowed = (sessionStorage.getItem(type) || localStorage.getItem(type)) === "true";
    if (!allowed) Swal.fire("Access Denied", "Permission Required", "error");
    return allowed;
}


const walletToast = Swal.mixin({
    toast: true,
    position: "top-end", // 👉 top-right
    showConfirmButton: false,
    timer: 2000,
    timerProgressBar: true,
    didOpen: (toast) => {
        toast.onmouseenter = Swal.stopTimer;
        toast.onmouseleave = Swal.resumeTimer;
    }
});

AppSignalR.subscribeWallet(function (data) {
    //console.log("Wallet Updated (layout):", data);
    fetchBalance_Layout();

    // 🔥 Your Settings Page Logic Here
    if (data) {
        let type = (data.type || "").toLowerCase();
        let icon = type === "credit" ? "success" : "warning";
        //let sign = ""; // type === "credit" ? "+" : "-";

        walletToast.fire({
            icon: icon,
            title: "Payment Success",
            text: `₹${data.amount} ${type === "credit" ? "added" : "deducted"}`
        });
    }
});

function fillTimeDropdown(dropdownId = '#time', interval = 5, isValueIn24=true) {
    const dropdown = $(dropdownId);
    dropdown.empty();
    dropdown.append(`<option value="">Select Time</option>`);
    //------------------------------------------
    // Generate 24 Hours
    //------------------------------------------
    for (let hour24 = 0; hour24 < 24; hour24++) {
        for (let minute = 0; minute < 60; minute += interval) {
            //------------------------------------------
            // Value (24 Hour Format)
            //------------------------------------------
            const hh = hour24.toString().padStart(2, '0');
            const mm = minute.toString().padStart(2, '0');
            const value24 = `${hh}:${mm}`;

            //------------------------------------------
            // Text (12 Hour Format)
            //------------------------------------------
            const period = hour24 >= 12 ? 'PM' : 'AM';
            let hour12 = hour24 % 12;
            if (hour12 === 0) {
                hour12 = 12;
            }
            const text12 = `${hour12}:${mm} ${period}`;

            //------------------------------------------
            // Append Option
            //------------------------------------------
            if(isValueIn24){
                dropdown.append(`<option value="${value24}">${text12}</option>`);
            }
            else{
                dropdown.append(`<option value="${text12}">${text12}</option>`);
            }
        }
    }
}


//WHATSAPP -- START
function sendWhatsAppMessage({mobileNo = '', templateName = '', variables = [], actionBy = 0, isPromotional = false}) {

    if (!mobileNo || !templateName)
        return;

    let requestData = {
        companyId: Number(sessionStorage.getItem("companyid") ?? 0),
        mobileNo: `91${mobileNo}`,
        messageType: "TEMPLATE",
        messageCount: 1,

        referenceId: generateGUID(),
        templateId: null,
        isPromotional: isPromotional,
        actionBy: Number(actionBy),

        templateName: templateName,
        message: variables.join('&')
    };

    console.log("requestData:", requestData);

    $.ajax({
        url: "/api/Wallet/SendWhatsApp", // 🔥 update controller name
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        beforeSend: function () {
            $("#btnSend").prop("disabled", true).text("Sending...");
        },
        success: function (res) {
            if (res.status === "Success") {

                Swal.fire({
                    icon: "success",
                    title: "Success",
                    text: res.message,
                    confirmButtonColor: "#28a745"
                });

                console.log("Balance:", res.data.Balance);
                console.log("Deducted:", res.data.DeductedAmount);

            } else {

                Swal.fire({
                    icon: "error",
                    title: "Failed",
                    text: res.message,
                    confirmButtonColor: "#dc3545"
                });
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "WhatsApp Message Failed");
        }
    });
}
function generateGUID() {
    return 'xxxxxxxx-xxxx-xxxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        const r = Math.random() * 16 | 0,
            v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

//WHATSAPP -- END
let isTempLoaderActive = false;

//LOADER FOR DOWNLOADS
let tempLoaderCounter = 0;

function showTempLoader(title = "Generating Report...") {
    isTempLoaderActive = true;

    tempLoaderCounter++;

    const loaderId = `tempLoader_${tempLoaderCounter}`;

    if (!$("#tempLoaderContainer").length) {
        $("body").append(`<div id="tempLoaderContainer" class="temp-loader-container"></div>`);
    }

    $("#tempLoaderContainer").append(`
        <div id="${loaderId}" class="temp-loader-item">
            <div class="temp-loader-spinner"></div>
            <div class="temp-loader-info">
                <div class="temp-loader-title">${title}</div>
                <div class="temp-loader-text">Please wait...</div>
            </div>
        </div>
    `);

    return loaderId;
}

function hideTempLoader(loaderId) {
    isTempLoaderActive = false;

    const $loader = $(`#${loaderId}`);

    $loader.addClass("temp-loader-hide");

    setTimeout(function () {
        $loader.remove();

        if (!$("#tempLoaderContainer").children().length)
            $("#tempLoaderContainer").remove();
    }, 200);
}


function handleApiSuccess(msg = "Saved successfully!") {
    Swal.fire({
        toast: true,
        position: "top-end",
        icon: "success",
        title: msg,
        showConfirmButton: false,
        timer: 800,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });
}

/* =========================================================
   FILE -> BASE64
========================================================= */
function fileToBase64(file) {
    return new Promise(function (resolve, reject) {
        const reader = new FileReader();

        reader.onload = function () {
            const base64 = reader.result.split(",")[1];
            resolve(base64);
        };

        reader.onerror = function () {
            reject(reader.error);
        };

        reader.readAsDataURL(file);
    });
}

function formatINR(amount) {
    if (!amount) return "0";
    return amount.toLocaleString("en-IN");
}
function formatINR2(amount) {
    if (!amount) return "₹ 0.00";

    return "₹ " + Number(amount).toLocaleString("en-IN", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
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

    // Get logo from sessionStorage
    /*
    const logo = sessionStorage.getItem('companylogo');
    // Set to img
    if (logo) {
        $(".infinitylogo_img").attr("src", logo);
    }*/

    //settings
    // initial fetch
    //layout_fetchData_Settings();

    // realtime admin updates
    /*
    if (!window._settingsSubscribed) {
        window._settingsSubscribed = true;
        AppSignalR.subscribeSettings(() => {
            layout_fetchData_Settings();
        });
    }
    */

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
    //MEGA-MENU
    // Prevent Bootstrap default dropdown behavior for Masters
    $('.modules-parent > .dropdown-toggle').on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();

        const $menu = $(this).next('.modules-mega-menu');

        // Close other open mega menus
        $('.modules-mega-menu').not($menu).removeClass('show');

        // Toggle current
        $menu.toggleClass('show');
    });

    // Close when clicking outside
    $(document).on('click', function () {
        $('.modules-mega-menu').removeClass('show');
    });

    // Don't close when clicking inside menu
    $('.modules-mega-menu').on('click', function (e) {
        e.stopPropagation();
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

   fetchBalance_Layout();


   //ALL-IN-ONE LINK ---- START -----------------------------
    $(".btnCopy_AllInLink").click(function () {
        const link = window.location.origin + "/AllInOneLink";
        navigator.clipboard.writeText(link);

        // optional toast/message
        $(this).html(`<i class="fa-solid fa-check me-1"></i>Copied`);

        setTimeout(() => {
            $(this).html(`
                <i class="fa-regular fa-copy me-1"></i>Copy Link
            `);
        }, 2000);
    });
   //ALL-IN-ONE LINK ---- END -----------------------------

});


window.AppSettings = {
    showLimit: false
};

function applyShowLimit() {

    $(".showLimit_section").toggleClass(
        "d-none",
        window.AppSettings.showLimit
    );
}

function updateShowLimitSetting(settingsData) {

    if (!Array.isArray(settingsData)) return;

    const showLimitItem = settingsData.find(x =>
        x.Name?.trim().toLowerCase() === "show limit"
    );

    window.AppSettings.showLimit =
        showLimitItem?.Value?.toString().toLowerCase() === "true"
        || showLimitItem?.Value == "1";

    applyShowLimit();

    // optional global event
    $(document).trigger(
        "showLimitChanged",
        [window.AppSettings.showLimit]
    );
}

function layout_fetchData_Settings() {
    $.ajax({
        url: "/api/Settings/GetSettings",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            //data = response; // Store the fetched data
            console.log("response fetchData. :", response);
            var data = response.data;
            //renderTable(data);
            updateShowLimitSetting(data || []);
        },
        error: function (xhr, statue, error) {
            handleApiError(xhr, statue, error, "Fetch Settings Failed.");
        }
    });
}


function fetchBalance_Layout() {
    let companyId = sessionStorage.getItem("companyid") || 1;

    var payload = {
        CompanyId: Number(companyId)
    };
    //console.log("payload wallet:", payload);
    $.ajax({
        type: "POST",
        url: "/api/Wallet/GetWalletBalance",
        data: JSON.stringify(payload),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            //console.log("wallet response:", response);
            let data = response.data;
            if (data && data.Balance > 0) {

                const balance = new Intl.NumberFormat('en-IN', {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }).format(data.Balance || 0);
                //console.log("balance:", balance);
                $(".walletBal2").text(`₹${balance}`);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch balance failed");
        }
    });
}

let ajaxActiveCount = 0;

$(document).ajaxStart(function () {
    ajaxActiveCount++;
    //console.log("ajaxActiveCount START:", ajaxActiveCount);

    // Temp Swal loader is already handling loading UI
    if (isTempLoaderActive) {
        return;
    }

    showLoader();
});

$(document).ajaxStop(function () {
    ajaxActiveCount--;
    if (ajaxActiveCount <= 0) {
        ajaxActiveCount = 0;

        // Temp Swal loader is handling loading UI
        if (isTempLoaderActive) {
            return;
        }
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
function formatDate_DMYToYMD(dateStr) {
    if (!dateStr) return "";

    const parts = dateStr.split("/"); // dd/mm/yyyy

    if (parts.length !== 3) return "";

    const [day, month, year] = parts;

    return `${year}-${month.padStart(2, "0")}-${day.padStart(2, "0")}`;
}

function formatTime24(date = new Date()) {
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    const seconds = String(date.getSeconds()).padStart(2, "0");
    return `${hours}:${minutes}:${seconds}`;
}
function formatDateTime(date = new Date()) {

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");

    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    const seconds = String(date.getSeconds()).padStart(2, "0");

    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
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

        case "last7days": // Last 7 days
            to = new Date();
            from = new Date();
            from.setDate(to.getDate() - 6);
            break;

        case "last30days": // Last 30 days
            to = new Date();
            from = new Date();
            from.setDate(to.getDate() - 29);
            break;

        case "yesterday":
            from = new Date();
            from.setDate(from.getDate() - 1);
            to = new Date(from);
            break;

        case "last12months": // ✅ Last 12 Months
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

