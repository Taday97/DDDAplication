using AutoMapper;
using DDDAplication.API;
using DDDAplication.API.Controllers;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;

namespace DDDAplication.Api.IntegrationTests
{
    [TestFixture]
    public class RoleControllerIntegrationTests
    {
        private HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public RoleControllerIntegrationTests()
        {
            _factory = new WebApplicationFactory<Program>(); 
            _client = _factory.CreateClient();
        }

        [OneTimeSetUp] 
        [SetUp]
        public void SetUp()
        {
        }



        [OneTimeTearDown] 
        public void Cleanup()
        {
            _client.Dispose(); 
            _factory.Dispose(); 
        }
    }
}