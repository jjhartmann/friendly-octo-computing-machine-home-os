using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using HomeOS.Hub.Platform.Views;

namespace HomeOS.Hub.Drivers.ZwaveZensys
{
    public class UsbNotifier
    {

        ManagementEventWatcher insertWatcher = null;
        ManagementEventWatcher removeWatcher = null;

        VLogger logger;

        public UsbNotifier(VLogger logger)
        {
            this.logger = logger;
        }


        public void AddRemoveUSBHandler(EventArrivedEventHandler usbRemoved)
        {

            if (removeWatcher == null)
            {
                WqlEventQuery q;
                ManagementScope scope = new ManagementScope("root\\CIMV2");
                scope.Options.EnablePrivileges = true;

                q = new WqlEventQuery();
                q.EventClassName = "__InstanceDeletionEvent";
                q.WithinInterval = new TimeSpan(0, 0, 3);
                q.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";
                removeWatcher = new ManagementEventWatcher(scope, q);
            }

            removeWatcher.EventArrived += new EventArrivedEventHandler(usbRemoved);
            removeWatcher.Start();

        }

        public void AddInsertUSBHandler(EventArrivedEventHandler usbAdded)
        {
            if (insertWatcher == null)
            {
                WqlEventQuery q;
                ManagementScope scope = new ManagementScope("root\\CIMV2");
                scope.Options.EnablePrivileges = true;

                q = new WqlEventQuery();
                q.EventClassName = "__InstanceCreationEvent";
                q.WithinInterval = new TimeSpan(0, 0, 3);
                q.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";
                insertWatcher = new ManagementEventWatcher(scope, q);
            }

            insertWatcher.EventArrived += new EventArrivedEventHandler(usbAdded);
            insertWatcher.Start();
        }

        public void DeleteInsertUSBHandler(EventArrivedEventHandler usbAdded)
        {
            if (insertWatcher == null)
                throw new Exception("Insert watcher was never added. Cannot delete.");

            insertWatcher.EventArrived -= usbAdded;
        }

        public void DeleteInsertUSBHandler()
        {
            if (insertWatcher == null)
                throw new Exception("Insert watcher was never added. Cannot delete.");

            insertWatcher.Stop();
        }

    }
}
