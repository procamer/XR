using Assimp;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace XR
{
    public class Texture : IDisposable
    {
        private Image image;
        private int gl;
        private readonly object _lock = new object();

        public int ID => gl;

        public Texture(string originalFile, string baseDir)
        {
            if (Settings.Core.Default.LoadTextures)
            {
                TextureLoader(ObtainPath(originalFile, baseDir));
            }
        }

        public Texture(EmbeddedTexture dataSource)
        {
            if (Settings.Core.Default.LoadTextures)
            {
                TextureLoader(dataSource);
            }
        }

        public Texture(StringCollection faces)
        {
            gl = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, gl);
            TextureLoader(faces);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (float)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (float)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)OpenTK.Graphics.ES10.TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)OpenTK.Graphics.ES10.TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge);
        }

        private void TextureLoader(StringCollection faces)
        {
            for (int i = 0; i < faces.Count; i++)
            {
                using (var image = new Bitmap(faces[i]))
                {
                    var data = image.LockBits(
                        new Rectangle(0, 0, image.Width, image.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                    if (data.Scan0 != null)
                    {
                        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i,
                            0,
                            PixelInternalFormat.Rgb,
                            image.Width,
                            image.Height,
                            0,
                            OpenTK.Graphics.OpenGL.PixelFormat.Bgr,
                            PixelType.UnsignedByte,
                            data.Scan0);
                    }
                    else
                    {
                        throw new Exception("Doku yükleme baþarýsýz oldu!");
                    }
                }

            }
        }

        private void TextureLoader(string path)
        {
            try
            {
                string ext = Path.GetExtension(path);
                if (File.Exists(path))
                {
                    switch (ext)
                    {
                        case ".tga":
                            SetFromTGA(path);
                            return;
                        default:
                            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                            SetFromStream(stream);
                            return;
                    }
                }
                else
                {
                    SetFromMissingTextureColor();
                }

            }
            catch (Exception)
            {
            }
        }

        private void TextureLoader(EmbeddedTexture tex)
        {
            if (tex.IsCompressed)
            {
                EmbeddedTexture compTex = tex;
                if (!compTex.HasCompressedData)
                {
                    return;
                }

                // note: have to keep the stream open for the lifetime of the image, so don't Dispose()
                SetFromStream(new MemoryStream(compTex.CompressedData));
                return;
            }

            var rawTex = tex;
            if (!rawTex.HasNonCompressedData || rawTex.Width < 1 || rawTex.Height < 1)
            {
                return;
            }
            var texels = rawTex.NonCompressedData;


            var image = new Bitmap(rawTex.Width, rawTex.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var bounds = new Rectangle(0, 0, rawTex.Width, rawTex.Height);
            BitmapData bmpData;

            try
            {
                bmpData = image.LockBits(bounds, ImageLockMode.WriteOnly, image.PixelFormat);
                // ignore exceptions thrown by LockBits - we just can't read the image in this case
            }
            catch
            {
                return;
            }

            var ptr = bmpData.Scan0;

            Debug.Assert(bmpData.Stride > 0);

            var countBytes = bmpData.Stride * image.Height;
            var tempBuffer = new byte[countBytes];

            var dataLineLength = image.Width * 4;
            var padding = bmpData.Stride - dataLineLength;
            Debug.Assert(padding >= 0);

            var n = 0;
            foreach (var texel in texels)
            {
                tempBuffer[n++] = texel.B;
                tempBuffer[n++] = texel.G;
                tempBuffer[n++] = texel.R;
                tempBuffer[n++] = texel.A;

                if (n % dataLineLength == 0)
                {
                    n += padding;
                }
            }

            Marshal.Copy(tempBuffer, 0, ptr, countBytes);
            image.UnlockBits(bmpData);

            this.image = image;

        }

        private string ObtainPath(string name, string basedir)
        {
            string path = Path.Combine(basedir, name);
            if (File.Exists(path)) { return path; }

            path = Path.Combine(basedir, Path.GetFileName(name));
            if (File.Exists(path)) { return path; }
            return path;
        }

        private void SetFromStream(Stream stream)
        {
            try
            {
                using (Image img = Image.FromStream(stream))
                {
                    image = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (Graphics gfx = Graphics.FromImage(image))
                    {
                        gfx.DrawImage(img, 0, 0, img.Width, img.Height);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void SetFromMissingTextureColor()
        {
            try
            {
                image = new Bitmap(5, 5, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics gfx = Graphics.FromImage(image))
                {
                    gfx.Clear(Settings.Core.Default.MissingTextureColor);
                }
            }
            catch (Exception)
            {
            }
        }

        private void SetFromTGA(string path)
        {
            try
            {
                Bitmap bitmap = Paloma.TargaImage.LoadTargaImage(path);
                image = bitmap;
                using (Graphics gfx = Graphics.FromImage(image))
                {
                    gfx.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
                }
            }
            catch (Exception)
            {
            }
        }

        public void UploadTexture(TextureUnit textureUnit)
        {
            // this may be required if ReleaseUpload() has been called before
            if (gl != 0)
            {
                GL.DeleteTexture(gl);
                gl = 0;
            }

            lock (_lock)
            { // this is a long CS, but at this time we don't expect concurrent action.
                // http://www.opentk.com/node/259
                Bitmap bitmap = null;
                bool shouldDisposeBitmap = false;

                // in order to LockBits(), we need to create a Bitmap. In case the given Image
                // *is* already a Bitmap however, we can directly re-use it.
                try
                {
                    if (image is Bitmap bitmapImg)
                    {
                        bitmap = bitmapImg;
                    }
                    else if (image != null)
                    {
                        bitmap = new Bitmap(image);
                        shouldDisposeBitmap = true;
                    }
                    else
                    {
                        SetFromMissingTextureColor();
                        bitmap = new Bitmap(image);
                        shouldDisposeBitmap = true;
                    }

                    GL.GetError();

                    // apply texture resolution bias? (i.e. low quality textures)
                    if (Settings.Graphics.Default.TexQualityBias > 0)
                    {
                        Bitmap bitmapBias = ApplyResolutionBias(bitmap, Settings.Graphics.Default.TexQualityBias);
                        if (shouldDisposeBitmap)
                        {
                            bitmap.Dispose();
                        }
                        bitmap = bitmapBias;
                        shouldDisposeBitmap = true;
                    }

                    BitmapData bitmapData = bitmap.LockBits(
                        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb
                     );

                    //// determine alpha pixels if this has not been done before
                    //if (_alphaState == AlphaState.NotKnownYet)
                    //{
                    //    _alphaState = LookForAlphaBits(textureData) ? AlphaState.HasAlpha : AlphaState.Opaque;
                    //}

                    GL.GenTextures(1, out int tex);

                    GL.ActiveTexture(textureUnit);
                    GL.BindTexture(TextureTarget.Texture2D, tex);

                    ConfigureFilters();

                    // upload
                    GL.TexImage2D(
                        TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, //Four,
                        bitmap.Width, bitmap.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        bitmapData.Scan0);

                    bitmap.UnlockBits(bitmapData);

                    // set final state only if the Gl texture object has been filled successfully
                    if (GL.GetError() == ErrorCode.NoError) { gl = tex; }
                }
                finally
                {
                    if (shouldDisposeBitmap)
                    {
                        bitmap.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Obtain a downscaled version of a given Bitmap. The downscaling
        /// factor is expressed as an log2 resolution bias.
        /// </summary>
        /// <param name="textureBitmap"></param>
        /// <param name="bias">Value greater 0</param>
        /// <returns></returns>
        private static Bitmap ApplyResolutionBias(Bitmap textureBitmap, int bias)
        {
            Debug.Assert(textureBitmap != null);
            Debug.Assert(bias > 0);

            var width = textureBitmap.Width >> bias;
            var height = textureBitmap.Height >> bias;

            var b = new Bitmap(width, height);
            using (var g = Graphics.FromImage(b))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(textureBitmap, 0, 0, width, height);
            }

            return b;
        }

        private void ConfigureFilters()
        {
            // assuming Gl texture is bound!
            var settings = Settings.Graphics.Default;
            var mips = settings.UseMips;

            switch (settings.TextureFilter)
            {
                // full aniso
                case 3:
                // low aniso
                case 2:
                // linear
                case 1:
                    GL.TexParameter(TextureTarget.Texture2D,
                        TextureParameterName.TextureMinFilter,
                        (int)(mips ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));

                    GL.TexParameter(TextureTarget.Texture2D,
                        TextureParameterName.TextureMagFilter,
                        (int)TextureMagFilter.Linear);
                    break;

                // point
                case 0:
                    GL.TexParameter(TextureTarget.Texture2D,
                        TextureParameterName.TextureMinFilter,
                        (int)(mips ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest));

                    GL.TexParameter(TextureTarget.Texture2D,
                        TextureParameterName.TextureMagFilter,
                        (int)TextureMagFilter.Nearest);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            // select anisotropic filtering if needed
            if (settings.TextureFilter >= 2)
            {
                GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out float maxAniso);
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt,
                    settings.TextureFilter >= 3 ? maxAniso : maxAniso * 0.5f);
            }
            else
            {
                GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt,
                    0.0f);
            }

            // generate MIPs?
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, mips ? 1 : 0);
            if (!mips)
            {
                return;
            }

            // already uploaded before? need glGenerateMipMap to update
            //if (State == TextureState.GlTextureCreated)
            //{
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            //}
        }

        public void Dispose()
        {
            if (gl != 0)
            {
                GL.DeleteTexture(gl);
                gl = 0;
            }
            if (image != null)
            {
                image.Dispose();
                image = null;
            }
            GC.SuppressFinalize(this);
        }

    }
}
