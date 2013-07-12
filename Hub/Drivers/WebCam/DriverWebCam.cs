using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.AddIn;
using System.Net;
using System.Threading;
using System.Drawing;
using HomeOS.Hub.Common;
using HomeOS.Hub.Common.WebCam.WebCamWrapper.Camera;
using HomeOS.Hub.Platform.Views;

//The WebCam library being used is documented here
//http://www.codeproject.com/KB/miscctrl/webcam_c_sharp.aspx

//the argument passed to this module should be a substring of the web camera name

namespace DriverWebCam
{
    [AddIn("HomeOS.Hub.Drivers.WebCam")]
    public class DriverWebCam : ModuleBase
    {
        CameraFrameSource _frameSource;
        Bitmap _latestFrame;
        byte[] _latestImageBytes = new byte[0];

        Port cameraPort;
        SafeThread worker = null;

        private WebFileServer imageServer;

        public override void Start()
        {
            if (moduleInfo.Args().Length == 0 || moduleInfo.Args()[0].Equals(""))
            {
                ListAvailableCameras();
                return;
            }

            string cameraStr = moduleInfo.Args()[0];

            foreach (Camera camera in CameraService.AvailableCameras)
            {
                //if (camera.ToString().ToLower().Contains(cameraStr))
                //{
                //    _frameSource = new CameraFrameSource(camera);
                //    break;
                //}

                if (cameraStr.ToLower().Contains(camera.ToString().ToLower()))
                {
                    _frameSource = new CameraFrameSource(camera);
                    break;
                }
            }

            if (_frameSource == null)
            {
                logger.Log("Camera matching {0} not found", cameraStr);
                ListAvailableCameras();
                return;
            }

            logger.Log("Will use camera {0}", _frameSource.Camera.ToString());


            //add the camera service port
            VPortInfo pInfo = GetPortInfoFromPlatform("webcam - " + cameraStr);

            List<VRole> roles = new List<VRole>() {RoleCamera.Instance};
            
            cameraPort = InitPort(pInfo);
            BindRoles(cameraPort, roles, OnOperationInvoke);

            RegisterPortWithPlatform(cameraPort);
            worker = new SafeThread(delegate()
            {
                GetVideo();
            }, "DriverWebCam-GetVideo", logger);
            worker.Start();

            imageServer = new WebFileServer(moduleInfo.BinaryDir(), moduleInfo.BaseURL(), logger);

        }

        private void ListAvailableCameras()
        {
            string str = "Available cameras: ";
            int index = 1;
            foreach (Camera camera in CameraService.AvailableCameras)
            {
                str += index + ". " + camera.ToString() + " ";
                index++;
            }

            logger.Log(str);
        }

        public override void Stop()
        {
            _frameSource.Camera.Dispose();
            if (worker != null)
                worker.Abort();
            imageServer.Dispose();
        }

        /// <summary>
        /// The demultiplexing routing for incoming
        /// </summary>
        /// <param name="message"></param>
        private List<VParamType> OnOperationInvoke(string roleName, String opName, IList<VParamType> parameters)
        {
            List<VParamType> ret = new List<VParamType>();

            switch (opName.ToLower())
            {
                //case RoleCamera.OpDownName:
                //case RoleCamera.OpUpName:
                //case RoleCamera.OpLeftName:
                //case RoleCamera.OpRightName:
                //case RoleCamera.OpZoomInName:
                //case RoleCamera.OpZommOutName:
                //    //nothing to be done here: web cameras do not support navigation
                //    break;
                case RoleCamera.OpGetImageName:
                    lock (this)
                    {
                        if (_latestImageBytes != null)
                        {
                            ret.Add(new ParamType(ParamType.SimpleType.jpegimage, _latestImageBytes));
                        }
                    }
                   break;
                default:
                    logger.Log("Unhandled camera operation {0}", opName);
                    break;
            }
            
            return ret;
        }

        private byte[] ImageToByteArray(Image image)
        {
            System.IO.MemoryStream memStream = new System.IO.MemoryStream();
            image.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);

            return memStream.ToArray();
        }

        private void GetVideo()
        {
            try
            {
                _frameSource.Camera.CaptureWidth = 320;
                _frameSource.Camera.CaptureHeight = 240;
                _frameSource.Camera.Fps = 20;

                _frameSource.NewFrame += OnImageCaptured;

                _frameSource.StartFrameCapture();
            }
            catch (Exception ex)
            {
                logger.Log("Error: Couldn't start frame capture on webcam {0}: {1}", _frameSource.Camera.ToString(), ex.ToString());
            }
        }

        public void OnImageCaptured(HomeOS.Hub.Common.WebCam.WebCamWrapper.Contracts.IFrameSource frameSource, 
                                    HomeOS.Hub.Common.WebCam.WebCamWrapper.Contracts.Frame frame, double fps)
        {
            List<VParamType> ret = new List<VParamType>();

            lock (this)
            {
                _latestFrame = frame.Image;

                var newImageBytes = ImageToByteArray(frame.Image);

                //make a copy, so we do not pass on this new object to the remote guys (which leads to higher memory consumption)

                if (_latestImageBytes.Length < newImageBytes.Length)
                {
                    _latestImageBytes = newImageBytes;
                }
                else
                {
                    Buffer.BlockCopy(newImageBytes, 0, _latestImageBytes, 0, newImageBytes.Length);
                }
            }
            
            ret.Add(new ParamType(ParamType.SimpleType.jpegimage, _latestImageBytes));
            

            cameraPort.Notify(RoleCamera.RoleName, RoleCamera.OpGetVideo, ret);
        }

        public override string GetDescription(string hint)
        {
            return "WebCam";
        }

        public override string ToString()
        {
            return "DriverWebCam-" + String.Join(",", moduleInfo.Args());
        }

        // ... we don't care about other people's ports
        public override void PortRegistered(VPort port) { }
        public override void PortDeregistered(VPort port) { }

    }
}
