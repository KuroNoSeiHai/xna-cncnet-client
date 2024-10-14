using System.IO;
using Rampastring.Tools;
using ClientCore;
using ClientCore.PlatformShim;

namespace DTAClient.Domain
{
    public static class FinalSunSettings
    {
        /// <summary>
        /// Checks for the existence of the FinalSun settings file and writes it if it doesn't exist.
        /// </summary>
        public static void WriteFinalSunIni()
        {
            // The encoding of the FinalSun/FinalAlert ini file should be legacy ANSI, not Windows-1252 and also not any specific encoding.
            // Otherwise, the map editor will not work in a non-ASCII path. ANSI doesn't mean a specific codepage,
            // it means the default non-Unicode codepage which can be changed from Control Panel.
            try
            {
                string finalSunIniPath = ClientConfiguration.Instance.FinalSunIniPath;
                var finalSunIniFile = new FileInfo(Path.Combine(ProgramConstants.GamePath, finalSunIniPath));

                Logger.Log("Checking for the existence of FinalSun.ini.");
                if (finalSunIniFile.Exists)
                {
                    Logger.Log("FinalSun settings file exists.");

                    IniFile iniFile = new IniFile();
                    iniFile.FileName = finalSunIniFile.FullName;
                    iniFile.Encoding = EncodingExt.ANSI;
                    iniFile.Parse();

                    iniFile.SetStringValue("General", "GameDirectory", SafePath.CombineDirectoryPath(ProgramConstants.GamePath));
                    iniFile.WriteIniFile();

                    return;
                }

                Logger.Log("FinalSun.ini doesn't exist - writing default settings.");

                if (!finalSunIniFile.Directory.Exists)
                    finalSunIniFile.Directory.Create();

                using var sw = new StreamWriter(finalSunIniFile.FullName, false, EncodingExt.ANSI);

                sw.WriteLine("[General]");
                sw.WriteLine("GameDirectory=" + SafePath.CombineDirectoryPath(ProgramConstants.GamePath));
                sw.WriteLine("");
            }
            catch
            {
                Logger.Log("An exception occurred while checking the existence of FinalSun settings");
            }
        }
    }
}