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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Vector2 dest,position,font_pos, hazard_pos,cursor;
        Texture2D cursorTex,texture,kinectRGBVideo, controller, hazard;
        SpeechRec speech;
        Color[] controller_data, hazard_data,buffer;
        kinectInit kinectIntialize;
        Rectangle bufferRect,cursorRect;

        string message = "Collision: false";
        string speechIn;

        bool controllerHit = false;

        int textureWidth;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            
            Content.RootDirectory = "Content";   
        }

        protected override void Initialize()
        {
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = 512;
            graphics.PreferredBackBufferHeight = 512;
            graphics.ApplyChanges();

            hazard_pos = new Vector2(300, 150);
             
            kinectSensor = Runtime.Kinects[0];
            kinectIntialize = new kinectInit();            
            kinectIntialize.initKinectNui(kinectSensor);
            kinectSensor.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(kinectSensor_VideoFrameReady);
            kinectIntialize.smoothKinectNui(kinectSensor);

            speech = new SpeechRec();
            speech.initSpeech();
            speechIn = speech.returnMsg();

            //Painting code
            textureWidth = 512;

            texture = new Texture2D(GraphicsDevice, textureWidth, textureWidth);

            buffer = new Color[textureWidth * textureWidth];
            for (int i = 0; i < textureWidth * textureWidth; i++)
            {
                 //buffer[i] = Color.Black;
                 buffer[i] = Color.Transparent;
            }
            bufferRect = new Rectangle(0, 0, textureWidth, textureWidth);

            UpdateTexture();

            cursor = Vector2.Zero;
            cursorRect = new Rectangle(0, 0, 10,10);

            UpdateCursor();

            //cursor
            cursorTex = new Texture2D(GraphicsDevice, 2, 2);
            Color[] data = new Color[4];
            data[0] = Color.White;
            data[1] = Color.White;
            data[2] = Color.White;
            data[3] = Color.White;
            cursorTex.SetData<Color>(data);

            dest = Vector2.Zero;


            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Arial");

            kinectRGBVideo = new Texture2D(GraphicsDevice, 512, 512);

            controller = Content.Load<Texture2D>("reddot");
            hazard = Content.Load<Texture2D>("hazard");

            controller_data = new Color[controller.Width * controller.Height];
            controller.GetData(controller_data);

            hazard_data = new Color[hazard.Width * hazard.Height];
            hazard.GetData(hazard_data);
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
           // Console.WriteLine("msg: "+speech.selected);
            UpdateCursor();
            UpdateTexture();

            //paint
            if (speech.selected == true)
            {
                if ((int)kinectIntialize.Pos.Y < 512 || (int)kinectIntialize.Pos.X < 512)
                {
                    buffer[(int)kinectIntialize.Pos.Y * textureWidth + (int)kinectIntialize.Pos.X] = Color.Green;
              //  Console.WriteLine("cursor " + cursor);
                UpdateTexture();
                }
                
            }

            //Kinect stuff
            position = kinectIntialize.Pos;
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
            //paint

            //kinect
            spriteBatch.Begin();
            spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 512, 512), Color.White);
            spriteBatch.DrawString(font, message, font_pos = new Vector2(0, 470), Color.White);
            spriteBatch.DrawString(font, "Speech Recognition" + speech.returnMsg(), font_pos = new Vector2(0, 490), Color.White);
            //spriteBatch.Draw(controller, position, Color.White);
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

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            spriteBatch.Draw(texture, bufferRect, Color.White);

            spriteBatch.Draw(cursorTex, cursorRect, Color.White);

            spriteBatch.End();

            
            base.Draw(gameTime);
        }

        private void UpdateTexture()
        {
            texture.SetData<Color>(buffer);
        }

        private void UpdateCursor()
        {
            cursorRect.X = (int)kinectIntialize.Pos.X;//(int)(cursor.X * 512 / textureWidth);
            cursorRect.Y = (int)kinectIntialize.Pos.Y;//(int)(cursor.Y * 512 / textureWidth);
        }

    }
}