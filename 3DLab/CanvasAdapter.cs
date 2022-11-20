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
            canvas.FillCircle(new Point(
                point.X + (this.canvasSize.Width / 2.0),
                point.Y + (this.canvasSize.Height / 2.0)), 2.0);
        }

        public void DrawConnection(Vector2 point1, Vector2 point2)
        {
            canvas.StrokeColor = Colors.Aqua;
            canvas.StrokeSize = 2;
            canvas.DrawLine(new Point(
                point1.X + (this.canvasSize.Width / 2.0),
                point1.Y + (this.canvasSize.Height / 2.0)),
                new Point(
                point2.X + (this.canvasSize.Width / 2.0),
                point2.Y + (this.canvasSize.Height / 2.0)));
        }
    }
}
