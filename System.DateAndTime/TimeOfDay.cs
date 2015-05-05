﻿using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System
{
    /// <summary>
    /// Represents a time of day, as would be read from a clock, within the range 00:00:00 to 23:59:59.9999999
    /// Has properties for working with both 12-hour and 24-hour time values.
    /// </summary>
    [XmlSchemaProvider("GetSchema")]
    public struct TimeOfDay : IEquatable<TimeOfDay>, IFormattable, IXmlSerializable
    {
        private const long TicksPerMillisecond = 10000;
        private const long TicksPerSecond = TicksPerMillisecond * 1000;   // 10,000,000
        private const long TicksPerMinute = TicksPerSecond * 60;         // 600,000,000
        private const long TicksPerHour = TicksPerMinute * 60;        // 36,000,000,000
        private const long TicksPerDay = TicksPerHour * 24;          // 864,000,000,000

        private const long MinTicks = 0L;
        private const long MaxTicks = 863999999999L;

        public static readonly TimeOfDay MinValue = new TimeOfDay(MinTicks);
        public static readonly TimeOfDay MaxValue = new TimeOfDay(MaxTicks);

        // Number of ticks (100ns units) since midnight at the beginning of a standard 24-hour day.
        // NOTE: This is the only field in this structure.
        private readonly long _ticks;

        public TimeOfDay(long ticks)
        {
            if (ticks < MinTicks || ticks > MaxTicks)
                throw new ArgumentOutOfRangeException("ticks");
            Contract.EndContractBlock();

            _ticks = ticks;
        }

        public TimeOfDay(int hours24, int minutes)
        {
            if (hours24 < 0 || hours24 > 23) throw new ArgumentOutOfRangeException("hours24");
            if (minutes < 0 || minutes > 59) throw new ArgumentOutOfRangeException("minutes");
            Contract.EndContractBlock();

            _ticks = hours24 * TicksPerHour +
                     minutes * TicksPerMinute;
        }

        public TimeOfDay(int hours12, int minutes, Meridiem meridiem)
        {
            if (hours12 < 1 || hours12 > 12) throw new ArgumentOutOfRangeException("hours12");
            if (minutes < 0 || minutes > 59) throw new ArgumentOutOfRangeException("minutes");
            if (meridiem < Meridiem.AM || meridiem > Meridiem.PM) throw new ArgumentOutOfRangeException("meridiem");
            Contract.EndContractBlock();

            int hours24 = Hours12To24(hours12, meridiem);
            _ticks = hours24 * TicksPerHour +
                     minutes * TicksPerMinute;
        }

        public TimeOfDay(int hours24, int minutes, int seconds)
        {
            if (hours24 < 0 || hours24 > 23) throw new ArgumentOutOfRangeException("hours24");
            if (minutes < 0 || minutes > 59) throw new ArgumentOutOfRangeException("minutes");
            if (seconds < 0 || seconds > 59) throw new ArgumentOutOfRangeException("seconds");
            Contract.EndContractBlock();

            _ticks = hours24 * TicksPerHour +
                     minutes * TicksPerMinute +
                     seconds * TicksPerSecond;
        }

        public TimeOfDay(int hours12, int minutes, int seconds, Meridiem meridiem)
        {
            if (hours12 < 1 || hours12 > 12) throw new ArgumentOutOfRangeException("hours12");
            if (minutes < 0 || minutes > 59) throw new ArgumentOutOfRangeException("minutes");
            if (seconds < 0 || seconds > 59) throw new ArgumentOutOfRangeException("seconds");
            if (meridiem < Meridiem.AM || meridiem > Meridiem.PM) throw new ArgumentOutOfRangeException("meridiem");
            Contract.EndContractBlock();

            int hours24 = Hours12To24(hours12, meridiem);
            _ticks = hours24 * TicksPerHour +
                     minutes * TicksPerMinute +
                     seconds * TicksPerSecond;
        }

        public TimeOfDay(int hours24, int minutes, int seconds, int milliseconds)
        {
            if (hours24 < 0 || hours24 > 23) throw new ArgumentOutOfRangeException("hours24");
            if (minutes < 0 || minutes > 59) throw new ArgumentOutOfRangeException("minutes");
            if (seconds < 0 || seconds > 59) throw new ArgumentOutOfRangeException("seconds");
            if (milliseconds < 0 || milliseconds > 999) throw new ArgumentOutOfRangeException("milliseconds");
            Contract.EndContractBlock();

            _ticks = hours24 * TicksPerHour +
                     minutes * TicksPerMinute +
                     seconds * TicksPerSecond +
                     milliseconds * TicksPerMillisecond;
        }

        public TimeOfDay(int hours12, int minutes, int seconds, int milliseconds, Meridiem meridiem)
        {
            if (hours12 < 1 || hours12 > 12) throw new ArgumentOutOfRangeException("hours12");
            if (minutes < 0 || minutes > 59) throw new ArgumentOutOfRangeException("minutes");
            if (seconds < 0 || seconds > 59) throw new ArgumentOutOfRangeException("seconds");
            if (milliseconds < 0 || milliseconds > 999) throw new ArgumentOutOfRangeException("milliseconds");
            if (meridiem < Meridiem.AM || meridiem > Meridiem.PM) throw new ArgumentOutOfRangeException("meridiem");
            Contract.EndContractBlock();

            int hours24 = Hours12To24(hours12, meridiem);
            _ticks = hours24 * TicksPerHour +
                     minutes * TicksPerMinute +
                     seconds * TicksPerSecond +
                     milliseconds * TicksPerMillisecond;
        }

        public int Hours24
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                Contract.Ensures(Contract.Result<int>() <= 23);

                return (int)((_ticks / TicksPerHour) % 24);
            }
        }

        public int Hours12
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 1);
                Contract.Ensures(Contract.Result<int>() <= 12);

                int hour = Hours24 % 12;
                return hour == 0 ? 12 : hour;
            }
        }

        public Meridiem Meridiem
        {
            get
            {
                return Hours24 < 12 ? Meridiem.AM : Meridiem.PM;
            }
        }

        public int Minutes
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                Contract.Ensures(Contract.Result<int>() <= 59);

                return (int)((_ticks / TicksPerMinute) % 60);
            }
        }

        public int Seconds
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                Contract.Ensures(Contract.Result<int>() <= 59);

                return (int)((_ticks / TicksPerSecond) % 60);
            }
        }

        public int Milliseconds
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                Contract.Ensures(Contract.Result<int>() <= 999);

                return (int)((_ticks / TicksPerMillisecond) % 1000);
            }
        }

        public long Ticks
        {
            get
            {
                Contract.Ensures(Contract.Result<long>() >= MinTicks);
                Contract.Ensures(Contract.Result<long>() <= MaxTicks);

                return _ticks;
            }
        }

        public DateTime On(Date date)
        {
            long ticks = date.DayNumber * TimeSpan.TicksPerDay + _ticks;
            return new DateTime(ticks);
        }

        /// <summary>
        /// Gets a <see cref="TimeOfDay"/> object that is set to the current time in the specified time zone.
        /// </summary>
        /// <param name="timeZoneInfo">The <see cref="TimeZoneInfo"/> instance.</param>
        /// <returns>The current <see cref="TimeOfDay"/> for the specified time zone.</returns>
        public static TimeOfDay NowInTimeZone(TimeZoneInfo timeZoneInfo)
        {
            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            DateTimeOffset localNow = TimeZoneInfo.ConvertTime(utcNow, timeZoneInfo);
            return TimeOfDayFromTimeSpan(localNow.TimeOfDay);
        }

        public static TimeOfDay Now
        {
            get
            {
                DateTime localNow = DateTime.Now;
                return TimeOfDayFromTimeSpan(localNow.TimeOfDay);
            }
        }

        public static TimeOfDay UtcNow
        {
            get
            {
                DateTime utcNow = DateTime.UtcNow;
                return TimeOfDayFromTimeSpan(utcNow.TimeOfDay);
            }
        }

        /// <summary>
        /// Determines if a time falls within the range provided.
        /// Supports both "normal" ranges such as 10:00-12:00, and ranges that span midnight such as 23:00-01:00.
        /// </summary>
        /// <param name="startTime">The starting time of day, inclusive.</param>
        /// <param name="endTime">The ending time of day, exclusive.</param>
        /// <returns>True, if the time falls within the range, false otherwise.</returns>
        public bool IsBetween(TimeOfDay startTime, TimeOfDay endTime)
        {
            long startTicks = startTime._ticks;
            long endTicks = endTime._ticks;

            return startTicks <= endTicks
                ? (startTicks <= _ticks && endTicks > _ticks)
                : (startTicks <= _ticks || endTicks > _ticks);
        }

        /// <summary>
        /// Calculates the duration between two time values.
        /// Assumes a standard day, with no invalid or ambiguous times due to Daylight Saving Time.
        /// Supports both "normal" ranges such as 10:00-12:00, and ranges that span midnight such as 23:00-01:00.
        /// </summary>
        /// <param name="startTime">The starting time of day, inclusive.</param>
        /// <param name="endTime">The ending time of day, exclusive.</param>
        /// <returns>A <see cref="TimeSpan"/> representing the duration between the two values.</returns>
        public static TimeSpan CalculateDuration(TimeOfDay startTime, TimeOfDay endTime)
        {
            long startTicks = startTime._ticks;
            long endTicks = endTime._ticks;

            return startTicks <= endTicks
                ? TimeSpan.FromTicks(endTicks - startTicks)
                : TimeSpan.FromTicks(endTicks - startTicks + TimeSpan.TicksPerDay);
        }

        public TimeOfDay Add(TimeSpan timeSpan)
        {
            return AddTicks(timeSpan.Ticks);
        }

        public TimeOfDay AddHours(double hours)
        {
            return AddTicks((long)(hours * TicksPerHour));
        }

        public TimeOfDay AddMinutes(double minutes)
        {
            return AddTicks((long)(minutes * TicksPerMinute));
        }

        public TimeOfDay AddSeconds(double seconds)
        {
            return AddTicks((long)(seconds * TicksPerSecond));
        }

        public TimeOfDay AddMilliseconds(double milliseconds)
        {
            return AddTicks((long)(milliseconds * TicksPerMillisecond));
        }

        public TimeOfDay AddTicks(long ticks)
        {
            long t = (_ticks + TicksPerDay + (ticks % TicksPerDay)) % TicksPerDay;
            return new TimeOfDay(t);
        }

        public TimeOfDay Subtract(TimeSpan timeSpan)
        {
            return AddTicks(-timeSpan.Ticks);
        }

        public static TimeOfDay operator +(TimeOfDay timeOfDay, TimeSpan timeSpan)
        {
            return timeOfDay.Add(timeSpan);
        }

        public static TimeOfDay operator -(TimeOfDay timeOfDay, TimeSpan timeSpan)
        {
            return timeOfDay.Subtract(timeSpan);
        }

        public bool Equals(TimeOfDay value)
        {
            return _ticks == value._ticks;
        }

        public static bool Equals(TimeOfDay t1, TimeOfDay t2)
        {
            return t1.Equals(t2);
        }

        public override bool Equals(object value)
        {
            if (ReferenceEquals(null, value)) return false;
            return value is TimeOfDay && Equals((TimeOfDay)value);
        }

        public override int GetHashCode()
        {
            return _ticks.GetHashCode();
        }

        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);
            return DateTime.MinValue.AddTicks(_ticks).ToString("T");
        }

        public string ToString(IFormatProvider provider)
        {
            Contract.Ensures(Contract.Result<string>() != null);
            return DateTime.MinValue.AddTicks(_ticks).ToString("T", provider);
        }

        public string ToString(string format)
        {
            Contract.Ensures(Contract.Result<string>() != null);
            format = NormalizeTimeFormat(format);
            return DateTime.MinValue.AddTicks(_ticks).ToString(format);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            Contract.Ensures(Contract.Result<string>() != null);
            format = NormalizeTimeFormat(format);
            return DateTime.MinValue.AddTicks(_ticks).ToString(format, provider);
        }

        /// <summary>
        /// Converts the value of the current <see cref="TimeOfDay"/> object to its equivalent
        /// long time string representation.
        /// </summary>
        /// <returns>A string that contains the long time string representation of the
        /// current <see cref="TimeOfDay"/> object.</returns>
        /// <remarks>The value of the current <see cref="TimeOfDay"/> object is formatted
        /// using the pattern defined by the <see cref="DateTimeFormatInfo.LongTimePattern" />
        /// property associated with the current thread culture.</remarks>
        public string ToLongTimeString()
        {
            return ToString(CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern);
        }

        /// <summary>
        /// Converts the value of the current <see cref="TimeOfDay"/> object to its equivalent
        /// short time string representation.
        /// </summary>
        /// <returns>A string that contains the short time string representation of the
        /// current <see cref="TimeOfDay"/> object.</returns>
        /// <remarks>The value of the current <see cref="TimeOfDay"/> object is formatted
        /// using the pattern defined by the <see cref="DateTimeFormatInfo.ShortTimePattern" />
        /// property associated with the current thread culture.</remarks>
        public string ToShortTimeString()
        {
            return ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
        }

        public static TimeOfDay Parse(string s)
        {
            DateTime dt = DateTime.Parse(s, null, DateTimeStyles.NoCurrentDateDefault);
            return TimeOfDayFromTimeSpan(dt.TimeOfDay);
        }

        public static TimeOfDay Parse(string s, IFormatProvider provider)
        {
            DateTime dt = DateTime.Parse(s, provider, DateTimeStyles.NoCurrentDateDefault);
            return TimeOfDayFromTimeSpan(dt.TimeOfDay);
        }

        public static TimeOfDay Parse(string s, IFormatProvider provider, DateTimeStyles styles)
        {
            if (((int)styles) >= 8)
                throw new ArgumentOutOfRangeException("styles");
            Contract.EndContractBlock();

            DateTime dt = DateTime.Parse(s, provider, DateTimeStyles.NoCurrentDateDefault | styles);
            return TimeOfDayFromTimeSpan(dt.TimeOfDay);
        }

        public static TimeOfDay ParseExact(string s, string format, IFormatProvider provider)
        {
            format = NormalizeTimeFormat(format);
            DateTime dt = DateTime.ParseExact(s, format, provider, DateTimeStyles.NoCurrentDateDefault);
            return TimeOfDayFromTimeSpan(dt.TimeOfDay);
        }

        public static TimeOfDay ParseExact(string s, string format, IFormatProvider provider, DateTimeStyles styles)
        {
            if (((int)styles) >= 8)
                throw new ArgumentOutOfRangeException("styles");
            Contract.EndContractBlock();

            format = NormalizeTimeFormat(format);
            DateTime dt = DateTime.ParseExact(s, format, provider, DateTimeStyles.NoCurrentDateDefault | styles);
            return TimeOfDayFromTimeSpan(dt.TimeOfDay);
        }

        public static TimeOfDay ParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles styles)
        {
            if (((int)styles) >= 8)
                throw new ArgumentOutOfRangeException("styles");
            Contract.EndContractBlock();

            for (int i = 0; i < formats.Length; i++)
            {
                formats[i] = NormalizeTimeFormat(formats[i]);
            }

            DateTime dt = DateTime.ParseExact(s, formats, provider, DateTimeStyles.NoCurrentDateDefault | styles);
            return TimeOfDayFromTimeSpan(dt.TimeOfDay);
        }

        public static bool TryParse(string s, out TimeOfDay time)
        {
            DateTime dt;
            if (!DateTime.TryParse(s, null, DateTimeStyles.NoCurrentDateDefault, out dt))
            {
                time = default(TimeOfDay);
                return false;
            }

            time = TimeOfDayFromTimeSpan(dt.TimeOfDay);
            return true;
        }

        public static bool TryParse(string s, IFormatProvider provider, DateTimeStyles styles, out TimeOfDay time)
        {
            if (((int)styles) >= 8)
                throw new ArgumentOutOfRangeException("styles");
            Contract.EndContractBlock();

            DateTime dt;
            if (!DateTime.TryParse(s, provider, DateTimeStyles.NoCurrentDateDefault | styles, out dt))
            {
                time = default(TimeOfDay);
                return false;
            }

            time = TimeOfDayFromTimeSpan(dt.TimeOfDay);
            return true;
        }

        public static bool TryParseExact(string s, string format, IFormatProvider provider, DateTimeStyles styles, out TimeOfDay time)
        {
            if (((int)styles) >= 8)
                throw new ArgumentOutOfRangeException("styles");
            Contract.EndContractBlock();

            format = NormalizeTimeFormat(format);

            DateTime dt;
            if (!DateTime.TryParseExact(s, format, provider, DateTimeStyles.NoCurrentDateDefault | styles, out dt))
            {
                time = default(TimeOfDay);
                return false;
            }

            time = TimeOfDayFromTimeSpan(dt.TimeOfDay);
            return true;
        }

        public static bool TryParseExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles styles, out TimeOfDay time)
        {
            if (((int)styles) >= 8)
                throw new ArgumentOutOfRangeException("styles");
            Contract.EndContractBlock();

            for (int i = 0; i < formats.Length; i++)
            {
                formats[i] = NormalizeTimeFormat(formats[i]);
            }

            DateTime dt;
            if (!DateTime.TryParseExact(s, formats, provider, DateTimeStyles.NoCurrentDateDefault | styles, out dt))
            {
                time = default(TimeOfDay);
                return false;
            }

            time = TimeOfDayFromTimeSpan(dt.TimeOfDay);
            return true;
        }

        public static bool operator ==(TimeOfDay left, TimeOfDay right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimeOfDay left, TimeOfDay right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Implicitly casts a <see cref="TimeSpan"/> object to a <see cref="TimeOfDay"/> by returning a new
        /// <see cref="TimeOfDay"/> object that has the equivalent hours, minutes, seconds, and fractional seconds
        /// components.  This is useful when using APIs that express a time-of-day as the elapsed time since
        /// midnight, such that their values can be assigned to a variable having a <see cref="TimeOfDay"/> type.
        /// </summary>
        /// <param name="timeSpan">A <see cref="TimeSpan"/> value representing the time elapsed since midnight,
        /// without regard to daylight saving time transitions.</param>
        /// <returns>A newly constructed <see cref="TimeOfDay"/> object with an equivalent value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="timeSpan"/> is either negative, or greater than <c>23:59:59.9999999</c>, and thus cannot be
        /// mapped to a <see cref="TimeOfDay"/>.
        /// </exception>
        /// <remarks>
        /// Fundamentally, a time-of-day and an elapsed-time are two different concepts.  In previous versions
        /// of the .NET framework, the <see cref="TimeOfDay"/> type did not exist, and thus several time-of-day
        /// values were represented by <see cref="TimeSpan"/> values erroneously.  For example, the
        /// <see cref="DateTime.TimeOfDay"/> property returns a value having a <see cref="TimeSpan"/> type.
        /// This implicit cast operator allows those APIs to be naturally used with <see cref="TimeOfDay"/>.
        /// <para>
        /// Also note that the input <paramref name="timeSpan"/> might actually *not* accurately represent the
        /// "time elapsed since midnight" on days containing a daylight saving time transition.
        /// </para>
        /// </remarks>
        public static implicit operator TimeOfDay(TimeSpan timeSpan)
        {
            long ticks = timeSpan.Ticks;
            if (ticks < 0 || ticks >= TimeSpan.TicksPerDay)
                throw new ArgumentOutOfRangeException("timeSpan");
            Contract.EndContractBlock();

            return new TimeOfDay(ticks);
        }

        /// <summary>
        /// Enables explicit casting of a <see cref="TimeOfDay"/> object to a <see cref="TimeSpan"/> by returning a new
        /// <see cref="TimeSpan"/> object that has the equivalent hours, minutes, seconds, and fractional seconds
        /// components.  This is useful when using APIs that express a time-of-day as the elapsed time since
        /// midnight, such that a <see cref="TimeOfDay"/> type can be passed to a method expecting a
        /// <see cref="TimeSpan"/> parameter as a time-of-day.
        /// </summary>
        /// <param name="timeOfDay">A <see cref="TimeOfDay"/> value.</param>
        /// <returns>
        /// A newly constructed <see cref="TimeSpan"/> object representing the time elapsed since midnight, without
        /// regard to daylight saving time transitions.
        /// </returns>
        public static explicit operator TimeSpan(TimeOfDay timeOfDay)
        {
            return new TimeSpan(timeOfDay.Ticks);
        }

        /// <summary>
        /// Converts the time from a 12-hour-clock representation to a 24-hour-clock representation.
        /// </summary>
        private static int Hours12To24(int hours12, Meridiem meridiem)
        {
            if (hours12 < 1 || hours12 > 12)
                throw new ArgumentOutOfRangeException("hours12");

            if (!Enum.IsDefined(typeof(Meridiem), meridiem))
                throw new ArgumentOutOfRangeException("meridiem");

            Contract.EndContractBlock();

            return meridiem == Meridiem.AM
                ? (hours12 == 12 ? 0 : hours12)
                : (hours12 == 12 ? 12 : hours12 + 12);
        }

        /// <summary>
        /// Constructs a <see cref="TimeOfDay"/> from a <see cref="TimeSpan"/> representing the time elapsed since
        /// midnight, without regard to daylight saving time transitions.
        /// </summary>
        private static TimeOfDay TimeOfDayFromTimeSpan(TimeSpan timeSpan)
        {
            return new TimeOfDay(timeSpan.Ticks);
        }

        /// <summary>
        /// Normalizes a format string that has standard or custom date/time formats,
        /// such that the formatted output can only contain a time-of-day when applied.
        /// </summary>
        /// <exception cref="FormatException">
        /// The format string contained a format specifier that is only applicable
        /// when a date would be part of the formatted output.
        /// </exception>
        private static string NormalizeTimeFormat(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return "T";
            }

            // standard formats
            if (format.Length == 1)
            {
                // pass-through formats
                if ("Tt".Contains(format))
                    return format;

                // ISO formats
                if (format == "s")
                    return "HH:mm:ss";

                if ("Oo".Contains(format))
                    return "HH:mm:ss.fffffff";


                // All other standard DateTime formats are invalid for TimeOfDay
                throw new FormatException();
            }

            // custom format - test for date components or embedded standard date formats
            // except when escaped by preceding \ or enclosed in "" or '' quotes

            var filtered = Regex.Replace(format, @"(\\.)|("".*"")|('.*')", String.Empty);
            if (Regex.IsMatch(filtered, "([dKMyz/]+)|(%[dDfFgGmMrRuUyY]+)"))
                throw new FormatException();

            // custom format with embedded standard format(s) - ISO replacement
            format = format.Replace("%s", "HH:mm:ss");
            format = Regex.Replace(format, @"(%[Oo])", "HH:mm:ss.fffffff");

            // pass through
            return format;
        }

        /// <summary>
        /// Gets a <see cref="XmlQualifiedName"/> that represents the <c>xs:time</c> type of the
        /// W3C XML Schema Definition (XSD) specification.
        /// </summary>
        /// <remarks>
        /// This is required to support the <see cref="XmlSchemaProviderAttribute"/> applied to this structure.
        /// </remarks>
        public static XmlQualifiedName GetSchema(object xs)
        {
            return new XmlQualifiedName("time", "http://www.w3.org/2001/XMLSchema");
        }

        /// <summary>
        /// Required by the <see cref="IXmlSerializable"/> interface.
        /// </summary>
        /// <returns><c>null</c></returns>
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates a <see cref="TimeOfDay"/> object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> stream from which the object is deserialized.</param>
        /// <remarks>
        /// An <c>xs:time</c> uses the ISO-8601 extended time format, with up to seven decimal places of fractional
        /// seconds.  The equivalent .NET Framework format string is <c>HH:mm:ss.FFFFFFF</c>.
        /// </remarks>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            var s = reader.NodeType == XmlNodeType.Element
                ? reader.ReadElementContentAsString()
                : reader.ReadContentAsString();

            TimeOfDay t;
            if (!TryParseExact(s, "HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture, DateTimeStyles.None, out t))
            {
                throw new FormatException();
            }

            this = t;
        }

        /// <summary>
        /// Converts a <see cref="TimeOfDay"/> object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> stream to which the object is serialized.</param>
        /// <remarks>
        /// An <c>xs:time</c> uses the ISO-8601 extended time format, with up to seven decimal places of fractional
        /// seconds.  The equivalent .NET Framework format string is <c>HH:mm:ss.FFFFFFF</c>.
        /// </remarks>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteString(ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture));
        }
    }
}
