using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodePasswordDLL
{
    public static class PasswordClass
    {
        public static string GetPassword(string pass)
        {
            string password = String.Empty;
            foreach (char a in pass)
            {
                char ch = a;
                ch--;
                password += ch;
            }
            return password;
        }

        public static string GetCodePassword(string pass)
        {
            string password = String.Empty;
            foreach (char a in pass)
            {
                char ch = a;
                ch++;
                password += ch;
            }
            return password;
        }
    }
}
