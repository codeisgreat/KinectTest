using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.IO;


namespace KinectTest
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        Runtime kinectSensor;
        SkeletonData skeleton;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Vector2 position, font_pos, resolution, hazard_pos;
        Texture2D kinectRGBVideo, controller, hazard;
        SpeechRec speech;
        Color[] controller_data, hazard_data;

        string message = "Collision: false";

        bool controllerHit = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
        }

        protected override void Initialize()
        {
            resolution = new Vector2(640, 480);
            hazard_pos = new Vector2(400, 150);

            kinectSensor = Runtime.Kinects[0];
            kinectSensor.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking);
            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);
            kinectSensor.NuiCamera.ElevationAngle = 5;
            kinectSensor.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
            kinectSensor.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(kinectSensor_VideoFrameReady);

            kinectSensor.SkeletonEngine.TransformSmooth = true;
            TransformSmoothParameters p = new TransformSmoothParameters
            {
                Smoothing = 0.75f,
                Correction = 0.0f,
                Prediction = 0.0f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            };
            kinectSensor.SkeletonEngine.SmoothParameters = p;

            speech = new SpeechRec();
            speech.initSpeech();
            base.Initialize();

        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Arial");

            kinectRGBVideo = new Texture2D(GraphicsDevice, 1337, 1337);

            controller = Content.Load<Texture2D>("reddot");
            hazard = Content.Load<Texture2D>("hazard");

            controller_data = new Color[controller.Width * controller.Height];
            controller.GetData(controller_data);

            hazard_data = new Color[hazard.Width * hazard.Height];
            hazard.GetData(hazard_data);

        }

        private void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            foreach (SkeletonData s in e.SkeletonFrame.Skeletons)
            {
                if (s.TrackingState == SkeletonTrackingState.Tracked)
                {
                    skeleton = s;
                    Joint rightHandJoint = skeleton.Joints[JointID.HandRight];
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

        protected override void UnloadContent()
        {
            kinectSensor.Uninitialize();
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {

            Rectangle controllerRectangle = new Rectangle((int)position.X, (int)position.Y, controller.Width, controller.Height);
            Rectangle hazardRectangle = new Rectangle((int)hazard_pos.X, (int)hazard_pos.Y, hazard.Width, hazard.Height);
            CollisionDetection collision = new CollisionDetection();
            if (collision.IntersectPixel(controllerRectangle, controller_data, hazardRectangle, hazard_data))
            {
                controllerHit = true;
            }
            else
            {
                controllerHit = false;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);
            spriteBatch.DrawString(font, message, font_pos = new Vector2(0, 430), Color.White);
            spriteBatch.DrawString(font, "Speech Recognition" + speech.returnMsg(), font_pos = new Vector2(0, 450), Color.White);
            spriteBatch.Draw(controller, position, Color.White);
            spriteBatch.Draw(hazard, hazard_pos, Color.White);

            if (controllerHit == true)
            {
                message = "Collision: true";
                hazard = Content.Load<Texture2D>("hazard_hit");
            }
            else
            {
                message = "Collision: false";
                hazard = Content.Load<Texture2D>("hazard");
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}