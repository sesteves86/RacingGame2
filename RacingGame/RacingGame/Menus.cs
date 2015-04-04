using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Threading;

namespace RacingGame
{
    class SplashMenu
    {
        Texture2D texture;
        Rectangle rectangle;
        bool pressedKey = false;
        int timer = 0;
        public bool isFinished = false;
        Color filterColor;
        

        public SplashMenu(Texture2D newTexture)
        {
            texture = newTexture;
            rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public void Update(GameTime gameTime)
        {
            if(pressedKey)
                timer += gameTime.ElapsedGameTime.Milliseconds;

            if (timer > 200)
            {
                isFinished = true;
                timer = 0;
                pressedKey = false;
            }

            var k = Keyboard.GetState().GetPressedKeys();
            if ( k.Length>0 && pressedKey==false)
            {
                pressedKey = true;
                timer = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            filterColor = new Color(255, 255, 255, (1 - timer / 200f)); //Fade off effect
            spriteBatch.Draw(texture, rectangle, filterColor);
        }
    }

    class StartMenu
    {
        Texture2D texture;
        Texture2D selectorTexture;
        Rectangle rectangle;
        Rectangle selectorRectangle;
        SpriteFont font;

        public int cursorPosition = 1;

        //Keyboard logic
        public bool isFinished = false;

        //Keys lastKeyPressed;
        bool wasEnterKeyPressed = false;
        bool wasUpKeyPressed = false;
        bool wasDownKeyPressed = false;

        public StartMenu(Texture2D newTexture, Texture2D newSelectorTexture, SpriteFont newFont)
        {
            font = newFont;
            texture = newTexture;
            selectorTexture = newSelectorTexture;
            rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();

            //Down
            if (keyboard.IsKeyDown(Keys.Down) && !wasDownKeyPressed)
            {
                wasDownKeyPressed = true;
                cursorPosition += 2;
                if (cursorPosition > 3) cursorPosition = 3;
            }
            if (!keyboard.IsKeyDown(Keys.Down))
                wasDownKeyPressed = false;

            //Up
            if (keyboard.IsKeyDown(Keys.Up) && !wasUpKeyPressed)
            {
                wasUpKeyPressed = true;
                cursorPosition -= 2;
                if (cursorPosition <1) cursorPosition = 1;
            }
            if (!keyboard.IsKeyDown(Keys.Up))
                wasUpKeyPressed = false;

            //Enter
            if (keyboard.IsKeyDown(Keys.Enter) && !wasEnterKeyPressed)
            {
                wasEnterKeyPressed = true;
            }
            if (!keyboard.IsKeyDown(Keys.Enter) && wasEnterKeyPressed)
            {
                wasEnterKeyPressed = false;
                Thread.Sleep(500);
                isFinished = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rectangle, Color.White); //Background

            //Draw Selector
            selectorRectangle = new Rectangle(190,100*cursorPosition-5,160, 40);
            spriteBatch.Draw(selectorTexture, selectorRectangle, Color.Gray);

            //Draw text
            spriteBatch.DrawString(font, "Quick Race", new Vector2(200f, 100f), Color.Yellow);
            spriteBatch.DrawString(font, "Championship", new Vector2(200f, 200f), Color.Gray);
            spriteBatch.DrawString(font, "Credits", new Vector2(200f, 300f), Color.Yellow);
        }    
    }

    class Credits
    {
        Texture2D backgroundTexture;
        SpriteFont font;
        public bool wasKeyPressed;
        public bool isFinished = false;

        public Credits(Texture2D newTexture, SpriteFont newFont)
        {
            backgroundTexture = newTexture;
            font = newFont;
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().GetPressedKeys().Length > 0 && !wasKeyPressed) // if any key pressed
                wasKeyPressed = true;

            if (Keyboard.GetState().GetPressedKeys().Length == 0 && wasKeyPressed){
                isFinished = true;
                wasKeyPressed = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 800, 600), Color.White);
            
            spriteBatch.DrawString(font, "Credits", new Vector2(200, 50), Color.Yellow);
            spriteBatch.DrawString(font, "Sergio Esteves", new Vector2(220, 150), Color.Yellow);

            spriteBatch.DrawString(font, "Press any key to continue", new Vector2(300, 450), Color.Yellow);
        }
    }

    class PreRaceMenu
    {
        //output
        public bool isReady = false;
        Texture2D backgroundTexture;
        Texture2D selectorTexture;
        Color trackSelectedColor;
        public int cursorTrack=1;
        int cursorY=1;

        //Keys lastKeyPressed;
        bool wasEnterKeyPressed = false;
        bool wasRightKeyPressed = false;
        bool wasLeftKeyPressed = false;
        bool wasUpKeyPressed = false;
        bool wasDownKeyPressed = false;
        const int numberOfTracks = 3;

        SpriteFont font;

        public PreRaceMenu(Texture2D newBackgroundTexture, Texture2D newSelectorTexture,SpriteFont newFont)
        {
            backgroundTexture = newBackgroundTexture;
            selectorTexture = newSelectorTexture;
            font = newFont;
        }

        public void Update(GameTime gameTime)
        {
            #region Pressed Keyboard keys 
            var keyboard = Keyboard.GetState();
            //Right
            if (keyboard.IsKeyDown(Keys.Right) && !wasRightKeyPressed)
            {
                wasRightKeyPressed = true;
                if (cursorY == 1)
                {
                    cursorTrack += 1;
                    if (cursorTrack > numberOfTracks)
                        cursorTrack = numberOfTracks;
                }
            }
            if (!keyboard.IsKeyDown(Keys.Right)){
                wasRightKeyPressed=false;
            }
            //Left
            if (keyboard.IsKeyDown(Keys.Left) && !wasLeftKeyPressed)
            {
                wasLeftKeyPressed = true;
                if (cursorY == 1)
                {
                    cursorTrack -= 1;
                    if (cursorTrack <1)
                        cursorTrack = 1;
                }
            }
            if (!keyboard.IsKeyDown(Keys.Left)){
                wasLeftKeyPressed=false;
            }
            //Up
            if (keyboard.IsKeyDown(Keys.Up) && !wasUpKeyPressed)
            {
                wasUpKeyPressed = true;
                cursorY -= 1;
                if (cursorY <1) cursorY = 1;
            }
            if (!keyboard.IsKeyDown(Keys.Up))
            {
                wasUpKeyPressed = false;
            }
            //Down
            if (keyboard.IsKeyDown(Keys.Down) && !wasDownKeyPressed)
            {
                cursorY += 1;
                if (cursorY >2) cursorY = 2;
            }
            if (!keyboard.IsKeyDown(Keys.Down))
            {
                wasDownKeyPressed = false;
            }
            //Enter
            if (keyboard.IsKeyDown(Keys.Enter) && !wasEnterKeyPressed)
            {
                wasEnterKeyPressed = true;
            }
            if (!keyboard.IsKeyDown(Keys.Enter) && wasEnterKeyPressed)
            {
                isReady = true;
                wasEnterKeyPressed = false;
            }
            #endregion
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 800, 600), Color.White);

            //Draw cursor
            if (cursorY == 1)
            {
                trackSelectedColor = new Color(200, 200, 100);
            }
            else
            {
                trackSelectedColor = new Color(60, 60, 60);
                spriteBatch.Draw(selectorTexture, new Rectangle(200, 300, 100, 50), Color.White);
            }

            spriteBatch.Draw(selectorTexture, new Rectangle(cursorTrack * 100 - 25, 170, 80, 50), trackSelectedColor);

            //Draw Text
            spriteBatch.DrawString(font, "Tracks", new Vector2(100, 50), Color.Yellow);
            spriteBatch.DrawString(font, "Track 1", new Vector2(80, 180), Color.Yellow);
            spriteBatch.DrawString(font, "Track 2", new Vector2(180, 180), Color.Yellow);
            spriteBatch.DrawString(font, "Track 3", new Vector2(280, 180), Color.Yellow);

            spriteBatch.DrawString(font, "Start", new Vector2(200, 300), Color.Yellow);

            

            
        }
    }
}
