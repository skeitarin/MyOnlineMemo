using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Base32;
using MessagingToolkit.QRCode.Codec;
using MyOnlineMemo.BusinessLayer;
using OtpSharp;

namespace MyOnlineMemo.Controllers
{
    public class AuthController : Controller
    {
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public string CreateAccount(string usrId, string psswrd)
        {
            var service = new AuthService();
            service.CreateAccount(usrId, psswrd);
            return usrId;
        }

        // GET: Auth
        public ActionResult TwoStepVerif()
        {
            byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
            string userId = "dummy";
            string qrcodeUrl = KeyUrl.GetTotpUrl(secretKey, userId) + "&issuer=MyOnlineMemo";

            var encoder = new QRCodeEncoder();
            encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            var qrImage = encoder.Encode(qrcodeUrl);

            string qrcodeString = "";
            using(var ms = new MemoryStream())
            {
                qrImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                qrcodeString = Convert.ToBase64String(ms.GetBuffer());
            }
            //Test
            Session["secretKey"] = Base32Encoder.Encode(secretKey);

            // この認証キーをユーザーごとに保存しておく
            ViewData["secretKey"] = Base32Encoder.Encode(secretKey);
            ViewData["url"] = string.Format("data:{0};base64,{1}", "image/png", qrcodeString);
            return View();
        }
        
        [HttpPost]
        public string SetTwoStepVerif(string verifKey)
        {
            byte[] secretKey = Base32Encoder.Decode(Session["secretKey"].ToString());
            long timeStepMatched = 0;
            var totp = new Totp(secretKey);

            var message = "";
            if(totp.VerifyTotp(verifKey, out timeStepMatched, new VerificationWindow(1, 1)))
            {
                message = "成功";
            }else
            {
                message = "失敗";
            }
            return message;
        }
    }
}