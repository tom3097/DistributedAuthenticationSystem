using DistributedAuthSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Extensions
{
    public static class OneTimePasswordListExt
    {
        #region fields

        private const string _allowedChars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";

        private static readonly Random _random;

        private const int _listSize = 10;

        private const int _passwordLength = 10;

        #endregion

        #region methods

        private static string RandomString(int length)
        {
            return new string(Enumerable.Repeat(_allowedChars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static void GeneratePasswords(this OneTimePasswordList oneTimePasswordList)
        {
            oneTimePasswordList.Passwords = new string[_listSize];
            for (int i = 0; i < _listSize; ++i)
            {
                oneTimePasswordList.Passwords[i] = RandomString(_passwordLength);
            }
            oneTimePasswordList.CurrentIndex = 0;
        }

        public static string CurrentPassword(this OneTimePasswordList oneTimePasswordList)
        {
            return oneTimePasswordList.Passwords[oneTimePasswordList.CurrentIndex];
        }

        public static void UseCurrentPassword(this OneTimePasswordList oneTimePasswordList)
        {
            if (oneTimePasswordList.CurrentIndex == _listSize)
            {
                return;
            }

            oneTimePasswordList.CurrentIndex += 1;
        }

        #endregion
    }
}