using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3DLab
{
    public class GraphicsDrawable : IDrawable
    {
        private readonly Vector3[] points = new Vector3[]
            {
                new Vector3 (-50, -50, -50),
                new Vector3 (50, -50, -50),
                new Vector3 (50, 50, -50),
                new Vector3 (-50, 50, -50),

                new Vector3 (-50, -50, 50),
                new Vector3 (50, -50, 50),
                new Vector3 (50, 50, 50),
                new Vector3 (-50, 50, 50),
            };

        private readonly SizeF canvasSize = new SizeF(600, 400);
        private float angle = 23f;

        public GraphicsDrawable()
        {

        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2;
            canvas.DrawLine(0, 0, canvasSize.Width, 0);
            canvas.DrawLine(canvasSize.Width, 0, canvasSize.Width, canvasSize.Height);
            canvas.DrawLine(canvasSize.Width, canvasSize.Height, 0, canvasSize.Height);
            canvas.DrawLine(0, canvasSize.Height, 0, 0);

            var ca = new CanvasAdapter(canvas, canvasSize);

            var projectedPoints = new List<Matrix4x4>(8);
            foreach (var point in points)
            {
                // M11 = X
                // M21 = Y
                // M31 = Z
                var radians = (Math.PI / 180) * angle;

                var vecMatrix = new Matrix4x4(
                    point.X, 0f, 0f, 0f,
                    point.Y, 0f, 0f, 0f,
                    point.Z, 0f, 0f, 0f,
                    0f, 0f, 0f, 0f);

                // Rotation X
                var rotationX = Matrix4x4.CreateRotationX((float)radians);
                var rotated = Matrix4x4.Multiply(rotationX, vecMatrix);

                // Rotation Y
                var rotationY = Matrix4x4.CreateRotationY((float)radians);
                rotated = Matrix4x4.Multiply(rotationY, rotated);

                // Rotation Z
                var rotationZ = Matrix4x4.CreateRotationZ((float)radians);
                rotated = Matrix4x4.Multiply(rotationZ, rotated);

                // Weak perpective
                var focal = 1f;
                var distance = 300;
                var x = focal / (distance - rotated.M31);
                var y = focal / (distance - rotated.M31);
                var projection = new Matrix4x4(
                    x, 0f, 0f, 0f,
                    0f, y, 0f, 0f,
                    0f, 0f, 0f, 0f,
                    0f, 0f, 0f, 0f);
                var projected = Matrix4x4.Multiply(projection, rotated);

                // Zoom in
                var projection2 = new Matrix4x4(
                    450, 0f, 0f, 0f,
                    0f, 450, 0f, 0f,
                    0f, 0f, 0f, 0f,
                    0f, 0f, 0f, 0f);
                projected = Matrix4x4.Multiply(projection2, projected);

                ca.DrawPoint(new Vector2(projected.M11, projected.M21));

                projectedPoints.Add(projected);

            }

            for (int i = 0; i < 4; i++)
            {
                Connect(ca, i, (i + 1) % 4, projectedPoints);
                Connect(ca, i + 4, ((i + 1) % 4) + 4, projectedPoints);
                Connect(ca, i, i + 4, projectedPoints);
            }

            angle += 1f;
            if (angle > 360)
            {
                angle = 0;
            }
        }

        private static void Connect(CanvasAdapter c,  int i, int j, List<Matrix4x4> points)
        {
            var a = new Vector2(points[i].M11, points[i].M21);
            var b = new Vector2(points[j].M11, points[j].M21);
            c.DrawConnection(a, b);
        }

    }
}
