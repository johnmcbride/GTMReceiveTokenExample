using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTMReceiveTokenExample
{
    public class GTMAccessToken
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string organizer_key { get; set; }
        public string account_key { get; set; }
        public string account_type { get; set; }
    }
}
