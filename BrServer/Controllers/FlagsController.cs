using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using BrServer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BrServer.Controllers
{
    [RoutePrefix("api/Flags")]
    public class FlagsController : ApiController
    {
        private charsoog_BrDBEntities db = new charsoog_BrDBEntities();

        [HttpPost, Route("Sync")]
        public IHttpActionResult Sync([FromBody] string input)
        {
            var flags = JArray
                .Parse(input)
                .Select(flag => new Flag()
                {
                    Id = (int)flag["ID"],
                    Name = (string)flag["Name"],
                    HasByDefault = (bool)flag["HasByDefault"]
                })
                .ToList();

            flags.ForEach(flag =>
            {
                if (flag.Id == -1)
                    db.Flags.Add(flag);
                else
                {
                    db.Flags.Attach(flag);
                    db.Entry(flag).State = EntityState.Modified;
                }
            });

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok((flags.Select(c=>c.Id).ToList()));

        }
                
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}