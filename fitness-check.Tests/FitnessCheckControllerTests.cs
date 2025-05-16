using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FitnessCheck.Controllers.v1;
using FitnessCheck.Data;
using FitnessCheck.Data.Entities;
using FitnessCheck.Services;
using FitnessCheckModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FitnessCheck.Tests
{
    [TestClass]
    public class FitnessCheckControllerTests
    {
        private readonly Mock<FitnessCheckDbContext> _dbContextMock;
        private readonly Mock<DbSet<CoreStrengthAttempt>> _mockCoreStrengthsDbSet;
        private readonly Mock<DbSet<MedicineBallPushAttempt>> _mockMedicineBallThrowsDbSet;
        private readonly Mock<DbSet<OneLegStandAttempt>> _mockOneLegStandsDbSet;
        private readonly Mock<DbSet<ShuttleRunAttempt>> _mockShuttleRunsDbSet;
        private readonly Mock<DbSet<StandingLongJumpAttempt>> _mockStandingLongJumpsDbSet;
        private readonly Mock<DbSet<TwelveMinutesRunAttempt>> _mockTwelveMinutesRunsDbSet;
        private readonly Mock<IPointsCalculator> _mockPointsCalculator;
        private readonly Mock<IGenderService> _mockGenderService;
        public FitnessCheckControllerTests()
        {
            // Initialize the mocks for DbContext and services
            _dbContextMock = new Mock<FitnessCheckDbContext>();
            _mockCoreStrengthsDbSet = new Mock<DbSet<CoreStrengthAttempt>>();
            _mockMedicineBallThrowsDbSet = new Mock<DbSet<MedicineBallPushAttempt>>();
            _mockOneLegStandsDbSet = new Mock<DbSet<OneLegStandAttempt>>();
            _mockShuttleRunsDbSet = new Mock<DbSet<ShuttleRunAttempt>>();
            _mockStandingLongJumpsDbSet = new Mock<DbSet<StandingLongJumpAttempt>>();
            _mockTwelveMinutesRunsDbSet = new Mock<DbSet<TwelveMinutesRunAttempt>>();
            
            // Initialize the mock for IPointsCalculator
            _mockPointsCalculator = new Mock<IPointsCalculator>();

            // Configure the mock to use PointsCalculator's CalculatePoints method
            var pointsCalculator = new PointsCalculator(_dbContextMock.Object);
            _mockPointsCalculator.Setup(pc => pc.CalculatePoints(It.IsAny<Func<ResultsCalculation, bool>>()))
                .Returns<Func<ResultsCalculation, bool>>(predicate => pointsCalculator.CalculatePoints(predicate));

            // Initialize the mock for IGenderService
            _mockGenderService = new Mock<IGenderService>();
        }

        private AttemptController CreateController()
        {
            // Setup for dbContext and services
            _dbContextMock.Setup(db => db.CoreStrengthAttempts).Returns(_mockCoreStrengthsDbSet.Object);
            _dbContextMock.Setup(db => db.MedicineBallPushAttempts).Returns(_mockMedicineBallThrowsDbSet.Object);
            _dbContextMock.Setup(db => db.OneLegStandAttempts).Returns(_mockOneLegStandsDbSet.Object);
            _dbContextMock.Setup(db => db.ShuttleRunAttempts).Returns(_mockShuttleRunsDbSet.Object);
            _dbContextMock.Setup(db => db.StandingLongJumpAttempts).Returns(_mockStandingLongJumpsDbSet.Object);
            _dbContextMock.Setup(db => db.TwelveMinutesRunAttempts).Returns(_mockTwelveMinutesRunsDbSet.Object);
            
            // Setup for services
            _mockGenderService.Setup(service => service.GetGenderAsync(It.IsAny<string>())).ReturnsAsync('m');
            _mockPointsCalculator.Setup(pc => pc.CalculatePoints(It.IsAny<Func<ResultsCalculation, bool>>()))
                .Returns(20);
            
            // Create and return FitnessCheckController instance
            return new FitnessCheckController(_dbContextMock.Object, _mockPointsCalculator.Object, _mockGenderService.Object);
        }
        private void SetupUserContext(AttemptController controller, string userId)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                }))
                }
            };
        }

        [TestMethod]
        public async Task AddCoreStrength_ValidInput_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();
            SetupUserContext(controller, "12345678-90ab-cdef-1234-567890abcdef");

            var coreStrengthDTO = new CoreStrengthDTO
            {
                ResultInSeconds = 60,
                ClassId = 123456
            };

            // Act
            var result = await controller.AddCoreStrength(coreStrengthDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "The result should be of type ObjectResult.");
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "The ObjectResult should not be null.");
            if (objectResult != null)
            {
                Assert.AreEqual(20, objectResult.Value, "The response message should indicate success.");
                Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode, "The status code should be 200 OK.");
            }

            // Verify that the CoreStrength entry was added to the database
            _dbContextMock.Verify(db => db.CoreStrengthAttempts.Add(It.IsAny<CoreStrengthAttempt>()), Times.Once, "CoreStrength entry should be added to the database once.");
            _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Once, "Database changes should be saved once.");
        }
        [TestMethod]
        public async Task AddMedicineBallThrow_ValidInput_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();
            SetupUserContext(controller, "12345678-90ab-cdef-1234-567890abcdef");

            var medicineBallThrowDTO = new MedicineBallPushDTO
            {
                ResultInCentimeters = 450,
                ClassId = 123456
            };

            // Act
            var result = await controller.AddMedicineBallThrow(medicineBallThrowDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "The result should be of type ObjectResult.");
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "The ObjectResult should not be null.");
            if (objectResult != null)
            {
                Assert.AreEqual(20, objectResult.Value, "The response message should indicate success.");
                Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode, "The status code should be 200 OK.");
            }

            // Verify that the MedicineBallThrow entry was added to the database
            _dbContextMock.Verify(db => db.MedicineBallPushAttempts.Add(It.IsAny<MedicineBallPushAttempt>()), Times.Once, "MedicineBallThrow entry should be added to the database once.");
            _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Once, "Database changes should be saved once.");
        }

        [TestMethod]
        public async Task AddOneLegStand_ValidInput_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();
            SetupUserContext(controller, "12345678-90ab-cdef-1234-567890abcdef");

            var oneLegStandDTO = new OneLegStandDTO
            {
                ResultInSeconds = 54,
                ClassId = 123456
            };

            // Act
            var result = await controller.AddOneLegStand(oneLegStandDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "The result should be of type ObjectResult.");
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "The ObjectResult should not be null.");
            if (objectResult != null)
            {
                Assert.AreEqual(20, objectResult.Value, "The response message should indicate success.");
                Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode, "The status code should be 200 OK.");
            }

            // Verify that the OneLegStand entry was added to the database
            _dbContextMock.Verify(db => db.OneLegStandAttempts.Add(It.IsAny<OneLegStandAttempt>()), Times.Once, "OneLegStand entry should be added to the database once.");
            _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Once, "Database changes should be saved once.");
        }

        [TestMethod]
        public async Task AddShuttleRun_ValidInput_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();
            SetupUserContext(controller, "12345678-90ab-cdef-1234-567890abcdef");

            var shuttleRunDTO = new ShuttleRunDTO
            {
                ResultInMilliseconds = 9340,
                ClassId = 123456
            };

            // Act
            var result = await controller.AddShuttleRun(shuttleRunDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "The result should be of type ObjectResult.");
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "The ObjectResult should not be null.");
            if (objectResult != null)
            {
                Assert.AreEqual(20, objectResult.Value, "The response message should indicate success.");
                Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode, "The status code should be 200 OK.");
            }

            // Verify that the ShuttleRun entry was added to the database
            _dbContextMock.Verify(db => db.ShuttleRunAttempts.Add(It.IsAny<ShuttleRunAttempt>()), Times.Once, "ShuttleRun entry should be added to the database once.");
            _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Once, "Database changes should be saved once.");
        }

        [TestMethod]
        public async Task AddStandingLongJump_ValidInput_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();
            SetupUserContext(controller, "12345678-90ab-cdef-1234-567890abcdef");

            var standingLongJumpDTO = new StandingLongJumpDTO
            {
                ResultInCentimeters = 340,
                ClassId = 123456
            };

            // Act
            var result = await controller.AddStandingLongJump(standingLongJumpDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "The result should be of type ObjectResult.");
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "The ObjectResult should not be null.");
            if (objectResult != null)
            {
                Assert.AreEqual(20, objectResult.Value, "The response message should indicate success.");
                Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode, "The status code should be 200 OK.");
            }

            // Verify that the StandingLongJump entry was added to the database
            _dbContextMock.Verify(db => db.StandingLongJumpAttempts.Add(It.IsAny<StandingLongJumpAttempt>()), Times.Once, "ShuttleRun entry should be added to the database once.");
            _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Once, "Database changes should be saved once.");
        }

        [TestMethod]
        public async Task AddTwelveMinutesRun_ValidInput_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();
            SetupUserContext(controller, "12345678-90ab-cdef-1234-567890abcdef");

            var twelveMinutesRunDTO = new TwelveMinutesRunDTO
            {
                ResultInRounds = 37,
                ClassId = 123456
            };

            // Act
            var result = await controller.AddTwelveMinutesRun(twelveMinutesRunDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult), "The result should be of type ObjectResult.");
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, "The ObjectResult should not be null.");
            if (objectResult != null)
            {
                Assert.AreEqual(20, objectResult.Value, "The response message should indicate success.");
                Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode, "The status code should be 200 OK.");
            }

            // Verify that the TwelveMinutesRun entry was added to the database
            _dbContextMock.Verify(db => db.TwelveMinutesRunAttempts.Add(It.IsAny<TwelveMinutesRunAttempt>()), Times.Once, "ShuttleRun entry should be added to the database once.");
            _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Once, "Database changes should be saved once.");
        }

        [TestMethod]
        public async Task AddCoreStrength_EmptyUserID_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();
            SetupUserContext(controller, "");

            // Create a CoreStrengthDTO with invalid input: negative ResultInSeconds
            var coreStrengthDTO = new CoreStrengthDTO
            {
                ResultInSeconds = 30,
                ClassId = 123456
            };

            // Act
            var result = await controller.AddCoreStrength(coreStrengthDTO);

            // Assert
            // The result should be a BadRequestObjectResult since the input is invalid
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "The result should be of type BadRequestObjectResult.");
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult, "The BadRequestObjectResult should not be null.");

            // The status code should be 400 Bad Request
            if (badRequestResult != null)
            {
                Assert.AreEqual("User identifier not found.", badRequestResult.Value, "The error message should match.");
                Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode, "The status code should be 400 Bad Request.");
            }

            // Verify that no CoreStrength entry was added to the database
            _dbContextMock.Verify(db => db.CoreStrengthAttempts.Add(It.IsAny<CoreStrengthAttempt>()), Times.Never, "No CoreStrength entry should be added to the database.");
            _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Never, "No database changes should be saved.");
        }

        [TestMethod]
        public async Task AddMedicineBallThrow_WrongUserID_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();
            SetupUserContext(controller, "Not A UUID");

            // Create a CoreStrengthDTO with invalid input: negative ResultInSeconds
            var coreStrengthDTO = new CoreStrengthDTO
            {
                ResultInSeconds = 30,
                ClassId = 123456
            };

            // Act
            var result = await controller.AddCoreStrength(coreStrengthDTO);

            // Assert
            // The result should be a BadRequestObjectResult since the input is invalid
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult), "The result should be of type BadRequestObjectResult.");
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult, "The BadRequestObjectResult should not be null.");

            // The status code should be 400 Bad Request
            if (badRequestResult != null)
            {
                Assert.AreEqual("User identifier is not a valid UUID.", badRequestResult.Value, "The error message should match.");
                Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode, "The status code should be 400 Bad Request.");
            }

            // Verify that no CoreStrength entry was added to the database
            _dbContextMock.Verify(db => db.CoreStrengthAttempts.Add(It.IsAny<CoreStrengthAttempt>()), Times.Never, "No CoreStrength entry should be added to the database.");
            _dbContextMock.Verify(db => db.SaveChangesAsync(default), Times.Never, "No database changes should be saved.");
        }


    }
}
