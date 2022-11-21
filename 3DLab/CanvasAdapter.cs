using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _3DLab
{
    internal class CanvasAdapter
    {
        private readonly ICanvas canvas;
        private readonly SizeF canvasSize;

        public CanvasAdapter(ICanvas canvas, SizeF canvasSize)
        {
            this.canvas = canvas;
            this.canvasSize = canvasSize;
        }

        public void DrawPoint(Vector2 point)
        {
            canvas.FillColor = Colors.Bisque;
            canvas.FillCircle(new PointF(
                point.X + (this.canvasSize.Width / 2.0f),
                point.Y + (this.canvasSize.Height / 2.0f)), 2.0f);
        }

        public void DrawConnection(Vector2 point1, Vector2 point2)
        {
            canvas.StrokeColor = Colors.Aqua;
            canvas.StrokeSize = 2f;
            canvas.DrawLine(new PointF(
                point1.X + (this.canvasSize.Width / 2.0f),
                point1.Y + (this.canvasSize.Height / 2.0f)),
                new PointF(
                point2.X + (this.canvasSize.Width / 2.0f),
                point2.Y + (this.canvasSize.Height / 2.0f)));
        }
    }
}
