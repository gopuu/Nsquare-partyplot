$(document).ready(function () {
    loadAllInOneLinks();
    $("#footer_msg").text("Memories. Made Here.");
});

function loadAllInOneLinks() {
    $.ajax({
        url: "/api/AllInOneMaster/GetAllInOneLinkMaster",
        type: "POST",
        contentType: "application/json",
        success: function (res) {
            console.log("res:", res);
            if (res.status !== "Success" || !res.data)
                return;

            let property = res.data;
            let links = property.Links || [];

            // Set Header
            $("#propertyName").text(property.PropertyName || "-");
            $("#companyName").text(property.CompanyName || "-");

            // Set Logo
            if (property.PropertyLogo) {
                $(".img_logo").attr("src", property.PropertyLogo);
            }

            let html = "";

            links.forEach(item => {
                html += `
                <div class="col-6 col-sm-4 col-lg-3 mb-4">
                    <div class="allinone-item" onclick="openLink('${escapeQuotes(item.Link)}')">
                        <img src="${item.Image || '/assets2/img/image_icon.png'}"
                             alt="${item.Title || ''}" />
                        <div class="allinone-title">
                            ${item.Title || '-'}
                        </div>
                    </div>
                </div>
                `;
            });

            $("#allInOneGrid").html(html);
        },
        error: function (xhr, status, error) {
            console.error("AllInOne API error", error);
        }
    });
}

function openLink2(url) {
    if (url && url !== "#") {
        window.open(url, "_blank");
    }
}
function openLink(value) {

    if (!value || value === "#")
        return;

    value = value.trim();

    // 🔹 If already full URL
    if (value.startsWith("http://") || value.startsWith("https://")) {
        window.open(value, "_blank");
        return;
    }

    // 🔹 If starts with + and digits (international format)
    if (/^\+\d{10,15}$/.test(value)) {
        window.location.href = "tel:" + value;
        return;
    }

    // 🔹 If only digits (10 to 15 digits)
    if (/^\d{10,15}$/.test(value)) {

        // If starts with 91 and 12 digits, assume India
        if (value.length === 12 && value.startsWith("91")) {
            window.location.href = "tel:+" + value;
        }
        else if (value.length === 10) {
            window.location.href = "tel:" + value;
        }
        else {
            window.location.href = "tel:+" + value;
        }

        return;
    }

    // 🔹 If email
    if (/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
        window.location.href = "mailto:" + value;
        return;
    }

    // 🔹 Otherwise assume website without protocol
    window.open("https://" + value, "_blank");
}

// Prevent onclick breaking due to quotes
function escapeQuotes(str) {
    if (!str) return "";
    return str.replace(/'/g, "\\'");
}