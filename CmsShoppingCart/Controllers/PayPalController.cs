using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using CmsShoppingCart.Models;
using CmsShoppingCart.Services;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace CmsShoppingCart.Controllers
{
    public class PayPalController : Controller
    {
        private readonly PayPalService _paypal;

        public PayPalController(PayPalService paypal)
        {
            _paypal = paypal;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearOrden()
        {
            string token;
            try
            {
                // Obtener el token de acceso de PayPal
                token = await _paypal.GetAccessTokenAsync();
            }
            catch (Exception ex)
            {
                // Manejar excepciones si no se pudo obtener el token
                return Content("Error al obtener el token de acceso: " + ex.Message);
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = "MXN",
                            value = "100.00"
                        }
                    }
                },
                application_context = new
                {
                    return_url = "https://localhost:44375/PayPal/Captura",
                    cancel_url = "https://localhost:44375/PayPal/Cancelado"
                }
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                // Enviar la solicitud para crear la orden
                response = await client.PostAsync("https://api-m.sandbox.paypal.com/v2/checkout/orders", content);
            }
            catch (Exception ex)
            {
                return Content("Error al crear la orden: " + ex.Message);
            }

            // Validación de la respuesta
            if (!response.IsSuccessStatusCode)
            {
                // Si la respuesta no fue exitosa, devuelve el error
                return Content("Error al crear orden: " + await response.Content.ReadAsStringAsync());
            }

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
            string approveLink = "";

            foreach (var link in jsonResponse.links)
            {
                if (link.rel == "approve")
                {
                    approveLink = link.href;
                    break;
                }
            }

            if (string.IsNullOrEmpty(approveLink))
            {
                return Content("No se encontró el link de aprobación.");
            }

            // Redirigir a la URL de aprobación de PayPal
            return Redirect(approveLink);
        }

        [HttpGet]
        public async Task<IActionResult> Captura(string token)
        {
            string accessToken;
            try
            {
                accessToken = await _paypal.GetAccessTokenAsync();
            }
            catch (Exception ex)
            {
                return Content("Error al obtener el token de acceso: " + ex.Message);
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response;
            try
            {
                // Enviar la solicitud para capturar el pago
                var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
                response = await client.PostAsync($"https://api-m.sandbox.paypal.com/v2/checkout/orders/{token}/capture", emptyJson);
            }
            catch (Exception ex)
            {
                return Content("Error al capturar el pago: " + ex.Message);
            }

            var content = await response.Content.ReadAsStringAsync();

            // Validar si la captura fue exitosa
            if (!response.IsSuccessStatusCode)
            {
                return Content("Error al capturar pago: " + content);
            }

            // Extraer datos útiles de la respuesta
            dynamic result = JsonConvert.DeserializeObject(content);
            string nombre = result.payer.name.given_name;
            string email = result.payer.email_address;
            string idTransaccion = result.purchase_units[0].payments.captures[0].id;

            // Mostrar la información de la transacción
            return Content($"Pago capturado correctamente. Nombre: {nombre}, Email: {email}, ID Transacción: {idTransaccion}", "application/json");
        }

        public IActionResult Cancelado()
        {
            return Content("El pago fue cancelado.");
        }
    }
}