using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RacingGame
{
    class Camera
    {
        public Matrix transform;
        Viewport viewPort;
        Vector2 center;

        public Camera(Viewport newViewPort)
            { viewPort = newViewPort; }

        public void Update(GameTime gameTime, float positionX, float positionY)
        {
            center = new Vector2(positionX - viewPort.Width / 2, positionY - viewPort.Height / 2);
            transform = Matrix.CreateScale(new Vector3(1f, 1f, 0)) * Matrix.CreateTranslation(-center.X, -center.Y, 0f);
        }
    }
}
