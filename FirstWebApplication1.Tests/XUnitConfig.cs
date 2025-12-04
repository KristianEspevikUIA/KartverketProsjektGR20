using Xunit;

// Forhindrer at testene kan kjøre parallelt. Kjører gjennom testene sekvensiellt
[assembly: CollectionBehavior(DisableTestParallelization = true)]