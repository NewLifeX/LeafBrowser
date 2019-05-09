using System;
using System.Security.Cryptography.X509Certificates;
using CefSharp;

namespace WindowsFormsApp1
{
    internal class MyRequestHandler : IRequestHandler
    {
        public event Action<String> msg;
        public event Action<String, Object> msg2;
        public Boolean GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, Boolean isProxy,
            String host, Int32 port, String realm, String scheme, IAuthCallback callback) => false;

        public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {

            //if (!response.ResponseHeaders["Content-Type"].Contains("application/json"))
            //{
            return null;
            //}

            //var filter = FilterManager.CreateFilter(request.Identifier.ToString());

            //return filter;
        }

        private void Filter_VOIDFUN(String arg1, String arg2, String arg3, Int64 arg4) => msg2?.Invoke(arg1, arg2);

        public Boolean OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            Boolean isRedirect)
        {
            var m = request.Method;
            msg?.Invoke(request.Url);
            msg?.Invoke(m);
            if (request.Method == "POST")
            {
                using (var postData = request.PostData)
                {
                    if (postData != null)
                    {
                        var elements = postData.Elements;

                        var charSet = request.GetCharSet();

                        foreach (var element in elements)
                        {
                            if (element.Type == PostDataElementType.Bytes)
                            {
                                var body = element.GetBody(charSet);
                                msg?.Invoke(body);
                            }
                        }
                    }
                }
            }

            return false;
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            var m = request.Method;
            msg?.Invoke(request.Url);
            msg?.Invoke(m);
            if (request.Method == "POST")
            {
                using (var postData = request.PostData)
                {
                    if (postData != null)
                    {
                        var elements = postData.Elements;

                        var charSet = request.GetCharSet();

                        foreach (var element in elements)
                        {
                            if (element.Type == PostDataElementType.Bytes)
                            {
                                var body = element.GetBody(charSet);
                                msg?.Invoke(body);
                            }
                        }
                    }
                }
            }

            return CefReturnValue.Continue;
        }

        public Boolean OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, String requestUrl, ISslInfo sslInfo, IRequestCallback callback) => true;

        public Boolean OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, String targetUrl, WindowOpenDisposition targetDisposition, Boolean userGesture) => false;

        public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, String pluginPath) { }

        public Boolean OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, String url) => false;

        public Boolean OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, String originUrl, Int64 newSize, IRequestCallback callback) => false;

        public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status) { }

        public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser) { }

        public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, Int64 receivedContentLength) { }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, ref String newUrl) { }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref String newUrl) { }

        public Boolean OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response) => false;

        public Boolean OnSelectClientCertificate(IWebBrowser browserControl, IBrowser browser, Boolean isProxy, String host, Int32 port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback) => true;

        public Boolean OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, Boolean userGesture, Boolean isRedirect) => true;

        public Boolean CanGetCookies(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request) => true;

        public Boolean CanSetCookie(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, Cookie cookie) => true;
    }
}