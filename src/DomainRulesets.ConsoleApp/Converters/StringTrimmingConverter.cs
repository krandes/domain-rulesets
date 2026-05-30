using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DomainRulesets.ConsoleApp.Converters;

public class StringTrimmingConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(string);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();

        return scalar.Value.Trim();
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var stringValue = (string?)value;

        emitter.Emit(new Scalar(stringValue ?? string.Empty));
    }
}