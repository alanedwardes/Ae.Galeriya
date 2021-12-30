using Ae.Galeriya.Core.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ae.Galeriya.Piwigo.Methods
{
    internal sealed class PiwigoLogoutMethod : IPiwigoWebServiceMethod
    {
        private readonly SignInManager<User> _signInManager;

        public string MethodName => "pwg.session.logout";
        public bool AllowAnonymous => false;

        public PiwigoLogoutMethod(SignInManager<User> signInManager) => _signInManager = signInManager;

        public async Task<object> Execute(IReadOnlyDictionary<string, IConvertible> parameters, IReadOnlyDictionary<string, FileMultipartSection> fileParameters, uint? userId, CancellationToken token)
        {
            await _signInManager.SignOutAsync();
            return true;
        }
    }
}
