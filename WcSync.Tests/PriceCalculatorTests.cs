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
    public class PriceCalculatorTests
    {
        [Test]
        public void NullProductShouldReturnNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProductService>>();
            var priceCalculator = new PriceCalculator(loggerMock.Object);

            // Act
            (var price, var salePrice) = priceCalculator.GetPrice(null);

            // Assert
            Assert.IsNull(price);
            Assert.IsNull(salePrice);
        }

        [Test]
        public void InconsistentAvailabilityShouldReturnNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProductService>>();
            var priceCalculator = new PriceCalculator(loggerMock.Object);

            var dbProduct = new DbProduct
            {
                Availability = new List<Store>
                {
                    new Store
                    {
                        Type = StoreType.Shop,
                        Price = 1000,
                        Quantity = 1,
                    },
                    new Store
                    {
                        Type = StoreType.Shop,
                        Price = 2000,
                        Quantity = 1,
                    }
                }
            };

            // Act
            (var price, var salePrice) = priceCalculator.GetPrice(dbProduct);

            // Assert
            Assert.IsNull(price);
            Assert.IsNull(salePrice);
        }

        [Test]
        public void WrongTypeOfAvailabilityShouldReturnNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProductService>>();
            var priceCalculator = new PriceCalculator(loggerMock.Object);

            var dbProduct = new DbProduct
            {
                Availability = new List<Store>
                {
                    new Store
                    {
                        Type = StoreType.ClosedShop,
                        Price = 1000,
                        Quantity = 1,
                    },
                    new Store
                    {
                        Type = StoreType.Inactive,
                        Price = 1000,
                        Quantity = 1,
                    },
                    new Store
                    {
                        Type = StoreType.RepairShop,
                        Price = 1000,
                        Quantity = 1,
                    }
                }
            };

            // Act
            (var price, var salePrice) = priceCalculator.GetPrice(dbProduct);

            // Assert
            Assert.IsNull(price);
            Assert.IsNull(salePrice);
        }

        [Test]
        public void ZeroQuantityShouldReturnNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProductService>>();
            var priceCalculator = new PriceCalculator(loggerMock.Object);

            var dbProduct = new DbProduct
            {
                Availability = new List<Store>
                {
                    new Store
                    {
                        Type = StoreType.Shop,
                        Price = 1000,
                        Quantity = 0,
                    }
                }
            };

            // Act
            (var price, var salePrice) = priceCalculator.GetPrice(dbProduct);

            // Assert
            Assert.IsNull(price);
            Assert.IsNull(salePrice);
        }

        [Test]
        public void AbsentAvailabilityShouldReturnNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProductService>>();
            var priceCalculator = new PriceCalculator(loggerMock.Object);

            var dbProduct = new DbProduct
            {
                Availability = new List<Store>()
            };

            // Act
            (var price, var salePrice) = priceCalculator.GetPrice(dbProduct);

            // Assert
            Assert.IsNull(price);
            Assert.IsNull(salePrice);
        }

        [Test]
        public void TestCalculationBelowLowerBoundary()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProductService>>();
            var priceCalculator = new PriceCalculator(loggerMock.Object);

            var dbProduct = new DbProduct
            {
                Availability = new List<Store>
                {
                    new Store
                    {
                        Type = StoreType.Shop,
                        Price = 1000,
                        Quantity = 1,
                    }
                }
            };

            // Act
            (var price, var salePrice) = priceCalculator.GetPrice(dbProduct);

            // Assert
            Assert.AreEqual(1000, price);
            Assert.AreEqual(1000, salePrice);
        }

        [Test]
        public void TestCalculationAboveLowerBoundary()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProductService>>();
            var priceCalculator = new PriceCalculator(loggerMock.Object);

            var dbProduct = new DbProduct
            {
                Availability = new List<Store>
                {
                    new Store
                    {
                        Type = StoreType.Shop,
                        Price = 10000,
                        Quantity = 1,
                    }
                }
            };

            // Act
            (var price, var salePrice) = priceCalculator.GetPrice(dbProduct);

            // Assert
            Assert.AreEqual(10000, price);
            Assert.AreEqual(9700, salePrice);
        }

        [Test]
        public void WrongAvailabilityShouldNotAffectCalculation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProductService>>();
            var priceCalculator = new PriceCalculator(loggerMock.Object);

            var dbProduct = new DbProduct
            {
                Availability = new List<Store>
                {
                    new Store
                    {
                        Type = StoreType.Shop,
                        Price = 10000,
                        Quantity = 1,
                    },
                    new Store
                    {
                        Type = StoreType.Shop,
                        Price = 10000,
                        Quantity = 0,
                    },
                    new Store
                    {
                        Type = StoreType.ClosedShop,
                        Price = 10000,
                        Quantity = 1,
                    },
                    new Store
                    {
                        Type = StoreType.ClosedShop,
                        Price = 0,
                        Quantity = 1,
                    }
                }
            };

            // Act
            (var price, var salePrice) = priceCalculator.GetPrice(dbProduct);

            // Assert
            Assert.AreEqual(10000, price);
            Assert.AreEqual(9700, salePrice);
        }
    }
}