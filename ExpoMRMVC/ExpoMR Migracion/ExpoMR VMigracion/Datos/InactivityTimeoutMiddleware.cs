using Microsoft.AspNetCore.Identity;
using ExpoMR.Controllers;
using ExpoMR.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpoMR.Datos
{
    public class InactivityTimeoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int _timeoutMinutes;

        public InactivityTimeoutMiddleware(RequestDelegate next, int timeoutMinutes)
        {
            _next = next;
            _timeoutMinutes = timeoutMinutes;
        }

        //public async Task Invoke(HttpContext context, SignInManager<IdentityUser> signInManager)
        //{
        //    if (context.User.Identity.IsAuthenticated)
        //    {
        //        var lastActivity = context.Session.GetString("LastActivityTime");
        //        if (lastActivity != null && DateTime.TryParse(lastActivity, out DateTime lastActivityTime))
        //        {
        //            var inactivityDuration = DateTime.UtcNow - lastActivityTime;
        //            if (inactivityDuration.TotalMinutes >= _timeoutMinutes)
        //            {
        //                // Cerrar sesión
        //                await signInManager.SignOutAsync();
        //                context.Session.Remove("LastActivityTime");
        //                context.Response.Redirect("/Cuentas/Acceso");
        //                return;
        //            }
        //        }

        //        context.Session.SetString("LastActivityTime", DateTime.UtcNow.ToString());
        //    }

        //    await _next(context);
        //}
    }
}
