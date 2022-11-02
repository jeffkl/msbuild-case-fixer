using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace MSBuildCaseFixer
{
    internal class MSBuildProjectCaseFixer
    {
        private readonly IFileSystem _fileSystem;
        private readonly IMSBuildProjectLoader _projectLoader;

        private readonly IDirectoryInfo _root;
        private ConcurrentDictionary<string, ConcurrentDictionary<string, MSBuildCaseFixerResult>>? _replacementsByFile;

        public MSBuildProjectCaseFixer(IMSBuildProjectLoader projectLoader, IDirectoryInfo root, IFileSystem fileSystem)
        {
            _projectLoader = projectLoader ?? throw new ArgumentNullException(nameof(projectLoader));
            _root = root;
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public void Run(IReadOnlyCollection<string> projectPaths, bool commitChanges)
        {
            Stopwatch sw = Stopwatch.StartNew();

            IReadOnlyCollection<IMSBuildProject> msbuildProjects;

            _replacementsByFile = new ConcurrentDictionary<string, ConcurrentDictionary<string, MSBuildCaseFixerResult>>(StringComparer.OrdinalIgnoreCase);

            Dictionary<string, string> globalProperties = new Dictionary<string, string>
            {
                ["ExcludeRestorePackageImports"] = bool.TrueString,
                ["TraversalTranslateProjectFileItems"] = bool.FalseString,
            };

            Stopwatch stopwatch = Stopwatch.StartNew();

            Console.Write(Strings.Message_LoadingProjects);

            _projectLoader.ProjectLoaded += (sender, project) =>
            {
                ProcessProject(project);
            };

            msbuildProjects = _projectLoader.Load(projectPaths, new Dictionary<string, string>());

            stopwatch.Stop();

            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, Strings.Message_LoadedProjects, msbuildProjects.Count, stopwatch.Elapsed.TotalSeconds));

            foreach (KeyValuePair<string, ConcurrentDictionary<string, MSBuildCaseFixerResult>> item in _replacementsByFile)
            {
                if (!item.Value.Any())
                {
                    continue;
                }

                Console.WriteLine("{0}", item.Key);

                foreach (KeyValuePair<string, MSBuildCaseFixerResult> value in item.Value)
                {
                    Console.WriteLine("  {0} => {1} [{2} / {3}]", value.Key, value.Value.CorrectedString, value.Value.ResultType, value.Value.ElementLocation);
                }

                Console.WriteLine();
            }

            if (commitChanges)
            {
                Stopwatch saveStopwatch = Stopwatch.StartNew();

                Console.Write("Committing changes...");
                foreach (KeyValuePair<string, ConcurrentDictionary<string, MSBuildCaseFixerResult>> fileForReplacement in _replacementsByFile)
                {
                    string fullPath = fileForReplacement.Key;
                    string contents;
                    Encoding encoding;

                    using (StreamReader reader = new StreamReader(fullPath, Encoding.Default, detectEncodingFromByteOrderMarks: true))
                    {
                        contents = reader.ReadToEnd();
                        encoding = reader.CurrentEncoding;
                    }

                    foreach (KeyValuePair<string, MSBuildCaseFixerResult> result in fileForReplacement.Value)
                    {
                        contents = contents.Replace(result.Value.IncorrectString, result.Value.CorrectedString);
                    }

                    using (StreamWriter writer = new StreamWriter(fullPath, append: false, encoding))
                    {
                        writer.Write(contents);
                        writer.Close();
                    }
                }

                saveStopwatch.Stop();

                Console.WriteLine("Done.  {0:N1}s", saveStopwatch.Elapsed.TotalSeconds);
            }

            // TODO: How to handle wildcards other than $(PkgSomething)\Content\Component\*.exe">
            // TODO: Better output

            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, Strings.Message_Complete, sw.Elapsed.TotalSeconds, Utility.CacheCount, Utility.FileCount));
        }

        /// <summary>
        /// Attempts to determine if the specified unevaluated value should be replaced with a value that has corrected case.
        /// </summary>
        /// <param name="unevaluated">An unevaluated value from an MSBuild project.</param>
        /// <param name="corrected">The value in the correct case according to the file system.</param>
        /// <param name="replacement">Receives a replacement value if one is found, otherwise <see cref="string.Empty" />.</param>
        /// <returns><c>true</c> if a replacement was found, otherwise <c>fasle</c>.</returns>
        internal static bool TryGetReplacement(string unevaluated, string corrected, out string replacement)
        {
            replacement = string.Empty;

            int unevaluatedIndex = unevaluated.Length - 1;
            int correctedIndex = corrected.Length - 1;

            int firstWildcardIndex = -1;

            int correctedWildcardIndex = -1;

            // Walk from the last character to the first looking for the first difference character different that is not just a different case
            for (; unevaluatedIndex >= 0 && correctedIndex >= 0; unevaluatedIndex--, correctedIndex--)
            {
                char unevaluatedChar = unevaluated[unevaluatedIndex];
                char correctedChar = corrected[correctedIndex];

                // If a wildcard is found, walk backwards until a directory separator is found
                if (unevaluatedChar == '*' && unevaluatedIndex >= 1)
                {
                    if (firstWildcardIndex != -1)
                    {
                        // Currently only support a single wildcard
                        return false;
                    }

                    // Keep track of where thew wildcard was at
                    firstWildcardIndex = unevaluatedIndex;

                    unevaluatedChar = unevaluated[--unevaluatedIndex];

                    while (correctedIndex >= 1 && correctedChar != Path.DirectorySeparatorChar)
                    {
                        correctedChar = corrected[--correctedIndex];
                    }

                    if (correctedIndex < 1)
                    {
                        return false;
                    }

                    correctedWildcardIndex = correctedIndex;
                }

                if (!char.ToLowerInvariant(unevaluatedChar).Equals(char.ToLowerInvariant(correctedChar)))
                {
                    if (correctedWildcardIndex != -1)
                    {
                        replacement = $"{unevaluated.Substring(0, unevaluatedIndex + 1)}{corrected.Substring(correctedIndex + 1, correctedWildcardIndex - correctedIndex)}{unevaluated.Substring(firstWildcardIndex)}";
                    }
                    else
                    {
                        // Once a difference is found, create a string by replacing that many characters in the unevaluated string
                        replacement = $"{unevaluated.Substring(0, unevaluatedIndex + 1)}{corrected.Substring(correctedIndex + 1)}";
                    }

                    // Only tell the caller that a replacement was made if not returning the exact same string
                    return !string.Equals(unevaluated, replacement);
                }
            }

            if (unevaluatedIndex < 0)
            {
                replacement = corrected.Substring(correctedIndex + 1);

                return !string.Equals(unevaluated, replacement);
            }

            return false;
        }

        private void ProcessProject(IMSBuildProject msbuildProject)
        {
            foreach (IMSBuildResolvedImport import in msbuildProject.Imports)
            {
                if (!import.EvaluatedProjectPath.StartsWith(_root.FullName))
                {
                    continue;
                }

                bool isCaseIncorrect = Utility.TryGetFullPathInCorrectCase(msbuildProject.DirectoryPath, import.EvaluatedProjectPath, out string? actual);

                if (!isCaseIncorrect || string.Equals(import.EvaluatedProjectPath, actual) || actual == null)
                {
                    continue;
                }

                if (!TryGetReplacement(import.UnevaluatedProjectPath, actual, out string replacement))
                {
                    continue;
                }

                ConcurrentDictionary<string, MSBuildCaseFixerResult> replacementsForThisFile = _replacementsByFile!.GetOrAdd(import.ContainingProject, (fullPath) => new ConcurrentDictionary<string, MSBuildCaseFixerResult>());

                replacementsForThisFile.TryAdd(
                    import.UnevaluatedProjectPath,
                    new MSBuildCaseFixerResult
                    {
                        CorrectedString = replacement,
                        ElementLocation = import.GetElementLocation(),
                        IncorrectString = import.UnevaluatedProjectPath,
                        ResultType = MSBuildCaseFixerResultType.Import
                    });
            }

            foreach (IMSBuildProjectItem item in msbuildProject.Items)
            {
                string projectFullPath = msbuildProject.FullPath;

                if (item.IsImported)
                {
                    if (!item.ProjectFullPath.StartsWith(_root.FullName))
                    {
                        continue;
                    }

                    projectFullPath = item.ProjectFullPath;
                }

                ConcurrentDictionary<string, MSBuildCaseFixerResult> replacementsForThisFile = _replacementsByFile!.GetOrAdd(projectFullPath, (fullPath) => new ConcurrentDictionary<string, MSBuildCaseFixerResult>());

                string fullPath = item.GetMetadataValue("FullPath");

                bool isCaseIncorrect = Utility.TryGetFullPathInCorrectCase(msbuildProject.DirectoryPath, fullPath, out string? actual);

                if (isCaseIncorrect && !string.Equals(fullPath, actual) && actual != null && TryGetReplacement(item.UnevaluatedInclude, actual, out string replacement))
                {
                    replacementsForThisFile.TryAdd(
                        item.UnevaluatedInclude,
                        new MSBuildCaseFixerResult
                        {
                            CorrectedString = replacement,
                            ElementLocation = item.GetElementLocation(),
                            IncorrectString = item.UnevaluatedInclude,
                            ResultType = MSBuildCaseFixerResultType.Item,
                        });
                }

                foreach (IMSBuildProjectItemMetadata metadatum in item.DirectMetadata)
                {
                    if (!File.Exists(metadatum.EvaluatedValue))
                    {
                        continue;
                    }

                    fullPath = metadatum.EvaluatedValue;

                    isCaseIncorrect = Utility.TryGetFullPathInCorrectCase(msbuildProject.DirectoryPath, metadatum.EvaluatedValue, out actual);

                    if (!isCaseIncorrect || actual == null || string.Equals(fullPath, actual))
                    {
                        continue;
                    }

                    if (!TryGetReplacement(metadatum.UnevaluatedValue, actual, out replacement))
                    {
                        continue;
                    }

                    replacementsForThisFile.TryAdd(
                        metadatum.UnevaluatedValue,
                        new MSBuildCaseFixerResult
                        {
                            CorrectedString = replacement,
                            ElementLocation = item.GetElementLocation(),
                            IncorrectString = metadatum.UnevaluatedValue,
                            ResultType = MSBuildCaseFixerResultType.ItemMetadata,
                        });
                }
            }
        }
    }
}