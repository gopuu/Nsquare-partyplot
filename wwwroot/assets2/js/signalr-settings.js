window.AppSignalR = (function () {

    let settingsConnection = null;
    let quotationConnection = null;
    let purchaseConnection = null;
    let walletConnection = null;

    let settingsSubscribers = [];
    let quotationSubscribers = [];
    let purchaseSubscribers = [];
    let walletSubscribers = [];

    function start(companyId = (sessionStorage.getItem("companyid") || 1)) {
        if (!settingsConnection) {
            settingsConnection = new signalR.HubConnectionBuilder()
                .withUrl("/settingsHub")
                .withAutomaticReconnect()
                .build();

            settingsConnection.on("OnSettingsChanged", function (data) {
                //console.log("🔔 Settings changed", data);

                settingsSubscribers.forEach(fn => {
                    try { fn(data); } catch (e) { console.error(e); }
                });
            });

            settingsConnection.start()
                //.then(() => console.log("✅ SettingsHub Connected"))
                .catch(err => console.error("SettingsHub Error:", err));
        }

        if (!quotationConnection) {
            quotationConnection = new signalR.HubConnectionBuilder()
                .withUrl("/quotationHub")
                .withAutomaticReconnect()
                .build();

            quotationConnection.on("OnQuotationChanged", function (data) {
                //console.log("🔔 Quotation changed", data);

                quotationSubscribers.forEach(fn => {
                    try { fn(data); } catch (e) { console.error(e); }
                });
            });

            quotationConnection.start()
                //.then(() => console.log("✅ QuotationHub Connected"))
                .catch(err => console.error("QuotationHub Error:", err));
        }

        if (!purchaseConnection) {
            purchaseConnection = new signalR.HubConnectionBuilder()
                .withUrl("/purchaseHub")
                .withAutomaticReconnect()
                .build();

            purchaseConnection.on("OnPurchaseChanged", function (data) {
                //console.log("🔔 Purchase changed", data);

                purchaseSubscribers.forEach(fn => {
                    try { fn(data); } catch (e) { console.error(e); }
                });
            });

            purchaseConnection.start()
                //.then(() => console.log("✅ Purchase Connected"))
                .catch(err => console.error("PurchaseHub Error:", err));
        }

        if (!walletConnection) {
            walletConnection = new signalR.HubConnectionBuilder()
                .withUrl("/walletHub")
                .withAutomaticReconnect()
                .build();

            walletConnection.on("OnWalletChanged", function (data) {

                // 🔥 Update UI subscribers
                walletSubscribers.forEach(fn => {
                    try { fn(data); } catch (e) { console.error(e); }
                });
                /*
                // 🔥 SweetAlert Toast
                if (data) {
                    let icon = data.Type === "CREDIT" ? "success" : "warning";
                    let sign = data.Type === "CREDIT" ? "+" : "-";

                    walletToast.fire({
                        icon: icon,
                        title: `${sign} ₹${data.Amount} ${data.Type === "CREDIT" ? "Added" : "Deducted"}`
                    });

                    // Optional: update balance UI directly
                    //if ($("#walletBalance").length) {
                    //    $("#walletBalance").text(data.balance);
                    //}
                }
                */
            });

            walletConnection.start()
                .then(() => {
                    // ✅ Join company group
                    walletConnection.invoke("JoinCompanyGroup", companyId.toString());
                })
                .catch(err => console.error("WalletHub Error:", err));
        }
    }
    function subscribeSettings(fn) {
        if (typeof fn === "function") {
            settingsSubscribers.push(fn);
        }
    }
    function subscribeQuotation(fn) {
        if (typeof fn === "function") {
            quotationSubscribers.push(fn);
        }
    }
    function subscribePurchase(fn) {
        if (typeof fn === "function") {
            purchaseSubscribers.push(fn);
        }
    }
    function subscribeWallet(fn) {
        if (typeof fn === "function") {
            walletSubscribers.push(fn);
        }
    }

    return {
        start,
        subscribeSettings,
        subscribeQuotation,
        subscribePurchase,
        subscribeWallet
    };

})();