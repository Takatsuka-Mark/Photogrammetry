using System.Collections.Concurrent;

namespace PhotogrammetryStore.Abstractions;

public record ParentMetadata(DateTimeOffset CreatedAt, ConcurrentDictionary<MetadataVariant, Guid> Children);
