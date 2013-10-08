using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Configuration;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace HomeOS.Shared
{
    public class DiagnosticsHelper
    {
        // Trace sources - Think of these as multiple output channels. So write to the ConfigTrace when you're 
        // logging information about configuration, write to the WebTrace when you're logging information about
        // the web role activities themselves (e.g. page load, etc.).  You can add more sources if you want - they
        // give you more ability to control what is and isn't logged.  For instance, rather than a single WebTrace,
        // you might have a source per major component of your web app - Home page, administration, profile management,
        // etc.  Have a switch per Source so you can adjust the verbosity of logging per component.
        public TraceSource ConfigTrace;
        public TraceSource WebTrace;
        // Add additional sources here

        // Corresponding trace switches to control level of output for each source
        public SourceSwitch ConfigTraceSwitch { get; set; }
        public SourceSwitch WebTraceSwitch { get; set; }
        // Add additional switches 1:1 with trace sources here; hook them up in the ctor

        // Given a string representation of a tracing level, e.g. from the config file,
        // translate it to the SourceLevels enum needed.
        protected SourceLevels SourceLevelFromString(string str)
        {
            SourceLevels lvl;

            try
            {
                lvl = (SourceLevels)Enum.Parse(typeof(SourceLevels), str, true);
            }
            catch (System.ArgumentException)
            {
                // Invalid value - just default to off.
                lvl = SourceLevels.Off;
            }

            return lvl;
        }

        // Event types
        public enum TraceEventID
        {
            traceGeneral = 0,           // General type
            traceFunctionEntry = 1,
            traceFunctionExit = 2,      // Note that can see just entry / exit by querying eventId lt 3
            traceException = 100,
            traceUnexpected = 200,
            traceFlow = 300             // Can see entry / exit plus errors with eventId le 200
        };

        public DiagnosticsHelper()
        {
            ConfigTraceSwitch = new SourceSwitch("ConfigTrace", "Off");
            WebTraceSwitch = new SourceSwitch("WebTrace", "Off");
            ConfigTrace = new TraceSource("ConfigTrace");
            ConfigTrace.Switch = ConfigTraceSwitch;
            WebTrace = new TraceSource("WebTrace");
            WebTrace.Switch = WebTraceSwitch;
        }

        protected void ShowTraceSwitchValues(string prefixComponent)
        {
            if (ConfigTraceSwitch.Level != SourceLevels.Off)
            {
                Trace.WriteLine(prefixComponent + "::ShowTraceSwitchValues - ConfigTrace switch value=" +
                    ConfigTraceSwitch.Level.ToString());
            }
            else
            {
                Trace.WriteLine(prefixComponent + "::ShowTraceSwitchValues - ConfigTrace switch is Off. You can enable in configuration file.");
            }

            if (WebTraceSwitch.Level != SourceLevels.Off)
            {
                Trace.WriteLine(prefixComponent + "::ShowTraceSwitchValues - WebTrace switch value=" +
                    WebTraceSwitch.Level.ToString());
            }
            else
            {
                Trace.WriteLine(prefixComponent + "::ShowTraceSwitchValues - WebTrace switch is Off. You can enable in configuration file.");
            }
        }

        public void WriteDiagnosticInfo(TraceSource src, TraceEventType evtType, TraceEventID evtID, string msg)
        {
            if (src != null)
                src.TraceEvent(evtType, (int)evtID, msg);
        }

        public void WriteDiagnosticInfo(TraceSource src, TraceEventType evtType, TraceEventID evtID, string format, params object[] args)
        {
            if (src != null)
                src.TraceEvent(evtType, (int)evtID, format, args);
        }
    }

    public class StandaloneDiagnosticHost : DiagnosticsHelper
    {
        public StandaloneDiagnosticHost() : base() { }
    }

    // Helper class for diagnostic output from the Azure web role.

    public class AutoEnterExitTrace : IDisposable
    {
        private DiagnosticsHelper _wd;
        private TraceSource _ts;
        private string _class_method;
        public AutoEnterExitTrace(DiagnosticsHelper wd, TraceSource ts, string class_method)
        {
            _wd = wd;
            _ts = ts;
            _class_method = class_method;
            if (null != wd)
            {
                wd.WriteDiagnosticInfo(ts, TraceEventType.Verbose, DiagnosticsHelper.TraceEventID.traceFunctionEntry, string.Format("tid:{0} Entering {1}", Thread.CurrentThread.ManagedThreadId, class_method));
            }
        }

        ~AutoEnterExitTrace()
        {
        }

        public void Dispose() 
        {
            if (null != _wd)
            {
                _wd.WriteDiagnosticInfo(_ts, TraceEventType.Verbose, DiagnosticsHelper.TraceEventID.traceFunctionExit, string.Format("tid:{0} Leaving {1}", Thread.CurrentThread.ManagedThreadId, _class_method));
            }
        }

        public void WriteDiagnosticInfo(TraceEventType evtType, DiagnosticsHelper.TraceEventID evtID, string message, params object[] args)
        {
            if (null != _wd)
            {
                _wd.WriteDiagnosticInfo(_ts, evtType, evtID, "tid:" + Thread.CurrentThread.ManagedThreadId.ToString() + " " + _class_method + ":" + message, args);
            }
        }
    }

    public class WebRoleDiagnostics : DiagnosticsHelper
    {
        // This is  list of configuration settings (in ServiceConfiguration.cscfg) that if changed
        // should not result in the role being recycled.  This is implemented in the RoleEnvironmentChanging
        // and RoleEnvironmentChanged callbacks below
        private string[] ExemptConfigurations;

        public WebRoleDiagnostics(string[] exemptConfigs, bool standalone = false)
            : base()
        {
            ExemptConfigurations = exemptConfigs;
            GetTraceSwitchValuesFromRoleConfiguration(standalone);
            
        }

        public void OnStartSetup()
        {
            // Set up for change notifications if any of our configuration values change at run-time.
            // First Changing is called - this determines whether or not the role is recycled based on what
            // is changing.  Then Changed is called to read the new values.
            RoleEnvironment.Changing += RoleEnvironmentChanging;
            RoleEnvironment.Changed += RoleEnvironmentChanged;

            // This code was added to the standard OnStart to trigger transfer of Azure logs
            // to the Diagnostic Storage every one minute for the WebRole.
            // You can change the time interval and
            // level of verbosity of logs (e.g. for only errors) by adjusting the
            // DiagnosticMonitorConfiguration values below.  If you don't want to transfer logs to
            // storage at all, just call DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString") without
            // passing the updated DiagnosticMonitorConfiguration structure.

            DiagnosticMonitorConfiguration dmc = DiagnosticMonitor.GetDefaultInitialConfiguration();
            dmc.Logs.BufferQuotaInMB = 500;

            // Transfer logs to storage every minute
            dmc.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(1d);
            // Transfer verbose, critical, etc. logs
            dmc.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;

            WriteDiagnosticInfo(ConfigTrace, TraceEventType.Verbose, DiagnosticsHelper.TraceEventID.traceGeneral, "Setting up Diagnostics Monitor - transfer Verbose logs every minute.");

            // Start up the diagnostic manager with the given configuration.
            try
            {
                DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", dmc);
            }
            catch (Exception exp)
            {
                WriteDiagnosticInfo(ConfigTrace, TraceEventType.Critical, DiagnosticsHelper.TraceEventID.traceException, "DiagnosticsManager.Start threw an exception " + exp.ToString());
            }


            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += RoleEnvironmentChanging;

        }

        /// <summary>
        /// HasNonExemptConfigurationChanges - Check if config changes contain any that aren't on our "exempt from recycle" list
        /// Returns TRUE if there is at least one config change that isn't on our list.
        /// </summary>
        /// <param name="chgs">Collection of changes from RoleEnvironmentChanging or RoleEnvironmentChanged</param>
        /// <returns></returns>
        private bool HasNonExemptConfigurationChanges(ReadOnlyCollection<RoleEnvironmentChange> chgs)
        {
            Func<RoleEnvironmentConfigurationSettingChange, bool> changeIsNonExempt =
                    x => !ExemptConfigurations.Contains(x.ConfigurationSettingName);

            var environmentChanges = chgs.OfType<RoleEnvironmentConfigurationSettingChange>();

            return environmentChanges.Any(changeIsNonExempt);
        }


        /// <summary>
        /// RoleEventChanging - Called when a change is about to be applied to the role.  Determines whether or not to recycle the role instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">A list of what is changing</param>
        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // Note: e.Cancel == true -> Azure should recycle the role.  If all the changes are in our "exempt" list,
            // we don't need to recycle the role.

            e.Cancel = HasNonExemptConfigurationChanges(e.Changes);

            // Note that we use Trace.WriteLine here rather than going through the Diagnostics class so that we will always log
            // this, even when the switch for whether to log or not is being changed.

            if (!e.Cancel)
            {
                Trace.WriteLine("WebRoleDiagnostics::RoleEnvironmentChanging - role is not recycling, getting new switch values from config file.");
            }
            else
            {
                Trace.WriteLine("WebRoleDiagnostics::RoleEnvironmentChanging - recycling role instance due to non-exempt configuration changes.");
            }
        }

        /// <summary>
        /// RoleEnvironmentChanged - Called after a change has been applied to the role.  
        /// NOTE: This is called AFTER RoleEnvironmentChanging is called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">List of what has changed</param>
        private void RoleEnvironmentChanged(object sender, RoleEnvironmentChangedEventArgs e)
        {
            // Refresh the diagnostic switches from the role config values.
            // This allows for run-time changing of the values of the switches without recycling (i.e. rebooting)
            // the role so we can turn on or off more verbose diagnostic output based on the switches we've
            // defined in ServiceConfiguration.cscfg.
            GetTraceSwitchValuesFromRoleConfiguration();

            // Log the change to the logs - using Trace.WriteLine to circumvent the switches so that if the switch was
            // turned off or to a low setting like Critical, the message showing this will still go into the logs.
            Trace.WriteLine("WebRoleDiagnostics::RoleEnvironmentChanged - Diagnostics switch values changed.  ConfigTrace = " +
                                ConfigTrace.Switch.Level.ToString() + " WebTrace = " +
                                WebTrace.Switch.Level.ToString());
        }

        public void GetTraceSwitchValuesFromRoleConfiguration(bool standalone = false)
        {
            if (!standalone)
            {
                ConfigTraceSwitch.Level = SourceLevelFromString(RoleEnvironment.GetConfigurationSettingValue("ConfigTrace"));
                WebTraceSwitch.Level = SourceLevelFromString(RoleEnvironment.GetConfigurationSettingValue("WebTrace"));
                // Uses Trace.WriteLine so that the information goes through regardless of the switch values
            }

            base.ShowTraceSwitchValues(standalone ? "StandaloneDiagnostics" : "WebRoleDiagnostics");
        }

    }
}
