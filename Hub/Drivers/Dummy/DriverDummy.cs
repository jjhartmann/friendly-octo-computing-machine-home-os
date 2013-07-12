using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;
using System.Threading;

namespace HomeOS.Hub.Drivers.Dummy
{

    /// <summary>
    /// A dummy driver module that 
    /// 1. opens and register a dummy ports
    /// 2. sends periodic notifications  (in Work())
    /// 3. sends back responses to received echo requests (in OnOperationInvoke())
    /// </summary>

    [System.AddIn.AddIn("HomeOS.Hub.Drivers.Dummy", Version="1.0.0.0")]
    public class DriverDummy :  ModuleBase
    {
        SafeThread workThread = null; 
        Port dummyPort;

        public override void Start()
        {


            logger.Log("Started: {0}", ToString());
        
            // ..... initialize the list of roles we are going to export
            List<VRole> listRole = new List<VRole>() {RoleDummy.Instance};
            
            //.................instantiate the port
            VPortInfo portInfo = GetPortInfoFromPlatform("port");
            dummyPort = InitPort(portInfo);

            //..... bind the port to roles and delegates
            BindRoles(dummyPort, listRole);

            //.................register the port after the binding is complete
            RegisterPortWithPlatform(dummyPort);

            //Work();

            //***
            workThread = new SafeThread(delegate() { Work(); } , "DriverDummy work thread" , logger);
            workThread.Start();
            //***
        }

        public override void Stop()
        {
            if(workThread!=null)
                workThread.Abort();
            logger.Log("Stop() at {0}", ToString());

        }


        /// <summary>
        /// Sit in a loop and send notifications 
        /// </summary>
        public void Work()
        {
            int counter = 0;
            while (true)
            {
                counter++;

                //IList<VParamType> retVals = new List<VParamType>() { new ParamType(counter) };

                //dummyPort.Notify(RoleDummy.RoleName, RoleDummy.OpEchoSubName, retVals);

                Notify(dummyPort, RoleDummy.Instance, RoleDummy.OpEchoSubName, new ParamType(counter));

                System.Threading.Thread.Sleep(1 * 5 * 1000);
            }
        }

        /// <summary>
        /// The demultiplexing routing for incoming operations
        /// </summary>
        /// <param name="message"></param>
        public override IList<VParamType> OnInvoke(string roleName, String opName, IList<VParamType> args)
        {

            if (!roleName.Equals(RoleDummy.RoleName))
            {
                logger.Log("Invalid role {0} in OnInvoke", roleName);
                return null;
            }

            switch (opName.ToLower())
            {
                case RoleDummy.OpEchoName:
                    int payload = (int)args[0].Value();
                    logger.Log("{0} Got EchoRequest {1}", this.ToString(), payload.ToString());

                    return new List<VParamType>() {new ParamType(-1 * payload)};

                default:
                    logger.Log("Invalid operation: {0}", opName);
                    return null;
            }
        }

        /// <summary>
        ///  Called when a new port is registered with the platform
        ///        the dummy driver does not care about other ports in the system
        /// </summary>
        public override void PortRegistered(VPort port) {}

        /// <summary>
        ///  Called when a new port is deregistered with the platform
        ///        the dummy driver does not care about other ports in the system
        /// </summary>
        public override void PortDeregistered(VPort port) { }
    }
}