window.AppSignalR = (function () {
    let connection = null;
    let subscribers = [];
    function start() {
        if (connection) return;

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/settingsHub")
            .withAutomaticReconnect()
            .build();

        connection.on("OnSettingsChanged", () => {
            console.log("🔔 Settings changed");
            subscribers.forEach(fn => {
                try { fn(); } catch (e) { console.error(e); }
            });
        });

        //connection.start()
        //    .then(() => console.log("✅ SignalR connected"))
        //    .catch(err => console.error("❌ SignalR error:", err));

        connection.start()
            .catch(err => {
                handleApiError(null, "SignalRError", err, "SignalR connection failed");
            });

    }

    function subscribe(fn) {
        if (typeof fn === "function") {
            subscribers.push(fn);
        }
    }

    return {
        start,
        subscribe
    };

})();
