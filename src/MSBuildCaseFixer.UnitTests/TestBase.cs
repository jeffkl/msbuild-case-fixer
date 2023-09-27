using Xunit.Abstractions;

namespace MSBuildCaseFixer.UnitTests
{
    public abstract class TestBase
    {
        public TestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public ITestOutputHelper OutputHelper { get; }
    }
}