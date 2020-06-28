using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WcSync.Cli;
using WcSync.Wc;
using WcSync.Db;
using WcSync.Db.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WcSync.Tests
{
    [TestFixture]
    public class WcSyncTests
    {
        private Mock<IWcProductService> _wcProductServiceMock;
        private Mock<IDbProductRepository> _dbProductRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _wcProductServiceMock = new Mock<IWcProductService>();
            _dbProductRepositoryMock = new Mock<IDbProductRepository>();
            _dbProductRepositoryMock
                .Setup(r => r.GetAvailableProducts())
                .Returns(new List<Product> {
                    new Product
                    {
                        Id = 0,
                        Availability = new List<Store>
                        {
                            new Store
                            {
                                Name = "test",
                                Quantity = 1,
                            }
                        }
                    }
                });
        }

        [Test]
        public async Task HappyFlow()
        {
            // Arrange
            var productService = new ProductService(
                _wcProductServiceMock.Object,
                _dbProductRepositoryMock.Object,
                new Mock<ILogger<ProductService>>().Object);

            // Act
            await productService.UpdateRecentProductsAsync();

            // Assert
        }
    }
}
