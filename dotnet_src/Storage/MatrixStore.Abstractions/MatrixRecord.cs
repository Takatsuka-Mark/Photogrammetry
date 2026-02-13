using LinearAlgebra;

namespace MatrixStore.Abstractions;

public record MatrixRecord<T>(Matrix<T> Matrix, DateTimeOffset CreatedAt, int? SequenceId);
