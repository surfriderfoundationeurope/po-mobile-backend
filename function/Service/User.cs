using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Surfrider.PlasticOrigins.Backend.Mobile.Service
{
    public class User : BaseEntity
    {
        public string Id => GetValue(nameof(Id), string.Empty);
        public string LastName => GetValue(nameof(LastName), string.Empty);
        public string FirstName => GetValue(nameof(FirstName), string.Empty);
        public string BirthYear => GetValue(nameof(BirthYear), string.Empty);
        public string PasswordHash => GetValue(nameof(PasswordHash), string.Empty);
        public string Email => GetValue(nameof(Email), string.Empty);

        [IgnoreDataMember]
        public string AuthToken { get; set; }
        [IgnoreDataMember]
        public DateTime AuthTokenExpiration { get; set; }

        public User()
        { }

        public User(string id, string lastName, string firstName, string birthYear, string passwordHash, string email)
        {
            SetValue(nameof(Id), id);
            SetValue(nameof(LastName), lastName);
            SetValue(nameof(FirstName), firstName);
            SetValue(nameof(BirthYear), birthYear);
            SetValue(nameof(PasswordHash), passwordHash);
            SetValue(nameof(Email), email);
        }

        protected override string GetEntityKey()
        {
            return Id;
        }
    }

    [DataContract]
    [DebuggerDisplay("BaseEntity (Key={EntityKey})")]
    public abstract class BaseEntity
    {
        private Dictionary<string, object> _values;

        public string EntityKey
        {
            get
            {
                return GetEntityKey();
            }
        }

        public IDictionary<string, object> AllValues
        {
            get
            {
                if (_values == null)
                {
                    _values = new Dictionary<string, object>();
                }
                return _values;
            }
        }

        protected abstract string GetEntityKey();

        public override int GetHashCode()
        {
            return EntityKey.GetHashCode();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(_values);
        }

        public static T FromJson<T>(string content) where T : BaseEntity, new()
        {
            Dictionary<string, object> values = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            T entity = new T { _values = values };
            return entity;
        }

        public virtual void Merge(IDictionary<string, object> values)
        {
            if (values == null)
                return;

            foreach (KeyValuePair<string, object> kvp in values)
            {
                if (!AllValues.ContainsKey(kvp.Key))
                {
                    AllValues[kvp.Key] = kvp.Value;
                }
            }
        }

        protected T GetValue<T>(string name, T defaultValue)
        {
            object obj;
            if (!AllValues.TryGetValue(name, out obj))
                return defaultValue;

            return ChangeType<T>(obj, defaultValue);
        }

        protected bool SetValue(string key, object value, [CallerMemberName] string propertyName = null, bool forceNull = false)
        {
            object oldValue = null;

            if (value == null && forceNull == false)
                return false;


            if (AllValues.TryGetValue(key, out oldValue) == false || oldValue != value)
            {
                AllValues[key] = value;
                return true;
            }

            return false;
        }

        protected static T ChangeType<T>(object value, T defaultValue)
        {
            return (T)ChangeType(value, typeof(T), defaultValue);
        }

        protected static object ChangeType(object value, Type conversionType, object defaultValue)
        {
            if (conversionType == null)
                throw new ArgumentNullException(nameof(conversionType));

            if (value == null)
                return defaultValue;

            object output = defaultValue;
            try
            {
                output = Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException)
            {
                output = InternalChangeType(value, conversionType, defaultValue);
            }
            catch (FormatException)
            {
                output = InternalChangeType(value, conversionType, defaultValue);
            }
            return output;
        }

        protected static object InternalChangeType(object value, Type conversionType, object defaultValue)
        {
            // Support some extra conversions such as "0" => false or "1" => true
            switch (conversionType.FullName)
            {
                case "System.Boolean":
                    return ToBoolean(value, (bool)defaultValue);
                case "System.DateTime":
                    return ToDateTime(value, (DateTime)defaultValue);
                default:
                    break;
            }

            return defaultValue;
        }

        protected static bool ToBoolean(object value, bool defaultValue)
        {
            if (value == null)
                return defaultValue;

            if (value is bool)
                return (bool)value;

            if (value is bool?)
            {
                bool? nullable = (bool?)value;
                return nullable.GetValueOrDefault(defaultValue);
            }

            return ToBoolean(value.ToString(), defaultValue);
        }

        protected static DateTime ToDateTime(object value, DateTime defaultValue)
        {
            if (value == null)
                return defaultValue;

            if (value is DateTime)
                return (DateTime)value;

            if (value is DateTime?)
            {
                DateTime? nullable = (DateTime?)value;
                return nullable.GetValueOrDefault(defaultValue);
            }

            return ToDateTime(value.ToString(), defaultValue);
        }
    }
}