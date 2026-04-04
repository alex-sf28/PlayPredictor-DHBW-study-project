using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using PlayPredictorWebAPI.Services.External;

namespace PlayPredictorWebAPI.Services.External
{
    public static class HttpClientRegistrationExtensions
    {
        public static void AddExternalHttpClients(this IServiceCollection services)
        {
            static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
            {
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            }

            services
                .AddHttpClient<IGoogleApiClient, GoogleApiClient>(client =>
                {
                    client.BaseAddress = new Uri("https://oauth2.googleapis.com/");
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());

            services
                .AddHttpClient<IFaceitApiClient, FaceitApiClient>(client =>
                {
                    client.BaseAddress = new Uri("https://open.faceit.com/data/v4/");
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy());
        }
    }
}