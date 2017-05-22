using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EgoraMap.Models;
using Microsoft.EntityFrameworkCore;

namespace EgoraMap.Controllers
{
    [Produces("application/json")]
    [Route("api/Routes")]
    public class RoutesController : Controller
    {
       
        DbEgoraContext db = new DbEgoraContext();

        List<ViewRoute> vrList = new List<ViewRoute>();
        public RoutesController(DbEgoraContext context)
        {
            //this.db = context;
            
            if (db.Routes.Any())
            {
                foreach (Route route in db.Routes)
                {
                    ViewRoute vr = new ViewRoute();
                    vr.Id = route.Id;
                    vr.Name = route.Name;
                    vr.Description = route.Description;
                    vr.ImageMap = route.RouteImage;
                    vr.PhotoPath = GetPhotos(route.Id);
                    vrList.Add(vr);
                }
            }
        }

        [HttpGet]
        public IEnumerable<ViewRoute> Get()
        {
            return vrList;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int Id)
        {
            Route route = db.Routes.FirstOrDefault(x => x.Id == Id);
            if (route == null)
                return NotFound();
            ViewRoute vr = new ViewRoute();
            vr.Id = route.Id;
            vr.Name = route.Name;
            vr.Description = route.Description;
            vr.ImageMap = route.RouteImage;
            vr.PhotoPath = GetPhotos(route.Id);
            return Ok(vr);
        }

        private IEnumerable<string> GetPhotos(int id)
        {
            List<string> photopath = new List<string>();
            List<Photo> photos;
            using (var db2 = new DbEgoraContext())
            {
                photos = db2.Photos.Where(x => x.RouteId == id).ToList();
            }
            if (!photos.Any())
                return null;
            foreach (Photo ph in photos)
            {
                photopath.Add(ph.PhotoName);
            }
            return photopath;
        }

    }
}