using Mapster;

namespace Shared.Mapping
{
    /// <summary>
    /// Base interface for mapping configurations
    /// </summary>
    public interface IMappingConfiguration
    {
        void Configure(TypeAdapterConfig config);
    }
}