using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TheCodeArchitect.AspNetCore.RealTime
{
    public interface IBitcoinRatesService
    {
        Task<decimal> RetrieveBtcEuroRate(CancellationToken cancellationToken);
    }

    public class BitcoinRatesService : IBitcoinRatesService
    {
        private readonly HttpClient _httpClient;

        public BitcoinRatesService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<decimal> RetrieveBtcEuroRate(CancellationToken cancellationToken)
        {
            var btcRatesResponse = await _httpClient.GetAsync("https://blockchain.info/ticker", cancellationToken);
            var btcRates = await btcRatesResponse.Content.ReadAsStringAsync();
            return
                JObject.Parse(btcRates)
                    .Children()
                    .First(c => c.Path == "EUR")
                    .First.Value<decimal>("buy");
        }
    }
}
