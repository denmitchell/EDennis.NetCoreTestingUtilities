using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetCoreTestingUtilities {
    
    /// <summary>
    /// These extension methods use the FluentAssertions library
    /// to check the status code result and get the returned object 
    /// from MVC controller actions that return an IActionResult
    /// </summary>
    public static class IActionResultExtensions {
        public static T GetObject<T>(this IActionResult result) {
            ObjectResult objResult = null;
            try {
                objResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
                return objResult.Value.Should().BeAssignableTo<T>().Subject;
            } catch {
                return default(T);
            }
        }

        public static bool IsOk(this IActionResult result)
            => result.IsResultOfType<OkResult>();

        public static bool IsCreated(this IActionResult result)
            => result.IsResultOfType<CreatedResult>();

        public static bool IsCreatedAtAction(this IActionResult result)
            => result.IsResultOfType<CreatedAtActionResult>();

        public static bool IsCreatedAtRoute(this IActionResult result)
            => result.IsResultOfType<CreatedAtRouteResult>();

        public static bool IsAccepted(this IActionResult result)
            => result.IsResultOfType<AcceptedResult>();

        public static bool IsAcceptedAtAction(this IActionResult result)
            => result.IsResultOfType<AcceptedAtActionResult>();

        public static bool IsAcceptedAtRoute(this IActionResult result)
            => result.IsResultOfType<AcceptedAtRouteResult>();

        public static bool IsNoContent(this IActionResult result)
            => result.IsResultOfType<NoContentResult>();

        public static bool IsEmpty(this IActionResult result)
            => result.IsResultOfType<EmptyResult>();

        public static bool IsNotFound(this IActionResult result)
            => result.IsResultOfType<NotFoundResult>();

        public static bool IsUnauthorized(this IActionResult result)
            => result.IsResultOfType<UnauthorizedResult>();

        public static bool IsForbidden(this IActionResult result)
            => result.IsResultOfType<ForbidResult>();

        public static bool IsBadRequest(this IActionResult result)
            => result.IsResultOfType<BadRequestResult>();

        public static bool IsFile(this IActionResult result)
            => result.IsResultOfType<FileResult>();

        public static bool IsChallenge(this IActionResult result)
            => result.IsResultOfType<ChallengeResult>();

        public static bool IsRedirect(this IActionResult result)
            => result.IsResultOfType<RedirectResult>();

        public static bool IsRedirectToAction(this IActionResult result)
            => result.IsResultOfType<RedirectToActionResult>();

        public static bool IsRedirectToRoute(this IActionResult result)
            => result.IsResultOfType<RedirectToRouteResult>();

        private static bool IsResultOfType<T>(this IActionResult result)
            where T : ActionResult {
            try {
                var nfResult = result.Should().BeOfType<T>().Subject;
                return true;
            } catch {
                return false;
            }
        }

    }
}
