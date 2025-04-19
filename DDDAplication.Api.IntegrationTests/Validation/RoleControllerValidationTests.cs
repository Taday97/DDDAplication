using DDDAplication.Api.IntegrationTests.Common;
using DDDAplication.Api.IntegrationTests.Helper;
using DDDAplication.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DDDAplication.API;
using FluentAssertions;
using DDDAplication.Domain.Entities;

namespace DDDAplication.Api.IntegrationTests.Validation
{
    [TestFixture]
    public class RoleControllerValidationTests
    {
        private HttpClient _client;
        private readonly ApiApplicationFactory<Program> _factory;

        public RoleControllerValidationTests()
        {
            _factory = new ApiApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task CreateRole_Should_Return_BadRequest_When_Invalid_Request()
        {
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = new RoleDto
            {
                Name = "", 
                NormalizedName = "" 
            };

            var response = await _client.PostAsJsonAsync("/api/role", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);


            responseBody.Errors.Should().ContainKey("Name").WhoseValue.Should().Contain("Name is required.");
            responseBody.Errors.Should().ContainKey("NormalizedName").WhoseValue.Should().Contain("NormalizedName is required.");
        }

        [Test]
        public async Task UpdateRole_Should_Return_BadRequest_When_Invalid_Request()
        {
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = new RoleDto
            {
                Name = "", 
                NormalizedName = "" 
            };
            var roleId = "1";

            var response = await _client.PutAsJsonAsync($"/api/role/{roleId}", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);

            responseBody.Errors.Should().ContainKey("Name").WhoseValue.Should().Contain("Name is required.");
            responseBody.Errors.Should().ContainKey("NormalizedName").WhoseValue.Should().Contain("NormalizedName is required.");
        }

        [Test]
        public async Task GetRoleById_Should_Return_BadRequest_When_Invalid_Id()
        {
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var roleId = "111";

            var response = await _client.GetAsync($"/api/role/{roleId}");

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public async Task CreateRole_Should_Return_BadRequest_When_Role_Name_Too_Long()
        {
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = new RoleDto
            {
                Name = new string('A', 101),
                NormalizedName = new string('A', 101) 
            };

            var response = await _client.PostAsJsonAsync("/api/role", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);

            responseBody.Errors.Should().ContainKey("Name").WhoseValue.Should().Contain("Name must be between 3 and 50 characters.");
            responseBody.Errors.Should().ContainKey("NormalizedName").WhoseValue.Should().Contain("NormalizedName must be between 3 and 50 characters.");
        }

        [Test]
        public async Task CreateRole_Should_Return_BadRequest_When_Role_Name_Too_Short()
        {
            var token = await TokenHelper.GetJwtTokenAsync(_client);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = new RoleDto
            {
                Name = "A",
                NormalizedName = "A" 
            };

            var response = await _client.PostAsJsonAsync("/api/role", requestBody);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseBody = await ResponseHelper.GetValidationErrors(response);


            responseBody.Errors.Should().ContainKey("Name").WhoseValue.Should().Contain("Name must be between 3 and 50 characters.");
            responseBody.Errors.Should().ContainKey("NormalizedName").WhoseValue.Should().Contain("NormalizedName must be between 3 and 50 characters.");
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
        }

    }
}
