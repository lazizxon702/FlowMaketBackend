using FlowMarket.Application.Products;
using Xunit;

namespace FlowMarket.Application.Tests;

public class ProductValidationTests
{
    [Fact]
    public void CreateProductCommand_AllowsValidInput()
    {
        var command = new CreateProductCommand(new("Product A", "Description", 19.99m, 10));
        Assert.Equal("Product A", command.Request.Name);
    }
}
