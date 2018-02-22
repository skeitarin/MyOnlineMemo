using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Base32;
using MessagingToolkit.QRCode.Codec;
using OtpSharp;
using System.IO;
using System.Text;

namespace MyOnlineMemo.Controllers
{
    public class AuthController : Controller
    {
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
        public ActionResult SetTwoStepVerif(string verifKey)
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
            ViewData["mes"] = message;
            return View("TwoStepVerif");
        }
    }
}