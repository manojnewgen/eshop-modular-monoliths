using Basket.Basket.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Basket.Data.JsonConverters
{
    public class ShoppingCartConverter : JsonConverter<ShoppingCart>
    {
        public override ShoppingCart? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonDocument = JsonDocument.ParseValue(ref reader);
            var jsonObject = jsonDocument.RootElement;

            var id = jsonObject.GetProperty("Id").GetGuid();
            var userName = jsonObject.GetProperty("UserName").GetString() ?? string.Empty;

            var itemElement = jsonObject.GetProperty("Items");

            var shoppingCart = ShoppingCart.Create(id, userName);
            var items = itemElement.Deserialize<List<ShoppingCartItem>>(options) ?? new List<ShoppingCartItem>();
            if (items.Any())
            {

                var itemsField = typeof(ShoppingCart).GetField("_items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (itemsField != null)
                {
                    itemsField.SetValue(shoppingCart, items);
                }
            }


            return shoppingCart;
        }

        public override void Write(Utf8JsonWriter writer, ShoppingCart value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Id", value.Id);
            writer.WriteString("UserName", value.UserName);
            writer.WritePropertyName("Items");
            JsonSerializer.Serialize(writer, value.Items, options);
            writer.WriteEndObject();
            //writer.WriteEndArray();

        }
    }
}
