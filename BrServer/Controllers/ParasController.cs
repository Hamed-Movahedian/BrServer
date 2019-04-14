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
    [RoutePrefix("api/Paras")]
    public class ParasController : ApiController
    {
        private charsoog_BrDBEntities db = new charsoog_BrDBEntities();

        [HttpPost, Route("Sync")]
        public IHttpActionResult Sync([FromBody] string input)
        {
            var paras = JArray
                .Parse(input)
                .Select(para => new Parachute()
                {
                    Id = (int)para["ID"],
                    Name = (string)para["Name"],
                    HasByDefault = (bool)para["HasByDefault"]
                })
                .ToList();

            paras.ForEach(para =>
            {
                if (para.Id == -1)
                    db.Parachutes.Add(para);
                else
                {
                    db.Parachutes.Attach(para);
                    db.Entry(para).State = EntityState.Modified;
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

            return Ok((paras.Select(c=>c.Id).ToList()));

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