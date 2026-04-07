window.AppSignalR = (function () {

    let settingsConnection = null;
    let quotationConnection = null;
    let purchaseConnection = null;

    let settingsSubscribers = [];
    let quotationSubscribers = [];
    let purchaseSubscribers = [];

    function start() {
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

    return {
        start,
        subscribeSettings,
        subscribeQuotation,
        subscribePurchase
    };

})();