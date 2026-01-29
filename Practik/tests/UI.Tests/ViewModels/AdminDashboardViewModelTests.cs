using Moq;
using System.Collections.ObjectModel;
using UI.Application.Services;
using UI.Domain.Entities;
using UI.Domain.Enums;
using UI.Presentation.Services;
using UI.Presentation.ViewModels;

namespace UI.Tests.ViewModels;

public class AdminDashboardViewModelTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IBackupService> _backupServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly AdminDashboardViewModel _viewModel;

    public AdminDashboardViewModelTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _backupServiceMock = new Mock<IBackupService>();
        _dialogServiceMock = new Mock<IDialogService>();

        _userServiceMock.Setup(s => s.GetEmployeesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>
        {
            new User { Id = 1, Username = "admin", Role = UserRole.Admin },
            new User { Id = 2, Username = "master", Role = UserRole.Master }
        });

        _viewModel = new AdminDashboardViewModel(_userServiceMock.Object, _backupServiceMock.Object, _dialogServiceMock.Object);
    }

    [Fact]
    public void LoadEmployeesAsync_ShouldPopulateEmployeesCollection()
    {
        // Assert
        Assert.Equal(2, _viewModel.Employees.Count);
    }

    [Fact]
    public async Task CreateEmployeeAsync_ShouldCallUserServiceAndReloadEmployees()
    {
        // Arrange
        _viewModel.NewUsername = "newuser";
        _viewModel.NewPassword = "password";
        _viewModel.NewUserRole = UserRole.Receptionist;

        // Act
        await _viewModel.CreateEmployeeCommand.ExecuteAsync(null);

        // Assert
        _userServiceMock.Verify(s => s.CreateEmployeeAsync("newuser", "password", UserRole.Receptionist, It.IsAny<CancellationToken>()), Times.Once);
        _dialogServiceMock.Verify(d => d.ShowInfo("Employee created successfully.", It.IsAny<string>()), Times.Once);
        _userServiceMock.Verify(s => s.GetEmployeesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2)); // Initial load + reload
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldCallUserServiceAndReloadEmployees()
    {
        // Arrange
        var user = new User { Id = 2, Username = "master", Role = UserRole.Receptionist };

        // Act
        await _viewModel.UpdateRoleCommand.ExecuteAsync(user);

        // Assert
        _userServiceMock.Verify(s => s.UpdateEmployeeRoleAsync(2, UserRole.Receptionist, It.IsAny<CancellationToken>()), Times.Once);
        _dialogServiceMock.Verify(d => d.ShowInfo("Employee role updated successfully.", It.IsAny<string>()), Times.Once);
        _userServiceMock.Verify(s => s.GetEmployeesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ShouldCallUserService_WhenConfirmed()
    {
        // Arrange
        var user = new User { Id = 2, Username = "master", Role = UserRole.Master };
        _dialogServiceMock.Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _userServiceMock.Setup(s => s.DeleteEmployeeAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        await _viewModel.DeleteEmployeeCommand.ExecuteAsync(user);

        // Assert
        _userServiceMock.Verify(s => s.DeleteEmployeeAsync(2, It.IsAny<CancellationToken>()), Times.Once);
        _dialogServiceMock.Verify(d => d.ShowInfo("Employee deleted successfully.", It.IsAny<string>()), Times.Once);
        _userServiceMock.Verify(s => s.GetEmployeesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
