using Products.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reqnroll;
using Reqnroll.Assist;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Products.Api.IntegrationTest.Helper;

namespace Products.Api.IntegrationTest.Steps
{
    [Binding]
    public class ProductsStepDefinitions : IClassFixture<CustomWebApplicationFactory>
        {
            private readonly CustomWebApplicationFactory _factory;
            private readonly HttpClient _client;
            private HttpResponseMessage _response;
            private List<ProductDto> _products;
            private ProductDto _createdProduct;
            private CreateProductDto _newProduct;

            public ProductsStepDefinitions(CustomWebApplicationFactory factory)
            {
                _factory = factory;
                _client = factory.CreateClient();
            }

            [Given(@"I am authenticated as a test user")]
            public void GivenIAmAuthenticatedAsATestUser()
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            }

            [When(@"I request all products")]
            public async Task WhenIRequestAllProducts()
            {
                _response = await _client.GetAsync("/api/Products");
            }

            [When(@"I request products with color ""(.*)""")]
            public async Task WhenIRequestProductsWithColor(string color)
            {
                _response = await _client.GetAsync($"/api/Products/getProductByColor?color={color}");
            }

            [Given(@"I have a new product with the following details:")]
            public void GivenIHaveANewProductWithTheFollowingDetails(Table table)
            {
                _newProduct = table.CreateInstance<CreateProductDto>();
            }

            [When(@"I send a request to create the product")]
            public async Task WhenISendARequestToCreateTheProduct()
            {
                _response = await _client.PostAsJsonAsync("/api/Products/createProduct", _newProduct);
            }

            [Then(@"the response should be successful")]
            public void ThenTheResponseShouldBeSuccessful()
            {
                _response.EnsureSuccessStatusCode();
            }

            [Then(@"I should receive a list of (.*) products")]
            public async Task ThenIShouldReceiveAListOfProducts(int expectedCount)
            {
                _products = await _response.Content.ReadFromJsonAsync<List<ProductDto>>();
                Assert.NotNull(_products);
                Assert.Equal(expectedCount, _products.Count);
            }

            [Then(@"all products should have color ""(.*)""")]
            public void ThenAllProductsShouldHaveColor(string expectedColor)
            {
                Assert.All(_products, p => Assert.Equal(expectedColor, p.Color));
            }

            [Then(@"the created product should have name ""(.*)""")]
            public async Task ThenTheCreatedProductShouldHaveName(string expectedName)
            {
                _createdProduct = await _response.Content.ReadFromJsonAsync<ProductDto>();
                Assert.NotNull(_createdProduct);
                Assert.Equal(expectedName, _createdProduct.Name);
            }

            [Then(@"the created product should have color ""(.*)""")]
            public void ThenTheCreatedProductShouldHaveColor(string expectedColor)
            {
                Assert.Equal(expectedColor, _createdProduct.Color);
            }

            [Then(@"I should be able to retrieve the product by its ID")]
            public async Task ThenIShouldBeAbleToRetrieveTheProductByItsID()
            {
                var productResponse = await _client.GetAsync($"/api/Products/getProductById?id={_createdProduct.Id}");
                productResponse.EnsureSuccessStatusCode();

                var retrievedProduct = await productResponse.Content.ReadFromJsonAsync<ProductDto>();
                Assert.Equal(_createdProduct.Id, retrievedProduct.Id);
                Assert.Equal(_createdProduct.Name, retrievedProduct.Name);
            }
        }
    }