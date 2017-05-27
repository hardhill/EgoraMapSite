using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EgoraMap.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Net;

namespace EgoraMap.Controllers
{
    [Produces("application/json")]
    [Route("api/Routes")]
    public class RoutesController : Controller
    {

        DbEgoraContext db = new DbEgoraContext();
        IHostingEnvironment _appEnvironment;

        List<ViewRoute> vrList = new List<ViewRoute>();
        public RoutesController(DbEgoraContext context, IHostingEnvironment appEnvironment)
        {
            //this.db = context;
            _appEnvironment = appEnvironment;
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

        [HttpDelete("{id}")]
        public IActionResult Delete(int Id)
        {

            Route route = db.Routes.FirstOrDefault(x => x.Id == Id);
            if (route == null) return NotFound();
            string fileimg = route.RouteImage;
            string filekml = route.RouteKML;
            int routeId = route.Id;
            using (DbEgoraContext db2 = new DbEgoraContext())
            {
                IEnumerable<Photo> photos = db2.Photos.Where(x => x.RouteId == routeId);

                foreach (var photo in photos)
                {
                    string filephoto = photo.PhotoName;
                    try
                    {
                        System.IO.File.Delete(_appEnvironment.WebRootPath + "/Files/Photo/" + filephoto);
                    }
                    catch (Exception e)
                    {
                        return NotFound("Ошибка удаления файла");
                    }
                }
                if (photos == null) return NotFound();
                try
                {
                    db.Photos.RemoveRange(photos);
                    db.SaveChanges();
                }
                catch(Exception e)
                {
                    return NotFound(e.Message);
                }
                
            }
            db.Remove(route);
            int saved = db.SaveChanges();
            if (saved > 0)
            {
                try
                {
                    System.IO.File.Delete(_appEnvironment.WebRootPath + "/Files/Img/" + fileimg);
                    System.IO.File.Delete(_appEnvironment.WebRootPath + "/Files/Kml/" + filekml);

                    return Ok();
                }
                catch (Exception e)
                {
                    return NotFound(e.Message);
                }
            }
            else return NotFound();

        }

        [HttpPost]
        public IActionResult AddMapRoute(IFormFile uploadImage, IFormFile uploadKML, IFormFileCollection ffile)
        {
            string urlimg, urlkml;

            string strName = HttpContext.Request.Form["Name"];
            string strDescription = HttpContext.Request.Form["Description"];
            if (strName == "")
            {
                return Forbid("Не указано название карты");
            }
            if (uploadImage == null || uploadKML == null)
            {
                return Forbid("Нет файла изображения маршрута или файла KML");
            }
            // получаем имя файла из переданных параметров
            string fileNameImage = System.IO.Path.GetFileName(uploadImage.FileName);
            string fileNameKML = System.IO.Path.GetFileName(uploadKML.FileName);
            //формируем имя файла для сохранения в БД
            urlimg = String.Format("{0}_{1}{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), Guid.NewGuid(), Path.GetExtension(fileNameImage));
            urlkml = String.Format("{0}_{1}{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), Guid.NewGuid(), Path.GetExtension(fileNameKML));

            List<Photo> photos = new List<Photo>();
            if (ffile != null)
            {
                foreach (IFormFile photoFile in ffile)
                {
                    string photoNameImage = System.IO.Path.GetFileName(photoFile.FileName);
                    string urlphoto = String.Format("{0}_{1}{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), Guid.NewGuid(), Path.GetExtension(photoNameImage));
                    Photo photo = new Photo();
                    photo.PhotoName = urlphoto;
                    photo.Photocreated = DateTime.Now;
                    photo.Description = photoNameImage;
                    photos.Add(photo);
                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + "/Files/Photo/" + urlphoto, FileMode.Create))
                    {
                        photoFile.CopyTo(fileStream);
                    }
                }
            }

            Route route = new Route();
            try
            {
                route.Name = strName;
                route.Description = strDescription;
                route.RouteImage = urlimg;
                route.RouteKML = urlkml;
                db.Routes.Add(route);
                int saved = db.SaveChanges();
                if (saved > 0)
                {
                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + "/Files/Img/" + urlimg, FileMode.Create))
                    {
                        uploadImage.CopyTo(fileStream);
                    }
                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + "/Files/Kml/" + urlkml, FileMode.Create))
                    {
                        uploadKML.CopyTo(fileStream);
                    }

                    foreach(var p in photos)
                    {
                        p.Route = route;
                    }
                }
                db.Photos.AddRange(photos);
                saved = db.SaveChanges();
                if (saved < 1)
                {
                    foreach (var photo in photos)
                    {
                        string filephoto = photo.PhotoName;
                        try
                        {
                            System.IO.File.Delete(_appEnvironment.WebRootPath + "/Files/Photo/" + filephoto);
                        }
                        catch (Exception e)
                        {
                            return NotFound("Ошибка удаления файла");
                        }
                    }
                }

                return Json("Route added");
            }
            catch (Exception e) { return Forbid(); }


            
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
