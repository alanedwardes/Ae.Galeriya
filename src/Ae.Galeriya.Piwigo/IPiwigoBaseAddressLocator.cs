using System;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoBaseAddressLocator
    {
        Uri GetBaseAddress();
    }
}