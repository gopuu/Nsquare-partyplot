using Microsoft.AspNetCore.SignalR;

public class SettingsHub : Hub
{
    public async Task NotifySettingsChanged()
    {
        await Clients.All.SendAsync("OnSettingsChanged");
    }
}
public class QuotationHub : Hub
{
    public async Task NotifyQuotationChanged()
    {
        await Clients.All.SendAsync("OnQuotationChanged");
    }
}
public class PurchaseHub : Hub
{
    public async Task NotifyPurchaseChanged()
    {
        await Clients.All.SendAsync("OnPurchaseChanged");
    }
}
