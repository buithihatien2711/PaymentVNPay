using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestPaymentMVC.Data;
using TestPaymentMVC.Models;

namespace TestPaymentMVC.Controllers
{
    public class OrderInfoesController : Controller
    {
        private readonly VNPayContext _context;
        private IConfiguration _configuration;

        public OrderInfoesController(VNPayContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: OrderInfoes
        public async Task<IActionResult> Index()
        {
            return View(await _context.OrderInfo.ToListAsync());
        }

        // GET: OrderInfoes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderInfo = await _context.OrderInfo
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orderInfo == null)
            {
                return NotFound();
            }

            return View(orderInfo);
        }

        // GET: OrderInfoes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OrderInfoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,Amount,OrderDescription,BankCode,Status,CreatedDate,vnp_TransactionNo,vpn_Message,vpn_TxnResponseCode")] OrderInfo orderInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(orderInfo);
        }

        // GET: OrderInfoes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderInfo = await _context.OrderInfo.FindAsync(id);
            if (orderInfo == null)
            {
                return NotFound();
            }
            return View(orderInfo);
        }

        // POST: OrderInfoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("OrderId,Amount,OrderDescription,BankCode,Status,CreatedDate,vnp_TransactionNo,vpn_Message,vpn_TxnResponseCode")] OrderInfo orderInfo)
        {
            if (id != orderInfo.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderInfoExists(orderInfo.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(orderInfo);
        }

        // GET: OrderInfoes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderInfo = await _context.OrderInfo
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orderInfo == null)
            {
                return NotFound();
            }

            return View(orderInfo);
        }

        // POST: OrderInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var orderInfo = await _context.OrderInfo.FindAsync(id);
            _context.OrderInfo.Remove(orderInfo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderInfoExists(string id)
        {
            return _context.OrderInfo.Any(e => e.OrderId == id);
        }
        public async Task<IActionResult> PaymentAsync()
        {
            //Get Config Info
            string vnp_Returnurl = _configuration.GetSection("VNPayInfo").GetSection("vnp_Returnurl").Value; //URL nhan ket qua tra ve 
            string vnp_Url = _configuration.GetSection("VNPayInfo").GetSection("vnp_Url").Value; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = _configuration.GetSection("VNPayInfo").GetSection("vnp_TmnCode").Value; //Ma website
            string vnp_HashSecret = _configuration.GetSection("VNPayInfo").GetSection("vnp_HashSecret").Value; //Chuoi bi mat
            if (string.IsNullOrEmpty(vnp_TmnCode) || string.IsNullOrEmpty(vnp_HashSecret))
            {
                return Json("Vui lòng cấu hình các tham số: vnp_TmnCode,vnp_HashSecret trong file appsetting.json");
            }
            //Get payment input
            OrderInfo order = new OrderInfo();
            //Save order to db
            order.OrderId = Guid.NewGuid().ToString();
            order.Amount = 10000;
            order.OrderDescription = "VIP1";
            order.CreatedDate = DateTime.Now;
            order.BankCode = "NCB";
            order.Status = 0;
            _context.Add(order);
            await _context.SaveChangesAsync();
            string locale = "vn";
            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", "2.0.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (order.Amount * 100).ToString());
            //if (cboBankCode.SelectedItem != null && !string.IsNullOrEmpty(cboBankCode.SelectedItem.Value))
            //{
            //    vnpay.AddRequestData("vnp_BankCode", cboBankCode.SelectedItem.Value);
            //}
            vnpay.AddRequestData("vnp_BankCode", "NCB");
            vnpay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Models.Utils.GetIpAddress());


            if (!string.IsNullOrEmpty(locale))
            {
                vnpay.AddRequestData("vnp_Locale", locale);
            }
            else
            {
                vnpay.AddRequestData("vnp_Locale", "vn");
            }
            vnpay.AddRequestData("vnp_OrderInfo", order.OrderDescription);
            //vnpay.AddRequestData("vnp_OrderType", orderCategory.SelectedItem.Value); //default value: other
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.OrderId);

            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            //log.InfoFormat("VNPAY URL: {0}", paymentUrl);
            Response.Redirect(paymentUrl);
            return Json("Success");
        }

        public async Task<IActionResult> StatusAsync()
        {
            string returnContent = string.Empty;
            if (Request.QueryString.Value.Length > 0)
            {
                string vnp_HashSecret = _configuration.GetSection("VNPayInfo").GetSection("vnp_HashSecret").Value; //Chuoi bi mat
                var vnpayData = Request.Query;
                //return Json(vnpayData);
                VnPayLibrary vnpay = new VnPayLibrary();
                if (vnpayData.Count > 0)
                {
                    foreach (var s in vnpayData)
                    {
                        //get all querystring data
                        if (!string.IsNullOrEmpty(s.Key) && s.Key.StartsWith("vnp_"))
                        {
                            vnpay.AddResponseData(s.Key, s.Value);
                        }
                    }
                }
                //Lay danh sach tham so tra ve tu VNPAY

                //vnp_TxnRef: Ma don hang merchant gui VNPAY tai command=pay    
                string orderId = vnpay.GetResponseData("vnp_TxnRef");
                //vnp_TransactionNo: Ma GD tai he thong VNPAY
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                //vnp_ResponseCode:Response code from VNPAY: 00: Thanh cong, Khac 00: Xem tai lieu
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                //vnp_SecureHash: MD5 cua du lieu tra ve
                string vnp_SecureHash = vnpay.GetResponseData("vnp_SecureHash");
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    //Cap nhat ket qua GD
                    //Yeu cau: Truy van vao CSDL cua  Merchant => lay ra duoc OrderInfo
                    //Giả sử OrderInfo lấy ra được như giả lập bên dưới
                    var order = await _context.OrderInfo
                        .FirstOrDefaultAsync(m => m.OrderId == orderId);
                    order.vnp_TransactionNo = vnpayTranId;
                    order.vpn_TxnResponseCode = vnp_ResponseCode;
                    order.Status = 0; //0: Cho thanh toan,1: da thanh toan,2: GD loi
                                      //Kiem tra tinh trang Order
                    if (order != null)
                    {
                        if (order.Status == 0)
                        {
                            if (vnp_ResponseCode == "00")
                            {
                                //Thanh toan thanh cong
                                ViewData["Status"] = "Thanh toán thành công, OrderId="+orderId+", VNPAY TranId="+vnpayTranId;
                                order.Status = 1;
                            }
                            else
                            {
                                //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                                //  displayMsg.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                                ViewData["Status"] = "Thanh toán lỗi, OrderId=" + orderId + ", VNPAY TranId=" + vnpayTranId + ",ResponseCode=" + vnp_ResponseCode;
                                order.Status = 2;
                            }
                            returnContent = "{\"RspCode\":\"00\",\"Message\":\"Confirm Success\"}";
                            //Thêm code Thực hiện cập nhật vào Database 
                            //Update Database
                        }
                        else
                        {
                            returnContent = "{\"RspCode\":\"02\",\"Message\":\"Order already confirmed\"}";
                        }
                        _context.Update(order);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        returnContent = "{\"RspCode\":\"01\",\"Message\":\"Order not found\"}";
                    }
                }
                else
                {
                    ViewData["Status"] = "Invalid signature";
                    returnContent = "{\"RspCode\":\"97\",\"Message\":\"Invalid signature\"}";
                }
            }
            else
            {
                returnContent = "{\"RspCode\":\"99\",\"Message\":\"Input data required\"}";
            }

            //Response.ClearContent();
            //Response.Write(returnContent);
            //Response.End();
            return View();
        }
    }
}