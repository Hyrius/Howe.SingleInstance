using System;
using System.Text.Json;

namespace SingleInstanceCore
{
	internal static class SerializationExtensions
	{
		private static readonly JsonSerializerOptions serializerOptions = new()
		{
			PropertyNamingPolicy = null,
			AllowTrailingCommas = true
		};

		internal static BinaryData Serialize<T>(this T obj)
		{
			return BinaryData.FromObjectAsJson(obj);
		}

		internal static T Deserialize<T>(this BinaryData data)
		{
			return JsonSerializer.Deserialize<T>(data, serializerOptions);
		}
    }
}