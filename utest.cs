using Microsoft.VisualStudio.TestTools.UnitTesting;
using Midi;
using System;

namespace Test
{
    [TestClass]
    public class utest
    {
        [TestMethod]
        public void TestMethod()
        {
            MainWindow window = new MainWindow();
            window.Show();
            window.Close();
            Assert.IsFalse(window.IsVisible);
        }
    }
}