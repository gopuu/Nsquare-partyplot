/* ═══════════════════════════════════════════════════
   COLUMN DEFINITIONS
═══════════════════════════════════════════════════ */
const REPORT_COLUMNS = [
    { key: "SrNo", label: "Sr No" },
    { key: "ReferenceNo", label: "Reference No" },
    { key: "TransferDate", label: "Transfer Date" },
    { key: "FromAccountName", label: "From Account" },
    { key: "ToAccountName", label: "To Account" },
    { key: "Amount", label: "Amount" },
    { key: "Notes", label: "Notes" },
    { key: "Category", label: "Category" }
];

let selectedColumns = REPORT_COLUMNS.map(c => c.key);
let selectedMonthDuration = "thismonth";
const HEADER_KEY = "fundTransferPrintWithHeader";

$(document).ready(function () {
    loadAccountDropdowns();
    fetchFundTransfers();
    setCurrentMonthDates();
    loadFundTransferCategory();

    //ADD 
    $(document).on("click", "#btn-add-fund-transfer", function () {
        var canAdd = (sessionStorage.getItem('CanAdd') || localStorage.getItem('CanAdd')) === 'true';

        if (!canAdd) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to add new record. Please contact admin.",
            });
            return;
        }

        resetFundTransferForm();
        $("#FundTransferModalTitle").text("Add Fund Transfer");
        $("#txtTransferDate").val(new Date().toISOString().split('T')[0]);
        //loadAccountDropdowns();
        $("#FundTransferModal").modal("show");
    });

    // SAVE FUND TRANSFER
    $("#btnSaveFundTransfer").click(function () {
        let model = {
            Id: $("#FundTransferId").val(),
            FromAccountId: $("#ddlFromAccount").val(),
            ToAccountId: $("#ddlToAccount").val(),
            Amount: $("#txtAmount").val(),
            TransferDate: $("#txtTransferDate").val(),
            ReferenceNo: $("#txtReferenceNo").val(),
            Notes: $("#txtNotes").val(),
            ActionBy: sessionStorage.getItem("userId") ?? 0,
            FundTransfer_Category_Id: $("#ddlCategory").val()
        };

        // ✅ Required field validation
        if (!validateRequiredFields(fundTransferValidationRules)) {
            return;
        }

        // ✅ Business validations
        if (model.FromAccountId === model.ToAccountId) {
            Swal.fire("Invalid", "From Account and To Account cannot be same!", "warning");
            return;
        }

        if (parseFloat(model.Amount) <= 0) {
            Swal.fire("Invalid", "Amount must be greater than zero.", "warning");
            return;
        }

        $.ajax({
            url: "/api/FundTransfer/SaveFundTransfer",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(model),
            success: function (response) {
                if (response.status === "Success") {
                    handleApiSuccess("Saved!");
                    $("#FundTransferModal").modal("hide");
                    resetFundTransferForm();
                    fetchFundTransfers(); // Reload grid
                }
                else {
                    Swal.fire("Error", response.message, "error");
                }
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Save FundTransfer Error")
            }
        });
    });

    // Edit button click for Fund Transfer
    $(document).on("click", ".btn-edit", function (event) {

        var canEdit = (sessionStorage.getItem('CanEdit') || localStorage.getItem('CanEdit')) === 'true';

        if (!canEdit) {
            Swal.fire({
                icon: "error",
                title: "Access Denied",
                text: "You don't have permission to Edit. Please contact admin.",
            });
            event.preventDefault();
            return;
        }

        resetFundTransferForm(); // Clear previous modal values

        // Get DataTable instance
        var table = $('#tableMain table').DataTable();

        // Get clicked row data
        var row = $(this).closest('tr');
        var rowData = table.row(row).data();

        //console.log("Editing Fund Transfer:", rowData);

        // Fill hidden ID field
        $("#FundTransferId").val(rowData.Id);

        // Load dropdown and then select values
        bindDropdown("#ddlFromAccount", accountList);
        $("#ddlFromAccount").val(rowData.FromAccountId).trigger("change");

        bindDropdown("#ddlToAccount", accountList, rowData.FromAccountId);
        $("#ddlToAccount").val(rowData.ToAccountId);

        // Amount
        $("#txtAmount").val(rowData.Amount);

        // Set date (yyyy-mm-dd format)
        if (rowData.TransferDate) {
            let date = new Date(rowData.TransferDate);
            let yyyy = date.getFullYear();
            let mm = ("0" + (date.getMonth() + 1)).slice(-2);
            let dd = ("0" + date.getDate()).slice(-2);
            $("#txtTransferDate").val(`${yyyy}-${mm}-${dd}`);
        } else {
            $("#txtTransferDate").val(new Date().toISOString().split('T')[0]);
        }

        $("#txtReferenceNo").val(rowData.ReferenceNo);
        $("#txtNotes").val(rowData.Notes);
        $("#ddlCategory").val(rowData.FundTransfer_Category_Id);

        // Update Title
        $("#FundTransferModalTitle").text("Edit Fund Transfer");

        // Show Modal
        $("#FundTransferModal").modal("show");
    });

    // Delete button click in table
    $(document).on("click", ".btn-delete", function () {
        // Get DataTable instance
        var table = $("#tableMain table").DataTable(); // make sure this matches your table id
        var rowData = table.row($(this).closest("tr")).data();
        if (!rowData || !rowData.Id) {
            Swal.fire("Error", "Unable to get selected row data.", "error");
            return;
        }
        // Store Id in modal using jQuery data
        $("#deleteRecordModal").data("id", rowData.Id).modal("show");
    });

    $("#delete-record").click(function () {
        var id = $("#deleteRecordModal").data("id");
        if (!id) {
            Swal.fire("Error", "Record Id not found.", "error");
            return;
        }
        $.ajax({
            url: `/api/FundTransfer/DeleteFundTransfer`,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify({ Id: id }),
            success: function (response) {
                Swal.fire("Deleted!", response.message || "Record deleted.", "success");
                $("#deleteRecordModal").modal("hide");
                fetchFundTransfers();
            },
            error: function (xhr, status, error) {
                handleApiError(xhr, status, error, "Delete Failed!")
            }
        });
    });

    $(document).on("change", "#ddlFromAccount", function () {
        let selectedFrom = $(this).val();
        let selectedTo = $("#ddlToAccount").val();
        bindDropdown("#ddlToAccount", accountList, selectedFrom, selectedTo);
    });

    $(document).on("change", "#ddlToAccount", function () {
        let selectedTo = $(this).val();
        let selectedFrom = $("#ddlFromAccount").val();
        bindDropdown("#ddlFromAccount", accountList, selectedTo, selectedFrom);
    });

    /* ── Other filter dropdowns ── */
    $("#filter_ddlFromAccount, #filter_ddlToAccount, #filter_ddlCategory, #filter_txtAmount").on("change", function () {
        fetchFundTransfers();
    });

    /* ── Quick range ── */
    $("#quickRange").on("change", function () {

        const newFilter = $(this).val();
        if (selectedMonthDuration === newFilter) return;
        selectedMonthDuration = newFilter;

        if (selectedMonthDuration === "customrange") return;

        const today = new Date();
        let fromDate, toDate;

        switch (selectedMonthDuration) {
            case "thismonth":
                fromDate = new Date(today.getFullYear(), today.getMonth(), 1);
                toDate = new Date(today.getFullYear(), today.getMonth() + 1, 0);
                break;
            case "lastmonth":
                fromDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
                toDate = new Date(today.getFullYear(), today.getMonth(), 0);
                break;
            case "last7days":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 6);
                toDate = today;
                break;
            case "last30days":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 29);
                toDate = today;
                break;
            case "yesterday":
                fromDate = new Date(today);
                fromDate.setDate(today.getDate() - 1);
                toDate = new Date(fromDate);
                break;
            case "last12months":
                fromDate = new Date(today);
                fromDate.setMonth(today.getMonth() - 12);
                toDate = today;
                break;
        }

        $("#fromDate").val(formatDate2(fromDate));
        $("#toDate").val(formatDate2(toDate));
        fetchFundTransfers();
    });

    $("#fromDate, #toDate").on("change", function () {
        // If user manually edits date → switch dropdown to custom
        if ($("#quickRange").val() !== "customrange") {
            $("#quickRange").val("customrange");
            // Optional: update variable also
            selectedMonthDuration = "customrange";
        }
    });


    /* ── Filter panel ── */
    $("#filterPanel").on("click", function (e) { e.stopPropagation(); });

    $(document).on("click", function (e) {
        if ($(e.target).closest(".dropdown, .dropdown-menu, #filterPanel, #btnToggleFilter").length > 0) return;
        $("#filterPanel").removeClass("active");
    });

    $("#btnToggleFilter").on("click", function () { $("#filterPanel").toggleClass("active"); });
    $("#btnCloseFilter").on("click", function () { $("#filterPanel").removeClass("active"); });

    $("#btnApplyFilter").on("click", function () {
        $("#filterPanel").removeClass("active");
        fetchFundTransfers();
    });

    $("#btnResetFilter").on("click", function () {
        $("#quickRange").val("thismonth");
        selectedMonthDuration = "thismonth";
        $("#txtAmount").val("");
        $("#filter_ddlFromAccount").val("0");
        $("#filter_ddlToAccount").val("0");
        $("#filter_ddlCategory").val("0");
        setCurrentMonthDates();
        fetchFundTransfers();
    });

    /* ── With Header ── */
    const saved = sessionStorage.getItem(HEADER_KEY);
    if (saved !== null)
        document.getElementById("toggleWithHeader").checked = (saved === "true");

    document.getElementById("toggleWithHeader").addEventListener("change", function () {
        sessionStorage.setItem(HEADER_KEY, this.checked);
    });

    /* ── Column selector ── */
    $("#btn-select-columns").on("click", function () {
        loadColumnCheckboxes();
        $("#modalColumns").modal("show");
    });

    $("#btn-reset-columns").on("click", function () {
        $(".column-checkbox").prop("checked", true);
        $("#chk-select-all").prop("checked", true);
    });

    $("#btn-save-columns").on("click", function () {
        selectedColumns = [];
        $(".column-checkbox:checked").each(function () {
            selectedColumns.push($(this).val());
        });
        $("#modalColumns").modal("hide");
        fetchFundTransfers();
    });

    $("#chk-select-all").on("change", function () {
        $(".column-checkbox").prop("checked", this.checked);
    });

});

//VALIDATIONS ---START
const fundTransferValidationRules = [
    { id: "ddlFromAccount", name: "From Account" },
    { id: "ddlToAccount", name: "To Account" },
    { id: "txtAmount", name: "Amount" },
    { id: "txtTransferDate", name: "Transfer Date" },
    { id: "ddlCategory", name: "Category" }
];
function validateRequiredFields(rules) {
    for (let field of rules) {
        let value = $("#" + field.id).val();

        if (!value || value.trim() === "" || value === "0") {
            Swal.fire("Required", `${field.name} is required.`, "warning");
            $("#" + field.id).focus();
            return false;
        }
    }
    return true;
}

//VALIDATIONS ---END

let accountList = [];
function loadAccountDropdowns() {
    var actionBy = Number(sessionStorage.getItem("userId") ?? 0);

    $.ajax({
        url: "/api/AccountMaster/GetAccount",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0, ActionBy: actionBy }),
        success: function (response) {
            accountList = response?.data ?? [];
            //console.log("accountList", accountList);

            // Only accounts allowed for transfer
            var staffIds = Number(sessionStorage.getItem("userId") ?? 0);
            loadStaffAccountsRights([staffIds]).then(() => {
                // Only accounts allowed for transfer
                const transferableAccounts = getAccountsByPermission("CanTransfer");
                accountList = transferableAccounts;
                //console.log("transferableAccounts:", transferableAccounts);

                bindDropdown("#ddlFromAccount", transferableAccounts);
                bindDropdown("#ddlToAccount", transferableAccounts);
                bindDropdown("#filter_ddlFromAccount", transferableAccounts);
                bindDropdown("#filter_ddlToAccount", transferableAccounts);
            });
            // bindDropdown("#ddlFromAccount", accountList);
            // bindDropdown("#ddlToAccount", accountList);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading account list")
        }
    });
}
function bindDropdown(selectId, data, excludeId = null, selectedValue = null) {
    $(selectId).empty();
    $(selectId).append(`<option value="0">-- Select Account --</option>`);

    //console.log("selectId:", selectId, ", data:", data, ", excludeId:", excludeId, ", selectedValue:",selectedValue);

    //console.log("A/c drodpown data:", data);
    data.forEach(item => {
        // allow showing selected value even if matching excludeId
        if (excludeId == null || item.AccountId != excludeId || item.AccountId == selectedValue) {
            const selected = item.AccountId == selectedValue ? "selected" : "";
            $(selectId).append(`<option value="${item.AccountId}" ${selected}>${item.AccountName}</option>`);
        }
    });
}

//----- ACCOUNTS RIGHTS -- START ------------------------------------
var accountRightsCache = [];
/* Load account rights for selected staff */
function loadStaffAccountsRights(staffIds = []) {
    //console.log("layout- staffIds:", staffIds);

    return $.ajax({
        url: "/api/AccountRights/GetAccountRights",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({ StaffIds: staffIds }),
        success: function (res) {
            //console.log("layout- AccountRights res:", res);
            accountRightsCache = res?.data ?? [];
        },
        error: function () {
            accountRightsCache = [];
        }
    });
}
function getAccountsByPermission(permission) {
    return (accountRightsCache || []).filter(a => a[permission] === true);
}
//----- ACCOUNTS RIGHTS -- END ------------------------------------

//----- CATEGORY -- START------------------------------------
function loadFundTransferCategory() {
    var payload = { id: 0 };
    $.ajax({
        url: '/api/FundTransfer_Category/GetFundTransferCategoryList', // 🔁 update controller name
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(payload), // 0 = get all
        success: function (response) {
            if (response.status === "Success") {
                let data = response.data;
                
                let ddlCategory = $("#filter_ddlCategory, #ddlCategory");
                // Clear existing
                ddlCategory.empty();

                // Default option
                $("#filter_ddlCategory").append(`<option value="0">All</option>`);
                $("#ddlCategory").append(`<option value="0">Select</option>`);

                // Append data
                $.each(data, function (i, item) {
                    let option = `<option value="${item.Id}">${item.Name}</option>`;
                    ddlCategory.append(option);
                });
            }
            else {
                console.error("API Error:", response.message);
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Error loading categories"); 
        }
    });
}
//----- CATEGORY -- END ------------------------------------


function resetFundTransferForm() {
    //$("#FundTransferForm")[0].reset();
    bindDropdown("#ddlFromAccount", accountList);
    bindDropdown("#ddlToAccount", accountList);

    $("#FundTransferId").val(0);
    $("#ddlFromAccount").val("0");
    $("#ddlToAccount").val("0");
    $("#txtAmount").val("0");
    $("#txtReferenceNo").val("");
    $("#txtNotes").val("");
    $("#ddlCategory").val("0");
    // Reset to today's date
    document.getElementById("txtTransferDate").valueAsDate = new Date();
}

// FETCH Fund Transfer Records
function fetchFundTransfers() {

    const payload = {
        Id: 0,
        FromAccountId: $('#filter_ddlFromAccount').val() || 0,
        ToAccountId: $('#filter_ddlToAccount').val() || 0,
        Amount: $('#filter_txtAmount').val() || 0,
        FilterDuration: $('#quickRange').val(),
        FromDate: $('#fromDate').val() || null,
        ToDate: $('#toDate').val() || null,
        FundTransfer_Category_Id: $('#filter_ddlCategory').val() || 0
    };

    //console.log("payload:", payload);

    $.ajax({
        url: "/api/FundTransfer/GetFundTransferList",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (response) {
            // console.log("Fund Transfers:", response);

            if (response.status === "Success") {
                const rows = response.data?.Rows || [];
                const totals = response.data?.Totals || 0;
                renderFundTransferTable(rows, totals);
            }
            else {
                Swal.fire("Error!", response.message, "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Something went wrong fetching Fund Transfers.")
        }
    });
}

function renderFundTransferTable(data, totalAmount) {

    //console.log("data:", data, ", totalAmount:", totalAmount);

    if ($.fn.dataTable.isDataTable("#tableMain table")) {
        $("#tableMain table").DataTable().destroy();
    }

    // clear table
    $("#tableMain table").empty();

    // ✅ If no data
    if (!data || data.length === 0) {

        $("#tableMain table").DataTable({
            data: [],
            columns: [
                { title: "#" },
                { title: "From" },
                { title: "To" },
                { title: "Amount" },
                { title: "Transfer Date" },
                { title: "# Reference" },
                { title: "Notes" },
                { title: "Category" },
                { title: "Actions" }
            ],
            language: {
                emptyTable: "No fund transfer records found"
            },
            paging: false,
            searching: false,
            ordering: false,
            initComplete: function () {
                $(this.api().table().header())
                    .find('th')
                    .addClass('bg-warning-subtle text-dark');
            },
        });

        return;
    }

    // add Sr.No
    data.forEach((x, i) => x.srno = i + 1);

    const table = $("#tableMain table").DataTable({

        data: data,

        columns: [
            { data: "srno", title: "#", className: "text-center" },
            { data: "FromAccountName", title: "From", className: "text-center" },
            { data: "ToAccountName", title: "To", className: "text-center" },
            {
                data: "Amount",
                title: "Amount",
                className: "text-end",
                render: val => `<b>₹ ${formatINR(val)}</b>`
            },
            {
                data: 'TransferDate_Format', // actual date (ISO format from backend: yyyy-MM-dd)
                title: 'Transfer Date',
                name: 'TransferDate',
                className: "text-center"
            },

            {
                data: "ReferenceNo",
                title: "# Reference",
                render: data => data ? `#${data}` : "-"
            },
            { data: "Notes", title: "Notes" },
            { data: "FundTransfer_Category_Name", title: "Category", className: "text-center" },

            {
                data: null,
                title: "Actions",
                className: "text-center",
                render: (data, type, row) => `
                    <div class="dropdown">
                        <button class="btn btn-outline-secondary btn-sm dropdown-toggle btn-action-xss"
                                type="button" data-bs-toggle="dropdown">
                            <i class="fa fa-ellipsis-v"></i>
                        </button>
                        <ul class="dropdown-menu">
                            <li>
                                <a class="dropdown-item btn-edit" data-id="${row.Id}">
                                    <i class="fa fa-edit text-warning me-2"></i> Edit
                                </a>
                            </li>
                            <li>
                                <a class="dropdown-item btn-delete" data-id="${row.Id}">
                                    <i class="fa fa-trash text-danger me-2"></i> Delete
                                </a>
                            </li>
                        </ul>
                    </div>
                `
            }
        ],

        pageLength: 10,
        ordering: true,
        searching: true,
        pagingType: "full_numbers",
        responsive: true,

        initComplete: function () {
            const api = this.api();

            // header style
            $(api.table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');

            // ✅ Add footer ONLY ONCE
            if ($("#tableMain table tfoot").length === 0) {
                const footer = `
            <tfoot>
                <tr style="background:#f3f3f3;font-weight:bold">
                    <td colspan="3" class="text-end">TOTAL</td>
                    <td class="text-end total-amount"></td>
                    <td colspan="4"></td>
                </tr>
            </tfoot>
        `;
                $("#tableMain table").append(footer);
            }

            // set value
            $("#tableMain table .total-amount")
                .html(`₹ ${Number(totalAmount).toLocaleString()}`);
        }
    });
}

/* ═══════════════════════════════════════════════════
   DOWNLOAD PDF / EXCEL
═══════════════════════════════════════════════════ */
function ViewFundTransferReport(actionName) {

    const withHeader = $("#toggleWithHeader").is(":checked");
    const userId = parseInt(sessionStorage.getItem("userId") || 0);

    const payload = {
        Id: 0,
        FromAccountId: $('#filter_ddlFromAccount').val() || 0,
        ToAccountId: $('#filter_ddlToAccount').val() || 0,
        Amount: $('#filter_txtAmount').val() || 0,
        FilterDuration: $('#quickRange').val(),
        FromDate: $('#fromDate').val() || null,
        ToDate: $('#toDate').val() || null,
        FundTransfer_Category_Id: $('#filter_ddlCategory').val() || 0,
        WithHeader: withHeader,
        Columns: selectedColumns,
        ActionBy: userId
    };
    console.log("payload:", payload);

    const url = actionName === "downloadExcel"
        ? "/api/FundTransfer/DownloadFundTransferReport_Excel"
        : "/api/FundTransfer/DownloadFundTransferReport_Pdf";

    showTempLoader("Generating Report...");

    $.ajax({
        url: url,
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        success: function (res) {

            Swal.close();

            if (res.status !== "Success") {
                Swal.fire("Error", res.message, "error");
                return;
            }

            const ext = actionName === "downloadExcel" ? "xlsx" : "pdf";
            const ts = new Date().toISOString().replace(/[:.-]/g, "");
            const link = document.createElement("a");
            link.href = res.data.FileUrl;
            link.download = `FundTransfer_Report_${ts}.${ext}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        },
        error: function (xhr, status, error) {
            Swal.close();
            handleApiError(xhr, status, error, "Download FundTransfer Report Failed");
            Swal.fire("Error", "Failed to generate report. Please try again.", "error");
        }
    });
}

/* ═══════════════════════════════════════════════════
   HELPERS
═══════════════════════════════════════════════════ */
function setCurrentMonthDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    $("#fromDate").val(formatDate2(firstDay));
    $("#toDate").val(formatDate2(lastDay));
}

function loadColumnCheckboxes() {
    let html = "";
    REPORT_COLUMNS.forEach(col => {
        const isChecked = selectedColumns.includes(col.key);
        html += `
        <div class="col-md-4 mb-2">
            <div class="form-check">
                <input class="form-check-input column-checkbox"
                       type="checkbox"
                       value="${col.key}"
                       ${isChecked ? "checked" : ""}>
                <label class="form-check-label">${col.label}</label>
            </div>
        </div>`;
    });
    $("#column-list").html(html);
    updateSelectAllState();
}

function updateSelectAllState() {
    const total = $(".column-checkbox").length;
    const checked = $(".column-checkbox:checked").length;
    $("#chk-select-all").prop("checked", total === checked);
}

