using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Qcg_Game_SignalR.Models;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace Qcg_Game_SignalR
{
    public class GameFunction
    {
        private class SignalRConnectionData
        {
            [JsonPropertyName("timestamp")]
            public string Timestamp { get; set; }
            [JsonPropertyName("hubName")]
            public string hubName { get; set; }
            [JsonPropertyName("connectionId")]
            public string ConnectionId { get; set; }
            [JsonPropertyName("errorMessage")]
            public string ErrorMessage { get; set; }
        }

        private class Body
        {
            [JsonPropertyName("roomId")]
            public string RoomId { get; set; }

            [JsonPropertyName("fullName")]
            public string FullName { get; set; }

            [JsonPropertyName("chatMessage")]
            public string ChatMessage { get; set; }

            [JsonPropertyName("userId")]
            public string UserId { get; set; }

            [JsonPropertyName("connectionId")]
            public string ConnectionId { get; set; }

            [JsonPropertyName("categoryGroup")]
            public string CategoryGroup { get; set; }

            [JsonPropertyName("cardName")]
            public string CardName { get; set; }

            [JsonPropertyName("fromPlayerUserId")]
            public string FromPlayerUserId { get; set; }

            [JsonPropertyName("toPlayerUserId")]
            public string ToPlayerUserId { get; set; }

            [JsonPropertyName("card")]
            public Card Card { get; set; }
        }

        private class Response<T>
        {
            [JsonPropertyName("status")]
            public int Status { get; set; }

            [JsonPropertyName("data")]
            public T Data { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }
        }

        private class Client
        {
            [JsonPropertyName("connectionId")]
            public string ConnectionId { get; set; }

            [JsonPropertyName("userId")]
            public string UserId { get; set; }
        }

        static Dictionary<string, List<string>> OnlineClientsInGroups = new Dictionary<string, List<string>>();
        static List<Client> ConnectedClients = new List<Client>();
        static HttpClient httpClient = new HttpClient();

        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "gameHub")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName("RoomConnectionsTrigger")]
        public async Task Run([EventGridTrigger] EventGridEvent eventGridEvent, [SignalR(HubName = "gameHub")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            JObject eventData = (JObject)eventGridEvent.Data;
            log.LogInformation(eventData.ToString());
            SignalRConnectionData data = eventData.ToObject<SignalRConnectionData>();
            log.LogInformation(eventGridEvent.ToString());
            log.LogInformation(eventGridEvent.Data.ToString());
            log.LogInformation(data.ConnectionId);
            if (eventGridEvent.EventType == "Microsoft.SignalRService.ClientConnectionConnected")
            {
                log.LogInformation("User connected");
                await signalRMessages.AddAsync(new SignalRMessage
                {
                    Target = "UserConnected",
                    Arguments = new[] { data.ConnectionId },
                });
            }
            else
            {
                Client client = new Client();
                var roomId = "";
                foreach (var item in OnlineClientsInGroups)
                {
                    if (item.Value.Contains(data.ConnectionId))
                    {
                        Response<Room> getRoomResponse = await httpClient.GetFromJsonAsync<Response<Room>>("https://fast-mesa-26421.herokuapp.com/room/" + item.Key);
                        Room room = getRoomResponse.Data;
                        if (room != null)
                        {
                            client = ConnectedClients.Find(client => client.ConnectionId == data.ConnectionId);
                            if (client != null)
                            {
                                var itemToRemove = room.Players?.FirstOrDefault(player => player.UserId == client.UserId);
                                room.Players?.Remove(itemToRemove);
                                var body = JsonConvert.SerializeObject(room);
                                log.LogInformation(body);
                                var requestContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                                if (room.Players.Count > 0)
                                    await httpClient.PutAsync("https://fast-mesa-26421.herokuapp.com/room/" + item.Key, requestContent);
                                else
                                    await httpClient.DeleteAsync("https://fast-mesa-26421.herokuapp.com/room/" + item.Key);
                            }
                            item.Value.Remove(data.ConnectionId);
                            roomId = item.Key;
                            break;
                        }
                    }
                }
                await signalRMessages.AddAsync(new SignalRMessage
                {
                    Target = "UserDisconnected",
                    Arguments = new[] { client.ConnectionId, client.UserId },
                    GroupName = roomId
                });
            }
            log.LogInformation(eventGridEvent.EventType);
            log.LogInformation(eventGridEvent.Data.ToString());
        }

        [FunctionName("addtogroup")]
        public async Task AddToGroup([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [SignalR(HubName = "gameHub")]
        IAsyncCollector<SignalRGroupAction> signalRGroupActions, ILogger logger)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            logger.LogInformation("request body: " + requestBody);
            Body bodyDetails = JsonConvert.DeserializeObject<Body>(requestBody);
            logger.LogInformation("body RoomId " + bodyDetails.RoomId);
            logger.LogInformation("body ConnectionId " + bodyDetails.ConnectionId);
            logger.LogInformation("body UserId " + bodyDetails.UserId);
            ConnectedClients.Add(new Client { UserId = bodyDetails?.UserId, ConnectionId = bodyDetails?.ConnectionId });
            logger.LogInformation("ConnectedClients[0].UserId "+ConnectedClients[0].UserId);

            if (!httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                StringValues value;
                if (req.Headers.TryGetValue("Authorization", out value))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", value.ToString());
                }

            }
            Response<Room> getRoomResponse = await httpClient.GetFromJsonAsync<Response<Room>>("https://fast-mesa-26421.herokuapp.com/room/" + bodyDetails.RoomId);
            Room room = getRoomResponse.Data;
            if (room != null)
            {
                Response<User> getUserResponse = await httpClient.GetFromJsonAsync<Response<User>>("https://fast-mesa-26421.herokuapp.com/user/" + bodyDetails.UserId);
                User user = getUserResponse.Data;
                room.Players.Add(new Player { 
                    UserId = bodyDetails.UserId,
                    Cards = new List<Card>(),
                    FullName = user.FirstName + " " + user.LastName,
                    Picture = user.Picture,
                    IsMaster = room.Players?.Count == 0 ? true : false,
                    IsReady = false,
                    IsTurn = false,
                    IsWin = false,
                    Points = 0
                });
                var body = JsonConvert.SerializeObject(room);
                var requestContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                await httpClient.PutAsync("https://fast-mesa-26421.herokuapp.com/room/" + bodyDetails.RoomId, requestContent);
            }
            if (!OnlineClientsInGroups.ContainsKey(bodyDetails.RoomId))
                OnlineClientsInGroups[bodyDetails.RoomId] = new List<string>();
            OnlineClientsInGroups[bodyDetails.RoomId].Add(bodyDetails.ConnectionId);

            await signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    ConnectionId = bodyDetails.ConnectionId,
                    GroupName = bodyDetails.RoomId,
                    Action = GroupAction.Add,
                });

        }

        [FunctionName("gameDataUpdated")]
        public async Task GameDataUpdated([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [SignalR(HubName = "gameHub")]
        IAsyncCollector<SignalRMessage> signalRMessages, ILogger logger)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Body bodyDetails = JsonConvert.DeserializeObject<Body>(requestBody);

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "gameDataUpdated",
                    Arguments = new[] { bodyDetails.RoomId },
                    GroupName = bodyDetails.RoomId,
                });
        }

        [FunctionName("gameStarted")]
        public async Task GameStarted([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [SignalR(HubName = "gameHub")]
        IAsyncCollector<SignalRMessage> signalRMessages, ILogger logger)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Body bodyDetails = JsonConvert.DeserializeObject<Body>(requestBody);

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "gameStarted",
                    Arguments = new[] { "" },
                    GroupName = bodyDetails.RoomId,
                });
        }

        [FunctionName("newChatMessage")]
        public async Task newChatMessage([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [SignalR(HubName = "gameHub")]
        IAsyncCollector<SignalRMessage> signalRMessages, ILogger logger)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Body bodyDetails = JsonConvert.DeserializeObject<Body>(requestBody);

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "newChatMessage",
                    Arguments = new[] { bodyDetails.FullName, bodyDetails.ChatMessage },
                    GroupName = bodyDetails.RoomId,
                });
        }

        [FunctionName("cardRequestNotify")]
        public async Task cardRequestNotify([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [SignalR(HubName = "gameHub")]
        IAsyncCollector<SignalRMessage> signalRMessages, ILogger logger)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Body bodyDetails = JsonConvert.DeserializeObject<Body>(requestBody);
            var card = JsonConvert.SerializeObject(new { card = bodyDetails.Card });

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "cardRequestNotify",
                    Arguments = new[] { bodyDetails.ConnectionId, bodyDetails.FromPlayerUserId, bodyDetails.ToPlayerUserId, card },
                    GroupName = bodyDetails.RoomId,
                });
        }


        [FunctionName("cardRequestFromPlayer")]
        public async Task CardRequestFromPlayer([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [SignalR(HubName = "gameHub")]
        IAsyncCollector<SignalRMessage> signalRMessages, ILogger logger)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Body bodyDetails = JsonConvert.DeserializeObject<Body>(requestBody);

            string cardFound = "0";

            Response<Room> getRoomResponse = await httpClient.GetFromJsonAsync<Response<Room>>("https://fast-mesa-26421.herokuapp.com/room/" + bodyDetails.RoomId);
            Room room = getRoomResponse.Data;

            Player toPlayer = room.Players.Find(p => p.UserId == bodyDetails.ToPlayerUserId);

            Player fromPlayer = room.Players.Find(p => p.UserId == bodyDetails.FromPlayerUserId);

            if (toPlayer != null && fromPlayer != null)
            {
                Card card = toPlayer.Cards.Find(c => c.CategoryGroup == bodyDetails.CategoryGroup && c.CardName == bodyDetails.CardName);

                if (card != null)
                {
                    toPlayer.Cards.Remove(card);

                    fromPlayer.Cards.Add(card);

                    fromPlayer.IsTurn = true;

                    cardFound = "1";
                }

                else
                {
                    fromPlayer.IsTurn = false;
                    toPlayer.IsTurn = true;
                }

                var indexOfFromPlayer = room.Players.IndexOf(fromPlayer);
                var indexOfToPlayer = room.Players.IndexOf(toPlayer);

                logger.LogInformation(indexOfToPlayer.ToString());
                logger.LogInformation(indexOfFromPlayer.ToString());
                room.Players[indexOfFromPlayer] = fromPlayer;
                room.Players[indexOfToPlayer] = toPlayer;
                logger.LogInformation(room.ToString());

                var body = JsonConvert.SerializeObject(room);
                var requestContent = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                await httpClient.PutAsync("https://fast-mesa-26421.herokuapp.com/room/" + bodyDetails.RoomId, requestContent);
            }

            await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "playersCardsUpdated",
                    Arguments = new[] { bodyDetails.RoomId, bodyDetails.CategoryGroup, bodyDetails.CardName, bodyDetails.FromPlayerUserId, bodyDetails.ToPlayerUserId, cardFound },
                    GroupName = bodyDetails.RoomId,
                });
        }
    }
}
