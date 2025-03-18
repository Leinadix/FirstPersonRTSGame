using Xunit;
using System;

namespace FirstPersonRTSGame.Tests
{
    public class BasicTests
    {
        [Fact]
        public void BasicTest_ShouldPass()
        {
            // This test should always pass
            Assert.True(true);
        }
        
        [Fact]
        public void MathTest_Addition()
        {
            // Basic math test
            int result = 2 + 2;
            Assert.Equal(4, result);
        }
    }
} 