using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using LogShift;
using System.IO;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class ParserTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Program.BaseTimeStamp = null;
        }


        [TestMethod]
        public void SimpleTest()
        {
            var buf = "11/12/2020 2:34:34 PM Test line";
            buf = Program.ShiftLine(buf);
            buf.Should().EndWith(" Test line");
            buf.Should().Contain("(TIME)00:00:00");
        }

        [TestMethod]
        public void TwoLineOffsetTest()
        {
            var line1 = @"11/12/2020 2:34:00 PM Test line";
            var line2 = @"11/12/2020 2:34:34.78 PM Test line2";
            line1 = Program.ShiftLine(line1);
            line2 = Program.ShiftLine(line2);
            line2.Should().EndWith(" Test line2");
            line1.Should().Contain("(TIME)00:00:00");
            line2.Should().Contain("(TIME)00:00:34.78");
        }


        [TestMethod]
        public void ThreeStampsOneLineTest()
        {
            var buf = @"11/12/2020 2:34:00 PM Test line, another stamp: 11/12/2020 2:34:04.432 PM  data is 11/12/2019 2:34:00 PM More stuff";
            buf = Program.ShiftLine(buf);
            buf.Should().Contain("(TIME)00:00:00");
            buf.Should().Contain("(TIME)00:00:04.432");
            buf.Should().Contain("11/12/2019 2:34:00 PM");
        }
    }
}
