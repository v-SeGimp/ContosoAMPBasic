﻿namespace Dashboard.Controllers
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;

    using SaaSFulfillmentClient.WebHook;

    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [AllowAnonymous]
    [RequireHttps]
    public class WebHookController : Controller
    {
        private readonly ILogger<WebHookController> logger;

        private readonly IWebhookProcessor webhookProcessor;

        private readonly DashboardOptions options;

        public WebHookController(
            IWebhookProcessor webhookProcessor,
            IOptionsMonitor<DashboardOptions> optionsMonitor,
            ILogger<WebHookController> logger)
        {
            this.webhookProcessor = webhookProcessor;
            this.logger = logger;
            this.options = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] WebhookPayload payload)
        {
            using (var reader = new StreamReader(Request.Body))
            {
                // Using this to surface the exact payload the webhook is receiving
                var body = await reader.ReadToEndAsync();

                this.logger.LogInformation($"Webhook payload is \n {body}");
            }

            // Options is injected as a singleton. This is not a good hack, but need to pass the host name and port
            this.options.BaseUrl = $"{this.Request.Scheme}://{this.Request.Host}/";
            await this.webhookProcessor.ProcessWebhookNotificationAsync(payload);
            this.logger.LogInformation($"Received webhook request: {JsonConvert.SerializeObject(payload)}");
            return this.Ok();
        }
    }
}