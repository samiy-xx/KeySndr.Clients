using System;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Webkit;
using Android.Widget;

namespace KeySndr.Clients.Mobile.Droid
{
    public class CustomWebClient : WebViewClient
    {
        private readonly Context context;

        private const int TimeOut = 10;

        public CustomWebClient(Context c)
        {
            context = c;
        }

        private bool SkipLoading(string p)
        {
            return (p.ToLower().EndsWith("jpg") || p.ToLower().EndsWith("png") || p.ToLower().EndsWith("jpeg") || p.ToLower().EndsWith("svg"));
        }

        public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
        {
            if (request.Url.Scheme == Uri.UriSchemeFile)
                return null;

            if (SkipLoading(request.Url.LastPathSegment))
                return null;

            var baseResponse = base.ShouldInterceptRequest(view, request);

            try
            {
                var result = Task.Run(async () =>
                {
                    using (var c = new HttpClient { Timeout = TimeSpan.FromSeconds(TimeOut) })
                    {
                        var response = await c.GetAsync(request.Url.ToString());
                        var content = await response.Content.ReadAsStreamAsync();
                        var req = response.RequestMessage;

                        return new WebResourceResponse(baseResponse.MimeType, "UTF-8", (int)response.StatusCode, response.ReasonPhrase, null, content);
                    }
                }).Result;

                return result;
            }
            catch (AggregateException e)
            {
                return baseResponse;
            }

        }

        public override void OnPageStarted(WebView view, string url, Bitmap favicon)
        {
            base.OnPageStarted(view, url, favicon);
            Toast.MakeText(context, "Loading url " + url, ToastLength.Short).Show();
        }

        public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
        {
            base.OnReceivedError(view, request, error);
            Toast.MakeText(context, "Error loading " + error.ErrorCode + " " + error.Description, ToastLength.Long);
        }

        public override void OnPageFinished(WebView view, string url)
        {
            base.OnPageFinished(view, url);
            Toast.MakeText(context, "Loaded", ToastLength.Short).Show();
        }
    }
}