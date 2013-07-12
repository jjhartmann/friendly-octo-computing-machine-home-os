# HomeOS setup (checks watchdog service status, stop/start service, copy binaries from network build drop location
# to the Hub c:\homeos2\hub\output location
Set-ExecutionPolicy Unrestricted

# Variables
$servicename = "HomeOS Hub Watchdog"
$processname = "HomeOS.Hub.Platform"
$base = "\\vibe10\PublicAll\HomeOS\HomeOS-DailyBuild"
$sourcepath
$destpath = "C:\HomeOS2\Hub\Output"
$insttoolpath = "$env:windir" + "\Microsoft.NET\Framework\v4.0.30319"   #parh to InstallUtil.exe tool
$cred = Get-Credential
$domain = "redmond"
$mapdrive = "x:"
$isserviceinstalled 
$isservicerunning

#-----------------------------------------------------------------------------------------------------
#Setting credentials for network share access
$net = New-Object -com WScript.Network
$user = $cred.UserName

#clean previously used map drive if active
$driveletterexists = Test-Path $mapdrive
if($driveletterexists -eq $True)
{
   $net.RemoveNetworkDrive($mapdrive, "true", "true") 
}
 
$net.mapnetworkdrive($mapdrive, $base, "true", $user, $cred.GetNetworkCredential().Password)
#Calculate lkg folder name for the daily build, then append hub folder name as a part of a path name
$latest = (Get-ChildItem $base | ? {$_.PSIsContainer} | sort CreationTime -desc | select -f 1).Name 

#Construct the complete path
$sourcepath = $base + '\' + $latest + '\Hub'
#-----------------------------------------------------------------------------------------------------

#Check user credentials
#function CheckCredentials()
#{
#$username = $cred.username
#$password = $cred.GetNetworkCredential().password

# Get current domain using logged-on user's credentials
#$log_domain = $username.Split("\")
#$log_domain = $log_domain[0]

#if ($log_domain -ne $domain) 
#{
 #write-host "Authentication failed - please verify your domain\username and password."
 #$cred = Get-Credential
 #exit #terminate the script.
#}
#else
#{
# write-host "Successfully authenticated with domain $domain"
#}
#}



#Check if the InstallUtil.ex exists in the $insttoolpath path, exit if not
if((Test-Path "$insttoolpath\InstallUtil.exe") -ne $True)
{
  Write-Host "Could not find the InstallUtil.exe file in the $insttoolpath path - exiting script!"
  Exit 
}


# Verify is HomeOS Hub Watchdog service installed 
# argument - string (service name)
# return - True - installed, False - otherwise 
function IsServiceInstalled($servicename)
{
    Write-Host "Verifying is '$servicename' service installed ..."
    Write-Host "                                            "
    
    $service = Get-WmiObject -Class Win32_Service -Filter "Name='$servicename'"

    if($service)
    { 
        Write-Host "Watchdog service is already installed on your system"
        Write-Host "                                                    "
        $isserviceinstalled = $True
        return $True
    } 
    else
    {
        Write-Host "Watchdog service is not installed on your system"
        Write-Host "                                            "
        $isserviceinstalled = $False
        return $False   
    }
}

#install service
function InstallService($servicename)
{
   Write-Host "Trying to install $servicename service"
   
   #Invoke-Command -ScriptBlock {"$insttoolpath + '\InstallUtil.exe' /username = $user /password = $cred.GetNetworkCredential().Password /install $destpath + '\homeos.hub.watchdog.exe'"}

   $installUtil = join-path $env:SystemRoot Microsoft.NET\Framework\v4.0.30319\installutil.exe
   $serviceExe = join-path $destpath HomeOS.Hub.Watchdog.exe
   $installUtilLog = join-path $destpath InstallUtil.log
   & $installUtil $serviceExe /logfile="$installUtilLog" | write-verbose

   $service = Get-WmiObject -Class Win32_Service -Filter "Name = '$servicename'"

   # change credentials if necessary
   if ($user -ne "" -and $password -ne "")
       { $service.change($null, $null, $null, $null, $null, $null, $user, $password, $null, $null, $null) | out-null }
}

#delete existing service
function UninstallService($servicename)
{
   Write-Host "Trying to uninstall $servicename sevice ..."
   
   #Invoke-Command -ScriptBlock {"$insttoolpath + '\InstallUtil.exe' /username = $user /password = $cred.GetNetworkCredential().Password /uninstall $destpath + '\homeos.hub.watchdog.exe'"}

   
   $service = Get-WmiObject -Class Win32_Service -Filter "Name = '$servicename'"
   if ($service -ne $null) 
   { 
	$service | stop-service
        $service.Delete() | out-null 
   }
}

# Verify is HomeOS.Hub.Platform service running 
function IsServiceRunning($servicename)
{
   Write-Host "Verifying status of the $servicename service ..." 
   Write-Host "                                                "

   $arrService = Get-Service -Name $servicename   
   if ($arrService.Status -eq "Running")
   {
        Write-Host "$servicename service is running"
        Write-Host "                               "
        $isservicerunning = $True
        return $True
   }
   else
   {
        Write-Host "$servicename service is not running"
        Write-Host "                                   "
        $isservicerunning = $False
        return $False
   }
}

function StartService($servicename)
{
    Write-Host "Trying to start Watchdog service ..."
    Write-Host "                                    "
    Start-Service $ServiceName

    $arrService = Get-Service -Name $ServiceName
    if ($arrService.Status -eq "Running")
    {   
        Write-Host "$servicename service was successfully started"
	Write-Host "                                             "
    }
    else
    {
        Write-Host "Could not start the $servicename service :("
        Exit
    }
}


function StopService($servicename)
{
    Write-Host "Trying to stop Watchdog service ..."
    Write-Host "                                   "
    Stop-Service $ServiceName

    $arrService = Get-Service -Name $ServiceName
    if ($arrService.Status -ne "Running")
    {   
        Write-Host "Watchdog service was successfully stopped"
	Write-Host "                                         "
    }
    else
    {
        Write-Host "Could not stop the Watchdog service :("
        Exit
    }
}


function KillProcess($processname)
{
    Write-Host "Looking for the running $processname process"  
    $isRunning = (Get-Process | Where-Object {$_.Name -eq $ProcessName}).Count -gt 0

    if($isRunning)
    {
        Write-Host "Found the $processname process, trying to stop it..."
 
        Stop-Process -name $processname -Force -passthru | wait-process | out-null
     
        $isRunning = (Get-Process | Where-Object {$_.Name -eq $ProcessName}).Count -gt 0
        if(!$isRunning)
        { 
           Write-Host "HomeOS.Hub.Platform process was stopped successfully"
           Write-Host "                                                    "
        }
        else
        {
           Write-Host "Could not stop HomeOS.Hub.Platform process"
        } 
    }
    
    {
       Write-Host "Could not find the HomeOS.Hub.Platform process"
    }    
}

#-------------------------------------------------------------------------------------------------

# Program steps

#$res = CheckCredentials

#Set execution policy for PowerShell script
#This happens while running SetPolicies batch file 

#Stop and uninstall service

if(IsServiceRunning($servicename))
{
   StopService($servicename)
   UninstallService($servicename)
}

#Kill process if it is running..
KillProcess($processname)

#Uninstall HomeOSDashboard app
if(Get-AppxPackage -Name 7f4abba6-d73b-4a1d-9dbb-4bd2b237024c)
{
    Write-Host "Found package 7f4abba6-d73b-4a1d-9dbb-4bd2b237024c"
    Write-Host "Removing this package"
    Remove-AppxPackage -Package 7f4abba6-d73b-4a1d-9dbb-4bd2b237024c_1.0.0.6_x86__s28y1v82wsfxy
    #Verify here
}


#Write-Host 'Removing previous HomeOS installation files...'
#Remove-Item $destpath\* -Force -Recurse -ErrorAction SilentlyContinue
Write-Host "                              "

Write-Host "Copying files from network build drop location..." -foregroundcolor green -backgroundcolor black
robocopy $sourcepath $destpath /E /IS /XD "$destpath\Config"

Write-Host "Releasing network resourses..."
Write-Host "                              "
$net.RemoveNetworkDrive("x:", "true", "true")

#Install Watchdog service if it not installed yet
if(!(IsServiceInstalled($servicename)))
{
   InstallService($servicename)
}

#Starting Watchdog service...
StartService($servicename)

IsServiceRunning($servicename)

#Verify Platform process is running

#Install DashboardHomeOS application package
if((Test-Path "$destpath\Dashboard\AppPackages\Dashboard_1.0.0.6_x86_Debug_Test\Add-AppDevPackage.ps1") -eq $True)
{
   if(!(Get-AppxPackage -Name 7f4abba6-d73b-4a1d-9dbb-4bd2b237024c))
   {
       Write-Host "Trying to install DashboardHomeOS app package"
       C:\homeos2\Hub\Output\Dashboard\AppPackages\Dashboard_1.0.0.6_x86_Debug_Test\Add-AppDevPackage.ps1
       Write-Host "                                       " 
       #Write-Host "Starting DashboardHomeOS application..."
       #C:\homeos2\Hub\Output\HomeOS.Hub.Dashboard.Launcher.exe	
   }
}
else
{
	Write-Host "Cannot find Add-AppDevPackage.ps1 file"
}








