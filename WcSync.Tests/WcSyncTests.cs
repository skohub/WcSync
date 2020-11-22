using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WcSync.Wc;
using WcSync.Db;
using System.Collections.Generic;
using System.Threading.Tasks;
using WcSync.Model.Entities;
using WcSync.Sync;
using System.Linq;

namespace WcSync.Tests
{
    [TestFixture]
    public class WcSyncTests
    {
        private ProductService _productService;
        private Mock<IWcProductService> _wcProductServiceMock;
        private Mock<IDbProductRepository> _dbProductRepositoryMock;
        private WcProduct DefaultWcProduct;
        private DbProduct DefaultDbProduct;

        [SetUp]
        public void SetUp()
        {
            DefaultWcProduct = new WcProduct
            {
                Availability = "test",
                Sku = "0",
                RegularPrice = 0,
                SalePrice = 0,
                StockStatus = "instock",
            };

            DefaultDbProduct = new DbProduct
            {
                Id = 0,
                Availability = new List<Store>
                {
                    new Store
                    {
                        Name = "test",
                        Quantity = 1,
                        Price = 0,
                        Type = StoreType.Shop,
                    }
                }
            };

            _wcProductServiceMock = new Mock<IWcProductService>();
            _wcProductServiceMock
                .Setup(m => m.GetProductsAsync())
                .Returns(Task.FromResult(new List<WcProduct>{ DefaultWcProduct }));

            _dbProductRepositoryMock = new Mock<IDbProductRepository>();
            _dbProductRepositoryMock
                .Setup(r => r.GetProducts())
                .Returns(new List<DbProduct>{ DefaultDbProduct });

            var loggerMock = new Mock<ILogger<ProductService>>();

            _productService = new ProductService(
                _wcProductServiceMock.Object,
                _dbProductRepositoryMock.Object,
                new PriceCalculator(loggerMock.Object),
                loggerMock.Object);
        }

        [Test]
        public async Task HappyFlow()
        {
            // Arrange

            // Act
            await _productService.UpdateAllProductsAsync();

            // Assert
        }

        [Test]
        public async Task TestProductUpToDate()
        {
            // Arrange
            DefaultDbProduct.Availability.First().Price = 10000;
            DefaultWcProduct.RegularPrice = 10000;
            DefaultWcProduct.SalePrice = 9700;

            // Act
            await _productService.UpdateAllProductsAsync();

            // Assert
            _wcProductServiceMock.Verify(
                s => s.UpdateProduct(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<decimal?>()),
                Times.Never);
        }

        [Test]
        [TestCase(0, 9700)]
        [TestCase(10000, 0)]
        public async Task TestUpdateProductCalledWhenPriceDiffers(decimal regularPrice, decimal salePrice)
        {
            // Arrange
            DefaultDbProduct.Availability.First().Price = 10000;
            DefaultWcProduct.RegularPrice = regularPrice;
            DefaultWcProduct.SalePrice = salePrice;

            // Act
            await _productService.UpdateAllProductsAsync();

            // Assert
            _wcProductServiceMock.Verify(
                s => s.UpdateProduct(
                    DefaultDbProduct.Id,
                    DefaultDbProduct.GetStockStatus(),
                    DefaultDbProduct.GetAvailability(),
                    10000,
                    9700),
                Times.Once);
        }
    }
}
