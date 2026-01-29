using Moq;
using System.Threading.Tasks;
using UI.Application.Services;
using UI.Domain.Entities;
using UI.Infrastructure.Session;
using UI.Presentation.ViewModels;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using UI.Domain.Enums;

namespace UI.Tests.ViewModels;

public class OrderManagementViewModelTests
{
    private readonly Mock<IRepairOrderService> _mockRepairOrderService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<IActOfWorkService> _mockActOfWorkService;
    private readonly Mock<IWindowService> _mockWindowService;
    private readonly UserSession _userSession;
    private readonly OrderManagementViewModel _viewModel;

    public OrderManagementViewModelTests()
    {
        _mockRepairOrderService = new Mock<IRepairOrderService>();
        _mockUserService = new Mock<IUserService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockActOfWorkService = new Mock<IActOfWorkService>();
        _mockWindowService = new Mock<IWindowService>();
        _userSession = new UserSession();
        _viewModel = new OrderManagementViewModel(
            _mockRepairOrderService.Object,
            _mockUserService.Object,
            _mockDialogService.Object,
            _userSession,
            _mockActOfWorkService.Object,
            _mockWindowService.Object
        );
    }

    [Fact]
    public async Task LoadOrdersAsync_ShouldPopulateOrdersAndMasters()
    {
        // Arrange
        var orders = new List<RepairOrder> { new RepairOrder { Id = 1, Description = "Test Order" } };
        var masters = new List<User> { new User { Id = 1, Username = "Test Master", Role = UserRole.Master } };
        _mockRepairOrderService.Setup(s => s.GetAllOrdersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(orders);
        _mockUserService.Setup(s => s.GetUsersByRoleAsync(UserRole.Master, It.IsAny<CancellationToken>())).ReturnsAsync(masters);

        // Act
        await _viewModel.LoadOrdersAsync();

        // Assert
        Assert.NotEmpty(_viewModel.Orders);
        Assert.Single(_viewModel.Orders);
        Assert.NotEmpty(_viewModel.Masters);
        Assert.Equal(2, _viewModel.Masters.Count); // Includes "All"
    }
}
