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

namespace RacingGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        //PlayerCar car;
        Texture2D backgroundTexture;
        Vector2 backgroundPosition;
        Texture2D yellowPixelTexture;
        Texture2D redCarTexture;
        Texture2D orangeCarTexture;
        Texture2D blueCarTexture;

        Texture2D textureSplashScreen, textureStartMenu, texturePreRaceMenu, texturePostRaceMenu;

        //Font
        SpriteFont font;
        
        //Game menus
        enum GameMenu
        {
            SplashScreen,
            StartMenu,
            PreRaceMenu,
            Racing,
            PostRaceMenu,
            Credits
        }

        GameMenu gameMenu = GameMenu.SplashScreen;//.Racing;//.SplashScreen;

        //Variables associated with the menus
        SplashMenu splashMenu;
        StartMenu startMenu;
        Credits credits;
        PreRaceMenu preRaceMenu;
        RacingController racingController;

        //Update variables
        //int timer = 0;
        //const int UPDATE_TIME = 100; //ms

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //this.IsMouseVisible=true;
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

            //Font
            font = Content.Load<SpriteFont>("SpriteFont1");
            
            backgroundTexture = Content.Load<Texture2D>("RacingTrack1");
            backgroundPosition = new Vector2(-400, 0);
            redCarTexture = Content.Load<Texture2D>("RacingCarRed");
            orangeCarTexture = Content.Load<Texture2D>("RacingCarOrange");
            blueCarTexture = Content.Load<Texture2D>("RacingCarBlue");


            yellowPixelTexture = Content.Load<Texture2D>("Yellow Pixel");

            //Menus
            textureSplashScreen = Content.Load<Texture2D>("SplashScreen");
            splashMenu = new SplashMenu(textureSplashScreen);
            textureStartMenu = Content.Load<Texture2D>("Racing Menu Background");
            startMenu = new StartMenu(textureStartMenu, yellowPixelTexture, font);
            credits = new Credits(textureStartMenu, font);
            preRaceMenu = new PreRaceMenu(textureStartMenu, yellowPixelTexture, font);
            List<ICar> carList = new List<ICar>();
            //carList.Add(new PlayerCar(0));
            racingController = new RacingController(GraphicsDevice, font, backgroundTexture, yellowPixelTexture);
            racingController.AddCar(redCarTexture, new PlayerCar(0));
            racingController.AddCar(blueCarTexture, new AICar(1));
            racingController.AddCar(orangeCarTexture, new AICar(2));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            switch (gameMenu)
            {
                case GameMenu.SplashScreen:
                    splashMenu.Update(gameTime);
                    if (splashMenu.isFinished)
                    {
                        gameMenu = GameMenu.StartMenu;
                        splashMenu.isFinished = false;
                    }

                    break;
                case GameMenu.StartMenu:
                    startMenu.Update(gameTime);
                    if (startMenu.isFinished)
                    {
                        switch (startMenu.cursorPosition)
                        {
                            case 1:
                                gameMenu = GameMenu.PreRaceMenu;
                                break;
                            case 2:
                                gameMenu = GameMenu.SplashScreen;
                                break;
                            case 3:
                                gameMenu = GameMenu.Credits;
                                break;
                        }
                        startMenu.isFinished = false;
                    }
                    break;
                case GameMenu.Credits:
                    credits.Update(gameTime);
                    if (credits.isFinished)
                    {
                        gameMenu = GameMenu.SplashScreen;
                        credits.isKeyPressed = false;
                        credits.isFinished = false;
                    }
                    break;
                case GameMenu.PreRaceMenu:
                    preRaceMenu.Update(gameTime);
                    if (preRaceMenu.wasEnterKeyPressed)
                    {
                        gameMenu = GameMenu.Racing;
                    }
                    break;
                case GameMenu.Racing:
                    /*timer += gameTime.ElapsedGameTime.Milliseconds;
                    if (timer > UPDATE_TIME)
                    {
                        timer = 0;
                        racingController.Update(gameTime);
                    }*/
                    racingController.Update(gameTime);
                    break;
                case GameMenu.PostRaceMenu:

                    break;
            }


            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Green);

            switch (gameMenu)
            {
                case GameMenu.SplashScreen:
                    spriteBatch.Begin();
                    splashMenu.Draw(spriteBatch);

                    break;
                case GameMenu.StartMenu:
                    spriteBatch.Begin();
                    startMenu.Draw(spriteBatch);

                    break;
                case GameMenu.Credits:
                    spriteBatch.Begin();
                    credits.Draw(spriteBatch);

                    break;
                case GameMenu.PreRaceMenu:
                    spriteBatch.Begin();
                    preRaceMenu.Draw(spriteBatch);
                    break;
                case GameMenu.Racing:
                    racingController.Draw(spriteBatch);

                    
                    break;
                case GameMenu.PostRaceMenu:
                    spriteBatch.Begin();

                    break;
            }
            
            spriteBatch.End();
         
            base.Draw(gameTime);
        }
    }
}
