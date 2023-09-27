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
    internal struct MSBuildCaseFixerResult
    {
        public string CorrectedString { get; set; }

        public string ElementLocation { get; set; }

        public string IncorrectString { get; set; }

        public MSBuildCaseFixerResultType ResultType { get; set; }
    }
}