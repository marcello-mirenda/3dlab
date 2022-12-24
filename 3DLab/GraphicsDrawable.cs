using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Font = Microsoft.Maui.Graphics.Font;

namespace _3DLab
{
    public class GraphicsDrawable : IDrawable
    {
        private readonly Vector3[] points = new Vector3[]
            {
                new Vector3 (-50f, -50f, -50f), // 0
                new Vector3 (50f, -50f, -50f),  // 1
                new Vector3 (50f, 50f, -50f),   // 2
                new Vector3 (-50f, 50f, -50f),  // 3
                new Vector3 (-50f, -50f, 50f),  // 4
                new Vector3 (50f, -50f, 50f),   // 5
                new Vector3 (50f, 50f, 50f),    // 6
                new Vector3 (-50f, 50f, 50f),   // 7
                new Vector3 (0f, 0f, 0f),   // 8 normal
                new Vector3 (0f, 0f, 0f),   // 9 normal
            };

        private readonly SizeF canvasSize = new SizeF(600f, 400f);
        private float angle = 0f;
        private readonly List<Matrix4x4> projectedPoints = new List<Matrix4x4>(8);
        private readonly Vector3 camera = new Vector3(0, 0, 400f);
        public GraphicsDrawable()
        {
            var v1 = points[4];
            var v2 = points[5];
            var v3 = points[6];
            var vv1 = v2 - v1;
            var vv2 = v3 - v2;
            var normal = Vector3.Cross(vv1, vv2);
            points[8] = new Vector3(0, 0, 50f);
            points[9] = Vector3.Normalize(normal) * 100f;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2f;
            canvas.DrawLine(0f, 0f, canvasSize.Width, 0f);
            canvas.DrawLine(canvasSize.Width, 0f, canvasSize.Width, canvasSize.Height);
            canvas.DrawLine(canvasSize.Width, canvasSize.Height, 0f, canvasSize.Height);
            canvas.DrawLine(0f, canvasSize.Height, 0f, 0f);

            var ca = new CanvasAdapter(canvas, canvasSize);

            var radians = MathF.PI * angle / 180f;
            projectedPoints.Clear();

            var rotadedPoints = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                var point = points[i];
                // M11 = X
                // M21 = Y
                // M31 = Z

                var vecMatrix = new Matrix4x4(
                    point.X, 0f, 0f, 0f,
                    point.Y, 0f, 0f, 0f,
                    point.Z, 0f, 0f, 0f,
                         0f, 0f, 0f, 0f);

                var rotated = vecMatrix;

                //// Rotation X
                //var rotationX = Matrix4x4.CreateRotationX(radians);
                //rotated = Matrix4x4.Multiply(rotationX, rotated);

                // Rotation Y
                var rotationY = Matrix4x4.CreateRotationY(radians);
                rotated = Matrix4x4.Multiply(rotationY, rotated);


                //// Rotation Z
                //var rotationZ = Matrix4x4.CreateRotationZ(radians);
                //rotated = Matrix4x4.Multiply(rotationZ, rotated);

                rotadedPoints[i] = new Vector3(rotated.M11, rotated.M21, rotated.M31);

                // Weak perpective
                var focal = 5f;
                var distance = 400f;
                var z = focal / (distance - rotated.M31);
                var projection = new Matrix4x4(
                     z, 0f, 0f, 0f,
                    0f, z, 0f, 0f,
                    0f, 0f, 0f, 0f,
                    0f, 0f, 0f, 0f);
                var projected = Matrix4x4.Multiply(projection, rotated);

                // Zoom in
                z = 100f;
                var projection2 = new Matrix4x4(
                    z, 0f, 0f, 0f,
                   0f, z, 0f, 0f,
                   0f, 0f, 0f, 0f,
                   0f, 0f, 0f, 0f);

                projected = Matrix4x4.Multiply(projection2, projected);

                //ca.DrawPoint(new Vector2(projected.M11, projected.M21));

                projectedPoints.Add(projected);

            }

            var vv = rotadedPoints[6] - camera;
            var scalar = Vector3.Dot(vv, rotadedPoints[9]);


            // Connect edges
            for (int i = 0; i < 4; i++)
            {
                Connect(ca, i, (i + 1) % 4, projectedPoints, Colors.Aqua);
                Connect(ca, i + 4, ((i + 1) % 4) + 4, projectedPoints, Colors.BlanchedAlmond);
                Connect(ca, i, i + 4, projectedPoints, Colors.Coral);
            }

            ca.DrawPoint(new Vector2(projectedPoints[6].M11, projectedPoints[6].M21), Colors.Goldenrod);
            ca.DrawPoint(new Vector2(projectedPoints[5].M11, projectedPoints[5].M21), Colors.Purple);
            ca.DrawPoint(new Vector2(projectedPoints[4].M11, projectedPoints[4].M21), Colors.RosyBrown);

            if (scalar < 0)
            {
                ca.DrawPoint(new Vector2(projectedPoints[8].M11, projectedPoints[8].M21), Colors.SteelBlue);
                ca.DrawPoint(new Vector2(projectedPoints[9].M11, projectedPoints[9].M21), Colors.SeaGreen);
                Connect(ca, 8, 9, projectedPoints, Colors.Red);
            }
            else
            {
                ca.DrawPoint(new Vector2(projectedPoints[8].M11, projectedPoints[8].M21), Colors.SteelBlue);
                ca.DrawPoint(new Vector2(projectedPoints[9].M11, projectedPoints[9].M21), Colors.SeaGreen);
                Connect(ca, 8, 9, projectedPoints, Colors.ForestGreen);
            }

            angle -= 1f;
            if (angle >= 361f || angle <= -361f)
            {
                angle = 0f;
            }


            canvas.FontColor = Colors.Blue;
            canvas.Font = Font.Default;
            canvas.FontSize = 20f;
            canvas.DrawString($"Angle {angle}, scalar {scalar}", 10f, 10f, 300f, 100f, HorizontalAlignment.Left, VerticalAlignment.Top);



        }

        private static void Connect(CanvasAdapter c, int i, int j, List<Matrix4x4> points, Color color)
        {
            var a = new Vector2(points[i].M11, points[i].M21);
            var b = new Vector2(points[j].M11, points[j].M21);
            c.DrawConnection(a, b, color);
        }

    }
}
