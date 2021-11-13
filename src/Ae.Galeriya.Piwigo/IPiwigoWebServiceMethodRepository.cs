using System.Collections.Generic;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoWebServiceMethodRepository
    {
        IEnumerable<string> GetMethods();
        IPiwigoWebServiceMethod GetMethod(string methodName);
    }
}
