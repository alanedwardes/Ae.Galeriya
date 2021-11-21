using System;

namespace Ae.Galeriya.Core.Exceptions
{
    public sealed class BlobNotFoundException : Exception
    {
        public BlobNotFoundException(Guid blobId) : base($"The blob {blobId} was not found")
        {
        }
    }
}
