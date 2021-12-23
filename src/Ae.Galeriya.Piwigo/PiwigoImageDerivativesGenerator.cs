using Ae.Galeriya.Piwigo.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;

namespace Ae.Galeriya.Piwigo
{
    internal sealed class PiwigoImageDerivativesGenerator : IPiwigoImageDerivativesGenerator
    {
        private readonly IPiwigoBaseAddressLocator _baseAddressLocator;
        private readonly IHttpContextAccessor _httpAccessor;

        public PiwigoImageDerivativesGenerator(IPiwigoBaseAddressLocator baseAddressLocator, IHttpContextAccessor httpAccessor)
        {
            _baseAddressLocator = baseAddressLocator;
            _httpAccessor = httpAccessor;
        }

        public IReadOnlyDictionary<string, PiwigoThumbnail> GenerateDerivatives(uint imageId)
        {
            var derivatives = new Dictionary<string, PiwigoThumbnail>
            {
                {
                    "square",
                    new PiwigoThumbnail
                    {
                        Width = 120,
                        Height = 120,
                        Url = CreateThumbnailUri(imageId, 120, 120, "square")
                    }
                },
                {
                    "thumb",
                    new PiwigoThumbnail
                    {
                        Width = 144,
                        Height = 144,
                        Url = CreateThumbnailUri(imageId, 144, 144, "square")
                    }
                },
                {
                    "2small",
                    new PiwigoThumbnail
                    {
                        Width = 240,
                        Height = 240,
                        Url = CreateThumbnailUri(imageId, 240, 240, "square")
                    }
                },
                {
                    "xsmall",
                    new PiwigoThumbnail
                    {
                        Width = 432,
                        Height = 324,
                        Url = CreateThumbnailUri(imageId, 432, 324, "square")
                    }
                },
                {
                    "small",
                    new PiwigoThumbnail
                    {
                        Width = 576,
                        Height = 432,
                        Url = CreateThumbnailUri(imageId, 576, 432, "square")
                    }
                },
                {
                    "medium",
                    new PiwigoThumbnail
                    {
                        Width = 792,
                        Height = 594,
                        Url = CreateThumbnailUri(imageId, 792, 594, "square")
                    }
                },
                {
                    "large",
                    new PiwigoThumbnail
                    {
                        Width = 1008,
                        Height = 756,
                        Url = CreateThumbnailUri(imageId, 1008, 756, "square")
                    }
                },
                {
                    "xlarge",
                    new PiwigoThumbnail
                    {
                        Width = 1224,
                        Height = 918,
                        Url = CreateThumbnailUri(imageId, 1224, 918, "square")
                    }
                },
                {
                    "xxlarge",
                    new PiwigoThumbnail
                    {
                        Width = 1656,
                        Height = 1242,
                        Url = CreateThumbnailUri(imageId, 1656, 1242, "square")
                    }
                }
            };
            return derivatives;
        }

        public Uri CreateThumbnailUri(uint imageId, int width, int height, string type)
        {
            return new Uri(_baseAddressLocator.GetBaseAddress(), $"/thumbs/{imageId}-{width}-{height}-{type}.jpg");
        }
    }
}
