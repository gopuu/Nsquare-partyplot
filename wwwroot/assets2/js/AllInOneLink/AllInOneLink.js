$(document).ready(function () {
        loadAllInOneLinks();
});

function loadAllInOneLinks() {
    $.ajax({
        url: "/api/AllInOneMaster/GetAllInOneLinkMaster",
        type: "POST",
        contentType: "application/json",
        success: function (res) {

            if (res.status !== "Success") return;

            let html = "";

            res.data.forEach(item => {
                html += `
            <div class="col-4 col-md-4">
                <div class="allinone-item"
                        onclick="openLink('${item.Link}')">

                    <img src="${item.Image || '/assets2/img/default-icon.png'}"
                            alt="${item.Title}" />

                    <div class="allinone-title">
                        ${item.Title}
                    </div>
                </div>
            </div>
            `;
            });

            $("#allInOneGrid").html(html);
        },
        error: function (err) {
            console.error("AllInOne API error", err);
        }
    });
}

function openLink(url) {
    if (url && url !== "#") {
        window.open(url, "_blank");
    }
}




