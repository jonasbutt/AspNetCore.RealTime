using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TheCodeArchitect.AspNetCore.RealTime.Controllers
{
    [Route("/sse")]
    public class ServerSentEventsController
    {
        private readonly IResponseStreamer _responseStreamer;
        private readonly IBitcoinRatesService _bitcoinRatesService;

        public ServerSentEventsController(
            IResponseStreamer responseStreamer, 
            IBitcoinRatesService bitcoinRatesService)
        {
            _responseStreamer = responseStreamer;
            _bitcoinRatesService = bitcoinRatesService;
        }

        [HttpGet]
        public async Task Get()
        {
            await _responseStreamer.StreamResponse(
                async (write, cancellationToken) =>
                {
                    while (true)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        var btcEuroRate = await _bitcoinRatesService.RetrieveBtcEuroRate(cancellationToken);
                        await write($"{DateTime.UtcNow} BTC = {btcEuroRate} EUR");
                        await Task.Delay(2000, cancellationToken);
                    }
                });
        }
    }
}
