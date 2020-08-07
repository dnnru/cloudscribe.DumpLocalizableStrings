#region Usings

using CommandLine;

#endregion

namespace cloudscribe.DumpLocalizableStrings
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Set input directory.")]
        public string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "Set output directory.")]
        public string Output { get; set; }
    }
}
