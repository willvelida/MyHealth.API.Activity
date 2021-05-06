using MyHealth.API.Activity.Validators;
using Xunit;

namespace MyHealth.API.Activity.UnitTests.ValidatorTests
{
    public class DateValidatorShould
    {
        private DateValidator _sut;

        public DateValidatorShould()
        {
            _sut = new DateValidator();
        }

        [Fact]
        public void ReturnFalseIfActivityDateIsNotInValidFormat()
        {
            // Arrange
            string testActivityDate = "100/12/2021";

            // Act
            var response = _sut.IsActivityDateValid(testActivityDate);

            // Assert
            Assert.False(response);
        }

        [Fact]
        public void ReturnTrueIfActivityDateIsInValidFormat()
        {
            // Arrange
            string testActivityDate = "2020-12-31";

            // Act
            var response = _sut.IsActivityDateValid(testActivityDate);

            // Assert
            Assert.True(response);
        }
    }
}
