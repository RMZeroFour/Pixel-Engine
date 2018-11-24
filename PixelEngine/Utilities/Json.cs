using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace PixelEngine.Utilities
{
	public static class Json
	{
		public static string Stringify<T>(T item)
		{
			if (item == null)
				return null;

			Type type = typeof(T);

			if (type == typeof(IJsonType))
			{
				return JsonHelper.Stringify((IJsonType)item);
			}
			else
			{
				if (item is IDictionary dict)
				{
					JsonObject obj = new JsonObject();
					foreach (DictionaryEntry entry in dict)
						obj[entry.Key.ToString()] = entry.Value;
					return JsonHelper.Stringify(obj);
				}
				else if (item is IEnumerable en)
				{
					JsonArray arr = new JsonArray();
					foreach (object o in en)
						arr.Add(o);
					return JsonHelper.Stringify(arr);
				}
				else if (item is JsonArray arr)
				{
					return JsonHelper.Stringify(arr);
				}
				else if (item is JsonObject j)
				{
					return JsonHelper.Stringify(j);
				}
				else
				{
					JsonObject obj = new JsonObject();

					IEnumerable<FieldInfo> fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
						.Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute)));

					foreach (FieldInfo field in fields)
						obj[field.Name] = field.GetValue(item);

					IEnumerable<PropertyInfo> props = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
						.Where(f => f.CanRead && f.CanWrite);

					foreach (PropertyInfo prop in props)
						obj[prop.Name] = prop.GetValue(item);

					return JsonHelper.Stringify(obj);
				}
			}
		}

		public static T Parse<T>(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
				return default;

			IJsonType item = JsonHelper.Parse(json);

			if (typeof(T) == typeof(IJsonType))
			{
				return (T)item;
			}
			else
			{
				if (item is JsonArray arr)
				{
					List<object> list = new List<object>();
					for (int i = 0; i < arr.Count; i++)
						list.Add(arr[i]);
				}
				else if (item is JsonObject obj)
				{
					Type type = typeof(T);
					T t = (T)FormatterServices.GetUninitializedObject(type);

					IEnumerable<FieldInfo> fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
						.Where(f => !f.IsDefined(typeof(CompilerGeneratedAttribute)));

					foreach (FieldInfo field in fields)
					{
						object data = obj[field.Name];
						if (data != null)
						{
							object val = Convert.ChangeType(data, field.FieldType);
							field.SetValue(t, val);
						}
					}

					IEnumerable<PropertyInfo> props = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
						.Where(f => f.CanRead && f.CanWrite);

					foreach (PropertyInfo prop in props)
					{
						object data = obj[prop.Name];
						if (data != null)
						{
							object val = Convert.ChangeType(data, prop.PropertyType);
							prop.SetValue(t, val);
						}
					}

					return t;
				}
			}

			return default;
		}

		private static class JsonHelper
		{
			public static string Stringify(IJsonType json)
			{
				void Convert(object o, StringBuilder text)
				{
					switch (o)
					{
						case null:
							text.Append("null");
							return;

						case byte b:
						case sbyte sb:
						case short s:
						case ushort us:
						case int i:
						case uint ui:
						case long l:
						case ulong ul:
							text.Append(o.ToString());
							return;

						case char c:
							Convert(c.ToString(), text);
							return;

						case bool b:
							text.Append(b ? "true" : "false");
							return;

						case float f:
							text.Append(f.ToString(CultureInfo.InvariantCulture));
							return;

						case double d:
							text.Append(d.ToString(CultureInfo.InvariantCulture));
							return;

						case decimal e:
							text.Append(e.ToString(CultureInfo.InvariantCulture));
							return;

						case string s:
							text.Append('"');
							foreach (char c in s)
							{
								if (char.IsControl(c))
									text.Append('\\');
								text.Append(c);
							}
							text.Append('"');
							return;

						case Dictionary<string, object> dict:
							{
								JsonObject jo = new JsonObject();
								foreach (KeyValuePair<string, object> item in dict)
									jo[item.Key] = item.Value;
								Convert(jo, text);
							}
							return;

						case IEnumerable list:
							{
								JsonArray ja = new JsonArray();
								foreach (object item in list)
									ja.Add(item);
								Convert(ja, text);
							}
							return;

						case JsonObject jo:
							{
								text.Append("{");
								int index = 0;
								foreach (DictionaryEntry item in jo.Data)
								{
									text.AppendFormat("\"{0}\" : ", item.Key);
									Convert(item.Value, text);
									if (index++ < jo.Data.Count - 1)
										text.Append(',');
								}
								text.Append("}");
							}
							return;

						case JsonArray ja:
							{
								text.Append('[');
								int index = 0;
								foreach (object item in ja.Data)
								{
									Convert(item, text);
									if (index++ < ja.Data.Count - 1)
										text.Append(", ");
								}
								text.Append(']');
							}
							return;
					}
				}

				StringBuilder res = new StringBuilder();
				Convert(json, res);
				return res.ToString();
			}

			public static IJsonType Parse(string json)
			{
				string[] GetSubObjects(string text)
				{
					StringBuilder sb = new StringBuilder();

					List<string> parts = new List<string>();

					int depth = 0;

					for (int i = 0; i < text.Length; i++)
					{
						switch (text[i])
						{
							case '[':
							case '{':
								depth++;
								break;

							case ']':
							case '}':
								depth--;
								break;

							case '"':
								sb.Append('"');
								while (text[++i] != '"')
									sb.Append(text[i]);
								sb.Append('"');
								continue;

							case ',':
							case ':':
								if (depth == 0)
								{
									parts.Add(sb.ToString());
									sb.Clear();
									continue;
								}
								break;
						}

						sb.Append(text[i]);
					}

					parts.Add(sb.ToString());

					return parts.ToArray();
				}

				string RemoveMarkers(string text, char first, char second)
				{
					text = text.Remove(text.IndexOf(first), 1);
					text = text.Remove(text.LastIndexOf(second), 1);
					return text;
				}

				object Convert(string text)
				{
					if (text.StartsWith("{") && text.EndsWith("}"))
					{
						JsonObject obj = new JsonObject();

						text = RemoveMarkers(text, '{', '}');

						string[] parts = GetSubObjects(text);
						for (int i = 0; i < parts.Length; i += 2)
						{
							string id = RemoveMarkers(parts[i], '"', '"');
							string val = parts[i + 1];

							obj[id] = Convert(val);
						}

						return obj;
					}
					else if (text.StartsWith("[") && text.EndsWith("]"))
					{
						JsonArray arr = new JsonArray();

						text = RemoveMarkers(text, '[', ']');

						string[] parts = GetSubObjects(text);
						foreach (string item in parts)
							arr.Add(Convert(item));

						return arr;
					}
					else
					{
						if (text == "\"true\"")
						{
							// bool
							return true;
						}
						else if (text == "\"false\"")
						{
							// bool
							return false;
						}
						else if (text.StartsWith("\""))
						{
							// string
							text = RemoveMarkers(text, '"', '"');
							return text;
						}
						else if (text.IndexOf('.') != -1)
						{
							// decimal
							if (double.TryParse(text, out double d))
								return d;
						}
						else
						{
							// integral
							if (long.TryParse(text, out long l))
								return l;

							if (ulong.TryParse(text, out ulong ul))
								return ul;
						}
					}

					// undefined
					return null;
				}

				string RemoveWhitespaces(string text)
				{
					StringBuilder sb = new StringBuilder(text);

					for (int i = sb.Length - 1; i >= 0; i--)
					{
						if (sb[i] == '"')
							while (sb[--i] != '"') ;

						if (char.IsWhiteSpace(sb[i]))
							sb.Remove(i, 1);
					}

					return sb.ToString();
				}

				json = RemoveWhitespaces(json);
				return (IJsonType)Convert(json);
			}
		}
	}

	public interface IJsonType { }

	public class JsonObject : IJsonType
	{
		internal Hashtable Data = new Hashtable();

		public int Count => Data.Count;

		public object this[string key]
		{
			get => Data[key];
			set => Data[key] = value;
		}

		public void Add(string key, object o) => Data.Add(key, o);
		public void Remove(string key) => Data.Remove(key);
	}

	public class JsonArray : IJsonType
	{
		internal ArrayList Data = new ArrayList();

		public int Count => Data.Count;

		public JsonArray() { }
		public JsonArray(IList entries)
		{
			foreach (object o in entries)
				Add(o);
		}

		public object this[int index]
		{
			get => Data[index];
			set => Data[index] = value;
		}

		public void Add(object o) => Data.Add(o);
		public void Remove(object o) => Data.Remove(o);
	}
}
