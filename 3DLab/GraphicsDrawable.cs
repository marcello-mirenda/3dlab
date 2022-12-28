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
                new Vector3 (0f, 0f, 0f),   // 8
                new Vector3 (0f, 0f, 0f),   // 9
                new Vector3 (0f, 0f, 0f),   // 10 
                new Vector3 (0f, 0f, 0f),   // 11
                new Vector3 (0f, 0f, 0f),   // 12 
                new Vector3 (0f, 0f, 0f),   // 13
            };

        private readonly int[][] faces = new int[][]
        {
            new int[] {3,2,1,0},
            new int[] {4,5,6,7},
            new int[] {0,1,5,4},
            new int[] {7,6,2,3},
            new int[] {0,4,7,3},
            new int[] {1,2,6,5},
        };

        private readonly int[][] edges = new int[12][]
        {
            new int[2] {0,1},
            new int[2] {0,3},
            new int[2] {0,4},
            new int[2] {1,2},
            new int[2] {1,5},
            new int[2] {2,3},
            new int[2] {2,6},
            new int[2] {3,7},
            new int[2] {4,5},
            new int[2] {4,7},
            new int[2] {5,6},
            new int[2] {6,7},
        };

        private readonly SizeF canvasSize = new SizeF(600f, 400f);
        private float angle = 0f;
        private readonly List<Matrix4x4> projectedPoints = new List<Matrix4x4>(8);
        private readonly Vector3 camera = new Vector3(0, 0, 400f);

        public GraphicsDrawable()
        {
            for (int i = 0; i < 6; i++)
            {
                var v1 = points[faces[i][0]];
                var v2 = points[faces[i][1]];
                var v3 = points[faces[i][2]];

                var vv1 = v2 - v1;
                var vv2 = v3 - v2;
                points[i + 8] = Vector3.Cross(vv1, vv2);
            }
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

                // Rotation X
                var rotationX = Matrix4x4.CreateRotationX(radians);
                rotated = Matrix4x4.Multiply(rotationX, rotated);

                // Rotation Y
                var rotationY = Matrix4x4.CreateRotationY(radians);
                rotated = Matrix4x4.Multiply(rotationY, rotated);

                // Rotation Z
                var rotationZ = Matrix4x4.CreateRotationZ(radians);
                rotated = Matrix4x4.Multiply(rotationZ, rotated);

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

                projectedPoints.Add(projected);

            }

            var hitCache = 0;
            foreach (var item in cache)
            {
                cache[item.Key] = false;
            }
            for (int i = 0; i < 6; i++)
            {
                var vv = rotadedPoints[faces[i][0]] - camera;
                var scalar = Vector3.Dot(vv, rotadedPoints[i + 8]);

                if (scalar < 0)
                {
                    var color = colors[i];
                    if (Connect(ca, faces[i][0], faces[i][1], projectedPoints, color, cache))
                    {
                        hitCache++;
                    }
                    if (Connect(ca, faces[i][1], faces[i][2], projectedPoints, color, cache))
                    {
                        hitCache++;
                    }
                    if (Connect(ca, faces[i][2], faces[i][3], projectedPoints, color, cache))
                    {
                        hitCache++;
                    }
                    if (Connect(ca, faces[i][3], faces[i][0], projectedPoints, color, cache))
                    {
                        hitCache++;
                    }
                }
            }

            angle -= 1f;
            if (angle >= 361f || angle <= -361f)
            {
                angle = 0f;
            }

            canvas.FontColor = Colors.Blue;
            canvas.Font = Font.Default;
            canvas.FontSize = 20f;
            canvas.DrawString($"Angle {angle}, HitCache {hitCache}", 10f, 10f, 300f, 100f, HorizontalAlignment.Left, VerticalAlignment.Top);
        }

        private static bool Connect(CanvasAdapter c, int i, int j, List<Matrix4x4> points, Color color, Dictionary<(int v1, int v2), bool> cache)
        {
            if (i > j)
            {
                if (cache[(j, i)])
                {
                    return true;
                }
                cache[(j, i)] = true;
            }
            else
            {
                if (cache[(i, j)])
                {
                    return true;
                }
                cache[(i, j)] = true;
            }
            var a = new Vector2(points[i].M11, points[i].M21);
            var b = new Vector2(points[j].M11, points[j].M21);
            c.DrawConnection(a, b, color);
            return false;
        }

        private readonly Color[] colors = new Color[6]
        {
            Colors.Cyan,
            Colors.DodgerBlue,
            Colors.LightGreen,
            Colors.Peru,
            Colors.Gold,
            Colors.Gainsboro
        };

        private readonly Dictionary<(int v1, int v2), bool> cache = new Dictionary<(int v1, int v2), bool>(12)
        {
            {(0,1), false },
            {(0,3), false },
            {(0,4), false },
            {(1,2), false },
            {(1,5), false },
            {(2,3), false },
            {(2,6), false },
            {(3,7), false },
            {(4,5), false },
            {(4,7), false },
            {(5,6), false },
            {(6,7), false },
        };
    }
}
