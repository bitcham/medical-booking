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

public class ClinicianServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IClinicianRepository> _clinicianRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ClinicianService _sut;

    public ClinicianServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _clinicianRepositoryMock = new Mock<IClinicianRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new ClinicianService(
            _userRepositoryMock.Object,
            _clinicianRepositoryMock.Object,
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
    public async Task Register_WhenValid_ShouldCreateClinicianAndReturnResponse()
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

        _clinicianRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Clinician>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Clinician c, CancellationToken _) => c);

        // Act
        var result = await _sut.Register(request);

        // Assert
        result.User.Email.Should().Be(request.Email);
        result.Specialization.Should().Be(request.Specialization);
        result.LicenseNumber.Should().Be(request.LicenseNumber);
        
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetByIdAsync_WhenClinicianNotFound_ShouldThrowClinicianNotFoundException()
    {
        // Arrange
        var clinicianId = Guid.NewGuid();

        _clinicianRepositoryMock
            .Setup(x => x.GetByIdAsync(clinicianId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Clinician?)null);

        // Act
        var act = () => _sut.GetByIdAsync(clinicianId);

        // Assert
        await act.Should().ThrowAsync<ClinicianNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenClinicianExists_ShouldReturnClinicianResponse()
    {
        // Arrange
        var clinician = TestFixtures.Clinicians.Create();

        _clinicianRepositoryMock
            .Setup(x => x.GetByIdAsync(clinician.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clinician);

        // Act
        var result = await _sut.GetByIdAsync(clinician.Id);

        // Assert
        result.Id.Should().Be(clinician.Id);
        result.Specialization.Should().Be(clinician.Specialization);
    }

    #endregion

    #region GetByUserId Tests

    [Fact]
    public async Task GetByUserIdAsync_WhenClinicianNotFound_ShouldThrowClinicianNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _clinicianRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Clinician?)null);

        // Act
        var act = () => _sut.GetByUserIdAsync(userId);

        // Assert
        await act.Should().ThrowAsync<ClinicianNotFoundException>();
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAllAsync_WhenNoClinicians_ShouldReturnEmptyList()
    {
        // Arrange
        _clinicianRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Clinician>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenHasClinicians_ShouldReturnAll()
    {
        // Arrange
        var clinicians = new List<Clinician>
        {
            TestFixtures.Clinicians.Create(),
            TestFixtures.Clinicians.Create()
        };

        _clinicianRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(clinicians);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    private static RegisterClinicianRequest CreateRegisterRequest()
    {
        return new RegisterClinicianRequest(
            "doc@test.com",
            "password",
            "Dr",
            "Smith",
            "LIC123456",
            "General Dentist",
            "Experienced dentist with 10 years of practice."
        );
    }
}
