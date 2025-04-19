using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static DDDAplication.Api.IntegrationTests.Validation.AuthControllerValidationTests;

namespace DDDAplication.Api.IntegrationTests.Helper
{
    public static class ResponseHelper
    {
        public static async Task<ValidationErrorResponse?> GetValidationErrors(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<ValidationErrorResponse>();
        }

    }

    public class ValidationErrorResponse
    {
        [JsonPropertyName("errors")]
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
