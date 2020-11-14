using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using LogShift;


namespace UnitTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void SimpleTest()
        {
            var buf = "11/12/2020 2:34:34 PM Test line";
            buf = Program.ShiftLog(buf);
            buf.Should().EndWith(" Test line\r\n");
            buf.Should().Contain("(TIME)00:00:00");
        }

        [TestMethod]
        public void TwoLineOffsetTest()
        {
            var buf = @"
11/12/2020 2:34:00 PM Test line
11/12/2020 2:34:34.78 PM Test line2
";
            buf = Program.ShiftLog(buf);
            buf.Should().EndWith(" Test line2\r\n\r\n");
            buf.Should().Contain("(TIME)00:00:00");
            buf.Should().Contain("(TIME)00:00:34.78");
        }


        [TestMethod]
        public void ThreeStampsOneLineTest()
        {
            var buf = @"11/12/2020 2:34:00 PM Test line, another stamp: 11/12/2020 2:34:04.432 PM  data is 11/12/2019 2:34:00 PM More stuff";
            buf = Program.ShiftLog(buf);
            buf.Should().Contain("(TIME)00:00:00");
            buf.Should().Contain("(TIME)00:00:04.432");
            buf.Should().Contain("11/12/2019 2:34:00 PM");
        }

    }
}
