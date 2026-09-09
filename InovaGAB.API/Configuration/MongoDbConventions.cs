using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;

namespace InovaGAB.API.Configuration;

public static class MongoDbConventions
{
    public static void Register()
    {
        var conventionPack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new EnumRepresentationConvention(BsonType.String),
            new IgnoreExtraElementsConvention(true)
        };

        ConventionRegistry.Register(
            "InovaGABConventions",
            conventionPack,
            _ => true);
    }
}