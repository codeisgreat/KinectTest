using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;

namespace KinectTest
{
    class kinectInit
    {
        SkeletonData skeleton;
        Vector2 position,resolution;
        Joint rightHandJoint;

        public Vector2 Pos
        {
            get { return position; }
            set { position = value; }
        }
        public void initKinectNui(Runtime run)
        {
            run.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking);
            run.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);
            run.NuiCamera.ElevationAngle = 5;
            run.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
        }

        public void smoothKinectNui(Runtime run)
        {
            run.SkeletonEngine.TransformSmooth = true;
            TransformSmoothParameters p = new TransformSmoothParameters
            {
                Smoothing = 0.75f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            };
            run.SkeletonEngine.SmoothParameters = p;
        }

        public void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            foreach (SkeletonData s in e.SkeletonFrame.Skeletons)
            {
                if (s.TrackingState == SkeletonTrackingState.Tracked)
                {
                    resolution = new Vector2(640, 480);
                    skeleton = s;
                    rightHandJoint = skeleton.Joints[JointID.HandRight];
                    position = new Vector2((((0.5f * rightHandJoint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * rightHandJoint.Position.Y) + 0.5f) * (resolution.Y)));
                }               
            }
        }
    }
}
