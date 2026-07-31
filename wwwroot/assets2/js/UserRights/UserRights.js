$(document).ready(function () {
    loadStaffDropdown();

    $("#ddlStaff").change(function () {
        const staffId = parseInt($(this).val()) || 0;

        if (!staffId) {
            $("#noStaffMsg").show();
            $("#rightsMainContent").addClass("d-none");
            $("#rightsTabs").empty();
            $("#rightsTabsContent").empty();
            $("#sopRightsContent").empty();
            return;
        }

        $("#noStaffMsg").hide();
        $("#rightsMainContent").removeClass("d-none");

        fetchUserRights(staffId);
        fetchSOPRights(staffId);
    });

    $("#btnSave").click(function (e) {
        e.preventDefault();

        const staffId = parseInt($("#ddlStaff").val()) || 0;

        if (!staffId) {
            Swal.fire("Warning", "Please select staff", "warning");
            return;
        }

        const $btn = $(this);

        $btn.prop("disabled", true);

        const loaderId = showTempLoader("Saving Module & SOP Access Rights...");

        $.when(
            saveUserRights(staffId),
            saveSOPRights(staffId)
        )
        .done(function (userRightsResponse, sopRightsResponse) {
            const userRes = Array.isArray(userRightsResponse) ? userRightsResponse[0] : userRightsResponse;
            const sopRes = Array.isArray(sopRightsResponse) ? sopRightsResponse[0] : sopRightsResponse;

            if (userRes.status !== "Success") {
                Swal.fire("Error", userRes.message || "Failed to save module rights.", "error");
                return;
            }

            if (sopRes.status !== "Success") {
                Swal.fire("Error", sopRes.message || "Failed to save SOP rights.", "error");
                return;
            }

            handleApiSuccess("Access rights saved successfully.");

            fetchUserRights(staffId);
            fetchSOPRights(staffId);
        })
        .fail(function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to Save Access Rights");
        })
        .always(function () {
            hideTempLoader(loaderId);
            $btn.prop("disabled", false);
        });
    });
});

function loadStaffDropdown(selectedId = null) {
    $.ajax({
        url: "/api/StaffMaster/GetStaffMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (response) {
            const ddl = $("#ddlStaff");

            ddl.empty();
            ddl.append(`<option value="">-- Select Staff --</option>`);

            (response.data || []).forEach(staff => {
                ddl.append(`<option value="${staff.Id}">${staff.Name}</option>`);
            });

            if (selectedId) {
                ddl.val(selectedId);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load staff");
        }
    });
}

function fetchUserRights(staffId) {
    const payload = {
        StaffId: staffId
    };

    $("#rightsTabs").html(`
        <li class="nav-item">
            <span class="nav-link disabled">
                <span class="spinner-border spinner-border-sm me-1"></span>
                Loading...
            </span>
        </li>
    `);

    $("#rightsTabsContent").empty();

    $.ajax({
        url: "/api/UserRights/GetUserRightsList",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            renderUserRightsTabs(response.data || []);
        },
        error: function (xhr, status, error) {
            $("#rightsTabs").empty();

            $("#rightsTabsContent").html(`
                <div class="text-center text-danger py-4">
                    Failed to load module rights.
                </div>
            `);

            handleApiError(xhr, status, error, "Failed to Load Rights");
        }
    });
}

function renderUserRightsTabs(data) {

    const fixedTabs = [
        "Dashboard",
        "Company",
        "Client",
        "Inventory",
        "Master",
        "Finance",
        "Report",
        "Staff"
    ];

    const tabMenu = $("#rightsTabs");
    const tabContent = $("#rightsTabsContent");

    tabMenu.html("");
    tabContent.html("");

    if (!data || data.length === 0) {
        tabContent.html(`<p class="text-center text-danger">No Rights Found</p>`);
        return;
    }

    fixedTabs.forEach((tabName, index) => {

        const clean = tabName.replace(/\s+/g, '');
        const tabId = "tab_" + clean;
        const contentId = "content_" + clean;
        const activeClass = index === 0 ? "active" : "";

        // TAB BUTTON
        tabMenu.append(`
            <li class="nav-item" role="presentation">
                <button class="nav-link ${activeClass}"
                    id="${tabId}"
                    data-bs-toggle="tab"
                    data-bs-target="#${contentId}"
                    type="button" role="tab">
                    ${tabName}
                </button>
            </li>
            `);

        const group = data.find(x => (x.GroupTitle || "").toLowerCase() === tabName.toLowerCase());
        const modules = group ? group.UserRights : [];

        let rows = "";
        let sr = 0;

        modules.forEach(m => {
            rows += `
                    <tr data-moduleid="${m.ModuleId}" data-staffid="${m.StaffId}">
                        <td class="text-center">${++sr}</td>
                        <td>${m.ModuleName}</td>

                        <td class="text-center">
                                <input type="checkbox" class="chkAdd_${clean}" ${m.CanAdd ? "checked" : ""}${!m.IsAddNeeded ? "hidden" : ""}>
                        </td>
                        <td class="text-center">
                                <input type="checkbox" class="chkEdit_${clean}" ${m.CanEdit ? "checked" : ""}${!m.IsEditNeeded ? "hidden" : ""}>
                        </td>
                        <td class="text-center">
                                <input type="checkbox" class="chkDelete_${clean}" ${m.CanDelete ? "checked" : ""}${!m.IsDeleteNeeded ? "hidden" : ""}>
                        </td>
                        <td class="text-center">
                                <input type="checkbox" class="chkView_${clean}" ${m.CanView ? "checked" : ""}${!m.IsViewNeeded ? "hidden" : ""}>
                        </td>
                    </tr>`;
        });


        // TAB CONTENT + SELECT ALL CHECKBOXES
        tabContent.append(`
            <div class="tab-pane fade show ${activeClass}" id="${contentId}" role="tabpanel">

                <div class="rights-table-wrapper">
                    <table class="rights-table">
                        <thead>
                            <tr>
                                <th class="col-sr">#</th>
                                <th class="col-module">Module Name</th>

                                <th class="col-perm">
                                    <input type="checkbox" class="selectAllAdd" data-group="${clean}"> Add
                                </th>
                                <th class="col-perm">
                                    <input type="checkbox" class="selectAllEdit" data-group="${clean}"> Edit
                                </th>
                                <th class="col-perm">
                                    <input type="checkbox" class="selectAllDelete" data-group="${clean}"> Delete
                                </th>
                                <th class="col-perm">
                                    <input type="checkbox" class="selectAllView" data-group="${clean}"> View
                                </th>
                            </tr>
                        </thead>

                        <tbody>
                            ${rows}
                        </tbody>
                    </table>
                </div>

            </div>
        `);

    });

    attachSelectAllEvents();   // 🔥 IMP: call handler
}

function attachSelectAllEvents() {

    // ADD — select all
    $(document).off("change", ".selectAllAdd").on("change", ".selectAllAdd", function () {
        const group = $(this).data("group");
        $(".chkAdd_" + group).prop("checked", this.checked);
    });

    // EDIT — select all
    $(document).off("change", ".selectAllAdd").on("change", ".selectAllEdit", function () {
        const group = $(this).data("group");
        $(".chkEdit_" + group).prop("checked", this.checked);
    });

    // DELETE — select all
    $(document).off("change", ".selectAllAdd").on("change", ".selectAllDelete", function () {
        const group = $(this).data("group");
        $(".chkDelete_" + group).prop("checked", this.checked);
    });

    // VIEW — select all
    $(document).off("change", ".selectAllAdd").on("change", ".selectAllView", function () {
        const group = $(this).data("group");
        $(".chkView_" + group).prop("checked", this.checked);
    });
}

function saveUserRights(staffId) {
    const actionBy = parseInt(sessionStorage.getItem("userId")) || 0;
    const userRights = [];

    $("#rightsTabsContent table tbody tr").each(function () {
        const $row = $(this);

        userRights.push({
            StaffId: staffId,
            ModuleId: parseInt($row.data("moduleid")) || 0,
            ActionBy: actionBy,
            CanAdd: $row.find("input[type='checkbox']").eq(0).is(":checked"),
            CanEdit: $row.find("input[type='checkbox']").eq(1).is(":checked"),
            CanDelete: $row.find("input[type='checkbox']").eq(2).is(":checked"),
            CanView: $row.find("input[type='checkbox']").eq(3).is(":checked")
        });
    });

    if (userRights.length === 0) {
        return $.Deferred().resolve({
            status: "Success",
            message: "No module rights found."
        }).promise();
    }

    const payload = {
        StaffId: staffId,
        UserRights: userRights
    };

    console.log("Module Rights Payload:", payload);

    return $.ajax({
        url: "/api/UserRights/SaveUserRights",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload)
    });
}

//SOP RIGHTS  --- START 
function fetchSOPRights(staffId) {
    const payload = {
        EmployeeId: staffId
    };

    $("#chkSOPSelectAll").prop("checked", false);

    $("#sopRightsContent").html(`
        <div class="text-center text-muted py-5">
            <div class="spinner-border spinner-border-sm text-warning me-2"></div>
            Loading SOP rights...
        </div>
    `);

    $.ajax({
        url: "/api/UserRights/GetUserRightsList_InfinityGrowth_SOP",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            if (response.status !== "Success") {
                $("#sopRightsContent").html(`
                    <div class="text-center text-danger py-5">
                        ${response.message || "Failed to load SOP rights."}
                    </div>
                `);
                return;
            }

            renderSOPRights(response.data || []);
        },
        error: function (xhr, status, error) {
            $("#sopRightsContent").html(`
                <div class="text-center text-danger py-5">
                    Failed to load SOP rights.
                </div>
            `);

            handleApiError(xhr, status, error, "Failed to Load SOP Rights");
        }
    });
}

function renderSOPRights(data) {
    const container = $("#sopRightsContent");

    container.empty();

    if (!data || data.length === 0) {
        container.html(`
            <div class="text-center text-muted py-5">
                <i class="fa fa-file-pdf fa-2x mb-2"></i>
                <div>No SOP documents found.</div>
            </div>
        `);
        return;
    }

    const categories = [];

    data.forEach(item => {
        const categoryId = parseInt(item.CategoryId) || 0;

        if (!categories.some(x => x.CategoryId === categoryId)) {
            categories.push({
                CategoryId: categoryId,
                CategoryName: item.CategoryName || "Other",
                ParentCategoryId: parseInt(item.ParentCategoryId) || 0
            });
        }
    });

    const rootCategories = categories.filter(x => !x.ParentCategoryId);

    let html = `<div class="sop-rights-list">`;

    rootCategories.forEach(category => {
        html += buildSOPCategory(category, categories, data, 0);
    });

    html += `</div>`;

    container.html(html);

    updateSOPSelectAllState();
}
function buildSOPCategory(category, categories, documents, level) {
    const childCategories = categories.filter(x => x.ParentCategoryId === category.CategoryId);
    const categoryDocuments = documents.filter(x => {
        return parseInt(x.CategoryId) === category.CategoryId;
    });

    const categoryClass = level === 0 ? "sop-main-category" : "sop-sub-category";
    const icon = level === 0 ? "fa-folder-open" : "fa-folder";
    const margin = level * 18;

    console.log("category:", category);
    let html = `
        <div class="${categoryClass}" data-categoryid="${category.CategoryId}">
            <div class="sop-category-header">
                <div class="d-flex align-items-center">
                    <i class="fa ${icon} text-warning me-2"></i>
                    <span>${escapeHtml(category.CategoryName)}</span>
                </div>

                <div class="form-check mb-0">
                    <input type="checkbox"
                           class="form-check-input chkSOPCategory"
                           data-categoryid="${category.CategoryId}">
                </div>
            </div>
    `;

    categoryDocuments.forEach(doc => {
        html += buildSOPDocument(doc, level + 1);
    });

    childCategories.forEach(child => {
        html += buildSOPCategory(child, categories, documents, level + 1);
    });

    html += `</div>`;

    return html;
}
function buildSOPDocument(doc, level) {
    const margin = level * 18;

    return `
        <div class="sop-document-row"
             data-documentid="${doc.DocumentId}"
             data-categoryid="${doc.CategoryId}"
             style="padding-left:${margin}px;">

            <div class="sop-document-name">
                <i class="fa fa-file-pdf text-danger me-2"></i>
                <span>${escapeHtml(doc.DocumentName || "-")}</span>
            </div>

            <div class="form-check mb-0">
                <input type="checkbox"
                       class="form-check-input chkSOPView"
                       data-documentid="${doc.DocumentId}"
                       data-accessrightid="${doc.AccessRightId || 0}"
                       ${doc.CanView ? "checked" : ""}>
            </div>

        </div>
    `;
}
$(document).on("change", "#chkSOPSelectAll", function () {
    $(".chkSOPView").prop("checked", this.checked);
    $(".chkSOPCategory").prop("checked", this.checked);
});

$(document).on("change", ".chkSOPCategory", function () {
    const category = $(this).closest(".sop-main-category, .sop-sub-category");

    category.find(".chkSOPView").prop("checked", this.checked);
    category.find(".chkSOPCategory").not(this).prop("checked", this.checked);

    updateSOPSelectAllState();
});

$(document).on("change", ".chkSOPView", function () {
    updateSOPCategoryStates();
    updateSOPSelectAllState();
});
function updateSOPCategoryStates() {
    $(".sop-main-category, .sop-sub-category").each(function () {
        const $category = $(this);
        const $documents = $category.find(".chkSOPView");
        const $categoryCheckbox = $category.children(".sop-category-header").find(".chkSOPCategory").first();

        if (!$documents.length) {
            $categoryCheckbox.prop("checked", false);
            $categoryCheckbox.prop("indeterminate", false);
            return;
        }

        const total = $documents.length;
        const checked = $documents.filter(":checked").length;

        $categoryCheckbox.prop("checked", checked === total);
        $categoryCheckbox.prop("indeterminate", checked > 0 && checked < total);
    });
}

function updateSOPSelectAllState() {
    const $documents = $(".chkSOPView");
    const $selectAll = $("#chkSOPSelectAll");

    if (!$documents.length) {
        $selectAll.prop("checked", false);
        $selectAll.prop("indeterminate", false);
        return;
    }

    const total = $documents.length;
    const checked = $documents.filter(":checked").length;

    $selectAll.prop("checked", checked === total);
    $selectAll.prop("indeterminate", checked > 0 && checked < total);

    updateSOPCategoryStates();
}
function escapeHtml(value) {
    return $("<div>").text(value || "").html();
}
function getSOPRights(staffId) {
    const sopRights = [];

    $(".chkSOPView").each(function () {
        sopRights.push({
            EmployeeId: staffId,
            DocumentId: parseInt($(this).data("documentid")) || 0,
            CanView: $(this).is(":checked")
        });
    });

    return sopRights;
}
function saveSOPRights(staffId) {
    const sopRights = getSOPRights(staffId);

    if (sopRights.length === 0) {
        return $.Deferred().resolve({
            status: "Success",
            message: "No SOP documents found."
        }).promise();
    }

    const payload = {
        StaffId: staffId,
        SOPRights: sopRights
    };

    console.log("SOP Rights Payload:", payload);

    return $.ajax({
        url: "/api/UserRights/SaveUserRights_InfinityGrowth_SOP",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload)
    });
}
//SOP RIGHTS  --- END 

