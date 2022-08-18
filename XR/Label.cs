using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace XR
{
    public class Label : IDisposable
    {
        private Shader shader;
        private Bitmap textBmp;
        private Graphics tempContext;
        private bool disposed;
        private int gl;
        private string _text;

        public bool WantRedraw
        {
            get { return tempContext != null; }
        }

        public bool WantRedrawNextFrame { get; set; }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                Graphics graphics = GetDrawableGraphicsContext();
                graphics.DrawString(_text, new Font("Segoe UI", 24), new SolidBrush(Color.Red), 5, 5);
            }
        }

        public Label()
        {
            shader = new Shader(@"Resources/Shaders/LabelVert.glsl", @"Resources/Shaders/LabelFrag.glsl");

            UploadBuffers();

            // Create Bitmap and OpenGL texture
            var cs = MainWindow.RenderSize;
            textBmp = new Bitmap(cs.Width, cs.Height); // match window size

            gl = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gl);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);

            // just allocate memory, so we can update efficiently using TexSubImage2D
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textBmp.Width, textBmp.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        }

        /// <summary>
        /// Invoked by the Renderer class when the size of the Gl control changes.
        /// This will automatically clear all text.
        /// </summary>
        public void Resize()
        {
            var cs = MainWindow.RenderSize;
            if (cs.Width == 0 || cs.Height == 0) { return; }

            textBmp.Dispose();
            textBmp = new Bitmap(cs.Width, cs.Height);
            GL.BindTexture(TextureTarget.Texture2D, gl);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, textBmp.Width, textBmp.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            GetDrawableGraphicsContext();
        }

        /// <summary>
        /// Obtain a drawable context so the caller can draw text and mark the text
        /// content as dirty to enforce automatic updating of the underlying Gl
        /// resources.
        /// </summary>
        /// <returns>Context to draw to or a null if resources have been
        ///    disposed already.</returns>
        public Graphics GetDrawableGraphicsContext()
        {
            if (disposed) { return null; }

            if (tempContext == null)
            {
                try
                {
                    tempContext = Graphics.FromImage(textBmp);
                }
                catch (Exception)
                {
                    // this happens when _textBmp is not a valid bitmap. It seems, this
                    // can happen if the application is inactive and then switched to.
                    // todo: find out how to avoid this.
                    tempContext = null;
                    return null;
                }

                tempContext.Clear(Color.Transparent);
                tempContext.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            }

            return tempContext;
        }


        ///// <summary>
        ///// Clears the entire overlay
        ///// </summary>
        //public void Clear()
        //{
        //    if (_tempContext == null)
        //    {
        //        _tempContext = Graphics.FromImage(_textBmp);
        //    }
        //    _tempContext.Clear(Color.Transparent);
        //}


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) { return; }
            disposed = true;
            if (disposing)
            {
                if (textBmp != null)
                {
                    textBmp.Dispose();
                    textBmp = null;
                }

                if (tempContext != null)
                {
                    tempContext.Dispose();
                    tempContext = null;
                }
            }
            if (gl > 0)
            {
                GL.DeleteTexture(gl);
                gl = 0;
            }
        }

        public void Render()
        {
            // Updates the GL texture if there were any changes to the .net offscreen buffer. 
            if (tempContext != null)
            {
                Commit();
                tempContext.Dispose();
                tempContext = null;
            }

            shader.Use();
            shader.SetMat4("modelMatrix", Matrix4.CreateTranslation(new Vector3(-1.0f, 1.0f, -1.0f)));

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.BindTexture(TextureTarget.Texture2D, gl);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4); // _verticeCount);
            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);
        }

        /// <summary>
        /// Commit all changes to OpenGl
        /// </summary>
        private void Commit()
        {
            try
            {
                BitmapData data = textBmp.LockBits(new Rectangle(0, 0, textBmp.Width, textBmp.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.BindTexture(TextureTarget.Texture2D, gl);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, textBmp.Width, textBmp.Height,
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                textBmp.UnlockBits(data);
            }
            catch (ArgumentException)
            {
                // this sometimes happens during Shutdown (presumably because Commit gets called
                // after other resources have already been cleaned up). Ignore it because it
                // doesn't matter at this time.
            }
        }

        public void UploadBuffers()
        {
            float[] vertices =
           {
                0,  0,  0,  0,
                1,  0,  1,  0,
                1, -1,  1,  1,
                0, -1,  0,  1
            };

            // VBO
            int VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // positions
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, vertices.Length, 0);
            // texCoord
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertices.Length, sizeof(float) * 2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

        }

    }
}
