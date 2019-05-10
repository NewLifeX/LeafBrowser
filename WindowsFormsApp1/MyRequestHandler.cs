using System;
using CefSharp;
using CefSharp.Handler;
using NewLife.Collections;
using NewLife.Log;

namespace LeafBrower
{
    internal class MyRequestHandler : DefaultRequestHandler
    {
        public Action<IRequest, IResponse, String> OnComplete;

        private readonly DictionaryCache<UInt64, IResponseFilter> _filters = new DictionaryCache<UInt64, IResponseFilter> { Expire = 60, Period = 60 };

        private String[] _blocks = new[] {
            "pos.baidu.com", "crs.baidu.com", "hm.baidu.com", "em.baidu.com", "eclick.baidu.com", "baidustatic.com",
            "googlesyndication.com","googletagservices.com",
            "b.qq.com" };

        public override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
            {
                //If we're unable to parse the Uri then cancel the request
                // avoid throwing any exceptions here as we're being called by unmanaged code
                return CefReturnValue.Cancel;
            }

            // 过滤广告
            if (uri.Host.EndsWithIgnoreCase(_blocks)) return CefReturnValue.Cancel;
            if (uri.PathAndQuery.StartsWithIgnoreCase("/Ad/")) return CefReturnValue.Cancel;

            //System.Diagnostics.Debug.WriteLine(request.ResourceType.ToString());
            //System.Diagnostics.Debug.WriteLine(url);

            XTrace.WriteLine("Load [{0}] {1}", request.ResourceType, uri);

            //Uri url;
            //if (Uri.TryCreate(request.Url, UriKind.Absolute, out url) == false)
            //{
            //    //If we're unable to parse the Uri then cancel the request
            //    // avoid throwing any exceptions here as we're being called by unmanaged code
            //    return CefReturnValue.Cancel;
            //}

            ////Example of how to set Referer
            //// Same should work when setting any header

            //// For this example only set Referer when using our custom scheme
            //if (url.Scheme == CefSharpSchemeHandlerFactory.SchemeName)
            //{
            //    //Referrer is now set using it's own method (was previously set in headers before)
            //    request.SetReferrer("http://google.com", ReferrerPolicy.Default);
            //}

            ////Example of setting User-Agent in every request.
            ////var headers = request.Headers;

            ////var userAgent = headers["User-Agent"];
            ////headers["User-Agent"] = userAgent + " CefSharp";

            ////request.Headers = headers;

            ////NOTE: If you do not wish to implement this method returning false is the default behaviour
            //// We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
            ////callback.Dispose();
            ////return false;

            ////NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            //if (!callback.IsDisposed)
            //{
            //    using (callback)
            //    {
            //        if (request.Method == "POST")
            //        {
            //            using (var postData = request.PostData)
            //            {
            //                if (postData != null)
            //                {
            //                    var elements = postData.Elements;

            //                    var charSet = request.GetCharSet();

            //                    foreach (var element in elements)
            //                    {
            //                        if (element.Type == PostDataElementType.Bytes)
            //                        {
            //                            var body = element.GetBody(charSet);
            //                        }
            //                    }
            //                }
            //            }
            //        }

            //        //Note to Redirect simply set the request Url
            //        //if (request.Url.StartsWith("https://www.google.com", StringComparison.OrdinalIgnoreCase))
            //        //{
            //        //    request.Url = "https://github.com/";
            //        //}

            //        //Callback in async fashion
            //        //callback.Continue(true);
            //        //return CefReturnValue.ContinueAsync;
            //    }
            //}

            return CefReturnValue.Continue;
        }

        public override IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            if (request.ResourceType == ResourceType.Xhr)
            {
                var filter = new MyResponseFilter();
                _filters.Set(request.Identifier, filter);
                return filter;
            }

            return null;
        }

        public override void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, Int64 receivedContentLength)
        {
            if (status == UrlRequestStatus.Success && request.ResourceType == ResourceType.Xhr && receivedContentLength > 0)
            {
                XTrace.WriteLine("LoadComplete [{0}] {1}", receivedContentLength, request.Url);
                //var html = frame.GetTextAsync().Result;
                //frame.GetTextAsync().ContinueWith(t =>
                //{
                //    var html = t.Result;
                //});
                var filter = _filters[request.Identifier];
                if (filter is MyResponseFilter rf)
                {
                    var ms = rf.Stream;
                    ms.Position = 0;

                    var html = ms.ToStr();
                    if (!html.IsNullOrEmpty()) OnComplete?.Invoke(request, response, html);
                }
            }
        }
    }
}