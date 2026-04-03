/*************************************************
 * STATIC DEFAULTS
 *************************************************/
let LABEL_SETTINGS = {
    fontFamily: "'Dancing Script', cursive",
    fontSize: 24
};

/*************************************************
 * LABEL DIMENSIONS (SINGLE SOURCE OF TRUTH)
 *************************************************/
let LABEL_DIMENSIONS = {
    realWidthPx: 180,
    realHeightPx: 90,
    previewWidthPx: 180,
    previewHeightPx: 90,
    minFontSize: 10,
    maxFontSize: 24
};

const FONT_FAMILY_MAP = {
    "Arial": "Arial, sans-serif",
    "Open Sans": "'Open Sans', sans-serif",
    "Dancing Script": "'Dancing Script', cursive",
    "Bebas Neue": "'Bebas Neue', sans-serif",
    "Roboto": "'Roboto', sans-serif",
    "Poppins": "'Poppins', sans-serif",
    "Lobster": "'Lobster', cursive",
    "Merriweather": "'Merriweather', serif",
    "Pacifico": "'Pacifico', cursive"
};

let FOOD_PACKAGE_DATA = [];

function calculateFontSize(text) {

    const baseSize = LABEL_DIMENSIONS.maxFontSize;
    const length = text.length;

    if (length <= 14) return baseSize;
    if (length <= 20) return baseSize - 2;
    if (length <= 28) return baseSize - 4;
    if (length <= 36) return baseSize - 6;

    return LABEL_DIMENSIONS.minFontSize;
}

/*************************************************
 * PAGE LOAD
 *************************************************/
var bookingId = 0;
document.addEventListener("DOMContentLoaded", function () {
    bookingId = sessionStorage.getItem("bookingid");

    var requestData = {
        BookingId: Number(bookingId),
        BanquetId: 0,
        EventDate: null,
        YearId: 0,
        MonthId: 0
    };
    console.log("GetBookingCalendar requestData:", requestData);
    $.ajax({
        url: "/api/BookingCalendar/GetBookingCalendar",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.status === "Success") {
                var data = response.data[0].JSON_FoodPackage;
                console.log("JSON_FoodPackage data:", data);

                if (data && data.length > 0) { 
                    FOOD_PACKAGE_DATA = data;
                    console.log("FOOD_PACKAGE_DATA 1: ", FOOD_PACKAGE_DATA);
                    //renderFoodLabels(FOOD_PACKAGE_DATA);
                    // Load label setup first
                    loadLabelSetup();
                }
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load booking calendar data');
        }
    });
});

function loadLabelSetup() {

    $.ajax({
        url: '/api/Company_FoodRate_LabelPrinting/GetCompanyFoodRateLabelPrinting',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            console.log("company setup res:", res);
            if (res.status !== "Success" || !res.data) return;

            /*************************************************
             * FONT SETTINGS
             *************************************************/
            const apiFont = (res.data.FontName || "Dancing Script").trim();
            LABEL_SETTINGS.fontFamily = FONT_FAMILY_MAP[apiFont] || apiFont;
            LABEL_SETTINGS.fontSize = parseInt(res.data.FontSize) || 24;
            LABEL_DIMENSIONS.maxFontSize = LABEL_SETTINGS.fontSize;

            /*************************************************
             * LABEL DIMENSIONS (SINGLE CALCULATION)
             *************************************************/
            const dims = calculateLabelDimensions(res.data.PlaqueSizeInches);
            Object.assign(LABEL_DIMENSIONS, dims);

            /*************************************************
             * RENDER LABELS
             *************************************************/
            if (Array.isArray(FOOD_PACKAGE_DATA) && FOOD_PACKAGE_DATA.length > 0) {
                renderFoodLabels(FOOD_PACKAGE_DATA);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, 'Failed to load label setup');
        }
    });
}

/*************************************************
 * RENDER FOOD LABELS
 *************************************************/
function renderFoodLabels(packages) {
    const container = document.getElementById("foodLabelContainer");
    if (!container) return;

    container.innerHTML = "";

    // ✅ Handle empty / null packages 
    if (!packages || packages.length === 0) {
        container.innerHTML = ` 
            <div class="text-center text-muted py-4">
                <h6 class="mb-1">No Packages Found</h6>
                <small>Please add a food package to see labels.</small>
            </div>
        `;
        return;
    }

    console.log("packages:", packages);
    packages.forEach(pkg => {

        const packageCard = document.createElement("div");
        packageCard.className = "package-card";

        /* HEADER */
        const headerRow = document.createElement("div");
        headerRow.className = "package-header mb-2";

        const title = document.createElement("h5");
        title.className = "text-danger fw-bold mb-0";
        title.innerHTML = `
            ${pkg.FoodPackageName} 
            <span class="booking-ref">(Booking Ref #${bookingId})</span>
        `;

        const toggleBtn = document.createElement("button");
        toggleBtn.className = "btn btn-sm btn-outline-secondary";
        toggleBtn.innerText = "Hide";

        headerRow.append(title, toggleBtn);
        packageCard.appendChild(headerRow);

        /* CONTENT */
        const content = document.createElement("div");
        content.className = "package-content";

        const groupedItems = {};
        pkg.MenuItemsList
            .filter(x => x.MenuItem_IsAdded)
            .forEach(item => {
                groupedItems[item.MenuHeader_Name] ??= [];
                groupedItems[item.MenuHeader_Name].push(item);
            });

        Object.keys(groupedItems).forEach(headerName => {

            const header = document.createElement("h6");
            header.className = "mt-3 mb-2 text-secondary fw-semibold";
            header.innerText = headerName;
            content.appendChild(header);

            const row = document.createElement("div");
            row.className = "row g-3 mb-2";

            groupedItems[headerName].forEach(item => {

                const col = document.createElement("div");
                col.className = "col-auto d-flex justify-content-center";

                const fontSize = calculateFontSize(item.MenuItem_Name);

                col.innerHTML = `
                        <div class="label-box border bg-white d-flex align-items-center justify-content-center text-center"
                             style="
                                width:${LABEL_DIMENSIONS.previewWidthPx}px;
                                height:${LABEL_DIMENSIONS.previewHeightPx}px;
                                padding:20px; box-sizing:border-box;
                             ">
                            <span class="food-label-preview"
                                  style="
                                    font-family:${LABEL_SETTINGS.fontFamily};
                                    font-size:${fontSize}px;
                                    line-height:1.2;
                                    word-break:break-word;
                                    padding:4px;
                                  ">
                                ${item.MenuItem_Name}
                            </span>
                        </div>
                    `;
                row.appendChild(col);
            });

            content.appendChild(row);
        });

        packageCard.appendChild(content);
        container.appendChild(packageCard);

        toggleBtn.onclick = () => {
            content.classList.toggle("collapsed");
            toggleBtn.innerText =
                content.classList.contains("collapsed") ? "Show" : "Hide";
        };
    });
}

function calculateLabelDimensions(plaqueSizeInches) {

    const DPI = 96;

    // 🔑 MUST MATCH SETUP PAGE
    const SETUP_PREVIEW_MAX_WIDTH = 300;

    let result = {
        realWidthPx: 180,
        realHeightPx: 90,
        previewWidthPx: 180,
        previewHeightPx: 90
    };

    if (!plaqueSizeInches) return result;

    const [wIn, hIn] = plaqueSizeInches
        .toLowerCase()
        .replace('x', '*')
        .split('*')
        .map(v => parseFloat(v.trim()));

    if (isNaN(wIn) || isNaN(hIn)) return result;

    const realWidthPx = wIn * DPI;
    const realHeightPx = hIn * DPI;

    const scale = Math.min(
        SETUP_PREVIEW_MAX_WIDTH / realWidthPx,
        1
    );

    // ✅ EXACT SAME PREVIEW SIZE AS SETUP PAGE
    result.realWidthPx = Math.round(realWidthPx);
    result.realHeightPx = Math.round(realHeightPx);
    result.previewWidthPx = Math.round(realWidthPx * scale);
    result.previewHeightPx = Math.round(realHeightPx * scale);

    return result;
}

/*************************************************
 * FONT CONTROL HELPERS
 *************************************************/
function getSelectedFont() {
    return LABEL_SETTINGS.fontFamily;
}

function getSelectedFontSize() {
    return LABEL_SETTINGS.fontSize;
}

function updateAllFoodLabels() {

    // Update static object (API-ready)
    LABEL_SETTINGS.fontFamily = getSelectedFont();
    LABEL_SETTINGS.fontSize = getSelectedFontSize();

    document.querySelectorAll(".food-label-preview").forEach(el => {
        el.style.fontFamily = LABEL_SETTINGS.fontFamily;
        el.style.fontSize = LABEL_SETTINGS.fontSize + "px";
    });
}
