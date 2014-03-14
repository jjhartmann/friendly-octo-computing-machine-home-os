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
    public class DiagnosticsHelper : IDiagnostics
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
        private IDiagnostics _wd;
        private TraceSource _ts;
        private string _class_method;
        public AutoEnterExitTrace(IDiagnostics wd, TraceSource ts, string class_method)
        {
            _wd = wd;
            _ts = ts;
            _class_method = class_method;
            if (null != wd)
            {
                wd.WriteDiagnosticInfo(ts, TraceEventType.Verbose, TraceEventID.traceFunctionEntry, string.Format("tid:{0} Entering {1}", Thread.CurrentThread.ManagedThreadId, class_method));
            }
        }

        ~AutoEnterExitTrace()
        {
        }

        public void Dispose() 
        {
            if (null != _wd)
            {
                _wd.WriteDiagnosticInfo(_ts, TraceEventType.Verbose, TraceEventID.traceFunctionExit, string.Format("tid:{0} Leaving {1}", Thread.CurrentThread.ManagedThreadId, _class_method));
            }
        }

        public void WriteDiagnosticInfo(TraceEventType evtType, TraceEventID evtID, string message, params object[] args)
        {
            if (null != _wd)
            {
                _wd.WriteDiagnosticInfo(_ts, evtType, evtID, "tid:" + Thread.CurrentThread.ManagedThreadId.ToString() + " " + _class_method + ":" + message, args);
            }
        }
    }
}
