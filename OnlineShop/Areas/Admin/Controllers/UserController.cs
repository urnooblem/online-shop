﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OnlineShop.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

       
        /// <summary>
        /// GET: Admin/User
        /// </summary>
        /// <returns>List User</returns>
        public ActionResult Index()
        {
            var model = db.Users.ToList();
            return View(model);
        }


        /// <summary>
        ///  GET: Admin/User/Create
        /// </summary>
        /// <returns>View Create User</returns>
        public ActionResult Create()
        {
            ViewBag.Rola = new SelectList(db.Roles, "Id", "Name");
            return View();
        }

        /// <summary>
        /// 
        ///     POST: Admin/User/Create
        ///     Create User          
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="password"></param>
        /// <param name="form"></param>
        /// <returns> Redirect To Action</returns>
        [HttpPost]
        public ActionResult Create(string Email, string password, FormCollection form)
        {
            var user = new ApplicationUser { UserName = Email, Email = Email };
            UserStore<ApplicationUser> store = new UserStore<ApplicationUser>(db);
            ApplicationUserManager userManager = new ApplicationUserManager(store);

            String hashedNewPassword = userManager.PasswordHasher.HashPassword(password);

            var result = userManager.Create(user, hashedNewPassword);
            store.SetPasswordHashAsync(user, hashedNewPassword);
            user.Roles.Add(new IdentityUserRole { RoleId = form["Rola"] });

            store.UpdateAsync(user);

            return RedirectToAction("Index", "User");
        }


        /// <summary>
        /// // GET: Admin/User/Edit/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns> View Edit User</returns>
        public ActionResult Edit(string id)
        {

            if (id == null)
                return HttpNotFound();
            var model = db.Users.Single(p => p.Id == id);
            if (model == null)
                return HttpNotFound();
            ViewBag.Rola = new SelectList(db.Roles, "Id", "Name");
            return View(model);
        }

        /// <summary>
        ///  POST: Admin/User/Edit
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="cpassword"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(ApplicationUser user, string password, string cpassword, FormCollection form)
        {
            ViewBag.Rola = new SelectList(db.Roles, "Id", "Name");
            //int id_rola;
            //Int32.TryParse(form["Rola"],out id_rola);
           
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
        
            UserStore<ApplicationUser> store = new UserStore<ApplicationUser>(db);
            ApplicationUserManager userManager = new ApplicationUserManager(store);
            ApplicationUser cUser = userManager.FindById(user.Id);
            if (password == cpassword && password != "")
            {
                String userId = user.Id;
                String newPassword = password;
                String hashedNewPassword = userManager.PasswordHasher.HashPassword(newPassword);
                store.SetPasswordHashAsync(cUser, hashedNewPassword);
                //store.SetPasswordHashAsync(cUser, hashedNewPassword);
                //var x = store.GetPasswordHashAsync(cUser);
                ////user.PasswordHash = x.ToString();
            }

            cUser.PhoneNumber = user.PhoneNumber;
            cUser.UserName = user.UserName;
            cUser.Email = user.Email;
            var roles = userManager.GetRoles(cUser.Id);
            foreach (var role in roles)
            {
                userManager.RemoveFromRole(cUser.Id, role);
            }
            cUser.Roles.Add(new IdentityUserRole { RoleId = form["Rola"] });
            store.UpdateAsync(cUser);

            return RedirectToAction("Index", "User");
        }

        /// <summary>
        /// SendEmail
        /// </summary>
        /// <returns>Redirect To Action</returns>

        public ActionResult SendEmail()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string New = "";
                    var model = db.products;
                    var senderEmail = new MailAddress("reklamashoppshopp@gmail.com", "Admin");
                    var receiverEmail = new MailAddress("2e.sienkiewicz@gmail.com", "Receiver");
                    foreach (var a in model)
                    {
                        var pom = (a.Add_date - DateTime.Now).Days;
                        if (pom <= 3)
                        {
                            New = New + "\n" + a.name + " "+"Cena" + a.price;
                        }
                    }



                    var password = "patrykPp@123";
                    var sub = "test";
                    var body = "Lista Nowosci:" + New;
                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };

                    //UserStore<ApplicationUser> store = new UserStore<ApplicationUser>(_db);
                    //ApplicationUserManager userManager = new ApplicationUserManager(store);
                    var send_user = db.Users.Where(p => p.newsletter == true).ToList();


                    var mess = new MailMessage();
                    mess.Body = body;
                    mess.Subject = sub;
                    mess.From = senderEmail;
                    mess.To.Add(receiverEmail);
                    mess.To.Add("sszaring@gmail.com");
                    foreach (var user in send_user)
                    {
                        mess.To.Add(user.Email);
                    }


                    {
                        smtp.Send(mess);
                    }
                    return RedirectToAction("Index", "User");
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Some Error";
            }
            return RedirectToAction("Index", "User");
        }

        /// <summary>
        /// GET: Delete User
        /// Delete User
        /// </summary>
        /// <param name="id"></param>
        /// <returns> Delete User</returns>
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }




            UserStore<ApplicationUser> store = new UserStore<ApplicationUser>(db);
            ApplicationUserManager userManager = new ApplicationUserManager(store);
            ApplicationUser cUser = userManager.FindById(id);
            if (cUser == null)
            {
                return HttpNotFound();
            }
            userManager.Delete(cUser);

            return RedirectToAction("Index", "User");
        }

    }
}