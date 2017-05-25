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
            if (route == null)  return NotFound();
            string fileimg = route.RouteImage;
            string filekml = route.RouteKML;
            using(DbEgoraContext db2 = new DbEgoraContext())
            {
                IEnumerable<Photo> photos = db2.Photos.Where(x => x.RouteId == route.Id);
                foreach(var photo in photos)
                {
                    string filephoto = photo.PhotoName;
                    try
                    {
                        System.IO.File.Delete(_appEnvironment.WebRootPath + "/Files/Photo/" + filephoto);
                    }
                    catch(Exception e)
                    {
                        return NotFound("Ошибка удаления файла");
                    }
                }
            }
            
            try
            {
                System.IO.File.Delete(_appEnvironment.WebRootPath+"/Files/Img/"+fileimg);
                System.IO.File.Delete(_appEnvironment.WebRootPath + "/Files/Kml/" + filekml);
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }
            db.Routes.Remove(route);
            db.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddEgoraMap(IFormFile uploadImage, IFormFile uploadKML, IFormFileCollection ffile)
        {
            string urlimg, urlkml;
            Route route = new Route();
            try
            {
                // получаем имя файла
                string fileNameImage = System.IO.Path.GetFileName(uploadImage.FileName);
                string fileNameKML = System.IO.Path.GetFileName(uploadKML.FileName);

                string strName = HttpContext.Request.Form["Name"];
                string strDescription = HttpContext.Request.Form["Description"];
                int arrPhoto = Request.Form.Files.Count;
                urlimg = String.Format("{0}_{1}{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), Guid.NewGuid(), Path.GetExtension(fileNameImage));
                urlkml = String.Format("{0}_{1}{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), Guid.NewGuid(), Path.GetExtension(fileNameKML));

                route.Name = strName;
                route.Description = strDescription;
                route.RouteImage = urlimg;
                route.RouteKML = urlkml;
            }
            catch (Exception e)
            {
                return Forbid("Добавление записи не выполнено." + e.Message.ToString());
            }

            try
            {
                // сохраняем файл в папку Files в проекте
                string pathimg = "/Files/Img/" + urlimg;
                string pathkml = "/Files/Kml/" + urlkml;
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + pathimg, FileMode.Create))
                {
                    await uploadImage.CopyToAsync(fileStream);
                }
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + pathkml, FileMode.Create))
                {
                    await uploadKML.CopyToAsync(fileStream);
                }
                if (ffile != null)
                {
                    foreach (var photoFile in ffile)
                    {
                        string photoNameImage = System.IO.Path.GetFileName(photoFile.FileName);
                        string urlphoto = String.Format("{0}_{1}{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), Guid.NewGuid(), Path.GetExtension(photoNameImage));
                        Photo photo = new Photo();
                        photo.PhotoName = urlphoto;
                        photo.Photocreated = DateTime.Now;
                        photo.Description = photoNameImage;
                        route.Photos.Add(photo);
                        using (var fileStream = new FileStream(_appEnvironment.WebRootPath + urlphoto, FileMode.Create))
                        {
                            await photoFile.CopyToAsync(fileStream);
                        }
                    }
                }
                db.Entry(route).State = EntityState.Added;
                db.SaveChanges();
                return Ok();
            }
            catch (Exception e)
            {
                return Forbid("Добавление записи не выполнено." + e.Message.ToString());
            }



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
