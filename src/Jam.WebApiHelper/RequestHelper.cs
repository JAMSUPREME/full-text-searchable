namespace Jam.WebApiHelper
{
    using System.Collections.Specialized;

    /// <summary>
    /// Lightweight helper for doing things with a Request.
    /// </summary>
    public static class RequestHelper
    {
        /// <summary>
        /// Parses a name value collection out of a Request Body that is a URL-encoded form.
        /// </summary>
        /// <param name="body">Request content read as a string.</param>
        /// <returns>A name value collection composed of the form in the body.</returns>
        public static NameValueCollection ParseFormBody(string body)
        {
            var nvColl = new NameValueCollection();
            foreach (string s in body.Split('&'))
            {
                var split = s.Split('=');
                string k = split[0];
                string v = split[1];
                nvColl.Add(k, v);
            }
            return nvColl;
        }
    }
}
