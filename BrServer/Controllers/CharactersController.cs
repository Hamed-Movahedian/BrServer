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
    [RoutePrefix("api/Characters")]
    public class CharactersController : ApiController
    {
        private charsoog_BrDBEntities db = new charsoog_BrDBEntities();

        [HttpPost, Route("Sync")]
        public IHttpActionResult Sync([FromBody] string input)
        {
            var characters = JArray
                .Parse(input)
                .Select(character => new Character()
                {
                    Id = (int)character["ID"],
                    Name = (string)character["Name"],
                    HasByDefault = (bool)character["HasByDefault"]
                })
                .ToList();

            characters.ForEach(character =>
            {
                if (character.Id == -1)
                    db.Characters.Add(character);
                else
                {
                    db.Characters.Attach(character);
                    db.Entry(character).State = EntityState.Modified;
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

            return Ok((characters.Select(c=>c.Id).ToList()));

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