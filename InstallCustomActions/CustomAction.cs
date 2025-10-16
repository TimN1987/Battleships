using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WixToolset.Dtf.WindowsInstaller;

namespace InstallCustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult InstallSetUp()
        {
            const string source = "BattleshipsInstaller";

            var eventSetUpSuccessful = RegisterEventSource();

            if (eventSetUpSuccessful != ActionResult.Success)
                return eventSetUpSuccessful;

            try
            {
                EventLog.WriteEntry(source, "Starting installation custom actions...", EventLogEntryType.Information);

                DatabaseSetUp.InitializeDatabase();
                EncryptionSetUp.GenerateEncryptionKey();

                EventLog.WriteEntry(source, "Installation custom actions completed successfully.", EventLogEntryType.Information);
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(source, $"An error occurred during installation: {ex.Message}", EventLogEntryType.Error);
                return ActionResult.Failure;
            }
        }

        private static ActionResult RegisterEventSource()
        {
            const string source = "BattleshipsInstaller";
            const string logName = "Application";

            try
            {
                if (!EventLog.SourceExists(source))
                    EventLog.CreateEventSource(source, logName);

                if (!EventLog.SourceExists("BattleshipsApp"))
                    EventLog.CreateEventSource("BattleshipsApp", "BattleshipsAppLog");

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(source, $"Event sources could not be registered: {ex.Message}", EventLogEntryType.Error);
                return ActionResult.Failure;
            }
        }
    }
}
