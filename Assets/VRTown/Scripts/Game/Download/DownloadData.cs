using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using GLTFast.Loading;
using UnityEngine;

namespace VRTown.Model
{
    public interface UrlTransform
    {
        string Transform(string url);
        string ReverseTransform(string url);
    }

    public class DinoUrlTransform : UrlTransform
    {
        readonly Regex FromRegex = new Regex(@"zip/pointer\?pointer=(-?\d+),(-?\d+)(&ext=.zip)?");
        readonly Regex ToRegex = new Regex(@"zip/pointer/(-?\d+)/(-?\d+).zip");

        public string Transform(string url)
        {
            return FromRegex.Replace(url, "zip/pointer/$1/$2.zip");
        }

        public string ReverseTransform(string url)
        {
            return ToRegex.Replace(url, "zip/pointer?pointer=$1,$2&ext=.zip");
        }
    }

    public class UrlTransformAll : UrlTransform
    {
        public UrlTransform[] transforms = { new DinoUrlTransform() };
        public string Transform(string url)
        {
            foreach (var transform in transforms)
            {
                var url2 = transform.Transform(url);
                if (url2 != null && !url.Equals(url2)) return url2;
            }
            return url;
        }

        public string ReverseTransform(string url)
        {
            foreach (var transform in transforms)
            {
                var url2 = transform.ReverseTransform(url);
                if (url2 != null && !url.Equals(url2)) return url2;
            }
            return url;
        }
    }

    public class ZipDownload : GLTFast.Loading.IDownload
    {
        readonly ZipArchive zip;
        readonly string path;

        #region Properties
        public bool Success => zip != null ? true : false;
        public string Error => zip == null ? "Error" : null;
        public byte[] Data { get { OpenData(); return _zipData; } }
        public string Text { get { OpenData(); return _textData; } }
        public bool? IsBinary { get { return !path.EndsWith("gltf"); } }
        #endregion Properties

        bool _isOpen = false;
        byte[] _zipData = null;
        string _textData = null;

        public ZipDownload(ZipArchive zip, string path)
        {
            this.zip = zip;
            this.path = path;
        }


        void OpenData()
        {
            if (!_isOpen)
            {
                var entry = zip.GetEntry(path);
                if (entry == null) UnityEngine.Debug.LogError($"[DOWNLOAD_DATA] {path} is NULL!");

                _isOpen = true;
                _zipData = new byte[entry.Length];
                var stream = entry.Open();
                stream.Read(_zipData, 0, _zipData.Length);
                _textData = Encoding.Default.GetString(_zipData);
            }
        }

        public object Current
        {
            get
            {
                return null;
            }
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }

        public void Dispose()
        {
            
        }
    }

    public class ZipTextureDownload : ZipDownload, ITextureDownload
    {
        public ZipTextureDownload(ZipArchive zip, string path) : base(zip, path)
        {
        }

        private Texture2D t;
        public Texture2D Texture
        {
            get
            {
                if (!t)
                {
                    t = new Texture2D(2, 2);
                    var _data = Data;
                    if (_data == null) return null;
                    t.LoadImage(_data);
                }
                return t;
            }
        }
    }
}