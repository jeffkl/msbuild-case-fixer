using System;
using System.Collections.Concurrent;

namespace MSBuildCaseFixer
{
    internal enum MSBuildCaseFixerResultType
    {
        None = 0,
        Import,
        Item,
        ItemMetadata
    }

    /// <summary>
    /// Represents a result from analyzing an MSBuild project for a string that is using the incorrect case.
    /// </summary>
    internal class MSBuildCaseFixerResult
    {
        // Results by full path to project
        //   Results by string to be replaced
        //    - Location
        //    - Replacement
        //    - Type

        private ConcurrentDictionary<string, ConcurrentDictionary<string, MSBuildCaseFixerResult>> replacementsByFile = new ConcurrentDictionary<string, ConcurrentDictionary<string, MSBuildCaseFixerResult>>(StringComparer.OrdinalIgnoreCase);

        public string CorrectedString { get; set; } = string.Empty;

        public string ElementLocation { get; set; } = string.Empty;

        public string IncorrectString { get; set; } = string.Empty;

        public MSBuildCaseFixerResultType ResultType { get; set; } = MSBuildCaseFixerResultType.None;
    }
}