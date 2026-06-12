using Altinn.Dan.Plugin.Nsg.Config;
using Altinn.Dan.Plugin.Nsg.Exceptions;
using Altinn.Dan.Plugin.Nsg.Models;
using Dan.Common.Interfaces;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Altinn.Dan.Plugin.Nsg.Test
{
    [TestClass]
    public class TokenCachingTests
    {
        private MockHttpMessageHandler _mockHandler = null!;
        private HttpClient _httpClient = null!;
        private TestTokenCacheProvider _cache = null!;
        private ApplicationSettings _settings = null!;
        private NSGv1 _sut = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_mockHandler);
            _cache = new TestTokenCacheProvider();

            _settings = new ApplicationSettings
            {
                TokenCaching = true,
                ClientIdSE = "nsgb-client",
                ClientSecretSE = "nsgb-secret",
                ScopeSE = "nsgb-scope",
                TokenUrlSE = "https://test.example/nsgb/oauth2/token",
                HvdClientId = "vdm-client",
                HvdClientSecret = "vdm-secret",
                HvdScope = "vdm-scope",
                HvdTokenUrl = "https://test.example/vdm/"
            };

            var httpClientFactory = A.Fake<IHttpClientFactory>();
            A.CallTo(() => httpClientFactory.CreateClient(A<string>._)).Returns(_httpClient);

            var entityRegistry = A.Fake<IEntityRegistryService>();

            _sut = new NSGv1(
                httpClientFactory,
                entityRegistry,
                Options.Create(_settings),
                NullLoggerFactory.Instance,
                _cache);
        }

        // ----- GetTokenSE (NSGB) -----

        [TestMethod]
        public async Task GetTokenSE_WhenCacheHit_ReturnsCachedTokenWithoutHttpCall()
        {
            _cache.Storage["TokenSE"] = new TokenResponse { AccessToken = "cached-nsgb", ExpiresIn = 3600 };

            var result = await _sut.GetTokenSE(useCache: true);

            Assert.AreEqual("cached-nsgb", result.AccessToken);
            Assert.AreEqual(0, _mockHandler.InvocationCount, "No HTTP call should be made on cache hit");
            Assert.AreEqual(0, _cache.SetCount, "No write to cache when serving from cache");
        }

        [TestMethod]
        public async Task GetTokenSE_WhenCacheMiss_FetchesTokenAndWritesToCache()
        {
            _mockHandler.EnqueueJsonResponse(HttpStatusCode.OK,
                "{\"access_token\":\"fresh-nsgb\",\"expires_in\":3600}");

            var result = await _sut.GetTokenSE(useCache: true);

            Assert.AreEqual("fresh-nsgb", result.AccessToken);
            Assert.AreEqual(1, _mockHandler.InvocationCount);
            Assert.AreEqual(1, _cache.SetCount);
            Assert.IsTrue(_cache.Storage.ContainsKey("TokenSE"));
            Assert.AreEqual("fresh-nsgb", _cache.Storage["TokenSE"].AccessToken);
        }

        [TestMethod]
        public async Task GetTokenSE_WhenTokenCachingDisabled_NeverReadsOrWritesCache()
        {
            _settings.TokenCaching = false;
            _mockHandler.EnqueueJsonResponse(HttpStatusCode.OK,
                "{\"access_token\":\"fresh-nsgb\",\"expires_in\":3600}");

            var result = await _sut.GetTokenSE(useCache: true);

            Assert.AreEqual("fresh-nsgb", result.AccessToken);
            Assert.AreEqual(0, _cache.TryGetCount, "Should not read cache when TokenCaching=false");
            Assert.AreEqual(0, _cache.SetCount, "Should not write cache when TokenCaching=false");
        }

        [TestMethod]
        public async Task GetTokenSE_WhenUseCacheFalse_SkipsBothCacheReadAndWrite()
        {
            // GetTokenSE har symmetrisk cache-logikk: når useCache=false hopper vi
            // over BÅDE lesing og skriving. Den stale oppføringen blir derfor stående.
            _cache.Storage["TokenSE"] = new TokenResponse { AccessToken = "stale", ExpiresIn = 3600 };
            _mockHandler.EnqueueJsonResponse(HttpStatusCode.OK,
                "{\"access_token\":\"fresh\",\"expires_in\":3600}");

            var result = await _sut.GetTokenSE(useCache: false);

            Assert.AreEqual("fresh", result.AccessToken, "Returns the freshly fetched token");
            Assert.AreEqual(0, _cache.TryGetCount, "useCache=false skips cache read");
            Assert.AreEqual(0, _cache.SetCount, "useCache=false also skips cache write (symmetric)");
            Assert.AreEqual("stale", _cache.Storage["TokenSE"].AccessToken, "Existing cache entry remains untouched");
        }

        [TestMethod]
        public async Task GetTokenSE_WhenServerReturns401_ThrowsAuthenticationException()
        {
            _mockHandler.EnqueueJsonResponse(HttpStatusCode.Unauthorized, "{\"error\":\"invalid_client\"}");

            await Assert.ThrowsExactlyAsync<NsgException>(
                async () => await _sut.GetTokenSE(useCache: false));

            Assert.AreEqual(0, _cache.SetCount, "Failed token responses must not be cached");
        }

        // ----- GenerateTokenSE (VDM) -----

        [TestMethod]
        public async Task GenerateTokenSE_UsesDifferentCacheKeyThanNSGB()
        {
            _mockHandler.EnqueueJsonResponse(HttpStatusCode.OK,
                "{\"access_token\":\"vdm-token\",\"expires_in\":3600}");

            await _sut.GenerateTokenSE(useCache: true);

            Assert.IsTrue(_cache.Storage.ContainsKey("TokenVdmSE"), "VDM should cache under TokenVdmSE");
            Assert.IsFalse(_cache.Storage.ContainsKey("TokenSE"), "VDM must NOT collide with NSGB's TokenSE key");
        }

        [TestMethod]
        public async Task GenerateTokenSE_WhenCacheHit_ReturnsCachedTokenWithoutHttpCall()
        {
            _cache.Storage["TokenVdmSE"] = new TokenResponse { AccessToken = "cached-vdm", ExpiresIn = 3600 };

            var result = await _sut.GenerateTokenSE(useCache: true);

            Assert.AreEqual("cached-vdm", result.AccessToken);
            Assert.AreEqual(0, _mockHandler.InvocationCount);
            Assert.AreEqual(0, _cache.SetCount);
        }

        [TestMethod]
        public async Task GenerateTokenSE_WhenCacheMiss_FetchesAndCaches()
        {
            _mockHandler.EnqueueJsonResponse(HttpStatusCode.OK,
                "{\"access_token\":\"fresh-vdm\",\"expires_in\":1800}");

            var result = await _sut.GenerateTokenSE(useCache: true);

            Assert.AreEqual("fresh-vdm", result.AccessToken);
            Assert.AreEqual(1, _mockHandler.InvocationCount);
            Assert.AreEqual(1, _cache.SetCount);
            Assert.AreEqual("fresh-vdm", _cache.Storage["TokenVdmSE"].AccessToken);
        }

        [TestMethod]
        public async Task GenerateTokenSE_WhenTokenCachingDisabled_NeverReadsOrWritesCache()
        {
            _settings.TokenCaching = false;
            _mockHandler.EnqueueJsonResponse(HttpStatusCode.OK,
                "{\"access_token\":\"fresh-vdm\",\"expires_in\":3600}");

            await _sut.GenerateTokenSE(useCache: true);

            Assert.AreEqual(0, _cache.TryGetCount);
            Assert.AreEqual(0, _cache.SetCount);
        }

        [TestMethod]
        public async Task GenerateTokenSE_DefaultParameterDoesNotReadFromCache()
        {
            // Standardverdien er useCache=false, så caching skal hoppes over selv om TokenCaching=true
            _cache.Storage["TokenVdmSE"] = new TokenResponse { AccessToken = "cached-vdm", ExpiresIn = 3600 };
            _mockHandler.EnqueueJsonResponse(HttpStatusCode.OK,
                "{\"access_token\":\"fresh-vdm\",\"expires_in\":3600}");

            var result = await _sut.GenerateTokenSE();

            Assert.AreEqual("fresh-vdm", result.AccessToken, "Default arg should bypass cache read");
            Assert.AreEqual(0, _cache.TryGetCount);
        }
    }

    // --- Test doubles ---

    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();
        public List<HttpRequestMessage> ReceivedRequests { get; } = new();
        public int InvocationCount => ReceivedRequests.Count;

        public void EnqueueJsonResponse(HttpStatusCode status, string json)
        {
            _responses.Enqueue(new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ReceivedRequests.Add(request);
            if (_responses.Count == 0)
                throw new InvalidOperationException(
                    $"No queued HTTP response for request to {request.RequestUri}");
            return Task.FromResult(_responses.Dequeue());
        }
    }

    internal class TestTokenCacheProvider : ITokenCacheProvider
    {
        public Dictionary<string, TokenResponse> Storage { get; } = new();
        public int TryGetCount { get; private set; }
        public int SetCount { get; private set; }

        public Task<(bool success, TokenResponse result)> TryGetToken(string key)
        {
            TryGetCount++;
            if (Storage.TryGetValue(key, out var token))
                return Task.FromResult((true, token));
            return Task.FromResult<(bool, TokenResponse)>((false, null!));
        }

        public Task Set(string key, TokenResponse value, TimeSpan timeToLive)
        {
            SetCount++;
            Storage[key] = value;
            return Task.CompletedTask;
        }
    }
}
