using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Rotate;

internal static class Program
{
    const uint Width = 800;
    const uint Height = 800;

    static readonly Color PixelColor = Color.Red;
    static readonly Image Framebuffer = new(Width / 2, Height / 2);

    static void Main()
    {
        var window = new RenderWindow(new VideoMode(Width, Height), "Software Renderer");
        window.SetFramerateLimit(60);
        window.Closed += (_, _) => window.Close();
        var centre = new Vector2f(Width / 2, Height / 2) / 2;

        var texture = new Texture(Framebuffer);
        var sprite = new Sprite(texture);
        sprite.Scale = new Vector2f(2, 2);

        var blank = new Image(Framebuffer);

        ReadOnlySpan<Vector3f> points =
        [
            new(-0.5f, -0.5f, -0.5f),
            new(0.5f, -0.5f, -0.5f),
            new(0.5f, 0.5f, -0.5f),
            new(-0.5f, 0.5f, -0.5f),
            new(-0.5f, -0.5f, 0.5f),
            new(0.5f, -0.5f, 0.5f),
            new(0.5f, 0.5f, 0.5f),
            new(-0.5f, 0.5f, 0.5f)
        ];

        Span<Vector2f> projected = stackalloc Vector2f[8];
        float angle = 0;

        while (window.IsOpen)
        {
            window.DispatchEvents();

            var (sin, cos) = MathF.SinCos(angle);
            for (int i = 0; i < points.Length; i++)
            {
                Vector3f point = points[i];
                point = RotateY(point, (sin, cos));
                point = RotateX(point, (sin, cos));
                point = RotateZ(point, (sin, cos));
                float z = 1f / (2f - point.Z);
                point.X *= z;
                point.Y *= z;
                float x1 = centre.X + (point.X * centre.X);
                float y1 = centre.Y + (point.Y * centre.Y);

                projected[i] = new Vector2f(x1, y1);
            }

            for (int i = 0; i < 4; i++)
            {
                ConnectPoints(i, (i + 1) % 4, projected);
                ConnectPoints(i + 4, ((i + 1) % 4) + 4, projected);
                ConnectPoints(i, i + 4, projected);
            }

            texture.Update(Framebuffer);
            window.Draw(sprite);
            window.Display();

            Framebuffer.Copy(blank, 0, 0);

            angle += 0.02f;
        }
    }
    static Vector3f RotateX(Vector3f point, (float, float) sincos)
    {
        var (sin, cos) = sincos;
        float y = (point.Y * cos) - (point.Z * sin);
        float z = (point.Y * sin) + (point.Z * cos);
        return new Vector3f(point.X, y, z);
    }

    static Vector3f RotateY(Vector3f point, (float, float) sincos)
    {
        var (sin, cos) = sincos;
        float x = (point.X * cos) + (point.Z * sin);
        float z = (-point.X * sin) + (point.Z * cos);
        return new Vector3f(x, point.Y, z);
    }

    static Vector3f RotateZ(Vector3f point, (float, float) sincos)
    {
        var (sin, cos) = sincos;
        float x = (point.X * cos) - (point.Y * sin);
        float y = (point.X * sin) + (point.Y * cos);
        return new Vector3f(x, y, point.Z);
    }

    static void ConnectPoints(int i, int j, ReadOnlySpan<Vector2f> points)
    {
        Vector2f a = points[i];
        Vector2f b = points[j];
        DrawLine(a, b);
    }

    static void DrawLine(Vector2f p1, Vector2f p2)
    {
        int x0 = (int)MathF.Round(p1.X);
        int y0 = (int)MathF.Round(p1.Y);
        int x1 = (int)MathF.Round(p2.X);
        int y1 = (int)MathF.Round(p2.Y);

        // test for vertical
        if (x0 == x1)
        {
            if (y0 > y1)
            {
                (y0, y1) = (y1, y0);
            }

            while (y0 <= y1)
            {
                Framebuffer.SetPixel((uint)x0, (uint)y0, PixelColor);
                y0++;
            }
            return;
        }

        // test for horizontal
        if (y0 == y1)
        {
            if (x0 > x1)
            {
                (x0, x1) = (x1, x0);
            }

            while (x0 <= x1)
            {
                Framebuffer.SetPixel((uint)x0, (uint)y0, PixelColor);
                x0++;
            }
            return;
        }

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            (x0, y0) = (y0, x0);
            (x1, y1) = (y1, x1);
        }

        if (x0 > x1)
        {
            (x0, x1) = (x1, x0);
            (y0, y1) = (y1, y0);
        }

        int dx = 2 * (x1 - x0);
        int dy = 2 * Math.Abs(y1 - y0);
        int error = dx;
        int sy = y0 < y1 ? 1 : -1;

        while (x0 <= x1)
        {
            if (steep)
            {
                Framebuffer.SetPixel((uint)y0, (uint)x0, PixelColor);
            }
            else
            {
                Framebuffer.SetPixel((uint)x0, (uint)y0, PixelColor);
            }

            error -= dy;
            if (error < 0)
            {
                y0 += sy;
                error += dx;
            }
            x0++;
        }
    }
}