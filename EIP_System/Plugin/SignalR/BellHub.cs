using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace EIP_System.Plugin.SignalR
{
    public class BellHub : Hub
    {
        
        public void Bell(int[] id)
        {
            Clients.All.bell(id);
        }
        public void Active()
        {
            var hubContent = GlobalHost.ConnectionManager.GetHubContext<BellHub>();

            hubContent.Clients.All.hello("hello");
        }
    }
}