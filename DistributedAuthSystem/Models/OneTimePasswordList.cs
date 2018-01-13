using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DistributedAuthSystem.Models
{
    public class OneTimePasswordList
    {
        #region fields

        private static readonly Random _random;

        private const string _allowedChars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";

        private const int _listSize = 10;

        private const int _passwordLength = 10;

        private string[] _passwords;

        private int _currentIndex;

        #endregion

        #region properties

        public string CurrentPassword
        {
            get => _currentIndex == _listSize ? null : _passwords[_currentIndex];
        }

        public bool IsLast
        {
            get => _currentIndex == _listSize - 1;
        }

        #endregion

        #region methods

        static OneTimePasswordList()
        {
            _random = new Random();
        }

        private static string RandomString(int length)
        {
            return new string(Enumerable.Repeat(_allowedChars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public OneTimePasswordList()
        {
            _passwords = new string[_listSize];
            for (int i = 0; i < _listSize; ++i)
            {
                _passwords[i] = RandomString(_passwordLength);
            }
            _currentIndex = 0;
        }

        public void UseCurrentPassword()
        {
            if (_currentIndex == _listSize)
            {
                return;
            }
            ++_currentIndex;
        }

        #endregion
    }
}