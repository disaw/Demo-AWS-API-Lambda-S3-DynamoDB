using System.Collections.Generic;
using Xunit;
using Domain.Helpers;
using FluentAssertions;

namespace UnitTests
{
    public class HelperTests
    {
        [Fact]
        public void GetMedian_ShouldReturnMedianOfDecimalNumbers_WhenOddCount()
        {
            var numbers = new List<decimal> { 4.5M, 2.5M, 7.0M, 12.5M, 0.25M, 123.6M, 23.6M, 1.4M, 6.7M, 8.9M, 5.678M };
            var expectedValue = 6.7M;

            var actualValue = Helper.GetMedian(numbers);

            actualValue.Should().Be(expectedValue);
        }

        [Fact]
        public void GetMedian_ShouldReturnMedianOfDecimalNumbers_WhenEvenCount()
        {
            var numbers = new List<decimal> { 4.5M, 2.5M, 7.0M, 12.5M, 0.25M, 123.6M, 23.6M, 1.4M, 6.7M, 8.9M };
            var expectedValue = (6.7M + 7.0M)/2;

            var actualValue = Helper.GetMedian(numbers);

            actualValue.Should().Be(expectedValue);
        }

        [Fact]
        public void GetMedian_ShouldReturnMedianOfDecimalNumbers_WhenNonDistinctNumbers()
        {
            var numbers = new List<decimal> { 4.5M, 2.5M, 7.0M, 12.5M, 0.25M, 123.6M, 23.6M, 1.4M, 6.7M, 8.9M, 6.7M, 0.25M, 0.25M };
            var expectedValue = (6.7M + 7.0M) / 2;

            var actualValue = Helper.GetMedian(numbers);

            actualValue.Should().Be(expectedValue);
        }
    }
}
