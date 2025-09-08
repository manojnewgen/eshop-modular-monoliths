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
    internal class ShoppingCartItemsConverter : JsonConverter<ShoppingCartItem>
    {
        // Replace the constructor call with property assignments since ShoppingCartItem does not have a 7-argument constructor.
        // Use the default constructor and set properties accordingly.

        public override ShoppingCartItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonDocument = JsonDocument.ParseValue(ref reader);
            var jsonObject = jsonDocument.RootElement;
            var id = jsonObject.GetProperty("Id").GetGuid();
            var shoppingCartId = jsonObject.GetProperty("ShoppingCartId").GetGuid();
            var productId = jsonObject.GetProperty("ProductId").GetGuid();
            var quantity = jsonObject.GetProperty("Quantity").GetInt32();
            var color = jsonObject.GetProperty("Color").GetString() ?? string.Empty;
            var price = jsonObject.GetProperty("Price").GetDecimal();
            var productName = jsonObject.GetProperty("ProductName").GetString() ?? string.Empty;

            // Use the constructor that matches the signature: ShoppingCartItem(Guid shoppingCartId, Guid productId, int quantity, decimal price, string productName, string color)
            var item = new ShoppingCartItem(shoppingCartId, productId, quantity, price, productName, color);

            // Set the Id property using reflection since it's protected
            item.GetType().GetProperty("Id")?.SetValue(item, id);

            return item;
        }

        public override void Write(Utf8JsonWriter writer, ShoppingCartItem value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Id", value.Id);
            writer.WriteString("ShoppingCartId", value.ShoppingCartId);
            writer.WriteString("ProductId", value.ProductId);
            writer.WriteNumber("Quantity", value.Quantity);
            writer.WriteNumber("Price", value.Price);
            writer.WriteString("ProductName", value.ProductName);
            writer.WriteString("Color", value.Color);
            writer.WriteEndObject();
        }
    }
}
