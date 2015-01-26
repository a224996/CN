using System.Drawing;
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


namespace JeonUtility
{
    class JImage
    {
        public static Bitmap CropToCircle(Bitmap srcImage)
        {
            var source = new Bitmap(srcImage.Width,srcImage.Height);
            var cropRect = new Rectangle(0, 0, srcImage.Width, srcImage.Width);

            Bitmap sourceImage = srcImage;
                
            using (Graphics g = Graphics.FromImage(source))
            {
                g.FillEllipse(new TextureBrush(sourceImage.Clone(cropRect, sourceImage.PixelFormat)), 0, 0, srcImage.Width, srcImage.Width);
                g.DrawEllipse( new Pen(Color.PaleVioletRed, 5), 0, 0, srcImage.Width, srcImage.Width);
                g.Dispose();
            }

            sourceImage.Dispose();
            return (Bitmap)source;
        }

        public static Bitmap MakeGrayscale(Bitmap original)
        {
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            Graphics g = Graphics.FromImage(newBitmap);
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][] 
               {
                   new float[] {.3f, .3f, .3f, 0, 0},
                   new float[] {.59f, .59f, .59f, 0, 0},
                   new float[] {.11f, .11f, .11f, 0, 0},
                   new float[] {0, 0, 0, 1, 0},
                   new float[] {0, 0, 0, 0, 1}
               });
            ImageAttributes attributes = new ImageAttributes();

            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            g.Dispose();
            return newBitmap;
        }

    }
}
