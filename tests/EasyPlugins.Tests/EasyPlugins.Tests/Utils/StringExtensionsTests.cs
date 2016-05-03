using NUnit.Framework;
using EasyPlugins.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugins.Utils
{
    [TestFixture()]
    public class StringExtensionsTests
    {
        [Test()]
        public void ToAlphaNumericTest()
        {
            var str = @"~!@#$%^&*()_+`1234567890QWERTYUIOP{}|qwertyuiop[]\\ASDFGHJKL:""asdfghjkl;'ZXCVBNM<>?zxcvbnm,./";
            Assert.That(str.ToAlphaNumeric(), Is.EqualTo(@"1234567890QWERTYUIOPqwertyuiopASDFGHJKLasdfghjklZXCVBNMzxcvbnm"));
            Assert.That(str.ToAlphaNumeric('-'), Is.EqualTo(@"-1234567890QWERTYUIOP-qwertyuiop-ASDFGHJKL-asdfghjkl-ZXCVBNM-zxcvbnm-"));
            Assert.That(str.ToAlphaNumeric('-', '~', '!', '[', ']', '\\'), Is.EqualTo(@"~!-1234567890QWERTYUIOP-qwertyuiop[]\\ASDFGHJKL-asdfghjkl-ZXCVBNM-zxcvbnm-"));
        }
    }
}