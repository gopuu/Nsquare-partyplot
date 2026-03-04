$(document).ready(function () {
    fetchAssetData();
    loadAssetCategoryDropdown();
    loadAssetSubCategoryDropdown();

    $("#btn-add").click(function () {
        resetAssetFields();
        $("#assetModalTitle").text("Add Asset Master");
        $("#assetModal").modal("show");
    });

    // Category → SubCategory (CASCADING)
    $("#ddlAssetCategory").change(function () {
        loadAssetSubCategoryDropdown($(this).val());
    });

    // SAVE
    $("#btnSave").click(function () {

        if ($("#assetName").val().trim() === "" || $("#ddlAssetCategory").val() == "0") {
            Swal.fire("Validation", "Asset Name & Category required", "warning");
            return;
        }

        let payload = {
            Id: $("#assetId").val(),
            AssetName: $("#assetName").val(),
            AssetUniqueCode: $("#assetUniqueCode").val(),
            CategoryId: $("#ddlAssetCategory").val(),
            SubCategoryId: $("#ddlAssetSubCategory").val() || null,
            Location: $("#location").val(),
            Remarks: $("#remarks").val(),
            OpeningQty: $("#openingQty").val(),
            ClosingQty: $("#closingQty").val(),
            IsActive: $("#isActive").val() === "true",
            ActionBy: sessionStorage.getItem("userId") ?? 0
        };

        $.ajax({
            url: "/api/AssetMaster/SaveAssetMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            success: function () {
                Swal.fire("Saved!", "", "success");
                $("#assetModal").modal("hide");
                fetchAssetData();
            }
        });
    });

    // EDIT
    $(document).on("click", ".btn-edit", function () {
        let data = $("#tableAsset table").DataTable()
            .row($(this).closest("tr")).data();

        $("#assetId").val(data.Id);
        $("#assetName").val(data.AssetName);
        $("#assetUniqueCode").val(data.AssetUniqueCode);
        $("#location").val(data.Location);
        $("#remarks").val(data.Remarks);
        $("#openingQty").val(data.OpeningQty);
        $("#closingQty").val(data.ClosingQty);
        $("#isActive").val(data.IsActive.toString());

        loadAssetCategoryDropdown(data.CategoryId);
        loadAssetSubCategoryDropdown(data.CategoryId, data.SubCategoryId);

        $("#assetModalTitle").text("Edit Asset Master");
        $("#assetModal").modal("show");
    });

    // DELETE
    $(document).on("click", ".btn-delete", function () {
        let data = $("#tableAsset table").DataTable()
            .row($(this).closest("tr")).data();
        $("#deleteRecordModal").data("id", data.Id).modal("show");
    });

    $("#delete-record").click(function () {
        $.ajax({
            url: "/api/AssetMaster/DeleteAssetMaster",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({
                Id: $("#deleteRecordModal").data("id"),
                ActionBy: sessionStorage.getItem("userId") ?? 0
            }),
            success: function () {
                Swal.fire("Deleted!", "", "success");
                $("#deleteRecordModal").modal("hide");
                fetchAssetData();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete AssetSubCategory Error");
            }
        });
    });
});

function fetchAssetData() {
    $.ajax({
        url: "/api/AssetMaster/GetAssetMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            renderAssetTable(res.data || []);
        }
    });
}

function renderAssetTable(data) {
    if ($.fn.dataTable.isDataTable("#tableAsset table")) {
        $("#tableAsset table").DataTable().destroy();
    }
    data.forEach((x, i) => x.srno = i + 1);
    $("#tableAsset table").DataTable({
        data: data,
        columns: [
            { data: "srno", title: "Sr No" },
            { data: "AssetName", title: "Asset Name" },
            { data: "AssetUniqueCode", title: "Code" },
            { data: "CategoryName", title: "Category" },
            { data: "SubCategoryName", title: "Sub Category", render: d => d ?? "-" },
            { data: "ClosingQty", title: "Qty" },
            {
                data: "IsActive",
                title: "Active",
                render: d => d ? "Yes" : "No"
            },
            {
                data: null,
                title: "Action",
                orderable: false,
                render: () => `
                    <button class="btn btn-outline-warning btn-edit btn-action-xss">
                        <i class="fa fa-edit"></i>
                    </button>
                    <button class="btn btn-outline-danger btn-delete btn-action-xss">
                        <i class="fa fa-trash"></i>
                    </button>`
            }
        ]
    });
}

function loadAssetCategoryDropdown(selectedId = 0) {
    $.ajax({
        url: "/api/AssetCategory/GetAssetCategoryMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),
        success: function (res) {
            let ddl = $("#ddlAssetCategory").empty();
            ddl.append(`<option value="0">-- Select Category --</option>`);
            res.data.forEach(x => {
                ddl.append(`<option value="${x.Id}" ${x.Id == selectedId ? "selected" : ""}>${x.Name}</option>`);
            });
        }
    });
}

function loadAssetSubCategoryDropdown(categoryId, selectedId = 0) {
    if (!categoryId) return;

    $.ajax({
        url: "/api/AssetSubCategory/GetAssetSubCategoryMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, CategoryId: categoryId }),
        success: function (res) {
            let ddl = $("#ddlAssetSubCategory").empty();
            ddl.append(`<option value="">-- Select Sub Category --</option>`);
            res.data.forEach(x => {
                ddl.append(`<option value="${x.Id}" ${x.Id == selectedId ? "selected" : ""}>${x.Name}</option>`);
            });
        }
    });
}

function resetAssetFields() {
    // hidden id
    $("#assetId").val(0);
    // text inputs
    $("#assetName").val("");
    $("#assetUniqueCode").val("");
    $("#location").val("");
    $("#remarks").val("");
    // numbers
    $("#openingQty").val(0);
    $("#closingQty").val(0);
    // dropdowns
    $("#ddlAssetCategory").val(0);
    $("#ddlAssetSubCategory").empty()
        .append(`<option value="">-- Select Sub Category --</option>`);
    // active status
    $("#isActive").val("true");
}
