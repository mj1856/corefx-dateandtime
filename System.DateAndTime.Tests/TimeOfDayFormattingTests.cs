﻿using Xunit;

namespace System.DateAndTime.Tests
{
    public class TimeOfDayFormattingTests
    {
        [Fact]
        public void ToLongDateString()
        {
            var time = new TimeOfDay(10, 49, 12, Meridiem.PM);
            var s = time.ToLongTimeString();
            Assert.Equal("10:49:12 PM", s);
        }

        [Fact]
        public void ToShortDateString()
        {
            var time = new TimeOfDay(22, 49);
            var s = time.ToShortTimeString();
            Assert.Equal("10:49 PM", s);
        }

        [Fact]
        public void ToStringWithStandardTimeFormat()
        {
            var time = new TimeOfDay(23, 59, 59);
            var s = time.ToString("t");
            Assert.Equal("11:59 PM", s);
        }

        [Fact]
        public void ToStringWithCustomTimeFormat()
        {
            var time = new TimeOfDay(23, 59, 59);
            var s = time.ToString("HH:mm:ss");
            Assert.Equal("23:59:59", s);
        }

        [Fact]
        public void ToStringWithCustomDateTimeFormat()
        {
            Assert.Throws<FormatException>(() =>
            {
                var time = new TimeOfDay(23, 59, 59);
                var s = time.ToString("yyyy-MM-dd HH:mm:ss");
            });
        }

        [Fact]
        public void ToStringWithStandardDateFormat()
        {
            Assert.Throws<FormatException>(() =>
            {
                var time = new TimeOfDay(23, 59, 59);
                var s = time.ToString("d");
            });
        }

        [Fact]
        public void ToStringWithCustomDateFormat()
        {
            Assert.Throws<FormatException>(() =>
            {
                var time = new TimeOfDay(23, 59, 59);
                var s = time.ToString("dd MMM yyyy");

            });
        }
    }
}
