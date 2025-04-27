Feature: Product Management Integration Test
  As a system administrator
  I want to manage products in the catalog
  So that customers can browse and purchase items

  Background: 
    Given I am authenticated as a test user

  Scenario: Get all available products
    When I request all products
    Then the response should be successful
    And I should receive a list of 3 products

  Scenario: Filter products by color
    When I request products with color "Red"
    Then the response should be successful
    And I should receive a list of 2 products
    And all products should have color "Red"

  Scenario: Create a new product
    Given I have a new product with the following details:
      | Name                   | Description                | Price  | Color | SKU     | StockQuantity |
      | Integration Test Product | Created during integration test | 108.99 | Green | TEST000 | 42           |
    When I send a request to create the product
    Then the response should be successful
    And the created product should have name "Integration Test Product"
    And the created product should have color "Green"
    And I should be able to retrieve the product by its ID