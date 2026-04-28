using Altinn.Dan.Plugin.Nsg.Config;
using Altinn.Dan.Plugin.Nsg.Models;
using Azure;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.Dan.Plugin.Nsg.Clients
{
    public class HvdClient
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        private readonly ApplicationSettings _settings;

        public HvdClient(HttpClient client, ILoggerFactory loggerFactory, IOptions<ApplicationSettings> settings)
        {
            _client = client;
            _logger = loggerFactory.CreateLogger<NSGv1>();
            _settings = settings.Value;
        }

        [Function("IsAlive")]
        public async Task<HttpStatusCode> CheckIsAlive([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req)
        {
            var token = await GenerateToken();
            var accestoken = token.AccessToken;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accestoken);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            var url = $"{_settings.HvdBaseUrl}isalive";

            try
            {
                var response = await _client.GetAsync(url);
                return response.StatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error in CheckIsAlive: {ErrorMessage}", ex.Message);
                var errorResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Error: {ex.Message}")
                };
                return HttpStatusCode.BadRequest;
            }
        }

        [Function("Organisationer")]
        public async Task<HttpResponseData> GetOrganisations([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
        {
            var token = await GenerateToken();
            var accestoken = token.AccessToken;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accestoken);

            var url = $"{_settings.HvdBaseUrl}organisationer";

            var requestBody = new StreamReader(req.Body).ReadToEndAsync();
            var evidenceRequest = JsonConvert.DeserializeObject<OrganisationerRequest>(await requestBody);

            var json = JsonConvert.SerializeObject(evidenceRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                var httpResponse = req.CreateResponse(response.StatusCode);
                await httpResponse.WriteStringAsync(responseBody);
                return httpResponse;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error in GetOrganisations: {ErrorMessage}", ex.Message);
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync($"Error: {ex.Message}");
                return errorResponse;
            }

        }

        private async Task<TokenResponse> GenerateToken()
        {
            var clientId = _settings.HvdClientId;
            var clientSecret = _settings.HvdClientSecret;
            var scope = _settings.HvdScope;

            var url = $"{_settings.HvdTokenUrl}oauth2/token";

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", scope)
            });

            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TokenResponse>(json);
        }

    }
}

