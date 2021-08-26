using CefSharp;
using NewLife;
using NewLife.Collections;
using NewLife.Log;
using System;
using System.Security.Cryptography.X509Certificates;

namespace LeafBrower
{
    internal class MyRequestHandler : IRequestHandler, IResourceRequestHandler
    {
        public Action<IRequest, IResponse, String> OnComplete;

        private readonly DictionaryCache<UInt64, IResponseFilter> _filters = new DictionaryCache<UInt64, IResponseFilter> { Expire = 60, Period = 60 };

        public virtual IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            if (request.ResourceType == ResourceType.Xhr)
            {
                var filter = new MyResponseFilter();
                _filters.Set(request.Identifier, filter);
                return filter;
            }

            return null;
        }

        public virtual void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, Int64 receivedContentLength)
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

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect) => false;

        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return this;
        }

        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            callback.Dispose();
            return false;
        }

        public bool OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            callback.Dispose();
            return false;
        }

        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            callback.Dispose();
            return false;
        }

        public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            callback.Dispose();
            return false;
        }

        public void OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath)
        {
        }

        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
        }

        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
        {
        }

        public ICookieAccessFilter GetCookieAccessFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return null;
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Continue;
        }

        public IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return null;
        }

        public void OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {
        }

        public bool OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) => false;

        public bool OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return false;
        }

        public void Dispose()
        {
        }
    }
}