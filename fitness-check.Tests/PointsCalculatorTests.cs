using System;
using System.Linq;
using FitnessCheck;
using FitnessCheck.Data;
using FitnessCheck.Services;
using FitnessCheckModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using Moq;

namespace fitness_check.Tests
{
    // Define ResultsCalculationTestData class for the test data
    public class ResultsCalculationTestData
    {
        public char gender { get; set; }
        public string discipline { get; set; }
        public int result { get; set; }
        public int expectedPoints { get; set; }
    }



    [TestClass]
    public class PointsCalculatorTests
    {
        private readonly Mock<FitnessCheckDbContext> _dbContextMock = new Mock<FitnessCheckDbContext>();

        // CreateMockDbSet method for creating a mock DbSet from IQueryable
        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> queryableList) where T : class
        {
            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableList.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableList.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryableList.GetEnumerator());
            return mockDbSet;
        }

        [TestMethod]
        public void Calculate_ValidGenderNoResults_ThrowsException()
        {
            // Arrange
            var emptyResults = new List<ResultsCalculation>().AsQueryable();
            _dbContextMock.Setup(db => db.Results)
                          .Returns(CreateMockDbSet(emptyResults).Object);

            var calculator = new PointsCalculator(_dbContextMock.Object);
            Func<ResultsCalculation, bool> predicate = _ => true;

            // Act & Assert
            Assert.ThrowsException<Exception>(() => calculator.CalculatePoints(predicate));
        }



        /// <summary>
        /// Seed the database with some results for testing
        /// The data is stored in a JSON file in the project
        /// </summary>
        private void SeedDatabase()
        {
            var resultsCalculation = DeserializeJSON<ResultsCalculation>("../../../ResultsCalculationData.json").AsQueryable();
            _dbContextMock.Setup(db => db.Results)
                          .Returns(CreateMockDbSet(resultsCalculation).Object);
        }

        private IEnumerable<T> DeserializeJSON<T>(string jsonFileName)
        {
            var json = File.ReadAllText(jsonFileName);
            return JsonSerializer.Deserialize<IEnumerable<T>>(json);
        }

        private object GetPropertyValue(ResultsCalculation obj, string propertyName)
        {
            var property = typeof(ResultsCalculation).GetProperty(propertyName);
            return property.GetValue(obj);
        }

        [TestMethod]
        public void PointsCalculator_Calculate_TestCalculateWithTestData()
        {
            // Arrange
            SeedDatabase();
            var calculator = new PointsCalculator(_dbContextMock.Object);

            // Get the TestData from the JSON file
            var resultsCalculation = DeserializeJSON<ResultsCalculationTestData>("../../../ResultsCalculationTestData.json").AsQueryable();

            // Loop through the data and calculate the points
            foreach (var item in resultsCalculation)
            {
                // Get the necessary attributes
                var gender = item.gender;
                var discipline = item.discipline;
                var result = item.result;
                var expectedPoints = item.expectedPoints;

                // Define a placeholder for the predicate
                Func<ResultsCalculation, bool> predicate;

                // Define the predicate based on the discipline
                switch (discipline)
                {
                    case "ShuttleRun":
                        predicate = x => (int)GetPropertyValue(x, discipline) >= result && x.Gender == gender;
                        break;
                    case "TwelveMinutesRun":
                        predicate = x => (float)GetPropertyValue(x, discipline) <= result && x.Gender == gender;
                        break;
                    default:
                        predicate = x => (int)GetPropertyValue(x, discipline) <= result && x.Gender == gender;
                        break;
                }

                // Act
                int resultPoints = calculator.CalculatePoints(predicate);

                // Assert
                Assert.AreEqual(expectedPoints, resultPoints);
            }
        }
    }
}
