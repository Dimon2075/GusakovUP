using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GusakovUP.Services
{
    public class CaptchaResult
    {
        public string Text { get; set; }
        public ImageSource Image { get; set; } 
    }

    public class CaptchaService
    {
        private static readonly Random _random = new Random();

        public static CaptchaResult GenerateCaptcha(int width, int height)
        {
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            char[] code = new char[4];
            for (int i = 0; i < code.Length; i++) code[i] = chars[_random.Next(chars.Length)];
            string captchaText = new string(code);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromRgb(243, 243, 243)), null, new Rect(0, 0, width, height));

                Pen linePen = new Pen(new SolidColorBrush(Color.FromRgb(77, 198, 197)), 1.5);
                for (int i = 0; i < 5; i++)
                {
                    drawingContext.DrawLine(linePen, new Point(_random.Next(width), _random.Next(height)), new Point(_random.Next(width), _random.Next(height)));
                }

                for (int i = 0; i < captchaText.Length; i++)
                {
                    FormattedText formattedText = new FormattedText(
                        captchaText[i].ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        new Typeface("Arial"), 24, new SolidColorBrush(Color.FromRgb(35, 62, 118)), 1.0);

                    double x = 15 + (i * (width / 5)) + _random.Next(-3, 3);
                    double y = (height / 2) - (formattedText.Height / 2) + _random.Next(-3, 3);
                    drawingContext.DrawText(formattedText, new Point(x, y));
                }
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            bmp.Freeze();

            return new CaptchaResult { Text = captchaText, Image = bmp };
        }
    }
}

