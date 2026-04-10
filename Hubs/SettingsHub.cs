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

public class WalletHub : Hub
{
    public async Task NotifyWalletChanged(object data)
    {
        await Clients.All.SendAsync("OnWalletChanged", data);
    }

    public async Task JoinCompanyGroup(string companyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, companyId);
    }

    public async Task LeaveCompanyGroup(string companyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, companyId);
    }
}

