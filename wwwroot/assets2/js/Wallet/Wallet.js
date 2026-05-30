
// 🔷 Init
$(document).ready(function () {
    fetchLedger();
    initRecharge();

    AppSignalR.subscribeWallet(function (data) {
        //console.log("Wallet Updated (ledger):", data);
        //Reload ledger
        fetchLedger();
    });
});

let companyId = Number(sessionStorage.getItem("companyid") || 0);

// RECHARGE START --------------------------------------------
// 🔷 2. Get Ledger
function fetchLedger() {
    var payload = {
        CompanyId: Number(companyId)
    };
    //console.log("payload ledger" + cnt++ + " :" , payload);
    $.ajax({
        url: "/api/Wallet/GetWalletLedger",
        type: "POST",
        data: JSON.stringify(payload),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            //console.log("response ledger:", response);
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
            {
                title: "Type",
                data: "TransactionType",
                className: "text-center",
                render: function (data) {
                    const txnType = (data || "").toLowerCase();
                    if (txnType === "credit") {
                        return '<span class="badge bg-success">Credit</span>';
                    } else if (txnType === "debit") {
                        return '<span class="badge bg-danger">Debit</span>';
                    }
                    return data;
                }
            },
            {
                title: "Amount",
                data: "Amount",
                className: "text-end", // 👉 Right align
                render: function (data, type, row) {
                    const txnType = (row.TransactionType || "").toLowerCase();

                    const amount = new Intl.NumberFormat('en-IN', {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    }).format(data || 0);

                    if (txnType === "credit") {
                        return '<span class="text-success fw-bold">+ ₹' + amount + '</span>';
                    } else if (txnType === "debit") {
                        return '<span class="text-danger fw-bold">- ₹' + amount + '</span>';
                    }

                    return '₹ ' + amount;
                }
            },
            {
                title: "Balance",
                data: "BalanceAfter",
                className: "text-end", // 👉 Right align
                render: function (data) {
                    const amount = new Intl.NumberFormat('en-IN', {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    }).format(data || 0);

                    return '₹ ' + amount;
                }
            },
            {
                title: "Ref. Type",
                data: "ReferenceType",
                className: "text-start"
            },
            {
                title: "Txn Ref. Id",
                data: "ReferenceId",
                className: "text-start"
            },
            {
                title: "Txn Date",
                data: "CreatedAt",
                className: "text-center"
            }
        ],
        initComplete: function () {
            $(this.api().table().header())
                .find('th')
                .addClass('bg-warning-subtle text-dark');
        },
        responsive: true,
        pageLength: 20,
        paging: false,
        searching: true,
        ordering: false,
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
            title: "💳 Wallet Recharge",
            html: `
                <div class="text-start">
                    <label class="fw-semibold mb-1">Enter Amount</label>
                    <div class="input-group mb-3">
                        <span class="input-group-text">₹</span>
                        <input id="swalAmount" type="number" class="form-control" placeholder="Enter amount" min="1">
                    </div>

                    <div class="d-flex gap-2 flex-wrap">
                        <button class="btn btn-outline-primary btn-sm quick-amt" data-amt="100">₹100</button>
                        <button class="btn btn-outline-primary btn-sm quick-amt" data-amt="500">₹500</button>
                        <button class="btn btn-outline-primary btn-sm quick-amt" data-amt="1000">₹1000</button>
                        <button class="btn btn-outline-primary btn-sm quick-amt" data-amt="2000">₹2000</button>
                    </div>

                    <small class="text-muted d-block mt-2">Minimum amount: ₹100</small>
                </div>
            `,
            confirmButtonText: "Proceed to Pay",
            cancelButtonText: "Cancel",
            showCancelButton: true,
            focusConfirm: false,
            didOpen: () => {
                const input = document.getElementById("swalAmount");
                input.focus();

                // Quick amount buttons
                document.querySelectorAll(".quick-amt").forEach(btn => {
                    btn.addEventListener("click", function () {
                        input.value = this.getAttribute("data-amt");
                        input.focus();
                    });
                });

                // 🔥 ENTER key = confirm
                input.addEventListener("keypress", function (e) {
                    if (e.key === "Enter") {
                        Swal.clickConfirm();
                    }
                });
            },
            preConfirm: () => {
                const value = document.getElementById("swalAmount").value;
                const amount = parseFloat(value);

                if (!amount || amount < 100) {
                    Swal.showValidationMessage("⚠️ Please enter a valid amount (minimum ₹100)");
                    return false;
                }

                return amount;
            }
        }).then((result) => {
            if (!result.isConfirmed) return;

            const amount = result.value;
            startRecharge(amount);
            //DEBUG: Directly verify without payment for testing ledger update on UI
            //verifyPayment({ razorpay_order_id: "1", razorpay_payment_id: "1", razorpay_signature: "1" }, amount)
        });

    });
}
function startRecharge(amount) {
    var payload = {
        CompanyId: companyId,
        Amount: amount
    };
    //console.log("create order payload:", payload);
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
            // ✅ Payment Success
            verifyPayment(response, amount);
        },

        modal: {
            ondismiss: function () {
                // ❌ User closed popup / cancelled
                Swal.fire({
                    icon: "warning",
                    title: "Payment Cancelled",
                    text: "You cancelled the payment."
                });
            }
        },

        theme: { color: "#0d6efd" }
    };

    let rzp = new Razorpay(options);

    // ❌ Payment Failed (bank/server error)
    rzp.on("payment.failed", function (response) {
        console.error("Payment Failed:", response);

        Swal.fire({
            icon: "error",
            title: "Payment Failed",
            text: response.error.description || "Something went wrong"
        });
    });

    rzp.open();
}
function verifyPayment(response, amount) {
    var payload = {
        CompanyId: companyId,
        Amount: amount,
        RazorpayOrderId: response.razorpay_order_id,
        RazorpayPaymentId: response.razorpay_payment_id,
        RazorpaySignature: response.razorpay_signature
    };
    //console.log("VerifyPayment payload:", payload);
    $.ajax({
        url: "/api/Wallet/WalletCredit_VerifyPayment",
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        data: JSON.stringify(payload),
        success: function (res) {
            //handleApiSuccess("Payment successful!");
            //Swal.fire("Success", "Wallet recharged!", "success");
            //fetchBalance();
            //fetchLedger();
        },
        error: function (xhr, status, error) {
            handleApiError(xhr, status, error, "Payment verification failed");
        }
    });
}
// RECHARGE END --------------------------------------------





