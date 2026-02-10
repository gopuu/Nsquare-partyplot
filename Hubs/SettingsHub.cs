using Microsoft.AspNetCore.SignalR;

public class SettingsHub : Hub
{
    public async Task NotifySettingsChanged()
    {
        await Clients.All.SendAsync("OnSettingsChanged");
    }
}

//namespace ERPAdmin.Hubs
//{
//    public class SettingsHub
//    {
//    }
//}
