using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TheCodeArchitect.AspNetCore.RealTime.Controllers
{
    [Route("/ws")]
    public class WebSocketsController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBitcoinRatesService _bitcoinRatesService;

        public WebSocketsController(
            IBitcoinRatesService bitcoinRatesService, 
            IHttpContextAccessor httpContextAccessor)
        {
            _bitcoinRatesService = bitcoinRatesService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task Get()
        {
            var webSocketManager = _httpContextAccessor.HttpContext.WebSockets;
            if (webSocketManager.IsWebSocketRequest == false)
            {
                return;
            }
            var webSocket = await webSocketManager.AcceptWebSocketAsync();
            var connectionOpen = true;
            while (connectionOpen)
            {
                (var connectionClosed, var message) = await ReceiveMessage(webSocket);
                connectionOpen = connectionClosed == false;
                if (message == "REQUEST_RATE_BTCEUR")
                {
                    var btcEuroRate = await _bitcoinRatesService.RetrieveBtcEuroRate(CancellationToken.None);
                    var responseMessage = $"{DateTime.UtcNow} BTC = {btcEuroRate} EUR";
                    var responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private static async Task<(bool ConnectionClosed, string Message)> ReceiveMessage(WebSocket webSocket)
        {
            var receiveBuffer = new ArraySegment<byte>(new byte[1024 * 4]);
            var receiveResult = await webSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);
            var connectionClosed = receiveResult.CloseStatus.HasValue;
            var message = receiveResult.MessageType.ToString();
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                message = Encoding.UTF8.GetString(receiveBuffer.Array, 0, receiveResult.Count);
            }
            return (connectionClosed, message);
        }
    }
}