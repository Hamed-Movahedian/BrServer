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
    [RoutePrefix("api/Players")]
    public class PlayerController : ApiController
    {
        private charsoog_BrDBEntities db = new charsoog_BrDBEntities();


        [HttpPost, Route("Save")]
        public IHttpActionResult Save([FromBody] string input)
        {
            var jInput = JToken.Parse(input);

            var jStat = jInput["PlayerStat"];

            var id = jInput["ID"].Value<int>();

            var player = db.Players.Find(id);

            if (player == null)
                return BadRequest("Player id not found!");

            player.Name = jInput["Name"].Value<string>();
            player.AiBehaviorIndex = jInput["AiBehaviorIndex"].Value<short>();
            player.CoinCount = jInput["CoinCount"].Value<int>();
            player.TicketCount = jInput["TicketCount"].Value<int>();
            player.HasBattlePass = jInput["HasBattlePass"].Value<bool>();

            player.CurrentCharacter = jInput["CurrentCharacter"].Value<int>();
            player.CurrentFlag = jInput["CurrentFlag"].Value<int>();
            player.CurrentEmote = jInput["CurrentEmote"].Value<int>();
            player.CurrentPara = jInput["CurrentPara"].Value<int>();

            player.TotalBattles = jStat["TotalBattles"].Value<int>();
            player.TotalWins = jStat["TotalWins"].Value<int>();
            player.TotalKills = jStat["TotalKills"].Value<int>();
            player.DoubleKills = jStat["DoubleKills"].Value<int>();
            player.TripleKills = jStat["TripleKills"].Value<int>();
            player.ItemsCollected = jStat["ItemsCollected"].Value<int>();
            player.GunsCollected = jStat["GunsCollected"].Value<int>();
            player.SupplyDrop = jStat["SupplyDrop"].Value<int>();
            player.SupplyCreates = jStat["SupplyCreates"].Value<int>();
            player.Experience = jStat["Experience"].Value<int>();

            // add characters
            var newCharecters =jInput["AvalableCharacters"].Values<int>();
            var oldCharacters = player.Characters.Select(c => c.Id);

            foreach (var i in newCharecters.Except(oldCharacters))
            {
                var character = db.Characters.Find(i);

                if (character == null)
                    return BadRequest($"Character id={i} not found.");

                player.Characters.Add(character);
            };

            // add Flags
            var newCharecters = jInput["AvalableFlags"].Values<int>();
            var oldCharacters = player.Characters.Select(c => c.Id);

            foreach (var i in newCharecters.Except(oldCharacters))
                foreach (var i in jInput["AvalableFlags"].Values<int>())
            {
                var flag = db.Flags.Find(i);

                if (flag == null)
                    return BadRequest($"Flag id={i} not found.");

                player.Flags.Add(flag);
            };

            // add Emotes
            foreach (var i in jInput["AvalableEmotes"].Values<int>())
            {
                var emote = db.Emotes.Find(i);

                if (emote == null)
                    return BadRequest($"Emote id={i} not found.");

                player.Emotes.Add(emote);
            };

            // add paras
            foreach (var i in jInput["AvalableParas"].Values<int>())
            {
                var para = db.Parachutes.Find(i);

                if (para == null)
                    return BadRequest($"Para id={i} not found.");

                player.Parachutes.Add(para);
            };


            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok("OK");
        }

        [HttpPost, Route("GetInitialInfo")]
        public IHttpActionResult GetInitialInfo([FromBody] string input)
        {
            int id;

            if (!int.TryParse(input, out id))
                return BadRequest("Invalid player id");

            Player player=null;
            if(id!=-1)
                player= db.Players.Find(id);
            else
            {
                // get available characters, flags, paras, emotes
                var characters = db.Characters.Where(c => c.HasByDefault).ToList();
                var flags = db.Flags.Where(c => c.HasByDefault).ToList();
                var emotes = db.Emotes.Where(c => c.HasByDefault).ToList();
                var parachutes = db.Parachutes.Where(c => c.HasByDefault).ToList();

                var rand = new Random();

                player = new Player
                {
                    Name = "",
                    CurrentCharacter = characters[rand.Next(0, characters.Count)].Id,
                    CurrentFlag = flags[rand.Next(0, flags.Count)].Id,
                    CurrentPara = parachutes[rand.Next(0, parachutes.Count)].Id,
                    CurrentEmote = emotes[rand.Next(0, emotes.Count)].Id,
                    CoinCount = 100,
                    TicketCount = 10,
                    HasBattlePass = false,
                    AiBehaviorIndex = -1,
                    TotalBattles = 0,
                    TotalWins = 0,
                    TotalKills = 0,
                    DoubleKills = 0,
                    TripleKills = 0,
                    ItemsCollected = 0,
                    GunsCollected = 0,
                    SupplyDrop = 0,
                    SupplyCreates = 0,
                    Experience = 0,
                    Characters = characters,
                    Flags = flags,
                    Parachutes = parachutes,
                    Emotes = emotes,
                };


                db.Players.Add(player);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }

            if (player == null)
                return BadRequest("Invalid player id");

            var output =
                new
                {
                    player.Id,
                    player.Name,
                    player.AiBehaviorIndex,
                    player.CurrentCharacter,
                    player.CurrentPara,
                    player.CurrentFlag,
                    player.CurrentEmote,
                    player.CoinCount,
                    player.TicketCount,
                    player.HasBattlePass,
                    player.TotalBattles,
                    player.TotalWins,
                    player.TotalKills,
                    player.DoubleKills,
                    player.TripleKills,
                    player.ItemsCollected,
                    player.SupplyDrop,
                    player.SupplyCreates,
                    player.GunsCollected,
                    player.Experience,
                    Characters = player.Characters.Select(c => c.Id),
                    Flags = player.Flags.Select(c => c.Id),
                    Emotes = player.Emotes.Select(c => c.Id),
                    Parachutes = player.Parachutes.Select(c => c.Id),
                };

            return Ok(output);
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