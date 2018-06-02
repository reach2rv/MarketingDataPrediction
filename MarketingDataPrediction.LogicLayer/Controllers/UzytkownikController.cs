﻿using Accord.Math;
using MarketingDataPrediction.DataLayer.Enums;
using MarketingDataPrediction.DataLayer.Models;
using MarketingDataPrediction.LogicLayer.BusinessObjects;
using MarketingDataPrediction.LogicLayer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace MarketingDataPrediction.LogicLayer.Controllers
{
    [Route("uzytkownik")]
    public class UzytkownikController : Controller
    {
        private MarketingDataPredictionDbContext _db = null;

        public UzytkownikController(DbContext context = null)
        {
            if (context == null)
            {
                _db = new MarketingDataPredictionDbContext();
            }
            else if (context != null)
            {
                _db = (MarketingDataPredictionDbContext)context;
            }
        }

        [Authorize(Roles = "Uzytkownik")]
        [HttpGet("[action]")]
        public JsonResult UczenieMaszynowe()
        {
            var response = from k in _db.Klient
                           where k.Wiek > 56
                           select new string[]
                           {
                               k.Wiek.ToString(),
                               ((WyksztalcenieEnum)k.Wyksztalcenie).ToString(),
                               ((StatusFinansowyEnum)k.Kzadluzenie).ToString(),
                               ((StatusFinansowyEnum)k.Khipoteczny).ToString(),
                               ((StanowiskoEnum)k.Stanowisko).ToString(),
                               ((StatusMatrymonialnyEnum)k.Smatrymonialny).ToString(),
                               ((StatusFinansowyEnum)k.Kkonsumencki).ToString(),
                               k.WskazSocEkon.Cci.ToString(),
                               k.WskazSocEkon.Cev.ToString(),
                               k.WskazSocEkon.Cpi.ToString(),
                               k.WskazSocEkon.Euribor3m.ToString(),
                               k.WskazSocEkon.IloscPrac.ToString(),
                               k.Kampania.DlugoscKontaktu.ToString(),
                               k.Kampania.DzienKontaktu.ToString(),
                               k.Kampania.MiesiacKontaktu.ToString(),
                               k.Kampania.RodzajKontaktu.ToString(),
                               k.Inne.IloscDni.ToString(),
                               k.Inne.IloscProb.ToString(),
                               k.Inne.IloscProbAkt.ToString(),
                               k.Inne.PopRezultat.ToString(),
                               ((RezultatEnum)k.Wynik.Rezultat).ToString()
                           };

            string[] nazwyKolumn =
            {
                "Wiek", "Wyksztalcenie", "Kredyt", "Hipoteka", "Stanowisko",
                "StatusMatrymonialny", "KredytKonsumencki", "Cci", "Cev", "Cpi",
                "Euribor3m", "IloscPracownikow", "DlugoscKontaktu", "DzienKontaktu", "MiesiacKontaktu",
                "RodzajKontaktu", "IloscDni", "IloscProb", "IloscProbAkt", "PoprzedniRezultat", "Wynik"
            };

            var dane = response.ToArray();

            var rfh = new RandomForestHelper(0.75);
            rfh.Uczenie(nazwyKolumn, dane);

            var actionResult = new KlientUczenieBO
            {
                Dane = dane,
                Wyniki = rfh.ZwrocWyniki(),
                Blad = rfh.PoliczBlad()
            };

            return Json(actionResult);
        }

        [Authorize(Roles = "Uzytkownik")]
        [HttpGet("[action]")]
        public JsonResult Statystyki()
        {
            var result = new StatystykiBO()
            {
                SredniWiekKlienta = (int)_db.Klient.Average(k => k.Wiek),
                SredniaDlugoscKontaktu = (int)_db.Klient.Average(k => k.Kampania.DlugoscKontaktu),
                MiesiaceKontaktu = _db.Kampania.GroupBy(k => k.MiesiacKontaktu)
                .OrderBy(g => g.FirstOrDefault().MiesiacKontaktu).Select(c => new MiesiacKontaktBO
                {
                    Miesiac = ((MiesiacEnum)c.FirstOrDefault().MiesiacKontaktu).ToString(),
                    IloscKontaktow = c.Count()
                }).ToArray()
            };

            return Json(result);
        }

        [Authorize(Roles = "Uzytkownik")]
        [HttpPost("[action]")]
        public JsonResult Zarejestruj([FromBody]UzytkownikBO nowyUzytkownik)
        {
            try
            {
                var lastUserId = _db.Uzytkownik.OrderByDescending(u => u.IdUzytkownik).FirstOrDefault().IdUzytkownik;

                _db.Uzytkownik.Add(new Uzytkownik
                {
                    IdUzytkownik = lastUserId + 1,
                    Email = nowyUzytkownik.Email,
                    Haslo = nowyUzytkownik.Haslo,
                    Imie = nowyUzytkownik.Imie,
                    Nazwisko = nowyUzytkownik.Nazwisko,
                    Admin = false
                });
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }

            return Json("User added");
        }

        [Authorize(Roles = "Uzytkownik,Admin")]
        [HttpPost("[action]")]
        public JsonResult ZmienProfil([FromBody]Uzytkownik uzytkownik)
        {
            int userId;
            int.TryParse(this.User.Identity.Name, out userId);

            var isAdmin = this.User.IsInRole("Admin");

            try
            {
                _db.Uzytkownik.Update(new Uzytkownik
                {
                    IdUzytkownik = userId,
                    Email = uzytkownik.Email,
                    Haslo = uzytkownik.Haslo,
                    Imie = uzytkownik.Imie,
                    Nazwisko = uzytkownik.Nazwisko,
                    Admin = isAdmin
                });
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }

            return Json("User updated");
        }

        [Authorize(Roles = "Uzytkownik,Admin")]
        [HttpGet("[action]")]
        public JsonResult ZmienProfil()
        {
            int userId;
            int.TryParse(this.User.Identity.Name, out userId);

            Uzytkownik response = null;

            try
            {
                _db.Uzytkownik.Where(u => u.IdUzytkownik == userId).FirstOrDefault();
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }

            return Json(response);
        }

    }
}
