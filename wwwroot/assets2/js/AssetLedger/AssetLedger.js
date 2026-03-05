let selectedDuration = "ThisMonth";

$(document).ready(function () {
    loadAssetDropdown();
    fetchLedgerData();

});


/* ================================
   LOAD ASSET DROPDOWN
================================ */

function loadAssetDropdown() {

    $.ajax({
        url: "/api/AssetMaster/GetAssetMaster",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify({ Id: 0 }),

        success: function (res) {

            let list = res.data || [];

            let html = `<option value="0">All Assets</option>`;

            list.forEach(x => {
                html += `<option value="${x.Id}">${x.AssetName}</option>`;
            });

            $("#filterAsset").html(html);

        },

        error: function (xhr, status, error) {

            handleApiError(xhr, status, error, "Load Asset Dropdown Error");

        }
    });

}


/* ================================
   FETCH LEDGER DATA
================================ */

function fetchLedgerData() {
    let payload = {
        Id: 0,
        AssetMaster_Id: $("#filterAsset").val(),
        TransactionType: $("#filterTransactionType").val(),
        ReferenceTable: $("#filterReferenceTable").val(),
        ReferenceId: 0,
        MonthDuration: selectedDuration,
        FromDate: $("#filterFromDate").val(),
        ToDate: $("#filterToDate").val(),
        IsReversal: $("#filterReversal").val() === "" ? null : $("#filterReversal").val() === "1"
    };
    console.log("payload:", payload);
    $.ajax({
        url: "/api/AssetLedger/GetAssetLedger",
        type: "POST",
        data: JSON.stringify(payload),
        contentType: "application/json",
        success: function (response) {
            if (response.status === "Success") {
                renderLedgerTable(response.data);
            }
            else {
                Swal.fire("Error", response.message, "error");
            }
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch Asset Ledger Error");
        }
    });
}


/* ================================
   RENDER TABLE
================================ */

function renderLedgerTable(data) {

    if ($.fn.dataTable.isDataTable('#tableMain table')) {

        $('#tableMain table').DataTable().clear().destroy();

    }

    if (!data || data.length === 0) {

        $('#tableMain table').html(`
            <thead>
                <tr>
                    <th>#</th>
                    <th>Asset</th>
                    <th>Transaction</th>
                    <th>Qty In</th>
                    <th>Qty Out</th>
                    <th>Date</th>
                    <th>Source</th>
                    <th>Reference</th>
                    <th>Remarks</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td colspan="9" class="text-center fw-bold text-danger">
                        No Data Found !!
                    </td>
                </tr>
            </tbody>
        `);

        return;

    }

    data.forEach((x, i) => x.srno = i + 1);

    $('#tableMain table').DataTable({

        data: data,

        columns: [

            { data: "srno", title: "#" },

            { data: "AssetName", title: "Asset" },

            {
                data: "TransactionType",
                title: "Transaction",
                render: function (type) {

                    let badge = "secondary";

                    if (type === "Purchase") badge = "success";
                    if (type === "Remove") badge = "danger";
                    if (type === "RepairSent") badge = "warning";
                    if (type === "RepairReceived") badge = "info";

                    return `<span class="badge bg-${badge}">${type}</span>`;
                }
            },

            {
                data: "QtyIn",
                title: "Qty In",
                className: "text-success fw-bold text-center"
            },

            {
                data: "QtyOut",
                title: "Qty Out",
                className: "text-danger fw-bold text-center"
            },

            {
                data: "TransactionDate2",
                title: "Date"
            },

            {
                data: "ReferenceTable",
                title: "Source"
            },

            {
                data: "ReferenceId",
                title: "ReferenceNo",
                render: function (d) {
                    return '#' + d;

                }
            },
            {
                data: "Remarks",
                title: "Remarks",
                render: function (d) {

                    return d ?? "-";

                }
            }

        ],
        pageLength: 10000000,
        ordering: true,
        searching: true,
        lengthChange: false,
        stateSave: false
    });
}


/* ================================
   DURATION FILTER
================================ */

$(document).on("click", ".ledger-duration", function (e) {

    e.preventDefault();

    $(".ledger-duration").removeClass("active");

    $(this).addClass("active");

    selectedDuration = $(this).data("value");

    $("#ledgerDurationDropdown").text("Filter : " + $(this).text());

    if (selectedDuration !== "Custom") {

        $("#filterFromDate").val("");

        $("#filterToDate").val("");

        fetchLedgerData();

    }

});


/* ================================
   AUTO REFRESH WHEN FILTER CHANGE
================================ */

$("#filterAsset, #filterTransactionType, #filterReferenceTable, #filterReversal").change(function () {
        fetchLedgerData();
    });


/* ================================
   DATE CHANGE (CUSTOM RANGE)
================================ */

$("#filterFromDate, #filterToDate").change(function () {

    if ($("#filterFromDate").val() && $("#filterToDate").val()) {

        selectedDuration = "Custom";

        fetchLedgerData();

    }

});


/* ================================
   RESET FILTERS
================================ */

$("#btnResetLedger").click(function () {

    $("#filterAsset").val(0);

    $("#filterTransactionType").val("");

    $("#filterReferenceTable").val("");

    $("#filterReversal").val("");

    $("#filterFromDate").val("");

    $("#filterToDate").val("");

    selectedDuration = "ThisMonth";

    $("#ledgerDurationDropdown").text("Filter : This Month");

    $(".ledger-duration").removeClass("active");

    $(".ledger-duration[data-value='ThisMonth']").addClass("active");

    fetchLedgerData();

});