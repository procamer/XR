
using System;
using System.Collections.Generic;

namespace XR
{
    public sealed class TextureSet : IDisposable
    {
        private readonly string baseDir;
        private readonly Dictionary<string, Texture> dict;
        private bool disposed;

        public TextureSet(string baseDir)
        {
            this.baseDir = baseDir;
            dict = new Dictionary<string, Texture>();
        }

        public void Add(string path, Assimp.EmbeddedTexture embeddedDataSource = null)
        {
            if (dict.ContainsKey(path)) return;

            dict.Add(path, embeddedDataSource == null ?
                 new Texture(path, baseDir) :
                 new Texture(embeddedDataSource));
        }

        public Texture GetTexture(string path)
        {
            if (path == null) return null;
            return dict[path];
        }

        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;
            foreach (KeyValuePair<string, Texture> k in dict)
            {
                k.Value.Dispose();
            }

            //lock (_loaded) // TODO don't know if this is safe here
            //{
            dict.Clear();
            //    _loaded.Clear();
            //}
        }

    }
}
