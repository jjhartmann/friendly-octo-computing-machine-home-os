rem This command script is run at role start time.
rem See ServiceDefinition.csdef for invocation details.

rem Set application pool idle timeout value to never time out idle worker processes.
%SystemRoot%\system32\inetsrv\appcmd set config -section:applicationPools -applicationPoolDefaults.processModel.idleTimeout:00:00:00

rem Set application pool to never recycle just because some arbitrary amount of time passed.
%SystemRoot%\system32\inetsrv\appcmd set config -section:applicationPools -applicationPoolDefaults.recycling.periodicRestart.time:00:00:00

rem Set application pool to auto-start.
%SystemRoot%\system32\inetsrv\appcmd set config -section:applicationPools -applicationPoolDefaults.autoStart:true
%SystemRoot%\system32\inetsrv\appcmd set config -section:applicationPools -applicationPoolDefaults.startMode:AlwaysRunning

rem Open the firewall sufficiently for the ports we want to use.
rem "Full IIS" mode doesn't work without this, as the firewall rules Azure adds
rem (based off ServiceDefinition.csdef) are too restrictive for the "Full IIS"
rem process model.
%SystemRoot%\system32\netsh advfirewall firewall add rule name="Gatekeeper Home Service" dir=in action=allow protocol=tcp localport=5002
%SystemRoot%\system32\netsh advfirewall firewall add rule name="Gatekeeper Home Service" dir=in action=allow protocol=tcp localport=51431
