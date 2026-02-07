$(document).ready(function () {
    loadStaffDropdown();
    //fetchUserRights();

    $("#rightsTabs").hide();
    //$("#rightsTabsContent").hide();
    $("#ddlStaff").change(function () {
        let staffId = $(this).val();
        if (staffId) {
            fetchUserRights(staffId);
            $("#rightsTabs").show();
            $("#rightsTabsContent").show();
        } else {
            $("#rightsTabs").hide();
            $("#rightsTabsContent").hide();
        }
    });

    $("#btnSave").click(function (e) {
        e.preventDefault();

        let staffId = $("#ddlStaff").val();
        if (!staffId) {
            Swal.fire("Warning", "Please select staff", "warning");
            return;
        }

        let actionBy = sessionStorage.getItem("userId") || 0;
        let userRights = [];

        // 🔁 Loop ALL rows (ALL tabs)
        $("#rightsTabsContent table tbody tr").each(function () {
            let moduleId = $(this).data("moduleid");
            let canAdd = $(this).find("input[type='checkbox']").eq(0).is(":checked");
            let canEdit = $(this).find("input[type='checkbox']").eq(1).is(":checked");
            let canDelete = $(this).find("input[type='checkbox']").eq(2).is(":checked");
            let canView = $(this).find("input[type='checkbox']").eq(3).is(":checked");

            userRights.push({
                StaffId: staffId,
                ModuleId: moduleId,
                ActionBy: actionBy,
                CanAdd: canAdd,
                CanEdit: canEdit,
                CanDelete: canDelete,
                CanView: canView
            });
        });
        if (userRights.length === 0) {
            Swal.fire("Info", "No rights to save", "info");
            return;
        }
        let payload = {
            StaffId: staffId,
            UserRights: userRights
        };
        console.log("Saving Payload:", payload);
        $('#btn-loader').show();
        $.ajax({
            url: "/api/UserRights/SaveUserRights",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function (res) {
                Swal.fire("Success", res.message || "User rights saved", "success");
            },
            error: function () {
                Swal.fire("Error", "Failed to save rights", "error");
            },
            complete: function () {
                $('#btn-loader').hide();
            }
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
            let ddl = $("#ddlStaff");
            ddl.empty().append(`<option value="">-- Select Staff --</option>`);
            response.data.forEach(staff => {
                ddl.append(`<option value="${staff.Id}">${staff.Name}</option>`);
            });

            if (selectedId) ddl.val(selectedId);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Failed to load dropdown data");
        }
    });
}

function fetchUserRights(staffId = 0) {
    $('#btn-loader').show();
    const payload = {
        StaffId: staffId
    };
    $.ajax({
        url: "/api/UserRights/GetUserRightsList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),

        success: function (response) {
            const result = response.data || [];
            renderUserRightsTabs(result);   // 🔵 MAIN FUNCTION
        },

        error: function (xhr, status, error) {
            console.error("Error fetching user rights:", error);
        },

        complete: function () {
            $('#btn-loader').hide();
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
    $(document).on("change", ".selectAllAdd", function () {
        const group = $(this).data("group");
        $(".chkAdd_" + group).prop("checked", this.checked);
    });

    // EDIT — select all
    $(document).on("change", ".selectAllEdit", function () {
        const group = $(this).data("group");
        $(".chkEdit_" + group).prop("checked", this.checked);
    });

    // DELETE — select all
    $(document).on("change", ".selectAllDelete", function () {
        const group = $(this).data("group");
        $(".chkDelete_" + group).prop("checked", this.checked);
    });

    // VIEW — select all
    $(document).on("change", ".selectAllView", function () {
        const group = $(this).data("group");
        $(".chkView_" + group).prop("checked", this.checked);
    });
}
