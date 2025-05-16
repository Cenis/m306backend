using System.Threading.Tasks;
using FitnessCheck.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FitnessCheck.Tests.Services
{
    [TestClass]
    public class RandomGenderServiceTests
    {
        private RandomGenderService _service;

        [TestInitialize]
        public void Initialize()
        {
            _service = new RandomGenderService();
        }

        [TestMethod]
        public async Task GetGenderAsync_ReturnsEitherMorF()
        {
            for (int i = 0; i < 10; i++)
            {
                char gender = await _service.GetGenderAsync("testUserId");
                Assert.IsTrue(gender == 'm' || gender == 'f', $"Unexpected gender '{gender}' returned.");
            }
        }
    }
}
