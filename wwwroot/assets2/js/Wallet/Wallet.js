
// 🔷 Init
$(document).ready(function () {
    fetchLedger();
    initRecharge();

    AppSignalR.subscribeWallet(function (data) {
        console.log("Wallet Updated:", data);
        //Reload ledger
        fetchLedger();
    });
    //start(companyId); // SignalR start
});
let companyId = sessionStorage.getItem("companyid") || 1;

// 🔷 2. Get Ledger
let cnt = 1;
function fetchLedger() {
    var payload = {
        CompanyId: Number(companyId)
    };
    console.log("payload ledger" + cnt++ + " :" , payload);
    $.ajax({
        url: "/api/Wallet/GetWalletLedger",
        type: "POST",
        data: JSON.stringify(payload),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            console.log("response ledger:", response);
            renderLedger(response.data);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Fetch ledger failed");
        }
    });
}

// 🔷 Render DataTable
function renderLedger(data) {
    if ($.fn.DataTable.isDataTable('#walletTable')) {
        $('#walletTable').DataTable().destroy();
    }

    $('#walletTable').DataTable({
        data: data,
        columns: [
            { title: "Type", data: "TransactionType" },
            { title: "Amount", data: "Amount" },
            { title: "Balance", data: "BalanceAfter" },
            { title: "Ref", data: "ReferenceType" },
            { title: "Date", data: "CreatedAt" }
        ],
        order: [[4, "desc"]],
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },
        responsive: true,
        pageLength: 20,
        paging: false,
        searching: true,
        ordering: true,
        lengthChange: false,

        scrollY: "60vh",          // table body scroll height
        scrollCollapse: true,     // shrink if less rows
        scrollX: true,            // horizontal scroll if needed

    });
}

//# 🔷 3. Recharge(Razorpay Flow)
function initRecharge() {
    $("#btnRecharge").click(function () {

        Swal.fire({
            title: "Enter Amount",
            input: "number",
            inputLabel: "Recharge Amount",
            inputPlaceholder: "Enter amount",
            confirmButtonText: "Proceed",
            showCancelButton: true
        }).then((result) => {

            if (!result.isConfirmed) return;

            let amount = parseFloat(result.value);

            if (!amount || amount <= 0) {
                Swal.fire("Invalid", "Enter valid amount", "warning");
                return;
            }

            startRecharge(amount);
            //openRazorpay(amount);
        });
    });
}
function startRecharge(amount) {
    var payload = {
        CompanyId: companyId,
        Amount: amount
    };
    console.log("create order payload:", payload);
    $.ajax({
        url: "/api/Wallet/WalletCredit_CreateOrder",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(payload),
        dataType: "json",
        success: function (res) {
            let order = res.data;
            openRazorpay(order, amount);
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Order creation failed");
        }
    });
}
function openRazorpay(order, amount) {
    let companyName = sessionStorage.getItem("companyname");

    let options = {
        key: order.Key,
        amount: order.Amount,
        currency: "INR",
        name: companyName,
        description: "Wallet Recharge",
        order_id: order.OrderId,
        handler: function (response) {
            // 🔐 Step: Verify Payment
            verifyPayment(response, amount);
        },
        theme: { color: "#0d6efd" }
    };
    let rzp = new Razorpay(options);
    rzp.open();
}
function verifyPayment(response, amount) {

    $.ajax({
        url: "/api/Wallet/WalletCredit_VerifyPayment",
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        data: JSON.stringify({
            CompanyId: companyId,
            Amount: amount,
            RazorpayOrderId: response.razorpay_order_id,
            RazorpayPaymentId: response.razorpay_payment_id,
            RazorpaySignature: response.razorpay_signature
        }),
        success: function (res) {
            handleApiSuccess("Payment successful!");
            //Swal.fire("Success", "Wallet recharged!", "success");
            //fetchBalance();
            fetchLedger();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Payment verification failed");
        }
    });
}

