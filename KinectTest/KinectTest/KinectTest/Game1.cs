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


namespace KinectTest
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Runtime kinectSensor;
        Texture2D kinectRGBVideo,overlay,redDot;
        Vector2 position;
        Vector2 resolution;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Set up kinect and initialize it to use colour
            kinectSensor = Runtime.Kinects[0];
            kinectSensor.Initialize(RuntimeOptions.UseColor | RuntimeOptions.UseSkeletalTracking);
            resolution = new Vector2(640,480);
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


            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);
            kinectSensor.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
            kinectSensor.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(kinectSensor_VideoFrameReady);
            //TiltKinectUp(5);
            base.Initialize();
            
        }
        
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            kinectRGBVideo = new Texture2D(GraphicsDevice, 1337, 1337);
            
            // TODO: use this.Content to load your game content here
            overlay = Content.Load<Texture2D>("overlay");
            redDot = Content.Load<Texture2D>("reddot");

            
        }
        void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            

            try
            {
                SkeletonFrame allSkeletons = e.SkeletonFrame;
                SkeletonData playerSkeleton = (from s in allSkeletons.Skeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                Joint rightHandJoint = playerSkeleton.Joints[JointID.HandRight];
                position = new Vector2((((0.5f * rightHandJoint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * rightHandJoint.Position.Y) + 0.5f) * (resolution.Y)));
            }
            catch
            {
                Console.WriteLine("Holy Gawd i broke it!!!");
            }

        }
        //pass a value to tilt up by that much
        public void TiltKinectUp(int angle)
        {
            if (kinectSensor.NuiCamera.ElevationAngle < Camera.ElevationMaximum)
            {
                try
                {                   
                    kinectSensor.NuiCamera.ElevationAngle += angle;   
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString() + " - no up tilt");
                }
            }
            else
            {
                angle = 0;
            }
            
        }

        public void TiltKinectDown(int angle)
        {
            if (kinectSensor.NuiCamera.ElevationAngle > Camera.ElevationMinimum)
            {
                try
                {                   
                    kinectSensor.NuiCamera.ElevationAngle -= angle;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString() + " - no down tilt");
                }
            }
            else
            {
                angle = 0;
            }
            
        }
        void kinectSensor_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
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
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            kinectSensor.Uninitialize();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = new KeyboardState();
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (state.IsKeyDown(Keys.Up))
            {
                TiltKinectUp(5);
            }
            if (state.IsKeyDown(Keys.Down))
            {
                TiltKinectDown(5);

            }
            // TODO: Add your update logic here
            
            
            base.Update(gameTime);
        }

      
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);
            spriteBatch.Draw(overlay, new Rectangle(0, 0, 640, 480), Color.White);
            spriteBatch.Draw(redDot, position, Color.White);
            Console.WriteLine(position);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
