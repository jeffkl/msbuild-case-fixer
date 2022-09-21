using System.CommandLine;

namespace MSBuildCaseFixer
{
    public static partial class Program
    {
        /// <summary>
        /// Executes the program with the specified arguments.
        /// </summary>
        /// <param name="args">An array of string representing the command-line arguments.</param>
        /// <returns>Zero if the program ran successfully, otherwise a non-zero number.</returns>
        public static int Main(string[] args)
        {
            RootCommand command = new RootCommand();

            Argument<string> projectArgument = new Argument<string>("project", description: "The path to the entry project to update.  This is usually a Visual Studio solution file or Traversal project that references all other projects in the repository.");

            projectArgument.SetDefaultValue(@"**\*proj|**\*sln");

            command.AddArgument(projectArgument);

            Option<string> rootOption = new Option<string>("--root", description: "The root folder of the repository so that imported build files can be updated.")
            {
                IsRequired = true
            };

            command.AddOption(rootOption);

            Option<bool> commitOption = new Option<bool>("--commit", description: "Specifies that the changes should be saved.");

            command.AddOption(commitOption);

            Option<bool> debugOption = new Option<bool>("--debug")
            {
                IsHidden = true
            };

            command.AddOption(debugOption);

            int exitCode = 0;

            command.SetHandler(
                (project, root, commit, debug) =>
                {
                    exitCode = Execute(project, root, commit, debug);
                },
                projectArgument,
                rootOption,
                commitOption,
                debugOption);

            command.Invoke(args);

            return exitCode;
        }
    }
}