using Xunit;

// Serilog Static Logger (Log.Logger) paralel test koşumlarında "The logger is already frozen"
// hatası fırlattığı için testlerin birbirini ezmemesi adına xUnit paralelleştirmesi kapatılıyor.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
