using CmsShoppingCart.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;

namespace CmsShoppingCart.Services
{
    public class PayPalService
    {
        private readonly PayPalSettings _settings;
        private readonly IHttpClientFactory _clientFactory;

        public PayPalService(IOptions<PayPalSettings> settings, IHttpClientFactory clientFactory)
        {
            _settings = settings.Value;
            _clientFactory = clientFactory;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var client = _clientFactory.CreateClient();
            var authToken = Encoding.ASCII.GetBytes($"{_settings.ClientId}:{_settings.Secret}");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

            var data = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await client.PostAsync($"{_settings.BaseUrl}/v1/oauth2/token", data);

            if (!response.IsSuccessStatusCode)
            {
                // Manejo de error
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al obtener el token de acceso: {errorContent}");
            }

            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<dynamic>(result);

            return json.access_token;
        }
    }
}
