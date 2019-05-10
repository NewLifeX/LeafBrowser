using System;
using System.Collections.Generic;
using System.IO;
using CefSharp;

namespace LeafBrower
{
    internal class MyResponseFilter : IResponseFilter
    {
        private Int32 contentLength = -1;
        public Stream Stream { get; } = new MemoryStream();

        public void SetContentLength(Int32 contentLength)
        {
            this.contentLength = contentLength;
        }

        public FilterStatus Filter(Stream dataIn, out Int64 dataInRead, Stream dataOut, out Int64 dataOutWritten)
        {
            try
            {
                if (dataIn == null)
                {
                    dataInRead = 0;
                    dataOutWritten = 0;

                    return FilterStatus.Done;
                }

                dataInRead = dataIn.Length;
                dataOutWritten = Math.Min(dataInRead, dataOut.Length);

                dataIn.CopyTo(dataOut);
                dataIn.Seek(0, SeekOrigin.Begin);

                var ms = Stream;
                dataIn.CopyTo(ms);

                if (ms.Length == contentLength)
                {
                    ms.Position = 0;

                    return FilterStatus.Done;
                }
                else if (contentLength < 0 || ms.Length < contentLength)
                {
                    dataInRead = dataIn.Length;
                    dataOutWritten = dataIn.Length;

                    return FilterStatus.NeedMoreData;
                }
                else
                {
                    return FilterStatus.Error;
                }
            }
            catch (Exception)
            {
                dataInRead = dataIn.Length;
                dataOutWritten = dataIn.Length;

                return FilterStatus.Done;
            }
        }

        public Boolean InitFilter() => true;

        public void Dispose() { }
    }
}
