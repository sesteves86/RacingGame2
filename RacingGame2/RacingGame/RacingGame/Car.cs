using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RacingGame
{
    interface ICar
    {
        //Possible actions
        bool IsTurningRight { get; }
        bool IsTurningLeft { get; }
        bool IsAccelerating { get; }
        bool IsBreaking { get; }

        //public variables
        int id { get; set;  }
        //int Lap { get; set; }
        //float Angle { get; set; }
        //Vector2 Position { get; set; }
        //Vector2 Velocity { get; set; }

        void GetTrackMap(Color[,] newColor2d);

        void Update(GameTime gameTimer, CarVariables car);
    }

    class PlayerCar:ICar
    {
        public bool IsAccelerating { get; set; }
        public bool IsBreaking { get; set; }
        public bool IsTurningRight { get; set; }
        public bool IsTurningLeft { get; set; }

        int localId;

        public PlayerCar(int newId) //, Texture2D newTexture, Texture2D newBackgroundTexture, GraphicsDevice newGraphicsDevice, SpriteFont newFont)
        {
            localId = newId;
        }

        public int id
        {
            get { return localId; }
            set { localId = value; }
        }

        public void GetTrackMap(Color[,] newColor2d) { }

        public void Update(GameTime gameTime, CarVariables car)
        {
            //Read input
                //Accelerating or braking
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                IsAccelerating = true;
                IsBreaking = false;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                IsAccelerating = false;
                IsBreaking = true;
            }
            else
            {
                IsAccelerating = false;
                IsBreaking = false;
            }
                //Turning right or left
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                IsTurningRight = true;
                IsTurningLeft = false;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                IsTurningRight = false;
                IsTurningLeft = true;
            }
            else
            {
                IsTurningRight = false;
                IsTurningLeft = false;
            }
        }

    }

    class AICar : ICar
    {
        Vector2 checkingPosition;
        Color[,] color2d = new Color[2000, 2000];

        int Id;
        bool turningRight;
        bool turningLeft;
        bool accelerating;
        bool breaking;

        int timer = 0;
        const int TIMER_RESET = 100;
        const int ANTECIPATION = 50;

        public bool IsTurningRight
        {
            get { return turningRight; }
        }
        public bool IsTurningLeft
        {
            get { return turningLeft; }
        }
        public bool IsAccelerating
        {
            get { return accelerating; }
        }
        public bool IsBreaking
        {
            get { return breaking; }
        }
        public int id
        {
            get { return Id; }
            set { Id = value; }
        }

        public AICar(int newId)//, Color[,] newColor2d)
        {
            Id = newId;
        }

        public void GetTrackMap( Color[,] newColor2d)
        {
            color2d = newColor2d;
        }

        public void Update(GameTime gameTime, CarVariables car)
        {
            //reset values
            accelerating = breaking = turningLeft = turningRight = false;

            //1st Layer
            Vector2 checkingPositionFront1 = new Vector2((car.positionX + ANTECIPATION * car.speedX), (car.positionY + ANTECIPATION * car.speedY));
            if (IsOnRoad(checkingPositionFront1)) //Accelerate if there's track ahead, otherwise break
            {
                accelerating = true;
            }
            else
            {
                breaking = true;
            }
            int iMax = 4;
            for (int i = 0; i < iMax; i++)
            {

                float vRightX = (float)(Math.Cos(car.orientation + Math.PI / Math.Pow(2, (iMax - i))) * Math.Abs(car.normalSpeed));
                float vRightY = (float)(Math.Sin(car.orientation + Math.PI / Math.Pow(2, (iMax - i))) * Math.Abs(car.normalSpeed));
                float vLeftX = (float)(Math.Cos(car.orientation - Math.PI / Math.Pow(2, (iMax - i))) * Math.Abs(car.normalSpeed));
                float vLeftY = (float)(Math.Sin(car.orientation - Math.PI / Math.Pow(2, (iMax - i))) * Math.Abs(car.normalSpeed));
                Vector2 checkingPositionRight = new Vector2((car.positionX + ANTECIPATION / (i + 1) * vRightX), (car.positionY + ANTECIPATION / (i + 1) * vRightY));
                Vector2 checkingPositionLeft = new Vector2((car.positionX + ANTECIPATION / (i + 1) * vLeftX), (car.positionY + ANTECIPATION / (i + 1) * vLeftY));

                if (IsOnRoad(checkingPositionRight) && !IsOnRoad(checkingPositionLeft))
                {
                    turningRight = true;
                    i = iMax;
                }
                else if (!IsOnRoad(checkingPositionRight) && IsOnRoad(checkingPositionLeft))
                {
                    turningLeft = true;
                    i = iMax;
                }

                //Check 2: change immediately direction if has road for one side but not for the other side
                vRightX = (float)(Math.Cos(car.orientation + Math.PI / 2) * Math.Abs(car.normalSpeed));
                vRightY = (float)(Math.Sin(car.orientation + Math.PI / 2) * Math.Abs(car.normalSpeed));
                vLeftX = (float)(Math.Cos(car.orientation - Math.PI / 2) * Math.Abs(car.normalSpeed));
                vLeftY = (float)(Math.Sin(car.orientation - Math.PI / 2) * Math.Abs(car.normalSpeed));
                checkingPositionRight = new Vector2((car.positionX + 50 * (i + 1) * vRightX), (car.positionY + 50 * (i + 1) * vRightY));
                checkingPositionLeft = new Vector2((car.positionX + 50 * (i + 1) * vLeftX), (car.positionY + 50 * (i + 1) * vLeftY));

                if (IsOnRoad(checkingPositionRight) && !IsOnRoad(checkingPositionLeft))
                {
                    turningRight = true;
                    i = iMax;
                }
                else if (!IsOnRoad(checkingPositionRight) && IsOnRoad(checkingPositionLeft))
                {
                    turningLeft = true;
                    i = iMax;
                }
            }
        }

        private bool IsOnRoad(Vector2 position)
        {
            bool isOnRoad;

            try
            {
                if (color2d[(int)position.X + 400, (int)position.Y].A < 10 || color2d[(int)position.X + 400, (int)position.Y].B>200) //Not on track
                    isOnRoad = false;
                else
                    isOnRoad = true;
            }
            catch
            { isOnRoad = false; }

            return isOnRoad;
        }

        private bool CheckonWall(Vector2 position)
        {
            bool onWall;

            try
            {
                //if (color2d[(int)positionX + 400 - texture.Width / 2 + 10, (int)positionY - texture.Height / 2 + 30].B < 200) //On wall
                if (color2d[(int)position.X + 400, (int)position.Y].B < 200) //On wall
                    onWall = false;
                else
                    onWall = true;
            }
            catch
            {
                onWall = false;
            }

            return onWall;
        }
    }

    class CarVariables
    {
        //movement variables
        public float angle { get; set; }
        public float orientation { get; set; }
        public float turning { get; set; }
        
        public float positionX { get; set; }
        public float positionY { get; set; }

        public float speedX { get; set; }
        public float speedY { get; set; }
        public float normalSpeed { get; set; }
        
        //Others
        public Texture2D texture { get; set; }

        public bool onRoad { get; set; }
        public bool onWall { get; set; }
        public bool crossingFinishLine { get; set; }

        public int lapTime { get; set; }
        public int bestLap { get; set; }
        public int currentLap { get; set; }
        public int countingLap { get; set; }

        public int id { get; set; }

    }

    
}
