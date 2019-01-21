using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using slnShoppingCar.Models;

namespace slnShoppingCar.Controllers
{
    public class HomeController : Controller
    {
        dbShoppingCarEntities db = new dbShoppingCarEntities();


        // 首頁
        public ActionResult Index()
        {
            var products = db.tProduct.ToList();

            if (Session["Member"] == null)
            {
                return View("Index", "_Layout", products);
            }
            return View("Index", "_LayoutMember", products);
        }

        // 登入
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string fUserId,string fPwd)
        {
            var member = db.tMember.Where(m => m.fUserId == fUserId && m.fPwd == fPwd).FirstOrDefault();

            if (member == null)
            {
                ViewBag.Message = "帳密錯誤，登入失敗";
                return View();
            }

            //使用Session變數紀錄歡迎詞
            Session["WelCome"] = member.fName + "歡迎光臨";
            //使用Session變數紀錄登入的會員物件
            Session["Member"] = member;
            //執行Home控制器的Index動作方法，
            return RedirectToAction("Index");
        }

        // 登出
        public ActionResult Logout()
        {
            Session.Clear(); //清除Session 變數
            return RedirectToAction("Index"); //執行Index 方法顯示產品列表
        }

        //登記
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(tMember pMember)
        {
            if (ModelState.IsValid == false)
            {
                return View();
            }

            var member = db.tMember.Where(m => m.fUserId == pMember.fUserId).FirstOrDefault();

            if (member == null)
            {
                db.tMember.Add(pMember);
                db.SaveChanges();

                return RedirectToAction("Login");
            }
            ViewBag.Message = "此帳號已有人使用，這次失敗";
            return View();
        }

        public ActionResult OrderList()
        {
            string fuserId = (Session["Member"] as tMember).fUserId;
            var    orders  = db.tOrder.Where(m => m.fUserId == fuserId).OrderByDescending(m => m.fDate).ToList();
            return View("OrderList", "_LayoutMember",orders);
        }

        public ActionResult OrderDetail(string fOrderGuid)
        {
            var orderDetails = db.tOrderDetail.Where(m => m.fOrderGuid == fOrderGuid).ToList();
            return View("OrderDetail", "_LayoutMember", orderDetails);
        }

            //購物車
            public ActionResult ShoppingCar()
        {
            //取得登入會員的帳號並指定給fUerId
            string fuserId = (Session["Member"] as tMember).fUserId;
            //找出末成為訂單明細的資料，即購物車內容
            var orderDetails = db.tOrderDetail.Where(m => m.fUserId == fuserId && m.fIsApproved == "否").ToList();

            return View("ShoppingCar", "_LayoutMember",orderDetails);

        }

        [HttpPost]
        public ActionResult ShoppingCar(string fReceiver,string fEmail,string fAddress)
        {
            //取得會員帳號並指定給fUSerId
            string fUSerId = (Session["Member"] as tMember).fUserId;
            string guid = Guid.NewGuid().ToString();

            tOrder order = new tOrder();
            order.fOrderGuid = guid;
            order.fUserId    = fUSerId;
            order.fReceiver  = fReceiver;
            order.fEmail     = fEmail;
            order.fAddress   = fAddress;
            order.fDate      = DateTime.Now;
            db.tOrder.Add(order);

            var carList = db.tOrderDetail.Where(m => m.fIsApproved == "否" && m.fUserId == fUSerId).ToList();

            foreach (var item in carList)
            {
                item.fOrderGuid  = guid;
                item.fIsApproved = "是";
            }
            db.SaveChanges();
            return RedirectToAction("OrderList");
        }



            //購物車_加入
            public ActionResult AddCar(string fPId)
        {
            //取得會員帳號並指定給fUSerId
            string fUSerId = (Session["Member"] as tMember).fUserId;

            //找到會員
            var currentCar = db.tOrderDetail.Where(m => m.fPId == fPId && m.fIsApproved == "否" && m.fUserId == fUSerId).FirstOrDefault();

            if (currentCar == null)
            {
                var product = db.tProduct.Where(m => m.fPId == fPId).FirstOrDefault();

                tOrderDetail orderDetail = new tOrderDetail();
                orderDetail.fUserId = fUSerId;
                orderDetail.fPId = product.fPId;
                orderDetail.fName = product.fName;
                orderDetail.fPrice = product.fPrice;
                orderDetail.fQty = 1;
                orderDetail.fIsApproved = "否";
                db.tOrderDetail.Add(orderDetail);

            }
            else
            {
                currentCar.fQty += 1;
            }
            db.SaveChanges();

            return RedirectToAction("ShoppingCar");
        }


        //購物車_刪除
        public ActionResult DeleteCar(int fId)
        {
            var orderDetail = db.tOrderDetail.Where(m => m.fId ==fId).FirstOrDefault();

            db.tOrderDetail.Remove(orderDetail);
            db.SaveChanges();

            return RedirectToAction("ShoppingCar");
        }
    }
}