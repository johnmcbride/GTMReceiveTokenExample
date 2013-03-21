using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GTMReceiveTokenExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void web1_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            GTMAccessToken _gtmAccessToken = null;

            if (e.Uri.Query.ToLower().IndexOf("code") != -1 )
            {
                //get the "code" from GTM
                string _code = e.Uri.Query.Replace("?code=", "");
                e.Cancel = true;

                var _rc = new RestSharp.RestClient(@"https://api.citrixonline.com");
                RestSharp.RestRequest _gtmTokenCodeReq = new RestSharp.RestRequest("/oauth/access_token?grant_type=authorization_code&code={responseKey}&client_id={api_key}", RestSharp.Method.GET);
                _gtmTokenCodeReq.AddUrlSegment("responseKey", _code);
                _gtmTokenCodeReq.AddUrlSegment("api_key", this.gtmAPIKey.Text);

                var _gtmTokenCodeResp = _rc.Execute(_gtmTokenCodeReq);

                if (_gtmTokenCodeResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var jsonCode = _gtmTokenCodeResp.Content;
                    _gtmAccessToken = Newtonsoft.Json.JsonConvert.DeserializeObject<GTMAccessToken>(jsonCode);
                }

                //now we have the token. Create a meeting
                var _gtmMeetingReq = new RestSharp.RestRequest(@"/G2M/rest/meetings", RestSharp.Method.POST);
                _gtmMeetingReq.AddHeader("Accept", "application/json");
                _gtmMeetingReq.AddHeader("Content-type", "application/json");
                _gtmMeetingReq.AddHeader("Authorization", string.Format("OAuth oauth_token={0}", _gtmAccessToken.access_token));

                //creating the meeting request json for the request.
                Newtonsoft.Json.Linq.JObject _meetingRequestJson = new Newtonsoft.Json.Linq.JObject();
                _meetingRequestJson.Add(new Newtonsoft.Json.Linq.JProperty("subject", "Immediate Meeting"));
                _meetingRequestJson.Add(new Newtonsoft.Json.Linq.JProperty("starttime", DateTime.UtcNow.AddSeconds(30).ToString("s")));
                _meetingRequestJson.Add(new Newtonsoft.Json.Linq.JProperty("endtime", DateTime.UtcNow.AddHours(1).AddSeconds(30).ToString("s")));
                _meetingRequestJson.Add(new Newtonsoft.Json.Linq.JProperty("passwordrequired", "false"));
                _meetingRequestJson.Add(new Newtonsoft.Json.Linq.JProperty("conferencecallinfo", "Hybrid"));
                _meetingRequestJson.Add(new Newtonsoft.Json.Linq.JProperty("timezonekey", ""));
                _meetingRequestJson.Add(new Newtonsoft.Json.Linq.JProperty("meetingtype", "Immediate"));

                //converting the jobject back to string for use within the request
                string gtmJSON = Newtonsoft.Json.JsonConvert.SerializeObject(_meetingRequestJson);

                _gtmMeetingReq.AddParameter("text/json", gtmJSON, RestSharp.ParameterType.RequestBody);

                var _responseMeetingRequest = _rc.Execute(_gtmMeetingReq);

                if (_responseMeetingRequest.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    // meeting created to add a message, format it and add it to the list
                    string _meetingResponseJson = _responseMeetingRequest.Content;

                    Newtonsoft.Json.Linq.JArray _meetingResponse = Newtonsoft.Json.Linq.JArray.Parse(_meetingResponseJson);
                    var _gtmMeetingObject = _meetingResponse[0];

                    MessageBox.Show(_gtmMeetingObject["joinURL"].ToString());
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string _gtmTokenApiURL = string.Format("https://api.citrixonline.com/oauth/authorize?client_id={0}", this.gtmAPIKey.Text);
            this.web1.Navigate(_gtmTokenApiURL);
        }
    }
}
