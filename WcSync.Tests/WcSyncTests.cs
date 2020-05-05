using System;
using Moq;
using NUnit.Framework;
using WcSync.Cli;
using WcSync.Wc;
using WcSync.Db;
using WcSync.Db.Models;
using System.Collections.Generic;

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
            _wcProductServiceMock
                .Setup(s => s.GetProductIdBySku(It.IsAny<string>()))
                .ReturnsAsync(0);

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
                                Number = 1,
                            }
                        }
                    }
                });


        }

        [Test]
        public void HappyFlow() 
        {
            // Arrange
            var productService = new ProductService(
                _wcProductServiceMock.Object,
                _dbProductRepositoryMock.Object);

            // Act
            productService.UpdateRecentProducts();

            // Assert
        }
    }
}
