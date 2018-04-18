using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Meziantou.PasswordManager.Web.Areas.Api.Data
{
    [JsonConverter(typeof(IntIdJsonConverter))]
    public struct IntId : IEquatable<IntId>
    {
        public int Value { get; }

        public bool HasValue => Value > 0;

        public IntId(int value)
        {
            if (value < 0)
            {
                Value = 0;
            }
            else
            {
                Value = value;
            }
        }

        public static implicit operator IntId(int value)
        {
            return new IntId(value);
        }

        public static implicit operator IntId(int? value)
        {
            return new IntId(value ?? 0);
        }

        public static implicit operator IntId(string value)
        {
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
                return new IntId(id);

            return default;
        }

        public static implicit operator int(IntId value)
        {
            return value.Value;
        }

        public static implicit operator string(IntId value)
        {
            if (value.HasValue)
                return value.Value.ToString(CultureInfo.InvariantCulture);

            return null;
        }

        public static bool operator ==(IntId id1, IntId id2)
        {
            return id1.Equals(id2);
        }

        public static bool operator !=(IntId id1, IntId id2)
        {
            return !(id1 == id2);
        }

        public override bool Equals(object obj)
        {
            return obj is IntId && Equals((IntId)obj);
        }

        public bool Equals(IntId other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            if (HasValue)
                return Value.ToString(CultureInfo.InvariantCulture);

            return "-1";
        }
    }
}
