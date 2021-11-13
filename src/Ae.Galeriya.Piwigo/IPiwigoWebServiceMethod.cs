using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo
{
    public interface IPiwigoWebServiceMethod
    {
        string MethodName { get; }
        Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, CancellationToken token);
    }
}
