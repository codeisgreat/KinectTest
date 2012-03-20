using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectTest
{
    class LoadKinect
    {
        SkeletonData skeleton;
        GraphicsDeviceManager graphics;
        Vector2 position,resolution;
        Joint rightHandJoint;
        public Texture2D kinectRGBVideo;
       // SpriteBatch spriteBatch;

        public Vector2 Pos
        {
            get { return position; }
            set { position = value; }
        }

        public LoadKinect(GraphicsDeviceManager graphicsIn)
        {
            this.graphics = graphicsIn;
        }

        public void loadKinectNui(Runtime run)
        {
            run.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking);
            run.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);
            run.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(kinectSensor_VideoFrameReady);
            run.NuiCamera.ElevationAngle = -2;
            run.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution1280x1024, ImageType.Color);
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

        private void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
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

        private void kinectSensor_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage p = e.ImageFrame.Image;

            Color[] color = new Color[p.Height * p.Width];
            kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, p.Width, p.Height);

            int index = 0;
            for (int y = 0; y < p.Height; y++)
            {
                for (int x = 0; x < p.Width; x++, index += 4)
                {
                    color[y * p.Width + x] =
                    new Color(p.Bits[index + 2], p.Bits[index + 1], p.Bits[index + 0]);
                }
            }
            kinectRGBVideo.SetData(color);
        }

        public void loadKinectVideo(GraphicsDevice g, int h, int w)
        {
            kinectRGBVideo = new Texture2D(g, h, w);
        }

      /*  public void renderKinectVideo()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 512, 512), Color.White);
            spriteBatch.End();
        }*/
    }
}
