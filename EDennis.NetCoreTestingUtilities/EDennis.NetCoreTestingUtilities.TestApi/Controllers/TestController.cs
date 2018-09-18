using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDennis.NetCoreTestingUtilities.TestApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace EDennis.NetCoreTestingUtilities.TestApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase {

        [HttpGet("ok-object-result/int")]
        public IActionResult GetOkInt() {
            return new OkObjectResult(1);
        }

        [HttpGet("ok-object-result/string")]
        public IActionResult GetOkString() {
            return new OkObjectResult("red");
        }

        [HttpGet("ok-object-result/person")]
        public IActionResult GetOkPerson() {
            return new OkObjectResult(new Person() { FirstName = "Bob", LastName = "Barker" });
        }

        [HttpGet("bad-request-object-result/int")]
        public IActionResult GetBadInt() {
            return new BadRequestObjectResult(1);
        }

        [HttpGet("bad-request-object-result/string")]
        public IActionResult GetBadString() {
            return new BadRequestObjectResult("red");
        }

        [HttpGet("bad-request-object-result/person")]
        public IActionResult GetBadPerson() {
            return new BadRequestObjectResult(new Person() { FirstName = "Bob", LastName = "Barker" });
        }

        [HttpGet("json-result/person")]
        public IActionResult GetJsonPerson() {
            return new JsonResult(new Person() { FirstName = "Bob", LastName = "Barker" });
        }

        [HttpGet("ok-result")]
        public IActionResult GetOkOnly() {
            return new OkResult();
        }

        [HttpGet("not-found")]
        public IActionResult GetNotFound() {
            return new NotFoundResult();
        }

        [HttpGet("not-found-null")]
        public IActionResult GetNotFoundNull() {
            return NotFound(null);
        }


        [HttpGet("forbid")]
        public IActionResult GetForbid() {
            return new StatusCodeResult(403);
        }

    }
}
