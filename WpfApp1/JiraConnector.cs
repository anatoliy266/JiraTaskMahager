using System;
using System.Net;
using System.Collections;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Web;


namespace WpfApp1
{
    enum ProtocolType {
        HTTP,
        HTTPS
    }

    enum RestApiPart
    {
        auth,
        search,
        issue,
        filter
    }

   enum RequestMethod
    {
        GET,
        POST,
        DELETE
    }

    enum SearchType
    {
        myIssues, 
        callCenter,
        hotLine,
        voiceMail
    }

    class JiraConnector
    {
        private string GUrl;
        private string BasicAuth;
        private CookieContainer Cookie = new CookieContainer();
        public string DebugText;

        public JiraConnector(string host, ProtocolType protoc) {
            string protocPart = "";
            try
            {
                switch (protoc)
                {
                    case ProtocolType.HTTP:
                        protocPart = "http://";
                        break;
                    case ProtocolType.HTTPS:
                        protocPart = "https://";
                        break;
                }
                if (host != "")
                {
                    GUrl = protocPart + host;
                }
                else
                {
                    throw new Exception("host are not speified");
                }
            }
            catch (Exception e)
            {
                DebugText = "JiraConnector::JiraConnector() -> " + e.Message;
            }
        }

        private string Get(RestApiPart restApiMethod, RequestMethod method, NameValueCollection fields = null, string filter = null, int maxCount = 1, int startAt = 0)
        {
            CookieContainer cookie = new CookieContainer();
            WebProxy proxy = new WebProxy(GUrl);
            string url = GUrl;
            //string userAgent = "Mozilla / 5.0(Windows NT 6.1; WOW64; rv: 17.0) Gecko / 20100101 Firefox / 17.0";
            string userAgent = "King Anthony";
            string basicAuth = BasicAuth;
            string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            string data = "";
            try
            {
                switch (restApiMethod)
                {
                    case RestApiPart.auth:
                        url += "/rest/auth/1/session";
                        data = "{\"username\":\"" + fields["userName"] + "\", \"password\":\"" + fields["password"] + "\"}";
                        break;
                    case RestApiPart.filter:
                        url += "/rest/api/2/filter?";
                        break;
                    case RestApiPart.issue:
                        url += "/rest/api/2/issue/";
                        break;
                    case RestApiPart.search:
                        url += "/rest/api/2/search?";
                        if (fields != null)
                        {
                            string fieldsUrlPart = fields[fields.Keys[0]];
                            int i = 1; 
                            while (i < fields.Keys.Count)
                            {
                                fieldsUrlPart += "," + fields[fields.Keys[i]];
                            }
                            url += fieldsUrlPart;
                            url += "&";
                        } else
                        {
                            url += "";
                        }
                        if (filter != null)
                        {
                            url += "jql=" + filter;
                            url += "&";
                        } else
                        {
                            url += "";
                        }
                        url += "startAt=" + startAt.ToString() + "&maxResults=" + maxCount.ToString();
                        break;
                }
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = userAgent;
                request.Accept = accept;
                request.Headers.Add("Authorization", "Basic " + BasicAuth);
                request.Headers.Add("DNT", "1");
                request.CookieContainer = cookie;
                request.KeepAlive = false;
                switch (method) {
                    case RequestMethod.GET:
                        request.Method = "GET";
                        break;
                    case RequestMethod.POST:
                        request.Method = "POST";
                        StreamWriter writer = new StreamWriter(request.GetRequestStream());
                        writer.Write(data);
                        break;
                    case RequestMethod.DELETE:
                        request.Method = "DELETE";
                        break;
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string text = reader.ReadToEnd();
                return text;
            } catch (Exception e)
            {
                if (e.Message.Contains("401"))
                {
                    return "JiraConnector::Get -> Unauthorized(401), ";
                } else if (e.Message.Contains("Закрыто"))
                {
                    return "JiraConnector::Get -> Connection Close";
                } else if (e.Message.Contains("403"))
                {
                    return "JiraConnector::Get -> Permission denied";
                } else
                {
                    return "JiraConnector::Get ->" + e.Message;
                }
            }
        }

        public void Auth(string userName, string pwd)
        {
            try
            {
                BasicAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + pwd));
            } catch (Exception e)
            {
                DebugText = "JiraConnector::Auth ->" + e.Message;
            }
            
        }

        public string getIssues(SearchType type)
        {
            try
            {
                string filter = "";
                NameValueCollection searchFields = new NameValueCollection();
                switch (type)
                {
                    case SearchType.myIssues:
                        filter = string.Join("%20", "assignee in (CurrentUser()) order by updated asc".Split(" ".ToCharArray()));
                        break;
                    case SearchType.callCenter:
                        filter = string.Join("%20", "project = HD AND status = \"To Do\" AND reporter in (s.tranzit) and assignee in(fk.support) order by updated asc".Split(" ".ToCharArray()));
                        break;
                    case SearchType.hotLine:
                        filter = string.Join("%20", "summary ~\"Горячая линия.\" AND summary ~\"email\" AND statusCategory not in (Done)AND category not in (\"Фарватер (бизнес)\", \"Фарватер (поддержка)\", \"Фарватер (разработка)\") AND status not in (\"Waiting reporter\") AND assignee in (fk.support)ORDER BY created asc".Split(" ".ToCharArray()));
                        break;
                    case SearchType.voiceMail:
                        filter = string.Join("%20", "summary ~ \"Новое голосовое сообщение\" AND project in (HD) AND status not in (Done, \"Waiting reporter\") ORDER BY created asc".Split(" ".ToCharArray()));
                        break;
                }
                var searchIssuesText = Get(RestApiPart.search, RequestMethod.GET, null, filter, 100, 0);
                return searchIssuesText;
            } catch (Exception e)
            {
                return "JiraConnector::getIssue ->" + e.Message;
            }
        }
    }
}
