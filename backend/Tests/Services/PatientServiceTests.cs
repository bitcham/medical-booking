using Core.Application.Dtos.Requests;
using Core.Application.Exceptions;
using Core.Application.Repositories.Contracts;
using Core.Application.Services;
using Core.Application.Services.Impl;
using Core.Domain.Entities;
using FluentAssertions;
using Moq;
using backend.Tests.Fixtures;

namespace backend.Tests.Services;

public class PatientServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPatientRepository> _patientRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly PatientService _sut;

    public PatientServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _patientRepositoryMock = new Mock<IPatientRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new PatientService(
            _userRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region Register Tests

    [Fact]
    public async Task Register_WhenEmailExists_ShouldThrowDuplicateEmailException()
    {
        // Arrange
        var request = CreateRegisterRequest();
        var existingUser = TestFixtures.Users.Create();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var act = () => _sut.Register(request);

        // Assert
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    [Fact]
    public async Task Register_WhenValid_ShouldCreatePatientAndReturnResponse()
    {
        // Arrange
        var request = CreateRegisterRequest();

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(x => x.Hash(request.Password))
            .Returns("hashed_password");

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        _patientRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient p, CancellationToken _) => p);

        // Act
        var result = await _sut.Register(request);

        // Assert
        result.User.Email.Should().Be(request.Email);
        result.PhoneNumber.Should().Be(request.PhoneNumber);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetByIdAsync_WhenPatientNotFound_ShouldThrowPatientNotFoundException()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = () => _sut.GetByIdAsync(patientId);

        // Assert
        await act.Should().ThrowAsync<PatientNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenPatientExists_ShouldReturnPatientResponse()
    {
        // Arrange
        var patient = TestFixtures.Patients.Create();

        _patientRepositoryMock
            .Setup(x => x.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        var result = await _sut.GetByIdAsync(patient.Id);

        // Assert
        result.Id.Should().Be(patient.Id);
    }

    #endregion

    #region GetByUserId Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenPatientNotFound_ShouldThrowPatientNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _patientRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        // Act
        var act = () => _sut.GetByUserIdAsync(userId);

        // Assert
        await act.Should().ThrowAsync<PatientNotFoundException>();
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenPatientExists_ShouldReturnPatientResponse()
    {
        // Arrange
        var patient = TestFixtures.Patients.Create();

        _patientRepositoryMock
            .Setup(x => x.GetByUserIdAsync(patient.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);

        // Act
        var result = await _sut.GetByUserIdAsync(patient.UserId);

        // Assert
        result.Id.Should().Be(patient.Id);
    }

    #endregion

    private static RegisterPatientRequest CreateRegisterRequest()
    {
        return new RegisterPatientRequest(
            "patient@test.com",
            "password",
            "John",
            "Doe",
            "123-456-7890",
            new DateOnly(1990, 1, 1),
            "123 Main St",
            "New York",
            "10001",
            "USA"
        );
    }
}
