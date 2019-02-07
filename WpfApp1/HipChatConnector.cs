using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace WpfApp1
{
    enum IssueIcons
    {
        newTask,
        hotTask,
        rancidTask
    }

    class HipChatConnector
    {
        public HipChatConnector() { }

        private string apiToken = "WZdWuTqTivlm49EFuPySDwUhTIIcXKxBwlfuW24m";

        private string CreateMessage(string _title, string _message)
        {
            string anotherMessage = "\"<table>" +
                                            "<tr>" +
                                                "<td width=10% height=10%>" +
                                                    "<img width=100px src=https://www.clipartmax.com/png/middle/254-2541230_software-carpentry-boot-camp-jira-png.png></img>" +
                                                "</td>" +
                                                "<td width=90%>" +
                                                    "<table>" +
                                                        "<tr>" +
                                                            "<p>"+_title+"</p>" +
                                                        "</tr>" +
                                                        "<tr>" +
                                                            "<td>" +
                                                                "<p>"+_message+"</p>" +
                                                            "</td>" +
                                                        "</tr>" +
                                                    "</table>" +
                                                "</td>" +
                                            "</tr>" +
                                          "</table>\"";
            string jsonContent = "{\"message\":" + anotherMessage + ",\"notify\":true,\"message_format\":\"html\"}";
            return jsonContent;
        }

        public void SendMessage(string user, string title, string message, NameValueCollection properties = null)
        {
            string msg = CreateMessage(title, message);
            //var msg = "{\"message\":\"message\",\"notify\":true,\"message_format\":\"text\"}";
            var url = "https://chat.550550.ru/v2/user/"+user+"/message?auth_token=" + apiToken;
            WebClient hipChatClient = new WebClient();
            hipChatClient.Encoding = Encoding.UTF8;
            //hipChatClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            hipChatClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            hipChatClient.UploadString(url, msg);
        }
    }
}
