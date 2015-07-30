using System.Collections;
using System.Diagnostics;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Utility class containing utility methods for ProcessStartInfo
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public static class ProcessStartInfoEx
    {
        public static ProcessStartInfo Clone(this ProcessStartInfo info)
        {
            Utility.Code.Require(info, "info");

            ProcessStartInfo clone = new ProcessStartInfo
            {
                FileName = info.FileName,
                Arguments = info.Arguments,
                CreateNoWindow = info.CreateNoWindow,
                WindowStyle = info.WindowStyle,
                WorkingDirectory = info.WorkingDirectory,
                Domain = info.Domain,
                LoadUserProfile = info.LoadUserProfile,
                UserName = info.UserName,
                Password = info.Password,
                ErrorDialog = info.ErrorDialog,
                ErrorDialogParentHandle = info.ErrorDialogParentHandle,
                Verb = info.Verb,
                UseShellExecute = info.UseShellExecute,
                RedirectStandardError = info.RedirectStandardError,
                RedirectStandardInput = info.RedirectStandardInput,
                RedirectStandardOutput = info.RedirectStandardOutput,
                StandardErrorEncoding = info.StandardErrorEncoding,
                StandardOutputEncoding = info.StandardOutputEncoding
            };

            foreach (DictionaryEntry entry in info.EnvironmentVariables)
            {
                string variable = entry.Key.ToString();

                if (!clone.EnvironmentVariables.ContainsKey(variable))
                {
                    clone.EnvironmentVariables.Add(variable, entry.Value.ToString());
                }
            }

            return clone;
        }

        public static ProcessStartInfo StartThroughCmdShell(ProcessStartInfo info)
        {
            Utility.Code.Require(info, "info");

            string fileName = info.FileName;
            string arguments = info.Arguments;

            info.FileName = "cmd.exe";
            info.Arguments = "/S /C \"\"" + fileName + "\" " + arguments + '"';

            return info;
        }
    }
}
