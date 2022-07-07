using Microsoft.IO;
using Xunit.Abstractions;

namespace MSBuildCaseFixer.UnitTests
{
    public abstract class TestBase
    {
        public TestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
            FileInfo a = new FileInfo("asdf");
        }

        public ITestOutputHelper OutputHelper { get; }
    }
}