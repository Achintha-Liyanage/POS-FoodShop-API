using AutoFixture;
using FluentAssertions;
using Moq;
using MyPOS.Application.DTOs.Products;
using MyPOS.Application.Services;
using MyPOS.Domain.Interfaces;
using MyPOS.Domain.Models;
using System.Threading.Tasks;

namespace MyPOS.Application.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly ProductService _sut; // System Under Test
        private readonly Fixture _fixture;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _sut = new ProductService(_mockProductRepository.Object);
            _fixture = new Fixture(); 
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnProductDto()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            var product = _fixture.Build<Product>()
                                  .With(p => p.Id, productId)
                                  .Create();
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                                  .ReturnsAsync(product);

            // Act
            var result = await _sut.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ProductDto>();
            result.Id.Should().Be(product.Id);
            result.Name.Should().Be(product.Name);
            result.Price.Should().Be(product.Price);
            result.Description.Should().Be(product.Description);
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                                  .ReturnsAsync((Product)null);

            // Act
            var result = await _sut.GetProductByIdAsync(productId);

            // Assert
            result.Should().BeNull();
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_WithValidData_ShouldReturnProductDtoAndCallAddAsync()
        {
            // Arrange
            var createProductDto = _fixture.Create<CreateProductDto>();
            var productToReturn = new Product 
            {
                Id = _fixture.Create<int>(), 
                Name = createProductDto.Name,
                Price = createProductDto.Price,
                Description = createProductDto.Description
            };

            _mockProductRepository.Setup(repo => repo.AddAsync(It.Is<Product>(p =>
                p.Name == createProductDto.Name &&
                p.Price == createProductDto.Price &&
                p.Description == createProductDto.Description)))
                .Returns(Task.CompletedTask) 
                .Callback<Product>(p => p.Id = productToReturn.Id); 

            // Act
            var result = await _sut.CreateProductAsync(createProductDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ProductDto>();
            result.Id.Should().Be(productToReturn.Id); 
            result.Name.Should().Be(createProductDto.Name);
            result.Price.Should().Be(createProductDto.Price);
            result.Description.Should().Be(createProductDto.Description);

            _mockProductRepository.Verify(repo => repo.AddAsync(It.Is<Product>(p =>
                p.Name == createProductDto.Name &&
                p.Price == createProductDto.Price &&
                p.Description == createProductDto.Description)), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WhenProductExists_ShouldCallGetByIdAndUpdateAsync()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            var updateProductDto = _fixture.Create<UpdateProductDto>();
            var existingProduct = _fixture.Build<Product>()
                                          .With(p => p.Id, productId)
                                          .Create();

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                                  .ReturnsAsync(existingProduct);
            _mockProductRepository.Setup(repo => repo.UpdateAsync(It.Is<Product>(p => p.Id == productId)))
                                  .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateProductAsync(productId, updateProductDto);

            // Assert
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.Is<Product>(p => 
                p.Id == productId &&
                p.Name == updateProductDto.Name &&
                p.Price == updateProductDto.Price &&
                p.Description == updateProductDto.Description
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_WhenProductDoesNotExist_ShouldNotCallUpdateAsyncAndReturn()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            var updateProductDto = _fixture.Create<UpdateProductDto>();

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                                  .ReturnsAsync((Product)null);

            // Act
            await _sut.UpdateProductAsync(productId, updateProductDto);

            // Assert
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Product>()), Times.Never);
            // Service currently doesn't throw or return a boolean, so we just verify no update was attempted.
            // If the service were to throw an exception, we'd use .Should().ThrowAsync<...>()
        }

        [Fact]
        public async Task DeleteProductAsync_WhenProductExists_ShouldCallGetByIdAndDeleteAsync()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            var existingProduct = _fixture.Build<Product>()
                                          .With(p => p.Id, productId)
                                          .Create();

            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                                  .ReturnsAsync(existingProduct);
            _mockProductRepository.Setup(repo => repo.DeleteAsync(existingProduct))
                                  .Returns(Task.CompletedTask);

            // Act
            await _sut.DeleteProductAsync(productId);

            // Assert
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.DeleteAsync(existingProduct), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_WhenProductDoesNotExist_ShouldNotCallDeleteAsync()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId))
                                  .ReturnsAsync((Product)null);

            // Act
            await _sut.DeleteProductAsync(productId);

            // Assert
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Product>()), Times.Never);
        }
    }
}
