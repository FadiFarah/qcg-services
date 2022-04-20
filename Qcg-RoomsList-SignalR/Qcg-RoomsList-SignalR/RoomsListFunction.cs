using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace Qcg_RoomsList_SignalR
{
    public class RoomsListFunction
    {
        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "roomsListHub")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }


        [FunctionName("roomsListUpdated")]
        public async Task RoomsListUpdated([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [SignalR(HubName = "roomsListHub")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger logger)
        {
            logger.LogInformation("Rooms list has been updated!");

            await signalRMessages.AddAsync(new SignalRMessage
            {
                Target = "roomsListUpdated",
                Arguments = new[] { "" },
            });
        }
    }
}
