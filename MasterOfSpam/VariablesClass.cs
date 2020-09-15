using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodePasswordDLL;

namespace MasterOfSpam
{
    public static class VariablesClass
    {
        public static Dictionary<string, string> Senders
        {
            get { return dicSenders; }
        }

        private static Dictionary<string, string> dicSenders = new Dictionary<string, string>()
        {
            { "test@vshatunoff.ru", PasswordClass.GetPassword(PasswordClass.GetCodePassword("ntcnjdsqzobr")) }
        };

        public static Dictionary<string, int> SmtpServers
        {
            get { return dicServers; }
        }

        private static Dictionary<string, int> dicServers = new Dictionary<string, int>()
        {
            { "smtp.yandex.ru", 25 }
        };
    }
}
