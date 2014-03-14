using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeOS.Shared
{
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

    public interface IDiagnostics
    {
        void WriteDiagnosticInfo(TraceSource src, TraceEventType evtType, TraceEventID evtID, string msg);

        void WriteDiagnosticInfo(TraceSource src, TraceEventType evtType, TraceEventID evtID, string format, params object[] args);
    }
}
