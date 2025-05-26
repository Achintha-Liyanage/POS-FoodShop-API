using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MyPOS.API.Controllers;
using MyPOS.Application.DTOs.Products;
using MyPOS.Application.Interfaces;
using System.Collections.Generic; // For IEnumerable in GetAllProducts
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyPOS.API.Tests
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILogger<ProductsController>> _mockLogger;
        private readonly ProductsController _sut;
        private readonly Fixture _fixture;

        public ProductsControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<ProductsController>>();
            _sut = new ProductsController(_mockProductService.Object, _mockLogger.Object);
            _fixture = new Fixture();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "Admin"), 
            }, "mock"));

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAllProducts_ShouldReturnOkObjectResultWithProductList()
        {
            // Arrange
            var productList = _fixture.CreateMany<ProductDto>(3).ToList();
            _mockProductService.Setup(service => service.GetAllProductsAsync())
                               .ReturnsAsync(productList);

            // Act
            var result = await _sut.GetAllProducts();

            // Assert
            result.Should().BeOfType<ActionResult<IEnumerable<ProductDto>>>();
            var actionResult = result as ActionResult<IEnumerable<ProductDto>>;
            actionResult.Result.Should().BeOfType<OkObjectResult>();
            var okResult = actionResult.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(productList);
            _mockProductService.Verify(service => service.GetAllProductsAsync(), Times.Once);
        }


        [Fact]
        public async Task GetProductById_WhenProductExists_ShouldReturnOkObjectResultWithProductDto()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            var productDto = _fixture.Build<ProductDto>()
                                     .With(p => p.Id, productId)
                                     .Create();
            _mockProductService.Setup(service => service.GetProductByIdAsync(productId))
                               .ReturnsAsync(productDto);

            // Act
            var result = await _sut.GetProductById(productId);

            // Assert
            result.Should().BeOfType<ActionResult<ProductDto>>();
            var actionResult = result as ActionResult<ProductDto>;
            actionResult.Result.Should().BeOfType<OkObjectResult>();
            var okResult = actionResult.Result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(productDto);
            _mockProductService.Verify(service => service.GetProductByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task GetProductById_WhenProductDoesNotExist_ShouldReturnNotFoundResult()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            _mockProductService.Setup(service => service.GetProductByIdAsync(productId))
                               .ReturnsAsync((ProductDto)null);

            // Act
            var result = await _sut.GetProductById(productId);

            // Assert
            result.Should().BeOfType<ActionResult<ProductDto>>();
            var actionResult = result as ActionResult<ProductDto>;
            actionResult.Result.Should().BeOfType<NotFoundResult>();
            _mockProductService.Verify(service => service.GetProductByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WithValidModelAndAdminRole_ShouldReturnCreatedAtActionResultWithProductDto()
        {
            // Arrange
            var createProductDto = _fixture.Create<CreateProductDto>();
            var productDtoToReturn = _fixture.Build<ProductDto>()
                                             .With(p => p.Name, createProductDto.Name)
                                             .With(p => p.Price, createProductDto.Price)
                                             .With(p => p.Description, createProductDto.Description)
                                             .Create();
            _mockProductService.Setup(service => service.CreateProductAsync(createProductDto))
                               .ReturnsAsync(productDtoToReturn);

            // Act
            var result = await _sut.CreateProduct(createProductDto);

            // Assert
            result.Should().BeOfType<ActionResult<ProductDto>>();
            var actionResult = result as ActionResult<ProductDto>;
            actionResult.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdAtActionResult = actionResult.Result as CreatedAtActionResult;
            createdAtActionResult.ActionName.Should().Be(nameof(ProductsController.GetProductById));
            createdAtActionResult.RouteValues["id"].Should().Be(productDtoToReturn.Id);
            createdAtActionResult.Value.Should().BeEquivalentTo(productDtoToReturn);
            _mockProductService.Verify(service => service.CreateProductAsync(createProductDto), Times.Once);
        }

        [Fact]
        public async Task CreateProduct_WithInvalidModel_ShouldReturnBadRequestObjectResult()
        {
            // Arrange
            var createProductDto = _fixture.Create<CreateProductDto>();
            _sut.ModelState.AddModelError("Name", "Required"); 

            // Act
            var result = await _sut.CreateProduct(createProductDto);

            // Assert
            result.Should().BeOfType<ActionResult<ProductDto>>();
            var actionResult = result as ActionResult<ProductDto>;
            actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
            _mockProductService.Verify(service => service.CreateProductAsync(It.IsAny<CreateProductDto>()), Times.Never);
        }
        
        [Fact]
        public async Task CreateProduct_WhenUserIsNotAdmin_ShouldReturnForbidResult()
        {
            // Arrange
            var createProductDto = _fixture.Create<CreateProductDto>();
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, "User"), 
            }, "mock"));
            _sut.ControllerContext.HttpContext.User = user;

            // Act
            var result = await _sut.CreateProduct(createProductDto);

            // Assert
            result.Should().BeOfType<ActionResult<ProductDto>>();
            var actionResult = result as ActionResult<ProductDto>;
            actionResult.Result.Should().BeOfType<ForbidResult>();
            _mockProductService.Verify(service => service.CreateProductAsync(It.IsAny<CreateProductDto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProduct_WhenProductExistsAndModelIsValid_ShouldReturnNoContentResult()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            var updateProductDto = _fixture.Create<UpdateProductDto>();
            var existingProductDto = _fixture.Build<ProductDto>().With(p => p.Id, productId).Create();

            _mockProductService.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync(existingProductDto);
            _mockProductService.Setup(service => service.UpdateProductAsync(productId, updateProductDto))
                               .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateProduct(productId, updateProductDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockProductService.Verify(service => service.GetProductByIdAsync(productId), Times.Once);
            _mockProductService.Verify(service => service.UpdateProductAsync(productId, updateProductDto), Times.Once);
        }

        [Fact]
        public async Task UpdateProduct_WhenProductDoesNotExist_ShouldReturnNotFoundResult()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            var updateProductDto = _fixture.Create<UpdateProductDto>();
            _mockProductService.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync((ProductDto)null);


            // Act
            var result = await _sut.UpdateProduct(productId, updateProductDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
             _mockProductService.Verify(service => service.GetProductByIdAsync(productId), Times.Once);
            _mockProductService.Verify(service => service.UpdateProductAsync(It.IsAny<int>(), It.IsAny<UpdateProductDto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProduct_WithInvalidModel_ShouldReturnBadRequestObjectResult()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            var updateProductDto = _fixture.Create<UpdateProductDto>();
            _sut.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _sut.UpdateProduct(productId, updateProductDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            _mockProductService.Verify(service => service.UpdateProductAsync(It.IsAny<int>(), It.IsAny<UpdateProductDto>()), Times.Never);
        }

        [Fact]
        public async Task DeleteProduct_WhenProductExists_ShouldReturnNoContentResult()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            var productDto = _fixture.Build<ProductDto>().With(p => p.Id, productId).Create();
            _mockProductService.Setup(service => service.GetProductByIdAsync(productId))
                               .ReturnsAsync(productDto); // Service needs to confirm product exists
            _mockProductService.Setup(service => service.DeleteProductAsync(productId))
                               .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.DeleteProduct(productId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _mockProductService.Verify(service => service.GetProductByIdAsync(productId), Times.Once);
            _mockProductService.Verify(service => service.DeleteProductAsync(productId), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_WhenProductDoesNotExist_ShouldReturnNotFoundResult()
        {
            // Arrange
            var productId = _fixture.Create<int>();
            _mockProductService.Setup(service => service.GetProductByIdAsync(productId))
                               .ReturnsAsync((ProductDto)null); // Product not found

            // Act
            var result = await _sut.DeleteProduct(productId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            _mockProductService.Verify(service => service.GetProductByIdAsync(productId), Times.Once);
            _mockProductService.Verify(service => service.DeleteProductAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
