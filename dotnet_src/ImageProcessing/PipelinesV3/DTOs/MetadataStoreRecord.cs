namespace ImageProcessing.PipelinesV3.DTOs;

public record MetadataStoreRecord(Guid RecordGuid);

// TODO think about the types here, and how I can enforce that the necessary inputs exist w/ type safety.
// TODO Possibly the records can be chained? This could add some metadata & tracing.
