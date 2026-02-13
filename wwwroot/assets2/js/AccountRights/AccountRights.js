$(document).ready(function () {

    loadStaffList();
    loadAccounts();

    $("#btnClear").click(function () {
        $("#rightsTableBody input[type='checkbox']").prop("checked", false);
    });

    $(".chkStaff, .chkEdit, .chkView, .chkTransfer").on("change", function () {
        hideRightsError();
    });

    $("#btnSave").click(function () {

        let staffIds = [];
        $(".chkStaff:checked").each(function () {
            staffIds.push(parseInt($(this).val()));
        });

        if (staffIds.length === 0) {
            showRightsError("Please select at least one staff member.");
            return;
        }

        let rights = [];
        $("#rightsTableBody tr").each(function () {
            rights.push({
                AccountId: $(this).data("accountid"),
                CanEdit: $(this).find(".chkEdit").is(":checked"),
                CanView: $(this).find(".chkView").is(":checked"),
                CanTransfer: $(this).find(".chkTransfer").is(":checked")
            });
        });

        //if (rights.length === 0) {
        //    showRightsError("Please assign at least one permission.");
        //    return;
        //}

        let payload = {
            StaffIds: staffIds,
            AccountRights: rights,
            ActionBy: 0
        };

        $.ajax({
            url: "/api/AccountRights/SaveAccountRights",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                showRightsError("Rights assigned successfully.", "success");
            }
        });
    });

    $("#txtSearchStaff").on("keyup", function () {
        let keyword = $(this).val().toLowerCase().trim();
        $("#staffList .staff-item").each(function () {

            let staffName = $(this).data("name");
            if (staffName.includes(keyword)) {
                $(this).find("label").html(
                    staffName.replace(keyword, `<mark>${keyword}</mark>`)
                );
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

    $("#txtSearchStaff").on("keydown", function (e) {
        if (e.key === "Escape") {
            $(this).val("").trigger("keyup");
        }
    });

    $(document).on("change", ".chkStaff", function () {
        let staffIds = getSelectedStaffIds();
        if (staffIds.length === 0) {
            // No staff selected → load empty/default rights
            loadAccounts([]);
            return;
        }
        // Load merged rights for selected staff
        loadAccounts(staffIds);
    });

    $(document).on("click", ".staff-item", function (e) {

        // Prevent double toggle if checkbox itself is clicked
        if ($(e.target).is("input")) return;

        const checkbox = $(this).find(".chkStaff");
        checkbox.prop("checked", !checkbox.prop("checked")).trigger("change");
    });

});

let rightsMsgTimer = null;
function showRightsError(message, type = "danger", autoHideMs = 60000) {

    const msgBox = $("#rightsMsg");

    // Clear any existing timer
    if (rightsMsgTimer) {
        clearTimeout(rightsMsgTimer);
        rightsMsgTimer = null;
    }

    msgBox
        .removeClass("d-none alert-danger alert-warning alert-success alert-info")
        .addClass(`alert-${type}`)
        .text(message)
        .fadeIn(150);

    // Auto-hide after 1 minute
    rightsMsgTimer = setTimeout(() => {
        hideRightsError();
    }, autoHideMs);
}

function hideRightsError() {

    if (rightsMsgTimer) {
        clearTimeout(rightsMsgTimer);
        rightsMsgTimer = null;
    }

    $("#rightsMsg")
        .fadeOut(200, function () {
            $(this).addClass("d-none").text("");
        });
}

function getSelectedStaffIds() {
    let staffIds = [];
    $(".chkStaff:checked").each(function () {
        staffIds.push(parseInt($(this).val()));
    });
    return staffIds;
}

/* ================= LOAD STAFF ================= */
function loadStaffList() {
    $.ajax({
        url: "/api/StaffMaster/GetStaffMaster",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            let html = "";
            res.data.forEach(s => {
                html += `
                <div class="staff-item" data-name="${s.Name.toLowerCase()}">
                    <input type="checkbox"
                           class="form-check-input me-2 chkStaff"
                           value="${s.Id}">
                    <span class="staff-name">${s.Name}</span>
                </div>`;
            });

            $("#staffList").html(html);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading StaffMaster");
        }
    });
}

/* ================= LOAD ACCOUNTS ================= */
function loadAccounts(staffIds = []) {
    $.ajax({
        url: "/api/AccountRights/GetAccountRights",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ StaffIds: staffIds }),
        success: function (res) {
            console.log("AccountRights res:", res);

            let rows = "";

            if (!res || !Array.isArray(res.data) || res.data.length === 0) {
                rows = `
                    <tr>
                        <td colspan="5" class="text-center text-muted py-3">
                            No Accounts Found
                        </td>
                    </tr>`;
            }
            else {
                let i = 0;
                res.data.forEach(a => {
                    rows += `
                        <tr data-accountid="${a.AccountId}">
                            <td class="text-muted text-center">${++i}</td>
                            <td class="fw-medium">${a.AccountName}</td>

                            <td class="text-center">
                                <input type="checkbox" class="form-check-input chkEdit"
                                       ${a.CanEdit ? "checked" : ""}>
                            </td>

                            <td class="text-center">
                                <input type="checkbox" class="form-check-input chkView"
                                       ${a.CanView ? "checked" : ""}>
                            </td>

                            <td class="text-center">
                                <input type="checkbox" class="form-check-input chkTransfer"
                                       ${a.CanTransfer ? "checked" : ""}>
                            </td>
                        </tr>`;
                });
            }
            $("#rightsTableBody").html(rows);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading AccountRights");
        }

    });
}

