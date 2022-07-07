using EnvironmentAbstractions.TestHelpers;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using Xunit.Abstractions;

namespace MSBuildCaseFixer.UnitTests
{
    public class ProgramTests : TestBase
    {
        public ProgramTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void TryFindOnPathTest()
        {
            IEnvironmentVariableProvider environmentVariableProvider = new MockEnvironmentVariableProvider
            {
                ["PATH"] = @"C:\App1;C:\App2;;C:\App4;;     ;C:\App3"
            };

            IFileSystem fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                [@"C:\App1"] = new MockDirectoryData(),
                [@"C:\App2"] = new MockDirectoryData(),
                [@"C:\App3"] = new MockDirectoryData(),
                [@"C:\App3\app.exe"] = new MockFileData(string.Empty),
            });

            Utility.TryFindOnPath("app.exe", validator: null, environmentVariableProvider, fileSystem, out IFileInfo? fileInfo).ShouldBeTrue();

            fileInfo.ShouldNotBeNull();

            fileInfo.Exists.ShouldBeTrue();

            fileInfo.FullName.ShouldBe(@"C:\App3\app.exe");
        }
    }
}