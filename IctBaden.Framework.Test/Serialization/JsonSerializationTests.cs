using System.Text.Json;
using System.Text.Json.Serialization;
using IctBaden.Framework.PropertyProvider;
using IctBaden.Framework.Resource;
using Xunit;

namespace IctBaden.Framework.Test.Serialization
{
    public class Test
    {
        public string Name { get; set; }
        [JsonInclude]
        public PropertyBag Properties { get; private set; } = new();
    }

    public class JsonSerializationTests
    {
        private static JsonSerializerOptions Settings
        {
            get
            {
                var settings = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                return settings;
            }
        }

        [Fact]
        public void PropertyBagShouldBeSerialized()
        {
            var test = new Test
            {
                Name = "test"
            };
            test.Properties.Set("abc", "123");
            test.Properties.Set("bcd", "234");
            test.Properties.Set("cde", "345");

            var json = JsonSerializer.Serialize(test, Settings);

            Assert.NotNull(json);
            Assert.Contains("test", json);
            Assert.Contains("abc", json);
            Assert.Contains("123", json);
            Assert.Contains("bcd", json);
            Assert.Contains("234", json);
            Assert.Contains("cde", json);
            Assert.Contains("345", json);
        }

        [Fact]
        public void PropertyBagShouldBeDeserialized()
        {
            var json = ResourceLoader.LoadString("IctBaden.Framework.Test.Serialization.test.json");

            var test = JsonSerializer.Deserialize<Test>(json!, Settings);

            Assert.NotNull(test);
            Assert.Equal("test", test.Name);
            Assert.True(test.Properties.Contains("abc"));
            Assert.True(test.Properties.Contains("bcd"));
            Assert.True(test.Properties.Contains("cde"));
            Assert.Equal("123", test.Properties.Get<string>("abc"));
            Assert.Equal("234", test.Properties.Get<string>("bcd"));
            Assert.Equal("345", test.Properties.Get<string>("cde"));
        }
        
        [Fact]
        public void PropertyBagShouldBeDeserializedAsSerialized()
        {
            var from = new Test
            {
                Name = "fromTo"
            };
            from.Properties.Set("abc", "def");
            from.Properties.Set("fgh", "678");
            from.Properties.Set("ghi", "789");
            from.Properties.Set("hik", "890");

            var json = JsonSerializer.Serialize(from, Settings);
            Assert.False(string.IsNullOrEmpty(json));

            var to = JsonSerializer.Deserialize<Test>(json, Settings);
            Assert.NotNull(to);
            
            Assert.Equal(from.Name, to.Name);
            Assert.Equal(from.Properties.Count, to.Properties.Count);
            foreach (var (key, value) in from.Properties)
            {
                var toValue = to.Properties[key];
                Assert.Equal(value?.ToString(), toValue.ToString());
            }
        }
        
    }
}